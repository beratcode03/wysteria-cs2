/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.CoinFlip;

public sealed class CoinFlipConfig : WysteriaPluginConfig
{
    public int MinBet { get; set; } = 10;
    public int MaxBet { get; set; } = 1000;
    public float HouseTaxPercentage { get; set; } = 0.05f;
}

public sealed class CoinFlipChallenge
{
    public ulong ChallengerSteam { get; init; }
    public string ChallengerName { get; init; } = "";
    public ulong TargetSteam { get; init; }
    public string TargetName { get; init; } = "";
    public int Amount { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

[MinimumApiVersion(319)]
public sealed class CoinFlipPlugin : WysteriaPlugin
{
    public override string ModuleName => "Wysteria.CoinFlip";
    public override string ModuleVersion => "1.5.1";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Player coin flip challenge system.";

    private CoinFlipConfig _config = new();
    private readonly Dictionary<ulong, CoinFlipChallenge> _pendingChallenges = new();
    private readonly Dictionary<ulong, DateTime> _cooldowns = new();
    private readonly Random _rng = new();

    private const double ChallengeTimeoutSeconds = 30.0;

    public override void Load(bool hotReload)
    {
        _config = ConfigStore.Load<CoinFlipConfig>("Wysteria.CoinFlip.json");
        Console.WriteLine("[WYSTERIA] Wysteria.CoinFlip loaded.");
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _pendingChallenges.Clear();
        return HookResult.Continue;
    }

    [ConsoleCommand("css_coinflip", "Challenge a player to a coin flip: !coinflip <amount> <player_name>")]
    public void OnCoinFlipCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid) return;
        if (!_config.Enabled || !IsMapAllowed(_config.AllowedMaps)) return;

        var team = caller.Team;
        if (team != CsTeam.Terrorist && team != CsTeam.CounterTerrorist && team != CsTeam.Spectator)
        {
            caller.PrintToChat("[WYSTERIA] Bu komut kullanılamaz durumda.");
            return;
        }

        if (!CheckCooldown(_cooldowns, caller.SteamID, _config.GlobalCooldown))
        {
            caller.PrintToChat("[WYSTERIA] Çok hızlı — lütfen bekleyin.");
            return;
        }

        if (!int.TryParse(info.GetArg(1), out var amount) || amount < _config.MinBet)
        {
            caller.PrintToChat($"[WYSTERIA] Minimum bahis: {_config.MinBet} kredi.");
            return;
        }
        if (amount > _config.MaxBet)
        {
            caller.PrintToChat($"[WYSTERIA] Maksimum bahis: {_config.MaxBet} kredi.");
            return;
        }

        var targetName = info.GetArg(2).Trim();
        if (string.IsNullOrEmpty(targetName))
        {
            caller.PrintToChat("[WYSTERIA] Kullanım: !coinflip <miktar> <oyuncu_adı>");
            return;
        }

        var target = Utilities.GetPlayers()
            .FirstOrDefault(p => p.IsValid && p.PlayerName.Contains(targetName, StringComparison.OrdinalIgnoreCase));
        if (target is null || !target.IsValid)
        {
            caller.PrintToChat("[WYSTERIA] Oyuncu bulunamadı.");
            return;
        }
        if (target.SteamID == caller.SteamID)
        {
            caller.PrintToChat("[WYSTERIA] Kendine meydan okuyamazsın.");
            return;
        }

        var challengerCredits = WysteriaPlugin.Adapter.GetPlayerCredits(caller);
        if (challengerCredits < amount)
        {
            caller.PrintToChat($"[WYSTERIA] Yeterli kredin yok. Bakiye: {challengerCredits}.");
            return;
        }
        var targetCredits = WysteriaPlugin.Adapter.GetPlayerCredits(target);
        if (targetCredits < amount)
        {
            caller.PrintToChat($"[WYSTERIA] Rakibin yeterli krediye sahip değil.");
            return;
        }
        _pendingChallenges[target.SteamID] = new CoinFlipChallenge
        {
            ChallengerSteam = caller.SteamID,
            ChallengerName = caller.PlayerName,
            TargetSteam = target.SteamID,
            TargetName = target.PlayerName,
            Amount = amount,
        };

        caller.PrintToChat($"[WYSTERIA] {target.PlayerName} kişisine {amount} kredi meydan okundu. Kabul etmesi bekleniyor...");
        target.PrintToChat($"[WYSTERIA] {caller.PlayerName} sana {amount} kredi yazı-tura meydan okudu! Kabul için !cfaccept");
    }

    [ConsoleCommand("css_cfaccept", "Accept a pending coin flip challenge")]
    public void OnCfAcceptCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid) return;
        if (!_config.Enabled || !IsMapAllowed(_config.AllowedMaps)) return;
        if (!CheckCooldown(_cooldowns, caller.SteamID, _config.GlobalCooldown))
        {
            caller.PrintToChat("[WYSTERIA] Çok hızlı — lütfen bekleyin.");
            return;
        }

        if (!_pendingChallenges.TryGetValue(caller.SteamID, out var challenge))
        {
            caller.PrintToChat("[WYSTERIA] Bekleyen bir meydan okuman yok.");
            return;
        }
        if ((DateTime.UtcNow - challenge.CreatedAt).TotalSeconds > ChallengeTimeoutSeconds)
        {
            _pendingChallenges.Remove(caller.SteamID);
            caller.PrintToChat("[WYSTERIA] Meydan okumanın süresi doldu.");
            return;
        }

        _pendingChallenges.Remove(caller.SteamID);

        var challenger = Utilities.GetPlayers()
            .FirstOrDefault(p => p.IsValid && p.SteamID == challenge.ChallengerSteam);
        if (challenger is null || !challenger.IsValid)
        {
            caller.PrintToChat("[WYSTERIA] Meydan okuyan oyuncu bulunamadı.");
            return;
        }

        Server.PrintToChatAll($"[WYSTERIA] {challenger.PlayerName} ve {caller.PlayerName} arasında {challenge.Amount} kredilik yazı-tura atılıyor...");

        AddTimer(3.0f, () =>
        {
            var currentCCredits = WysteriaPlugin.Adapter.GetPlayerCredits(challenger);
            var currentTCredits = WysteriaPlugin.Adapter.GetPlayerCredits(caller);
            if (currentCCredits < challenge.Amount || currentTCredits < challenge.Amount)
            {
                Server.PrintToChatAll($"[WYSTERIA] Yazı-tura iptal — kredi yetersiz ({challenger.PlayerName} vs {caller.PlayerName}).");
                return;
            }

            var challengerWins = _rng.Next(2) == 0;
            var winner = challengerWins ? challenger : caller;
            var loser = challengerWins ? caller : challenger;
            var tax = (int)(challenge.Amount * _config.HouseTaxPercentage);
            var payout = challenge.Amount - tax;

            WysteriaPlugin.Adapter.AddPlayerCredits(loser, -challenge.Amount);
            WysteriaPlugin.Adapter.AddPlayerCredits(winner, payout);

            Server.PrintToChatAll($"[WYSTERIA] Yazı-Tura: {winner.PlayerName} kazandı! {loser.PlayerName} kaybetti. Ödül: {payout} kredi (ev vergisi: {tax}).");
        });
    }
}
