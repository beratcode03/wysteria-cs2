using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace Wysteria.Core;

public sealed class RealJailbreakAdapter : IJailbreakAdapter, IEconomyAdapter
{
    private readonly Dictionary<ulong, int> _credits = new();
    private readonly Dictionary<uint, CHandle<CBaseEntity>> _markers = new();
    private readonly string _creditsFilePath;

    public bool IsAvailable => true;

    public EconomyPurchaseResult TryPurchase(EconomyPurchaseRequest request)
    {
        if (_credits.TryGetValue(request.PlayerId, out var bal) && bal >= request.Cost)
        {
            _credits[request.PlayerId] -= request.Cost;
            SaveCredits();
            return EconomyPurchaseResult.Success("Purchase successful.");
        }
        return EconomyPurchaseResult.InsufficientFunds("Insufficient credits.");
    }

    public RealJailbreakAdapter()
    {
        var dir = Path.Combine(Server.GameDirectory, "addons", "counterstrikesharp", "data", "wysteria");
        Directory.CreateDirectory(dir);
        _creditsFilePath = Path.Combine(dir, "credits.json");
        LoadCredits();
    }

    private void LoadCredits()
    {
        if (File.Exists(_creditsFilePath))
        {
            try
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(_creditsFilePath));
                if (data != null)
                {
                    foreach (var kvp in data)
                    {
                        if (ulong.TryParse(kvp.Key, out ulong steamId))
                            _credits[steamId] = kvp.Value;
                    }
                }
            }
            catch { }
        }
    }

    public void SaveCredits()
    {
        try
        {
            var stringDict = _credits.ToDictionary(k => k.Key.ToString(), v => v.Value);
            File.WriteAllText(_creditsFilePath, JsonSerializer.Serialize(stringDict, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    public bool AreCellsOpen() => WysteriaRound.CellsOpen;

    public void OpenCells()
    {
        WysteriaRound.OpenCells();
        Server.ExecuteCommand("ent_fire func_door open");
        Server.ExecuteCommand("ent_fire func_movelinear open");
        Server.ExecuteCommand("ent_fire prop_door_rotating open");
    }

    public void CloseCells()
    {
        WysteriaRound.CloseCells();
        Server.ExecuteCommand("ent_fire func_door close");
        Server.ExecuteCommand("ent_fire func_movelinear close");
        Server.ExecuteCommand("ent_fire prop_door_rotating close");
    }

    public void LockDoors(float duration)
    {
        Server.ExecuteCommand("ent_fire func_door lock");
        Server.ExecuteCommand("ent_fire prop_door_rotating lock");
    }

    public void UnlockDoors()
    {
        Server.ExecuteCommand("ent_fire func_door unlock");
        Server.ExecuteCommand("ent_fire prop_door_rotating unlock");
    }

    public void DamageDoor(CBaseEntity door, int damage)
    {
        // Source 2 entities take damage via AcceptInput or CTakeDamageInfo which is complex in C#.
        // We can just try to open or break it if health reaches 0.
        if (door != null && door.IsValid)
        {
            if (door.Health > 0)
            {
                door.Health -= damage;
                if (door.Health <= 0)
                {
                    door.AcceptInput("Break");
                    door.AcceptInput("Kill");
                }
                else
                {
                    Utilities.SetStateChanged(door, "CBaseEntity", "m_iHealth");
                }
            }
        }
    }

    public int GetPlayerCredits(CCSPlayerController player)
    {
        if (player == null || !player.IsValid) return 0;
        return _credits.TryGetValue(player.SteamID, out var c) ? c : 0;
    }

    public void AddPlayerCredits(CCSPlayerController player, int amount)
    {
        if (player == null || !player.IsValid) return;
        var cur = GetPlayerCredits(player);
        _credits[player.SteamID] = cur + amount;
        SaveCredits();
    }

    public void SetPlayerCredits(CCSPlayerController player, int amount)
    {
        if (player == null || !player.IsValid) return;
        _credits[player.SteamID] = amount;
        SaveCredits();
    }

    public bool HasContraband(CCSPlayerController player)
    {
        if (player == null) return false;
        var pawn = player.PlayerPawn?.Value;
        if (pawn == null || !pawn.IsValid || !player.PawnIsAlive) return false;

        var weapons = pawn.WeaponServices?.MyWeapons;
        if (weapons == null) return false;

        foreach (var w in weapons)
        {
            var weapon = w.Value;
            if (weapon == null || !weapon.IsValid) continue;
            var name = weapon.DesignerName;
            if (name.Contains("knife") || name.Contains("bayonet")) continue;
            if (name.Contains("fists") || name.Contains("melee")) continue;
            
            // If it's a primary, secondary or grenade, it's contraband for prisoners.
            return true;
        }
        return false;
    }

    public bool IsWarden(CCSPlayerController player) => WardenService.IsWarden(player);

    public void SetWarden(CCSPlayerController? player)
    {
        WardenService.Reset();
        if (player != null && player.IsValid)
        {
            WardenService.Claim(player);
        }
    }

    public void PlaySound(CCSPlayerController? target, string sound)
    {
        if (target != null && target.IsValid)
        {
            target.ExecuteClientCommand($"play {sound}");
        }
        else
        {
            Server.ExecuteCommand($"play {sound}");
        }
    }

    public void PrintAlert(CCSPlayerController? target, string message)
    {
        if (target != null && target.IsValid)
            WysteriaMessages.Center(target, message);
    }

    public string GetActiveMapName() => Server.MapName ?? string.Empty;

    public IEnumerable<CBaseEntity> GetMotorizedDoors()
    {
        var doors = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_door").ToList();
        doors.AddRange(Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("func_movelinear"));
        return doors;
    }

    public void MarkPlayer(CCSPlayerController player, string icon, float duration)
    {
        if (player == null || !player.IsValid || !player.PawnIsAlive) return;
        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;

        UnmarkPlayer(player);

        var glow = Utilities.CreateEntityByName<CBaseEntity>("env_sprite");
        if (glow != null)
        {
            glow.Teleport(new Vector(pawn.AbsOrigin!.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + 80), new QAngle(0,0,0), null);
            glow.DispatchSpawn();
            Server.ExecuteCommand($"ent_fire {glow.DesignerName} setparent !activator");
            Server.ExecuteCommand($"ent_fire {glow.DesignerName} showsprite");

            _markers[player.Index] = new CHandle<CBaseEntity>((nint)glow.Index);

            if (duration > 0)
            {
                // This doesn't actually bind to the timer exactly, but we rely on unmark.
                // We don't have direct access to AddTimer here since this isn't a plugin class.
                // But UnmarkPlayer cleans it up.
            }
        }
    }

    public void UnmarkPlayer(CCSPlayerController player)
    {
        if (player == null) return;
        if (_markers.TryGetValue(player.Index, out var handle))
        {
            if (handle.IsValid && handle.Value != null)
            {
                handle.Value.AcceptInput("Kill");
            }
            _markers.Remove(player.Index);
        }
    }
}
