using AlundraEngine;
using AlundraEngine.DatasBin;
using AlundraEngine.Text;
using System.Text;

namespace DebugInfo;

internal class Program
{
    static string NL          = Environment.NewLine; // shortcut
    static string NORMAL      = Console.IsOutputRedirected ? "" : "\x1b[39m";
    static string RED         = Console.IsOutputRedirected ? "" : "\x1b[91m";
    static string GREEN       = Console.IsOutputRedirected ? "" : "\x1b[92m";
    static string YELLOW      = Console.IsOutputRedirected ? "" : "\x1b[93m";
    static string BLUE        = Console.IsOutputRedirected ? "" : "\x1b[94m";
    static string MAGENTA     = Console.IsOutputRedirected ? "" : "\x1b[95m";
    static string CYAN        = Console.IsOutputRedirected ? "" : "\x1b[96m";
    static string GREY        = Console.IsOutputRedirected ? "" : "\x1b[97m";
    static string BOLD        = Console.IsOutputRedirected ? "" : "\x1b[1m";
    static string NOBOLD      = Console.IsOutputRedirected ? "" : "\x1b[22m";
    static string UNDERLINE   = Console.IsOutputRedirected ? "" : "\x1b[4m";
    static string NOUNDERLINE = Console.IsOutputRedirected ? "" : "\x1b[24m";
    static string REVERSE     = Console.IsOutputRedirected ? "" : "\x1b[7m";
    static string NOREVERSE   = Console.IsOutputRedirected ? "" : "\x1b[27m";

    static int _indent = 0;

    public static void Main(string[] args)
    {
        var alundraFolder = "C:\\Users\\casad\\dev\\repo\\Alundra Remake\\Alundra (France)\\Alundra (France)_extracted";
        var dataFolder = Path.Combine(alundraFolder, "DATA");

        DisplaySoundListNames();

        //var etcResRFileName = Path.Combine(dataFolder, "ETC_RES.R");
        //var etcResR = new EtcResR(etcResRFileName);
        //DisplayInfoEtcResR(etcResR);

        //var datasBinFileName = Path.Combine(dataFolder, "DATAS.BIN");
        //var datasBin = new DatasBin(datasBinFileName);
        //DisplayAllMapOffset(datasBin);
        //DisplayAllMapInfo(datasBin.GameMaps[162]); // Inoa
    }

    private static void DisplaySoundListNames()
    {
        const long startOffset = 0x80000000;
        //const long headerOffset = 0x800;
        const long tableOffset = 0x800A7488 - startOffset; // + headerOffset;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding shiftJis = Encoding.GetEncoding("shift_jis");

        var exeFile = @"C:\Users\casad\AppData\Roaming\pcsx-redux\SLES01198_mem_wram.bin";

        using var fs = new FileStream(exeFile, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        fs.Seek(tableOffset, SeekOrigin.Begin);

        List<uint> addresses = new();

        // Lire tous les pointeurs
        while (true)
        {
            var add = br.ReadUInt32();

            if (add == 0x0)
            {
                break;
            }

            addresses.Add(add);
            Log($"0x{add:X8}");
        }

        foreach (uint addr in addresses)
        {
            long dataOffset = addr - startOffset; // + headerOffset;
        
            if (dataOffset < 0)
            {
                Log($"Invalid address: 0x{addr:X8}");
                continue;
            }

            fs.Seek(dataOffset, SeekOrigin.Begin);

            List<byte> buffer = new List<byte>();

            while (true)
            {
                var val = br.ReadByte();

                if (val == 0x0)
                {
                    break;
                }

                buffer.Add(val);
            }
            
            var array = buffer.ToArray();
            string decodedString = shiftJis.GetString(array);
            string text = System.Text.Encoding.ASCII.GetString(array);
            Console.WriteLine("Data at 0x{0:X8}: {1} -> {2}", addr, text, decodedString);
        }
    }

    private static void DisplayInfoEtcResR(EtcResR etcResR)
    {
        int i = 0;
        Log("Tile");
        foreach (var value in etcResR.TileTable)
        {
            Log($"{i++} {MAGENTA}{value} {BLUE}{etcResR.Strings[value]}{NORMAL}");
        }

        i = 0;
        Log("IconNameTable");
        foreach (var value in etcResR.IconNameTable)
        {
            Log($"{i++} {MAGENTA}{value}{NORMAL}");
        }

        i = 0;
        Log("PaletteTable");
        foreach (var value in etcResR.PaletteTable)
        {
            Log($"{i++} {MAGENTA}{value} {BLUE}{etcResR.Strings[value]}{NORMAL}");
        }

        i = 0;
        Log("StringTableOffset");
        foreach (var value in etcResR.StringTable)
        {
            Log($"{i++} {MAGENTA}{value}{NORMAL}");
        }

        i = 0;
        Log("Strings");
        foreach (var value in etcResR.Strings.Where(x => x != null))
        {
            Log($"{i++} {GREEN}{value}{NORMAL}");
        }
    }

    private static void DisplayAllMapInfo(GameMap gameMap)
    {
        Log($"Header");
        Indent();
        Log($"{BLUE}InfoBlockOffset {NORMAL}{gameMap.Header.InfoBlockOffset}");
        Log($"{BLUE}MapBlockOffset {NORMAL}{gameMap.Header.MapBlockOffset}");
        Log($"{BLUE}TileSheetsOffset {NORMAL}{gameMap.Header.TileSheetsOffset}");
        Log($"{BLUE}SpriteInfoOffset {NORMAL}{gameMap.Header.SpriteInfoOffset}");
        Log($"{BLUE}SpriteSheetsOffset {NORMAL}{gameMap.Header.SpriteSheetsOffset}");
        Log($"{BLUE}ScrollScreenOffset {NORMAL}{gameMap.Header.ScrollScreenOffset}");
        Log($"{BLUE}StringTableOffset {NORMAL}{gameMap.Header.StringTableOffset}");
        DeIndent();

        Log($"Info");
        Indent();
        Log($"{BLUE}MapId {NORMAL}{gameMap.Info.MapId}");
        Log($"{BLUE}Gravity {NORMAL}{gameMap.Info.Gravity}");
        Log($"{BLUE}TerminalVelocity {NORMAL}{gameMap.Info.TerminalVelocity}");
        Log($"{BLUE}SlideEffectId {NORMAL}{gameMap.Info.SlideEffectId}");
        Log($"{BLUE}BalanceLevel {NORMAL}{gameMap.Info.BalanceLevel}");
        Log($"{BLUE}C {NORMAL}{gameMap.Info.C}");
        Log($"{BLUE}D {NORMAL}{gameMap.Info.D}");
        Log($"{BLUE}E {NORMAL}{gameMap.Info.E}");
        Log($"{BLUE}F {NORMAL}{gameMap.Info.F}");
        Log($"{BLUE}_10 {NORMAL}{gameMap.Info._10}");
        Log($"{BLUE}Palettes");
        Log($"{BLUE}PortalFlag1 {NORMAL}{gameMap.Info.PortalFlag1}");
        Log($"{BLUE}PortalFlag2 {NORMAL}{gameMap.Info.PortalFlag2}");

        Log($"Portals");
        Indent();
        for (var i = 0; i < gameMap.Info.Portals.Length; i++)
        {
            var portal = gameMap.Info.Portals[i];

            if (portal.DestMapId == 0)
            {
                continue;
            }

            Log($"[{i}] {BLUE}{portal.X1}{NORMAL}x{BLUE}{portal.Y1}{NORMAL},{BLUE}{portal.X2}{NORMAL}x{BLUE}{portal.Y2}{NORMAL} -> {BLUE}{portal.DestMapId}{NORMAL} {BLUE}{portal.DestTileX}{NORMAL}x{BLUE}{portal.DestTileY}{NORMAL} ({portal.ZLevel} {portal.Flags})");

            //Log($"{BLUE}X {NORMAL}{portal.X}");
            //Log($"{BLUE}Y {NORMAL}{portal.Y}");
            //Log($"{BLUE}Width {NORMAL}{portal.Width}");
            //Log($"{BLUE}Height {NORMAL}{portal.Height}");
            //Log($"{BLUE}DestMapId {NORMAL}{portal.DestMapId}");
            //Log($"{BLUE}DestTileX {NORMAL}{portal.DestTileX}");
            //Log($"{BLUE}DestTileY {NORMAL}{portal.DestTileY}");
            //Log($"{BLUE}Unknown1 {NORMAL}{portal.Unknown1}");
            //Log($"{BLUE}Unknown2 {NORMAL}{portal.Unknown2}");
            //Log($"{BLUE}Unknown3 {NORMAL}{portal.Unknown3}");
            //Log($"{BLUE}Unknown4 {NORMAL}{portal.Unknown4}");
        }
        DeIndent();
        DeIndent();

        Log($"Map");
        Indent();
        Log($"SizeX x SizeZ {BLUE}{gameMap.Map.Width}{NORMAL}x{BLUE}{gameMap.Map.Height}{NORMAL}");
        Log($"Width2 x Height2 {BLUE}{gameMap.Map.Width2}{NORMAL}x{BLUE}{gameMap.Map.Height2}{NORMAL}");
        Log($"WallTilesOffset {BLUE}{gameMap.Map.WallTilesOffset}{NORMAL}");
        Log($"{RED}Missing data{NORMAL}");

        Log($"Tiles");
        Indent();
        foreach (var mapTile in gameMap.Map.MapTiles)
        {
            //mapTile.SizeZ
        }
        DeIndent();
        DeIndent();


        Log($"Strings");
        Indent();
        foreach (var @string in gameMap.Strings)
        {
            if (!string.IsNullOrWhiteSpace(@string))
            {
                Log(TextInterpreter.DecodeString(@string));
            }
        }
        DeIndent();
    }

    private static void DisplayAllMapOffset(DatasBin datasBin)
    {
        for (int i = 0; i < datasBin.GameMaps.Length; i++)
        {
            var gameMap = datasBin.GameMaps[i];
            if (gameMap == null)
            {
                continue;
            }

            Log($"Map[{GREEN}{gameMap.Info.MapId}{NORMAL}] {BLUE}{gameMap.Offset}{NORMAL} {GREEN}{gameMap.Map.Width}{NORMAL}x{GREEN}{gameMap.Map.Height}");
        }
    }

    private static void Log(string message)
    {
        for (int i = 0; i < _indent; i++)
        {
            Console.Write("  ");
        }

        Console.WriteLine(message);
    }

    private static void DeIndent()
    {
        _indent--;
    }

    private static void Indent()
    {
        _indent++;
    }
}