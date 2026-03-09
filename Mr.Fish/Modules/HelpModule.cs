using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace Fish.Modules;

public class HelpModule : ApplicationCommandModule
{
    [SlashCommand("help", "Информация о боте и командах")]
    [SlashCooldown(1, 3, SlashCooldownBucketType.User)]
    public async Task HelpCommand(InteractionContext ctx)
    {
        var embed = new DiscordEmbedBuilder()
            .WithTitle("🎣 Mr. Fish — Гайд по рыбалке")
            .WithColor(new DiscordColor(0x3498db))
            .WithDescription("Добро пожаловать в Mr. Fish! Здесь ты можешь ловить рыбу, соревноваться с друзьями и попадать в топы. Вот что умеет бот:")
            .AddField("📜 Основные команды",
                """
                🎣 `/fish` — поймать рыбу и узнать, что тебе попалось
                🏆 `/leaderboard` — топ 10 самых удачных рыбаков
                🐟 `/looserboard` — топ 10 самых неудачных уловов
                ❓ `/help` — показать это сообщение
                """)
            .AddField("⚙️ Как проходит рыбалка",
                """
                Каждый бросок — это шанс поймать уникальную рыбу с разными характеристиками.
                Рыба получает случайное прилагательное, влияющее на итоговые очки.
                Чем ниже редкость — тем ценнее улов!
                """)
            .AddField("⭐ Особая рыба",
                "Иногда тебе может попасться особенная рыба — её имя будет выделено звёздочками `★`. За такую даётся больше очков!")
            .AddField("⏱️ Кулдаун",
                "После каждой рыбалки нужно подождать **3 секунды** перед следующим забросом.")
            .AddField("🏅 Как считаются очки",
                """
                Итоговые очки зависят от:
                · **Редкости рыбы** — чем реже, тем больше очков
                · **Прилагательного** — множитель от `×1.0` до `×10.0`
                · **Веса** — чем тяжелее, тем лучше
                · **Особенности** — если рыба особенная, очки увеличиваются!
                """)
            .WithFooter("Удачной рыбалки! Mr. Fish всегда ждёт тебя на дне 🌊");

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(embed));
    }
}
