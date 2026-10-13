# Core Keeper WAR (コアキーパー戦争MOD)
Rules: every creature targets its nearest enemy at any distance; creatures near an enemy spawner convert it; creatures from player-placed spawners are charmed allies.
- `WarCore/` rules, pure C#, tested: `dotnet run --project WarCore.Tests` → PASS 全9件
- `WarMod/` Unity mod folder (copy into SDK Assets/) for the official Mod SDK (mod.io). **Not compiled or run in-game** (needs your SDK + game). Component names verified against the real game DLLs; remaining `VERIFY` items in `WarMod/GameBindings.cs` need in-game testing.
- `.claude/agents/ck-modder.md` Opus subagent that fixes bindings against your SDK DLLs.
Setup: see SETUP.md
