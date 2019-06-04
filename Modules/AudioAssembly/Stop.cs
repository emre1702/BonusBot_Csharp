using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using Discord.Commands;
using Victoria.Entities.Enums;

namespace AudioAssembly
{
    partial class AudioModule
    {
        [Command("stop")]
        public async Task StopAsync()
        {
            await player.StopAsync();
        }

        [Command("pause")]
        [Alias("unresume")]
        [AudioProviso(createPlayerIfNeeded: false)]
        public async Task PauseAsync()
        {
            if (player.Status != PlayerStatus.Paused)
                await player.PauseAsync();
        }
    }
}
