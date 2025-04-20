using GraphicsTools.Alundra;

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
        var dataFolder = "C:\\Users\\casad\\dev\\repo\\Alundra Remake\\Alundra (France)\\Alundra (France)_extracted\\DATA";

        var etcResRFileName = Path.Combine(dataFolder, "ETC_RES.R");
        var etcResR = new EtcResR(etcResRFileName);

        DisplayInfoEtcResR(etcResR);

        //var datasBinFileName = Path.Combine(dataFolder, "DATAS.BIN");
        //var datasBin = new DatasBin(datasBinFileName);
        //DisplayAllMapOffset(datasBin);
        //DisplayAllMapInfo(datasBin.GameMaps[162]); // Inoa
    }

    private static void DisplayInfoEtcResR(EtcResR etcResR)
    {
        int i = 0;
        Log("Tile");
        foreach (var value in etcResR.TileTable)
        {
            Log($"{i++} {MAGENTA}{value} {BLUE}{etcResR.Strings[value]} {GREEN}{etcResR.Strings[etcResR.StringTable[value]]}{NORMAL}");
        }

        i = 0;
        Log("IconNameTable");
        foreach (var value in etcResR.IconNameTable)
        {
            Log($"{i++} {MAGENTA}{value} {BLUE}{etcResR.Strings[value]} {GREEN}{etcResR.Strings[etcResR.StringTable[value]]}{NORMAL}");
        }

        i = 0;
        Log("PaletteTable");
        foreach (var value in etcResR.PaletteTable)
        {
            Log($"{i++} {MAGENTA}{value} {BLUE}{etcResR.Strings[value]} {GREEN}{etcResR.Strings[etcResR.StringTable[value]]}{NORMAL}");
        }

        i = 0;
        Log("StringTable");
        foreach (var value in etcResR.StringTable)
        {
            Log($"{i++} {MAGENTA}{value} {BLUE}{etcResR.Strings[value]} {GREEN}{etcResR.Strings[etcResR.StringTable[value]]}{NORMAL}");
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
        Log($"{BLUE}InfoBlock {NORMAL}{gameMap.Header.InfoBlock}");
        Log($"{BLUE}MapBlock {NORMAL}{gameMap.Header.MapBlock}");
        Log($"{BLUE}TileSheets {NORMAL}{gameMap.Header.TileSheets}");
        Log($"{BLUE}SpriteInfo {NORMAL}{gameMap.Header.SpriteInfo}");
        Log($"{BLUE}SpriteSheets {NORMAL}{gameMap.Header.SpriteSheets}");
        Log($"{BLUE}ScrollScreen {NORMAL}{gameMap.Header.ScrollScreen}");
        Log($"{BLUE}StringTable {NORMAL}{gameMap.Header.StringTable}");
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

            Log($"[{i}] {BLUE}{portal.X1}{NORMAL}x{BLUE}{portal.Y1}{NORMAL},{BLUE}{portal.X2}{NORMAL}x{BLUE}{portal.Y2}{NORMAL} -> {BLUE}{portal.DestMapId}{NORMAL} {BLUE}{portal.DestX}{NORMAL}x{BLUE}{portal.DestY}{NORMAL} ({portal.Unknown1} {portal.Unknown2} {portal.Unknown3} {portal.Unknown4})");

            //Log($"{BLUE}X1 {NORMAL}{portal.X1}");
            //Log($"{BLUE}Y1 {NORMAL}{portal.Y1}");
            //Log($"{BLUE}X2 {NORMAL}{portal.X2}");
            //Log($"{BLUE}Y2 {NORMAL}{portal.Y2}");
            //Log($"{BLUE}DestMapId {NORMAL}{portal.DestMapId}");
            //Log($"{BLUE}DestX {NORMAL}{portal.DestX}");
            //Log($"{BLUE}DestY {NORMAL}{portal.DestY}");
            //Log($"{BLUE}Unknown1 {NORMAL}{portal.Unknown1}");
            //Log($"{BLUE}Unknown2 {NORMAL}{portal.Unknown2}");
            //Log($"{BLUE}Unknown3 {NORMAL}{portal.Unknown3}");
            //Log($"{BLUE}Unknown4 {NORMAL}{portal.Unknown4}");
        }
        DeIndent();
        DeIndent();

        Log($"Map");
        Indent();
        Log($"Width x Height {BLUE}{gameMap.Map.Width}{NORMAL}x{BLUE}{gameMap.Map.Height}{NORMAL}");
        Log($"Width2 x Height2 {BLUE}{gameMap.Map.Width2}{NORMAL}x{BLUE}{gameMap.Map.Height2}{NORMAL}");
        Log($"WallTilesOffset {BLUE}{gameMap.Map.WallTilesOffset}{NORMAL}");
        Log($"{RED}Missing data{NORMAL}");

        Log($"Tiles");
        Indent();
        foreach (var mapTile in gameMap.Map.MapTiles)
        {
            //mapTile.Height
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