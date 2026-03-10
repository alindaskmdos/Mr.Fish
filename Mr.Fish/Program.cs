using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus;
using Fish.Extension;


public class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string token = config["Discord:Token"]!;

        var discordConfig = new DiscordConfiguration
        {
            Token = token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged,
        };

        var discord = new DiscordClient(discordConfig);

        var services = new ServiceCollection();
        services.AddServices();

        var provider = services.BuildServiceProvider();

        discord.AddSlashCommands(provider);

        Extension.AddDatabase();

        await discord.ConnectAsync();
        await Task.Delay(-1);
    }
}