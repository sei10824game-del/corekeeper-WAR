// ALL game-specific names live here. Names marked VERIFY are unconfirmed (no game DLLs were available
// when written): check with ilspycmd/dnSpy against Pug.ECS.Components.dll, fix here only (see .claude/agents/ck-modder.md).
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace CoreKeeperWar.Mod
{
    /// Our own marker added to every creature/spawner we manage.
    public struct WarFactionCD : IComponentData { public Faction faction; }

    public static class GameBindings
    {
        // VERIFY: components identifying a hostile creature and a spawner entity in the live game.
        public static EntityQueryDesc Creatures => new EntityQueryDesc { All = new[] { ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadOnly<EnemyCD>() } };       // VERIFY EnemyCD
        public static EntityQueryDesc Spawners  => new EntityQueryDesc { All = new[] { ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadOnly<SpawnerCD>() } };      // VERIFY SpawnerCD

        // VERIFY: how the game aims an enemy. Expected: write a target position / entity the move system already follows.
        public static void SetTarget(EntityManager em, Entity creature, float2 pos) { /* em.SetComponentData(creature, new TargetPositionCD{ value = pos }); */ }

        // VERIFY: charm/ally status so the creature stops attacking players (e.g. a Charmed / Owner component).
        public static void MakeAlly(EntityManager em, Entity creature, Entity owner) { /* em.AddComponentData(creature, new CharmedCD{ owner = owner }); */ }
        public static void MakeHostile(EntityManager em, Entity creature) { /* em.RemoveComponent<CharmedCD>(creature); */ }

        // VERIFY: swapping spawner ownership. Simplest: keep the entity, only flip WarFactionCD (new creatures read it).
        public static void ConvertSpawner(EntityManager em, Entity spawner, Faction to) => em.SetComponentData(spawner, new WarFactionCD { faction = to });
    }
}
