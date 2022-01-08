using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;

namespace PsnHackathonBot.Modules
{
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;

        public AdminModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Command("clear")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearMessages(int length)
        {
            await Context.Message.DeleteAsync();
            var _ = Task.Run(async () =>
            {
                var items = Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, length).Flatten();
                await foreach (var message in items)
                {
                    await Context.Channel.DeleteMessageAsync(message);
                }
            });
        }
    }
}