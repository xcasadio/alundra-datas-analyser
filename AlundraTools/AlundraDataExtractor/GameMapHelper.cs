using AlundraEngine;
using AlundraEngine.DatasBin;
using System.Drawing;
using System.Drawing.Imaging;

namespace AlundraDataExtractor;

public enum SpriteSheetLayoutMode
{
    /// <summary>
    /// The eight native 256x256 VRAM pages stacked vertically, holes included. It keeps the exported
    /// sheet aligned with the original VRAM coordinates, but a VRAM region reused under several
    /// palettes shares one cell (the last one drawn wins), so some quads crop another palette's colors.
    /// </summary>
    Original,

    /// <summary>
    /// Only the quads actually used, shelf-packed, one cell per (VRAM region, palette) pair. Default:
    /// every quad crops its own colors.
    /// </summary>
    Compact
}

public static class GameMapHelper
{
    public static void SaveTileSheet(GameMap gameMap, string fileName, TileAnimDescriptor[] tileAnimDescriptors = null)
    {
        using var bitmap = new Bitmap(GameMapTilesheetLayout.OriginalImageWidth, GameMapTilesheetLayout.OriginalImageHeight);
        using var graphics = Graphics.FromImage(bitmap);
        var tileCache = new HashSet<ushort>();

        foreach (var mapTile in gameMap.Map.MapTiles)
        {
            var tile = mapTile.TileId;

            if (tile != 0xffff)
            {
                if (tileCache.Add(tile))
                {
                    if (tileAnimDescriptors != null)
                    {
                        DrawAllAnimatedTiles(gameMap, tile, graphics, tileAnimDescriptors);
                    }
                    else
                    {
                        DrawTile(gameMap, tile, graphics);
                    }
                }
            }

            if (mapTile.WallTiles != null)
            {
                foreach (var wallTile in mapTile.WallTiles.Tiles)
                {
                    if (wallTile != 0xffff && tileCache.Add(wallTile))
                    {
                        if (tileAnimDescriptors != null)
                        {
                            DrawAllAnimatedTiles(gameMap, wallTile, graphics, tileAnimDescriptors);
                        }
                        else
                        {
                            DrawTile(gameMap, wallTile, graphics);
                        }
                    }
                }
            }
        }

        bitmap.Save(fileName, ImageFormat.Png);
    }

    private static void DrawAllAnimatedTiles(GameMap gameMap, ushort tileId, Graphics graphics, TileAnimDescriptor[] tileAnimDescriptors)
    {
        var tile = tileId & 0x3ff;

        var spriteIndex = tile >= tileAnimDescriptors.Length ? 0 : tileAnimDescriptors[tile].SpriteIndex;

        if (spriteIndex != 0 && gameMap.Info.SpriteMapEntries[spriteIndex].Enabled == 1)
        {
            var entry = gameMap.Info.SpriteMapEntries[spriteIndex];

            for (int frame = 0; frame < entry.NumberOfFrame; frame++)
            {
                ushort animatedTileId = (ushort)(tileId + frame * entry.TileHeight);
                DrawTile(gameMap, animatedTileId, graphics);
            }
        }
        else
        {
            DrawTile(gameMap, tileId, graphics);
        }
    }

    private static void DrawTile(GameMap gameMap, ushort tileId, Graphics graphics)
    {
        var tileBitmap = gameMap.GetTileBitmap(tileId);
        var localTileId = tileId & 0x3ff;
        var x = GameMapTilesheetLayout.GetTileX(localTileId);
        var y = GameMapTilesheetLayout.GetTileY(localTileId);
        graphics.DrawImage(tileBitmap, x, y);
    }

    // The portrait bit of SpriteTableHeader.FlagsPortraitShadowType (EntityFlags.HasPortrait).
    private const byte HasPortraitFlag = 0x80;

    // Fills SpriteRecord.DialoguePortrait of every record that has a portrait, so that SaveSpriteSheet gives
    // it a cell and a position and the map JSON carries it. Call it right after GameMap.Load, with the reader
    // the map was loaded from.
    public static void LoadDialoguePortraits(GameMap gameMap, BinaryReader br)
    {
        foreach (var spriteRecord in gameMap.SpriteInfo.SpriteRecords.Where(x => x != null))
        {
            if ((spriteRecord.Header.FlagsPortraitShadowType & HasPortraitFlag) != 0)
            {
                spriteRecord.DialoguePortrait = spriteRecord.GetPortraitImageset(br).Images[0];
            }
        }
    }

    // Writes the map spritesheet PNG and records, on every SiImage, where its quad landed there
    // (AtlasX/AtlasY). Call this before serializing the map to JSON so those fields are set.
    //
    // Two layouts are available, see SpriteSheetLayoutMode. Both deduplicate on Signature
    // (Spritesheet+Palette+SourceX+SourceY+Swidth+Sheight already combined, see SiImage), so a quad
    // and its mirrored twin share one cell, and both stamp the resulting position on every SiImage
    // instance carrying that signature.
    public static void SaveSpriteSheet(GameMap gameMap, string fileName, SpriteSheetLayoutMode layoutMode = SpriteSheetLayoutMode.Compact)
    {
        var uniqueImages = new List<SiImage>();
        var imagesBySignature = new Dictionary<long, List<SiImage>>();

        foreach (var image in EnumerateImages(gameMap))
        {
            if (!imagesBySignature.TryGetValue(image.Signature, out var images))
            {
                images = new List<SiImage>();
                imagesBySignature[image.Signature] = images;
                uniqueImages.Add(image);
            }

            images.Add(image);
        }

        var layout = layoutMode switch
        {
            SpriteSheetLayoutMode.Original => CreateOriginalSpriteSheetLayout(uniqueImages),
            SpriteSheetLayoutMode.Compact => CreateCompactSpriteSheetLayout(uniqueImages),
            _ => throw new ArgumentOutOfRangeException(nameof(layoutMode), layoutMode, "Unsupported spritesheet layout mode.")
        };

        var canvas = new ushort[layout.Width * layout.Height];

        foreach (var image in layout.DrawOrder)
        {
            var (x, y) = layout.PositionBySignature[image.Signature];
            DrawSpriteWords(canvas, layout.Width, layout.Height, gameMap.GetSpriteWords(image), image.Swidth, image.Sheight, x, y);
        }

        SaveWordCanvas(canvas, layout.Width, layout.Height, fileName);

        foreach (var (signature, position) in layout.PositionBySignature)
        {
            foreach (var image in imagesBySignature[signature])
            {
                image.AtlasX = position.X;
                image.AtlasY = position.Y;
            }
        }
    }

    // Sprite sheets are built from the raw CLUT words, not from drawn bitmaps: a word keeps its bit 15 (STP),
    // and drawing 128-alpha texels through Graphics.DrawImage would blend their RGB with what is underneath.
    // A word 0x0000 is transparent and never overwrites; any other word does (the last texel drawn wins,
    // alpha code included).
    private static void DrawSpriteWords(ushort[] canvas, int canvasWidth, int canvasHeight, ushort[] words, int width, int height, int x, int y)
    {
        for (var row = 0; row < height && y + row < canvasHeight; row++)
        {
            for (var column = 0; column < width && x + column < canvasWidth; column++)
            {
                var word = words[row * width + column];

                if (word != 0)
                {
                    canvas[(y + row) * canvasWidth + x + column] = word;
                }
            }
        }
    }

    // Alpha code per texel (E19.g G0-R1): 0 for the transparent word 0x0000 (left as it is), 128 when bit 15 (STP) is set
    // (0x8000 included, which is semi-transparent black), 255 otherwise. The RGB is the one the extractor has
    // always written: ImageHelper.FromPsxColor's channels, in Format32bppArgb memory order (B, G, R, A), so the
    // PNG red is the word's low five bits.
    private const byte AlphaSemiTransparent = 128;
    private const byte AlphaOpaque = 255;

    private static void SaveWordCanvas(ushort[] canvas, int width, int height, string fileName)
    {
        var pixels = new byte[width * height * 4];

        for (var i = 0; i < canvas.Length; i++)
        {
            var word = canvas[i];

            if (word == 0)
            {
                continue;
            }

            pixels[i * 4] = (byte)(((word >> 10) & 0x1f) << 3);
            pixels[i * 4 + 1] = (byte)(((word >> 5) & 0x1f) << 3);
            pixels[i * 4 + 2] = (byte)((word & 0x1f) << 3);
            pixels[i * 4 + 3] = (word & 0x8000) != 0 ? AlphaSemiTransparent : AlphaOpaque;
        }

        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var data = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
        bitmap.UnlockBits(data);
        bitmap.Save(fileName, ImageFormat.Png);
    }

    // Original layout (no longer the default, see Compact): the eight 256x256 VRAM pages stacked
    // vertically, each quad drawn at the VRAM window it samples (SourceX/SourceY, not Sx/Sy - a
    // mirrored quad names its source one texel early, see SiImage). Pages keep their holes, so the
    // sheet stays readable next to the original VRAM dumps, and AtlasX/AtlasY come out equal to the
    // native coordinates.
    //
    // These coordinates are not collision-free: the same VRAM region is legitimately reused with a
    // different palette across frames of the same animation (e.g. a color-cycling sparkle), and all
    // of those quads land on one cell here, so the last one drawn wins and the others crop the
    // wrong color. Draw order is first-seen order, as the historical export had it. Compact, the
    // default, keeps every (region, palette) pair.
    private static SpriteSheetLayout CreateOriginalSpriteSheetLayout(List<SiImage> uniqueImages)
    {
        var positionBySignature = new Dictionary<long, (int X, int Y)>();

        foreach (var image in uniqueImages)
        {
            positionBySignature[image.Signature] = (image.SourceX, (image.Spritesheet & 0x7) * VramPageSize + image.SourceY);
        }

        return new SpriteSheetLayout(VramPageSize, VramPageSize * VramPageCount, uniqueImages, positionBySignature);
    }

    // Compact layout: one cell per unique Signature, so a region reused under several palettes gets
    // one cell per palette and every quad crops the color it was meant to show. This is the default
    // layout of the sprite sheets. Tallest-first shelf packing: simple, deterministic, and good
    // enough for the small (mostly 16-48px) quads found in practice.
    private static SpriteSheetLayout CreateCompactSpriteSheetLayout(List<SiImage> uniqueImages)
    {
        var packingOrder = CompactPackingOrder(uniqueImages);
        var (positions, canvasHeight) = ShelfPack(packingOrder);
        var positionBySignature = new Dictionary<long, (int X, int Y)>();

        for (var i = 0; i < packingOrder.Count; i++)
        {
            positionBySignature[packingOrder[i].Signature] = positions[i];
        }

        return new SpriteSheetLayout(CompactCanvasWidth, Math.Max(canvasHeight, 1), packingOrder, positionBySignature);
    }

    private const int CompactCanvasWidth = 512;

    private static List<SiImage> CompactPackingOrder(IEnumerable<SiImage> images)
    {
        return images
            .OrderByDescending(image => image.Sheight)
            .ThenBy(image => image.Signature)
            .ToList();
    }

    // Shelf packing of the images in the order given, on a CompactCanvasWidth-wide canvas, 1 px of padding.
    // Returns the position of each image (same index) and the canvas height.
    private static (List<(int X, int Y)> Positions, int CanvasHeight) ShelfPack(List<SiImage> packingOrder)
    {
        const int padding = 1; // keep neighbouring cells from bleeding into each other when sampled

        var positions = new List<(int X, int Y)>(packingOrder.Count);
        int cursorX = 0, cursorY = 0, shelfHeight = 0, canvasHeight = 0;

        foreach (var image in packingOrder)
        {
            int cellWidth = image.Swidth + padding;
            int cellHeight = image.Sheight + padding;

            if (cursorX + cellWidth > CompactCanvasWidth)
            {
                cursorX = 0;
                cursorY += shelfHeight;
                shelfHeight = 0;
            }

            positions.Add((cursorX, cursorY));
            cursorX += cellWidth;
            shelfHeight = Math.Max(shelfHeight, cellHeight);
            canvasHeight = Math.Max(canvasHeight, cursorY + shelfHeight);
        }

        return (positions, canvasHeight);
    }

    // Writes the map's effect sheet, if it has any effect quad, and records on every effect quad where its
    // cell landed (AtlasX/AtlasY). Call this before serializing the map to JSON, like SaveSpriteSheet.
    //
    // The effect quads are in no entity animation (SpriteInfo.SpriteEffectRecords), so the entity sheet has
    // never drawn them and their AtlasX/AtlasY were always 0. The sheet is always Compact: one cell per
    // (VRAM page, palette, source region), not per Signature - two quads that differ only by the semi/ABR bits
    // of the Spritesheet byte show the same texels, and those bits belong to the quad, not to the cell. Quads
    // of size 0 x 0 (map 161) get no cell and keep AtlasX/AtlasY = 0. Only the effect SiImage instances are
    // stamped: an entity image with the same Signature keeps its own entity-sheet position.
    public static void SaveEffectSheet(GameMap gameMap, string fileName)
    {
        var effectImages = EnumerateEffectImages(gameMap).Where(image => image.Swidth != 0 && image.Sheight != 0).ToList();
        var representativeByCell = new Dictionary<EffectCell, SiImage>();

        foreach (var image in effectImages)
        {
            representativeByCell.TryAdd(GetEffectCell(image), image);
        }

        if (representativeByCell.Count == 0)
        {
            return;
        }

        var packingOrder = CompactPackingOrder(representativeByCell.Values);
        var (positions, canvasHeight) = ShelfPack(packingOrder);
        var height = Math.Max(canvasHeight, 1);
        var positionByCell = new Dictionary<EffectCell, (int X, int Y)>();
        var canvas = new ushort[CompactCanvasWidth * height];

        for (var i = 0; i < packingOrder.Count; i++)
        {
            var image = packingOrder[i];
            positionByCell[GetEffectCell(image)] = positions[i];
            DrawSpriteWords(canvas, CompactCanvasWidth, height, gameMap.GetSpriteWords(image), image.Swidth, image.Sheight, positions[i].X, positions[i].Y);
        }

        SaveWordCanvas(canvas, CompactCanvasWidth, height, fileName);

        foreach (var image in effectImages)
        {
            var (x, y) = positionByCell[GetEffectCell(image)];
            image.AtlasX = x;
            image.AtlasY = y;
        }
    }

    // JUSTIFICATION: C# language bridge only - the dictionary key of an effect cell.
    private readonly record struct EffectCell(int Page, byte Palette, byte SourceX, byte SourceY, byte Width, byte Height);

    private static EffectCell GetEffectCell(SiImage image)
    {
        return new EffectCell(image.Spritesheet & 0x7, image.Palette, image.SourceX, image.SourceY, image.Swidth, image.Sheight);
    }

    private static IEnumerable<SiImage> EnumerateEffectImages(GameMap gameMap)
    {
        foreach (var effectRecord in gameMap.SpriteInfo.SpriteEffectRecords.Where(x => x?.PreloadedAnims != null))
        {
            foreach (var animation in effectRecord.PreloadedAnims.Where(x => x?.Frames != null))
            {
                foreach (var frame in animation.Frames)
                {
                    if (frame?.Images?.Images == null)
                    {
                        continue;
                    }

                    foreach (var image in frame.Images.Images)
                    {
                        yield return image;
                    }
                }
            }
        }
    }

    // JUSTIFICATION: C# language bridge only - carries one resolved spritesheet layout so both modes
    // share the drawing and AtlasX/AtlasY stamping code above.
    private sealed record SpriteSheetLayout(
        int Width,
        int Height,
        IReadOnlyList<SiImage> DrawOrder,
        Dictionary<long, (int X, int Y)> PositionBySignature);

    private const int VramPageSize = 256;
    private const int VramPageCount = 8;

    private static IEnumerable<SiImage> EnumerateImages(GameMap gameMap)
    {
        foreach (var spriteRecord in gameMap.SpriteInfo.SpriteRecords.Where(x => x != null))
        {
            if (spriteRecord.AnimSets == null)
            {
                continue;
            }

            foreach (var animationSet in spriteRecord.AnimSets)
            {
                if (animationSet == null)
                {
                    continue;
                }

                foreach (var animation in animationSet.PreloadedAnims)
                {
                    if (animation?.Frames == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < animation.NumberOfFrames; i++)
                    {
                        var frame = animation.Frames[i];

                        if (frame?.Images?.Images == null)
                        {
                            continue;
                        }

                        foreach (var image in frame.Images.Images)
                        {
                            yield return image;
                        }
                    }
                }
            }
        }

        // The inventory's opening portrait belongs to no animation (GameMap.InventoryPortrait): it enters
        // the atlas after every animation image and receives its AtlasX/AtlasY like them.
        if (gameMap.InventoryPortrait != null)
        {
            yield return gameMap.InventoryPortrait;
        }

        // The dialogue portraits come last too, in record order: they belong to no animation either.
        foreach (var spriteRecord in gameMap.SpriteInfo.SpriteRecords.Where(x => x?.DialoguePortrait != null))
        {
            yield return spriteRecord.DialoguePortrait;
        }
    }
}