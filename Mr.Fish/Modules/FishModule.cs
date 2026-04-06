using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fish.Services;
using Fish.Repositories;

namespace Fish.Modules;

public class FishModule(
    FishingService fishingService,
    LeaderboardRepository leaderboardRepository,
    EconomyRepository economyRepository,
    EconomyService economyService)
    : ApplicationCommandModule
{
    [SlashCommand("fish", "Поймать рыбов!")]
    [SlashCooldown(1, 3, SlashCooldownBucketType.User)]
    public async Task FishCommand(InteractionContext ctx)
    {
        var economyProfile = await economyRepository.GetOrCreate(ctx.User.Id);
        var currentRod = economyService.GetCurrentRodOrDefault(economyProfile.RodTier);

        var (fish, description, isFishOfTheDay) = fishingService.RollFish(ctx.User.Id, currentRod.LuckBonus);
        await leaderboardRepository.Save(fish);

        int earnedScales = economyService.CalculateScalesReward(fish.Points);
        var updatedEconomy = await economyRepository.AddScales(ctx.User.Id, earnedScales);

        string fishName = fish.IsSpecial ? $"★ {fish.FishName} ★" : fish.FishName;
        string fishOfTheDayBonus = isFishOfTheDay ? "\n**Бонус рыбы дня:** `x1.5`" : "";

        string reply = $"**{fish.AdjectiveName} {fishName}**\n\n" +
                       $"{description} \n\n" +
                       $"**Вес:** `{fish.WeightKg:F6} кг`\n" +
                       $"**Редкость:** `{fish.Rarity}` \n" +
                       $"**Очки:** `{fish.Points:F2}`{fishOfTheDayBonus}\n" +
                       $"**Чешуйки:** `+{earnedScales}` (Баланс: `{updatedEconomy.Scales}`)\n" +
                       $"**Удочка:** `{currentRod.Name}` (`Удача +{currentRod.LuckBonus}`)";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(reply).AsEphemeral(true));
    }
}
