namespace Alundra.Gameplay.Scripts;

public class SpriteEventHandlers
{
    private readonly GameEngine _gameEngine;

    public delegate void SpriteEventHandler(Entity entity);

    private readonly Dictionary<int, SpriteEventHandler>[] _typeHandlers = new Dictionary<int, SpriteEventHandler>[6];

    public SpriteEventHandlers(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;

        _typeHandlers[ScriptHelper.ProgramALoad] = new Dictionary<int, SpriteEventHandler>();
        //there are no spriteevent handlers for map
        _typeHandlers[ScriptHelper.ProgramCTick] = new Dictionary<int, SpriteEventHandler>();
        _typeHandlers[ScriptHelper.ProgramDTouch] = new Dictionary<int, SpriteEventHandler>();
        _typeHandlers[ScriptHelper.ProgramEDeactivate] = new Dictionary<int, SpriteEventHandler>();
        _typeHandlers[ScriptHelper.ProgramFInteract] = new Dictionary<int, SpriteEventHandler>();

        //register the ones that have been implemented here
        Register(ScriptHelper.ProgramCTick, 0x17, etick_17_jarsandboxes_Handler);
    }

    private void Register(int type, byte code, SpriteEventHandler handler)
    {
        _typeHandlers[type].Add(code, handler);
    }

    public void RunSpriteHandler(int eventType, int eventId, Entity entity)
    {
        var handlers = _typeHandlers[eventType];
        if (handlers.ContainsKey(eventId))
        {
            handlers[eventId](entity);
        }
    }

    //All AI_xxx functions

    //8007b7b0
    public void etick_17_jarsandboxes_Handler(Entity entity)
    {
        if (entity.PlatformEntity == null)
        {
            return;
        }

        if (entity._24 == 0)
        {
            entity.TargetAnimationId = 0;
            return;
        }


        if(entity._24 == -1)
        {
            entity.TargetAnimationId = 3;
        }
        else
        {
            entity.TargetAnimationId = (uint)ScriptHelper.Anim24Table[entity._24];
        }

        if (entity.PlatformEntity != null)//redudant check
        {
            entity._2c = 0;
        }

        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x30) & 0xff7f;//turn off bit 8, turn on bits 5 and 6

    }
}