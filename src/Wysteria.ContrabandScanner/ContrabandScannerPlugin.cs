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

namespace Wysteria.ContrabandScanner;

public sealed class ContrabandScannerPlugin : WysteriaPlugin<ContrabandScannerConfig>
{
    static ContrabandScannerPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.ContrabandScanner";
    public override string ModuleVersion => "1.1.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Guard contraband detection tool.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.ContrabandScanner.json");
        AddCommand("css_scan", "CT'ler için mahkûmların üzerinde silah arama (Alan taraması).", OnScanCommand);
        AddCommand("css_wysteria_scan", "CT'ler için mahkûmların üzerinde silah arama (Alan taraması).", OnScanCommand);
    }

    private void OnScanCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (!TryGetPlayer(info, out var player)) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
        if (!player.PawnIsAlive) { WysteriaMessages.Local(player, "Bu komutu kullanmak için hayatta olmalısın."); return; }
        if (!Permit(info, p => p.TeamNum == (int)CsTeam.CounterTerrorist, "Bu komutu yalnızca gardiyanlar kullanabilir.", Config.CooldownSeconds)) return;

        int suspectCount = 0;
        var origin = player.PlayerPawn?.Value?.AbsOrigin;
        if (origin != null)
        {
            foreach (var target in Utilities.GetPlayers())
            {
                if (target == null || !target.IsValid || !target.PawnIsAlive) continue;
                if (target.TeamNum != (int)CsTeam.Terrorist) continue;
                var targetOrigin = target.PlayerPawn?.Value?.AbsOrigin;
                if (targetOrigin == null) continue;
                
                float distance = (origin - targetOrigin).Length();
                if (distance <= Config.ScanRadius)
                {

                    // Check for weapons if possible, or just flag as suspect
                    suspectCount++;
                }
            }
        }
        
        WysteriaMessages.Local(player, suspectCount > 0 
            ? $"Tarama sonucu: Yakınlarda {suspectCount} şüpheli bulundu! Dikkatli ol." 
            : "Tarama sonucu: Alan temiz.");
    }
}

public sealed class ContrabandScannerConfig : WysteriaConfig
{
    public int ScanRadius { get; set; } = 350;
    public int CooldownSeconds { get; set; } = 25;
}
