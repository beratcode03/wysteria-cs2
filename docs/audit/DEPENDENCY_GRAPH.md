# Wysteria CS2 Jailbreak - Dependency Graph

## The Core
**Wysteria.Core** -> `CounterStrikeSharp.API`

Every single module relies strictly on `Wysteria.Core`. Circular dependencies are explicitly banned.
```text
Wysteria.AdminTools        -> Wysteria.Core
Wysteria.Advertiser        -> Wysteria.Core
Wysteria.BountySystem      -> Wysteria.Core
Wysteria.CellSearch        -> Wysteria.Core
Wysteria.CoinFlip          -> Wysteria.Core
Wysteria.Contraband        -> Wysteria.Core
Wysteria.ContrabandScanner -> Wysteria.Core
Wysteria.Disguise          -> Wysteria.Core
Wysteria.DoorControl       -> Wysteria.Core
Wysteria.FakeRebel         -> Wysteria.Core
Wysteria.FreeDay           -> Wysteria.Core
Wysteria.GuardJail         -> Wysteria.Core
Wysteria.GuardOrders       -> Wysteria.Core
Wysteria.GuardRadio        -> Wysteria.Core
Wysteria.GuardStamina      -> Wysteria.Core
Wysteria.GuardTraining     -> Wysteria.Core
Wysteria.HostageGuard      -> Wysteria.Core
Wysteria.InmateRanks       -> Wysteria.Core
Wysteria.MetalDetector     -> Wysteria.Core
Wysteria.PrisonBetrayal    -> Wysteria.Core
Wysteria.PrisonGym         -> Wysteria.Core
Wysteria.PrisonJob         -> Wysteria.Core
Wysteria.PrisonLockdown    -> Wysteria.Core
Wysteria.RebelAdrenaline   -> Wysteria.Core
Wysteria.RiotAlarm         -> Wysteria.Core
Wysteria.RoundStats        -> Wysteria.Core
Wysteria.SecretPassage     -> Wysteria.Core
Wysteria.TeamBalance       -> Wysteria.Core
Wysteria.VoiceControl      -> Wysteria.Core
Wysteria.WardenAnvil       -> Wysteria.Core
Wysteria.WardenDraw        -> Wysteria.Core
Wysteria.WardenLazer       -> Wysteria.Core
Wysteria.WardenMarker      -> Wysteria.Core
Wysteria.WardenMicCheck    -> Wysteria.Core
Wysteria.WardenPanic       -> Wysteria.Core
Wysteria.WardenRules       -> Wysteria.Core
Wysteria.WardenShield      -> Wysteria.Core
Wysteria.WardenVote        -> Wysteria.Core
```
