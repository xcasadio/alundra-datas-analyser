namespace Alundra.Gameplay.Scripts;

public class SpriteEventHandlers
{
    public delegate void SpriteEventHandler(Entity entity);

    private GameState _gameState;
    private readonly Dictionary<int, SpriteEventHandler>[] _typeHandlers = new Dictionary<int, SpriteEventHandler>[6];

    public SpriteEventHandlers(GameState gameState)
    {
        _gameState = gameState;

        _typeHandlers[ScriptHelper.ProgramALoad] = new Dictionary<int, SpriteEventHandler>();
        //there are no spriteevent handlers for map
        _typeHandlers[ScriptHelper.ProgramCTick] = new Dictionary<int, SpriteEventHandler>();
        _typeHandlers[ScriptHelper.ProgramDTouch] = new Dictionary<int, SpriteEventHandler>();
        _typeHandlers[ScriptHelper.ProgramEDeactivate] = new Dictionary<int, SpriteEventHandler>();
        _typeHandlers[ScriptHelper.ProgramFInteract] = new Dictionary<int, SpriteEventHandler>();

        //register the ones that have been implimented here
        Register(ScriptHelper.ProgramCTick, 0x17, etick_17_jarsandboxes_Handler);
    }

    private void Register(int type, byte code, SpriteEventHandler handler)
    {
        _typeHandlers[type].Add(code, handler);
    }

    public void RunSpriteHandler(int eventtype, int eventid, Entity entity)
    {
        var handlers = _typeHandlers[eventtype];
        if (handlers.ContainsKey(eventid))
        {
            handlers[eventid](entity);
            return;
        }
    }

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
            entity.TargetAnimationId = ScriptHelper.Anim24Table[entity._24];
        }

        if (entity.PlatformEntity != null)//redudant check
        {
            entity._2c = 0;
        }

        entity.PlatformEntity = null;
        entity.Flags = (entity.Flags | 0x30) & 0xff7f;//turn off bit 8, turn on bits 5 and 6

    }
}