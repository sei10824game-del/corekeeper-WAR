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
    [UpdateAfter(typeof(StateRequestGroup))]   // must run after the game clears PathFindCD.targetEntity...
    [UpdateBefore(typeof(PathFindSystem))]     // ...and before it computes the path
    public partial class WarSystem : PugSimulationSystemBase
    {
        EntityQuery _creatures, _spawners, _players;
        readonly List<Unit> _cu = new List<Unit>(), _su = new List<Unit>(), _all = new List<Unit>();
        readonly List<Entity> _allEnt = new List<Entity>();
        double _next;
        bool _scanned;
        const float PlaceRange = 8f;
        readonly Dictionary<Entity, Entity> _targets = new Dictionary<Entity, Entity>();

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
            var em0 = EntityManager;
            foreach (var kv in _targets) if (em0.Exists(kv.Key) && em0.Exists(kv.Value)) GameBindings.SetTarget(em0, kv.Key, kv.Value);
            if (SystemAPI.Time.ElapsedTime < _next) return;
            _next = SystemAPI.Time.ElapsedTime + 0.5; // ponytail: 2 Hz full rescan O(n^2); spatial hash if >1000 units
            _targets.Clear();

            var em = EntityManager;
            // players first: a spawner that appears after the first scan right next to a player was placed by them
            var plEnts = _players.ToEntityArray(Allocator.Temp);
            var plPos = new List<Unity.Mathematics.float3>();
            foreach (var pl in plEnts) plPos.Add(em.GetComponentData<LocalTransform>(pl).Position);

            var sEnts = _spawners.ToEntityArray(Allocator.Temp);
            _su.Clear();
            foreach (var s in sEnts)
            {
                var p = em.GetComponentData<LocalTransform>(s).Position;
                if (!em.HasComponent<WarFactionCD>(s))
                {
                    var f = Faction.Monster;
                    if (_scanned) foreach (var pp in plPos) if (Unity.Mathematics.math.distancesq(pp, p) <= PlaceRange * PlaceRange) f = Faction.Player; // ponytail: heuristic; use a placed-by-player component if one exists
                    em.AddComponentData(s, new WarFactionCD { faction = f });
                    UnityEngine.Debug.Log($"[WAR] new spawner obj={(em.HasComponent<ObjectDataCD>(s) ? em.GetComponentData<ObjectDataCD>(s).objectID.ToString() : "?")} owner={f} first={!_scanned}");
                }
                var sp = em.GetComponentData<SpawnerCD>(s);
                _su.Add(new Unit { X = p.x, Y = p.y, Faction = em.GetComponentData<WarFactionCD>(s).faction, R = sp.maxSpawnDistance + 2f });
            }
            _scanned = true;

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
            foreach (var pl in plEnts)
            {
                var p = em.GetComponentData<LocalTransform>(pl).Position;
                _all.Add(new Unit { X = p.x, Y = p.y, Faction = Faction.Player }); _allEnt.Add(pl);
            }

            int set = 0, conv = 0, chasing = 0;
            for (int i = 0; i < cEnts.Length; i++)
            {
                var u = _cu[i];
                int n = War.Nearest(u.X, u.Y, _all, War.Opposite(u.Faction));
                if (n >= 0) { _targets[cEnts[i]] = _allEnt[n]; set++; if (GameBindings.IsChasing(em, cEnts[i])) chasing++; }
                int s = War.SpawnerToConvert(u, _su);
                if (s >= 0)
                {
                    conv++;
                    em.SetComponentData(sEnts[s], new WarFactionCD { faction = u.Faction });
                    _su[s] = new Unit { X = _su[s].X, Y = _su[s].Y, Faction = u.Faction };
                }
            }
            UnityEngine.Debug.Log($"[WAR] creatures={cEnts.Length} spawners={sEnts.Length} players={_players.CalculateEntityCount()} targeted={set} chasing={chasing} converted={conv}");
        }
    }
}
