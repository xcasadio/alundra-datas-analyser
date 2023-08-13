using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GraphicsTools.Alundra.SoundBin;

namespace GraphicsTools
{
    public class SoundFont
    {
        VabHeader vabhead;
        class sampleinfo
        {
            public int index;
            public int samplestart;

            public int sampleend;
            public int loopstart;
            public int loopend;
            public bool loop;
            public int length;//in bytes
            public byte[] buff;
        }
        List<sampleinfo> sampleinfos = new List<sampleinfo>();
        public SoundFont(VabHeader header, byte[] vabbody, string name)
        {
            soundEngine = "EMU8000";
            this.name = name;
            version = new sfVersionTag { wMajor = 2, wMinor = 1 };
            int adpcmpos = 0;
            for (int dex = 0; dex < header.VagOffsetTable.Length; dex++)
            {
                var si = new sampleinfo();
                si.index = dex;
                int adpcmlength = header.VagOffsetTable[dex] << 3;
                adpcmpos += adpcmlength;
                var blocks = adpcmlength / 16;
                var bytespersample = 2;
                si.length = blocks * samples_per_block * bytespersample;
                si.buff = new byte[si.length];
                int blockloopstart, blockloopend;
                DecodeADPCM(vabbody, adpcmpos, adpcmlength, si.buff, false, out blockloopstart, out blockloopend, out si.loop);
                si.loopstart = blockloopstart * samples_per_block;//loops to start of block
                si.loopend = (blockloopend * samples_per_block) + samples_per_block - 1;//loops at end of block
                sampleinfos.Add(si);
            }

            int totalsize = 0;
            for (int dex = 0; dex < sampleinfos.Count; dex++)
            {
                var si = sampleinfos[dex];
                si.samplestart = totalsize / 2;
                si.sampleend = si.samplestart + si.length / 2;

                si.loopstart += si.samplestart;
                si.loopend += si.samplestart;


                totalsize += si.length;

            }
            sample_data = new byte[totalsize];
            for (int dex = 0; dex < sampleinfos.Count; dex++)
            {
                var si = sampleinfos[dex];
                Array.Copy(si.buff, 0, sample_data, si.samplestart * 2, si.length);
            }

            ushort instdex = 0;
            ushort instzonedex = 0;
            ushort igenndx = 0;
            ushort imodndx = 0;
            ushort sampledex = 0;

            for(int pdex = 0;pdex<header.Header.ps;pdex++)
            {
                var prog = header.ProgAttributes[pdex];
                var inst = new sfInst { achInstName = "inst " + instdex.ToString(), wInstBagNdx = instzonedex };
                instruments.Add(inst);
                instdex++;
                for (int tdex = 0;tdex<prog.tones;tdex++)
                {
                    var tone = header.VagAttributes[pdex][tdex];
                    var bg = new sfInstBag { wInstGenNdx = igenndx, wInstModNdx = imodndx };
                    ibags.Add(bg);
                    instzonedex++;
                    //add the generators
                    igens.Add(new sfGenList { sfGenOper = SFGenerator.keyRange, genAmount = new genAmountType(tone.min, tone.max) });
                    igenndx++;
                    igens.Add(new sfGenList { sfGenOper = SFGenerator.sampleID, genAmount = new genAmountType(sampledex) });
                    igenndx++;
                    //add the moderators
                    //arent any
                    //add the sample
                    var sinfo = sampleinfos[tone.vag];
                    var smpl = new sfSample
                    {
                        achSampleName = "sample " + sampledex,
                        dwSampleRate = 44100,
                        sfSampleType = SFSampleLink.monoSample,
                        byOriginalPitch = tone.center,
                        chPitchCorrection = (sbyte)tone.shift,
                        dwStart = (uint)sinfo.samplestart,
                        dwEnd = (uint)sinfo.sampleend,
                        dwStartLoop = (uint)sinfo.loopstart,
                        dwEndLoop = (uint)sinfo.loopend
                    };
                    samples.Add(smpl);
                    sampledex++;
                }
                //terminal instrument
                instruments.Add(new sfInst { achInstName = "EOI", wInstBagNdx = instzonedex });
                //add terminal ibag
                ibags.Add(new sfInstBag { wInstGenNdx = igenndx, wInstModNdx = imodndx });
                //add terminal igen
                igens.Add(new sfGenList());
                //add terminal imod
                imods.Add(new sfModList());
                //add terminal sample
                samples.Add(new sfSample { achSampleName = "EOS" });
            }

            //just add presets for each instrument
            ushort prdex = 0;
            ushort pzonedex = 0;
            ushort pgenndx = 0;
            ushort pmodndx = 0;
            for (int dex = 0;dex<presets.Count -1;dex++)
            {
                var preset = new sfPresetHeader { achPresetName = "preset " + prdex.ToString(), wPreset = prdex, wBank = 0, wPresetBagNdx = pzonedex };
                presets.Add(preset);
                prdex++;
                pbags.Add(new sfPresetBag { nModNdx = pmodndx, wGenNdx = pgenndx });
                pzonedex++;
                pgens.Add(new sfGenList { sfGenOper = SFGenerator.instrument, genAmount = new genAmountType((ushort)dex) });
                pgenndx++;
            }
            //perminal preset
            presets.Add(new sfPresetHeader { achPresetName = "EOP", wPresetBagNdx = pzonedex });
            //terminal pbag
            pbags.Add(new sfPresetBag());
            //terminal pgen
            pgens.Add(new sfGenList());
            //terminal pmod
            pmods.Add(new sfModList());
        }


        //riff chunk

        //info list chunk
        //soundfont header info
        public sfVersionTag version;
        public string soundEngine;
        public string name;

        //sdta list chunk
        //sample data
        public byte[] sample_data = new byte[0];

        //pdta chunk
        //programs/instruments/sample headers
        public List<sfPresetHeader> presets = new List<sfPresetHeader>();
        public List<sfPresetBag> pbags = new List<sfPresetBag>();
        public List<sfModList> pmods = new List<sfModList>();
        public List<sfGenList> pgens = new List<sfGenList>();
        public List<sfInst> instruments = new List<sfInst>();
        public List<sfInstBag> ibags = new List<sfInstBag>();
        public List<sfModList> imods = new List<sfModList>();
        public List<sfGenList> igens = new List<sfGenList>();
        public List<sfSample> samples = new List<sfSample>();

        public void Write(BinaryWriter bw)
        {
            bw.Write("RIFF".ToCharArray());
            long rifflenpos = bw.BaseStream.Position; //at the end will write the length here
            bw.Write((int)0);//this will be the riff chunk len
            bw.Write("sfbk".ToCharArray());
            bw.Write("LIST".ToCharArray());
            long infolenpos = bw.BaseStream.Position;
            bw.Write((int)0);//this will be the info chunk len
            bw.Write("INFO".ToCharArray());
            bw.Write("ifil".ToCharArray());
            bw.Write((int)4);
            bw.Write(version.wMajor);
            bw.Write(version.wMinor);
            WriteStringField("isng", soundEngine, bw);
            WriteStringField("INAM", name, bw);
            int infolen = (int)(bw.BaseStream.Position - infolenpos) - 4;
            bw.Write("LIST".ToCharArray());
            bw.Write(sample_data.Length + 12);
            bw.Write("sdta".ToCharArray());
            bw.Write("smple".ToCharArray());
            bw.Write(sample_data.Length);
            bw.Write(sample_data);
            bw.Write("LIST".ToCharArray());
            long pdtalenpos = bw.BaseStream.Position;
            bw.Write((int)0);//this will be the pdta chunk len
            bw.Write("pdta".ToCharArray());

            bw.Write("phdr".ToCharArray());
            bw.Write(presets.Count * 38);
            foreach(var preset in presets)
            {
                preset.Write(bw);
            }

            bw.Write("pbag".ToCharArray());
            bw.Write(pbags.Count * 4);
            foreach (var pbag in pbags)
            {
                pbag.Write(bw);
            }

            bw.Write("pmod".ToCharArray());
            bw.Write(pmods.Count * 10);
            foreach (var pmod in pmods)
            {
                pmod.Write(bw);
            }

            bw.Write("pgen".ToCharArray());
            bw.Write(pgens.Count * 4);
            foreach (var pgen in pgens)
            {
                pgen.Write(bw);
            }

            bw.Write("inst".ToCharArray());
            bw.Write(instruments.Count * 22);
            foreach (var inst in instruments)
            {
                inst.Write(bw);
            }

            bw.Write("ibag".ToCharArray());
            bw.Write(ibags.Count * 4);
            foreach (var ibag in ibags)
            {
                ibag.Write(bw);
            }

            bw.Write("imod".ToCharArray());
            bw.Write(imods.Count * 10);
            foreach (var imod in imods)
            {
                imod.Write(bw);
            }

            bw.Write("igen".ToCharArray());
            bw.Write(igens.Count * 4);
            foreach (var igen in igens)
            {
                igen.Write(bw);
            }

            bw.Write("pbag".ToCharArray());
            bw.Write(pbags.Count * 4);
            foreach (var pbag in pbags)
            {
                pbag.Write(bw);
            }

            bw.Write("pmod".ToCharArray());
            bw.Write(pmods.Count * 10);
            foreach (var pmod in pmods)
            {
                pmod.Write(bw);
            }

            bw.Write("shdr".ToCharArray());
            bw.Write(samples.Count * 46);
            foreach (var shdr in samples)
            {
                shdr.Write(bw);
            }

            //calc chunk lenths for root chunk and this last chunk
            int pdtalen = (int)(bw.BaseStream.Position - pdtalenpos) - 4;
            int rifflen = (int)(bw.BaseStream.Position - rifflenpos) - 4;
            long save = bw.BaseStream.Position;
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
                str = str + (char)0;
            else
                str = str + (char)0 + (char)0;
            bw.Write(fieldName.ToCharArray());
            bw.Write(str.Length);
            bw.Write(str.ToCharArray());
        }
    }

    public class sfVersionTag
    {
        public short wMajor;
        public short wMinor;
    }
    public class sfSample
    {
        public string achSampleName;//length of 20
        public uint dwStart;//index in sample datapoints
        public uint dwEnd;
        public uint dwStartLoop;
        public uint dwEndLoop;
        public uint dwSampleRate;//in hz
        public byte byOriginalPitch;
        public sbyte chPitchCorrection;//midi keynumber of "center"
        public ushort wSampleLink;//left or right associated sample for stereo samples
        public SFSampleLink sfSampleType;
        public void Write(BinaryWriter bw)
        {
            helper.WriteString(achSampleName, 20, bw);
            bw.Write(dwStart);
            bw.Write(dwEnd);
            bw.Write(dwStartLoop);
            bw.Write(dwEndLoop);
            bw.Write(dwSampleRate);
            bw.Write(byOriginalPitch);
            bw.Write(chPitchCorrection);
            bw.Write(wSampleLink);
            bw.Write((short)sfSampleType);
        }
        
    }
    public static class helper
    {
        public static void WriteString(string s, int len, BinaryWriter bw)
        {
            bw.Write(s.ToCharArray());
            bw.Write(empty, 0, len - s.Length);
        }
        static byte[] empty = new byte[20];
    }
    //final sample is named EOS and rest of record is zeroed

    public enum SFSampleLink
    {
        monoSample = 1,
        rightSample = 2,
        leftSample = 4,
        linkedSample = 8,
        RomMonoSample = 0x8001,
        RomRightSample = 0x8002,
        RomLeftSample = 0x8004,
        RomLinkedSample = 0x8008
    }

    public class sfPresetHeader
    {
        public string achPresetName;//20 bytes
        public ushort wPreset;//midi preset num
        public ushort wBank;//midi bank num
        public ushort wPresetBagNdx;//index to zone list
        public uint dwLibrary;
        public uint dwGenre;
        public uint dwMorphology;
        public void Write(BinaryWriter bw)
        {
            helper.WriteString(achPresetName, 20, bw);
            bw.Write(wPreset);
            bw.Write(wBank);
            bw.Write(wPresetBagNdx);
            bw.Write(dwLibrary);
            bw.Write(dwGenre);
            bw.Write(dwMorphology);
        }
    }
    //terminal record has the final presetbagindex and a name of EOP

    //zone list
    public class sfPresetBag
    {
        public ushort wGenNdx;
        public ushort nModNdx;
        public void Write(BinaryWriter bw)
        {
            bw.Write(wGenNdx);
            bw.Write(nModNdx);
        }
    }

    public class sfModList
    {
        //cast to short when writing these enums
        public SFModulator sfModSrcOper;
        public SFGenerator sfModDestOper;
        public short modAmount;
        public SFModulator sfModAmtSrcOper;
        public SFTransform sfModTransOper;//linear is the only one defined
        public void Write(BinaryWriter bw)
        {
            bw.Write(sfModSrcOper.data);
            bw.Write((short)sfModDestOper);
            bw.Write(modAmount);
            bw.Write(sfModAmtSrcOper.data);
            bw.Write((short)sfModTransOper);
        }
    }
    //terminal record is zeroed out

    public class sfGenList
    {
        public SFGenerator sfGenOper;
        public genAmountType genAmount;
        public void Write(BinaryWriter bw)
        {
            bw.Write((short)sfGenOper);
            bw.Write(genAmount.wAmount);
        }
    }

    public class sfInst
    {
        public string achInstName;//20 bytes
        public ushort wInstBagNdx;
        public void Write(BinaryWriter bw)
        {
            helper.WriteString(achInstName, 20, bw);
            bw.Write(wInstBagNdx);
        }
    }

    public class sfInstBag
    {
        public ushort wInstGenNdx;
        public ushort wInstModNdx;
        public void Write(BinaryWriter bw)
        {
            bw.Write(wInstGenNdx);
            bw.Write(wInstModNdx);
        }
    }

    public enum SFGenerator
    {
        startAddrsOffset = 0,
        endAddrsOffset = 1,
        startloopAddrsOffset = 2,
        endLoopAddrsOffset = 3,
        //TODO add ones in here
        initialFilterFc = 8,//lowpass filter
        initialFilterQ = 9,
        modLfoToFilterFc = 10,
        modEnvToFilterFc = 11,//add to low pass filter from envelope
        //TOTO add ones in here
        delayModEnv = 25,//25-30 used to mod either low pass filter or note pitch over time
        attackModEnv = 26,
        holdModEnv = 27,
        decayModEnv = 28,
        sustainModEnv = 29,
        releaseModEnv = 30,
        //TODO add ones in here
        delayVolEnv = 33,//33-38 mod volume
        attackVolEnv = 34,
        holdVolEnv = 35,
        decayVolEnv = 36,
        sustainVolEnv = 37,
        releaseVolEnv = 38,
        instrument = 41,//always the last pgen in a zone
        reserved1 = 42,
        keyRange = 43,//this one can split different instruments for different areas of the keyboard, first pgen
        velRange = 44,
        startloopAddrsCoarseOffset = 45,
        keynum = 46,
        velocity = 47,
        initialAttenuation = 48,
        reserved2 = 49,
        endloopAddrsCoarseOffset = 50,
        coarseTune = 51,//fine tuning
        fineTune = 52,//fine tuning
        sampleID = 53,//always last igen
        sampleModes = 54,
        reserved3 = 55,
        scaleRuning = 56,
        exclusiveClass = 57,
        overridingRootKey = 58,//overrides the center note thats in the sounds sample
        unused5 = 59,
        endOper = 60
    }

    public class genAmountType
    {
        public genAmountType(byte lo, byte hi)
        {
            data = (ushort)(lo | (hi << 8));
        }
        public genAmountType(ushort data)
        {
            this.data = data;
        }
        private ushort data;
        public byte byRangesLo { get { return (byte)(data & 0xff); } }
        public byte byRangesHi { get { return (byte)((data >> 8) & 0xff); } }
        public short shAmount { get { return (short)data; } }
        public ushort wAmount { get { return data; } }
    }

    public enum SFTransform
    {
        linear = 0
    }

    public class SFModulator
    {
        public ushort data;
        public sfsourceType sourceType
        {
            get
            {
                return (sfsourceType)(data >> 10);
            }
        }

        //determines if index refers to a controller enum or a midi continue control command
        public bool cc
        {
            get
            {
                return ((data >> 7) & 1) == 1;
            }
        }
        public byte controllerIndex
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
                return (byte)(data & 0x7f);
            }
        }

        public sfdirection direction
        {
            get
            {
                return (sfdirection)((data >> 8) & 1);
            }
        }

        public sfpolarity polarity
        {
            get
            {
                return (sfpolarity)((data >> 9) & 1);
            }
        }

        public enum sfdirection
        {
            minToMax = 0,
            maxToMin = 1
        }

        public enum sfpolarity
        {
            unipolar = 0,//0 to 1
            bipolar = 1,//-1 t0 1
        }

        public enum sfsourceType
        {
            linear = 0,
            concave = 1,
            convex = 2,
            _switch = 3
        }
    }

    


}
