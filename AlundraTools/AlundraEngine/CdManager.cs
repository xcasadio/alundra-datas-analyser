namespace AlundraEngine;

public class CdManager
{
    //8005a724
    public void InitCDReading()
    {
        int result;
        byte[] setLocParams = new byte[8];

        if ((StaticVariables.g_isCdResetRequested != 0 
             || (StaticVariables.g_cdIsReady != 0 && StaticVariables.g_cdDataLoaded == 0)) 
            && StaticVariables.g_cdInitRequired != 0)
        {
            do
            {
                /* pause */
                result = CdControlB('\v', null, 0x0);
            } while (result == 0);

            do
            {
                /* stop */
                result = CdControlB('\t', null, 0x0);
            } while (result == 0);

            setLocParams[0] = 0x80;

            do
            {
                result = CdControlB('\x0e', setLocParams, 0x0);
            } while (result == 0);
            StaticVariables.g_cdInitRequired = 0;
        }
    }

    private int CdControlB(char com, byte[]? param, byte result)

    {
        int iVar1;
        int iVar2;
        int iVar3;
        uint uVar4;
        int iVar5;
        /*
        iVar3 = StaticVariables.INT_ARRAY_800c8238[0x27];
        iVar5 = 3;
        do
        {
            StaticVariables.INT_ARRAY_800c8238[0x27] = 0;

            if ((com != 1) && (((byte)StaticVariables.INT_ARRAY_800c8238[0x2b] & 0x10) != 0))
            {
                //CD_cw(1, (undefined1*)0x0, (undefined1*)0x0, 0);
            }
            if ((param == 0x0 || (StaticVariables.INT_ARRAY_800c8238[com + 7] == 0) ||
                (iVar1 = CD_cw(2, param, result, 0), iVar1 == 0))
            {
                StaticVariables.INT_ARRAY_800c8238[0x27] = iVar3;
                iVar1 = CD_cw(com, param, result, 0);
                iVar2 = 0;
                if (iVar1 == 0) break;
            }
            iVar5 = iVar5 + -1;
            iVar2 = -1;
            StaticVariables.INT_ARRAY_800c8238[0x27] = iVar3;
        } while (iVar5 != -1);
        if (iVar2 != 0)
        {
            return 0;
        }

        iVar3 = CD_sync(0, result);
        SYS_OBJ_538();
        */
        return 1; //iVar3 == 2 ? 1 : 0;
    }

}