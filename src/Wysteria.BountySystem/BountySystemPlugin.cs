/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.BountySystem;

public sealed class BountySystemConfig : WysteriaConfig
{
    public int MinBountyCredits { get; set; } = 100;
    public int MaxBountyCredits { get; set; } = 5000;
    public int AutoBountyKillThreshold { get; set; } = 3;
    public int AutoBountyAmount { get; set; } = 200;
    public float HudIconDurationSeconds { get; set; } = 120.0f;
}

public sealed class BountyEntry
{
    public ulong TargetSteam { get; init; }
    public string TargetName { get; init; } = "";
    public int Total { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public sealed class BountySystemPlugin : WysteriaPlugin<BountySystemConfig>
{
    static BountySystemPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.BountySystem";
    public override string ModuleVersion => "1.3.5";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Player bounty and reward system.";

    private readonly Dictionary<ulong, BountyEntry> _bounties = new();
    private readonly Dictionary<ulong, int> _guardKillStreak = new();
    private readonly object _stateLock = new();

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.BountySystem.json");
        
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);

        AddCommand("css_bounty", "Place a bounty: !bounty <player_name> <amount> (Active Warden only)", OnBountyCommand);
        AddCommand("css_wysteria_bounty", "Place a bounty: !bounty <player_name> <amount> (Active Warden only)", OnBountyCommand);
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        lock (_stateLock)
        {
            _bounties.Clear();
            _guardKillStreak.Clear();
        }
        foreach (var p in Utilities.GetPlayers())
            Adapter.UnmarkPlayer(p);
        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!Config.Enabled || !IsMapAllowed()) return HookResult.Continue;

        var attacker = @event.Attacker;
        var victim = @event.Userid;
        if (attacker is null || !attacker.IsValid) return HookResult.Continue;
        if (victim is null || !victim.IsValid) return HookResult.Continue;

        if (IsPrisoner(attacker) && IsGuard(victim))
        {
            lock (_stateLock)
            {
                _guardKillStreak.TryGetValue(attacker.SteamID, out var kills);
                kills++;
                _guardKillStreak[attacker.SteamID] = kills;

                if (kills >= Config.AutoBountyKillThreshold && !_bounties.ContainsKey(attacker.SteamID))
                {
                    PlaceBounty(attacker, Config.AutoBountyAmount, auto: true);
                }
            }
        }

        lock (_stateLock)
        {
            if (attacker.SteamID != victim.SteamID && _bounties.TryGetValue(victim.SteamID, out var bounty))
            {
                AwardBounty(attacker, victim, bounty);
            }
        }

        return HookResult.Continue;
    }

    private void OnBountyCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        
        if (!IsGuard(caller))
        {
            WysteriaMessages.Local(caller, "Bu komutu yalnızca gardiyanlar kullanabilir.");
            return;
        }
        if (!Adapter.IsWarden(caller))
        {
            WysteriaMessages.Local(caller, "Bu komutu yalnızca aktif Warden kullanabilir.");
            return;
        }
        if (!caller.PawnIsAlive)
        {
            WysteriaMessages.Local(caller, "Bu komutu kullanmak için hayatta olmalısın.");
            return;
        }
        if (!Permit(info, p => true, "", Config.GlobalCooldown)) return;

        var targetName = info.GetArg(1).Trim();
        if (string.IsNullOrEmpty(targetName))
        {
            WysteriaMessages.Local(caller, "Kullanım: !bounty <oyuncu_adı> <miktar>");
            return;
        }
        if (!int.TryParse(info.GetArg(2), out var amount) || amount < Config.MinBountyCredits)
        {
            WysteriaMessages.Local(caller, $"Minimum ödül: {Config.MinBountyCredits} kredi.");
            return;
        }
        if (amount > Config.MaxBountyCredits)
        {
            WysteriaMessages.Local(caller, $"Maksimum ödül: {Config.MaxBountyCredits} kredi.");
            return;
        }

        var target = Utilities.GetPlayers()
            .FirstOrDefault(p => p.IsValid && p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        
        if (target is null || !target.IsValid)
        {
            WysteriaMessages.Local(caller, "Oyuncu bulunamadı.");
            return;
        }
        if (!IsPrisoner(target))
        {
            WysteriaMessages.Local(caller, "Ödül yalnızca mahkûmlara konabilir.");
            return;
        }
        if (!target.PawnIsAlive)
        {
            WysteriaMessages.Local(caller, "Ödül konulacak mahkum hayatta olmalı.");
            return;
        }

        var wardenCredits = Adapter.GetPlayerCredits(caller);
        if (wardenCredits < amount)
        {
            WysteriaMessages.Local(caller, $"Yeterli kredin yok. Bakiye: {wardenCredits}.");
            return;
        }

        Adapter.AddPlayerCredits(caller, -amount);
        PlaceBounty(target, amount, auto: false);
        WysteriaMessages.Local(caller, $"{target.PlayerName} üzerine {amount} kredi ödül kondu.");
    }

    private void PlaceBounty(CCSPlayerController target, int amount, bool auto)
    {
        lock (_stateLock)
        {
            if (_bounties.TryGetValue(target.SteamID, out var existing))
            {
                existing.Total += amount;
            }
            else
            {
                _bounties[target.SteamID] = new BountyEntry
                {
                    TargetSteam = target.SteamID,
                    TargetName = target.PlayerName,
                    Total = amount,
                };
            }
        }

        Adapter.MarkPlayer(target, "bounty_red", Config.HudIconDurationSeconds);

        var label = auto ? "OTOMATIK" : "WARDEN";
        lock (_stateLock)
        {
            WysteriaMessages.Public($"*** {label} ÖDÜL: {target.PlayerName} üzerine {_bounties[target.SteamID].Total} kredi! ***");
        }
    }

    private void AwardBounty(CCSPlayerController killer, CCSPlayerController victim, BountyEntry bounty)
    {
        _bounties.Remove(victim.SteamID);
        Adapter.UnmarkPlayer(victim);

        Adapter.AddPlayerCredits(killer, bounty.Total);
        WysteriaMessages.Public($"*** {killer.PlayerName}, {victim.PlayerName} hedefini ele geçirdi! {bounty.Total} kredi ödül kazanıldı! ***");
    }
}
