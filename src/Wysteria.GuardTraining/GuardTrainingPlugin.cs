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

namespace Wysteria.GuardTraining;

public sealed class GuardTrainingPlugin : WysteriaPlugin<GuardTrainingConfig>
{
    static GuardTrainingPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.GuardTraining";
    public override string ModuleVersion => "1.0.6";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Guard target training and rewards.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.GuardTraining.json");
        AddCommand("css_train", "CT'ler için antrenman modunu açıp buff kazanma.", OnTrainCommand);
        AddCommand("css_wysteria_train", "CT'ler için antrenman modunu açıp buff kazanma.", OnTrainCommand);
    }

    private void OnTrainCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (!TryGetPlayer(info, out var player)) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        if (!WysteriaRound.IsLive) { WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); return; }
        if (!player.PawnIsAlive) { WysteriaMessages.Local(player, "Bu komutu kullanmak için hayatta olmalısın."); return; }
        if (!Permit(info, p => p.TeamNum == (int)CsTeam.CounterTerrorist, "Bu komutu yalnızca gardiyanlar kullanabilir.", Config.CooldownSeconds)) return;

        var pawn = player.PlayerPawn?.Value;
        if (pawn == null) return;
        
        var originalHealth = pawn.Health;
        pawn.Health = 200;
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
        WysteriaMessages.Center(player, $"Eğitim başladı ({Config.TrainingDurationSeconds:0.0}s). Canınız geçici olarak yükseltildi.");

        AddTimer(Config.TrainingDurationSeconds, () => {
            if (WysteriaMessages.IsUsable(player) && player.PlayerPawn?.Value != null)
            {
                player.PlayerPawn.Value.Health = originalHealth;
                Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
                WysteriaMessages.Local(player, "Eğitim tamamlandı, canınız normale döndü.");
            }
        });
    }
}

public sealed class GuardTrainingConfig : WysteriaConfig
{
    public float TrainingDurationSeconds { get; set; } = 10.0f;
    public int CooldownSeconds { get; set; } = 20;
}
