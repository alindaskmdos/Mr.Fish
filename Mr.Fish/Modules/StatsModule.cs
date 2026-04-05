using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fish.Repositories;

namespace Fish.Modules;

public class StatsModule(LeaderboardRepository leaderboardRepository)
    : ApplicationCommandModule
{
    [SlashCommand("mystats", "Личная статистика рыбалки")]
    [SlashCooldown(1, 3, SlashCooldownBucketType.User)]
    public async Task MyStatsCommand(InteractionContext ctx)
    {
        var stats = await leaderboardRepository.GetUserStats(ctx.User.Id);

        if (stats is null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("У тебя пока нет статистики, сначала поймай рыбку через /fish.").AsEphemeral(true));
            return;
        }

        string reply = "**Твоя статистика**\n\n" +
                       $"**Всего уловов:** `{stats.TotalCatches}`\n" +
                       $"**Общий вес:** `{stats.TotalWeightKg:F2} кг`\n" +
                       $"**Средний вес:** `{stats.AverageWeightKg:F2} кг`\n" +
                       $"**Сумма очков:** `{stats.TotalPoints:F2}`\n" +
                       $"**Лучший улов по очкам:** `{stats.BestPoints:F2}` ({stats.BestFishName})\n" +
                       $"**Максимальная редкость:** `{stats.MaxRarity}` ({stats.RarestFishName})";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(reply).AsEphemeral(true));
    }
}
