using System.Linq;
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
        public Task ClearMessages(int length)
        {
            var _ = Task.Run(async () =>
            {
                var items = Context.Channel.GetMessagesAsync(length + 1).Flatten();
                await foreach (var message in items)
                {
                    await Context.Channel.DeleteMessageAsync(message);
                }
            });
            return Task.CompletedTask;
        }
    }
}