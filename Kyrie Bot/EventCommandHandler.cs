using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Commands;
using System.Reflection;
using System.Linq;

namespace Kryie_Bot
{
    public class EventCommandHandler
    {
        public static DiscordSocketClient client;
        public static CommandService commands;
        public EventCommandHandler(DiscordSocketClient _client, CommandService _commands) 
        {
            client = _client;
            commands = _commands;
        }
        public static DiscordSocketClient getClient()
        {
            return client;
        }

        async public static Task OnReady() 
        {
            Console.WriteLine("Kyrie Bot is Ready to be used...");
        }

        //Whenever a user joins the server it will welcome them in the channel that the programmer wants.
        async public static Task OnUserJoin(SocketGuildUser user)
        {
            ulong message = user.Guild.Channels.FirstOrDefault(cha => cha.Name == "new-comers").Id; //Change ulong id to another channel id depending where you want the welcome message to go.
            await user.Guild.GetTextChannel(message).SendMessageAsync("Welcome to the server " + user.Mention + " be sure to also check out our guilded server \n https://www.guilded.gg/Kyrie-Studios/groups/53N0vLRd/channels/0a1ba8b8-e4c0-4f28-856c-b0d41355c39a/chat!");                                                                                        
        }

        // Installs all commands from the command module file and is send to the event MessageReceived to be listened to. 
        public async Task InstallCommandsAsync() 
        {
            
            client.MessageReceived += OnMessageReceived;
            client.Ready += OnReady;
            client.UserJoined += OnUserJoin;
            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
        }

        private async Task OnMessageReceived(SocketMessage msg)
        {
            SocketUserMessage message = msg as SocketUserMessage;
            if (message == null) return;
            char prefix = '-';
            Int32 posOfPrefix = 0;
            if (!message.HasCharPrefix(prefix, ref posOfPrefix)) return;

            SocketCommandContext context = new SocketCommandContext(client, message);
            await commands.ExecuteAsync(context, posOfPrefix, null);
        }
    }
}
