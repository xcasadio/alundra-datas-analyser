namespace AlundraEngine.Editor;

public static class PathHelper
{
    public static string GetEtcFileName(string dataFolder)
    {
        //"ETC_RES.R"
        var files = Directory.GetFiles(dataFolder, "*.R");

        if (files.Length == 0)
        {
            throw new FileNotFoundException($"ETC_XXX.R file not found in the specified data folder {dataFolder}.");
        }

        return Path.Combine(dataFolder, files[0]);
    }
}