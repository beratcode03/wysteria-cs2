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

namespace Wysteria.PrisonSchedule;

public sealed class PrisonSchedulePlugin : WysteriaPlugin<PrisonScheduleConfig>
{
    private int _currentActivityIndex = -1;

    static PrisonSchedulePlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.PrisonSchedule";
    public override string ModuleVersion => "1.5.1";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Jailbreak activity scheduling.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.PrisonSchedule.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);

        AddCommand("css_schedule", "View current and next scheduled activity.", OnScheduleCommand);
        AddCommand("css_next_activity", "Advance to the next scheduled activity (Warden/Admin).", OnNextActivityCommand);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _currentActivityIndex = -1;
        return HookResult.Continue;
    }

    private void OnScheduleCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;

        if (Config.Activities == null || Config.Activities.Length == 0)
        {
            WysteriaMessages.Local(player, "No activities scheduled.");
            return;
        }

        string current = _currentActivityIndex >= 0 && _currentActivityIndex < Config.Activities.Length 
            ? Config.Activities[_currentActivityIndex] : "Free Time / None";
            
        string next = (_currentActivityIndex + 1) < Config.Activities.Length 
            ? Config.Activities[_currentActivityIndex + 1] : "End of Schedule";

        WysteriaMessages.Local(player, $"Current: {current} | Next: {next}");
    }

    private void OnNextActivityCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!Permit(info, p => WardenService.IsWarden(p) || CounterStrikeSharp.API.Modules.Admin.AdminManager.PlayerHasPermissions(p, "@css/admin"), "Only Warden can advance schedule.", Config.GlobalCooldown)) return;

        if (Config.Activities == null || Config.Activities.Length == 0) return;

        _currentActivityIndex++;
        
        if (_currentActivityIndex < Config.Activities.Length)
        {
            WysteriaMessages.Public($"[Schedule] Next Activity Started: {Config.Activities[_currentActivityIndex]}!");
        }
        else
        {
            WysteriaMessages.Public("[Schedule] All scheduled activities have concluded.");
        }
    }
}

public sealed class PrisonScheduleConfig : WysteriaConfig
{
    public string[] Activities { get; set; } = new string[] { "Cell Inspection", "Gym Time", "Dining Hall", "Yard Time" };
}
