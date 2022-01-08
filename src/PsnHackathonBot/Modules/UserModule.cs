using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PsnHackathonBot.Modules
{
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        private static DateTime IstTime => DateTime.UtcNow.AddHours(5).AddMinutes(30);
        private static readonly DateTime StartTime = new(2022, 1, 7, 11, 0, 0);
        private static readonly DateTime EndTime = new(2022, 1, 9, 19, 0, 0);
        private static bool Started => IstTime > StartTime;
        private static bool Ended => IstTime > EndTime;

        [Command("time")]
        public async Task TimeLeft(string code = null)
        {
            var timeNow = IstTime;

            if (!Started)
            {
                var difference = StartTime - timeNow;
                await ReplyAsync(null, false,
                    TimespanEmbed(difference)
                        .WithTitle("The hackathon has not started")
                        .Build());

                return;
            }

            if (!Ended)
            {
                var difference = EndTime - timeNow;
                await ReplyAsync(null, false,
                    TimespanEmbed(difference)
                        .WithTitle("The hackathon is live!")
                        .Build());

                return;
            }

            await ReplyAsync(null, false, new EmbedBuilder().WithTitle("The hackathon has ended").Build());
        }

        [Command("rules")]
        public async Task Rules()
        {
            await ReplyAsync(
                "Although we promote a inclusive and friendly culture, we do enforce a few rules.\nhttps://hackclub.com/conduct/\nhttps://static.mlh.io/docs/mlh-code-of-conduct.pdf");
        }

        [Command("themes")]
        public async Task Themes()
        {
            await ReplyAsync("The themes for the hackathon are **Hybrid Learning** and **Accessibility**.\nYour hack should be around one or both themes");
        }

        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync(null, false,
                new EmbedBuilder()
                    .WithTitle("Command Menu")
                    .AddField("!time", "Shows the time remaining before the start/end")
                    .AddField("!rules", "Shows the rules and code of conduct")
                    .AddField("!themes", "Shows the themes")
                    .Build());
        }

        private static EmbedBuilder TimespanEmbed(TimeSpan difference)
        {
            var days = difference.TotalDays > 0 ? $"{Math.Floor(difference.TotalDays)} Days" : "";
            var hours = $"{difference.Hours} hours";
            var minutes = $"{difference.Minutes} minutes";

            return new EmbedBuilder()
                .AddField("Time left", $"{days} {hours} {minutes}");
        }
    }
}