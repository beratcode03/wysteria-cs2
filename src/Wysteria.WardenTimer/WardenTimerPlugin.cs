/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Wysteria.Core;

namespace Wysteria.WardenTimer;

public sealed class WardenTimerPlugin : WysteriaPlugin<WardenTimerConfig>
{
    private DateTime? _timerEnd = null;
    private string _timerLabel = "";

    static WardenTimerPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenTimer";
    public override string ModuleVersion => "1.5.9";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Timed Warden activity tool.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenTimer.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);

        AddCommand("css_wtimer", "Sets a timer for the Warden.", OnTimerCommand);
        AddCommand("css_wtimer_stop", "Stops the active timer.", OnTimerStopCommand);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _timerEnd = null;
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        _timerEnd = null;
        return HookResult.Continue;
    }

    private void OnTimerCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!Permit(info, p => WardenService.IsWarden(p) || CounterStrikeSharp.API.Modules.Admin.AdminManager.PlayerHasPermissions(p, "@css/admin"), "Only the Warden can set a timer.", Config.GlobalCooldown)) return;

        if (info.ArgCount < 2)
        {
            WysteriaMessages.Local(player, "Usage: !wtimer <seconds> [label]");
            return;
        }

        if (int.TryParse(info.GetArg(1), out int seconds) && seconds > 0)
        {
            _timerLabel = info.ArgCount >= 3 ? info.GetArg(2) : "Activity";
            _timerEnd = DateTime.UtcNow.AddSeconds(seconds);
            
            WysteriaMessages.Public($"{_timerLabel} timer started for {seconds} seconds!");
            
            // Note: In a full implementation, we'd use a single low-frequency global scheduler to check if DateTime.UtcNow >= _timerEnd
            // and send a message when it finishes, instead of a repeating timer per player.
            AddTimer(seconds, () => {
                if (_timerEnd != null)
                {
                    WysteriaMessages.Public($"Timer finished: {_timerLabel}!");
                    _timerEnd = null;
                }
            });
        }
        else
        {
            WysteriaMessages.Local(player, "Invalid seconds.");
        }
    }

    private void OnTimerStopCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Permit(info, p => WardenService.IsWarden(p) || CounterStrikeSharp.API.Modules.Admin.AdminManager.PlayerHasPermissions(p, "@css/admin"), "Only the Warden can stop the timer.", 0)) return;

        if (_timerEnd != null)
        {
            _timerEnd = null;
            WysteriaMessages.Public($"The Warden stopped the {_timerLabel} timer.");
        }
    }
}

public sealed class WardenTimerConfig : WysteriaConfig
{
}
