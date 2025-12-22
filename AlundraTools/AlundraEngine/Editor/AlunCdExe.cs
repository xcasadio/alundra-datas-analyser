namespace AlundraEngine.Editor;

public class AlunCdExe
{
    public readonly Bitmap MemoryCardPaletteImage;
    public readonly Bitmap MemoryCardFrame1Image;
    public readonly Bitmap MemoryCardFrame2Image;
    public readonly Bitmap MemoryCardFrame3Image;

    public AlunCdExe(string gamePath)
    {
        try
        {
            var exeFilePath = Path.Combine(gamePath, "ALUN_CD.EXE");

            using var br = new BinaryReader(File.OpenRead(exeFilePath));

            //formule: File Offset = RAM Address - 0x8001F800

            Color[] clut = new Color[16];
            br.BaseStream.Position = 0xA5314; //0x800c4b14 french version
            for (var i = 0; i < 16; i++)
            {
                clut[i] = ImageHelper.FromPsxColor(br.ReadInt16());
            }
            br.BaseStream.Position = 0xA5314; //0x800c4b14 french version
            var bytes = br.ReadBytes(32);
            MemoryCardPaletteImage = ImageHelper.BitmapFromPsxBuff(bytes, 16, 1, 16, null);

            br.BaseStream.Position = 0xA5194; //0x800c4994 french version
            bytes = br.ReadBytes(128);
            MemoryCardFrame1Image = ImageHelper.BitmapFromPsxBuff(bytes, 0, 0, 16, 16, 4, clut);

            br.BaseStream.Position = 0xA5214; //0x800c4a14 french version
            bytes = br.ReadBytes(128);
            MemoryCardFrame2Image = ImageHelper.BitmapFromPsxBuff(bytes, 0, 0, 16, 16, 4, clut);

            br.BaseStream.Position = 0xA5294; //0x800c4a94 french version
            bytes = br.ReadBytes(128);
            MemoryCardFrame3Image = ImageHelper.BitmapFromPsxBuff(bytes, 0, 0, 16, 16, 4, clut);
        }
        catch
        {
            //do nothing
        }
    }
}