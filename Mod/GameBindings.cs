// Game-specific names, verified by decompiling the user's Pug.ECS.Components.dll (SDK 1.3.0.5 / game 2026-10).
// Items marked VERIFY are semantics still to confirm in-game.
using Unity.Entities;
using Unity.Transforms;

namespace CoreKeeperWar.Mod
{
    /// Our own marker on every creature/spawner we manage.
    public struct WarFactionCD : IComponentData { public Faction faction; }

    public static class GameBindings
    {
        public static EntityQuery CreatureQuery(ref SystemState s) => s.GetEntityQuery(ComponentType.ReadOnly<EnemyCD>(), ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadWrite<FactionCD>());
        public static EntityQuery SpawnerQuery(ref SystemState s) => s.GetEntityQuery(ComponentType.ReadOnly<SpawnerCD>(), ComponentType.ReadOnly<LocalTransform>());

        /// Chase target any distance away. ChaseStateCD.targetEntity = current target; neverTimeoutChasing stops it giving up.
        /// VERIFY: the state machine must be in the chase state (internalState) and the game's own target-select system may overwrite targetEntity each tick.
        public static void SetTarget(EntityManager em, Entity creature, Entity target)
        {
            if (!em.HasComponent<ChaseStateCD>(creature)) return;
            var c = em.GetComponentData<ChaseStateCD>(creature);
            c.targetEntity = target; c.initialTargetEntity = target; c.neverTimeoutChasing = true;
            em.SetComponentData(creature, c);
        }

        /// Ally = player faction: FactionCD.CanAttack already returns false vs Player, and monsters target it.
        public static void MakeAlly(EntityManager em, Entity e) { var f = em.GetComponentData<FactionCD>(e); f.faction = FactionID.Player; em.SetComponentData(e, f); }
        public static void MakeHostile(EntityManager em, Entity e) { var f = em.GetComponentData<FactionCD>(e); f.faction = f.originalFaction; em.SetComponentData(e, f); }
    }
}
