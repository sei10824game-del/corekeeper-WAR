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

        public const int SearchRadius = 40; // ponytail: BFS radius cap (tiles). Targets beyond it get no path; raise if perf allows.

        /// Far-chase trick (from decompiled ChaseStateRequest): the entity in PathFindCD.targetEntity bypasses the chaseAtDistanceSq gate
        /// and, once a path exists, the line-of-sight check. The game clears it every tick while not chasing, so call this EVERY frame
        /// after StateRequestGroup and before PathFindSystem.
        public static void SetTarget(EntityManager em, Entity creature, Entity target)
        {
            if (!em.HasComponent<ChaseStateCD>(creature)) return;
            var c = em.GetComponentData<ChaseStateCD>(creature);
            var pfe = c.pathFindEntity;
            if (pfe == Entity.Null || !em.HasComponent<PathFindCD>(pfe)) return;
            var pf = em.GetComponentData<PathFindCD>(pfe);
            pf.targetEntity = target;
            pf.searchRadius = new Unity.Mathematics.int2(SearchRadius, SearchRadius);
            em.SetComponentData(pfe, pf);
            c.neverTimeoutChasing = true;
            c.shouldEnterStateCheckTimer = 0f;                    // re-evaluate entering Chase right away
            if (c.targetEntity != Entity.Null) c.targetEntity = target; // already chasing something else: switch
            em.SetComponentData(creature, c);
        }

        public static bool IsChasing(EntityManager em, Entity e) => em.HasComponent<StateInfoCD>(e) && em.GetComponentData<StateInfoCD>(e).IsCurrentState(StateID.Chase);

        /// Ally = player faction: FactionCD.CanAttack already returns false vs Player, and monsters target it.
        public static void MakeAlly(EntityManager em, Entity e) { var f = em.GetComponentData<FactionCD>(e); f.faction = FactionID.Player; em.SetComponentData(e, f); }
        public static void MakeHostile(EntityManager em, Entity e) { var f = em.GetComponentData<FactionCD>(e); f.faction = f.originalFaction; em.SetComponentData(e, f); }
    }
}
