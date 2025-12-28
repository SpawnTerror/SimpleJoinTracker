/*
 * SimpleJoinTracker - Advanced Connection Tracking & Ranking System for CS2
 * * Author: SpawnTerror
 * Version: 1.0.0
 * Framework: CounterStrikeSharp
 * * Description:
 * Tracks player connections using SQLite.
 * assign ranks based on connection count.
 * Supports custom colors, Hex colors, and Rainbow animations.
 * * GitHub: https://github.com/SpawnTerror/SimpleJoinTracker
 */

using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleJoinTrackerPlugin;

// 1. MAIN CONFIG
public class SimpleJoinTrackerConfig
{
    public string ServerNamePrefix { get; set; } = "{lime}HYPERION KZ |"; 
}

// 2. RANK STRUCTURE
public class RankDefinition
{
    public int MinCount { get; set; }
    public string Title { get; set; } = "";
    public string Color { get; set; } = "{white}";
    public bool IsRainbow { get; set; } = false;
}

// 3. MAIN PLUGIN
public class SimpleJoinTracker : BasePlugin
{
    public override string ModuleName => "SimpleJoinTracker";
    public override string ModuleVersion => "1.0.0"; // Reset to 1.0 for Release
    public override string ModuleAuthor => "SpawnTerror";

    private string DbPath => Path.Combine(ModuleDirectory, "player_data.db");
    private string ConfigPath => Path.Combine(ModuleDirectory, "config.json");
    private string RanksPath => Path.Combine(ModuleDirectory, "ranks.json");
    
    public SimpleJoinTrackerConfig Config { get; set; } = new();
    public List<RankDefinition> Ranks { get; set; } = new();

    public override void Load(bool hotReload)
    {
        LoadConfig();
        LoadRanks();
        InitDatabase();
        RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull);
    }

    // --- RANK LOGIC ---
    private RankDefinition GetRank(int count)
    {
        foreach (var rank in Ranks)
        {
            if (count >= rank.MinCount) return rank;
        }
        return new RankDefinition { Title = "KZ Nub", Color = "{grey}" };
    }

    private void LoadRanks()
    {
        try
        {
            if (File.Exists(RanksPath))
            {
                string json = File.ReadAllText(RanksPath);
                Ranks = JsonSerializer.Deserialize<List<RankDefinition>>(json) ?? new();
            }
            else
            {
                // GENERATE DEFAULTS
                Ranks = new List<RankDefinition>
                {
                    new() { MinCount = 0, Title = "KZ Nub", Color = "{grey}" },
                    new() { MinCount = 20, Title = "Spacebar Warrior I", Color = "{white}" },
                    new() { MinCount = 50, Title = "Spacebar Warrior Elite", Color = "{white}" },
                    new() { MinCount = 100, Title = "Pre-Strafe Bot", Color = "{blue}" },
                    new() { MinCount = 500, Title = "Bunnyhopper Elite", Color = "{lime}" },
                    new() { MinCount = 1000, Title = "Strafe God", Color = "{red}" },
                    new() { MinCount = 2500, Title = "Hyperion God", Color = "{darkred}" },
                    new() { MinCount = 3000, Title = "Autostrafer", Color = "{white}", IsRainbow = true }
                };
                string json = JsonSerializer.Serialize(Ranks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(RanksPath, json);
            }
            Ranks = Ranks.OrderByDescending(r => r.MinCount).ToList();
        }
        catch (Exception ex)
        {
            Ranks = new List<RankDefinition> { new() { MinCount = 0, Title = "Error", Color = "{grey}" } };
            Console.WriteLine($"[SimpleJoinTracker] Rank Load Error: {ex.Message}");
        }
    }

    private string GenerateRainbowText(string text)
    {
        string[] colors = { ChatColors.Red.ToString(), ChatColors.Gold.ToString(), ChatColors.Lime.ToString(), ChatColors.Blue.ToString(), ChatColors.Magenta.ToString(), ChatColors.Purple.ToString() };
        StringBuilder sb = new StringBuilder();
        int colorIndex = 0;
        foreach (char c in text)
        {
            if (c == ' ') { sb.Append(' '); continue; }
            sb.Append(colors[colorIndex % colors.Length]);
            sb.Append(c);
            colorIndex++;
        }
        return sb.ToString();
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid == null || !@event.Userid.IsValid || @event.Userid.IsBot) return HookResult.Continue;
        ulong steamId = @event.Userid.SteamID;
        var playerRef = @event.Userid;
        Task.Run(async () => await HandlePlayerJoin(steamId, playerRef));
        return HookResult.Continue;
    }

    private async Task HandlePlayerJoin(ulong steamId, CCSPlayerController player)
    {
        try
        {
            int count = 1;
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"INSERT INTO player_stats (steamid, join_count) VALUES (@steamid, 1) ON CONFLICT(steamid) DO UPDATE SET join_count = join_count + 1 RETURNING join_count;";
            command.Parameters.AddWithValue("@steamid", (long)steamId);
            var result = await command.ExecuteScalarAsync();
            if (result != null) count = Convert.ToInt32(result);

            var rankDef = GetRank(count);
            string countStr = GetOrdinal(count);

            Server.NextFrame(() => 
            {
                if (player == null || !player.IsValid) return;
                AddTimer(1.0f, () => 
                {
                    if (player == null || !player.IsValid) return;
                    string prefix = ReplaceColors(Config.ServerNamePrefix);
                    string finalRankTitle = rankDef.IsRainbow ? GenerateRainbowText(rankDef.Title) : ReplaceColors(rankDef.Color) + rankDef.Title;
                    
                    Server.PrintToChatAll($" {prefix} {finalRankTitle} {ChatColors.Red}{player.PlayerName} {ChatColors.Default}connected for the {ChatColors.Lime}{countStr} {ChatColors.Default}time!");
                });
            });
        }
        catch (Exception ex) { Console.WriteLine($"[SimpleJoinTracker] Error: {ex.Message}"); }
    }

    private string GetOrdinal(int number)
    {
        if (number % 100 >= 11 && number % 100 <= 13) return number + "th";
        return (number % 10) switch { 1 => number + "st", 2 => number + "nd", 3 => number + "rd", _ => number + "th" };
    }

    private void InitDatabase()
    {
        try
        {
            using var connection = new SqliteConnection($"Data Source={DbPath}");
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS player_stats ( steamid INTEGER PRIMARY KEY, join_count INTEGER DEFAULT 1 );";
            command.ExecuteNonQuery();
        }
        catch (Exception ex) { Logger.LogError($"[SimpleJoinTracker] DB Init Failed: {ex.Message}"); }
    }

    private void LoadConfig()
    {
        try
        {
            if (File.Exists(ConfigPath)) { string json = File.ReadAllText(ConfigPath); var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip }; Config = JsonSerializer.Deserialize<SimpleJoinTrackerConfig>(json, options) ?? new(); }
            else { Config = new SimpleJoinTrackerConfig(); string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true }); File.WriteAllText(ConfigPath, json); }
        }
        catch { Config = new SimpleJoinTrackerConfig(); }
    }

    private string ReplaceColors(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        try { return input.Replace("{default}", ChatColors.Default.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{white}", ChatColors.White.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{green}", ChatColors.Green.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{lime}", ChatColors.Lime.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{red}", ChatColors.Red.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{blue}", ChatColors.Blue.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{gold}", ChatColors.Gold.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{grey}", ChatColors.Grey.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{purple}", ChatColors.Purple.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{olive}", ChatColors.Olive.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{darkred}", ChatColors.DarkRed.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{lightblue}", ChatColors.LightBlue.ToString(), StringComparison.OrdinalIgnoreCase); }
        catch { return input; }
    }
}