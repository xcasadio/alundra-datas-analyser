using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AlundraGameRuntimeMcpServer;

[McpServerToolType]
internal sealed class RuntimeHelperTools(RuntimeInspectorClient client)
{
    [McpServerTool(Name = "alundra_get_player_core")]
    [Description("Get a compact player snapshot for quick runtime/Ghidra comparisons.")]
    public async Task<CallToolResult> GetPlayerCore(CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Frame: {await TryReadDisplayValueAsync("static.FrameNumber", cancellationToken)}");
            builder.AppendLine($"Paused: {await TryReadDisplayValueAsync("static.IsGamePaused", cancellationToken)}");
            builder.AppendLine($"DoNextFrame: {await TryReadDisplayValueAsync("static.DoNextFrame", cancellationToken)}");
            builder.AppendLine($"CurrentMap: {await TryReadDisplayValueAsync("static.g_currentMap", cancellationToken)}");
            builder.AppendLine($"DesiredMap: {await TryReadDisplayValueAsync("static.g_desiredMap", cancellationToken)}");
            builder.AppendLine(await BuildEntityCoreBodyAsync("player", cancellationToken));
            return ToolSupport.Success(builder.ToString().TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_get_map_core")]
    [Description("Get a compact current-map snapshot with live ids and header/info fields useful for comparison work.")]
    public async Task<CallToolResult> GetMapCore(CancellationToken cancellationToken = default)
    {
        try
        {
            var map = await client.ReadValueAsync("map", cancellationToken);

            var builder = new StringBuilder();
            builder.AppendLine($"Frame: {await TryReadDisplayValueAsync("static.FrameNumber", cancellationToken)}");
            builder.AppendLine($"Paused: {await TryReadDisplayValueAsync("static.IsGamePaused", cancellationToken)}");
            builder.AppendLine($"DoNextFrame: {await TryReadDisplayValueAsync("static.DoNextFrame", cancellationToken)}");
            builder.AppendLine($"CurrentMap: {await TryReadDisplayValueAsync("static.g_currentMap", cancellationToken)}");
            builder.AppendLine($"DesiredMap: {await TryReadDisplayValueAsync("static.g_desiredMap", cancellationToken)}");
            builder.AppendLine($"Path: {map.Path}");
            builder.AppendLine($"Type: {map.TypeName}");
            builder.AppendLine($"Summary: {map.Summary}");
            builder.AppendLine($"MapInfo: mapId={await TryReadDisplayValueAsync("map.Info.MapId", cancellationToken)} gravity={await TryReadDisplayValueAsync("map.Info.Gravity", cancellationToken)} zViscosity={await TryReadDisplayValueAsync("map.Info.ZViscosity", cancellationToken)} slideEffectId={await TryReadDisplayValueAsync("map.Info.SlideEffectId", cancellationToken)} balanceLevel={await TryReadDisplayValueAsync("map.Info.BalanceLevel", cancellationToken)}");
            builder.AppendLine($"MapFlags: loaded={await TryReadDisplayValueAsync("map.Loaded", cancellationToken)} offset={await TryReadDisplayValueAsync("map.Offset", cancellationToken)} info._10={await TryReadDisplayValueAsync("map.Info._10", cancellationToken)} info._11={await TryReadDisplayValueAsync("map.Info._11", cancellationToken)}");
            builder.AppendLine($"HeaderOffsets: info={await TryReadDisplayValueAsync("map.Header.InfoBlockOffset", cancellationToken)} map={await TryReadDisplayValueAsync("map.Header.MapBlockOffset", cancellationToken)} spriteRecords={await TryReadDisplayValueAsync("map.Header.SpriteRecordsOffset", cancellationToken)} spriteSheet={await TryReadDisplayValueAsync("map.Header.SpriteSheetOffset", cancellationToken)} strings={await TryReadDisplayValueAsync("map.Header.StringTableOffset", cancellationToken)}");
            return ToolSupport.Success(builder.ToString().TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_get_entity_core")]
    [Description("Get a compact entity snapshot from a runtime entity path. Defaults to the player entity.")]
    public async Task<CallToolResult> GetEntityCore(
        [Description("Runtime path to an entity, such as 'player' or 'engine.StaticVariables.PlayerEntity'.")] string path = "player",
        CancellationToken cancellationToken = default)
    {
        try
        {
            return ToolSupport.Success(await BuildEntityCoreBodyAsync(NormalizePath(path, "player"), cancellationToken));
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_get_script_state")]
    [Description("Get a compact EventProgramState snapshot for an entity path. Defaults to the player entity.")]
    public async Task<CallToolResult> GetScriptState(
        [Description("Runtime path to an entity whose EventProgramState should be inspected. Defaults to 'player'.")] string path = "player",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPath = NormalizePath(path, "player");
            var entity = await client.ReadValueAsync(normalizedPath, cancellationToken);
            var scriptPath = $"{normalizedPath}.EventProgramState";

            var builder = new StringBuilder();
            builder.AppendLine($"EntityPath: {entity.Path}");
            builder.AppendLine($"EntityType: {entity.TypeName}");
            builder.AppendLine($"EntitySummary: {entity.Summary}");
            builder.AppendLine($"EventTrigger: {await TryReadDisplayValueAsync($"{normalizedPath}.EventTrigger", cancellationToken)}");
            builder.AppendLine($"MapEventProgramId: {await TryReadDisplayValueAsync($"{normalizedPath}.MapEventProgramId", cancellationToken)}");
            builder.AppendLine($"Script: sp={await TryReadDisplayValueAsync($"{scriptPath}.Sp", cancellationToken)} codeIndex={await TryReadDisplayValueAsync($"{scriptPath}.CodeIndex", cancellationToken)} result={await TryReadDisplayValueAsync($"{scriptPath}.Result", cancellationToken)} _30={await TryReadDisplayValueAsync($"{scriptPath}._30", cancellationToken)} _34={await TryReadDisplayValueAsync($"{scriptPath}._34", cancellationToken)}");
            builder.AppendLine($"Parameters: {await ReadIndexedValuesAsync($"{scriptPath}.Parameters", 10, cancellationToken)}");
            builder.AppendLine($"ProgramIndexes: {await ReadIndexedValuesAsync($"{normalizedPath}.ProgramIndexes", 6, cancellationToken)}");
            builder.AppendLine($"SpriteProgramIndexes: {await ReadIndexedValuesAsync($"{normalizedPath}.SpriteProgramIndexes", 6, cancellationToken)}");
            return ToolSupport.Success(builder.ToString().TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_get_animation_core")]
    [Description("Get a compact animation-state snapshot for an entity path. Defaults to the player entity.")]
    public async Task<CallToolResult> GetAnimationCore(
        [Description("Runtime path to an entity, such as 'player'.")] string path = "player",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPath = NormalizePath(path, "player");
            var entity = await client.ReadValueAsync(normalizedPath, cancellationToken);

            var builder = new StringBuilder();
            builder.AppendLine($"EntityPath: {entity.Path}");
            builder.AppendLine($"EntityType: {entity.TypeName}");
            builder.AppendLine($"EntitySummary: {entity.Summary}");
            builder.AppendLine($"Animation: current={await TryReadDisplayValueAsync($"{normalizedPath}.CurrentAnimationId", cancellationToken)} target={await TryReadDisplayValueAsync($"{normalizedPath}.TargetAnimationId", cancellationToken)} frameIndex={await TryReadDisplayValueAsync($"{normalizedPath}.AnimationFrameIndex", cancellationToken)} direction={await TryReadDisplayValueAsync($"{normalizedPath}.AnimationDirection", cancellationToken)}");
            builder.AppendLine($"Timing: frameCounter={await TryReadDisplayValueAsync($"{normalizedPath}.FrameCounter", cancellationToken)} nextFrameDelay={await TryReadDisplayValueAsync($"{normalizedPath}.NextFrameDelay", cancellationToken)} animCompleteCounter={await TryReadDisplayValueAsync($"{normalizedPath}.AnimCompleteCounter", cancellationToken)} animFlags={await TryReadDisplayValueAsync($"{normalizedPath}.AnimFlags", cancellationToken)}");
            builder.AppendLine($"Direction: current={await TryReadDisplayValueAsync($"{normalizedPath}.CurrentDirection", cancellationToken)} target={await TryReadDisplayValueAsync($"{normalizedPath}.TargetDirection", cancellationToken)} lastTarget={await TryReadDisplayValueAsync($"{normalizedPath}.LastTargetDirection", cancellationToken)}");
            builder.AppendLine($"Forces: forceResetAnimationFlag={await TryReadDisplayValueAsync($"{normalizedPath}.ForceResetAnimationFlag", cancellationToken)} forceAdjusted={await TryReadDisplayValueAsync($"{normalizedPath}.ForceAdjusted", cancellationToken)} isZForceApplied={await TryReadDisplayValueAsync($"{normalizedPath}.IsZForceApplied", cancellationToken)}");
            builder.AppendLine($"FrameRefs: firstFrame={await TryReadDisplayValueAsync($"{normalizedPath}.FirstFrame", cancellationToken)} frame={await TryReadDisplayValueAsync($"{normalizedPath}.Frame", cancellationToken)} animationSet={await TryReadDisplayValueAsync($"{normalizedPath}.AnimationSet", cancellationToken)}");
            return ToolSupport.Success(builder.ToString().TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_get_entity_flags")]
    [Description("Get a compact flag-oriented snapshot for an entity path. Defaults to the player entity.")]
    public async Task<CallToolResult> GetEntityFlags(
        [Description("Runtime path to an entity, such as 'player'.")] string path = "player",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedPath = NormalizePath(path, "player");
            var entity = await client.ReadValueAsync(normalizedPath, cancellationToken);

            var builder = new StringBuilder();
            builder.AppendLine($"EntityPath: {entity.Path}");
            builder.AppendLine($"EntityType: {entity.TypeName}");
            builder.AppendLine($"EntitySummary: {entity.Summary}");
            builder.AppendLine($"Identity: index={await TryReadDisplayValueAsync($"{normalizedPath}.Index", cancellationToken)} status={await TryReadDisplayValueAsync($"{normalizedPath}.Status", cancellationToken)} eventTrigger={await TryReadDisplayValueAsync($"{normalizedPath}.EventTrigger", cancellationToken)} mapEventProgramId={await TryReadDisplayValueAsync($"{normalizedPath}.MapEventProgramId", cancellationToken)}");
            builder.AppendLine($"Flags: flags={await TryReadDisplayValueAsync($"{normalizedPath}.Flags", cancellationToken)} flags2={await TryReadDisplayValueAsync($"{normalizedPath}.Flags2", cancellationToken)} combinedOr={await TryReadDisplayValueAsync($"{normalizedPath}.CombinedVramFlagsOR", cancellationToken)} combinedAnd={await TryReadDisplayValueAsync($"{normalizedPath}.CombinedVramFlagsAND", cancellationToken)}");
            builder.AppendLine($"Collision: tileAttributes={await TryReadDisplayValueAsync($"{normalizedPath}.TileAttributes", cancellationToken)} collidedWithEntityZ={await TryReadDisplayValueAsync($"{normalizedPath}.CollidedWithEntityZ", cancellationToken)} isOnGround={await TryReadDisplayValueAsync($"{normalizedPath}.IsOnGround", cancellationToken)} platformUpdateFlag={await TryReadDisplayValueAsync($"{normalizedPath}.PlatformUpdateFlag", cancellationToken)}");
            builder.AppendLine($"Links: blockedBy={await TryReadDisplayValueAsync($"{normalizedPath}.BlockedByEntity", cancellationToken)} platform={await TryReadDisplayValueAsync($"{normalizedPath}.PlatformEntity", cancellationToken)} touching={await TryReadDisplayValueAsync($"{normalizedPath}.TouchingEntity", cancellationToken)} riding={await TryReadDisplayValueAsync($"{normalizedPath}.RidingEntity", cancellationToken)} xCollision={await TryReadDisplayValueAsync($"{normalizedPath}.XCollisionEntity", cancellationToken)}");
            return ToolSupport.Success(builder.ToString().TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_get_map_runtime_state")]
    [Description("Get a compact runtime-oriented map state snapshot using static and current-map values.")]
    public async Task<CallToolResult> GetMapRuntimeState(CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Frame: {await TryReadDisplayValueAsync("static.FrameNumber", cancellationToken)}");
            builder.AppendLine($"Paused: {await TryReadDisplayValueAsync("static.IsGamePaused", cancellationToken)}");
            builder.AppendLine($"DoNextFrame: {await TryReadDisplayValueAsync("static.DoNextFrame", cancellationToken)}");
            builder.AppendLine($"Maps: current={await TryReadDisplayValueAsync("static.g_currentMap", cancellationToken)} desired={await TryReadDisplayValueAsync("static.g_desiredMap", cancellationToken)} info.mapId={await TryReadDisplayValueAsync("map.Info.MapId", cancellationToken)}");
            builder.AppendLine($"MapInfo: gravity={await TryReadDisplayValueAsync("map.Info.Gravity", cancellationToken)} zViscosity={await TryReadDisplayValueAsync("map.Info.ZViscosity", cancellationToken)} slideEffectId={await TryReadDisplayValueAsync("map.Info.SlideEffectId", cancellationToken)} balanceLevel={await TryReadDisplayValueAsync("map.Info.BalanceLevel", cancellationToken)}");
            builder.AppendLine($"MapBlocks: offset={await TryReadDisplayValueAsync("map.Offset", cancellationToken)} infoOffset={await TryReadDisplayValueAsync("map.Header.InfoBlockOffset", cancellationToken)} mapOffset={await TryReadDisplayValueAsync("map.Header.MapBlockOffset", cancellationToken)} stringsOffset={await TryReadDisplayValueAsync("map.Header.StringTableOffset", cancellationToken)}");
            builder.AppendLine($"RuntimeRefs: scrollParameters={await TryReadDisplayValueAsync("map.ScrollParameters", cancellationToken)} scrollScreen={await TryReadDisplayValueAsync("map.ScrollScreen", cancellationToken)} spriteInfo={await TryReadDisplayValueAsync("map.SpriteInfo", cancellationToken)} mapData={await TryReadDisplayValueAsync("map.Map", cancellationToken)}");
            return ToolSupport.Success(builder.ToString().TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_get_trace_compact")]
    [Description("Return a compact execution trace with one short line per checkpoint.")]
    public async Task<CallToolResult> GetTraceCompact(
        [Description("Number of recent entries to return (default 8).")] int count = 8,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var trace = await client.GetTraceAsync(count, cancellationToken);
            if (trace.Entries.Count == 0)
            {
                return ToolSupport.Success("No runtime trace entries recorded yet.");
            }

            var lines = trace.Entries.Select(entry =>
                $"#{entry.Sequence} f={entry.Frame} {entry.Checkpoint} {entry.TimestampUtc:HH:mm:ss.fffK}");

            return ToolSupport.Success(string.Join(Environment.NewLine, lines));
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    private async Task<string> BuildEntityCoreBodyAsync(string path, CancellationToken cancellationToken)
    {
        var entity = await client.ReadValueAsync(path, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine($"Path: {entity.Path}");
        builder.AppendLine($"Type: {entity.TypeName}");
        builder.AppendLine($"Summary: {entity.Summary}");
        builder.AppendLine($"Identity: index={await TryReadDisplayValueAsync($"{path}.Index", cancellationToken)} status={await TryReadDisplayValueAsync($"{path}.Status", cancellationToken)} name={await TryReadDisplayValueAsync($"{path}.Name", cancellationToken)}");
        builder.AppendLine($"Hp: {await TryReadDisplayValueAsync($"{path}.Hp", cancellationToken)}/{await TryReadDisplayValueAsync($"{path}.HpMax", cancellationToken)}");
        builder.AppendLine($"Pos: x={await TryReadDisplayValueAsync($"{path}.PosX", cancellationToken)} y={await TryReadDisplayValueAsync($"{path}.PosY", cancellationToken)} z={await TryReadDisplayValueAsync($"{path}.PosZ", cancellationToken)}");
        builder.AppendLine($"Tile: x={await TryReadDisplayValueAsync($"{path}.TileX", cancellationToken)} y={await TryReadDisplayValueAsync($"{path}.TileY", cancellationToken)} z={await TryReadDisplayValueAsync($"{path}.TileZ", cancellationToken)}");
        builder.AppendLine($"Direction: current={await TryReadDisplayValueAsync($"{path}.CurrentDirection", cancellationToken)} target={await TryReadDisplayValueAsync($"{path}.TargetDirection", cancellationToken)}");
        builder.AppendLine($"Animation: current={await TryReadDisplayValueAsync($"{path}.CurrentAnimationId", cancellationToken)} target={await TryReadDisplayValueAsync($"{path}.TargetAnimationId", cancellationToken)} frameIndex={await TryReadDisplayValueAsync($"{path}.AnimationFrameIndex", cancellationToken)}");
        builder.AppendLine($"Logic: eventTrigger={await TryReadDisplayValueAsync($"{path}.EventTrigger", cancellationToken)} mapEventProgramId={await TryReadDisplayValueAsync($"{path}.MapEventProgramId", cancellationToken)}");
        builder.AppendLine($"Flags: flags={await TryReadDisplayValueAsync($"{path}.Flags", cancellationToken)} flags2={await TryReadDisplayValueAsync($"{path}.Flags2", cancellationToken)}");
        return builder.ToString().TrimEnd();
    }

    private async Task<string> ReadIndexedValuesAsync(string path, int count, CancellationToken cancellationToken)
    {
        var values = new string[count];

        for (var index = 0; index < count; index++)
        {
            values[index] = await TryReadDisplayValueAsync($"{path}[{index}]", cancellationToken);
        }

        return $"[{string.Join(", ", values)}]";
    }

    private async Task<string> TryReadDisplayValueAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var value = await client.ReadValueAsync(path, cancellationToken);
            return string.IsNullOrWhiteSpace(value.ScalarValue) ? value.Summary : value.ScalarValue;
        }
        catch (Exception exception)
        {
            return $"<error: {exception.Message}>";
        }
    }

    private static string NormalizePath(string path, string fallback)
        => string.IsNullOrWhiteSpace(path) ? fallback : path.Trim();
}