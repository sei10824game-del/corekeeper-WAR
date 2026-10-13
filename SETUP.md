# Setup (official Mod SDK, verified from Pugstorm/CoreKeeperModSDK 1.3.0.5)
1. Install Unity Hub; download the SDK (Code > Download ZIP); Hub > Add project from disk. Editor 6000.0.58f2, enable "Linux Build Support (Mono)". Add arg `-disable-assembly-updater`.
2. Follow https://mod.io/g/corekeeper/r/core-keeper-mod-sdk-introduction up to "Creating new objects" (this copies the game DLLs from your Steam install into the project).
3. Copy the whole `WarMod/` folder (asmdef + asset + .cs) into the SDK project's `Assets/`. Scripts must live in a folder with the asmdef; loose scripts in Assets fail with CS0246.
4. Dump real names (run in SDK project root, git-bash/Linux/Mac) and paste the output to Claude:
   `grep -ao "[A-Za-z]*\(Spawner\|Charm\|Enemy\|Target\|Minion\|Owner\)[A-Za-z]*" Assets/Plugins/CoreKeeper/Pug.ECS.Components.dll | sort -u`
