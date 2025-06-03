namespace AlundraEngine.Gameplay;

public class Frame {
    byte delay; /* delay & 0x80 → indique un frame avec données de sprite/transform. */
    byte transformIndexLow; /* transformIndex = high<<8 | low → index dans la table de TransformData */
    byte transformIndexHigh; /* transformIndex = high<<8 | low → index dans la table de TransformData */
    byte spriteIndexLow; /* spriteIndex = high<<8 | low → index vers la table de SpriteImageData */
    byte spriteIndexHigh; /* spriteIndex = high<<8 | low → index vers la table de SpriteImageData */
};