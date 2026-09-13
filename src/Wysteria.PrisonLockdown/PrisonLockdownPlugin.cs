/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.PrisonLockdown;

public sealed class PrisonLockdownPlugin : WysteriaPlugin<PrisonLockdownConfig>
{
    static PrisonLockdownPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.PrisonLockdown";
    public override string ModuleVersion => "1.1.7";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Emergency prison lockdown.";

    private bool _lockdownActive;
    private bool _usedThisRound;
    private CounterStrikeSharp.API.Modules.Timers.Timer? _lockdownTimer;
    private readonly object _stateLock = new();

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.PrisonLockdown.json");

        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);

        AddCommand("css_lockdown", "Trigger sector lockdown (Active Warden only, once per round)", OnLockdownCommand);
        AddCommand("css_wysteria_lockdown", "Trigger sector lockdown (Active Warden only, once per round)", OnLockdownCommand);
    }

    public override void Unload(bool hotReload)
    {
        ClearTimer();
        base.Unload(hotReload);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        lock (_stateLock)
        {
            _lockdownActive = false;
            _usedThisRound = false;
        }
        ClearTimer();
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        lock (_stateLock)
        {
            _lockdownActive = false;
        }
        ClearTimer();
        return HookResult.Continue;
    }

    private void ClearTimer()
    {
        lock (_stateLock)
        {
            _lockdownTimer?.Kill();
            _lockdownTimer = null;
        }
    }

    private void OnLockdownCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller == null || !caller.IsValid || !caller.PawnIsAlive) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        
        if (!IsGuard(caller))
        {
            WysteriaMessages.Local(caller, "Bu komutu yalnızca gardiyanlar kullanabilir.");
            return;
        }
        
        if (!Adapter.IsWarden(caller))
        {
            WysteriaMessages.Local(caller, "Bu komutu yalnızca aktif Warden kullanabilir.");
            return;
        }

        lock (_stateLock)
        {
            if (_usedThisRound)
            {
                WysteriaMessages.Local(caller, "Bu round lockdown zaten kullanıldı.");
                return;
            }
            if (!WysteriaRound.IsLive)
            {
                WysteriaMessages.Local(caller, "Lockdown yalnızca canlı round sırasında kullanılabilir.");
                return;
            }

            _usedThisRound = true;
            _lockdownActive = true;
            
            Adapter.LockDoors(Config.LockdownDurationSeconds);
            Adapter.CloseCells();
            
            WysteriaMessages.Public($"*** SEKTÖR LOCKDOWN BAŞLATILDI — {caller.PlayerName} tarafından! {Config.LockdownDurationSeconds:F0} saniye boyunca hücreler kilitli. ***");

            ClearTimer(); // Ensure no overlapping timers
            
            _lockdownTimer = AddTimer(Config.LockdownDurationSeconds, () =>
            {
                lock (_stateLock)
                {
                    if (_lockdownActive)
                    {
                        _lockdownActive = false;
                        Adapter.UnlockDoors();
                        WysteriaMessages.Public("Lockdown sona erdi — kapılar tekrar açık.");
                    }
                    _lockdownTimer = null;
                }
            });
        }
    }
}

public sealed class PrisonLockdownConfig : WysteriaConfig
{
    public float LockdownDurationSeconds { get; set; } = 10.0f;
    public int DoorDamageThreshold { get; set; } = 300;
}
