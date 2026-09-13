/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.CellSearch;

public sealed class CellSearchPlugin : WysteriaPlugin<CellSearchConfig>
{
    static CellSearchPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.CellSearch";
    public override string ModuleVersion => "1.0.9";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Guard-led cell search system.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.CellSearch.json");
        AddCommand("css_cellsearch", "Hücrelerde yasaklı eşya araması (Gardiyanlara özel).", OnCellSearchCommand);
        AddCommand("css_wysteria_cellsearch", "Hücrelerde yasaklı eşya araması (Gardiyanlara özel).", OnCellSearchCommand);
    }

    private void OnCellSearchCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (!TryGetPlayer(info, out var player)) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
        if (!player.PawnIsAlive) { WysteriaMessages.Local(player, "Bu komutu kullanmak için hayatta olmalısın."); return; }
        if (!Permit(info, p => p.TeamNum == (int)CsTeam.CounterTerrorist, "Bu komutu yalnızca gardiyanlar kullanabilir.", Config.CooldownSeconds)) return;
            
            WysteriaMessages.Local(player, "Hücre araması yapılıyor...");
            
            AddTimer(Config.SearchDurationSeconds, () =>
            {
                if (player == null || !player.IsValid || !player.PawnIsAlive) return;

                var origin = player.PlayerPawn?.Value?.AbsOrigin;
                if (origin == null) return;

                bool found = false;
                foreach (var t in Utilities.GetPlayers())
                {
                    if (WysteriaMessages.IsUsable(t) && t.TeamNum == (int)CsTeam.Terrorist && t.PawnIsAlive)
                    {
                        var tOrigin = t.PlayerPawn?.Value?.AbsOrigin;
                        if (tOrigin != null && WysteriaGeometry.DistanceSquared(origin, tOrigin) < 300 * 300)
                        {
                            if (Adapter.HasContraband(t))
                            {
                                var weapons = t.PlayerPawn?.Value?.WeaponServices?.MyWeapons;
                                if (weapons != null)
                                {
                                    foreach (var w in weapons)
                                    {
                                        if (w.Value != null && w.Value.IsValid)
                                        {
                                            var name = w.Value.DesignerName;
                                            if (!name.Contains("knife") && !name.Contains("bayonet") && !name.Contains("fists") && !name.Contains("melee"))
                                            {
                                                w.Value.AcceptInput("Kill"); // Destroy the contraband
                                                found = true;
                                            }
                                        }
                                    }
                                }
                                if (found)
                                {
                                    WysteriaMessages.Public($"{player.PlayerName}, {t.PlayerName} adlı mahkumun üstündeki kaçak eşyaları bulup imha etti!");
                                }
                            }
                        }
                    }
                }

                if (!found)
                {
                    WysteriaMessages.Local(player, "Etrafta kaçak eşya/silah bulunamadı.");
                }
            });
    }
}

public sealed class CellSearchConfig : WysteriaConfig
{
    public float SearchDurationSeconds { get; set; } = 4.0f;
    public int CooldownSeconds { get; set; } = 20;
}
