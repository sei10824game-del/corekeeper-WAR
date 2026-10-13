# Setup (official Mod SDK, verified from Pugstorm/CoreKeeperModSDK 1.3.0.5)
1. Install Unity Hub; download the SDK (Code > Download ZIP); Hub > Add project from disk. Editor 6000.0.58f2, enable "Linux Build Support (Mono)". Add arg `-disable-assembly-updater`.
2. Follow https://mod.io/g/corekeeper/r/core-keeper-mod-sdk-introduction up to "Creating new objects" (this copies the game DLLs from your Steam install into the project).
3. In the project: Create a mod folder, copy `WarCore/War.cs` and `Mod/*.cs` into it. Systems need no registration: the SDK picks up any SystemBase in the mod (see SDK Examples.zip > SystemExample.cs).
4. Dump real names (run in SDK project root, git-bash/Linux/Mac) and paste the output to Claude:
   `grep -ao "[A-Za-z]*\(Spawner\|Charm\|Enemy\|Target\|Minion\|Owner\)[A-Za-z]*" Assets/Plugins/CoreKeeper/Pug.ECS.Components.dll | sort -u`
