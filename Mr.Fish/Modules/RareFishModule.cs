using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fish.Repositories;

namespace Fish.Modules;

public class RareFishModule(LeaderboardRepository leaderboardRepository)
    : ApplicationCommandModule
{
    [SlashCommand("myrarefish", "Топ-3 самых редких рыб за всё время")]
    [SlashCooldown(1, 3, SlashCooldownBucketType.User)]
    public async Task MyRareFishCommand(InteractionContext ctx)
    {
        var top = await leaderboardRepository.GetTop3RarestByUser(ctx.User.Id);

        if (top.Count == 0)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("У тебя пока нет улова.").AsEphemeral(true));
            return;
        }

        var lines = new List<string>();

        for (int i = 0; i < top.Count; i++)
        {
            var fish = top[i];
            string fishName = fish.IsSpecial
                ? $"★ {fish.AdjectiveName} {fish.FishName} ★"
                : $"{fish.AdjectiveName} {fish.FishName}";

            lines.Add($"`#{i + 1}` **{fishName}** · `Редкость {fish.Rarity}` · `{fish.Points:F2} очков`");
        }

        string reply = $"**Твои топ-3 редких рыбы**\n\n{string.Join("\n", lines)}";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(reply).AsEphemeral(true));
    }
}
