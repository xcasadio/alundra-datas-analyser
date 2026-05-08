using System.Text.Json;

namespace AlundraEngine.RuntimeInspection;

internal static class RuntimeInspectorJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        IncludeFields = true,
    };
}

internal sealed class RuntimeInspectionRequest
{
    public string Command { get; init; } = string.Empty;
    public JsonElement Arguments { get; init; }
}

internal sealed class RuntimeInspectionResponse
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public JsonElement Data { get; init; }

    public static RuntimeInspectionResponse FromData<T>(T data)
        => new()
        {
            Success = true,
            Data = JsonSerializer.SerializeToElement(data, RuntimeInspectorJson.Options),
        };

    public static RuntimeInspectionResponse FromError(string error)
        => new()
        {
            Success = false,
            Error = error,
            Data = JsonSerializer.SerializeToElement<object?>(null, RuntimeInspectorJson.Options),
        };
}

internal sealed class RuntimePathRequest
{
    public string? Path { get; init; }
}

internal sealed class RuntimeTraceRequest
{
    public int Count { get; init; } = 20;
}

internal sealed class RuntimeSnapshotResponse
{
    public string FilePath { get; init; } = string.Empty;
}

internal sealed class RuntimeFrameProbeSnapshot
{
    public string Checkpoint { get; init; } = string.Empty;
    public uint Frame { get; init; }
    public bool IsWarpTransitionRunning { get; init; }
    public int MapTransitionEffectId { get; init; }
    public int IsGameEnding { get; init; }
    public string FilePath { get; init; } = string.Empty;
}

internal sealed class RuntimeFadeOutQueueResponse
{
    public uint Frame { get; init; }
    public int PostProcessState { get; init; }
    public int CurrentTransitionType { get; init; }
}

internal sealed class RuntimeTemporaryWarpQueueRequest
{
    public int EffectId { get; init; }
}

internal sealed class RuntimeTemporaryWarpQueueResponse
{
    public uint Frame { get; init; }
    public int EffectId { get; init; }
    public int MapTransitionEffectId { get; init; }
}

internal sealed class RuntimeStatusSnapshot
{
    public string Checkpoint { get; init; } = string.Empty;
    public uint Frame { get; init; }
    public bool IsPaused { get; init; }
    public bool DoNextFrame { get; init; }
    public uint CurrentMap { get; init; }
    public uint DesiredMap { get; init; }
    public RuntimePlayerSnapshot? Player { get; init; }
}

internal sealed class RuntimeCollisionTileSnapshot
{
    public int Index { get; init; }
    public int MapHeight { get; init; }
    public byte Walkability { get; init; }
    public byte GroundProperty { get; init; }
    public byte Slope { get; init; }
    public byte Height { get; init; }
    public uint TileFlags { get; init; }
    public uint CollisionFlag { get; init; }
}

internal sealed class RuntimePlayerCollisionSnapshot
{
    public string Checkpoint { get; init; } = string.Empty;
    public uint Frame { get; init; }
    public int PosX { get; init; }
    public int PosY { get; init; }
    public int PosZ { get; init; }
    public int ModdedPosZ { get; init; }
    public int TerrainHeight { get; init; }
    public uint EntityFlags { get; init; }
    public int TileAttributes { get; init; }
    public int Slope18c { get; init; }
    public uint CurrentAnimationId { get; init; }
    public uint TargetAnimationId { get; init; }
    public int FinalForceX { get; init; }
    public int FinalForceY { get; init; }
    public int ForceAdjusted { get; init; }
    public int WarpLockTimer { get; init; }
    public uint GravityFlag { get; init; }
    public uint CollisionFlagsOr { get; init; }
    public IReadOnlyList<uint> CollisionFlags { get; init; } = [];
    public IReadOnlyList<RuntimeCollisionTileSnapshot> Tiles { get; init; } = [];
}

internal sealed class RuntimePlayerXYMoveSnapshot
{
    public uint Frame { get; init; }
    public int StartPosX { get; init; }
    public int StartPosY { get; init; }
    public int StartPosZ { get; init; }
    public int EntryFinalForceX { get; init; }
    public int EntryFinalForceY { get; init; }
    public int AttemptedPosX { get; init; }
    public int AttemptedPosY { get; init; }
    public int AttemptedPosZ { get; init; }
    public int AttemptedGroundHeight { get; init; }
    public int ExitPosX { get; init; }
    public int ExitPosY { get; init; }
    public int ExitPosZ { get; init; }
    public int ExitTerrainHeight { get; init; }
    public uint CollisionFlagsOr { get; init; }
    public IReadOnlyList<uint> CollisionFlags { get; init; } = [];
    public int CandidateIndex { get; init; }
    public int ResultIndex { get; init; }
    public int IterationCount { get; init; }
    public int DidAdjustForObstacle { get; init; }
    public int ModXState { get; init; }
    public string ExitPath { get; init; } = string.Empty;
}

internal sealed class RuntimePlayerSnapshot
{
    public int PosX { get; init; }
    public int PosY { get; init; }
    public int PosZ { get; init; }
    public int Hp { get; init; }
    public int HpMax { get; init; }
    public uint CurrentAnimationId { get; init; }
    public uint TargetAnimationId { get; init; }
}

internal sealed class RuntimeStackSnapshot
{
    public string Checkpoint { get; init; } = string.Empty;
    public string StackTrace { get; init; } = string.Empty;
}

internal sealed class RuntimeValueSnapshot
{
    public string Path { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string? ScalarValue { get; init; }
}

internal sealed class RuntimeMemberSnapshot
{
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
}

internal sealed class RuntimeMembersSnapshot
{
    public string Path { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public IReadOnlyList<RuntimeMemberSnapshot> Members { get; init; } = [];
}

internal sealed class RuntimeTraceEntry
{
    public long Sequence { get; init; }
    public string Checkpoint { get; init; } = string.Empty;
    public uint Frame { get; init; }
    public DateTimeOffset TimestampUtc { get; init; }
}

internal sealed class RuntimeTraceSnapshot
{
    public IReadOnlyList<RuntimeTraceEntry> Entries { get; init; } = [];
}