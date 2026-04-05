using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fish.Services;

namespace Fish.Modules;

public class FishOfTheDayModule(FishingService fishingService)
    : ApplicationCommandModule
{
    [SlashCommand("fishofday", "Показать рыбу дня")]
    [SlashCooldown(1, 3, SlashCooldownBucketType.User)]
    public async Task FishOfTheDayCommand(InteractionContext ctx)
    {
        var fishOfTheDay = fishingService.GetFishOfTheDay();
        var now = DateTime.UtcNow;
        var timeLeft = now.Date.AddDays(1) - now;

        string reply = $"**Рыба дня:** `{fishOfTheDay.Name}`\n\n" +
                       $"**Редкость:** `{fishOfTheDay.Rarity}`\n" +
                       "**Бонус за поимку сегодня:** `x1.5` к очкам\n" +
                       $"**Обновится через:** `{timeLeft:hh\\:mm\\:ss}` (UTC)";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(reply).AsEphemeral(true));
    }
}
