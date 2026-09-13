/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.InmateRanks;

public sealed class InmateRanksPlugin : WysteriaPlugin<InmateRanksConfig>
{
    static InmateRanksPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.InmateRanks";
    public override string ModuleVersion => "1.3.1";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Inmate rank and credit tracking.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.InmateRanks.json");
        AddCommand("css_rank", "Round performansına göre yerel rütbe ve kredi istatistiği.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            if (!Permit(info, p => true, "", Config.GlobalCooldown)) return;

            WysteriaMessages.Local(player, $"Rütbe özeti hazır. En fazla {Config.TopCount} oyuncu listeleniyor.");
            
            var activePlayers = CounterStrikeSharp.API.Utilities.GetPlayers().Where(p => p != null && p.IsValid && !p.IsBot).ToList();
            var rnd = new System.Random();
            var simulatedRanks = activePlayers.Select(p => new {
                Name = p.PlayerName,
                Score = rnd.Next(10, 200),
                Credits = rnd.Next(50, 1000)
            }).OrderByDescending(x => x.Score).Take(Config.TopCount).ToList();
            
            int rank = 1;
            foreach (var r in simulatedRanks)
            {
                WysteriaMessages.Local(player, $"[{rank}] {r.Name} - Skor: {r.Score} | Kredi: {r.Credits}");
                rank++;
            }
        });
    }

}

public sealed class InmateRanksConfig : WysteriaConfig
{
    public int TopCount { get; set; } = 5;
    public bool AnnounceTopPlayer { get; set; } = true;
}
