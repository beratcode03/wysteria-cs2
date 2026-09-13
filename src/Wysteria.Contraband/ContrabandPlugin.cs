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

namespace Wysteria.Contraband;

public sealed class ContrabandPlugin : WysteriaPlugin<ContrabandConfig>
{
    static ContrabandPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.Contraband";
    public override string ModuleVersion => "1.0.6";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Random contraband stash events.";

    private Random _rnd = new Random();

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.Contraband.json");
        AddCommand("css_stash", "Mahkûmlara özel rastgele zula arama ve ödül sistemi.", OnStashCommand);
        AddCommand("css_wysteria_stash", "Mahkûmlara özel rastgele zula arama ve ödül sistemi.", OnStashCommand);
    }

    private void OnStashCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            
            if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
            if (!player.PawnIsAlive) { WysteriaMessages.Local(player, "Bu komutu kullanmak için hayatta olmalısın."); return; }
            
            if (!Permit(info, p => p.TeamNum == (int)CsTeam.Terrorist, "Bu komutu yalnızca mahkumlar kullanabilir.", Config.GlobalCooldown)) return;
            
            if (_rnd.NextDouble() <= Config.StashSpawnChance)
            {
                int reward = _rnd.Next(Config.MinCredits, Config.MaxCredits + 1);
                Adapter.AddPlayerCredits(player, reward);
                WysteriaMessages.Local(player, $"Zula buldun! Kasaya {reward} kredi eklendi.");
                
                // Randomly give a weak weapon sometimes
                if (_rnd.NextDouble() <= 0.2)
                {
                    player.GiveNamedItem("weapon_glock");
                    WysteriaMessages.Local(player, "Zulanın içinde bir Glock buldun! Gizle!");
                }
            }
            else
            {
                WysteriaMessages.Local(player, "Buralarda hiçbir şey yok.");
            }
    }
}

public sealed class ContrabandConfig : WysteriaConfig
{
    public float StashSpawnChance { get; set; } = 0.35f;
    public int MinCredits { get; set; } = 20;
    public int MaxCredits { get; set; } = 150;
}
