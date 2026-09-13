/* ==========================================================================
 * Wysteria Works - Jailbreak module.
 * Official Website: https://wysteriaworks.com
 * Powered by Wysteria Framework (CS2 / CounterStrikeSharp)
 * ========================================================================== */

using System;
using System.Collections.Generic;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Wysteria.Core;

namespace Wysteria.PrisonJob;

public sealed class PrisonJobPlugin : WysteriaPlugin<PrisonJobConfig>
{
    static PrisonJobPlugin() => WysteriaCoreGuard.Require();
    private readonly Dictionary<ulong, int> _jobsThisRound = new();
    private int _jobsRound;
    private readonly object _jobLock = new();

    public override string ModuleName => "Wysteria.PrisonJob";
    public override string ModuleVersion => "1.6.3";
    public override string ModuleAuthor => "wisteriae";
    public override string ModuleDescription => "Wysteria Works - Prison jobs and credit rewards.";

    public override void Load(bool hotReload)
    {
        LoadWysteriaConfig("Wysteria.PrisonJob.json");
        AddCommand("css_job", "Hücre çevresindeki iş noktalarından kredi kazanma.", OnJobCommand);
        AddCommand("css_wysteria_job", "Hücre çevresindeki iş noktalarından kredi kazanma.", OnJobCommand);
    }

    private void OnJobCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid) return;
        if (!Config.Enabled || !IsMapAllowed()) return;
        
        if (!WysteriaRound.IsLive) 
        { 
            WysteriaMessages.Local(player, "Bu özellik yalnızca canlı round sırasında kullanılabilir."); 
            return; 
        }

        if (!IsAlive(player))
        {
            WysteriaMessages.Local(player, "Bu komutu kullanmak için hayatta olmalısın.");
            return;
        }

        if (!IsPrisoner(player)) 
        {
            WysteriaMessages.Local(player, "Bu komutu yalnızca mahkûmlar kullanabilir.");
            return;
        }

        if (!Permit(info, p => true, "", Config.GlobalCooldown)) return;

        // Check Zone Distance
        var origin = player.PlayerPawn?.Value?.AbsOrigin;
        if (origin == null) return;
        
        float distSq = WysteriaGeometry.DistanceSquared(
            origin, 
            new Vector(Config.JobZoneCenterX, Config.JobZoneCenterY, Config.JobZoneCenterZ)
        );

        if (distSq > Config.JobZoneRadius * Config.JobZoneRadius)
        {
            WysteriaMessages.Local(player, "Çalışmak için iş alanının (hücre bölgesi) içinde olmalısın.");
            return;
        }

        lock (_jobLock)
        {
            if (_jobsRound != WysteriaRound.RoundNumber) 
            { 
                _jobsThisRound.Clear(); 
                _jobsRound = WysteriaRound.RoundNumber; 
            }
            
            var completedJobs = _jobsThisRound.GetValueOrDefault(player.SteamID);
            if (completedJobs >= Config.MaxJobsPerRound) 
            { 
                WysteriaMessages.Local(player, "Bu round iş limitine ulaştın."); 
                return; 
            }
            
            _jobsThisRound[player.SteamID] = completedJobs + 1;
        }

        WysteriaMessages.Center(player, $"İş başladı; {Config.JobDuration:0.0} saniye bekle.");
        var worker = player;
        
        AddTimer(Config.JobDuration, () => {
            if (IsAlive(worker))
            {
                // Make sure they are still in the zone when job finishes
                var currentOrigin = worker.PlayerPawn?.Value?.AbsOrigin;
                if (currentOrigin != null)
                {
                    float currentDistSq = WysteriaGeometry.DistanceSquared(
                        currentOrigin, 
                        new Vector(Config.JobZoneCenterX, Config.JobZoneCenterY, Config.JobZoneCenterZ)
                    );
                    
                    if (currentDistSq <= Config.JobZoneRadius * Config.JobZoneRadius)
                    {
                        Adapter.AddPlayerCredits(worker, Config.CreditsReward);
                        WysteriaMessages.Local(worker, $"İş tamamlandı. {Config.CreditsReward} kredi kazandın.");
                    }
                    else
                    {
                        WysteriaMessages.Local(worker, "İş alanından ayrıldığın için iş iptal edildi!");
                    }
                }
            }
        });
    }
}

public sealed class PrisonJobConfig : WysteriaConfig
{
    public int CreditsReward { get; set; } = 50;
    public int MaxJobsPerRound { get; set; } = 2;
    public float JobDuration { get; set; } = 5.0f;
    public float JobZoneCenterX { get; set; } = 0.0f;
    public float JobZoneCenterY { get; set; } = 0.0f;
    public float JobZoneCenterZ { get; set; } = 0.0f;
    public float JobZoneRadius { get; set; } = 500.0f;
}
