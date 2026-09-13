# Wysteria CS2 Jailbreak - Permission Audit

## Root (Z Flag / `@css/root`) Bypass
The absolute most critical aspect of the Wysteria architectural rule is that the "Z" root admin must *always* bypass standard permission restrictions.

- Root checking strategy: `AdminManager.PlayerHasPermissions(player, "@css/root")`
- Fallback: Admin-specific commands default to `@css/generic`, `@css/ban`, or module-specific custom permissions via Config (e.g., `Config.Permission`).

## Warden Validation
Most gameplay modules strictly depend on `Wysteria.Core`'s `WardenService`.
`WardenService.IsWarden(player)` is utilized before executing any Warden-specific command (e.g., Lazer, MicCheck, FakeRebel, GuardJail).

## Team Validation
Commands strictly validate `CsTeam.Terrorist` and `CsTeam.CounterTerrorist`.

All modules are audited against this standard, and custom menus implement double-validation (hiding the button AND verifying the callback).
