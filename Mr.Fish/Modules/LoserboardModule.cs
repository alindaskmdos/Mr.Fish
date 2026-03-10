using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fish.Repositories;
using Fish.Models.Enums;
using System.Diagnostics;


namespace Fish.Modules;

public class LoserboardModule(LeaderboardRepository leaderboardRepository)
    : ApplicationCommandModule
{
    [SlashCommand("loserboard", "топ 10 рыбов")]
    [SlashCooldown(1, 5, SlashCooldownBucketType.User)]
    public async Task LoserboardCommand(
        InteractionContext ctx,
        [Option("type", "Тип лидерборда")] LeaderboardType type = LeaderboardType.Points
    )
    {
        var top = (type) switch
        {
            (LeaderboardType.Points) => await leaderboardRepository.GetTop10WorstByPoints(),
            (LeaderboardType.Weight) => await leaderboardRepository.GetTop10WorstByWeight(),
            _ => await leaderboardRepository.GetTop10WorstByPoints(),
        };

        if (top.Count == 0)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("🎣 Лидерборд пуст — никто ещё не рыбачил!"));
            return;
        }

        var medals = new[] { "🥇", "🥈", "🥉" };
        var lines = new List<string>();

        for (int i = 0; i < top.Count; i++)
        {
            var fish = top[i];
            string place = i < 3 ? medals[i] : $"`#{i + 1}`";
            string fishName = fish.IsSpecial
                ? $"★ {fish.AdjectiveName} {fish.FishName} ★"
                : $"{fish.AdjectiveName} {fish.FishName}";

            string userName;
            try
            {
                var member = await ctx.Guild.GetMemberAsync(fish.UserId);
                userName = member.DisplayName;
            }
            catch
            {
                userName = $"<@{fish.UserId}>";
            }

            lines.Add($"{place} **{userName}** — {fishName} · `{fish.Points:F2} очков` · {fish.WeightKg:F2} кг");
        }

        string reply = $"🏆 **Топ рыбаков**\n\n{string.Join("\n", lines)}";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(reply));
    }
}
