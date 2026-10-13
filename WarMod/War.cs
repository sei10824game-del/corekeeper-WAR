using System.Collections.Generic;

namespace CoreKeeperWar
{
    public enum Faction : byte { Monster = 0, Player = 1 }

    public struct Unit { public float X, Y; public Faction Faction; public float R; } // R: extra spawn radius (0 = default)

    public static class War
    {
        public const float ConvertRange = 2.5f;      // tiles; creature this close to an enemy spawner converts it
        public const float SpawnOwnerRange = 6f;     // new creature adopts faction of a spawner within this range

        public static Faction Opposite(Faction f) => f == Faction.Player ? Faction.Monster : Faction.Player;

        static float D2(float ax, float ay, float bx, float by) { var dx = ax - bx; var dy = ay - by; return dx * dx + dy * dy; }

        /// Index of nearest unit of faction `target` (any distance), or -1. O(n) per call; fine for hundreds of units.
        public static int Nearest(float x, float y, IReadOnlyList<Unit> pool, Faction target)
        {
            int best = -1; float bd = float.MaxValue;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i].Faction != target) continue;
                var d = D2(x, y, pool[i].X, pool[i].Y);
                if (d < bd) { bd = d; best = i; }
            }
            return best;
        }

        /// Faction a freshly spawned creature should take: owner of nearest spawner in range, else Monster (vanilla).
        public static Faction FactionOfNewCreature(float x, float y, IReadOnlyList<Unit> spawners)
        {
            int best = -1; float bd = float.MaxValue;
            for (int i = 0; i < spawners.Count; i++)
            {
                var r = System.Math.Max(SpawnOwnerRange, spawners[i].R);
                var d = D2(x, y, spawners[i].X, spawners[i].Y);
                if (d <= r * r && d < bd) { bd = d; best = i; }
            }
            return best < 0 ? Faction.Monster : spawners[best].Faction;
        }

        /// Index of an enemy-owned spawner within ConvertRange of the creature, or -1.
        public static int SpawnerToConvert(Unit creature, IReadOnlyList<Unit> spawners)
        {
            var i = Nearest(creature.X, creature.Y, spawners, Opposite(creature.Faction));
            return i >= 0 && D2(creature.X, creature.Y, spawners[i].X, spawners[i].Y) <= ConvertRange * ConvertRange ? i : -1;
        }
    }
}
