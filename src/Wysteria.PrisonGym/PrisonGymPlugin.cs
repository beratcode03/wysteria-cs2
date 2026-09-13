/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.PrisonGym;

public sealed class PrisonGymPlugin : WysteriaPlugin<PrisonGymConfig>
{
    private readonly HashSet<ulong> _usedThisRound = new();
    private Vector _zoneCenter = new(0, 0, 0);
    private bool _zoneConfigured;
    private readonly object _stateLock = new();

    static PrisonGymPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.PrisonGym";
    public override string ModuleVersion => "1.5.0";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Limited inmate gym bonus.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.PrisonGym.json");
        RegisterEventHandler<EventRoundStart>((_, _) =>
        {
            lock (_stateLock)
            {
                _usedThisRound.Clear();
            }
            return HookResult.Continue;
        });
        RegisterEventHandler<EventRoundEnd>((_, _) =>
        {
            lock (_stateLock)
            {
                _usedThisRound.Clear();
            }
            return HookResult.Continue;
        });

        _zoneConfigured = WysteriaGeometry.TryParseVector(Config.GymZoneCenter, out _zoneCenter);
        if (!_zoneConfigured)
            Console.WriteLine($"[WYSTERIA] {ModuleName}: GymZoneCenter geçersiz; spor salonu devre dışı.");

        RegisterListener<Listeners.OnPlayerButtonsChanged>((player, buttons, oldButtons) =>
        {
            if (!Config.Enabled || !IsMapAllowed() || !WysteriaRound.IsLive ||
                !IsPrisoner(player) || !IsAlive(player) ||
                !_zoneConfigured ||
                (buttons & PlayerButtons.Use) == 0 || (oldButtons & PlayerButtons.Use) != 0)
                return;

            var pawn = player.PlayerPawn?.Value;
            if (pawn?.AbsOrigin is null ||
                WysteriaGeometry.DistanceSquared(pawn.AbsOrigin, _zoneCenter) >
                Math.Max(0, Config.GymZoneRadius) * Math.Max(0, Config.GymZoneRadius))
                return;

            StartWorkout(player);
        });
    }

    private void StartWorkout(CCSPlayerController player)
    {
        lock (_stateLock)
        {
            if (!_usedThisRound.Add(player.SteamID))
            {
                WysteriaMessages.Local(player, "Spor salonunu bu round zaten kullandın.");
                return;
            }
        }

        var duration = Math.Max(0.1f, Config.GymDuration);
        WysteriaMessages.Center(player, $"Antrenman başladı: {duration:0.0} saniye");
        
        var workerHandle = new CHandle<CCSPlayerController>((nint)player.Index);
        
        AddTimer(duration, () =>
        {
            var p = workerHandle.Value;
            if (p == null || !WysteriaRound.IsLive || !IsAlive(p))
            {
                if (p != null && p.IsValid) WysteriaMessages.Local(p, "Antrenman tamamlanamadı.");
                return;
            }

            var pawn = p.PlayerPawn?.Value;
            if (pawn is null || !pawn.IsValid) return;

            if (Config.BonusType.Equals("speed", StringComparison.OrdinalIgnoreCase))
            {
                pawn.VelocityModifier = Math.Clamp(
                    pawn.VelocityModifier * (1 + Math.Clamp(Config.GymSpeedBonus, -0.9f, 1.0f)),
                    0.1f, 2.0f);
                    
                var pawnHandle = new CHandle<CCSPlayerPawn>((nint)pawn.Index);
                AddTimer(duration, () =>
                {
                    if (pawnHandle.IsValid && pawnHandle.Value != null)
                        pawnHandle.Value.VelocityModifier = 1.0f; // Safely reset to 1.0 instead of overriding bullet tags
                });
                WysteriaMessages.Local(p, $"Antrenman tamamlandı: +{Config.GymSpeedBonus:P0} hız.");
            }
            else
            {
                pawn.Health = Math.Min(pawn.MaxHealth, pawn.Health + Math.Max(0, Config.GymBonusHP));
                Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");
                WysteriaMessages.Local(p, $"Antrenman tamamlandı: +{Config.GymBonusHP} HP.");
            }
        });
    }
}

public sealed class PrisonGymConfig : WysteriaConfig
{
    public int GymBonusHP { get; set; } = 10;
    public float GymSpeedBonus { get; set; } = 0.05f;
    public float GymDuration { get; set; } = 5.0f;
    public string BonusType { get; set; } = "Health";
    public string GymZoneCenter { get; set; } = "0,0,0";
    public float GymZoneRadius { get; set; } = 128.0f;
}
