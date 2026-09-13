/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.AdminTools;

public sealed class AdminToolsPlugin : WysteriaPlugin<AdminToolsConfig>
{
    static AdminToolsPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.AdminTools";
    public override string ModuleVersion => "1.0.7";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Admin status and module controls.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.AdminTools.json");
        AddCommand("css_wysteria", "Show Wysteria health summary.", (_, info) => {
            if (!Config.Enabled || !IsMapAllowed()) return;
            
            bool HasAccess(CCSPlayerController p)
            {
                if (string.IsNullOrWhiteSpace(Config.RequiredSteamIds)) return false;
                return Config.RequiredSteamIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Contains(p.SteamID.ToString(), StringComparer.Ordinal);
            }

            if (!Permit(info, HasAccess, "Yönetici iznin yok.")) return;
            
            if (TryGetPlayer(info, out var player))
            {
                var playerCount = CounterStrikeSharp.API.Utilities.GetPlayers().Count(p => p != null && p.IsValid && !p.IsBot);
                var mapName = CounterStrikeSharp.API.Server.MapName;
                player.PrintToChat($" \x04[Wysteria Admin]\x01 Health Summary:");
                player.PrintToChat($" \x04-\x01 Map: \x0C{mapName}");
                player.PrintToChat($" \x04-\x01 Real Players: \x0C{playerCount}");
                player.PrintToChat($" \x04-\x01 TickCount: \x0C{CounterStrikeSharp.API.Server.TickCount}");
            }
        });
    }

}

public sealed class AdminToolsConfig : WysteriaConfig
{
    public string RequiredSteamIds { get; set; } = "";
    public bool AllowReload { get; set; } = false;
}
