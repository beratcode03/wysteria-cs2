/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Wysteria.Core;

namespace Wysteria.RebelAdrenaline;

public sealed class RebelAdrenalinePlugin : WysteriaPlugin<RebelAdrenalineConfig>
{
    private readonly HashSet<ulong> _firstGuardKills = new();
    private readonly Dictionary<ulong, DateTime> _active = new();
    private readonly Dictionary<ulong, float> _originalSpeed = new();

    static RebelAdrenalinePlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.RebelAdrenaline";
    public override string ModuleVersion => "1.0.8";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - First-kill rebel adrenaline bonus.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.RebelAdrenaline.json");
        RegisterEventHandler<EventRoundStart>((_, _) =>
        {
            ClearEffects();
            _firstGuardKills.Clear();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundEnd>((_, _) =>
        {
            ClearEffects();
            _firstGuardKills.Clear();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDeath>((@event, _) =>
        {
            if (@event.Userid is { IsValid: true } player)
                ClearEffect(player);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDeath>((@event, _) =>
        {
            var attacker = @event.Attacker;
            var victim = @event.Userid;
            if (!Config.Enabled || !IsMapAllowed() || !WysteriaRound.IsLive ||
                attacker is null || victim is null || attacker.SteamID == victim.SteamID ||
                !IsPrisoner(attacker) || !IsGuard(victim) ||
                !_firstGuardKills.Add(attacker.SteamID))
                return HookResult.Continue;

            Activate(attacker);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerHurt>((@event, _) =>
        {
            if (!Config.Enabled || !IsMapAllowed() || @event.Userid is not { IsValid: true } victim ||
                !_active.TryGetValue(victim.SteamID, out var expires) ||
                expires <= DateTime.UtcNow || @event.DmgHealth <= 0)
                return HookResult.Continue;

            var prevented = (int)MathF.Floor(@event.DmgHealth * Math.Clamp(Config.AdrenalineDamageResist, 0, 0.95f));
            if (prevented <= 0) return HookResult.Continue;
            var pawn = victim.PlayerPawn?.Value;
            if (pawn is { IsValid: true })
                pawn.Health = Math.Min(pawn.MaxHealth, pawn.Health + prevented);
            return HookResult.Continue;
        });
    }

    private void Activate(CCSPlayerController player)
    {
        var duration = Math.Max(0.1f, Config.AdrenalineDuration);
        _active[player.SteamID] = DateTime.UtcNow.AddSeconds(duration);
        var pawn = player.PlayerPawn?.Value;
        if (pawn is { IsValid: true })
        {
            _originalSpeed[player.SteamID] = pawn.VelocityModifier;
            pawn.VelocityModifier = Math.Clamp(
                pawn.VelocityModifier * (1 + Math.Clamp(Config.AdrenalineSpeedBonus, -0.9f, 1.0f)),
                0.1f, 2.0f);
        }

        WysteriaMessages.Center(player, $"İSYAN ADRENALİNİ! +{Config.AdrenalineSpeedBonus:P0} hız / {duration:0.0}s");
        AddTimer(duration, () =>
        {
            if (!_active.Remove(player.SteamID)) return;
            RestoreSpeed(player);
            if (WysteriaMessages.IsUsable(player))
                WysteriaMessages.Local(player, "Adrenalin etkisi sona erdi.");
        });
    }

    private void ClearEffect(CCSPlayerController player)
    {
        _active.Remove(player.SteamID);
        RestoreSpeed(player);
    }

    private void RestoreSpeed(CCSPlayerController player)
    {
        if (!_originalSpeed.Remove(player.SteamID, out var speed)) return;
        var pawn = player.PlayerPawn?.Value;
        if (pawn is { IsValid: true })
            pawn.VelocityModifier = speed;
    }

    private void ClearEffects()
    {
        foreach (var player in Utilities.GetPlayers())
            ClearEffect(player);
        _active.Clear();
        _originalSpeed.Clear();
    }
}

public sealed class RebelAdrenalineConfig : WysteriaConfig
{
    public float AdrenalineSpeedBonus { get; set; } = 0.15f;
    public float AdrenalineDamageResist { get; set; } = 0.20f;
    public float AdrenalineDuration { get; set; } = 5.0f;
}
