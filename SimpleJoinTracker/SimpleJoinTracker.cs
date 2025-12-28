/*
 * SimpleJoinTracker - Simple Connection Tracking & Ranking System
 * * Author: SpawnTerror
 * Version: 1.0.0
 * Framework: CounterStrikeSharp
 * * Description:
 * Tracks player connections using SQLite.
 * Assigns ranks based on connection count.
 * Use config file for custom ranks.
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
    public bool IsRainbow { get; set; } = false; // NEW: Enable for fancy effect
}

// 3. MAIN PLUGIN
public class SimpleJoinTracker : BasePlugin
{
    public override string ModuleName => "SimpleJoinTracker";
    public override string ModuleVersion => "5.1.0";
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
            if (count >= rank.MinCount)
            {
                return rank;
            }
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
                    // 0 - 19
                    new() { MinCount = 0, Title = "KZ Nub", Color = "{grey}" },
                    
                    // SPACEBAR WARRIOR (White)
                    new() { MinCount = 20, Title = "Spacebar Warrior I", Color = "{white}" },
                    new() { MinCount = 30, Title = "Spacebar Warrior II", Color = "{white}" },
                    new() { MinCount = 40, Title = "Spacebar Warrior III", Color = "{white}" },
                    new() { MinCount = 50, Title = "Spacebar Warrior Elite", Color = "{white}" },

                    // CHECKPOINT SPAMMER (LightBlue)
                    new() { MinCount = 65, Title = "Checkpoint Spammer I", Color = "{lightblue}" },
                    new() { MinCount = 80, Title = "Checkpoint Spammer II", Color = "{lightblue}" },
                    new() { MinCount = 95, Title = "Checkpoint Spammer III", Color = "{lightblue}" },
                    new() { MinCount = 110, Title = "Checkpoint Spammer Elite", Color = "{lightblue}" },

                    // PRE-STRAFE BOT (Blue)
                    new() { MinCount = 130, Title = "Pre-Strafe Bot I", Color = "{blue}" },
                    new() { MinCount = 150, Title = "Pre-Strafe Bot II", Color = "{blue}" },
                    new() { MinCount = 170, Title = "Pre-Strafe Bot III", Color = "{blue}" },
                    new() { MinCount = 190, Title = "Pre-Strafe Bot Elite", Color = "{blue}" },

                    // LJ GRINDER (Olive)
                    new() { MinCount = 220, Title = "LJ Grinder I", Color = "{olive}" },
                    new() { MinCount = 250, Title = "LJ Grinder II", Color = "{olive}" },
                    new() { MinCount = 280, Title = "LJ Grinder III", Color = "{olive}" },
                    new() { MinCount = 310, Title = "LJ Grinder Elite", Color = "{olive}" },

                    // BUNNYHOPPER (Lime)
                    new() { MinCount = 350, Title = "Bunnyhopper I", Color = "{lime}" },
                    new() { MinCount = 400, Title = "Bunnyhopper II", Color = "{lime}" },
                    new() { MinCount = 450, Title = "Bunnyhopper III", Color = "{lime}" },
                    new() { MinCount = 500, Title = "Bunnyhopper Elite", Color = "{lime}" },

                    // TECH HUNTER (Purple)
                    new() { MinCount = 600, Title = "Tech Hunter I", Color = "{purple}" },
                    new() { MinCount = 700, Title = "Tech Hunter II", Color = "{purple}" },
                    new() { MinCount = 800, Title = "Tech Hunter III", Color = "{purple}" },
                    new() { MinCount = 900, Title = "Tech Hunter Elite", Color = "{purple}" },

                    // STRAFE GOD (Red)
                    new() { MinCount = 1000, Title = "Strafe God I", Color = "{red}" },
                    new() { MinCount = 1150, Title = "Strafe God II", Color = "{red}" },
                    new() { MinCount = 1300, Title = "Strafe God III", Color = "{red}" },
                    new() { MinCount = 1450, Title = "Strafe God Elite", Color = "{red}" },

                    // MOVEMENT LEGEND (Gold)
                    new() { MinCount = 1600, Title = "Movement Legend I", Color = "{gold}" },
                    new() { MinCount = 1800, Title = "Movement Legend II", Color = "{gold}" },
                    new() { MinCount = 2000, Title = "Movement Legend III", Color = "{gold}" },
                    new() { MinCount = 2200, Title = "Movement Legend Elite", Color = "{gold}" },

                    // HYPERION GOD (Dark Red)
                    new() { MinCount = 2500, Title = "Hyperion God", Color = "{darkred}" },

                    // AUTOSTRAFER (Rainbow as per Hyperion's Idea)
                    new() { MinCount = 3000, Title = "Autostrafer", Color = "{white}", IsRainbow = true }
                };

                string json = JsonSerializer.Serialize(Ranks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(RanksPath, json);
            }

            Ranks = Ranks.OrderByDescending(r => r.MinCount).ToList();
        }
        catch (Exception ex)
        {
            Ranks = new List<RankDefinition> { new() { MinCount = 0, Title = "Error Loading Ranks", Color = "{grey}" } };
            Console.WriteLine($"[SimpleJoinTracker] Rank Load Error: {ex.Message}");
        }
    }

    // --- RAINBOW GENERATOR ---
    private string GenerateRainbowText(string text)
    {
        string[] colors = { 
            ChatColors.Red.ToString(), 
            ChatColors.Gold.ToString(), 
            ChatColors.Lime.ToString(), 
            ChatColors.Blue.ToString(), 
            ChatColors.Magenta.ToString(),
            ChatColors.Purple.ToString()
        };

        StringBuilder sb = new StringBuilder();
        int colorIndex = 0;

        foreach (char c in text)
        {
            if (c == ' ')
            {
                sb.Append(' ');
                continue;
            }
            
            sb.Append(colors[colorIndex % colors.Length]);
            sb.Append(c);
            colorIndex++;
        }
        return sb.ToString();
    }

    // --- PLAYER JOIN LOGIC --- // Removed checks for map changes
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
            command.CommandText = @"
                INSERT INTO player_stats (steamid, join_count) 
                VALUES (@steamid, 1)
                ON CONFLICT(steamid) 
                DO UPDATE SET join_count = join_count + 1 
                RETURNING join_count;
            ";
            command.Parameters.AddWithValue("@steamid", (long)steamId);
            
            var result = await command.ExecuteScalarAsync();
            if (result != null) count = Convert.ToInt32(result);

            // Get Rank
            var rankDef = GetRank(count);
            string countStr = GetOrdinal(count);

            Server.NextFrame(() => 
            {
                if (player == null || !player.IsValid) return;

                AddTimer(1.0f, () => 
                {
                    if (player == null || !player.IsValid) return;

                    string prefix = ReplaceColors(Config.ServerNamePrefix);
                    
                    // HANDLE RANK COLOR (Rainbow vs Normal)
                    string finalRankTitle;
                    if (rankDef.IsRainbow)
                    {
                        finalRankTitle = GenerateRainbowText(rankDef.Title);
                    }
                    else
                    {
                        finalRankTitle = ReplaceColors(rankDef.Color) + rankDef.Title;
                    }

                    string nameColor = ChatColors.Red.ToString();
                    string countColor = ChatColors.Lime.ToString();

                    Server.PrintToChatAll($" {prefix} {finalRankTitle} {nameColor}{player.PlayerName} {ChatColors.Default}connected for the {countColor}{countStr} {ChatColors.Default}time!");
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
            if (File.Exists(ConfigPath))
            {
                string json = File.ReadAllText(ConfigPath);
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                Config = JsonSerializer.Deserialize<SimpleJoinTrackerConfig>(json, options) ?? new();
            }
            else
            {
                Config = new SimpleJoinTrackerConfig();
                string json = JsonSerializer.Serialize(Config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigPath, json);
            }
        }
        catch { Config = new SimpleJoinTrackerConfig(); }
    }

    private string ReplaceColors(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        try 
        {
            return input
                .Replace("{default}", ChatColors.Default.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{white}", ChatColors.White.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{darkred}", ChatColors.DarkRed.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{green}", ChatColors.Green.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{lime}", ChatColors.Lime.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{red}", ChatColors.Red.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{blue}", ChatColors.Blue.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{gold}", ChatColors.Gold.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{yellow}", ChatColors.Yellow.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{silver}", ChatColors.Silver.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{grey}", ChatColors.Grey.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{purple}", ChatColors.Purple.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{olive}", ChatColors.Olive.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{magenta}", ChatColors.Magenta.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{lightred}", ChatColors.LightRed.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{lightblue}", ChatColors.LightBlue.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        catch { return input; }
    }
}