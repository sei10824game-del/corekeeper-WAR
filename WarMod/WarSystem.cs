using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace CoreKeeperWar.Mod
{
    /// Server-only. Creatures chase the nearest enemy at any distance; creatures near an enemy spawner convert it;
    /// creatures first seen near a spawner inherit its faction. Auto-registered by the SDK (no IMod needed).
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class WarSystem : PugSimulationSystemBase
    {
        EntityQuery _creatures, _spawners, _players;
        readonly List<Unit> _cu = new List<Unit>(), _su = new List<Unit>(), _all = new List<Unit>();
        readonly List<Entity> _allEnt = new List<Entity>();
        double _next;

        protected override void OnCreate()
        {
            _creatures = GetEntityQuery(ComponentType.ReadOnly<EnemyCD>(), ComponentType.ReadOnly<LocalTransform>(), ComponentType.ReadWrite<FactionCD>());
            _spawners = GetEntityQuery(ComponentType.ReadOnly<SpawnerCD>(), ComponentType.ReadOnly<LocalTransform>());
            _players = GetEntityQuery(ComponentType.ReadOnly<PlayerGhost>(), ComponentType.ReadOnly<LocalTransform>());
            base.OnCreate();
            UnityEngine.Debug.Log("[WAR] system created in " + World.Name);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (SystemAPI.Time.ElapsedTime < _next) return;
            _next = SystemAPI.Time.ElapsedTime + 0.5; // ponytail: 2 Hz full rescan O(n^2); spatial hash if >1000 units

            var em = EntityManager;
            var sEnts = _spawners.ToEntityArray(Allocator.Temp);
            _su.Clear();
            foreach (var s in sEnts)
            {
                if (!em.HasComponent<WarFactionCD>(s)) em.AddComponentData(s, new WarFactionCD { faction = Faction.Monster });
                var p = em.GetComponentData<LocalTransform>(s).Position;
                _su.Add(new Unit { X = p.x, Y = p.y, Faction = em.GetComponentData<WarFactionCD>(s).faction });
            }

            var cEnts = _creatures.ToEntityArray(Allocator.Temp);
            _cu.Clear();
            foreach (var c in cEnts)
            {
                var p = em.GetComponentData<LocalTransform>(c).Position;
                if (!em.HasComponent<WarFactionCD>(c)) // first sight = fresh spawn
                {
                    var f = War.FactionOfNewCreature(p.x, p.y, _su);
                    em.AddComponentData(c, new WarFactionCD { faction = f });
                    if (f == Faction.Player) GameBindings.MakeAlly(em, c);
                }
                _cu.Add(new Unit { X = p.x, Y = p.y, Faction = em.GetComponentData<WarFactionCD>(c).faction });
            }

            // target pool: creatures + spawners + players (players are Player side)
            _all.Clear(); _allEnt.Clear();
            for (int i = 0; i < cEnts.Length; i++) { _all.Add(_cu[i]); _allEnt.Add(cEnts[i]); }
            for (int i = 0; i < sEnts.Length; i++) { _all.Add(_su[i]); _allEnt.Add(sEnts[i]); }
            foreach (var pl in _players.ToEntityArray(Allocator.Temp))
            {
                var p = em.GetComponentData<LocalTransform>(pl).Position;
                _all.Add(new Unit { X = p.x, Y = p.y, Faction = Faction.Player }); _allEnt.Add(pl);
            }

            int set = 0, conv = 0;
            for (int i = 0; i < cEnts.Length; i++)
            {
                var u = _cu[i];
                int n = War.Nearest(u.X, u.Y, _all, War.Opposite(u.Faction));
                if (n >= 0) { GameBindings.SetTarget(em, cEnts[i], _allEnt[n]); set++; }
                int s = War.SpawnerToConvert(u, _su);
                if (s >= 0)
                {
                    conv++;
                    em.SetComponentData(sEnts[s], new WarFactionCD { faction = u.Faction });
                    _su[s] = new Unit { X = _su[s].X, Y = _su[s].Y, Faction = u.Faction };
                }
            }
            UnityEngine.Debug.Log($"[WAR] creatures={cEnts.Length} spawners={sEnts.Length} players={_players.CalculateEntityCount()} targeted={set} converted={conv}");
        }
    }
}
