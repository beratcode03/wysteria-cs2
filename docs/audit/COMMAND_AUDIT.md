# Wysteria CS2 Jailbreak - Command Audit

All commands successfully identified in the repository.
Refactoring ensures compatibility by creating `wysteria_` or `css_wysteria_` equivalents while preserving the legacy ones as aliases.

## Audited Plugins and Commands

- **Wysteria.AdminTools**: `css_wysteria`
- **Wysteria.Disguise**: `css_disguise`
- **Wysteria.WardenVote**: `css_wardenvote`
- **Wysteria.SecretPassage**: `css_ventstatus`
- **Wysteria.Core**: `css_wysteria_w`, `css_wysteria_opencells`
- **Wysteria.WardenMarker**: `css_isaret`
- **Wysteria.WardenLazer**: `wysteria_lazer`, `wysteria_lazer_clear`, `wysteria_lazer_menu`
- **Wysteria.RoundStats**: `css_stats`
- **Wysteria.WardenDraw**: `css_draw`
- **Wysteria.Contraband**: `css_stash`
- **Wysteria.ContrabandScanner**: `css_scan`
- **Wysteria.InmateRanks**: `css_rank`
- **Wysteria.GuardOrders**: `css_order`
- **Wysteria.CellSearch**: `css_cellsearch`
- **Wysteria.WardenShield**: `css_shield`
- **Wysteria.VoiceControl**: `wysteria_mutets`, `wysteria_unmutets`, `wysteria_voice_menu`
- **Wysteria.GuardRadio**: `css_radio`
- **Wysteria.GuardStamina**: `css_stamina`
- **Wysteria.RiotAlarm**: `css_riot`
- **Wysteria.TeamBalance**: `css_balance`
- **Wysteria.GuardJail**: `css_punish`
- **Wysteria.WardenPanic**: `css_panic`
- **Wysteria.FakeRebel**: `css_fakerebel`
- **Wysteria.FreeDay**: `css_freeday`
- **Wysteria.WardenRules**: `css_rules`
- **Wysteria.GuardTraining**: `css_train`
- **Wysteria.WardenMicCheck**: `wysteria_miccheck`, `wysteria_miccheck_pass`, `wysteria_miccheck_menu`
- **Wysteria.PrisonJob**: `css_job`
- **Wysteria.DoorControl**: `css_doors`
- **Wysteria.PrisonBetrayal**: `css_snitch`
- **Wysteria.CoinFlip**: `css_coinflip`, `css_cfaccept`

_Note: All commands correctly implement root (Z-flag) bypass checks where administrative logic is required. Warden-specific commands validate `WardenService.IsWarden(player)`._
