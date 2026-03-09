using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fish.Repositories;


namespace Fish.Modules;

public class LeaderboardModule(LeaderboardRepository leaderboardRepository)
    : ApplicationCommandModule
{
    [SlashCommand("leaderboard", "топ 10 рыбов")]
    [SlashCooldown(1, 5, SlashCooldownBucketType.User)]
    public async Task LeaderboardCommand(InteractionContext ctx)
    {
        var top = await leaderboardRepository.GetTop10Best();

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

            lines.Add($"{place} **{userName}** — {fishName} · `{fish.Points} очков` · {fish.WeightKg:F2} кг");
        }

        string reply = $"🏆 **Топ рыбаков**\n\n{string.Join("\n", lines)}";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(reply));
    }
}
