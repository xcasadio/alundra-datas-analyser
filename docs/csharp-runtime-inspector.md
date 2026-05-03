# C# Runtime Inspector

This bridge exposes the running C# game through a local named pipe so it can be inspected side by side with the original game.

## What it exposes

- Main-thread status snapshot: frame, map ids, pause state, player core state.
- Reflection-based value reads from runtime roots: `game`, `engine`, `static`, `player`, `map`, `renderer`.
- Member listing for any inspected object.
- Recent execution checkpoints emitted from the main game loop.
- Main-thread stack trace captured at the next checkpoint.
- Compact MCP helper tools for common comparisons: `alundra_get_player_core`, `alundra_get_map_core`, `alundra_get_entity_core`, `alundra_get_script_state`, `alundra_get_trace_compact`.
- Reverse-oriented compact helpers: `alundra_get_animation_core`, `alundra_get_entity_flags`, `alundra_get_map_runtime_state`.

## Runtime behavior

- In `DEBUG`, the inspector starts automatically when the game launches.
- In `RELEASE`, set `ALUNDRA_RUNTIME_INSPECTOR=1` to enable it.
- Pipe name defaults to `alundra-csharp-runtime` and can be overridden with `ALUNDRA_RUNTIME_PIPE`.

## MCP entry

VS Code should use the `alundra-csharp-runtime` MCP server entry from `.vscode/mcp.json`.