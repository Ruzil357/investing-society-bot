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

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service,
            IConfiguration config)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.UserJoined += OnMemberJoinedGuild;
            
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnMemberJoinedGuild(SocketGuildUser user)
        {
            if (user.IsBot) return;
            
            var attendeeRoleId = _config.GetSection("roles").GetValue<ulong>("attendee");

            var channels = _config.GetSection("channels");
            var welcomeUser = channels.GetValue<ulong>("join");
            var faq = channels.GetValue<ulong>("faq");
            var askOrganizers = channels.GetValue<ulong>("askOrg");

            await user.AddRoleAsync(user.Guild.GetRole(attendeeRoleId));
            
            var welcomeMessage = new EmbedBuilder()
                .WithTitle("Welcome to the server")
                .WithDescription($"Thanks for joining the hackathon, {user.Mention}! We hope you enjoy your time here. The hackathon will start on the **7th of January**")
                .AddField("Looking for help?", $"Read the FAQs in in <#{faq}>. If you have any questions, ask the organizers in <#{askOrganizers}>")
                .Build();

            await user.Guild.GetTextChannel(welcomeUser).SendMessageAsync(user.Mention, false, welcomeMessage);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (arg is not SocketUserMessage { Source: MessageSource.User } message)
            {
                return;
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
    }
}