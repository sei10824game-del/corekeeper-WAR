using CoreLib; // VERIFY: IMod namespace (see CoreLib/CoreLibMod.cs)
using Unity.Entities;

namespace CoreKeeperWar.Mod
{
    public class WarMod : IMod
    {
        public const string Version = "0.1.0";
        public const string Name = "Core Keeper WAR";
        public void EarlyInit() { }
        public void Init() { }          // VERIFY: lifecycle method names match IMod in your SDK
        public void Shutdown() { }
        public void ModObjectLoaded(UnityEngine.Object obj) { }
        public void Update() { }
        // register server system as CoreLib does: world.GetOrCreateSystemManaged<WarSystem>() + add to a SimulationSystemGroup
    }
}
