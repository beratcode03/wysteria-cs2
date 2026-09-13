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

namespace Wysteria.WardenRules;

public sealed class WardenRulesPlugin : WysteriaPlugin<WardenRulesConfig>
{
    static WardenRulesPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.WardenRules";
    public override string ModuleVersion => "1.3.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Warden rule display system.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.WardenRules.json");
        AddCommand("css_rules", "Show or set the current Wysteria rules.", (_, info) => {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            
            var arg = info.ArgString;
            if (!string.IsNullOrEmpty(arg) && WardenService.IsWarden(player))
            {
                if (arg.Length > Config.MaxRuleLength)
                {
                    WysteriaMessages.Local(player, $"Kural çok uzun! Maksimum {Config.MaxRuleLength} karakter.");
                    return;
                }
                Config.Rules = arg;
                WysteriaMessages.Public($"Warden yeni kuralları belirledi: {Config.Rules}");
            }
            else
            {
                WysteriaMessages.Public($"Kurallar: {Config.Rules}");
                if (WardenService.IsWarden(player)) WysteriaMessages.Local(player, $"Warden yetkilerin aktif. 'css_rules <yeni kural>' ile kuralları değiştirebilirsin.");
            }
        });
    }

}

public sealed class WardenRulesConfig : WysteriaConfig
{
    public int MaxRuleLength { get; set; } = 120;
    public string ClaimCommand { get; set; } = "wysteria_w";
    public string Rules { get; set; } = "Warden kararları round içi geçerlidir.";
}
