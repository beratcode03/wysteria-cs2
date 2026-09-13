/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.GuardQueue;

public sealed class GuardQueuePlugin : WysteriaPlugin<GuardQueueConfig>
{
    private readonly List<ulong> _queue = new();

    static GuardQueuePlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.GuardQueue";
    public override string ModuleVersion => "1.1.7";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Queue system for guard selection.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.GuardQueue.json");
        
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        AddCommand("css_guard", "Join the CT queue.", OnJoinQueueCommand);
        AddCommand("css_qleave", "Leave the CT queue.", OnLeaveQueueCommand);
        AddCommand("css_qstatus", "Check your position in the CT queue.", OnQueueStatusCommand);
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid != null)
        {
            _queue.Remove(@event.Userid.SteamID);
        }
        return HookResult.Continue;
    }

    private void OnJoinQueueCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        
        if (player.TeamNum == (int)CsTeam.CounterTerrorist)
        {
            WysteriaMessages.Local(player, "You are already a Guard.");
            return;
        }

        if (_queue.Contains(player.SteamID))
        {
            WysteriaMessages.Local(player, $"You are already in the queue. Position: {_queue.IndexOf(player.SteamID) + 1}/{_queue.Count}");
            return;
        }

        if (_queue.Count >= Config.MaxQueueSize)
        {
            WysteriaMessages.Local(player, "The Guard queue is currently full.");
            return;
        }

        _queue.Add(player.SteamID);
        WysteriaMessages.Local(player, $"Joined the Guard queue. Position: {_queue.Count}");
        
        // Note: Actual logic to move players from the queue to the CT team 
        // when a slot opens up would typically hook TeamBalance logic or 
        // periodically check team counts if event-based is not feasible.
    }

    private void OnLeaveQueueCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        
        if (_queue.Remove(player.SteamID))
        {
            WysteriaMessages.Local(player, "You have left the Guard queue.");
        }
        else
        {
            WysteriaMessages.Local(player, "You are not in the Guard queue.");
        }
    }

    private void OnQueueStatusCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        
        int index = _queue.IndexOf(player.SteamID);
        if (index >= 0)
        {
            WysteriaMessages.Local(player, $"You are #{index + 1} in the Guard queue (Total: {_queue.Count}).");
        }
        else
        {
            WysteriaMessages.Local(player, "You are not in the Guard queue.");
        }
    }
}

public sealed class GuardQueueConfig : WysteriaConfig
{
    public int MaxQueueSize { get; set; } = 10;
}
