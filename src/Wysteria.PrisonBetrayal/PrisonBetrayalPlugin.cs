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

namespace Wysteria.PrisonBetrayal;

public sealed class PrisonBetrayalPlugin : WysteriaPlugin<PrisonBetrayalConfig>
{
    static PrisonBetrayalPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.PrisonBetrayal";
    public override string ModuleVersion => "1.1.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Inmate weapon report system.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.PrisonBetrayal.json");
        AddCommand("css_snitch", "Mahkûmun silahlı bir mahkûmu gizlice bildirmesi.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(info.CallingPlayer!, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
        if (!WysteriaRound.CellsOpen) { WysteriaMessages.Local(info.CallingPlayer!, "Hücreler açılmadan bu komut kullanılamaz."); return; }
if (!Permit(info, player => player.TeamNum == (int)CsTeam.Terrorist, "Only prisoners can use this.", Config.GlobalCooldown)) return;
            if (info.ArgCount < 2) { WysteriaMessages.Local(player, "Kullanım: !snitch <oyuncu>"); return; }
            var target = WysteriaMessages.FindPlayer(info.GetArg(1));
            if (target is null || target.TeamNum != (int)CsTeam.Terrorist || target.SteamID == player.SteamID) {
                WysteriaMessages.Local(player, "Geçerli bir mahkûm hedefi bulunamadı."); return;
            }
            if (Adapter.HasContraband(target))
            {
                Adapter.AddPlayerCredits(player, Config.SnitchRewardCredits);
                WysteriaMessages.Local(player, $"İhbar başarılı! Hedefte silah bulundu. {Config.SnitchRewardCredits} kredi kazandın.");
                WysteriaMessages.Local(target, "Biri seni ispiyonladı! Gardiyanlar silahlı olduğunu biliyor!");
                Adapter.PrintAlert(target, "Biri seni ispiyonladı! Gardiyanlar silahlı olduğunu biliyor!");
            }
            else
            {
                Adapter.SetPlayerCredits(player, Math.Max(0, Adapter.GetPlayerCredits(player) - Config.FalseReportPenalty));
                WysteriaMessages.Local(player, $"Yanlış ihbar! Hedefte silah yok. {Config.FalseReportPenalty} kredi cezası uygulandı.");
            }
        });
    }

}

public sealed class PrisonBetrayalConfig : WysteriaConfig
{
    public int SnitchRewardCredits { get; set; } = 100;
    public int FalseReportPenalty { get; set; } = 50;
}
