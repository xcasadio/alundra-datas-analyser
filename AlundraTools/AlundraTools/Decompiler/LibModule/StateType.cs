namespace AlundraTools.Decompiler.LibModule;

public enum StateType
{
    Eof = 0,//
    Code = 2,//
    Switch = 6,//
    BssAlloc = 8,
    Patch = 10,
    Def=12,//
    Ref=14,//
    Section=16,//
    Local=18,//
    File=28,
    Processor=46,
    Bss=48//
}