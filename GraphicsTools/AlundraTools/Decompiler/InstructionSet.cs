namespace AlundraTools.Decompiler;

public class InstructionSet
{
    public static int ValAtOffset(uint instruction, int width, int bitoffset)
    {
        return (int)((instruction & (width << bitoffset)) >> bitoffset);
    }

    public static int SignedValAtOffset(uint instruction, int width, int bitoffset)
    {
        var signoffset = 0;
        while (width >> signoffset > 1)
            signoffset++;
        return (int)(ValAtOffset(instruction, width ^ (1 << signoffset), bitoffset) | (ValAtOffset(instruction, 1, signoffset) == 1 ? (0xffffffff >> signoffset) << signoffset : 0));
    }



        

        

        

}