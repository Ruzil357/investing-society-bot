using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace PsnHackathonBot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;
        private readonly IMessageLogger _messageLogger;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
            IConfiguration config, IMessageLogger messageLogger)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
            _messageLogger = messageLogger;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.MessageUpdated += OnMessageUpdated;
            _client.MessageDeleted += OnMessageDeleted;
            _client.UserJoined += OnMemberJoinedGuild;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnMemberJoinedGuild(SocketGuildUser user)
        {
            if (user.IsBot || user.Guild.Id != _config.GetValue<ulong>("serverId")) return;

            var channels = _config.GetSection("channels");
            var welcomeUser = channels.GetValue<ulong>("join");

            var welcomeMessage = new EmbedBuilder()
                .WithTitle("Welcome to the server")
                .WithDescription($"Thanks for joining the club, {user.Mention}! We hope you enjoy your time here.")
                .Build();

            await user.Guild.GetTextChannel(welcomeUser).SendMessageAsync(null, false, welcomeMessage);
        }

        private async Task OnMessageUpdated(Cacheable<IMessage, ulong> arg1, SocketMessage arg2,
            ISocketMessageChannel arg3)
        {
            var oldMessage = await arg1.GetOrDownloadAsync();

            if (arg2 is not SocketUserMessage { Source: MessageSource.User } newMessage)
            {
                return;
            }

            if (_config.GetSection("channels").GetValue<ulong>("log") != arg3.Id)
            {
                if (newMessage.Channel is not SocketGuildChannel channel)
                {
                    return;
                }

                var guild = channel.Guild;
                if (guild.Id != _config.GetValue<ulong>("serverId"))
                {
                    return;
                }

                _messageLogger.OnMessageEdited(newMessage, oldMessage);
            }
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg is not SocketUserMessage { Source: MessageSource.User } message)
            {
                return;
            }

            if (_config.GetSection("channels").GetValue<ulong>("log") != message.Channel.Id)
            {
                if (message.Channel is not SocketGuildChannel channel)
                {
                    return;
                }

                var guild = channel.Guild;
                if (guild.Id != _config.GetValue<ulong>("serverId"))
                {
                    return;
                }

                _messageLogger.OnMessageCreated(message);
            }

            var argPos = 0;
            var prefix = _config["defaultPrefix"];
            if (!message.HasStringPrefix(prefix, ref argPos) &&
                !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnMessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            if (!arg1.HasValue)
            {
                return;
            }

            if (arg1.Value is not SocketUserMessage { Source: MessageSource.User } message)
            {
                return;
            }

            if (arg2 is not SocketGuildChannel channel)
            {
                return;
            }


            if (_config.GetSection("channels").GetValue<ulong>("log") != channel.Id)
            {
                var guild = channel.Guild;
                if (guild.Id != _config.GetValue<ulong>("serverId"))
                {
                    return;
                }

                _messageLogger.OnMessageDeleted(message);
            }
        }
    }
}