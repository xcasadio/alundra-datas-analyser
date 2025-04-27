namespace Alundra.Gameplay;

public class EntityEffect {
    int Id;
    int[] mapEffectRecord; /* MapEffectRecord */
    int[] spriteEffectRecord; /* SpriteEffectRecord */
    int[] spriteRef; /* SpriteRef */
    EntityEffect[] nextEffect;
    int _14;
    int _18;
    int zSortValue2;
    int _20;
    int _24;
    int sheetSize;
    int paletteIndex;
    int mapEffectId;
    int updateMode;
    Entity[] attachedEntity;
    int x;
    int y;
    int z;
    int velocityX;
    int velocityY;
    int velocityZ;
    int xForce;
    int yForce;
    int zForce;
    int zSortOffset;
    int zSortValue; /* stored to 1c, is it a depth sorting id? */
    int status; /* 2 is active */
    bool targetIsMapSprite;
    bool currentIsMapSprite;
    byte targetSpriteTableIndex;
    byte currentSpriteTableIndex;
    byte targetAnimation;
    byte currentAnimation;
    short _72;
    EffectFrame[] effectFrame;
    EffectFrame[] initialEffectFrame;
    byte delay;
    byte destroyFlag; /* if this is set true the effect is destroyed on next update (status = 0) */
};