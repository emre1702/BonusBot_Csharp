using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using Discord.Commands;

namespace AudioAssembly
{
    partial class AudioModule
    {
        [Command("join"), AudioProviso(true)]
        [Alias("come")]
        public async Task JoinAsync()
        {
            await ReplyAsync("Connected!");
        }

        [Command("move"), AudioProviso(true)]
        public async Task MoveAsync()
        {
            var old = player.VoiceChannel;
            await _lavaSocketClient.MoveChannelsAsync(Context.User.VoiceChannel);
            await ReplyAsync($"Moved from {old.Name} to {player.VoiceChannel.Name}!");
        }
    }
}
