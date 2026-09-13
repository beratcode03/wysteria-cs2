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
using Wysteria.Core;

namespace Wysteria.PrisonMail;

public sealed class PrisonMailPlugin : WysteriaPlugin<PrisonMailConfig>
{
    private readonly Dictionary<ulong, List<string>> _inboxes = new();

    static PrisonMailPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.PrisonMail";
    public override string ModuleVersion => "1.3.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Private player-to-player mail.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.PrisonMail.json");

        AddCommand("css_mail", "Send a mail to a player.", OnMailCommand);
        AddCommand("css_inbox", "Check your inbox.", OnInboxCommand);
        AddCommand("css_mail_clear", "Clear your inbox.", OnMailClearCommand);
    }

    private void OnMailCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!Permit(info, p => true, "", Config.GlobalCooldown)) return;

        if (info.ArgCount < 3)
        {
            WysteriaMessages.Local(player, "Usage: !mail <player name> <message>");
            return;
        }

        string targetName = info.GetArg(1);
        string message = "";
        try 
        {
            message = info.ArgString.Substring(info.ArgString.IndexOf(targetName) + targetName.Length).Trim();
        } 
        catch { }

        if (message.Length > Config.MaxMessageLength)
        {
            WysteriaMessages.Local(player, $"Message too long (Max: {Config.MaxMessageLength} chars).");
            return;
        }

        var target = Utilities.GetPlayers().FirstOrDefault(x => x.IsValid && x.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        if (target != null && target.IsValid)
        {
            if (!_inboxes.ContainsKey(target.SteamID)) _inboxes[target.SteamID] = new List<string>();
            
            if (_inboxes[target.SteamID].Count >= Config.MaxInboxSize)
            {
                WysteriaMessages.Local(player, "Target's inbox is full.");
                return;
            }

            _inboxes[target.SteamID].Add($"From {player.PlayerName}: {message}");
            WysteriaMessages.Local(player, $"Mail sent to {target.PlayerName}.");
            WysteriaMessages.Local(target, "You have received a new mail! Type !inbox to read it.");
        }
        else
        {
            WysteriaMessages.Local(player, "Target player not found.");
        }
    }

    private void OnInboxCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;

        if (_inboxes.TryGetValue(player.SteamID, out var messages) && messages.Count > 0)
        {
            player.PrintToConsole("--- PRISON MAIL INBOX ---");
            for (int i = 0; i < messages.Count; i++)
            {
                player.PrintToConsole($"[{i+1}] {messages[i]}");
            }
            player.PrintToConsole("-------------------------");
            WysteriaMessages.Local(player, $"You have {messages.Count} mails. Check your console. Type !mail_clear to empty inbox.");
        }
        else
        {
            WysteriaMessages.Local(player, "Your inbox is empty.");
        }
    }

    private void OnMailClearCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        
        if (_inboxes.Remove(player.SteamID))
        {
            WysteriaMessages.Local(player, "Inbox cleared.");
        }
    }
}

public sealed class PrisonMailConfig : WysteriaConfig
{
    public int MaxMessageLength { get; set; } = 100;
    public int MaxInboxSize { get; set; } = 5;
}
