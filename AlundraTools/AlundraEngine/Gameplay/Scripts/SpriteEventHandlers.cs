using AlundraEngine.Gameplay.Scripts.Boss;
using System.Diagnostics;

namespace AlundraEngine.Gameplay.Scripts;

public class SpriteEventHandlers
{
    private readonly GameEngine _gameEngine;
    public delegate void SpriteEventHandler(GameEngine gameEngine, Entity entity);
    private readonly Dictionary<int, SpriteEventHandler?>[] _typeHandlers = new Dictionary<int, SpriteEventHandler?>[6];

    public SpriteEventHandlers(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;

        //g_entityEventFunctionsByType  // 80098f4c
        _typeHandlers[ScriptHelper.ProgramALoad] = new Dictionary<int, SpriteEventHandler?>(); // 800c4b34
        _typeHandlers[ScriptHelper.ProgramBMap] = null; // there are no spriteEvent handlers for map
        _typeHandlers[ScriptHelper.ProgramCTick] = new Dictionary<int, SpriteEventHandler?>(); // 800c4f34  
        _typeHandlers[ScriptHelper.ProgramDTouch] = new Dictionary<int, SpriteEventHandler?>(); // 800c5334  
        _typeHandlers[ScriptHelper.ProgramEDeactivate] = new Dictionary<int, SpriteEventHandler?>(); // 800c5734  
        _typeHandlers[ScriptHelper.ProgramFInteract] = new Dictionary<int, SpriteEventHandler?>(); // 800c5b34  

        Register(ScriptHelper.ProgramALoad, 0, FunctionTypeA.SetSpawnFlagFromZPos);
        Register(ScriptHelper.ProgramALoad, 1, FunctionTypeA.SetAnimationTo2);
        Register(ScriptHelper.ProgramALoad, 2, FunctionTypeA.SetRandomizedAnimIdAndFlag);
        Register(ScriptHelper.ProgramALoad, 3, FunctionTypeA.SetAnimationTo4);
        Register(ScriptHelper.ProgramALoad, 4, FunctionTypeA.SetAnimationTo9);
        Register(ScriptHelper.ProgramALoad, 5, FunctionTypeA.SetAnimationTo7);
        Register(ScriptHelper.ProgramALoad, 6, FunctionTypeA.SetAnimationTo3);
        Register(ScriptHelper.ProgramALoad, 7, FunctionTypeA.SetAnimationTo9_2);
        Register(ScriptHelper.ProgramALoad, 8, FunctionTypeA.SetAnimationTo8AndCustomByte3);
        Register(ScriptHelper.ProgramALoad, 9, FunctionTypeA.SetAnimationTo10);
        Register(ScriptHelper.ProgramALoad, 10, FunctionTypeA.SetAnimationTo6);
        Register(ScriptHelper.ProgramALoad, 11, FunctionTypeA.SpawnWarpAndSetAnim);
        Register(ScriptHelper.ProgramALoad, 12, FunctionTypeA.TriggerMultipleWarpsAndClearHistory);
        Register(ScriptHelper.ProgramALoad, 13, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramALoad, 14, FunctionTypeA.SetAnim8AndResetLoader);
        Register(ScriptHelper.ProgramALoad, 15, FunctionTypeA.SpawnSpecificWarpAndResetLoader);
        Register(ScriptHelper.ProgramALoad, 16, FunctionTypeA.SetAnim0AndResetLoader);
        Register(ScriptHelper.ProgramALoad, 17, FunctionTypeA.SetAnimEAndResetLoader);
        Register(ScriptHelper.ProgramALoad, 18, FunctionTypeA.SetTargetAnimationTo14);
        Register(ScriptHelper.ProgramALoad, 19, FunctionTypeA.SetCustomByteFromProgramIndex);
        Register(ScriptHelper.ProgramALoad, 20, FunctionTypeA.SetCustomByteFromZPos);
        Register(ScriptHelper.ProgramALoad, 21, FunctionTypeA.SpawnWarpDropAndAdjustPosition);
        Register(ScriptHelper.ProgramALoad, 22, AI_Melzas2.SpawnVerticalWarpColumns);
        Register(ScriptHelper.ProgramALoad, 254, FunctionTypeA.FUN_80061bcc);
        Register(ScriptHelper.ProgramALoad, 255, FunctionTypeA.FUN_80061bd4);

        Register(ScriptHelper.ProgramCTick, 0, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramCTick, 1, FunctionTypeC.AI_UpdateEntityAI_IdleSkittish);
        Register(ScriptHelper.ProgramCTick, 2, FunctionTypeC.AI_UpdateEntityAI_CuriousFlying);
        Register(ScriptHelper.ProgramCTick, 3, FunctionTypeC.AI_UpdateEntityAI_1);
        Register(ScriptHelper.ProgramCTick, 4, FunctionTypeC.AI_FUN_80066984);
        Register(ScriptHelper.ProgramCTick, 5, FunctionTypeC.AI_UpdateEntityAI_0);
        Register(ScriptHelper.ProgramCTick, 6, FunctionTypeC.AI_UpdateEntityAI_3);
        Register(ScriptHelper.ProgramCTick, 7, FunctionTypeC.AI_UpdateEntityAI_4);
        Register(ScriptHelper.ProgramCTick, 8, FunctionTypeC.AI_UpdateEntityAI_5);
        Register(ScriptHelper.ProgramCTick, 9, FunctionTypeC.AI_UpdateEntityAI_6);
        Register(ScriptHelper.ProgramCTick, 10, FunctionTypeC.AI_UpdateEntityAI_6_2);
        Register(ScriptHelper.ProgramCTick, 11, FunctionTypeC.AI_UpdateEntityAI_7);
        Register(ScriptHelper.ProgramCTick, 12, FunctionTypeC.AI_UpdateEntityAI_8);
        Register(ScriptHelper.ProgramCTick, 13, FunctionTypeC.AI_UpdateEntityAI_8_2);
        Register(ScriptHelper.ProgramCTick, 14, FunctionTypeC.AI_UpdateEntityAI_9);
        Register(ScriptHelper.ProgramCTick, 15, FunctionTypeC.AI_UpdateEntityAI_10);
        Register(ScriptHelper.ProgramCTick, 16, FunctionTypeC.AI_FUN_80069f44);
        Register(ScriptHelper.ProgramCTick, 17, FunctionTypeC.AI_UpdateEntityAI_11);
        Register(ScriptHelper.ProgramCTick, 18, FunctionTypeC.AI_FUN_8006a564);
        Register(ScriptHelper.ProgramCTick, 19, FunctionTypeC.AI_UpdateEntityAI_12);
        Register(ScriptHelper.ProgramCTick, 20, FunctionTypeC.AI_UpdateEntityAI_13);
        Register(ScriptHelper.ProgramCTick, 21, FunctionTypeC.AI_UpdateEntityAI_14);
        Register(ScriptHelper.ProgramCTick, 22, FunctionTypeC.AI_FUN_8006b510);
        Register(ScriptHelper.ProgramCTick, 23, FunctionTypeC.AI_FUN_8006b848);
        Register(ScriptHelper.ProgramCTick, 24, FunctionTypeC.AI_FUN_8006b8cc);
        Register(ScriptHelper.ProgramCTick, 25, FunctionTypeC.AI_UpdateEntityAI_15);
        Register(ScriptHelper.ProgramCTick, 26, FunctionTypeC.AI_UpdateEntityAI_17);
        Register(ScriptHelper.ProgramCTick, 27, FunctionTypeC.AI_UpdateEntityAI_18);
        Register(ScriptHelper.ProgramCTick, 28, FunctionTypeC.AI_UpdateEntityAI_19);
        Register(ScriptHelper.ProgramCTick, 29, FunctionTypeC.AI_UpdateEntityAI_20);
        Register(ScriptHelper.ProgramCTick, 30, FunctionTypeC.AI_FUN_8006ce08);
        Register(ScriptHelper.ProgramCTick, 31, FunctionTypeC.AI_UpdateEntityAI_20_2);
        Register(ScriptHelper.ProgramCTick, 32, FunctionTypeC.AI_UpdateEntityAI_21);
        Register(ScriptHelper.ProgramCTick, 33, FunctionTypeC.AI_UpdateEntityAI_22);
        Register(ScriptHelper.ProgramCTick, 34, FunctionTypeC.AI_UpdateEntityAI_21);
        Register(ScriptHelper.ProgramCTick, 35, FunctionTypeC.AI_UpdateEntityAI_23);
        Register(ScriptHelper.ProgramCTick, 36, FunctionTypeC.AI_FUN_8006e83c);
        Register(ScriptHelper.ProgramCTick, 37, FunctionTypeC.AI_UpdateEntityAI_23_2);
        Register(ScriptHelper.ProgramCTick, 38, FunctionTypeC.AI_FUN_8006eb9c);
        Register(ScriptHelper.ProgramCTick, 39, FunctionTypeC.AI_UpdateEntityAI_Boss);
        Register(ScriptHelper.ProgramCTick, 40, FunctionTypeC.AI_UpdateBossEntityState);
        Register(ScriptHelper.ProgramCTick, 41, FunctionTypeC.AI_UpdateBossEntityState);
        Register(ScriptHelper.ProgramCTick, 42, FunctionTypeC.AI_FUN_8006f8e4);
        Register(ScriptHelper.ProgramCTick, 43, FunctionTypeC.AI_UpdateEntityAI_Boos);
        Register(ScriptHelper.ProgramCTick, 44, FunctionTypeC.AI_UpdateEntityAI_Spec);
        Register(ScriptHelper.ProgramCTick, 45, FunctionTypeC.AI_UpdateEntityIA_Watc);
        Register(ScriptHelper.ProgramCTick, 46, FunctionTypeC.AI_UpdateEntityAI_Twin);
        Register(ScriptHelper.ProgramCTick, 47, FunctionTypeC.AI_UpdateEntityAI_0);
        Register(ScriptHelper.ProgramCTick, 48, FunctionTypeC.AI_UpdateEntityAI_Warp);
        Register(ScriptHelper.ProgramCTick, 49, FunctionTypeC.AI_FUN_80075a3c);
        Register(ScriptHelper.ProgramCTick, 50, FunctionTypeC.AI_UpdateLoaderBossAI);
        Register(ScriptHelper.ProgramCTick, 51, FunctionTypeC.AI_FUN_80077734);
        Register(ScriptHelper.ProgramCTick, 52, FunctionTypeC.AI_FUN_80078a5c);
        Register(ScriptHelper.ProgramCTick, 53, FunctionTypeC.AI_FUN_80078b54);
        Register(ScriptHelper.ProgramCTick, 54, FunctionTypeC.AI_UpdateEntityIA_Fire);
        Register(ScriptHelper.ProgramCTick, 55, FunctionTypeC.AI_FUN_80079b14);
        Register(ScriptHelper.ProgramCTick, 56, FunctionTypeC.AI_FUN_8007a2f8);
        Register(ScriptHelper.ProgramCTick, 57, FunctionTypeC.AI_FUN_8007a4a8);
        Register(ScriptHelper.ProgramCTick, 58, FunctionTypeC.AI_FUN_8007a4b0);
        Register(ScriptHelper.ProgramCTick, 59, FunctionTypeC.AI_FUN_8007a680);
        Register(ScriptHelper.ProgramCTick, 60, FunctionTypeC.AI_UpdateIceProjectile);
        Register(ScriptHelper.ProgramCTick, 61, FunctionTypeC.AI_FUN_8007a958);
        Register(ScriptHelper.ProgramCTick, 62, FunctionTypeC.AI_FUN_8007a978);
        Register(ScriptHelper.ProgramCTick, 63, FunctionTypeC.AI_FUN_8007ac60);
        Register(ScriptHelper.ProgramCTick, 64, FunctionTypeC.AI_ApplyMatchingEntity);
        Register(ScriptHelper.ProgramCTick, 65, FunctionTypeC.AI_UpdateHomingProject);
        Register(ScriptHelper.ProgramCTick, 66, FunctionTypeC.AI_FUN_8007b04c);
        Register(ScriptHelper.ProgramCTick, 67, FunctionTypeC.AI_FUN_8007b1f0);
        Register(ScriptHelper.ProgramCTick, 68, FunctionTypeC.AI_FUN_8007b3c4);
        Register(ScriptHelper.ProgramCTick, 69, FunctionTypeC.AI_FUN_8007b6ec);
        Register(ScriptHelper.ProgramCTick, 70, FunctionTypeC.AI_FUN_8007b7b0);
        Register(ScriptHelper.ProgramCTick, 71, FunctionTypeC.AI_FUN_8007b834);
        Register(ScriptHelper.ProgramCTick, 72, FunctionTypeC.AI_ProcessWarpTransitionState);
        Register(ScriptHelper.ProgramCTick, 73, FunctionTypeC.AI_FUN_8007bb30);
        Register(ScriptHelper.ProgramCTick, 74, FunctionTypeC.AI_FUN_8007bb9c);
        Register(ScriptHelper.ProgramCTick, 75, FunctionTypeC.AI_FUN_8007bd8c);
        Register(ScriptHelper.ProgramCTick, 76, FunctionTypeC.AI_FUN_8007c024);
        Register(ScriptHelper.ProgramCTick, 77, FunctionTypeC.AI_ApplyZGravityIfIdle);
        Register(ScriptHelper.ProgramCTick, 78, FunctionTypeC.AI_FUN_8007c768);
        Register(ScriptHelper.ProgramCTick, 79, FunctionTypeC.AI_FUN_8006f860);
        Register(ScriptHelper.ProgramCTick, 80, FunctionTypeC.AI_FUN_800756ec);
        Register(ScriptHelper.ProgramCTick, 81, FunctionTypeC.AI_UpdateFollowerBehaviour);
        Register(ScriptHelper.ProgramCTick, 82, FunctionTypeC.AI_FUN_80074ae8);
        Register(ScriptHelper.ProgramCTick, 83, FunctionTypeC.AI_FUN_8007763c);
        Register(ScriptHelper.ProgramCTick, 84, FunctionTypeC.AI_FUN_8007d554);
        Register(ScriptHelper.ProgramCTick, 85, FunctionTypeC.AI_FUN_80073728);
        Register(ScriptHelper.ProgramCTick, 86, FunctionTypeC.AI_FUN_80079950);
        Register(ScriptHelper.ProgramCTick, 87, FunctionTypeC.AI_UpdateEntityDelayed);
        Register(ScriptHelper.ProgramCTick, 88, AI_Melzas2.AI_Melzas2_FinalBoss);
        Register(ScriptHelper.ProgramCTick, 89, FunctionTypeC.AI_SpawnWarpIfValid);
        Register(ScriptHelper.ProgramCTick, 90, FunctionTypeC.AI_UpdateMelzas2CutsceneChannels);
        Register(ScriptHelper.ProgramCTick, 91, FunctionTypeC.AI_UpdateEntityAI_IdleLookAround);
        Register(ScriptHelper.ProgramCTick, 92, FunctionTypeC.AI_UpdateEntityTriggerWarpBehavior);
        Register(ScriptHelper.ProgramCTick, 93, FunctionTypeC.AI_UpdateEntityAI_IdleCurious);
        Register(ScriptHelper.ProgramCTick, 94, FunctionTypeC.AI_FUN_8007c0d8);
        Register(ScriptHelper.ProgramCTick, 95, FunctionTypeC.AI_FUN_80064294);
        Register(ScriptHelper.ProgramCTick, 96, FunctionTypeC.AI_FUN_800647b0);
        Register(ScriptHelper.ProgramCTick, 97, FunctionTypeC.AI_FUN_80064884);
        Register(ScriptHelper.ProgramCTick, 98, FunctionTypeC.AI_FUN_80064d90);
        Register(ScriptHelper.ProgramCTick, 99, FunctionTypeC.AI_FUN_80065100);
        Register(ScriptHelper.ProgramCTick, 100, FunctionTypeC.AI_FUN_80065204);
        Register(ScriptHelper.ProgramCTick, 101, FunctionTypeC.AI_FUN_80065750);
        Register(ScriptHelper.ProgramCTick, 102, FunctionTypeC.AI_FUN_80065b0c);
        Register(ScriptHelper.ProgramCTick, 255, FunctionTypeC.FUN_8007c174);

        Register(ScriptHelper.ProgramDTouch, 0, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramDTouch, 1, FunctionTypeD.AI_FUN_8007d9a4); 
        Register(ScriptHelper.ProgramDTouch, 2, FunctionTypeD.AI_FUN_8007da08);  
        Register(ScriptHelper.ProgramDTouch, 3, FunctionTypeD.AI_FUN_8007db38);  
        Register(ScriptHelper.ProgramDTouch, 4, AI_EmptyFunction); // null      
        Register(ScriptHelper.ProgramDTouch, 5, FunctionTypeD.AI_FUN_8007dbe0); 
        Register(ScriptHelper.ProgramDTouch, 6, FunctionTypeD.AI_FUN_8007dcd8);  
        Register(ScriptHelper.ProgramDTouch, 7, FunctionTypeD.AI_FUN_8007dd3c);  
        Register(ScriptHelper.ProgramDTouch, 8, FunctionTypeD.AI_FUN_8007dda0);  
        Register(ScriptHelper.ProgramDTouch, 9, FunctionTypeD.AI_FUN_8007de04);  
        Register(ScriptHelper.ProgramDTouch, 10, FunctionTypeD.AI_FUN_8007de68);  
        Register(ScriptHelper.ProgramDTouch, 11, FunctionTypeD.AI_FUN_8007dee8); 
        Register(ScriptHelper.ProgramDTouch, 12, FunctionTypeD.AI_FUN_8007df4c); 
        Register(ScriptHelper.ProgramDTouch, 13, FunctionTypeD.AI_FUN_8007e074); 
        Register(ScriptHelper.ProgramDTouch, 14, FunctionTypeD.AI_FUN_8007e0d8); 
        Register(ScriptHelper.ProgramDTouch, 15, FunctionTypeD.AI_FUN_8007e114); 
        Register(ScriptHelper.ProgramDTouch, 16, FunctionTypeD.AI_FUN_8007e140); 
        Register(ScriptHelper.ProgramDTouch, 17, FunctionTypeD.AI_FUN_8007e1c4); 
        Register(ScriptHelper.ProgramDTouch, 18, FunctionTypeD.AI_FUN_8007e228); 
        Register(ScriptHelper.ProgramDTouch, 19, FunctionTypeD.AI_FUN_8007e2a0); 
        Register(ScriptHelper.ProgramDTouch, 20, FunctionTypeD.AI_FUN_8007e304); 
        Register(ScriptHelper.ProgramDTouch, 21, FunctionTypeD.AI_FUN_8007e424); 
        Register(ScriptHelper.ProgramDTouch, 22, FunctionTypeD.AI_FUN_8007e548); 
        Register(ScriptHelper.ProgramDTouch, 23, FunctionTypeD.AI_FUN_8007e5b0); 
        Register(ScriptHelper.ProgramDTouch, 24, FunctionTypeD.AI_FUN_8007e628); 
        Register(ScriptHelper.ProgramDTouch, 25, FunctionTypeD.AI_FUN_8007e68c); 
        Register(ScriptHelper.ProgramDTouch, 26, FunctionTypeD.AI_FUN_8007e694); 
        Register(ScriptHelper.ProgramDTouch, 27, FunctionTypeD.AI_FUN_8007e704); 
        Register(ScriptHelper.ProgramDTouch, 28, FunctionTypeD.AI_FUN_8007e754); 
        Register(ScriptHelper.ProgramDTouch, 29, FunctionTypeD.AI_FUN_8007e790); 
        Register(ScriptHelper.ProgramDTouch, 30, FunctionTypeD.AI_FUN_8007e79c); 
        Register(ScriptHelper.ProgramDTouch, 31, FunctionTypeD.AI_FUN_8007e7e4); 
        Register(ScriptHelper.ProgramDTouch, 32, FunctionTypeD.AI_FUN_8007e828); 
        Register(ScriptHelper.ProgramDTouch, 33, FunctionTypeD.AI_FUN_8007e86c); 
        Register(ScriptHelper.ProgramDTouch, 34, FunctionTypeD.AI_FUN_8007e8ac); 
        Register(ScriptHelper.ProgramDTouch, 35, FunctionTypeD.AI_FUN_8007e8f0); 
        Register(ScriptHelper.ProgramDTouch, 36, FunctionTypeD.AI_FUN_8007e994); 
        Register(ScriptHelper.ProgramDTouch, 37, FunctionTypeD.AI_FUN_8007e9ac); 
        Register(ScriptHelper.ProgramDTouch, 38, FunctionTypeD.AI_FUN_8007e9e8); 
        Register(ScriptHelper.ProgramDTouch, 39, FunctionTypeD.AI_FUN_8007ea24); 
        Register(ScriptHelper.ProgramDTouch, 40, FunctionTypeD.AI_FUN_8007ea84); 
        Register(ScriptHelper.ProgramDTouch, 41, FunctionTypeD.AI_FUN_8007eb1c); 
        Register(ScriptHelper.ProgramDTouch, 42, FunctionTypeD.AI_FUN_8007eb58); 
        Register(ScriptHelper.ProgramDTouch, 43, FunctionTypeD.AI_FUN_8007eba8); 
        Register(ScriptHelper.ProgramDTouch, 44, FunctionTypeD.AI_FUN_8007ebf0); 
        Register(ScriptHelper.ProgramDTouch, 45, FunctionTypeD.AI_FUN_8007ec60); 
        Register(ScriptHelper.ProgramDTouch, 46, FunctionTypeD.AI_FUN_8007ec9c); 

        Register(ScriptHelper.ProgramEDeactivate, 0, FunctionTypeE.FUN_8007ed10);
        Register(ScriptHelper.ProgramEDeactivate, 1, FunctionTypeE.FUN_8007ed10);
        Register(ScriptHelper.ProgramEDeactivate, 2, FunctionTypeE.AI_FUN_8007ed30);
        Register(ScriptHelper.ProgramEDeactivate, 3, FunctionTypeE.AI_FUN_8007eda0);
        Register(ScriptHelper.ProgramEDeactivate, 4, FunctionTypeE.AI_FUN_8007ee04);
        Register(ScriptHelper.ProgramEDeactivate, 5, FunctionTypeE.AI_FUN_8007ee68);
        Register(ScriptHelper.ProgramEDeactivate, 6, FunctionTypeE.AI_FUN_8007eef0);
        Register(ScriptHelper.ProgramEDeactivate, 7, FunctionTypeE.AI_FUN_8007ef10);
        Register(ScriptHelper.ProgramEDeactivate, 8, FunctionTypeE.AI_FUN_8007ef30);
        Register(ScriptHelper.ProgramEDeactivate, 9, FunctionTypeE.AI_HandleIceLightHitEffect);
        Register(ScriptHelper.ProgramEDeactivate, 10, FunctionTypeE.AI_FUN_8007f23c);
        Register(ScriptHelper.ProgramEDeactivate, 11, FunctionTypeE.AI_FUN_8007f25c);
        Register(ScriptHelper.ProgramEDeactivate, 12, FunctionTypeE.AI_FUN_8007f27c);
        Register(ScriptHelper.ProgramEDeactivate, 13, FunctionTypeE.AI_FUN_8007f30c);
        Register(ScriptHelper.ProgramEDeactivate, 14, FunctionTypeE.AI_FUN_8007f378);
        Register(ScriptHelper.ProgramEDeactivate, 15, FunctionTypeE.AI_FUN_8007f3b0);
        Register(ScriptHelper.ProgramEDeactivate, 16, FunctionTypeE.AI_FUN_8007f3e8);
        Register(ScriptHelper.ProgramEDeactivate, 17, FunctionTypeE.AI_UpdateArrows);
        Register(ScriptHelper.ProgramEDeactivate, 18, FunctionTypeE.AI_FUN_8007f658);
        Register(ScriptHelper.ProgramEDeactivate, 19, FunctionTypeE.AI_FUN_8007f690);
        Register(ScriptHelper.ProgramEDeactivate, 20, FunctionTypeE.AI_FUN_8007f6c8);
        Register(ScriptHelper.ProgramEDeactivate, 21, FunctionTypeE.AI_FUN_8007f7cc);
        Register(ScriptHelper.ProgramEDeactivate, 22, FunctionTypeE.AI_FUN_8007f878);
        Register(ScriptHelper.ProgramEDeactivate, 23, FunctionTypeE.AI_FUN_8007f8ac);
        Register(ScriptHelper.ProgramEDeactivate, 24, FunctionTypeE.AI_FUN_8007f974);
        Register(ScriptHelper.ProgramEDeactivate, 25, FunctionTypeE.AI_FUN_8007faa0);
        Register(ScriptHelper.ProgramEDeactivate, 26, FunctionTypeE.AI_FUN_8007fac0);
        Register(ScriptHelper.ProgramEDeactivate, 27, FunctionTypeE.AI_DestroyEntity);
        Register(ScriptHelper.ProgramEDeactivate, 28, FunctionTypeE.AI_FUN_8007fb58);

        Register(ScriptHelper.ProgramFInteract, 0, AI_EmptyFunction); // null
        Register(ScriptHelper.ProgramFInteract, 1, Script_FInteract_FUN_8007fc64);
        Register(ScriptHelper.ProgramFInteract, 255, Script_FInteract_FUN_8007fc88);
    }

    private void Register(int type, byte code, SpriteEventHandler handler)
    {
        _typeHandlers[type].Add(code, handler);
    }

    public void RunSpriteEvent(int eventType, int eventId, Entity entity)
    {
        if (eventType == ScriptHelper.ProgramBMap)
        {
            return;
        }

        var handlers = _typeHandlers[eventType];

        if (handlers.TryGetValue(eventId, out var handler)) //g_entityEventFunctionsByType  // 80098f4c
        {
            if (handler != AI_EmptyFunction && _gameEngine.StaticVariables.IsLogScriptEnabled)
            {
                _gameEngine.LogManager.Log(entity, $"exec func[{eventType}][{eventId}] => {handler.Method.Name}");
            }

            handler(_gameEngine, entity);
        }
        else
        {
            Debugger.Break();
        }
    }

    private void AI_EmptyFunction(GameEngine gameEngine, Entity entity) { }
    
    // 8007fc64
    private void Script_FInteract_FUN_8007fc64(GameEngine gameEngine, Entity entity)
    {
        entity.Bytes[0] = 1;
        entity.Bytes[1] = 0;
        entity.Bytes[2] = 0;
        entity.Bytes[3] = 0;
        _gameEngine.StaticVariables.PlayerEntity.TargetAnimationId = 0;
        _gameEngine.StaticVariables.g_playerControlFlags |= 4;
    }

    //8007fc88
    //open a treasure ?
    private void Script_FInteract_FUN_8007fc88(GameEngine gameEngine, Entity entity)
    {
        Entity entitySpawn;
        string scriptText;
        int iVar4;
        int itemId;

        if (_gameEngine.StaticVariables.g_playerControlFlags == 0
            && _gameEngine.StaticVariables.g_isGameEnding == 0
            && _gameEngine.StaticVariables.g_entitySlots[0].CarriedEntity == null
            && !_gameEngine.IsDialogFinished())
        {
            itemId = (int)entity.ContentsItemId;
            iVar4 = 0x48;

            if (itemId != 0 && entity.TargetAnimationId == 0)
            {
                entity.TargetAnimationId = 2;
                entitySpawn = _gameEngine.SpawnWarpEntity(entity, 0, (uint)(itemId + 0x1e), entity.PosX, entity.PosY, entity.PosZ + 0x100000, 0);
                iVar4 = 0x49;

                if (entitySpawn != null)
                {
                    _gameEngine.EntityManager.BlockEntitiesBy(entity);
                    entitySpawn.ForceZ = 0x8000;
                    entitySpawn.DelayOrAngle = 0x40;
                    entitySpawn.Flags &= 0xfffffe7f;
                    entitySpawn.IsBlockedByEntity = 0;
                    entitySpawn.Bytes[0] = 2;
                    entitySpawn.Bytes[1] = 0;
                    entitySpawn.Bytes[2] = 0;
                    entitySpawn.Bytes[3] = 0;
                    entitySpawn.ItemState = 0;
                    entitySpawn.AIValues[0] = (short)entity.Index; // entity but we can't so we store only the Index
                    iVar4 = _gameEngine.PlayerManager.FUN_80033d34(itemId) ? 1 : 0;
                    entitySpawn.AIValues[2] = (short)iVar4;

                    if (iVar4 != 0 && _gameEngine.StaticVariables.g_itemDropProperties[itemId].SoundSfxIndex == 0)
                    {
                        entitySpawn.AIValues[4] = 1;
                        entitySpawn.AIValues[5] = 0;
                        _gameEngine.SoundManager.LoadBgm(0);
                        return;
                    }

                    entitySpawn.AIValues[4] = 0;
                    entitySpawn.AIValues[5] = 0;
                    return;
                }
            }

            scriptText = _gameEngine.EtcRes.GetEtcString(iVar4); //0x48 or 0x49
            _gameEngine.UIManager.InitializeDialogMessage(scriptText, 0);
        }
    }
}