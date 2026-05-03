using System.Text.Json;

namespace AlundraGameRuntimeMcpServer;

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
}

internal sealed class RuntimePathRequest
{
    public string? Path { get; init; }
}

internal sealed class RuntimeTraceRequest
{
    public int Count { get; init; } = 20;
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