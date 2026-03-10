using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Fish.Data;
using Fish.Modules;
using Fish.Services;
using Fish.Repositories;

namespace Fish.Extension;


public static class Extension
{
    private const string ConnectionString = "Data Source=mrfish.db";

    public static DiscordClient AddSlashCommands(this DiscordClient discordClient, ServiceProvider services)
    {
        var slash = discordClient.UseSlashCommands(
            new SlashCommandsConfiguration
            {
                Services = services,
            }
        );

        slash.RegisterCommands<FishModule>();
        slash.RegisterCommands<HelpModule>();
        slash.RegisterCommands<LeaderboardModule>();
        slash.RegisterCommands<LoserboardModule>();

        return discordClient;
    }

    public static ServiceCollection AddServices(this ServiceCollection services)
    {
        services.AddScoped<FishingService>();
        services.AddSingleton<FishingData>();

        services.AddScoped<LeaderboardRepository>(_ => new LeaderboardRepository(ConnectionString));

        return services;
    }

    public static void AddDatabase()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        var createTable = connection.CreateCommand();
        createTable.CommandText = """
                CREATE TABLE IF NOT EXISTS leaderboard (
                    user_id     INTEGER NOT NULL,
                    fish_name   TEXT NOT NULL,
                    adjective   TEXT NOT NULL,
                    weight_kg   REAL NOT NULL,
                    points      REAL NOT NULL,
                    rarity      INTEGER NOT NULL,
                    is_special  INTEGER NOT NULL,
                    caught_at   TEXT NOT NULL
                )
                """;
        createTable.ExecuteNonQuery();
    }
}
