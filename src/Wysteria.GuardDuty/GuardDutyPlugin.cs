/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.GuardDuty;

public sealed class GuardDutyPlugin : WysteriaPlugin<GuardDutyConfig>
{
    private readonly Dictionary<ulong, DateTime> _dutyStartTimes = new();
    private readonly Dictionary<ulong, TimeSpan> _totalDutyDurations = new();

    static GuardDutyPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.GuardDuty";
    public override string ModuleVersion => "1.6.1";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Guard duty session tracking.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.GuardDuty.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        AddCommand("css_duty", "Toggles guard duty on or off.", OnDutyCommand);
        AddCommand("css_duty_stats", "Shows your current duty statistics.", OnDutyStatsCommand);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _dutyStartTimes.Clear();
        _totalDutyDurations.Clear();
        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        foreach (var kvp in _dutyStartTimes)
        {
            var duration = DateTime.UtcNow - kvp.Value;
            if (_totalDutyDurations.ContainsKey(kvp.Key))
                _totalDutyDurations[kvp.Key] += duration;
            else
                _totalDutyDurations[kvp.Key] = duration;
        }
        _dutyStartTimes.Clear();
        return HookResult.Continue;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            _dutyStartTimes.Remove(@event.Userid.SteamID);
            _totalDutyDurations.Remove(@event.Userid.SteamID);
        }
        return HookResult.Continue;
    }

    private void OnDutyCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!Permit(info, p => p.TeamNum == (int)CsTeam.CounterTerrorist, "This command is only for Guards.", Config.GlobalCooldown)) return;

        if (_dutyStartTimes.ContainsKey(player.SteamID))
        {
            var duration = DateTime.UtcNow - _dutyStartTimes[player.SteamID];
            if (_totalDutyDurations.ContainsKey(player.SteamID))
                _totalDutyDurations[player.SteamID] += duration;
            else
                _totalDutyDurations[player.SteamID] = duration;
            
            _dutyStartTimes.Remove(player.SteamID);
            WysteriaMessages.Local(player, $"Duty stopped. Session lasted: {duration.TotalMinutes:F1} minutes.");
        }
        else
        {
            _dutyStartTimes[player.SteamID] = DateTime.UtcNow;
            WysteriaMessages.Local(player, "Duty started. You are now officially on active guard duty.");
        }
    }

    private void OnDutyStatsCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;

        var total = _totalDutyDurations.GetValueOrDefault(player.SteamID, TimeSpan.Zero);
        if (_dutyStartTimes.TryGetValue(player.SteamID, out var startTime))
        {
            total += (DateTime.UtcNow - startTime);
        }

        WysteriaMessages.Local(player, $"Total Guard Duty Time this round: {total.TotalMinutes:F1} minutes.");
    }
}

public sealed class GuardDutyConfig : WysteriaConfig
{
}
