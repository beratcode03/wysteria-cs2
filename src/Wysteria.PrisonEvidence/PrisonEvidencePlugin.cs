/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using Wysteria.Core;

namespace Wysteria.PrisonEvidence;

public sealed class PrisonEvidencePlugin : WysteriaPlugin<PrisonEvidenceConfig>
{
    private readonly List<string> _memoryLog = new();
    private readonly object _lock = new();

    static PrisonEvidencePlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.PrisonEvidence";
    public override string ModuleVersion => "1.1.9";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Round evidence event logging.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.PrisonEvidence.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);

        AddCommand("css_evidence", "Dumps recent evidence logs to console (Admin only).", OnEvidenceCommand);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        LogEvidence("Round Started");
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        var attacker = @event.Attacker;
        var victim = @event.Userid;

        if (attacker != null && victim != null && attacker.IsValid && victim.IsValid)
        {
            LogEvidence($"KILL: {attacker.PlayerName} killed {victim.PlayerName}");
        }
        return HookResult.Continue;
    }

    public void LogEvidence(string message)
    {
        if (!Config.Enabled) return;
        lock (_lock)
        {
            _memoryLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
            if (_memoryLog.Count > Config.MaxLogLines)
            {
                _memoryLog.RemoveAt(0);
            }
        }
    }

    private void OnEvidenceCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!Permit(info, p => CounterStrikeSharp.API.Modules.Admin.AdminManager.PlayerHasPermissions(p, "@css/admin"), "This command requires Admin privileges.", Config.GlobalCooldown)) return;

        lock (_lock)
        {
            player.PrintToConsole("--- PRISON EVIDENCE LOG ---");
            foreach (var log in _memoryLog)
            {
                player.PrintToConsole(log);
            }
            player.PrintToConsole("---------------------------");
            WysteriaMessages.Local(player, "Evidence log has been printed to your console.");
        }
    }
}

public sealed class PrisonEvidenceConfig : WysteriaConfig
{
    public int MaxLogLines { get; set; } = 100;
}
