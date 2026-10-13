using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.NetCode;

namespace CoreKeeperWar.Mod
{
    /// Server-only. Creature AI: aim at nearest enemy at any distance; convert enemy spawners; tag new spawns.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class WarSystem : PugSimulationSystemBase
    {
        EntityQuery _creatures, _spawners;
        readonly List<Unit> _cu = new List<Unit>(), _su = new List<Unit>(), _all = new List<Unit>();
        float _next;

        protected override void OnCreate()
        {
            _creatures = GetEntityQuery(GameBindings.Creatures);
            _spawners = GetEntityQuery(GameBindings.Spawners);
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            if (SystemAPI.Time.ElapsedTime < _next) return;
            _next = (float)SystemAPI.Time.ElapsedTime + 0.5f; // ponytail: 2 Hz full rescan, spatial hash if >1000 units

            var em = EntityManager;
            var spawnerEnts = _spawners.ToEntityArray(Allocator.Temp);
            _su.Clear();
            foreach (var s in spawnerEnts)
            {
                if (!em.HasComponent<WarFactionCD>(s)) em.AddComponentData(s, new WarFactionCD { faction = Faction.Monster });
                var p = em.GetComponentData<LocalTransform>(s).Position;
                _su.Add(new Unit { X = p.x, Y = p.y, Faction = em.GetComponentData<WarFactionCD>(s).faction });
            }

            var ents = _creatures.ToEntityArray(Allocator.Temp);
            _cu.Clear();
            foreach (var c in ents)
            {
                var p = em.GetComponentData<LocalTransform>(c).Position;
                if (!em.HasComponent<WarFactionCD>(c)) // first sight = fresh spawn: inherit nearest spawner's faction
                {
                    var f = War.FactionOfNewCreature(p.x, p.y, _su);
                    em.AddComponentData(c, new WarFactionCD { faction = f });
                    if (f == Faction.Player) GameBindings.MakeAlly(em, c, Entity.Null); // VERIFY owner
                }
                _cu.Add(new Unit { X = p.x, Y = p.y, Faction = em.GetComponentData<WarFactionCD>(c).faction });
            }

            // targets = every creature + spawner of the other side
            _all.Clear(); _all.AddRange(_cu); _all.AddRange(_su);
            for (int i = 0; i < ents.Length; i++)
            {
                var u = _cu[i];
                int n = War.Nearest(u.X, u.Y, _all, War.Opposite(u.Faction));
                if (n >= 0) GameBindings.SetTarget(em, ents[i], new Unity.Mathematics.float2(_all[n].X, _all[n].Y));
                int s = War.SpawnerToConvert(u, _su);
                if (s >= 0) { GameBindings.ConvertSpawner(em, spawnerEnts[s], u.Faction); _su[s] = new Unit { X = _su[s].X, Y = _su[s].Y, Faction = u.Faction }; }
            }
            base.OnUpdate();
            // ponytail: player entities aren't in the pool yet, so monsters only chase creatures/spawners; add players via a Player component query.
        }
    }
}
