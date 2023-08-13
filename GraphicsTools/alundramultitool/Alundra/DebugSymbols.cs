using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsTools.Alundra
{
    public static class DebugSymbols
    {
        public static Dictionary<string, Dictionary<byte, string>> EventHandlerNames = new Dictionary<string, Dictionary<byte, string>>();

        public static string[] EntityVarOffsets = new string[GameMap.eventobject_size];
        public static Dictionary<uint, NameComment> FunctionNames = new Dictionary<uint, NameComment>();
        public static Dictionary<uint, NameComment> GlobalVariableNames = new Dictionary<uint, NameComment>();
        public static Dictionary<uint, string> Comments = new Dictionary<uint, string>();
        public static string[] MapNames = new string[502];

        public static uint Adjustment = 0 + 0xdc;

        public class NameComment
        {
            public string name;
            public string comment;
        }

        static void AddHandlerName(string handlertype, byte eventcode, string name)
        {
            EventHandlerNames[handlertype][eventcode] = name;
        }

        static void AddFunction(uint addr, string name, string comment)
        {
            FunctionNames.Add(addr, new NameComment { name = name, comment = comment });
        }
        static void AddGlobalVariable(uint addr, string name, string comment)
        {
            GlobalVariableNames.Add(addr, new NameComment { name = name, comment = comment });
        }

        static void AddComment(uint addr, string comment)
        {
            Comments.Add(addr, comment);
        }

        static void AddGlobalVariableRange(uint addr_start,uint addr_end, string name, string comment)
        {
            for (uint addr = addr_start; addr <= addr_end; addr++)
            {
                GlobalVariableNames.Add(addr, new NameComment { name = name, comment = comment });
            }
        }
        public static void Init()
        {
            Comments.Clear();
            GlobalVariableNames.Clear();
            FunctionNames.Clear();
            EventHandlerNames = new Dictionary<string, Dictionary<byte, string>>();
            EventHandlerNames.Add("eload", new Dictionary<byte, string>());
            EventHandlerNames.Add("etick", new Dictionary<byte, string>());
            EventHandlerNames.Add("etouch", new Dictionary<byte, string>());
            EventHandlerNames.Add("edeactivate", new Dictionary<byte, string>());
            EventHandlerNames.Add("einteract", new Dictionary<byte, string>());

            AddFunction(0x2abe8, "debugcheck", "tons of potential error messages");
            AddFunction(0x2bc18, "update", "main update");
            AddFunction(0x2bdb8, "render", "renders maps and sprites");
            AddFunction(0x2bef0, "", "calls 2e2c0");
            AddFunction(0x2c038, "main", "main loop");
            AddFunction(0x2cf8c, "rendermap", "something with map");
            AddFunction(0x2e2c0, "update", "calls 3bbf0");
            AddFunction(0x2e4c8, "writeinputotlocation", "");
            AddFunction(0x2e61c, "getplayerinput", "");
            AddFunction(0x2f440, "checkportals", "");
            AddFunction(0x30a3c, "seektozero", "called in unknownplayerupdate");
            AddFunction(0x30bb0, "unknownplayerupdate", "called inside player update");
            AddFunction(0x317a4, "domapwarp", "");
            AddFunction(0x31b4c, "getportalunderplayer", "");
            AddFunction(0x32098, "moveplayer", "checkportals moveplayer big function");
            AddFunction(0x36c54, "setridingentities", "");
            AddFunction(0x36da4, "setxyforces", "");
            AddFunction(0x36e88, "setadjustedxyforces", "");
            AddFunction(0x36f64, "incrementforce", "");
            AddFunction(0x36f9c, "updateforces", "");
            AddFunction(0x373cc, "collidewithentitiesjumping", "");
            AddFunction(0x37564, "collidewithentities", "");
            AddFunction(0x37704, "collideentities2", "");
            AddFunction(0x378a0, "collidewithmap", "");
            AddFunction(0x37b4c, "updateridingentity", "");
            AddFunction(0x37ddc, "movez", "");
            AddFunction(0x37f2c, "movexy", "");
            AddFunction(0x3862c, "moveentity", "");
            AddFunction(0x38724, "colideentitiesz", "");
            AddFunction(0x3886c, "updatetile", "");
            AddFunction(0x38b68, "dophysics", "");
            AddFunction(0x38cf0, "addtolists", "");
            AddFunction(0x38e48, "processdestroyedentities", "");
            AddFunction(0x38ee8, "doevents", "");
            AddFunction(0x391a0, "updatecounters", "after doevents");
            AddFunction(0x392c8, "updateanim", "(entity)");
            AddFunction(0x39648, "updateanims", "called before dophysics");
            AddFunction(0x396ac, "updateactiveeffect", "called after dophysics, unknown 1b8 pointer");
            AddFunction(0x39b24, "updatebalancerecord", "after dophysics");
            AddFunction(0x3a1e4, "setdepthsortvals", "");
            AddFunction(0x3bbf0, "updateentities", "calls doevents and dophysics");
            AddFunction(0x3c240, "getnexteffectrecord", "sprite related thing record, 0x80 long, gets empty one");
            AddFunction(0x3c278, "geteffectspritetablerecord", "(ismapsprite, tableindex, out addtosheet, out addtopal)");
            AddFunction(0x3c390, "initeffect", "initeffect(effectrecord, param1, param2, param3, prev1, prev2, prev3, x, y, z)");
            AddFunction(0x3c46c, "updateeffectanims", "loads in animsets, advances animations/etc");
            AddFunction(0x3cb88, "updateeffectbytype", "update effect positions based on type");
            AddFunction(0x3c684, "createeffect_type0", "(ismapsprite,effectid,animid,x,y,z)");
            AddFunction(0x3c730, "createeffect_type1", "(ismapsprite,effectid,animid,entity,unknown,xoff,yoff,zoff)");
            AddFunction(0x3c8d0, "createeffect_type3", "(ismapsprite, effectid, animid, entity, unknown, x, y, z)");
            AddFunction(0x3c99c, "createeffect_maptype", "(effectid, checkboundingbox) this is used by events");
            AddFunction(0x3cd18, "updateeffects", "after calls doevents, creates spriterefs from the effects");
            AddFunction(0x3cf7c, "updatemapevents", "before calls doevents");
            AddFunction(0x3d298, "getentityfromrefid", "can also get entities");
            AddFunction(0x3d160, "outputdebuginfo", "");
            AddFunction(0x2cba0, "outputpatchdebuginfo", "patch data error");
            AddFunction(0x42964, "initeventdata", "(entity,eventprogramtype,eventdata)");
            AddFunction(0x42adc, "runentityeventscripts", "innerdoevents");
            AddFunction(0x432a4, "presentframe", "advances frame and sound");
            AddFunction(0x48ed4, "drawui", "overlays ui/dialogs");
            AddFunction(0x4a04c, "playsoundeffect", "");
            AddFunction(0x4c2a4, "playmusic", "");
            AddFunction(0x4f9e8, "seektozero", "called in unknownplayerupdate");
            AddFunction(0x50054, "", "something with sound.bin");
            AddFunction(0x83e08, "printdebug","");
            AddFunction(0x83e18, "printdebugparams","");
            AddFunction(0x84ef8, "printdebugerror", "");
            AddFunction(0x857c0, "debugfunction", "");
            AddFunction(0x89e98, "", "calls cd");
            AddFunction(0x8a4c0, "diskread", "");
            AddFunction(0x5c800, "opencdfile", "");
            AddFunction(0x8794c, "putdrawenv", "");
            AddFunction(0x87b24, "putdispenv", "");
            AddFunction(0x42fec, "somedrawcommands", "");
            //new[] { "outputdebuginfo", "printdebug", "printdebugparams", "printdebugerror" }

            AddFunction(0x2e720, "validateclut", "");
            AddFunction(0x2e968, "processpalettes", "");
            AddFunction(0x2c9e0, "checkcamerapans", "");
            AddFunction(0x2e3a4, "rendersprites", "");
            AddFunction(0x2ddb8, "rendersprite", "");

            AddFunction(0x869bc, "SetPolyFT4", "setcmdto4pointtexturedpoly");
            AddFunction(0x86a20, "SetSprt", "setcmdtosomething1");
            AddFunction(0x86930, "SetShadeTex", "setsomethingtocmd3");
            AddFunction(0x84598, "strcpy", "(dest, src)");
            AddFunction(0x8460c, "strcpyn", "(dest, src, length)");
            AddFunction(0x8448c, "strcat", "(dest, src)");
            AddFunction(0x87508, "clearimage", "");
            AddFunction(0x8759c, "loadimage", "");
            AddFunction(0x87600, "storeimage", "");
            AddFunction(0x87374, "drawsync", "");

            AddFunction(0x4f2b4, "getsomething00", "");
            AddFunction(0x4f1e8, "getsomething02", "");
            AddFunction(0x4f44c, "getsomething04", "");
            AddFunction(0x4f4c0, "getsomething04special", "(val)");
            AddFunction(0x4f4ec, "getsomething04specialsub", "(val)//subtracts provided val instead of adds");
            AddFunction(0x4f464, "getsomething04specialinner", "(val)");
            AddFunction(0x4f380, "getsomething06", "");
            AddFunction(0x4f3f4, "getsomething06special", "(val)");
            AddFunction(0x4f398, "getsomething06specialinner", "(val)");
            AddFunction(0x4f518, "getsomething08", "");
            AddFunction(0x4f58c, "getsomething08special", "(val)");
            AddFunction(0x4f5b8, "getsomething08specialsub", "(val)//subtracts provided val instead of adds");
            AddFunction(0x4f530, "getsomething08specialinner", "(val)");
            AddFunction(0x4fd98, "getsomething0e", "");
            AddFunction(0x4fd80, "getsomething10", "");
            AddFunction(0x4fd28, "processsomething", "");
            AddFunction(0x48374, "processsunknownvals", "");
            AddFunction(0x508a8, "validatekanji", "(text) //makes system call");

            AddGlobalVariable(0x1530d4, "dialogtextlinestartx", "");
            AddGlobalVariable(0x107308, "dialogtextsfx", "");
            AddGlobalVariable(0x119888, "something", "");
            AddGlobalVariable(0x10721c, "dialogrendercharcounter", "");

            AddFunction(0x4c378, "playmusic", "");

            AddFunction(0x83e00, "emptyfunc", "");


            AddFunction(0x59a08, "wrapssetupdialogportrait", "(x,y,z,camx,camy,sx,sy,width,height,palette,spritesheet)");
            AddFunction(0x59a74, "setupdialogportrait", "(x,y,z,camx,camy,sx,sy,width,height,palette,spritesheet)//sets vals to");
            AddFunction(0x5bfcc, "setname", "(nameid)");
            AddFunction(0x48de0, "setuirecordcallsetup", "(uirecordid)");
            AddFunction(0x42e98, "settext", "(textid,playercontrolflag)");
            AddFunction(0x45fb0, "setupdialogdrawcmds", "(text,playercontrolflag)");
            AddFunction(0x8438c, "zerooutmemory", "ptr, length");
            AddFunction(0x83e98, "emptyfunction", "");
            AddFunction(0x83ee8, "getetcstring", "etcid");
            AddFunction(0x45f08, "getdialocchoiceinner", "");
            AddFunction(0x42f2c, "isdialogactive", "");
            AddFunction(0x45eb0, "isdialogactiveinner", "  *dialogstate & 4 != 0");
            AddFunction(0x45f30, "setdialogdialogchoiceinner", "(val)");
            AddFunction(0x45f68, "setdialogsomethinginner", "(val)");
            AddFunction(0x45f40, "checkdialogsomethinginner", "");
            AddFunction(0x5999c, "wrapsetupdialogportrait2", "(x,y,z,camx,camy,sx,sy,width,height,palette,spritesheet)//sets vals to");

            AddGlobalVariable(0x12b780, "etcstringtable", "");
            AddGlobalVariable(0x12e020, "etcstringtableptr", "");
            AddGlobalVariable(0x1072e8, "dialogchoice", "");
            AddGlobalVariable(0x1072f8, "dialogchoicesaved", "");
            AddGlobalVariable(0x107200, "dialogsomething", "");
            AddGlobalVariable(0x107204, "dialogsomethingbit3on", "");
            AddGlobalVariable(0x1ef1ac, "dialogvalx", "");
            AddGlobalVariable(0x1ef1b0, "dialogvaly", "");
            AddGlobalVariable(0x1ef188, "dialogvalxsaved", "");
            AddGlobalVariable(0x1ef18c, "dialogvalysaved", "");
            AddGlobalVariable(0x1ef190, "dialogvalxsaved2", "");
            AddGlobalVariable(0x1ef194, "dialogvalysaved2", "");
            AddGlobalVariable(0x1ef198, "dialogvalxmodded", "");
            AddGlobalVariable(0x1ef19c, "dialogvalymodded", "");

            AddGlobalVariable(0x1ef1a0, "dialogvalunknown1", "");
            AddGlobalVariable(0x1ef1a4, "dialogvalunknown2", "");
            AddGlobalVariable(0x1ef1a8, "dialogvalunknown3", "");

            AddGlobalVariable(0x1ef174, "dialogxpos", "");
            AddGlobalVariable(0x1ef178, "dialogypos", "");
            AddGlobalVariable(0x1ef17c, "dialogzpos", "");
            AddGlobalVariable(0x1ef180, "dialogcamxpos", "");
            AddGlobalVariable(0x1ef184, "dialogcamypos", "");
            AddGlobalVariable(0x12e038, "uirecordid", "");
            AddGlobalVariable(0x153154, "uirecord", "");
            AddGlobalVariable(0xa8f24, "uirecordsrc.status", "");
            AddGlobalVariable(0xa8f28, "uirecordsrc.funcptr1", "");
            AddGlobalVariable(0xa8f2c, "uirecordsrc.x", "");
            AddGlobalVariable(0xa8f2e, "uirecordsrc.y", "");
            AddGlobalVariable(0xa8f30, "uirecordsrc.width", "");
            AddGlobalVariable(0xa8f32, "uirecordsrc.height", "");
            AddGlobalVariable(0xa8f34, "uirecordsrc.setupfunc", "");
            AddGlobalVariable(0xa8f38, "uirecordsrc.renderfunc", "");
            AddGlobalVariable(0xa8f3c, "uirecordsrc.funcptr2", "");
            AddGlobalVariable(0x1efba8, "dialognameboxstate", "");
            AddGlobalVariable(0x1efbac, "dialognameboxlerper.curtick", "");
            AddGlobalVariable(0x1efbb0, "dialognameboxlerper.ticks", "");
            AddGlobalVariable(0x1efbb4, "dialognameboxlerper.tickstolinger", "");
            AddGlobalVariable(0x1efbb8, "dialognameboxlerper.x1", "");
            AddGlobalVariable(0x1efbba, "dialognameboxlerper.y1", "");
            AddGlobalVariable(0x1efbbc, "dialognameboxlerper.x2", "");
            AddGlobalVariable(0x1efbbe, "dialognameboxlerper.y2", "");
            AddGlobalVariable(0x1efbc4, "dialognameboxlerperafterx", "");
            AddGlobalVariable(0x1efbc6, "dialognameboxlerperaftery", "");

            AddGlobalVariable(0xc5c14, "stringptrtable", "");

            AddGlobalVariable(0x1ddab0, "fontbitmap", "");


            AddFunction(0x491a4, "inituifunc_1_dlg", "used by dialogbox");
            AddFunction(0x4c998, "inituifunc_2_main", "used by main ui");
            AddFunction(0x550d4, "inituifunc_3", "");
            AddFunction(0x5c300, "inituifunc_4_dlgname", "used by dialog name box");

            AddFunction(0x47de4, "renderdialogboxfunc", "");
            AddFunction(0x4d218, "rendermainuifunc", "");
            AddFunction(0x50bcc, "render2func", "");
            AddFunction(0x518c4, "render3func", "");
            AddFunction(0x54bcc, "render4func", "");
            AddFunction(0x4ba10, "render5func", "");
            AddFunction(0x5695c, "renderitemmenufunc", "");
            AddFunction(0x4c170, "render8func", "");
            AddFunction(0x52584, "render9func", "");
            AddFunction(0x5a1f8, "renderafunc", "");
            AddFunction(0x52c50, "renderbfunc", "");
            AddFunction(0x5c4ac, "renderdialognameboxfunc", "");

            AddFunction(0x48c1c, "lerpuibox", "(boxdrawer, lerper)");
            AddFunction(0x45ec8, "zerodialogstate", "(ui)");
            AddFunction(0x48af4, "zerouirecord", "(ui)");
            AddFunction(0x5c2d8, "zerodialognamestate", "(ui)");
            AddFunction(0x48570, "getrenderedtextwidth", "(text)");
            AddFunction(0x481c4, "rendertext", "(cmdlist,text)");
            AddFunction(0x5aa2c, "renderarenderinner", "");
            AddFunction(0x486a4, "rendertextinner", "(linetext, outputbitmap, vramx, vramy, startx, starty, outputbitmapwidth, outputbitmapheight )");
            AddFunction(0x46ed8, "renderdialogtext", "");

            AddGlobalVariable(0x9b9cc, "fontcharinfos.kerning", "20 byte records for each ascii character");
            AddGlobalVariable(0x9b9d0, "fontcharinfos.height", "character height");
            AddGlobalVariable(0x9b9d4, "fontcharinfos.sx", "20 byte records for each ascii character");
            AddGlobalVariable(0x9b9d8, "fontcharinfos.sy", "20 byte records for each ascii character");
            AddGlobalVariable(0x9b9dc, "fontcharinfos.y", "output y offset");

            AddGlobalVariable(0x1d0598, "rendertextbuff", "");
            AddGlobalVariable(0x142900, "uipalettes", "");

            AddGlobalVariable(0x107208, "dialogletterwait", "");
            AddGlobalVariable(0x10720c, "dialogletterwaitremaining", "");
            
            AddGlobalVariable(0x107218, "dialogtextbufferpos", "");

            AddGlobalVariableRange(0x1f5140, 0x1f5167, "drawarearecordsrange", "40 byte records (2 of them?)(linked list of sorts?)");

            AddGlobalVariableRange(0x1efbc8, 0x1efbdc, "dialognamecmdlist", "20 byte records (2 of them?)(other half of linked list of sorts?)");

            AddGlobalVariable(0x1f3748, "drawareas", "");
            AddGlobalVariable(0x1e5cd8, "drawareaid", "");
            AddGlobalVariable(0x1045e0, "dialogboxlerper.curtick", "");
            AddGlobalVariable(0x1045e4, "dialogboxlerper.ticks", "");
            AddGlobalVariable(0x1045e8, "dialogboxlerper.tickstolinger", "");
            AddGlobalVariable(0x1045ec, "dialogboxlerper.x1", "");
            AddGlobalVariable(0x1045ee, "dialogboxlerper.y1", "");
            AddGlobalVariable(0x1045f0, "dialogboxlerper.x2", "");
            AddGlobalVariable(0x1045f2, "dialogboxlerper.y2", "");

            AddGlobalVariable(0x1045f8, "dialogboxlerperafterx", "");
            AddGlobalVariable(0x1045fa, "dialogboxlerperaftery", "");

            AddGlobalVariable(0x9ebc4, "boxdrawer1.x", "");
            AddGlobalVariable(0x9ebc6, "boxdrawer1.y", "");
            AddGlobalVariable(0x9ebc8, "boxdrawer1.width", "");
            AddGlobalVariable(0x9ebca, "boxdrawer1.height", "");

            AddGlobalVariable(0xa6bf4, "boxdrawer2", "");
            AddGlobalVariable(0x1072ec, "dialogchoiceunknown1", "");
            AddGlobalVariable(0x1072cc, "dialogchoiceunknown2", "");

            AddFunction(0x3a3e4, "getinitdata", "");//20 byte long records

            AddFunction(0x3aa0c, "activateentity", "");
            AddFunction(0x3a348, "getspritefromspritetable", "(ismapsprite, tableindex, outvar1, outvar2)");
            AddFunction(0x3a2ec, "getnextavailableentity", "");
            AddFunction(0x3a51c, "initentity", "(entity,ownerentity,sprite,initdata,spritetableindex,entityid,x,y,z,0,dir,outvar1,outvar2)");
            AddFunction(0x2eaac, "dirfromvector", "");
            AddFunction(0x453b4, "getbalancerecordfromspriteindex", "");
            AddFunction(0x4542c, "getbalancerecordfromspriteindex2", "");
            AddFunction(0x42a9c, "initcodeprograms", "");
            AddFunction(0x3a458, "initentitydimensions", "");
            AddFunction(0x33078, "maybegetcontents", "");
            AddFunction(0x03afec, "hideentity", "(entity)");
            AddFunction(0x3d97c, "turnentity", "(entity,turncode)");
            AddFunction(0x3d8d4, "getcardinaldirtoplayer", "(entity)");
            AddFunction(0x399a8, "createrandompoofs", "(x, y, z)");
            AddFunction(0x39fd4, "setdepthsortval", "(entity)");
            AddFunction(0x2e5d0, "initinput", "");
            AddFunction(0x45a7c, "initui", "");
            AddFunction(0x48a94, "initdialog", "");
            AddFunction(0x598a0, "initdialogvals", "");
            AddFunction(0x5c150, "inidialognamebox", "");

            AddFunction(0x456b8, "loaduibitmaps", "");
            AddFunction(0x459e0, "loaduipalettes", "");
            AddFunction(0x45e24, "loadfontbitmap", "");

            //spriteevents
            AddFunction(0x3ae08, "destroyentity", "(entity,animid) -1 loads animid from spriterecord");
            AddFunction(0x331cc, "spawnentitycontents", "(entity) uses 3c,40,274,278,27c,280, and some 8 byte lookuptable");
            AddFunction(0x3a784, "spawnentity", "(ownerentity,ismapsprite,tableindex,xpos,ypos,zpos,dir) //called by a ton of stuff");
            AddFunction(0x32fa8, "getcontentsitemid", "(contentsid) uses randomized tables to return itemid");
            AddFunction(0x3303c, "checkitemid", "(itemid) not fully implimented");




            AddComment(0x2c1d4, "dbininfo.gamemaps");
            AddComment(0x2c1d8, "dbininfo.gamemaps+1");
            AddComment(0x2c408, "gamemap[c]*8");
            AddComment(0x2c414, "gamemap[d]*8");
            AddComment(0x2c418, "gamemap[e]*8");
            //init functions
            AddFunction(0x3baa8, "loadentities", "");
            AddFunction(0x3ce18, "loadmapevents", "");
            AddFunction(0x3caac, "loadmapeffects", "");
            AddFunction(0x45388, "initbalancedata", "(balancelevel) //mapinfo[0xb]");
            AddFunction(0x45354, "loadbalancebinfile", "(0, 0)");
            AddFunction(0x2e0b0, "loadglobalspritetable", "(filename, spriteinfooffset, spritesoffset, spritesrepeatoffset, stringtableoffset)");
            AddFunction(0x2e220, "loadmapspritetable", "(spriteinfoptr)");
            AddFunction(0x2cc74, "initmapstuff", "(umapaoffset)");
            AddFunction(0x83e28, "loadfromdisk", "(filename, buffer, binoffset, datalength)");
            AddFunction(0x50144, "loadfromdiskinner", "");
            AddFunction(0x42e48, "loadglobalstrings", "(filename, stringsrepeatoffset)");
            AddFunction(0x42e88, "loadmapstrings", "(stringtableptr)");
            AddFunction(0x2ce10, "initmap", "(infoblockptr, mapblockptr, tilesheetsptr)");
            AddFunction(0x2e24c, "loadspriteinfo", "(spritesheetptr)");
            AddFunction(0x5d2f8, "unknown", "(curmapid,scrollscreenptr)");

            AddFunction(0x5f6b0, "unknown", "(0x340,0x100,0x100,0x1f0,*0x1f2bd6, *0x1f2c02, *0x1f2c2e, *0x1f2c5a, 0x12e160)");
            AddFunction(0x44edc, "unknown", "(gamemap[c]*8,gamemap[d]*8,gamemap[e]*8,warpinfo.98)");
            AddFunction(0x5ca04, "loadaran_xa", "//intro music or video?");
            AddGlobalVariable(0x121c6c, "aranxacdpos", "");

            AddFunction(0x83de0, "getdatafilescdpos", "");
            AddFunction(0x4fdb0, "getdatafilescdpos_inner", "");
            AddGlobalVariable(0x1192a4, "soundbinposint", "");
            AddFunction(0x5fcf8, "junkfunction?", "");
            AddFunction(0x2c71c, "loadetc_usar", "");

            //general std
            AddFunction(0x8444c, "rand", "");
            AddFunction(0x843e8, "tolower", "");
            AddFunction(0x84238, "atoi", "");
            AddFunction(0x846cc, "todigit", "");
            AddFunction(0x8dcd4, "PCread", "or write");
            AddFunction(0x8dd94, "_SN_read", "or read");
            AddFunction(0x8ddac, "PCwrite", " or read");
            AddFunction(0x8de6c, "_SN_write", "or write");

            //audio sdk
            AddFunction(0x8e20c, "SsSeqOpen", "");
            AddFunction(0x8472c, "printf", "");
            AddFunction(0x8ded4, "_snd_openflag", "");
            AddFunction(0x90640, "SsInit", "");
            AddFunction(0x8cf18, "ResetCallback", "");
            AddFunction(0x97c1c, "SpuInit", "");
            AddFunction(0x97c3c, "_SpuInit", "");
            AddFunction(0x90558, "_SsInit", "");
            AddFunction(0x90510, "SsSeqClose", "");
            AddFunction(0x90398, "_SsClose", "");
            AddFunction(0x96284, "SpuVmSetSeqVol", "");
            AddFunction(0x96500, "SpuVmSeqKeyOff", "");
            AddFunction(0x8cfa8, "VSyncCallback", "");
            AddFunction(0x91794, "SsSeqPlay", "");
            AddFunction(0x91694, "SpuVmSetSeqVol", "");
            AddFunction(0x91fb0, "SsVabClose", "");
            AddFunction(0x995c8, "SpuFree", "");
            AddFunction(0x992c8, "_spu_gcSPU", "");
            AddFunction(0x920b8, "SsVabOpenHeadSticky", "");
            AddFunction(0x92118, "SsVabOpenHeadWithMode", "");
            AddFunction(0x9a7a4, "_spu_getInTransfer", "");
            AddFunction(0x9a778, "_spu_setInTransfer", "");
            AddFunction(0x99000, "SpuMalloc", "");
            AddFunction(0x925cc, "SsVabTransBodyPartly", "");
            AddFunction(0x9a610, "SpuSetTransferMode", "");
            AddFunction(0x9a5d4, "SpuSetTransferStartAddr", "");
            AddFunction(0x98dc4, "_spu_FsetRXXa", "");
            AddFunction(0x9a644, "SpuWritePartly", "");
            AddFunction(0x98c8c, "_spu_write", "");
            AddFunction(0x97d04, "SpuStart", "");
            AddFunction(0x84000, "EnterCriticalSection", "");
            AddFunction(0x98ea4, "_SpuDataCallback", "");
            AddFunction(0x83fc0, "OpenEvent", "");
            AddFunction(0x83ff0, "EnableEvent", "");
            AddFunction(0x84010, "ExitCriticalSection", "");
            AddFunction(0x9a7b4, "SpuSetCommonAttr", "");
            AddFunction(0x9ac18, "SpuGetAllKeysStatus", "");
            AddFunction(0x914b8, "SsSetTableSize", "");
            AddFunction(0x91298, "SsSetTickMode", "");
            AddFunction(0x8dac0, "GetVideoMode", "");
            AddFunction(0x92774, "SsUtFlush", "");
            AddFunction(0x95728, "SpuVmFlush", "");
            AddFunction(0x927b4, "SsUtGetProgAtr", "");
            AddFunction(0x928b8, "SpuVmVSetUp", "");
            AddFunction(0x9297c, "SsUtGetVagAtr", "(vabid, progNum, toneNum, vagatrptr)");
            AddFunction(0x92bb4, "SsUtGetVBaddrInSB", "");
            AddFunction(0x978a4, "SsUtSetDetVVol", "");
            AddFunction(0x998d4, "SpuSetReverbModeParam", "");
            AddFunction(0x9985c, "_SpuIsInAllocateArea_", "");
            AddFunction(0x99de4, "_spu_setReverbAttr", "");
            AddFunction(0x9a350, "SpuClearReverbWorkArea", "");
            AddFunction(0x989e4, "_spu_t", "");
            AddFunction(0x9a4ec, "WaitEvent", "");
            AddFunction(0x98d7c, "_spu_FsetRXX", "");
            AddFunction(0x98f54, "SpuSetMute", "");
            AddFunction(0x98fac, "SpuInitMalloc", "");
            AddFunction(0x9971c, "SpuSetReverb", "");
            AddFunction(0x9a2b4, "SpuSetReverbDepth", "");
            AddFunction(0x9a4fc, "SpuSetKey", "");
            AddFunction(0x9a6d0, "SpuIsTransferCompleted", "");
            AddFunction(0x83fe0, "TestEvent", "");
            AddFunction(0x96c6c, "SsUtKeyOn", "");
            AddFunction(0x92ff4, "_svm_orev1", "");
            AddFunction(0x938c0, "SpuVmDoAllocate", "");
            AddFunction(0x93ad8, "vmNoiseOn", "");
            AddFunction(0x94224, "note2pitch2", "");
            AddFunction(0x93264, "SpuVmKeyOnNow", "(vabid,prog,tone,note,fine,voll,volr)");


            //gpu sdk
            AddFunction(0x859ec, "LoadClut2", "");
            //AddFunction(0x8759c, "LoadImage", "");
            AddFunction(0x873e0, "checkRECT", "commandtext, rect  //clear, load, store");
            AddFunction(0x866f4, "GetClut", "");
            AddFunction(0x86220, "FntPrint", "");
            AddFunction(0x845dc, "strlen", "seektozero");
            AddFunction(0x85ba0, "FntLoad", "");
            AddFunction(0x85898, "LoadTPage", "");
            AddFunction(0x8662c, "GetTPage", "");
            AddFunction(0x8725c, "GetGraphType", "");
            AddFunction(0x86600, "memset", "");
            AddFunction(0x85b60, "SetDumpFnt", "");
            AddFunction(0x86b5c, "SetDrawTPage", "");
            AddFunction(0x87fd4, "SetDrawOffset", "");
            //AddFunction(0x87374, "DrawSync", "");//already had this one ;)
            AddFunction(0x8807c, "SetDrawArea", "");
            AddFunction(0x8816c, "SetDrawMode", "");
            AddFunction(0x85c44, "FntOpen", "");
            AddFunction(0x86a70, "SetTile", "");
            AddFunction(0x86908, "SetSemiTrans", "");
            AddFunction(0x869f8, "SetSprt8", "");
            AddFunction(0x86ed8, "ResetGraph", "");
            AddFunction(0x89bf8, "memset", "");
            AddFunction(0x89c24, "GPU_cw", "");
            AddFunction(0x896f8, "_reset", "");
            AddFunction(0x8d094, "SetIntrMask", "");
            AddFunction(0x89b1c, "_version", "");
            AddFunction(0x881c4, "SetDrawEnv", "");
            AddFunction(0x884c4, "get_cs", "");
            AddFunction(0x88590, "get_ce", "");
            AddFunction(0x8865c, "get_ofs", "");
            AddFunction(0x88468, "get_mode", "");
            AddFunction(0x886a4, "get_tw", "");
            AddFunction(0x8727c, "DrawSyncCallback", "");
            AddFunction(0x86a0c, "SetSprt16", "");
            AddFunction(0x869d0, "SetPolyG4", "");
            AddFunction(0x86b40, "SetDrawMove", "");
            AddFunction(0x872d8, "SetDispMask", "");
            AddFunction(0x87664, "MoveImage", "");

            //cd api
            AddFunction(0x89c34, "CdInit", "");
            AddFunction(0x89d8c, "CdReset", "");
            AddFunction(0x89ed8, "CdSyncCallback", "");
            AddFunction(0x89ef0, "CdReadyCallback", "");
            AddFunction(0x8cc2c, "CdReadCallback", "");
            AddFunction(0x8bc10, "CdSearchFile", "");
            AddFunction(0x8bf14, "CD_newmedia", "");
            AddFunction(0x8c53c, "cd_read", "");
            AddFunction(0x8a33c, "CdIntToPos", "");
            AddFunction(0x89f08, "CdControl", "");
            AddFunction(0x8af84, "CD_cw", "");
            AddFunction(0x8ca58, "CdRead", "");
            AddFunction(0x8cd24, "VSync", "advance frame and sound");
            AddFunction(0x8ce6c, "v_wait", "framecounter wait");
            AddFunction(0x8bbc0, "puts", "iowithdebugger");
            AddFunction(0x8cd14, "ChangeClearPAD", "");
            AddFunction(0x8cf08, "ChangeClearRCnt", "");
            AddFunction(0x89d4c, "CdStatus", "");
            AddFunction(0x8a16c, "CdControlB", "");
            AddFunction(0x8aa38, "CD_sync", "");
            AddFunction(0x8c880, "cd_read_retry", "");
            AddFunction(0x8a040, "CdControlF", "");
            AddFunction(0x89d7c, "CdLastPos", "");
            AddFunction(0x89df8, "CdFlush", "");
            AddFunction(0x8b454, "CD_flush", "");
            AddFunction(0x89d5c, "CdMode", "");
            AddFunction(0x8a440, "CdPosToInt", "");
            AddFunction(0x8cb60, "CdReadSync", "");
            AddFunction(0x89eb8, "CdReady", "");
            AddFunction(0x8acb8, "CD_ready", "");
            AddFunction(0x8c5dc, "strncmp", "");
            AddFunction(0x8c5a8, "memcpy", "");
            AddFunction(0x8c1f0, "CD_searchdir", "");
            AddFunction(0x84534, "strcmp", "");
            AddFunction(0x8c298, "CD_cachefile", "");
            AddFunction(0x8bef0, "_cmp", "");

            //pad sdk
            AddFunction(0x8cc94, "PadRead", "getcontrollerinput");
            AddFunction(0x8cd04, "PAD_dr", "getcontrollerhardwareaccess");
            AddFunction(0x8cc44, "PadInit", "");
            AddFunction(0x8ccf4, "PAD_init", "");




            /*FunctionNames.Add(0x36d30, "setridingentities");
            FunctionNames.Add(0x36e80, "setxyforces");
            FunctionNames.Add(0x36f64, "setadjustedxyforces");
            FunctionNames.Add(0x37040, "incrementforce");
            FunctionNames.Add(0x37078, "updateforces");
            FunctionNames.Add(0x374a8, "collidewithentitiesjumping");
            FunctionNames.Add(0x37640, "collidewithentities");
            FunctionNames.Add(0x377E0, "collidewithentities2");
            FunctionNames.Add(0x3797c, "collidewithmap");
            FunctionNames.Add(0x37c28, "updateridingentity");
            FunctionNames.Add(0x37eb8, "movez");
            FunctionNames.Add(0x38008, "movexy");
            FunctionNames.Add(0x38708, "moveentity");
            FunctionNames.Add(0x38800, "calcytile");
            FunctionNames.Add(0x38948, "processtile");
            FunctionNames.Add(0x38c44, "dophysics");
            */

            //NudgeAddresses();


            AddHandlerName("etick", 0x17, "jarsandboxes");




            //FunctionNames.Add(0x, "");

            EntityVarOffsets[0x0] = "indexentityid";
            EntityVarOffsets[0xc] = "ownerentity";
            EntityVarOffsets[0x10] = "status";//3 = invisible, 4 = destroyed
            EntityVarOffsets[0x14] = "health";
            EntityVarOffsets[0x18] = "maxhealth";
            EntityVarOffsets[0x1c] = "somecounter";

            EntityVarOffsets[0x28] = "refentity";//platform entity
            EntityVarOffsets[0x30] = "refxoff";//platformxoff
            EntityVarOffsets[0x34] = "refyoff";//platformyoff
            EntityVarOffsets[0x38] = "refzoff";//platformzoff
            EntityVarOffsets[0x3c] = "contentsitemid";
            EntityVarOffsets[0x40] = "contentsflagid";
            EntityVarOffsets[0x44] = "datasbinrecord";
            EntityVarOffsets[0x48] = "entityrefid";
            EntityVarOffsets[0x4c] = "codesa_load";
            EntityVarOffsets[0x50] = "codesb_map";
            EntityVarOffsets[0x54] = "codesc_tick";
            EntityVarOffsets[0x58] = "codesd_touch";
            EntityVarOffsets[0x5c] = "codese_deactivate";
            EntityVarOffsets[0x60] = "codesf_interact";
            EntityVarOffsets[0x64] = "sprite";
            EntityVarOffsets[0x68] = "spritetableindex";
            EntityVarOffsets[0x6c] = "gravityflags";
            EntityVarOffsets[0x70] = "sprcodes_load";
            EntityVarOffsets[0x74] = "sprcodes_map";
            EntityVarOffsets[0x78] = "sprcodes_tick";
            EntityVarOffsets[0x7c] = "sprcodes_touch";
            EntityVarOffsets[0x80] = "sprcodes_deactivate";
            EntityVarOffsets[0x84] = "sprcodes_interact";
            EntityVarOffsets[0x88] = "targetanim";
            EntityVarOffsets[0x8c] = "targetdir";
            EntityVarOffsets[0x90] = "curanim";

            EntityVarOffsets[0x94] = "dir";
            EntityVarOffsets[0x98] = "framedex";
            EntityVarOffsets[0x9c] = "animset";
            EntityVarOffsets[0xa0] = "curframe";
            EntityVarOffsets[0xa4] = "nextframe";
            EntityVarOffsets[0xa8] = "nextframedelay";
            EntityVarOffsets[0xac] = "wierdnextframedelayflag";
            EntityVarOffsets[0xb0] = "animcompletecounter";
            EntityVarOffsets[0xb4] = "animflags";
            EntityVarOffsets[0xb8] = "zforce";//risefall speed
            EntityVarOffsets[0xbc] = "targetxforce";
            EntityVarOffsets[0xc0] = "targetyforce";
            EntityVarOffsets[0xc4] = "xforce";
            EntityVarOffsets[0xc8] = "yforce";
            EntityVarOffsets[0xcc] = "interact_xforce";
            EntityVarOffsets[0xd0] = "interact_yforce";
            EntityVarOffsets[0xd4] = "xforcestep";
            EntityVarOffsets[0xd8] = "yforcestep";
            EntityVarOffsets[0xdc] = "adjustedxforce";
            EntityVarOffsets[0xe0] = "adjustedyforce";
            EntityVarOffsets[0xe4] = "finalxforce";
            EntityVarOffsets[0xe8] = "finalyforce";
            EntityVarOffsets[0xec] = "finalzforce";

            EntityVarOffsets[0xf0] = "acceleration";
            EntityVarOffsets[0xf4] = "speed";
            EntityVarOffsets[0xf8] = "appliedzforce";
            EntityVarOffsets[0xfc] = "screenclipx";
            EntityVarOffsets[0x100] = "screenclipy";
            EntityVarOffsets[0x104] = "screenclipz";
            EntityVarOffsets[0x108] = "negxmod";
            EntityVarOffsets[0x10c] = "negymod";

            EntityVarOffsets[0x114] = "xpos";
            EntityVarOffsets[0x118] = "ypos";
            EntityVarOffsets[0x11c] = "zpos";
            EntityVarOffsets[0x120] = "xtile";
            EntityVarOffsets[0x124] = "ytile";
            EntityVarOffsets[0x128] = "ztile";
            EntityVarOffsets[0x12c] = "ridingentity";

            EntityVarOffsets[0x130] = "?xcollision?";
            EntityVarOffsets[0x134] = "entityzcollision";
            EntityVarOffsets[0x138] = "mapzcollision";
            EntityVarOffsets[0x13c] = "forceadjusted";
            EntityVarOffsets[0x140] = "entitycollision";
            EntityVarOffsets[0x144] = "";
            EntityVarOffsets[0x148] = "maptiletl";
            EntityVarOffsets[0x14c] = "maptiletr";
            EntityVarOffsets[0x150] = "maptilebl";
            EntityVarOffsets[0x154] = "maptilebr";
            EntityVarOffsets[0x158] = "mapheighttl";
            EntityVarOffsets[0x15c] = "mapheighttr";
            EntityVarOffsets[0x160] = "mapheightbl";
            EntityVarOffsets[0x164] = "mapheightbr";
            EntityVarOffsets[0x168] = "donemoving";

            EntityVarOffsets[0x180] = "";
            EntityVarOffsets[0x184] = "";
            EntityVarOffsets[0x188] = "somethingforceindex";
            EntityVarOffsets[0x18c] = "";
            EntityVarOffsets[0x190] = "";
            EntityVarOffsets[0x194] = "imageref.images";
            EntityVarOffsets[0x198] = "imageref.x";
            EntityVarOffsets[0x19c] = "imageref.y";
            EntityVarOffsets[0x1a0] = "imageref.z";
            EntityVarOffsets[0x1a4] = "imageref.sort";
            EntityVarOffsets[0x1a8] = "imageref.numimages";

            EntityVarOffsets[0x1b0] = "addedtossheet?";
            EntityVarOffsets[0x1b4] = "addedtopallete?";
            EntityVarOffsets[0x1b8] = "activeeffect";
            EntityVarOffsets[0x1bc] = "depthsortval";
            EntityVarOffsets[0x1c0] = "sorttop";
            EntityVarOffsets[0x1c4] = "balancerecord";
            EntityVarOffsets[0x1c8] = "balancevalref";
            EntityVarOffsets[0x1cc] = "damagedtickcounter";
            EntityVarOffsets[0x1d0] = "framecoltickcounter";
            EntityVarOffsets[0x1d4] = "framecollision";
            EntityVarOffsets[0x1d8] = "adjustedxpos";
            EntityVarOffsets[0x1dc] = "adjustedypos";
            EntityVarOffsets[0x1e0] = "adjustedzpos";
            EntityVarOffsets[0x1e4] = "xmod";
            EntityVarOffsets[0x1e8] = "ymod";
            EntityVarOffsets[0x1ec] = "zmod";
            EntityVarOffsets[0x1f0] = "width";
            EntityVarOffsets[0x1f4] = "depth";
            EntityVarOffsets[0x1f8] = "height";
            EntityVarOffsets[0x1fc] = "framex";
            EntityVarOffsets[0x200] = "framey";
            EntityVarOffsets[0x204] = "framez";
            EntityVarOffsets[0x208] = "framexoff";
            EntityVarOffsets[0x20c] = "frameyoff";
            EntityVarOffsets[0x210] = "framezoff";
            EntityVarOffsets[0x214] = "framewidth";
            EntityVarOffsets[0x218] = "framedepth";
            EntityVarOffsets[0x21c] = "frameheight";
            EntityVarOffsets[0x220] = "hitcounter";
            EntityVarOffsets[0x224] = "touchingentity";
            EntityVarOffsets[0x228] = "triggerevt";
            EntityVarOffsets[0x22c] = "mapeventprogramid";
            EntityVarOffsets[0x230] = "entity(self)";
            EntityVarOffsets[0x234] = "tickprogsp";
            EntityVarOffsets[0x238] = "tickprogexp";

            EntityVarOffsets[0x23c] = "evttickprog";
            EntityVarOffsets[0x240] = "evtx";
            EntityVarOffsets[0x244] = "evty";

            EntityVarOffsets[0x260] = "evtlogicresult";

            EntityVarOffsets[0x26c] = "unknownevtanim";
            EntityVarOffsets[0x270] = "unknownevtdir";
            EntityVarOffsets[0x278] = "spawneditemid";
            EntityVarOffsets[0x280] = "spawneditemgameflag";
            EntityVarOffsets[0x284] = "spawnedzforce";

            MapNames[115] = "tarns manor";

            MapNames[162] = "inoa";
            MapNames[163] = "jess house";
            MapNames[164] = "beaumont/septimus/fortuneteller house";
            MapNames[165] = "wendell/giles house";
            MapNames[166] = "lutas/bonaire house";
            MapNames[167] = "nadia/gustav house";
            MapNames[168] = "klein/sybill house";
            MapNames[169] = "inoa 2";
            MapNames[176] = "inoa 3";
            MapNames[183] = "inoa 4";
            MapNames[186] = "inoa 5";
            MapNames[193] = "inoa 6";
            MapNames[198] = "inoa 7";
            MapNames[205] = "inoa 8";
            MapNames[212] = "inoa 9";
            MapNames[219] = "inoa 10";
            MapNames[226] = "inoa 11";
            MapNames[230] = "inoa 12";
            MapNames[237] = "inoa 13";
            MapNames[241] = "inoa 14";
            MapNames[248] = "inoa 15";
            MapNames[252] = "inoa 16";
            MapNames[259] = "inoa 17";
            MapNames[266] = "inoa 18";
            MapNames[271] = "inoa 19";
            MapNames[275] = "inoa 20";
            MapNames[282] = "inoa 21";
            MapNames[289] = "inoa 22";
            MapNames[293] = "inoa burning";
            MapNames[297] = "inoa burned";
            MapNames[301] = "inoa burned 2";

            MapNames[303] = "miner/graveyard keeper";

            MapNames[310] = "inoa w portal and meia";
            MapNames[311] = "a house and a well";
            MapNames[312] = "ship interior no entities";
            MapNames[313] = "ship interior no entities";
            MapNames[314] = "unfinished coast? no entities";
            MapNames[315] = "unfinished coast no entities one gaincontrol event";
            MapNames[320] = "unfinished ship map";

            MapNames[321] = "zorgia boss map";
            MapNames[322] = "boss map";
            MapNames[323] = "boss map dragon";
            MapNames[324] = "boss map bridge";
            MapNames[325] = "boss map fire dragon";
            MapNames[326] = "boss map priest";
            MapNames[327] = "boss map";

            MapNames[330] = "inoa";

            MapNames[337] = "lake shrine int";
            MapNames[338] = "lake shrine int";
            MapNames[339] = "lake shrine int";
            MapNames[353] = "lake shrine int";
            MapNames[354] = "lake shrine roof";
            MapNames[355] = "lake shrine roof";
            MapNames[356] = "lake shrine roof";


            MapNames[357] = "inoa";
            MapNames[364] = "inoa";
            MapNames[367] = "inoa";

            MapNames[368] = "great tree";
            MapNames[369] = "great tree int";

            MapNames[371] = "inoa";

            MapNames[389] = "ship";
            MapNames[390] = "ship captains";
            MapNames[391] = "ship night";
            MapNames[392] = "ship int";

            
            AddGlobalVariable(0x13d224, "getentitieslist","");
            AddGlobalVariable(0x13d228, "getentitiesliststart", "");
            AddGlobalVariable(0x1d918c, "numentities", "");
            AddGlobalVariable(0x9b5b4, "eventhandlers","");

            AddGlobalVariable(0x1ac488, "largestmapsize", "");
            AddGlobalVariable(0x1ac484, "largestmapid", "");

            AddGlobalVariable(0x139df8, "toprocesslist", "");
            AddGlobalVariable(0x13d5c8, "toprocesscount", "");

            AddGlobalVariable(0x139f00, "tocollidelist", "");
            AddGlobalVariable(0x140e10, "tocollidecount", "");

            AddGlobalVariable(0x1d77d8, "torenderlist", "");
            AddGlobalVariable(0x1d7b60, "torendercount", "");

            AddGlobalVariable(0x1ac498, "playerentity", "");
            AddGlobalVariable(0x1ac72c, "entitiesafterplayer", "");

            AddGlobalVariable(0x1e6118, "mapgameflags", "");
            AddGlobalVariable(0x1d84e0, "somegravitysetting", "");
            AddGlobalVariable(0x1dd0d4, "globalgameflags", "");
            AddGlobalVariable(0x1dd7e8, "playerinput", "");

            AddGlobalVariable(0x1dd818, "initentity", "");
            AddGlobalVariable(0x1ddaa8, "initentityend", "");

            AddGlobalVariable(0x1efe00, "playercontrolsetting", "");
            AddGlobalVariable(0x1efe10, "gamemap", "gamemapinfo");

            AddGlobalVariable(0x1d78e0, "camtargetx", "");
            AddGlobalVariable(0x1d7930, "camtargety", "");
            AddGlobalVariable(0x1d7934, "camtargetz", "");
            AddGlobalVariable(0x1ef1d4, "camxpos", "");
            AddGlobalVariable(0x1ef1d8, "camypos", "");

            AddGlobalVariable(0x1530dc, "mapsizetodraw", "");
            AddGlobalVariable(0x9ad04, "framecounter", "");
            AddGlobalVariable(0x1ac464, "maptilesptr", "");
            AddGlobalVariable(0x141db8, "tilexylookuptable ", "//helps with the math to get an x and a y and a sheet from a tile index. tilesheet, x, y. x = x%10 * 24 y = y/10*16");
            AddGlobalVariable(0x1dd044, "tilepageinfo", "");
            AddGlobalVariable(0x13a000, "numsprites", "");
            AddGlobalVariable(0x1c6598, "spriteframecounter", "");

            AddGlobalVariable(0x1e5ce0, "eventprogsset", "a prog was set by a script, main event handler will respond");

            AddGlobalVariable(0x1f2bb0, "spritesheets", "");
            AddGlobalVariable(0x12e040, "palettes", "");
            AddGlobalVariable(0x1ef124, "dialogcmdlist", "");
            AddGlobalVariable(0x1ef120, "dialogstatus", "");
            AddGlobalVariable(0x1ef132, "dialogpalette", "");
            AddGlobalVariable(0x1ef13a, "dialogsheet", "");
            AddGlobalVariable(0x1efbf0, "dialogname", "");
            AddGlobalVariable(0x10689c, "dialogstate", "");//0 no dialog, 5 swooshing open/closed, 4 running text,3 waiting, 14 next one, 6 user advanced
            AddGlobalVariable(0x107310, "dialogrecords", "28 byte records, offset 0 has some status flags, offset 0x10 has a handler function pointer");
            AddGlobalVariable(0x1068a0, "dialogtextbuffer", "");
            AddGlobalVariable(0x10722c, "dialogtextcmdbuffer", "");
            AddGlobalVariable(0x1f3df0, "mapstrings", "");
            AddGlobalVariable(0x1f2ef0, "globalstrings", "");
            AddGlobalVariable(0x1f2c68, "SIEntityRecords","initdata");
            AddGlobalVariable(0x1f2c74, "numSIEntityRecords", "initdatas");
            AddGlobalVariable(0x1f2c78, "mapeffect12byterecords", "has effect data");
            AddGlobalVariable(0x1f2c84, "nummapeffect12byterecords", "has effect");
            AddGlobalVariable(0x1f2c88, "mapeventrecords", "has map triggered eventprog(b) data");

            AddGlobalVariable(0x1efbfc, "activecollisionentity", "");
            AddGlobalVariable(0x1efbf4, "camfollowentity", "camera follows");

            AddGlobalVariable(0x133730, "effectslist", "0x80 byte records and 0x80 in the list");
            AddGlobalVariable(0x1380f0, "spritereflist", "list of pointers to spriteref");
            AddGlobalVariable(0x1ef1d0, "spritereflisttailptr", "this is used to add to the list");

            AddGlobalVariable(0x13c028, "mapeventlist", "these things are 0x48 bytes long, and max of 0x40 of them");

            AddGlobalVariable(0x9ad10, "randomseed", "");

            AddGlobalVariable(0x1f2c60, "mapspritetable", "");
            AddGlobalVariable(0x1f36f8, "globalspritetable", "");

            AddGlobalVariable(0x1f922c, "activeevtcode", "");
            AddGlobalVariable(0x1428f8, "prevevtcode", "");
            AddGlobalVariable(0x1f91d0, "activeprogindex", "");
            AddGlobalVariable(0x13c020, "activeentityrefid", "");
            AddGlobalVariable(0x140e14, "activeevtprogtype", "");

            AddGlobalVariable(0xc7b88, "8byterecordsfor3c", "");

            AddGlobalVariable(0x1045d8, "mapbalancelevel", "a balancerecord is chosen with a value above this");
            
            AddGlobalVariable(0x1f51d0, "unknowndebugrecords", "");
            
            AddGlobalVariable(0x1ef1e0, "dbinheader.spriteinfo", "");
            AddGlobalVariable(0x1ef1e4, "dbinheader.sprites", "");
            AddGlobalVariable(0x1ef1e8, "dbinheader.spritesrepeat", "");
            AddGlobalVariable(0x1ef1ec, "dbinheader.stringtable", "");
            AddGlobalVariable(0x1ef1f0, "dbinheader.stringsrepeat", "");
            AddGlobalVariable(0x1ef1f4, "dbinheader.umapa", "");
            AddGlobalVariable(0x1ef1f8, "dbinheader.umapb", "");
            AddGlobalVariable(0x1ef1fc, "dbinheader.umapb2", "");
            AddGlobalVariable(0x1ef200, "dbinheader.umapb3", "");
            AddGlobalVariable(0x1ef204, "dbinheader.umapb4", "");
            AddGlobalVariable(0x1ef208, "dbinheader.maps", "");

            AddGlobalVariable(0x1f0f90, "warpinfo.90", "");
            AddGlobalVariable(0x1f0f94, "warpinfo.breakoutgameloop", "");
            AddGlobalVariable(0x1f0f98, "warpinfo.98", "");
            AddGlobalVariable(0x1f0f9c, "warpinfo.mapid", "");
            AddGlobalVariable(0x1f0fa0, "warpinfo.a0", "");
            AddGlobalVariable(0x1f0fa4, "warpinfo.a4", "");
            AddGlobalVariable(0x1f0fa8, "warpinfo.a8", "");
            AddGlobalVariable(0x1f0fac, "warpinfo.ac", "");
            AddGlobalVariable(0x1f0fb0, "warpinfo.b0", "");
            AddGlobalVariable(0x1f0fb4, "warpinfo.b4", "");
            AddGlobalVariable(0x1f0fb8, "warpinfo.b8", "");

            AddGlobalVariable(0x12e188, "balancerecords", "data/balance.bin");

            AddComment(0x2c3a8, "mapinfo.infoblock");
            AddComment(0x2c3ac, "mapinfo.mapblock");
            AddComment(0x2c3b0, "mapinfo.tilesheets");
            AddComment(0x2c3c4, "mapinfo.spriteinfo");
            AddComment(0x2c3d0, "mapinfo.stringtable");
            AddComment(0x2c3dc, "mapinfo.scrollscreen");

            AddGlobalVariable(0x1f50f8, "curmapid", "");

            AddGlobalVariable(0x153460, "mapinfo", "");

            AddGlobalVariable(0x45e24, "loadfontinner", "");


            //audio variables
            AddGlobalVariable(0x26848, "numsfx", "");
            AddGlobalVariable(0x139070, "somesfxlist", "");
            AddGlobalVariable(0xa9934, "global_vab_headoffset", "");
            AddGlobalVariable(0xa9938, "global_vab_bodyoffset", "");
            AddGlobalVariable(0xa993c, "vab_offset_list_beginning", "8 byte records:beginnig of head and beginning of body");
            AddGlobalVariable(0xa9b98, "music_offset_list.seq", "12 byte record:seq offset, vabhead offset, vabbody offset");
            AddGlobalVariable(0xa9b9c, "music_offset_list.head", "12 byte record: this is past the first value which is the seq offset");
            AddGlobalVariable(0xa9ba0, "music_offset_list.body", "12 byte record: this is past the first values");
            AddGlobalVariable(0x1d7938, "music_seq_vab_index", "");
            AddGlobalVariable(0x1d7b64, "map_vab_index", "");
            
            AddGlobalVariable(0x1f28e0, "seqdatatable", "seqsep 4max open at a time, 1 in sep currently");

            AddGlobalVariable(0x1142a0, "global_vab_headbuff", "used for loading vab from disk to audio memory");
            AddGlobalVariable(0x142920, "vab_bodybuff", "used for loading vab from disk to audio memory");

            AddGlobalVariable(0x116aa0, "map_vab_headbuff", "used for loading vab from disk to audio memory");


            AddGlobalVariable(0xa9e4c, "musicvabid", "");
            AddGlobalVariable(0x139df0, "musicseqid", "");
            AddGlobalVariable(0xa9e4e, "globalsfxvabid", "when there is a seqid?");
            AddGlobalVariable(0xa9e50, "mapsfxvabid", "when seqid is -1?");

            AddGlobalVariable(0xa9ef0, "sfxrecords.vabid", "can be -1");
            AddGlobalVariable(0xa9ef2, "sfxrecords.prognum", "");
            AddGlobalVariable(0xa9ef4, "sfxrecords.tonenum", "");
            AddGlobalVariable(0xa9ef6, "sfxrecords.note", "");
            AddGlobalVariable(0xa9ef8, "sfxrecords.unknownaddr", "4 byte aligned?");
            AddGlobalVariable(0xa9efa, "sfxrecords.seqid", "index to seq buffer offset table");
            AddGlobalVariable(0xa9efc, "sfxrecords.refrecordid", "index to a different sfxrecord");
            AddGlobalVariable(0xa9f00, "sfxrecords.00", "");
            AddGlobalVariable(0xa9f04, "sfxrecords.numtones", "how many times to add to tonenum");

            AddGlobalVariable(0xa9dd0, "seseqbuff_offsets", "seqid is index into this array of words");

            AddGlobalVariable(0x1e7c08, "seseqbuff", "loads beginning of file from audio.bin");
            AddGlobalVariable(0x1f5128, "seqnumtoseqidtable", "shorts");

            AddGlobalVariable(0x1efc08, "voicesactive", "array of bytes to signify if the voices are being used");
            AddGlobalVariable(0x1efc20, "voiceinfo", "has a lot of info about the state of the 24 spu voices");

            AddFunction(0x49f9c, "voicesplayingsfx", "");

            AddFunction(0x49388, "loadaudiobin", "loads audio from disk 1");
            AddFunction(0x495d8, "loadaudiobininner", "loads audio from disk 2");
            AddFunction(0x4b19c, "loadmapaudio", "(mapid)");
            AddFunction(0x498d0, "loadmapvab", "(mapid)");
            AddFunction(0x49744, "loadmapvabinner", "loads audio from disk 3");
            AddFunction(0x4affc, "mapidtovabindex", "(mapid)");
            AddGlobalVariable(0xc8930, "mapidtovablist", "");

            AddGlobalVariable(0x1ef1b8, "cdifile_datasbin", "");
            AddGlobalVariable(0x1efe38, "cdifile_soundbin", "");

            AddGlobalVariable(0x1F801040, "portjoydata", "controller port");
            AddGlobalVariable(0x1F801044, "portjoystat", "controller port");
            AddGlobalVariable(0x1F801048, "portjoymode", "controller port");
            AddGlobalVariable(0x1F80104A, "portjoyctrl", "controller port");
            AddGlobalVariable(0x1F80104E, "portjoybaud", "controller port");
            AddGlobalVariable(0x1F801050, "portsiodata", "serial port");
            AddGlobalVariable(0x1F801054, "portsiostat", "serial port");
            AddGlobalVariable(0x1F801058, "portsiomode", "serial port");
            AddGlobalVariable(0x1F80105A, "portsictrlo", "serial port");
            AddGlobalVariable(0x1F80105C, "portsiomisc", "serial port");
            AddGlobalVariable(0x1F80105E, "portsiobaud", "serial port");

            AddGlobalVariable(0x1F801070, "portintstat", "interrupt status");
            AddGlobalVariable(0x1F801074, "portintmask", "interrupt mask");

            AddGlobalVariable(0x1F801800, "portcd1", "");
            AddGlobalVariable(0x1F801801, "portcd2", "");
            AddGlobalVariable(0x1F801802, "portcd3", "");
            AddGlobalVariable(0x1F801803, "portcd4", "");

            AddGlobalVariableRange(0x1F801C00, 0x1F801E80, "portspu", "audio");
            AddGlobalVariableRange(0x1f8020, 0x1f802f, "portexpdebug", "debugger io");
            AddGlobalVariableRange(0x1F801080, 0x1F8010FC, "portdma", "dma");

            AddGlobalVariableRange(0x1F800000, 0x1F800400, "scratchpad", "data cache");

            AddGlobalVariableRange(0x1F801000, 0x1F801020, "memorycontrol", "");

            AddGlobalVariable(0x1F801810, "portgpu1", "graphics card");
            AddGlobalVariable(0x1F801814, "portgpu2", "graphics card");

            AddGlobalVariable(0x1F801820, "portmdec", "");
            AddGlobalVariable(0x1F801824, "portmdec", "");

            AddFunction(0x8db44, "entrypoint", "");
            bool isstartmenu = false;
            if (isstartmenu)
            {
                AddGlobalVariable(0x1f4b28, "soundbinhandle","");
                AddFunction(0x290e4, "readfromdisk", "(filename, buffer, position, length)");

                AddFunction(0x36028, "entrypoint", "");
                AddFunction(0x255a0, "main", "");
                AddFunction(0x2535c, "tomenu", "");
                AddFunction(0x21f2c, "menuloop", "");

                //startmenu audio sdk funcs
                AddFunction(0x38838, "SsInit", "");
                AddFunction(0x35440, "ResetCallback", "");
                AddFunction(0x3fdc4, "SpuInit", "");
                AddFunction(0x3fde4, "_SpuInit", "");
                AddFunction(0x38750, "_SsInit", "");
                AddFunction(0x3a1a8, "SsVabClose", "");
                AddFunction(0x41760, "SpuFree", "");
                AddFunction(0x41460, "_spu_gcSPU", "");
                AddFunction(0x3a7c4, "SsVabTransBodyPartly", "");
                AddFunction(0x42730, "SpuSetTransferMode", "");
                AddFunction(0x426f4, "SpuSetTransferStartAddr", "");
                AddFunction(0x40f6c, "_spu_FsetRXXa", "");
                AddFunction(0x42898, "_spu_setInTransfer", "");
                AddFunction(0x42764, "SpuWritePartly", "");
                AddFunction(0x40e34, "_spu_write", "");
                AddFunction(0x3a2b0, "SsVabOpenHeadSticky", "");
                AddFunction(0x3a310, "SsVabOpenHeadWithMode", "");
                AddFunction(0x428c4, "_spu_getInTransfer", "");
                AddFunction(0x41198, "SpuMalloc", "");
                AddFunction(0x3feac, "SpuStart", "");
                AddFunction(0x29c88, "EnterCriticalSection", "");
                AddFunction(0x4104c, "_SpuDataCallback", "");
                AddFunction(0x29c38, "OpenEvent", "");
                AddFunction(0x29c68, "EnableEvent", "");
                AddFunction(0x29c98, "ExitCriticalSection", "");
                AddFunction(0x41070, "SpuQuit", "");
                AddFunction(0x29c48, "CloseEvent", "");
                AddFunction(0x29c78, "DisableEvent", "");
                AddFunction(0x410ec, "SpuSetMute", "");
                AddFunction(0x41144, "SpuInitMalloc", "");
                AddFunction(0x418b4, "SpuSetReverb", "");
                AddFunction(0x419f4, "_SpuIsInAllocateArea_", "");
                AddFunction(0x41a6c, "SpuSetReverbModeParam", "");
                AddFunction(0x41f7c, "_spu_setReverbAttr", "");
                AddFunction(0x424e8, "SpuClearReverbWorkArea", "");
                AddFunction(0x40b8c, "_spu_t", "");
                AddFunction(0x42684, "WaitEvent", "");
                AddFunction(0x40f24, "_spu_FsetRXX", "");
                AddFunction(0x4244c, "SpuSetReverbDepth", "");
                AddFunction(0x427f0, "SpuIsTransferCompleted", "");
                AddFunction(0x29c58, "TestEvent", "");
                AddFunction(0x428d4, "SpuSetCommonAttr", "");
                AddFunction(0x42d38, "SpuGetAllKeysStatus", "");

                //cd sdk funcs
                AddFunction(0x30e74, "CdInit", "");
                AddFunction(0x30fcc, "CdReset", "");
                AddFunction(0x31118, "CdSyncCallback", "");
                AddFunction(0x31130, "CdReadyCallback", "");
                AddFunction(0x33dec, "CdReadCallback", "");
                AddFunction(0x2b154, "printf", "");
                AddFunction(0x31148, "CdControl", "");
                AddFunction(0x321c4, "CD_cw", "");
                AddFunction(0x31680, "CdPosToInt", "");
                AddFunction(0x313ac, "CdControlB", "");
                AddFunction(0x31c78, "CD_sync", "");
                AddFunction(0x31280, "CdControlF", "");
                AddFunction(0x32e50, "CdSearchFile", "");
                AddFunction(0x33154, "CD_newmedia", "");
                AddFunction(0x3377c, "cd_read", "");
                AddFunction(0x3157c, "CdIntToPos", "");
                AddFunction(0x33c18, "CdRead", "");
                AddFunction(0x3524c, "VSync", "");
                AddFunction(0x35394, "v_wait", "");
                AddFunction(0x32e00, "puts", "");
                AddFunction(0x3523c, "ChangeClearPAD", "");
                AddFunction(0x35430, "ChangeClearRCnt", "");
                AddFunction(0x30f8c, "CdStatus", "");
                AddFunction(0x33a40, "cd_read_retry", "");
                AddFunction(0x30fbc, "CdLastPos", "");
                AddFunction(0x31038, "CdFlush", "");
                AddFunction(0x32694, "CD_flush", "");
                AddFunction(0x30f9c, "CdMode", "");
                AddFunction(0x33d20, "CdReadSync", "");
                AddFunction(0x310f8, "CdReady", "");
                AddFunction(0x31ef8, "CD_ready", "");
                AddFunction(0x29ef4, "strncmp", "");
                AddFunction(0x337e8, "memcpy", "");
                AddFunction(0x33430, "CD_searchdir", "");
                AddFunction(0x29e90, "strcmp", "");
                AddFunction(0x334d8, "CD_cachefile", "");
                AddFunction(0x33130, "_cmp", "");
                AddFunction(0x33e04, "CdRead2", "");
                AddFunction(0x31538, "CdDataCallback", "");
            }

            //AddPlayerVariableRange(0x1ac498, "playercharacter")

        }

        /*static void NudgeAddresses()
        {
            var backup = FunctionNames;
            FunctionNames = new Dictionary<uint, string>();
            foreach (var ent in backup)
            {
                FunctionNames.Add(ent.Key - Adjustment, ent.Value);
            }
        }*/
    }
}
