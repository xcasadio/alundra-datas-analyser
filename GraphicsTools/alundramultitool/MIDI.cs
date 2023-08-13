using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace midi
{
    public class InputPort
    {
        const int MM_MIM_DATA = 0x3C3;

        private NativeMethods.MidiInProc midiInProc;
        private IntPtr handle;

        public delegate void KeyDataHandler(object sender, int number, int velocity);

        public event KeyDataHandler KeyDown;
        public event KeyDataHandler KeyUp;

        public InputPort()
        {
            midiInProc = new NativeMethods.MidiInProc(MidiProc);
            handle = IntPtr.Zero;
        }

        public static int InputCount
        {
            get { return NativeMethods.midiInGetNumDevs(); }
        }

        public bool Close()
        {
            bool result = NativeMethods.midiInClose(handle)
                == NativeMethods.MMSYSERR_NOERROR;
            handle = IntPtr.Zero;
            return result;
        }

        public bool Open(int id)
        {
            return NativeMethods.midiInOpen(
                out handle,
                id,
                midiInProc,
                IntPtr.Zero,
                NativeMethods.CALLBACK_FUNCTION)
                    == NativeMethods.MMSYSERR_NOERROR;
        }

        public bool Start()
        {
            return NativeMethods.midiInStart(handle)
                == NativeMethods.MMSYSERR_NOERROR;
        }

        public bool Stop()
        {
            return NativeMethods.midiInStop(handle)
                == NativeMethods.MMSYSERR_NOERROR;
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
                case MM_MIM_DATA:
                    int status = dwParam1 & 0xff;
                    int data1 = (dwParam1 >> 8) & 0xff;
                    int data2 = (dwParam1 >> 16) & 0xff;
                    int timestamp = dwParam2;
                    ReceiveData(status, data1, data2, timestamp);
                    break;
            }
            // Receive messages here
        }

        const int MIDI_NOTE_OFF = 0x8;
        const int MIDI_NOTE_ON = 0x9;
        const int MIDI_POLY_KEY_PRESSURE = 0xa;
        const int MIDI_CONTROL_CHANGE = 0xb;
        const int MIDI_PROGRAM_CHANGE = 0xc;
        const int MIDI_CHANNEL_PRESSURE = 0xd;
        const int MIDI_PITCH_BEND_CHANGE = 0xe;

        void ReceiveData(int status, int data1, int data2, int timestamp)
        {
            int message = (status >> 4) & 0xf;
            int channel = (status) & 0xf;
            int i = 0;
            switch (message)
            {
                case MIDI_NOTE_OFF:
                    KeyUp.Invoke(this, data1, data2);
                    break;
                case MIDI_NOTE_ON:
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
        internal const int MMSYSERR_NOERROR = 0;
        internal const int CALLBACK_FUNCTION = 0x00030000;

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
            int uDeviceID,
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


    public class SEQ
    {
        public uint magic;
        public uint version;
        public ushort ticksperquarternote;
        public uint tempo;//3 bytes
        public byte timesignumerator;
        public byte timesigndemoninator;
        public uint datasize;
        public List<SEQTrk> tracks = new List<SEQTrk>();

        public SEQ(BinaryReader br)
        {
            magic = Get4b(br);
            version = Get4b(br);
            ticksperquarternote = Get2b(br);
            tempo = Get3b(br);
            timesignumerator = Getb(br);
            timesigndemoninator = Getb(br);
            datasize = Get4b(br);
            var track = new SEQTrk(br);
            tracks.Add(track);
            //should be end of file
        }

        public void WriteSMF(BinaryWriter bw)
        {
            PutString("MThd", bw);
            Put4b(6, bw);
            Put2b(0, bw);
            Put2b(1, bw);
            Put2b(ticksperquarternote, bw);

            var track = tracks[0];
            //i thikn theres only one track in these
            PutString("MTrk", bw);
            Put4b(0, bw);
            Putb(0x00, bw);
            Putb(0xff, bw);
            Putb(0x51, bw);
            Putb(0x03, bw);
            Put3b(tempo, bw);
            Putb(0x00, bw);
            Putb(0xff, bw);
            Putb(0x58, bw);
            Putb(0x04, bw);
            Putb(timesignumerator, bw);
            Putb(timesigndemoninator, bw);
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

            foreach(var evt in track.events)
            {
                evt.WriteSMF(bw);
            }
            
        }

        public static void PutVariableB(uint i, BinaryWriter bw)
        {
            int len = varintlen(i);
            for (int dex = 0; dex < len; dex++)
            {
                byte b = (byte)(((i >> (7 * len - dex - 1)) & 0x7f) | ((dex < len - 1) ? 0x80 : 0));
                bw.Write(b);
            }

        }
        public static uint GetVariableB(BinaryReader br)
        {
            int i = 0;
            byte b = br.ReadByte();
            i |= b & 0x7f;
            if (b <= 0x7f)
                return (uint)i;
            i <<= 7;
            b = br.ReadByte();
            i |= b & 0x7f;
            if (b <= 0x7f)
                return (uint)i;
            i <<= 7;
            b = br.ReadByte();
            i |= b & 0x7f;
            if (b <= 0x7f)
                return (uint)i;
            i <<= 7;
            b = br.ReadByte();
            i |= b & 0x7f;
            return (uint)i;
        }

        static int varintlen(uint value)
        {
            int len = 0;
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
        public static void Put2b(ushort s, BinaryWriter bw)
        {
            bw.Write((byte)((s >> 8) & 0xff));
            bw.Write((byte)((s >> 0) & 0xff));
        }
        public static void Put3b(uint i, BinaryWriter bw)
        {
            bw.Write((byte)((i >> 16) & 0xff));
            bw.Write((byte)((i >> 8) & 0xff));
            bw.Write((byte)((i >> 0) & 0xff));
        }
        public static void Put4b(uint i, BinaryWriter bw)
        {
            bw.Write((byte)((i >> 24) & 0xff));
            bw.Write((byte)((i >> 16) & 0xff));
            bw.Write((byte)((i >> 8) & 0xff));
            bw.Write((byte)((i >> 0) & 0xff));
        }

        public static uint Get4b(BinaryReader br)
        {
            int i = 0;
            i |= br.ReadByte() << 24;
            i |= br.ReadByte() << 16;
            i |= br.ReadByte() << 8;
            i |= br.ReadByte() << 0;
            return (uint)i;
        }
        public static uint Get3b(BinaryReader br)
        {
            int i = 0;
            i |= br.ReadByte() << 16;
            i |= br.ReadByte() << 8;
            i |= br.ReadByte() << 0;
            return (uint)i;
        }
        public static ushort Get2b(BinaryReader br)
        {
            int i = 0;
            i |= br.ReadByte() << 8;
            i |= br.ReadByte() << 0;
            return (ushort)i;
        }
        public static byte Getb(BinaryReader br)
        {
            return br.ReadByte();
        }

        public class SEQTrk
        {
            public SEQTrk(BinaryReader br)
            {
                magic = Get4b(br);
                length = Get4b(br);
                long savedpos = br.BaseStream.Position;
                byte lastStatus = 0;
                while(br.BaseStream.Position-savedpos < length)
                {
                    
                    var evt = new SEQEvt(br, lastStatus);
                    lastStatus = evt.status;
                    events.Add(evt);
                }
            }
            public uint magic;//MTrk
            public uint length;//length of event data
            public List<SEQEvt> events = new List<SEQEvt>();
        }

        public class SEQEvt
        {
            int[] evtDataLengths = new[] { 2, 2, 2, 2, 1, 1, 2, 0 };
            
            public SEQEvt(BinaryReader br, byte lastStatus)
            {
                delta = GetVariableB(br);
                status = Getb(br);
                if ((status & 0x80) == 0)
                {
                    //running status
                    status = lastStatus;
                    br.BaseStream.Position -= 1;
                    int datalen = evtDataLengths[(status & 0x7) >> 4];
                    
                    
                    if (status>=0xf0)//dispatch event
                    {
                        if (status != 0xff)
                        {
                            //some kind of error
                            throw new Exception("unknown midi data in seq at 0x" + br.BaseStream.Position.ToString("x"));
                        }
                        //0xff meta events
                        metaType = br.ReadByte();

                        //we need the datalength but seq doesnt have it
                        switch (metaType)
                        {
                            case 0x2f://end of track
                                metaLength = 0;
                                break;
                            case 0x51://tempo
                                metaLength = 3;
                                break;
                            case 0x54://smte offset
                                metaLength = 5;
                                break;
                            case 0x58://time sig
                                metaLength = 4;
                                break;
                            case 0x59://key sig
                                metaLength = 2;
                                break;
                            default:
                                throw new Exception("unknown midi data in seq at 0x" + br.BaseStream.Position.ToString("x"));
                        }

                        if (metaLength>0)
                        {
                            data = new byte[metaLength];
                            br.Read(data, 0, (int)metaLength);
                        }

                    }
                    else
                    {
                        if (datalen > 0)
                        {
                            data = new byte[datalen];
                            br.Read(data, 0, datalen);
                        }
                    }
                }
                //switch status
            }
            public uint delta;
            public byte status;
            public byte[] data = new byte[0];
            public byte metaType = 0;
            public uint metaLength = 0;

            public void WriteSMF(BinaryWriter bw)
            {
                PutVariableB(delta, bw);
                Putb(status, bw);
                if (metaType != 0)
                {
                    Putb(metaType, bw);
                    PutVariableB(metaLength, bw);
                }
                bw.Write(data);
            }
        }

    }

    



}
