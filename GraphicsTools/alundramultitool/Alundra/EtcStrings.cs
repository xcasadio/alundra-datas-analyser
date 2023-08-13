using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GraphicsTools.Alundra
{
    public class EtcStrings
    {
        string[] strings = new string[1024];
        int numstrings = 0;

        public EtcStrings(string file)
        {
            var buff = File.ReadAllBytes(file);
            int dex = 0;
            int offset = buff[dex + 1] << 8 | buff[dex];
            while (offset > 0 && offset < buff.Length)
            {
                StringBuilder sb = new StringBuilder();
                char chr = (char)buff[offset++];
                while (chr != 0)
                {
                    sb.Append(chr);
                    chr = (char)buff[offset++];
                }
                strings[numstrings] = sb.ToString();
                numstrings++;
            }
        }

        public string GetEtcString(int id)
        {
            return strings[id];
        }
    }
}
