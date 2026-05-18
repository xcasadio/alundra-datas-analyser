using System.Runtime.InteropServices;
using static AlundraEngine.Sound.SoundBin;

namespace AlundraEngine.Sound;

public class SoundFont
{
    VabHeader _vabhead;
    class Sampleinfo
    {
        public int Index;
        public int Samplestart;

        public int Sampleend;
        public int Loopstart;
        public int Loopend;
        public bool Loop;
        public int Length;//in bytes
        public byte[] Buff;
    }

    readonly List<Sampleinfo> _sampleinfos = new();
    public SoundFont(VabHeader header, byte[] vabbody, string name)
    {
        SoundEngine = "EMU8000";
        Name = name;
        Version = new SfVersionTag { WMajor = 2, WMinor = 1 };
        var adpcmpos = 0;
        for (var dex = 0; dex < header.VagOffsetTable.Length; dex++)
        {
            var si = new Sampleinfo();
            si.Index = dex;
            var adpcmlength = header.VagOffsetTable[dex] << 3;
            adpcmpos += adpcmlength;
            var blocks = adpcmlength / 16;
            var bytespersample = 2;
            si.Length = blocks * SamplesPerBlock * bytespersample;
            si.Buff = new byte[si.Length];
            int blockloopstart, blockloopend;
            DecodeAdpcm(vabbody, adpcmpos, adpcmlength, si.Buff, false, out blockloopstart, out blockloopend, out si.Loop);
            si.Loopstart = blockloopstart * SamplesPerBlock;//loops to start of block
            si.Loopend = blockloopend * SamplesPerBlock + SamplesPerBlock - 1;//loops at end of block
            _sampleinfos.Add(si);
        }

        var totalsize = 0;
        for (var dex = 0; dex < _sampleinfos.Count; dex++)
        {
            var si = _sampleinfos[dex];
            si.Samplestart = totalsize / 2;
            si.Sampleend = si.Samplestart + si.Length / 2;

            si.Loopstart += si.Samplestart;
            si.Loopend += si.Samplestart;


            totalsize += si.Length;

        }
        SampleData = new byte[totalsize];
        for (var dex = 0; dex < _sampleinfos.Count; dex++)
        {
            var si = _sampleinfos[dex];
            Array.Copy(si.Buff, 0, SampleData, si.Samplestart * 2, si.Length);
        }

        ushort instdex = 0;
        ushort instzonedex = 0;
        ushort igenndx = 0;
        ushort imodndx = 0;
        ushort sampledex = 0;

        for(var pdex = 0;pdex<header.Header.Ps;pdex++)
        {
            var prog = header.ProgAttributes[pdex];
            var inst = new SfInst { AchInstName = "inst " + instdex.ToString(), WInstBagNdx = instzonedex };
            Instruments.Add(inst);
            instdex++;
            for (var tdex = 0;tdex<prog.Tones;tdex++)
            {
                var tone = header.VagAttributes[pdex][tdex];
                var bg = new SfInstBag { WInstGenNdx = igenndx, WInstModNdx = imodndx };
                Ibags.Add(bg);
                instzonedex++;
                //add the generators
                Igens.Add(new SfGenList { SfGenOper = SfGenerator.KeyRange, GenAmount = new GenAmountType(tone.Min, tone.Max) });
                igenndx++;
                Igens.Add(new SfGenList { SfGenOper = SfGenerator.SampleId, GenAmount = new GenAmountType(sampledex) });
                igenndx++;
                //add the moderators
                //arent any
                //add the sample
                var sinfo = _sampleinfos[tone.Vag];
                var smpl = new SfSample
                {
                    AchSampleName = "sample " + sampledex,
                    DwSampleRate = 44100,
                    SfSampleType = SfSampleLink.MonoSample,
                    ByOriginalPitch = tone.Center,
                    ChPitchCorrection = (sbyte)tone.Shift,
                    DwStart = (uint)sinfo.Samplestart,
                    DwEnd = (uint)sinfo.Sampleend,
                    DwStartLoop = (uint)sinfo.Loopstart,
                    DwEndLoop = (uint)sinfo.Loopend
                };
                Samples.Add(smpl);
                sampledex++;
            }
            //terminal instrument
            Instruments.Add(new SfInst { AchInstName = "EOI", WInstBagNdx = instzonedex });
            //add terminal ibag
            Ibags.Add(new SfInstBag { WInstGenNdx = igenndx, WInstModNdx = imodndx });
            //add terminal igen
            Igens.Add(new SfGenList());
            //add terminal imod
            Imods.Add(new SfModList());
            //add terminal sample
            Samples.Add(new SfSample { AchSampleName = "EOS" });
        }

        //just add presets for each instrument
        ushort prdex = 0;
        ushort pzonedex = 0;
        ushort pgenndx = 0;
        ushort pmodndx = 0;
        for (var dex = 0;dex<Presets.Count -1;dex++)
        {
            var preset = new SfPresetHeader { AchPresetName = "preset " + prdex.ToString(), WPreset = prdex, WBank = 0, WPresetBagNdx = pzonedex };
            Presets.Add(preset);
            prdex++;
            Pbags.Add(new SfPresetBag { NModNdx = pmodndx, WGenNdx = pgenndx });
            pzonedex++;
            Pgens.Add(new SfGenList { SfGenOper = SfGenerator.Instrument, GenAmount = new GenAmountType((ushort)dex) });
            pgenndx++;
        }
        //perminal preset
        Presets.Add(new SfPresetHeader { AchPresetName = "EOP", WPresetBagNdx = pzonedex });
        //terminal pbag
        Pbags.Add(new SfPresetBag());
        //terminal pgen
        Pgens.Add(new SfGenList());
        //terminal pmod
        Pmods.Add(new SfModList());
    }


    //riff chunk

    //info list chunk
    //soundfont header info
    public SfVersionTag Version;
    public string SoundEngine;
    public string Name;

    //sdta list chunk
    //sample data
    public byte[] SampleData = new byte[0];

    //pdta chunk
    //programs/instruments/sample headers
    public List<SfPresetHeader> Presets = new();
    public List<SfPresetBag> Pbags = new();
    public List<SfModList> Pmods = new();
    public List<SfGenList> Pgens = new();
    public List<SfInst> Instruments = new();
    public List<SfInstBag> Ibags = new();
    public List<SfModList> Imods = new();
    public List<SfGenList> Igens = new();
    public List<SfSample> Samples = new();

    public void Write(BinaryWriter bw)
    {
        bw.Write("RIFF".ToCharArray());
        var rifflenpos = bw.BaseStream.Position; //at the end will write the length here
        bw.Write((int)0);//this will be the riff chunk len
        bw.Write("sfbk".ToCharArray());
        bw.Write("LIST".ToCharArray());
        var infolenpos = bw.BaseStream.Position;
        bw.Write((int)0);//this will be the info chunk len
        bw.Write("INFO".ToCharArray());
        bw.Write("ifil".ToCharArray());
        bw.Write((int)4);
        bw.Write(Version.WMajor);
        bw.Write(Version.WMinor);
        WriteStringField("isng", SoundEngine, bw);
        WriteStringField("INAM", Name, bw);
        var infolen = (int)(bw.BaseStream.Position - infolenpos) - 4;
        bw.Write("LIST".ToCharArray());
        bw.Write(SampleData.Length + 12);
        bw.Write("sdta".ToCharArray());
        bw.Write("smple".ToCharArray());
        bw.Write(SampleData.Length);
        bw.Write(SampleData);
        bw.Write("LIST".ToCharArray());
        var pdtalenpos = bw.BaseStream.Position;
        bw.Write((int)0);//this will be the pdta chunk len
        bw.Write("pdta".ToCharArray());

        bw.Write("phdr".ToCharArray());
        bw.Write(Presets.Count * 38);
        foreach(var preset in Presets)
        {
            preset.Write(bw);
        }

        bw.Write("pbag".ToCharArray());
        bw.Write(Pbags.Count * 4);
        foreach (var pbag in Pbags)
        {
            pbag.Write(bw);
        }

        bw.Write("pmod".ToCharArray());
        bw.Write(Pmods.Count * 10);
        foreach (var pmod in Pmods)
        {
            pmod.Write(bw);
        }

        bw.Write("pgen".ToCharArray());
        bw.Write(Pgens.Count * 4);
        foreach (var pgen in Pgens)
        {
            pgen.Write(bw);
        }

        bw.Write("inst".ToCharArray());
        bw.Write(Instruments.Count * 22);
        foreach (var inst in Instruments)
        {
            inst.Write(bw);
        }

        bw.Write("ibag".ToCharArray());
        bw.Write(Ibags.Count * 4);
        foreach (var ibag in Ibags)
        {
            ibag.Write(bw);
        }

        bw.Write("imod".ToCharArray());
        bw.Write(Imods.Count * 10);
        foreach (var imod in Imods)
        {
            imod.Write(bw);
        }

        bw.Write("igen".ToCharArray());
        bw.Write(Igens.Count * 4);
        foreach (var igen in Igens)
        {
            igen.Write(bw);
        }

        bw.Write("pbag".ToCharArray());
        bw.Write(Pbags.Count * 4);
        foreach (var pbag in Pbags)
        {
            pbag.Write(bw);
        }

        bw.Write("pmod".ToCharArray());
        bw.Write(Pmods.Count * 10);
        foreach (var pmod in Pmods)
        {
            pmod.Write(bw);
        }

        bw.Write("shdr".ToCharArray());
        bw.Write(Samples.Count * 46);
        foreach (var shdr in Samples)
        {
            shdr.Write(bw);
        }

        //calc chunk lenths for root chunk and this last chunk
        var pdtalen = (int)(bw.BaseStream.Position - pdtalenpos) - 4;
        var rifflen = (int)(bw.BaseStream.Position - rifflenpos) - 4;
        var save = bw.BaseStream.Position;
        //seek bank and write all the chunk lengths
        bw.BaseStream.Position = rifflenpos;
        bw.Write(rifflen);
        bw.BaseStream.Position = infolenpos;
        bw.Write(infolen);
        bw.BaseStream.Position = pdtalenpos;
        bw.Write(pdtalen);
        bw.BaseStream.Position = save;
    }

    void WriteStringField(string fieldName, string str, BinaryWriter bw)
    {
        if (str.Length % 2 == 1)
        {
            str = str + (char)0;
        }
        else
        {
            str = str + (char)0 + (char)0;
        }

        bw.Write(fieldName.ToCharArray());
        bw.Write(str.Length);
        bw.Write(str.ToCharArray());
    }
}

public class SfVersionTag
{
    public short WMajor;
    public short WMinor;
}
public class SfSample
{
    public string AchSampleName;//length of 20
    public uint DwStart;//index in sample datapoints
    public uint DwEnd;
    public uint DwStartLoop;
    public uint DwEndLoop;
    public uint DwSampleRate;//in hz
    public byte ByOriginalPitch;
    public sbyte ChPitchCorrection;//midi keynumber of "center"
    public ushort WSampleLink;//left or right associated sample for stereo samples
    public SfSampleLink SfSampleType;
    public void Write(BinaryWriter bw)
    {
        Helper.WriteString(AchSampleName, 20, bw);
        bw.Write(DwStart);
        bw.Write(DwEnd);
        bw.Write(DwStartLoop);
        bw.Write(DwEndLoop);
        bw.Write(DwSampleRate);
        bw.Write(ByOriginalPitch);
        bw.Write(ChPitchCorrection);
        bw.Write(WSampleLink);
        bw.Write((short)SfSampleType);
    }
        
}
public static class Helper
{
    public static void WriteString(string s, int len, BinaryWriter bw)
    {
        bw.Write(s.ToCharArray());
        bw.Write(_empty, 0, len - s.Length);
    }
    static readonly byte[] _empty = new byte[20];
}
//final sample is named EOS and rest of record is zeroed

public enum SfSampleLink
{
    MonoSample = 1,
    RightSample = 2,
    LeftSample = 4,
    LinkedSample = 8,
    RomMonoSample = 0x8001,
    RomRightSample = 0x8002,
    RomLeftSample = 0x8004,
    RomLinkedSample = 0x8008
}

public class SfPresetHeader
{
    public string AchPresetName;//20 bytes
    public ushort WPreset;//midi preset num
    public ushort WBank;//midi bank num
    public ushort WPresetBagNdx;//index to zone list
    public uint DwLibrary;
    public uint DwGenre;
    public uint DwMorphology;
    public void Write(BinaryWriter bw)
    {
        Helper.WriteString(AchPresetName, 20, bw);
        bw.Write(WPreset);
        bw.Write(WBank);
        bw.Write(WPresetBagNdx);
        bw.Write(DwLibrary);
        bw.Write(DwGenre);
        bw.Write(DwMorphology);
    }
}
//terminal record has the final presetbagindex and a name of EOP

//zone list
public class SfPresetBag
{
    public ushort WGenNdx;
    public ushort NModNdx;
    public void Write(BinaryWriter bw)
    {
        bw.Write(WGenNdx);
        bw.Write(NModNdx);
    }
}

public class SfModList
{
    //cast to short when writing these enums
    public SfModulator SfModSrcOper;
    public SfGenerator SfModDestOper;
    public short ModAmount;
    public SfModulator SfModAmtSrcOper;
    public SfTransform SfModTransOper;//linear is the only one defined
    public void Write(BinaryWriter bw)
    {
        bw.Write(SfModSrcOper.Data);
        bw.Write((short)SfModDestOper);
        bw.Write(ModAmount);
        bw.Write(SfModAmtSrcOper.Data);
        bw.Write((short)SfModTransOper);
    }
}
//terminal record is zeroed out

public class SfGenList
{
    public SfGenerator SfGenOper;
    public GenAmountType GenAmount;
    public void Write(BinaryWriter bw)
    {
        bw.Write((short)SfGenOper);
        bw.Write(GenAmount.WAmount);
    }
}

public class SfInst
{
    public string AchInstName;//20 bytes
    public ushort WInstBagNdx;
    public void Write(BinaryWriter bw)
    {
        Helper.WriteString(AchInstName, 20, bw);
        bw.Write(WInstBagNdx);
    }
}

public class SfInstBag
{
    public ushort WInstGenNdx;
    public ushort WInstModNdx;
    public void Write(BinaryWriter bw)
    {
        bw.Write(WInstGenNdx);
        bw.Write(WInstModNdx);
    }
}

public enum SfGenerator
{
    StartAddrsOffset = 0,
    EndAddrsOffset = 1,
    StartloopAddrsOffset = 2,
    EndLoopAddrsOffset = 3,
    //TODO add ones in here
    InitialFilterFc = 8,//lowpass filter
    InitialFilterQ = 9,
    ModLfoToFilterFc = 10,
    ModEnvToFilterFc = 11,//add to low pass filter from envelope
    //TOTO add ones in here
    DelayModEnv = 25,//25-30 used to mod either low pass filter or note pitch over time
    AttackModEnv = 26,
    HoldModEnv = 27,
    DecayModEnv = 28,
    SustainModEnv = 29,
    ReleaseModEnv = 30,
    //TODO add ones in here
    DelayVolEnv = 33,//33-38 mod volume
    AttackVolEnv = 34,
    HoldVolEnv = 35,
    DecayVolEnv = 36,
    SustainVolEnv = 37,
    ReleaseVolEnv = 38,
    Instrument = 41,//always the last pgen in a zone
    Reserved1 = 42,
    KeyRange = 43,//this one can split different instruments for different areas of the keyboard, first pgen
    VelRange = 44,
    StartloopAddrsCoarseOffset = 45,
    Keynum = 46,
    Velocity = 47,
    InitialAttenuation = 48,
    Reserved2 = 49,
    EndloopAddrsCoarseOffset = 50,
    CoarseTune = 51,//fine tuning
    FineTune = 52,//fine tuning
    SampleId = 53,//always last igen
    SampleModes = 54,
    Reserved3 = 55,
    ScaleRuning = 56,
    ExclusiveClass = 57,
    OverridingRootKey = 58,//overrides the center note thats in the sounds sample
    Unused5 = 59,
    EndOper = 60
}

public class GenAmountType
{
    public GenAmountType(byte lo, byte hi)
    {
        _data = (ushort)(lo | (hi << 8));
    }
    public GenAmountType(ushort data)
    {
        _data = data;
    }
    private readonly ushort _data;
    public byte ByRangesLo { get { return (byte)(_data & 0xff); } }
    public byte ByRangesHi { get { return (byte)((_data >> 8) & 0xff); } }
    public short ShAmount { get { return (short)_data; } }
    public ushort WAmount { get { return _data; } }
}

public enum SfTransform
{
    Linear = 0
}

public class SfModulator
{
    public ushort Data;
    public SfsourceType SourceType
    {
        get
        {
            return (SfsourceType)(Data >> 10);
        }
    }

    //determines if index refers to a controller enum or a midi continue control command
    public bool Cc
    {
        get
        {
            return ((Data >> 7) & 1) == 1;
        }
    }
    public byte ControllerIndex
    {
        get
        {
            //0 = no controoler,
            //2 = note on velocity
            //3 = note on key number
            //10 = poly pressure
            //13 = channel pressure
            //14 = pitch wheel
            //16  pitch wheel sensitivity
            return (byte)(Data & 0x7f);
        }
    }

    public Sfdirection Direction
    {
        get
        {
            return (Sfdirection)((Data >> 8) & 1);
        }
    }

    public Sfpolarity Polarity
    {
        get
        {
            return (Sfpolarity)((Data >> 9) & 1);
        }
    }

    public enum Sfdirection
    {
        MinToMax = 0,
        MaxToMin = 1
    }

    public enum Sfpolarity
    {
        Unipolar = 0,//0 to 1
        Bipolar = 1,//-1 t0 1
    }

    public enum SfsourceType
    {
        Linear = 0,
        Concave = 1,
        Convex = 2,
        Switch = 3
    }
}

