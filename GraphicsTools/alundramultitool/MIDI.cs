using System.Diagnostics;
using System.Runtime.InteropServices;

namespace midi
{
    public class InputPort
    {
        const int MmMimData = 0x3C3;

        private NativeMethods.MidiInProc _midiInProc;
        private IntPtr _handle;

        public delegate void KeyDataHandler(object sender, int number, int velocity);

        public event KeyDataHandler KeyDown;
        public event KeyDataHandler KeyUp;

        public InputPort()
        {
            _midiInProc = new NativeMethods.MidiInProc(MidiProc);
            _handle = IntPtr.Zero;
        }

        public static int InputCount
        {
            get { return NativeMethods.midiInGetNumDevs(); }
        }

        public bool Close()
        {
            var result = NativeMethods.midiInClose(_handle)
                         == NativeMethods.MmsyserrNoerror;
            _handle = IntPtr.Zero;
            return result;
        }

        public bool Open(int id)
        {
            return NativeMethods.midiInOpen(
                out _handle,
                id,
                _midiInProc,
                IntPtr.Zero,
                NativeMethods.CallbackFunction)
                    == NativeMethods.MmsyserrNoerror;
        }

        public bool Start()
        {
            return NativeMethods.midiInStart(_handle)
                == NativeMethods.MmsyserrNoerror;
        }

        public bool Stop()
        {
            return NativeMethods.midiInStop(_handle)
                == NativeMethods.MmsyserrNoerror;
        }

        private void MidiProc(IntPtr hMidiIn,
            int wMsg,
            IntPtr dwInstance,
            int dwParam1,
            int dwParam2)
        {
            Debug.Print($"wMsg:0x{wMsg.ToString("X")}");
            switch (wMsg)
            {
                case MmMimData:
                    var status = dwParam1 & 0xff;
                    var data1 = (dwParam1 >> 8) & 0xff;
                    var data2 = (dwParam1 >> 16) & 0xff;
                    var timestamp = dwParam2;
                    ReceiveData(status, data1, data2, timestamp);
                    break;
            }
            // Receive messages here
        }

        const int MidiNoteOff = 0x8;
        const int MidiNoteOn = 0x9;
        const int MidiPolyKeyPressure = 0xa;
        const int MidiControlChange = 0xb;
        const int MidiProgramChange = 0xc;
        const int MidiChannelPressure = 0xd;
        const int MidiPitchBendChange = 0xe;

        void ReceiveData(int status, int data1, int data2, int timestamp)
        {
            var message = (status >> 4) & 0xf;
            var channel = status & 0xf;
            var i = 0;
            switch (message)
            {
                case MidiNoteOff:
                    KeyUp.Invoke(this, data1, data2);
                    break;
                case MidiNoteOn:
                    KeyDown.Invoke(this, data1, data2);
                    break;
                default:
                    Debug.Print($"message:0x{message.ToString("X")}");
                    break;
            }
        }

    }


    internal static class NativeMethods
    {
        internal const int MmsyserrNoerror = 0;
        internal const int CallbackFunction = 0x00030000;

        internal delegate void MidiInProc(
            IntPtr hMidiIn,
            int wMsg,
            IntPtr dwInstance,
            int dwParam1,
            int dwParam2);

        [DllImport("winmm.dll")]
        internal static extern int midiInGetNumDevs();

        [DllImport("winmm.dll")]
        internal static extern int midiInClose(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiInOpen(
            out IntPtr lphMidiIn,
            int uDeviceId,
            MidiInProc dwCallback,
            IntPtr dwCallbackInstance,
            int dwFlags);

        [DllImport("winmm.dll")]
        internal static extern int midiInStart(
            IntPtr hMidiIn);

        [DllImport("winmm.dll")]
        internal static extern int midiInStop(
            IntPtr hMidiIn);
    }


    public class Seq
    {
        public uint Magic;
        public uint Version;
        public ushort Ticksperquarternote;
        public uint Tempo;//3 bytes
        public byte Timesignumerator;
        public byte Timesigndemoninator;
        public uint Datasize;
        public List<SeqTrk> Tracks = new();

        public Seq(BinaryReader br)
        {
            Magic = Get4B(br);
            Version = Get4B(br);
            Ticksperquarternote = Get2B(br);
            Tempo = Get3B(br);
            Timesignumerator = Getb(br);
            Timesigndemoninator = Getb(br);
            Datasize = Get4B(br);
            var track = new SeqTrk(br);
            Tracks.Add(track);
            //should be end of file
        }

        public void WriteSmf(BinaryWriter bw)
        {
            PutString("MThd", bw);
            Put4B(6, bw);
            Put2B(0, bw);
            Put2B(1, bw);
            Put2B(Ticksperquarternote, bw);

            var track = Tracks[0];
            //i thikn theres only one track in these
            PutString("MTrk", bw);
            Put4B(0, bw);
            Putb(0x00, bw);
            Putb(0xff, bw);
            Putb(0x51, bw);
            Putb(0x03, bw);
            Put3B(Tempo, bw);
            Putb(0x00, bw);
            Putb(0xff, bw);
            Putb(0x58, bw);
            Putb(0x04, bw);
            Putb(Timesignumerator, bw);
            Putb(Timesigndemoninator, bw);
            Putb(0x18, bw);
            Putb(0x08, bw);
            //gm reset
            Putb(0x00, bw);
            Putb(0xf0, bw);
            Putb(0x05, bw);
            Putb(0x7e, bw);
            Putb(0x7f, bw);
            Putb(0x09, bw);
            Putb(0x01, bw);
            Putb(0xf7, bw);
            //gm reset2
            Putb(0x00, bw);
            Putb(0xf0, bw);
            Putb(0x05, bw);
            Putb(0x7e, bw);
            Putb(0x7f, bw);
            Putb(0x09, bw);
            Putb(0x02, bw);
            Putb(0xf7, bw);

            foreach(var evt in track.Events)
            {
                evt.WriteSmf(bw);
            }
            
        }

        public static void PutVariableB(uint i, BinaryWriter bw)
        {
            var len = Varintlen(i);
            for (var dex = 0; dex < len; dex++)
            {
                var b = (byte)(((i >> (7 * len - dex - 1)) & 0x7f) | (dex < len - 1 ? 0x80 : 0));
                bw.Write(b);
            }

        }
        public static uint GetVariableB(BinaryReader br)
        {
            var i = 0;
            var b = br.ReadByte();
            i |= b & 0x7f;
            if (b <= 0x7f)
            {
                return (uint)i;
            }

            i <<= 7;
            b = br.ReadByte();
            i |= b & 0x7f;
            if (b <= 0x7f)
            {
                return (uint)i;
            }

            i <<= 7;
            b = br.ReadByte();
            i |= b & 0x7f;
            if (b <= 0x7f)
            {
                return (uint)i;
            }

            i <<= 7;
            b = br.ReadByte();
            i |= b & 0x7f;
            return (uint)i;
        }

        static int Varintlen(uint value)
        {
            var len = 0;
            do
            {
                value >>= 7;
                len++;
            } while (len < 4 && value != 0);
            return len;
        }

        public static void PutString(string s, BinaryWriter bw)
        {
            bw.Write(s.ToCharArray());
        }
        public static void Putb(byte b, BinaryWriter bw)
        {
            bw.Write(b);
        }
        public static void Put2B(ushort s, BinaryWriter bw)
        {
            bw.Write((byte)((s >> 8) & 0xff));
            bw.Write((byte)((s >> 0) & 0xff));
        }
        public static void Put3B(uint i, BinaryWriter bw)
        {
            bw.Write((byte)((i >> 16) & 0xff));
            bw.Write((byte)((i >> 8) & 0xff));
            bw.Write((byte)((i >> 0) & 0xff));
        }
        public static void Put4B(uint i, BinaryWriter bw)
        {
            bw.Write((byte)((i >> 24) & 0xff));
            bw.Write((byte)((i >> 16) & 0xff));
            bw.Write((byte)((i >> 8) & 0xff));
            bw.Write((byte)((i >> 0) & 0xff));
        }

        public static uint Get4B(BinaryReader br)
        {
            var i = 0;
            i |= br.ReadByte() << 24;
            i |= br.ReadByte() << 16;
            i |= br.ReadByte() << 8;
            i |= br.ReadByte() << 0;
            return (uint)i;
        }
        public static uint Get3B(BinaryReader br)
        {
            var i = 0;
            i |= br.ReadByte() << 16;
            i |= br.ReadByte() << 8;
            i |= br.ReadByte() << 0;
            return (uint)i;
        }
        public static ushort Get2B(BinaryReader br)
        {
            var i = 0;
            i |= br.ReadByte() << 8;
            i |= br.ReadByte() << 0;
            return (ushort)i;
        }
        public static byte Getb(BinaryReader br)
        {
            return br.ReadByte();
        }

        public class SeqTrk
        {
            public SeqTrk(BinaryReader br)
            {
                Magic = Get4B(br);
                Length = Get4B(br);
                var savedpos = br.BaseStream.Position;
                byte lastStatus = 0;
                while(br.BaseStream.Position-savedpos < Length)
                {
                    
                    var evt = new SeqEvt(br, lastStatus);
                    lastStatus = evt.Status;
                    Events.Add(evt);
                }
            }
            public uint Magic;//MTrk
            public uint Length;//length of event data
            public List<SeqEvt> Events = new();
        }

        public class SeqEvt
        {
            int[] _evtDataLengths = new[] { 2, 2, 2, 2, 1, 1, 2, 0 };
            
            public SeqEvt(BinaryReader br, byte lastStatus)
            {
                Delta = GetVariableB(br);
                Status = Getb(br);
                if ((Status & 0x80) == 0)
                {
                    //running status
                    Status = lastStatus;
                    br.BaseStream.Position -= 1;
                    var datalen = _evtDataLengths[(Status & 0x7) >> 4];
                    
                    
                    if (Status>=0xf0)//dispatch event
                    {
                        if (Status != 0xff)
                        {
                            //some kind of error
                            throw new Exception("unknown midi data in seq at 0x" + br.BaseStream.Position.ToString("x"));
                        }
                        //0xff meta events
                        MetaType = br.ReadByte();

                        //we need the datalength but seq doesnt have it
                        switch (MetaType)
                        {
                            case 0x2f://end of track
                                MetaLength = 0;
                                break;
                            case 0x51://tempo
                                MetaLength = 3;
                                break;
                            case 0x54://smte offset
                                MetaLength = 5;
                                break;
                            case 0x58://time sig
                                MetaLength = 4;
                                break;
                            case 0x59://key sig
                                MetaLength = 2;
                                break;
                            default:
                                throw new Exception("unknown midi data in seq at 0x" + br.BaseStream.Position.ToString("x"));
                        }

                        if (MetaLength>0)
                        {
                            Data = new byte[MetaLength];
                            br.Read(Data, 0, (int)MetaLength);
                        }

                    }
                    else
                    {
                        if (datalen > 0)
                        {
                            Data = new byte[datalen];
                            br.Read(Data, 0, datalen);
                        }
                    }
                }
                //switch status
            }
            public uint Delta;
            public byte Status;
            public byte[] Data = new byte[0];
            public byte MetaType = 0;
            public uint MetaLength = 0;

            public void WriteSmf(BinaryWriter bw)
            {
                PutVariableB(Delta, bw);
                Putb(Status, bw);
                if (MetaType != 0)
                {
                    Putb(MetaType, bw);
                    PutVariableB(MetaLength, bw);
                }
                bw.Write(Data);
            }
        }

    }

    



}
