using System;
using CoreKeeperWar;

var sp = new[] { new Unit { X = 0, Y = 0, Faction = Faction.Monster }, new Unit { X = 50, Y = 0, Faction = Faction.Player } };
void Check(bool ok, string m) { if (!ok) throw new Exception("FAIL " + m); }

Check(War.Nearest(40, 0, sp, Faction.Player) == 1, "nearest player-owned far away is still found");
Check(War.Nearest(40, 0, sp, Faction.Player) != War.Nearest(40, 0, sp, Faction.Monster), "factions separate");
Check(War.Nearest(0, 0, new Unit[0], Faction.Player) == -1, "empty pool");
Check(War.FactionOfNewCreature(51, 1, sp) == Faction.Player, "spawn near player spawner is charmed");
Check(War.FactionOfNewCreature(1, 1, sp) == Faction.Monster, "spawn near monster spawner stays hostile");
Check(War.FactionOfNewCreature(25, 0, sp) == Faction.Monster, "spawn out of range defaults to monster");
Check(War.SpawnerToConvert(new Unit { X = 49, Y = 0, Faction = Faction.Monster }, sp) == 1, "monster converts player spawner");
Check(War.SpawnerToConvert(new Unit { X = 40, Y = 0, Faction = Faction.Monster }, sp) == -1, "too far to convert");
Check(War.SpawnerToConvert(new Unit { X = 49, Y = 0, Faction = Faction.Player }, sp) == -1, "own spawner not converted");
Check(War.SpawnerToConvert(new Unit { X = 1, Y = 0, Faction = Faction.Player }, sp) == 0, "ally converts monster spawner");
var big = new[] { new Unit { X = 0, Y = 0, Faction = Faction.Player, R = 12 } };
Check(War.FactionOfNewCreature(10, 0, big) == Faction.Player, "per-spawner radius R covers wide spawn distance");
Check(War.FactionOfNewCreature(10, 0, new[] { new Unit { X = 0, Y = 0, Faction = Faction.Player } }) == Faction.Monster, "default radius too small without R");
Console.WriteLine("PASS: 全11件");
