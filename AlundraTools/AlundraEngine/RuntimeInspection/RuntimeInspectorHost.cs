using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Text.Json;
using AlundraEngine.Gameplay;

namespace AlundraEngine.RuntimeInspection;

public sealed class RuntimeInspectorHost : IDisposable
{
    public const string DefaultPipeName = "alundra-csharp-runtime";

    private readonly object _gameRoot;
    private readonly GameEngine _engine;
    private readonly string _pipeName;
    private readonly int _maxTraceEntries;
    private readonly ConcurrentQueue<PendingRequest> _pendingRequests = new();
    private readonly Queue<RuntimeTraceEntry> _traceEntries = new();
    private readonly object _traceLock = new();
    private readonly object _playerXYMoveLock = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Task _listenTask;

    private long _traceSequence;
    private RuntimePlayerXYMoveSnapshot? _lastPlayerXYMoveSnapshot;

    private RuntimeInspectorHost(object gameRoot, GameEngine engine, string pipeName, int maxTraceEntries)
    {
        _gameRoot = gameRoot;
        _engine = engine;
        _pipeName = pipeName;
        _maxTraceEntries = maxTraceEntries;
        _listenTask = Task.Run(ListenAsync);
    }

    public string PipeName => _pipeName;

    public static RuntimeInspectorHost? TryStart(object gameRoot, GameEngine engine)
    {
        if (!ShouldEnable())
        {
            return null;
        }

        var pipeName = Environment.GetEnvironmentVariable("ALUNDRA_RUNTIME_PIPE");
        var maxTraceEntries = ParseInt("ALUNDRA_RUNTIME_TRACE_SIZE", 256);

        return new RuntimeInspectorHost(
            gameRoot,
            engine,
            string.IsNullOrWhiteSpace(pipeName) ? DefaultPipeName : pipeName,
            Math.Clamp(maxTraceEntries, 32, 32768));
    }

    public void Checkpoint(string checkpoint)
    {
        RecordTrace(checkpoint);
        ProcessPendingRequests(checkpoint);
    }

    // JUSTIFICATION: backend MonoGame only
    internal void RecordPlayerXYMoveSnapshot(RuntimePlayerXYMoveSnapshot snapshot)
    {
        lock (_playerXYMoveLock)
        {
            _lastPlayerXYMoveSnapshot = snapshot;
        }
    }

    public void Dispose()
    {
        _shutdown.Cancel();

        while (_pendingRequests.TryDequeue(out var pending))
        {
            pending.Completion.TrySetResult(RuntimeInspectionResponse.FromError("Runtime inspector is shutting down."));
        }

        try
        {
            _listenTask.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Best effort shutdown; the game is already exiting.
        }

        _shutdown.Dispose();
    }

    private static bool ShouldEnable()
    {
        var raw = Environment.GetEnvironmentVariable("ALUNDRA_RUNTIME_INSPECTOR");
        if (string.Equals(raw, "1", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(raw, "0", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

#if DEBUG
        return true;
#else
        return false;
#endif
    }

    private async Task ListenAsync()
    {
        while (!_shutdown.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeServerStream(
                    _pipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await pipe.WaitForConnectionAsync(_shutdown.Token).ConfigureAwait(false);

                using var reader = new StreamReader(pipe, Encoding.UTF8, false, 4096, leaveOpen: true);
                using var writer = new StreamWriter(pipe, new UTF8Encoding(false), 4096, leaveOpen: true)
                {
                    AutoFlush = true,
                };

                while (!_shutdown.IsCancellationRequested && pipe.IsConnected)
                {
                    var line = await reader.ReadLineAsync().ConfigureAwait(false);
                    if (line is null)
                    {
                        break;
                    }

                    RuntimeInspectionResponse response;
                    try
                    {
                        var request = JsonSerializer.Deserialize<RuntimeInspectionRequest>(line, RuntimeInspectorJson.Options)
                                      ?? throw new InvalidOperationException("Request payload is empty.");

                        response = await EnqueueAndWaitAsync(request).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        response = RuntimeInspectionResponse.FromError(exception.Message);
                    }

                    await writer.WriteLineAsync(JsonSerializer.Serialize(response, RuntimeInspectorJson.Options)).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (_shutdown.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                if (_shutdown.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    await Task.Delay(250, _shutdown.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private Task<RuntimeInspectionResponse> EnqueueAndWaitAsync(RuntimeInspectionRequest request)
    {
        var pending = new PendingRequest(request);
        _pendingRequests.Enqueue(pending);
        return pending.Completion.Task;
    }

    private void ProcessPendingRequests(string checkpoint)
    {
        while (_pendingRequests.TryDequeue(out var pending))
        {
            RuntimeInspectionResponse response;
            try
            {
                response = ExecuteRequest(pending.Request, checkpoint);
            }
            catch (Exception exception)
            {
                response = RuntimeInspectionResponse.FromError(exception.Message);
            }

            pending.Completion.TrySetResult(response);
        }
    }

    private RuntimeInspectionResponse ExecuteRequest(RuntimeInspectionRequest request, string checkpoint)
        => request.Command switch
        {
            "status" => RuntimeInspectionResponse.FromData(CreateStatusSnapshot(checkpoint)),
            "player-collision" => RuntimeInspectionResponse.FromData(CapturePlayerCollisionSnapshot(checkpoint)),
            "player-xy-move" => RuntimeInspectionResponse.FromData(CapturePlayerXYMoveSnapshot()),
            "frame-probe" => RuntimeInspectionResponse.FromData(CaptureFrameProbe(checkpoint)),
            "stack" => RuntimeInspectionResponse.FromData(new RuntimeStackSnapshot
            {
                Checkpoint = checkpoint,
                StackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true).ToString(),
            }),
            "queue-start-fade-out" => RuntimeInspectionResponse.FromData(QueueStartFadeOut()),
            "queue-temporary-warp-effect" => RuntimeInspectionResponse.FromData(QueueTemporaryWarpEffect(ParseArguments<RuntimeTemporaryWarpQueueRequest>(request.Arguments))),
            "snapshot" => RuntimeInspectionResponse.FromData(CaptureSnapshot()),
            "trace" => RuntimeInspectionResponse.FromData(CreateTraceSnapshot(ParseArguments<RuntimeTraceRequest>(request.Arguments))),
            "read" => RuntimeInspectionResponse.FromData(ReadValue(ParseArguments<RuntimePathRequest>(request.Arguments))),
            "members" => RuntimeInspectionResponse.FromData(ListMembers(ParseArguments<RuntimePathRequest>(request.Arguments))),
            _ => RuntimeInspectionResponse.FromError($"Unknown command '{request.Command}'."),
        };

    // JUSTIFICATION: backend MonoGame only
    private RuntimeFadeOutQueueResponse QueueStartFadeOut()
    {
        _engine.StaticVariables.g_postProcessState = 1;

        return new RuntimeFadeOutQueueResponse
        {
            Frame = _engine.StaticVariables.FrameNumber,
            PostProcessState = _engine.StaticVariables.g_postProcessState,
            CurrentTransitionType = _engine.StaticVariables.g_currentTransitionType,
        };
    }

    // JUSTIFICATION: backend MonoGame only
    private RuntimeTemporaryWarpQueueResponse QueueTemporaryWarpEffect(RuntimeTemporaryWarpQueueRequest request)
    {
        _engine.QueueTemporaryWarpTransitionEffect(request.EffectId);

        return new RuntimeTemporaryWarpQueueResponse
        {
            Frame = _engine.StaticVariables.FrameNumber,
            EffectId = request.EffectId,
            MapTransitionEffectId = _engine.StaticVariables.g_mapTransitionEffectId,
        };
    }

    // JUSTIFICATION: backend MonoGame only
    private RuntimeSnapshotResponse CaptureSnapshot()
        => new()
        {
            FilePath = CaptureSnapshotPath(),
        };

    // JUSTIFICATION: backend MonoGame only
    private RuntimeFrameProbeSnapshot CaptureFrameProbe(string checkpoint)
        => new()
        {
            Checkpoint = checkpoint,
            Frame = _engine.StaticVariables.FrameNumber,
            IsWarpTransitionRunning = _engine.GetType()
                .GetField("_isWarpTransitionRunning", BindingFlags.Instance | BindingFlags.NonPublic)?
                .GetValue(_engine) as bool? ?? false,
            MapTransitionEffectId = _engine.StaticVariables.g_mapTransitionEffectId,
            IsGameEnding = _engine.StaticVariables.g_isGameEnding,
            FilePath = CaptureSnapshotPath(),
        };

    // JUSTIFICATION: backend MonoGame only
    private string CaptureSnapshotPath()
    {
        var saveSnapshotMethod = _gameRoot.GetType().GetMethod("SaveSnapshot", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("The game host does not expose a SaveSnapshot method.");

        var snapshotPath = saveSnapshotMethod.Invoke(_gameRoot, null) as string;
        if (string.IsNullOrWhiteSpace(snapshotPath))
        {
            throw new InvalidOperationException("SaveSnapshot did not return a snapshot path.");
        }

        return snapshotPath;
    }

    private RuntimeStatusSnapshot CreateStatusSnapshot(string checkpoint)
    {
        var player = _engine.StaticVariables.PlayerEntity;
        return new RuntimeStatusSnapshot
        {
            Checkpoint = checkpoint,
            Frame = _engine.StaticVariables.FrameNumber,
            IsPaused = _engine.StaticVariables.IsGamePaused,
            DoNextFrame = _engine.StaticVariables.DoNextFrame,
            CurrentMap = _engine.StaticVariables.g_currentMap,
            DesiredMap = _engine.StaticVariables.g_desiredMap,
            Player = player == null
                ? null
                : new RuntimePlayerSnapshot
                {
                    PosX = player.PosX,
                    PosY = player.PosY,
                    PosZ = player.PosZ,
                    Hp = player.Hp,
                    HpMax = player.HpMax,
                    CurrentAnimationId = player.CurrentAnimationId,
                    TargetAnimationId = player.TargetAnimationId,
                },
        };
    }

    // JUSTIFICATION: backend MonoGame only
    private RuntimePlayerCollisionSnapshot CapturePlayerCollisionSnapshot(string checkpoint)
    {
        var player = _engine.StaticVariables.PlayerEntity
            ?? throw new InvalidOperationException("Player entity is not available.");

        var collisionFlags = new uint[4];
        var collisionFlagsOr = global::AlundraEngine.PhysicsEngine.GetCollisionFlagsWithPlayer(player, collisionFlags, _engine);
        var tiles = new RuntimeCollisionTileSnapshot[4];

        for (var index = 0; index < 4; index++)
        {
            var tile = player.MapTiles[index];
            uint tileFlags = 0;
            byte walkability = 0;
            byte groundProperty = 0;
            byte slope = 0;
            byte height = 0;

            if (tile != null)
            {
                walkability = tile.Walkability;
                groundProperty = tile.GroundProperty;
                slope = tile.Slope;
                height = tile.Height;
                tileFlags = (uint)(walkability | (groundProperty << 8) | (slope << 16) | (height << 24));
            }

            tiles[index] = new RuntimeCollisionTileSnapshot
            {
                Index = index,
                MapHeight = player.MapHeights[index],
                Walkability = walkability,
                GroundProperty = groundProperty,
                Slope = slope,
                Height = height,
                TileFlags = tileFlags,
                CollisionFlag = collisionFlags[index],
            };
        }

        return new RuntimePlayerCollisionSnapshot
        {
            Checkpoint = checkpoint,
            Frame = _engine.StaticVariables.FrameNumber,
            PosX = player.PosX,
            PosY = player.PosY,
            PosZ = player.PosZ,
            ModdedPosZ = player.ModdedPosZ,
            TerrainHeight = player.TerrainHeight,
            EntityFlags = player.Flags,
            TileAttributes = player.TileAttributes,
            Slope18c = player.Slope_18c,
            CurrentAnimationId = player.CurrentAnimationId,
            TargetAnimationId = player.TargetAnimationId,
            FinalForceX = player.FinalForceX,
            FinalForceY = player.FinalForceY,
            ForceAdjusted = player.ForceAdjusted,
            WarpLockTimer = _engine.StaticVariables.g_warpLockTimer,
            GravityFlag = _engine.StaticVariables.g_gravityFlag,
            CollisionFlagsOr = collisionFlagsOr,
            CollisionFlags = collisionFlags.ToArray(),
            Tiles = tiles,
        };
    }

    // JUSTIFICATION: backend MonoGame only
    private RuntimePlayerXYMoveSnapshot CapturePlayerXYMoveSnapshot()
    {
        lock (_playerXYMoveLock)
        {
            return _lastPlayerXYMoveSnapshot ?? new RuntimePlayerXYMoveSnapshot
            {
                Frame = _engine.StaticVariables.FrameNumber,
                CandidateIndex = -1,
                ResultIndex = -1,
                ExitPath = "NoSnapshot",
            };
        }
    }

    private RuntimeTraceSnapshot CreateTraceSnapshot(RuntimeTraceRequest request)
    {
        var count = Math.Clamp(request.Count, 1, _maxTraceEntries);
        lock (_traceLock)
        {
            return new RuntimeTraceSnapshot
            {
                Entries = _traceEntries.TakeLast(count).ToArray(),
            };
        }
    }

    private RuntimeValueSnapshot ReadValue(RuntimePathRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
        {
            throw new InvalidOperationException("A non-empty path is required.");
        }

        var result = EvaluatePath(request.Path);
        return new RuntimeValueSnapshot
        {
            Path = request.Path,
            TypeName = GetTypeName(result.ValueType, result.Value),
            Summary = SummarizeValue(result.Value),
            ScalarValue = GetScalarValue(result.Value),
        };
    }

    private RuntimeMembersSnapshot ListMembers(RuntimePathRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Path))
        {
            return new RuntimeMembersSnapshot
            {
                Path = "<roots>",
                TypeName = "roots",
                Members = CreateRoots()
                    .Select(root => new RuntimeMemberSnapshot
                    {
                        Name = root.Key,
                        Kind = "root",
                        TypeName = GetTypeName(root.Value?.GetType(), root.Value),
                        Summary = SummarizeValue(root.Value),
                    })
                    .OrderBy(static root => root.Name, StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
            };
        }

        var result = EvaluatePath(request.Path);
        if (result.Value is null)
        {
            return new RuntimeMembersSnapshot
            {
                Path = request.Path,
                TypeName = GetTypeName(result.ValueType, null),
                Members = [],
            };
        }

        var members = new List<RuntimeMemberSnapshot>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in EnumerateFields(result.Value.GetType()))
        {
            if (field.Name.StartsWith("<", StringComparison.Ordinal))
            {
                continue;
            }

            if (!seen.Add(field.Name))
            {
                continue;
            }

            object? fieldValue;
            string summary;
            try
            {
                fieldValue = field.GetValue(result.Value);
                summary = SummarizeValue(fieldValue);
            }
            catch (Exception exception)
            {
                fieldValue = null;
                summary = $"<error: {exception.Message}>";
            }

            members.Add(new RuntimeMemberSnapshot
            {
                Name = field.Name,
                Kind = "field",
                TypeName = GetTypeName(field.FieldType, fieldValue),
                Summary = summary,
            });
        }

        foreach (var property in EnumerateProperties(result.Value.GetType()))
        {
            if (!seen.Add(property.Name))
            {
                continue;
            }

            object? propertyValue;
            string summary;
            try
            {
                propertyValue = property.GetValue(result.Value);
                summary = SummarizeValue(propertyValue);
            }
            catch (Exception exception)
            {
                propertyValue = null;
                summary = $"<error: {exception.Message}>";
            }

            members.Add(new RuntimeMemberSnapshot
            {
                Name = property.Name,
                Kind = "property",
                TypeName = GetTypeName(property.PropertyType, propertyValue),
                Summary = summary,
            });
        }

        return new RuntimeMembersSnapshot
        {
            Path = request.Path,
            TypeName = GetTypeName(result.ValueType, result.Value),
            Members = members
                .OrderBy(static member => member.Name, StringComparer.OrdinalIgnoreCase)
                .Take(128)
                .ToArray(),
        };
    }

    private EvaluationResult EvaluatePath(string path)
    {
        var tokens = Tokenize(path);
        if (tokens.Count == 0)
        {
            throw new InvalidOperationException("Path is empty.");
        }

        if (tokens[0].Kind != PathTokenKind.Name)
        {
            throw new InvalidOperationException("Path must start with a root name.");
        }

        var roots = CreateRoots();
        if (!roots.TryGetValue(tokens[0].Value, out var current))
        {
            throw new InvalidOperationException($"Unknown root '{tokens[0].Value}'. Use list_members with an empty path to discover roots.");
        }

        var currentType = current?.GetType();

        for (var index = 1; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind == PathTokenKind.Name)
            {
                var member = ResolveMember(current, token.Value);
                current = member.Value;
                currentType = member.ValueType;
                continue;
            }

            var indexed = ResolveIndex(current, token.Value);
            current = indexed.Value;
            currentType = indexed.ValueType;
        }

        return new EvaluationResult(current, currentType);
    }

    private Dictionary<string, object?> CreateRoots()
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["game"] = _gameRoot,
            ["engine"] = _engine,
            ["static"] = _engine.StaticVariables,
            ["player"] = _engine.StaticVariables.PlayerEntity,
            ["map"] = _engine.CurrentMap,
            ["renderer"] = _engine.Renderer,
        };

    // JUSTIFICATION: backend MonoGame only
    private RuntimeTraceEntry CreateTraceEntry(string checkpoint)
    {
        var bossSlotIndex = -1;
        var bossStatus = 0;
        uint bossTargetAnimationId = 0;
        var bossBytes1 = 0;
        var bossBytes2 = 0;
        var bossDelayOrAngle = 0;
        var bossAIValue1 = 0;
        var bossAIValue4 = 0;
        var matchingFollowers = -1;
        IReadOnlyList<int> bossDelays = [];

        var entitySlots = _engine.StaticVariables.g_entitySlots;
        for (var index = 0; index < entitySlots.Length; index++)
        {
            var entity = entitySlots[index];
            if (string.IsNullOrWhiteSpace(entity.Name) ||
                !entity.Name.Contains("Mille-pattes (corps principal)", StringComparison.Ordinal))
            {
                continue;
            }

            bossSlotIndex = index;
            bossStatus = entity.Status;
            bossTargetAnimationId = entity.TargetAnimationId;
            bossBytes1 = entity.Bytes[1];
            bossBytes2 = entity.Bytes[2];
            bossDelayOrAngle = entity.DelayOrAngleOrEntityId;
            bossAIValue1 = entity.AIValues[1];
            bossAIValue4 = entity.AIValues[4];

            if (index + 14 < entitySlots.Length)
            {
                var delays = new int[15];
                for (var delayIndex = 0; delayIndex < delays.Length; delayIndex++)
                {
                    delays[delayIndex] = entitySlots[index + delayIndex].DelayOrAngleOrEntityId;
                }

                matchingFollowers = 0;
                for (var delayIndex = 0; delayIndex < delays.Length - 1; delayIndex++)
                {
                    if (delays[delayIndex] == delays[delayIndex + 1])
                    {
                        matchingFollowers++;
                    }
                }

                bossDelays = delays;
            }

            break;
        }

        return new RuntimeTraceEntry
        {
            Sequence = Interlocked.Increment(ref _traceSequence),
            Checkpoint = checkpoint,
            Frame = _engine.StaticVariables.FrameNumber,
            TimestampUtc = DateTimeOffset.UtcNow,
            BossSlotIndex = bossSlotIndex,
            BossStatus = bossStatus,
            BossTargetAnimationId = bossTargetAnimationId,
            BossBytes1 = bossBytes1,
            BossBytes2 = bossBytes2,
            BossDelayOrAngle = bossDelayOrAngle,
            BossAIValue1 = bossAIValue1,
            BossAIValue4 = bossAIValue4,
            GlobalA4 = _engine.StaticVariables.DAT_801911a4,
            GlobalB4 = _engine.StaticVariables.DAT_801911b4,
            MatchingFollowers = matchingFollowers,
            BossDelays = bossDelays,
        };
    }

    private void RecordTrace(string checkpoint)
    {
        var entry = CreateTraceEntry(checkpoint);

        lock (_traceLock)
        {
            _traceEntries.Enqueue(entry);
            while (_traceEntries.Count > _maxTraceEntries)
            {
                _traceEntries.Dequeue();
            }
        }
    }

    private static T ParseArguments<T>(JsonElement arguments) where T : new()
    {
        if (arguments.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return new T();
        }

        return arguments.Deserialize<T>(RuntimeInspectorJson.Options) ?? new T();
    }

    private static IEnumerable<FieldInfo> EnumerateFields(Type type)
    {
        for (var current = type; current != null && current != typeof(object); current = current.BaseType)
        {
            foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (!field.IsStatic)
                {
                    yield return field;
                }
            }
        }
    }

    private static IEnumerable<PropertyInfo> EnumerateProperties(Type type)
    {
        for (var current = type; current != null && current != typeof(object); current = current.BaseType)
        {
            foreach (var property in current.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (property.GetIndexParameters().Length == 0 && property.GetMethod != null)
                {
                    yield return property;
                }
            }
        }
    }

    private static EvaluationResult ResolveMember(object? current, string memberName)
    {
        if (current is null)
        {
            throw new InvalidOperationException($"Cannot access member '{memberName}' on a null value.");
        }

        var type = current.GetType();
        var property = EnumerateProperties(type)
            .FirstOrDefault(candidate => string.Equals(candidate.Name, memberName, StringComparison.OrdinalIgnoreCase));
        if (property != null)
        {
            return new EvaluationResult(property.GetValue(current), property.PropertyType);
        }

        var field = EnumerateFields(type)
            .FirstOrDefault(candidate => string.Equals(candidate.Name, memberName, StringComparison.OrdinalIgnoreCase));
        if (field != null)
        {
            return new EvaluationResult(field.GetValue(current), field.FieldType);
        }

        throw new InvalidOperationException($"Member '{memberName}' was not found on type '{type.FullName}'.");
    }

    private static EvaluationResult ResolveIndex(object? current, string indexToken)
    {
        if (current is null)
        {
            throw new InvalidOperationException($"Cannot index into null with token '{indexToken}'.");
        }

        var normalizedToken = indexToken.Trim();
        if (current is Array array)
        {
            var index = ParseIndex(normalizedToken, array.Length);
            return new EvaluationResult(array.GetValue(index), array.GetType().GetElementType());
        }

        if (current is IList list)
        {
            var index = ParseIndex(normalizedToken, list.Count);
            var value = list[index];
            return new EvaluationResult(value, value?.GetType());
        }

        if (current is IDictionary dictionary)
        {
            var key = ParseDictionaryKey(normalizedToken);
            if (!dictionary.Contains(key))
            {
                throw new InvalidOperationException($"Dictionary key '{key}' was not found.");
            }

            var value = dictionary[key];
            return new EvaluationResult(value, value?.GetType());
        }

        throw new InvalidOperationException($"Type '{current.GetType().FullName}' does not support indexing.");
    }

    private static List<PathToken> Tokenize(string path)
    {
        var tokens = new List<PathToken>();

        for (var index = 0; index < path.Length;)
        {
            var current = path[index];
            if (current == '.')
            {
                index++;
                continue;
            }

            if (current == '[')
            {
                var closing = path.IndexOf(']', index + 1);
                if (closing < 0)
                {
                    throw new InvalidOperationException("Path contains an unterminated indexer.");
                }

                tokens.Add(new PathToken(PathTokenKind.Index, path[(index + 1)..closing]));
                index = closing + 1;
                continue;
            }

            var start = index;
            while (index < path.Length)
            {
                var character = path[index];
                if (character == '.' || character == '[')
                {
                    break;
                }

                index++;
            }

            tokens.Add(new PathToken(PathTokenKind.Name, path[start..index]));
        }

        return tokens;
    }

    private static int ParseIndex(string value, int count)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            throw new InvalidOperationException($"'{value}' is not a valid integer index.");
        }

        if (index < 0 || index >= count)
        {
            throw new InvalidOperationException($"Index {index} is out of bounds for count {count}.");
        }

        return index;
    }

    private static object ParseDictionaryKey(string value)
    {
        if ((value.StartsWith('"') && value.EndsWith('"')) || (value.StartsWith('\'') && value.EndsWith('\'')))
        {
            return value[1..^1];
        }

        return value;
    }

    private static string SummarizeValue(object? value)
        => value switch
        {
            null => "null",
            string text => text.Length <= 96 ? $"\"{text}\"" : $"\"{text[..93]}...\"",
            char character => $"'{character}'",
            bool boolean => boolean ? "true" : "false",
            Enum enumValue => $"{enumValue} ({Convert.ToInt64(enumValue, CultureInfo.InvariantCulture)})",
            sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal
                => Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.ToString() ?? string.Empty,
            Entity entity => $"Entity(Index={entity.Index}, Pos=({entity.PosX},{entity.PosY},{entity.PosZ}), Hp={entity.Hp}/{entity.HpMax}, Anim={entity.CurrentAnimationId})",
            GameEngine engine => $"GameEngine(Frame={engine.StaticVariables.FrameNumber}, Map={engine.StaticVariables.g_currentMap})",
            StaticVariables variables => $"StaticVariables(Frame={variables.FrameNumber}, Map={variables.g_currentMap}, Desired={variables.g_desiredMap}, Paused={variables.IsGamePaused})",
            Array array => $"{array.GetType().Name}[{array.Length}]",
            ICollection collection => $"{value.GetType().Name} (count={collection.Count})",
            _ => value.GetType().FullName ?? value.GetType().Name,
        };

    private static string? GetScalarValue(object? value)
        => value switch
        {
            null => "null",
            string text => text,
            char character => character.ToString(),
            bool boolean => boolean ? "true" : "false",
            Enum enumValue => Convert.ToInt64(enumValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal
                => Convert.ToString(value, CultureInfo.InvariantCulture),
            _ => null,
        };

    private static string GetTypeName(Type? declaredType, object? runtimeValue)
        => runtimeValue?.GetType().FullName
           ?? declaredType?.FullName
           ?? "null";

    private static int ParseInt(string variableName, int defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(variableName);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
    }

    private sealed class PendingRequest
    {
        public PendingRequest(RuntimeInspectionRequest request)
        {
            Request = request;
            Completion = new TaskCompletionSource<RuntimeInspectionResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public RuntimeInspectionRequest Request { get; }
        public TaskCompletionSource<RuntimeInspectionResponse> Completion { get; }
    }

    private readonly record struct EvaluationResult(object? Value, Type? ValueType);
    private readonly record struct PathToken(PathTokenKind Kind, string Value);

    private enum PathTokenKind
    {
        Name,
        Index,
    }
}