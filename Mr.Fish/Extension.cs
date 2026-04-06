using DSharpPlus;
using DSharpPlus.Entities;
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
        slash.RegisterCommands<RareFishModule>();
        slash.RegisterCommands<StatsModule>();
        slash.RegisterCommands<FishOfTheDayModule>();
        slash.RegisterCommands<ShopModule>();

        discordClient.ComponentInteractionCreated += async (_, e) =>
        {
            using var scope = services.CreateScope();
            var economyService = scope.ServiceProvider.GetRequiredService<EconomyService>();
            var economyRepository = scope.ServiceProvider.GetRequiredService<EconomyRepository>();

            if (economyService.TryParseBuyRodCustomId(e.Interaction.Data.CustomId, out int tier, out int page))
            {
                var rod = economyService.GetRodByTier(tier);
                if (rod is null)
                {
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                            .WithContent("Не удалось найти такую удочку.")
                            .AsEphemeral(true));
                    return;
                }

                var buyResult = await economyRepository.TryBuyRod(e.User.Id, rod);
                string statusMessage = buyResult.Success
                    ? $"Ты купил удочку: {rod.Name}. Остаток: {buyResult.Profile.Scales} чешуек."
                    : buyResult.Message;

                var builder = ShopModule.BuildShopResponse(economyService, buyResult.Profile, page, statusMessage);
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, builder);
                return;
            }

            if (economyService.TryParseShopPageCustomId(e.Interaction.Data.CustomId, out int requestedPage))
            {
                var profile = await economyRepository.GetOrCreate(e.User.Id);
                var builder = ShopModule.BuildShopResponse(economyService, profile, requestedPage);
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, builder);
                return;
            }
        };

        return discordClient;
    }

    public static ServiceCollection AddServices(this ServiceCollection services)
    {
        services.AddScoped<FishingService>();
        services.AddScoped<EconomyService>();
        services.AddSingleton<FishingData>();

        services.AddScoped<LeaderboardRepository>(_ => new LeaderboardRepository(ConnectionString));
        services.AddScoped<EconomyRepository>(_ => new EconomyRepository(ConnectionString));

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

        var createEconomyTable = connection.CreateCommand();
        createEconomyTable.CommandText = """
                CREATE TABLE IF NOT EXISTS user_economy (
                    user_id     INTEGER PRIMARY KEY,
                    scales      INTEGER NOT NULL DEFAULT 0,
                    rod_tier    INTEGER NOT NULL DEFAULT 0
                )
                """;
        createEconomyTable.ExecuteNonQuery();
    }
}
