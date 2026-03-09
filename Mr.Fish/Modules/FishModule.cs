using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fish.Services;
using Fish.Repositories;

namespace Fish.Modules;

public class FishModule(FishingService fishingService, LeaderboardRepository leaderboardRepository)
    : ApplicationCommandModule
{
    [SlashCommand("fish", "Поймать рыбов!")]
    [SlashCooldown(1, 3, SlashCooldownBucketType.User)]
    public async Task FishCommand(InteractionContext ctx)
    {
        var (fish, description) = fishingService.RollFish(ctx.User.Id);
        await leaderboardRepository.Save(fish);

        string fishName = fish.IsSpecial ? $"★ {fish.FishName} ★" : fish.FishName;

        string reply = $"**{fish.AdjectiveName} {fishName}**\n\n" +
                       $"{description} \n\n" +
                       $"**Вес:** `{fish.WeightKg} кг`\n" +
                       $"**Редкость:** `{fish.Rarity}` \n" +
                       $"**Очки:** `{fish.Points}`";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(reply).AsEphemeral(true));
    }
}
