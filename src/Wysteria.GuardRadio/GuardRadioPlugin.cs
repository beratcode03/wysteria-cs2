/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.GuardRadio;

public sealed class GuardRadioPlugin : WysteriaPlugin<GuardRadioConfig>
{
    static GuardRadioPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.GuardRadio";
    public override string ModuleVersion => "1.0.9";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Private guard radio macros.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.GuardRadio.json");
        AddCommand("css_radio", "Yalnızca CT oyuncularına özel radyo makrosu.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            if (!Permit(info, IsGuard, "Bu telsizi yalnızca gardiyanlar kullanabilir."))
                return;
            if (info.ArgCount < 2)
            {
                WysteriaMessages.Local(player, $"Makrolar: {string.Join(", ", Config.Macros.Keys)}");
                return;
            }

            var key = info.GetArg(1).Trim().ToLowerInvariant();
            if (!Config.Macros.TryGetValue(key, out var message))
            {
                WysteriaMessages.Local(player, "Geçersiz radyo makrosu.");
                return;
            }

            var cts = Utilities.GetPlayers().Where(p => WysteriaMessages.IsUsable(p) && p.TeamNum == (int)CsTeam.CounterTerrorist).ToList();
            WysteriaMessages.Team(cts, $"[GİZLİ TELSİZ] {player.PlayerName}: {message}");
            
            foreach (var ct in cts)
            {
                // Play a standard radio beep/click sound for the CT team to simulate radio
                ct.ExecuteClientCommand("play sounds/ui/csgo_ui_button_rollover_large.vsnd");
            }
            
            UseCooldown(player, Math.Max(0, Config.RadioCooldown));
        });
    }
}

public sealed class GuardRadioConfig : WysteriaConfig
{
    public float RadioCooldown { get; set; } = 3.0f;
    public Dictionary<string, string> Macros { get; set; } = new()
    {
        ["armory"] = "Armory Under Attack!",
        ["cells"] = "Cells are compromised!",
        ["riot"] = "Riot in progress!"
    };
}
