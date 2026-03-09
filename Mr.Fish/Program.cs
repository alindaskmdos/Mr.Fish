using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using DSharpPlus;
using Fish.Extension;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Fish.Services;


public class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
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