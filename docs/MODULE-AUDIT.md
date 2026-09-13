# Wysteria CS2 Module Audit

Every module is a separate project and has a direct ProjectReference to `../Wysteria.Core/Wysteria.Core.csproj`. Every entry point calls `WysteriaCoreGuard.Require()`.

| # | Module | Project | Config | Audience | Core dependency | Visibility |
|---:|---|---|---|---|---|---|
| 1 | PrisonJob | `src/Wysteria.PrisonJob/` | `Configs/Wysteria.PrisonJob.json` | T | required | LOCAL/CENTER |
| 2 | Contraband | `src/Wysteria.Contraband/` | `Configs/Wysteria.Contraband.json` | T | required | LOCAL/CENTER |
| 3 | WardenShield | `src/Wysteria.WardenShield/` | `Configs/Wysteria.WardenShield.json` | Warden | required | PUBLIC where appropriate |
| 4 | FakeRebel | `src/Wysteria.FakeRebel/` | `Configs/Wysteria.FakeRebel.json` | T | required | LOCAL/CENTER |
| 5 | GuardStamina | `src/Wysteria.GuardStamina/` | `Configs/Wysteria.GuardStamina.json` | CT | required | LOCAL/CENTER |
| 6 | WardenDraw | `src/Wysteria.WardenDraw/` | `Configs/Wysteria.WardenDraw.json` | Warden | required | LOCAL/CENTER |
| 7 | GuardJail | `src/Wysteria.GuardJail/` | `Configs/Wysteria.GuardJail.json` | Warden | required | PUBLIC where appropriate |
| 8 | PrisonBetrayal | `src/Wysteria.PrisonBetrayal/` | `Configs/Wysteria.PrisonBetrayal.json` | T | required | LOCAL/CENTER |
| 9 | WardenPanic | `src/Wysteria.WardenPanic/` | `Configs/Wysteria.WardenPanic.json` | Warden | required | PUBLIC where appropriate |
| 10 | Advertiser | `src/Wysteria.Advertiser/` | `Configs/Wysteria.Advertiser.json` | Everyone | required | PUBLIC where appropriate |
| 11 | CellSearch | `src/Wysteria.CellSearch/` | `Configs/Wysteria.CellSearch.json` | CT | required | LOCAL/CENTER |
| 12 | WardenVote | `src/Wysteria.WardenVote/` | `Configs/Wysteria.WardenVote.json` | Everyone | required | PUBLIC where appropriate |
| 14 | GuardOrders | `src/Wysteria.GuardOrders/` | `Configs/Wysteria.GuardOrders.json` | Warden | required | PUBLIC where appropriate |
| 15 | RiotAlarm | `src/Wysteria.RiotAlarm/` | `Configs/Wysteria.RiotAlarm.json` | CT | required | LOCAL/CENTER |
| 16 | DoorControl | `src/Wysteria.DoorControl/` | `Configs/Wysteria.DoorControl.json` | Warden | required | PUBLIC where appropriate |
| 18 | InmateRanks | `src/Wysteria.InmateRanks/` | `Configs/Wysteria.InmateRanks.json` | Everyone | required | LOCAL/CENTER |
| 19 | ContrabandScanner | `src/Wysteria.ContrabandScanner/` | `Configs/Wysteria.ContrabandScanner.json` | CT | required | LOCAL/CENTER |
| 20 | FreeDay | `src/Wysteria.FreeDay/` | `Configs/Wysteria.FreeDay.json` | Warden | required | PUBLIC where appropriate |
| 21 | GuardTraining | `src/Wysteria.GuardTraining/` | `Configs/Wysteria.GuardTraining.json` | CT | required | LOCAL/CENTER |
| 22 | TeamBalance | `src/Wysteria.TeamBalance/` | `Configs/Wysteria.TeamBalance.json` | Warden | required | LOCAL/CENTER |
| 23 | RoundStats | `src/Wysteria.RoundStats/` | `Configs/Wysteria.RoundStats.json` | Everyone | required | LOCAL/CENTER |
| 24 | WardenRules | `src/Wysteria.WardenRules/` | `Configs/Wysteria.WardenRules.json` | Warden | required | LOCAL/CENTER |
| 25 | AdminTools | `src/Wysteria.AdminTools/` | `Configs/Wysteria.AdminTools.json` | Admin | required | LOCAL/CENTER |

## Open verification

The repository can be compiled on a machine with the .NET 8 SDK. Live validation still requires the target CS2 server and its real JailShop/CellSearch/WardenManager contracts.
