using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using Discord.Commands;

namespace AudioAssembly
{
    partial class AudioModule
    {
        [Command("skip"), AudioProviso]
        public async Task SkipAsync()
        {
            try
            {
                var skipped = await player.SkipAsync();

                await ReplyAsync($"Skipped: {skipped.Audio.Title}\nNow Playing: {player.CurrentTrack.Audio.Title}");
            }
            catch
            {
                await ReplyAsync("There are no more items left in queue.");
            }
        }
    }
}
