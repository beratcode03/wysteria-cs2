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

namespace Wysteria.RoundStats;

public sealed class RoundStatsPlugin : WysteriaPlugin<RoundStatsConfig>
{
    static RoundStatsPlugin() => WysteriaCoreGuard.Require();

    public override string ModuleName => "Wysteria.RoundStats";
    public override string ModuleVersion => "1.3.0";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Private round statistics summary.";

    public class PlayerStats
    {
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int DamageDealt { get; set; }
    }

    private Dictionary<ulong, PlayerStats> _stats = new();

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.RoundStats.json");
        AddCommand("css_stats", "Oyuncunun kendi round istatistiklerini özel mesajla göstermesi.", (_, info) =>
        {
            if (!TryGetPlayer(info, out var player)) return;
            if (!Config.Enabled || !IsMapAllowed()) return;
            if (!Permit(info, p => true, "", Config.GlobalCooldown)) return;

            if (_stats.TryGetValue(player.SteamID, out var stats))
            {
                WysteriaMessages.Local(player, $"[Round Stats] Kills: {stats.Kills} | Deaths: {stats.Deaths} | Damage: {stats.DamageDealt}");
            }
            else
            {
                WysteriaMessages.Local(player, "Şu an için hiç istatistiğin yok.");
            }
        });
        
        RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            if (@event.Userid == null || @event.Attacker == null) return HookResult.Continue;
            
            var victim = @event.Userid;
            var attacker = @event.Attacker;
            
            if (attacker.IsValid && attacker.SteamID != 0)
            {
                if (!_stats.ContainsKey(attacker.SteamID)) _stats[attacker.SteamID] = new PlayerStats();
                _stats[attacker.SteamID].Kills++;
            }
            
            if (victim.IsValid && victim.SteamID != 0)
            {
                if (!_stats.ContainsKey(victim.SteamID)) _stats[victim.SteamID] = new PlayerStats();
                _stats[victim.SteamID].Deaths++;
            }
            
            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerHurt>((@event, info) =>
        {
            if (@event.Attacker == null || !@event.Attacker.IsValid || @event.Attacker.SteamID == 0) return HookResult.Continue;
            
            var attacker = @event.Attacker;
            if (!_stats.ContainsKey(attacker.SteamID)) _stats[attacker.SteamID] = new PlayerStats();
            
            _stats[attacker.SteamID].DamageDealt += @event.DmgHealth;
            
            return HookResult.Continue;
        });

        RegisterEventHandler<EventRoundStart>((@event, info) =>
        {
            _stats.Clear();
            return HookResult.Continue;
        });
    }

}

public sealed class RoundStatsConfig : WysteriaConfig
{
    public int KeepRounds { get; set; } = 10;
    public bool ShowToPlayerOnly { get; set; } = true;
}
