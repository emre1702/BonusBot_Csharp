using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using BonusBot.Common.Entities;
using Discord.Commands;

namespace AudioAssembly
{
    partial class AudioModule
    {
        [Command("volume"), AudioProviso]
        [Alias("SetVolume", "SetVol", "vol", "volum", "lautstärke")]
        public async Task<RuntimeResult> SetVolume([NumberRangeProviso(0, 200)] uint volume)
        {
            var guild = _databaseHandler.Get<GuildEntity>(Context.User.Guild.Id);
            guild.LastPlayerVolume = volume;
            _databaseHandler.Save(guild);

            await player.SetVolumeAsync((int)volume);
            return Reply($"Changed volume to {player.CurrentVolume}.");
        }

        [Command("volume"), AudioProviso]
        [Alias("GetVolume", "GetVol", "vol", "volum", "lautstärke")]
        public Task GetVolume()
        {
            return ReplyAsync($"Current volume: {player.CurrentVolume}.");
        }
    }
}
