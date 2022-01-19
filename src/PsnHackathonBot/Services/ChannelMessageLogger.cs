using System;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace PsnHackathonBot.Services
{
    public class ChannelMessageLogger : IMessageLogger
    {
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _client;

        private ulong LogChannelId => _config.GetSection("channels").GetValue<ulong>("log");
        private ulong GuildId => _config.GetValue<ulong>("serverId");

        public ChannelMessageLogger(IConfiguration config, DiscordSocketClient client)
        {
            _config = config;
            _client = client;
        }

        public void OnMessageCreated(SocketUserMessage message)
        {
            var embed = GenerateEmbed(message)
                .WithDescription(
                    $"Message sent by {message.Author.Mention} in <#{message.Channel.Id}>"
                )
                .AddField("Content", message.Content);

            _client.GetGuild(GuildId).GetTextChannel(LogChannelId).SendMessageAsync(null, false, embed.Build());
        }

        public void OnMessageEdited(SocketUserMessage newMessage, IMessage oldMessage)
        {
            var embed = GenerateEmbed(newMessage)
                .WithDescription(
                    $"Message edited by {newMessage.Author.Mention} in <#{newMessage.Channel.Id}>"
                )
                .AddField("New Content", newMessage.Content);

            if (oldMessage != null)
            {
                embed.AddField("Old Content", oldMessage.Content);
            }

            _client.GetGuild(GuildId).GetTextChannel(LogChannelId).SendMessageAsync(null, false, embed.Build());
        }

        public void OnMessageDeleted(SocketUserMessage message)
        {
            var embed = GenerateEmbed(message)
                .WithDescription(
                    $"Message deleted by {message.Author.Mention} in <#{message.Channel.Id}>"
                )
                .AddField("Content", message.Content);

            _client.GetGuild(GuildId).GetTextChannel(LogChannelId).SendMessageAsync(null, false, embed.Build());
        }

        private static EmbedBuilder GenerateEmbed(SocketMessage message)
        {
            return new EmbedBuilder()
                .WithTitle($"{message.Author.Username}#{message.Author.Discriminator}")
                .WithFooter($"Id: {message.Author.Id}")
                .WithCurrentTimestamp();
        }
    }
}