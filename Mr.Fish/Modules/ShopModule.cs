using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Fish.Models;
using Fish.Repositories;
using Fish.Services;

namespace Fish.Modules;

public class ShopModule(EconomyRepository economyRepository, EconomyService economyService)
    : ApplicationCommandModule
{
    private const int RodsPerPage = 5;

    [SlashCommand("wallet", "Показать баланс и текущую удочку")]
    [SlashCooldown(1, 3, SlashCooldownBucketType.User)]
    public async Task WalletCommand(InteractionContext ctx)
    {
        var profile = await economyRepository.GetOrCreate(ctx.User.Id);
        var currentRod = economyService.GetCurrentRodOrDefault(profile.RodTier);

        string reply = "**Кошелек рыбака**\n\n" +
                       $"**Чешуйки:** `{profile.Scales}`\n" +
                       $"**Текущая удочка:** `{currentRod.Name}`\n" +
                       $"**Бонус удачи:** `+{currentRod.LuckBonus}`";

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(reply).AsEphemeral(true));
    }

    [SlashCommand("shop", "Магазин удочек")]
    [SlashCooldown(1, 3, SlashCooldownBucketType.User)]
    public async Task ShopCommand(InteractionContext ctx)
    {
        var profile = await economyRepository.GetOrCreate(ctx.User.Id);
        var rods = economyService.GetAllRods().OrderBy(r => r.Tier).ToList();
        int currentRodIndex = rods.FindIndex(r => r.Tier == profile.RodTier);
        if (currentRodIndex < 0)
            currentRodIndex = 0;

        int startPage = currentRodIndex / RodsPerPage;
        var builder = BuildShopResponse(economyService, profile, startPage);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
    }

    public static DiscordInteractionResponseBuilder BuildShopResponse(
        EconomyService economyService,
        UserEconomy profile,
        int requestedPage,
        string? statusMessage = null)
    {
        var rods = economyService.GetAllRods().OrderBy(r => r.Tier).ToList();
        var currentRod = economyService.GetCurrentRodOrDefault(profile.RodTier);

        int totalPages = Math.Max(1, (int)Math.Ceiling(rods.Count / (double)RodsPerPage));
        int page = Math.Clamp(requestedPage, 0, totalPages - 1);

        var pageRods = rods
            .Skip(page * RodsPerPage)
            .Take(RodsPerPage)
            .ToList();

        var lines = new List<string>();
        foreach (var rod in pageRods)
        {
            string state = rod.Tier switch
            {
                _ when rod.Tier == profile.RodTier => "текущая",
                _ when rod.Tier < profile.RodTier => "куплено",
                _ => $"цена: {rod.Price} чешуек"
            };

            lines.Add($"`T{rod.Tier}` **{rod.Name}** · `Удача +{rod.LuckBonus}` · `{state}`");
        }

        string statusBlock = string.IsNullOrWhiteSpace(statusMessage)
            ? ""
            : $"**Статус:** {statusMessage}\n\n";

        string content = "**Магазин удочек**\n\n" +
                         statusBlock +
                         $"**Твой баланс:** `{profile.Scales} чешуек`\n" +
                         $"**Сейчас экипировано:** `{currentRod.Name}`\n" +
                         $"**Страница:** `{page + 1}/{totalPages}`\n\n" +
                         string.Join("\n", lines) +
                         "\n\nНажми кнопку нужного тира для покупки.";

        var builder = new DiscordInteractionResponseBuilder()
            .WithContent(content)
            .AsEphemeral(true);

        var buyButtons = new List<DiscordComponent>();
        foreach (var rod in pageRods)
        {
            bool isBaseRod = rod.Tier == 0;
            bool disabled = isBaseRod || rod.Tier <= profile.RodTier;

            var style = rod.Tier == profile.RodTier || isBaseRod
                ? ButtonStyle.Secondary
                : ButtonStyle.Primary;

            string label = isBaseRod
                ? "База T0"
                : disabled
                    ? (rod.Tier == profile.RodTier ? $"Текущая T{rod.Tier}" : $"Куплено T{rod.Tier}")
                    : $"Купить T{rod.Tier}";

            buyButtons.Add(new DiscordButtonComponent(
                style,
                economyService.BuildBuyRodCustomId(rod.Tier, page),
                label,
                disabled));
        }

        if (buyButtons.Count > 0)
            builder.AddComponents(buyButtons);

        builder.AddComponents(
        [
            new DiscordButtonComponent(
                ButtonStyle.Secondary,
                economyService.BuildShopPageCustomId(page - 1),
                "Назад",
                page == 0
            ),
            new DiscordButtonComponent(
                ButtonStyle.Secondary,
                economyService.BuildShopPageCustomId(page + 1),
                "Вперед",
                page >= totalPages - 1
            )
        ]);

        return builder;
    }
}
