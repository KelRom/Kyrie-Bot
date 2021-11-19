using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Kryie_Bot
{
    class Program
    {
        // Make a new instance of a discord client that has access to all intents.
        private static readonly DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All,
        });
        // Make a new instance of a Command Service so that we can call commands within Discord, case sensitivity does not matter.
        private static readonly CommandService commands = new CommandService(new CommandServiceConfig()
        { CaseSensitiveCommands = false });

        private static readonly EventCommandHandler handler = new EventCommandHandler(client, commands);
        static void Main(string[] args)
        {
            
            MainAsync().GetAwaiter().GetResult();
        }

        // Main Async method that starts the bot, adds all command and event handle methods.
        async static Task MainAsync()
        {
            DotNetEnv.Env.TraversePath().Load();
            await client.LoginAsync(TokenType.Bot, System.Environment.GetEnvironmentVariable("TOKEN"));
            await client.StartAsync();
            await handler.InstallCommandsAsync();
            await Task.Delay(-1);
        }
    }
}
