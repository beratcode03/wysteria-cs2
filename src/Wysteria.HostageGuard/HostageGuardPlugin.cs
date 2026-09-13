/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.HostageGuard;

public sealed class HostageGuardPlugin : WysteriaPlugin<HostageGuardConfig>
{
    private readonly Dictionary<ulong, MoveType_t> _frozen = new();
    private readonly Dictionary<ulong, DateTime> _lastUse = new();

    static HostageGuardPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.HostageGuard";
    public override string ModuleVersion => "1.1.8";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Short guard hostage mechanic.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.HostageGuard.json");
        RegisterEventHandler<EventRoundEnd>((_, _) =>
        {
            foreach (var player in Utilities.GetPlayers()) Release(player);
            _frozen.Clear();
            _lastUse.Clear();
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDeath>((@event, _) =>
        {
            if (@event.Userid is { IsValid: true } player)
                Release(player);
            return HookResult.Continue;
        });
        RegisterEventHandler<EventPlayerDisconnect>((@event, _) =>
        {
            if (@event.Userid is { IsValid: true } player)
            {
                Release(player);
                _lastUse.Remove(player.SteamID);
            }
            return HookResult.Continue;
        });
        RegisterListener<Listeners.OnPlayerButtonsChanged>((player, buttons, oldButtons) =>
        {
            if (!Config.Enabled || !IsMapAllowed() || !WysteriaRound.IsLive || !IsPrisoner(player) ||
                !IsAlive(player) || (buttons & PlayerButtons.Use) == 0 ||
                (oldButtons & PlayerButtons.Use) != 0 || !Ready(player.SteamID))
                return;

            var target = FindHostageTarget(player);
            if (target is not null) TakeHostage(player, target);
        });
    }

    private bool Ready(ulong steam)
    {
        if (!_lastUse.TryGetValue(steam, out var last) ||
            (DateTime.UtcNow - last).TotalSeconds >= Config.CooldownSeconds)
        {
            _lastUse[steam] = DateTime.UtcNow;
            return true;
        }
        return false;
    }

    private CCSPlayerController? FindHostageTarget(CCSPlayerController prisoner)
    {
        var origin = prisoner.PlayerPawn?.Value?.AbsOrigin;
        if (origin is null) return null;
        return Utilities.GetPlayers()
            .Where(IsGuard)
            .Where(IsAlive)
            .Where(guard => guard.PlayerPawn?.Value?.IsScoped != true)
            .Where(guard => guard.PlayerPawn?.Value?.AbsOrigin is not null)
            .Select(guard => (guard, distance: WysteriaGeometry.DistanceSquared(
                origin, guard.PlayerPawn!.Value!.AbsOrigin!)))
            .Where(item => item.distance <= Config.InteractionRadius * Config.InteractionRadius)
            .OrderBy(item => item.distance)
            .Select(item => item.guard)
            .FirstOrDefault(guard => IsBehind(prisoner, guard));
    }

    private static bool IsBehind(CCSPlayerController prisoner, CCSPlayerController guard)
    {
        var prisonerPawn = prisoner.PlayerPawn?.Value;
        var guardPawn = guard.PlayerPawn?.Value;
        if (prisonerPawn?.AbsOrigin is null || guardPawn?.AbsOrigin is null ||
            prisonerPawn.EyeAngles is null) return false;

        var direction = guardPawn.AbsOrigin - prisonerPawn.AbsOrigin;
        var yaw = prisonerPawn.EyeAngles.Y * MathF.PI / 180;
        var forward = new Vector(MathF.Cos(yaw), MathF.Sin(yaw), 0);
        var length = MathF.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        return length > 0 && (direction.X * forward.X + direction.Y * forward.Y) / length < -0.25f;
    }

    private void TakeHostage(CCSPlayerController captor, CCSPlayerController guard)
    {
        var pawn = guard.PlayerPawn?.Value;
        if (pawn is null || !pawn.IsValid || _frozen.ContainsKey(guard.SteamID)) return;
        _frozen[guard.SteamID] = pawn.MoveType;
        pawn.MoveType = MoveType_t.MOVETYPE_NONE;
        WysteriaMessages.Team(Utilities.GetPlayers().Where(IsGuard),
            $"{guard.PlayerName} rehin alındı. Aktif Warden’a yardım edin!");
        var warden = Utilities.GetPlayers().FirstOrDefault(WardenService.IsWarden);
        if (warden is not null) WysteriaMessages.Center(warden, "REHİN ALMA: Bir guard donduruldu!");
        AddTimer(Math.Max(0.1f, Config.HostageDuration), () => Release(guard));
        WysteriaMessages.Local(captor, "Gardiyan rehin alındı.");
    }

    private void Release(CCSPlayerController player)
    {
        if (!_frozen.Remove(player.SteamID, out var originalMoveType) || !player.IsValid) return;
        var pawn = player.PlayerPawn?.Value;
        if (pawn is { IsValid: true } && pawn.MoveType == MoveType_t.MOVETYPE_NONE)
            pawn.MoveType = originalMoveType;
    }
}

public sealed class HostageGuardConfig : WysteriaConfig
{
    public float HostageDuration { get; set; } = 15.0f;
    public float CooldownSeconds { get; set; } = 60.0f;
    public float InteractionRadius { get; set; } = 96.0f;
}
