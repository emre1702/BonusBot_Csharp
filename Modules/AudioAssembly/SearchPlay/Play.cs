using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using Discord.Commands;
using Victoria.Entities;
using Victoria.Entities.Enums;

namespace AudioAssembly.SearchPlay
{
    partial class AudioSearchModule
    {
        [Command, AudioProviso]
        [Alias("play", "start")]
        [Priority(5)]
        public async Task PlaySearchResult([NumberRangeProviso(1, 20)] int number)
        {
            var lastSearchResult = _trackHandler.LastSearchResult;
            if (lastSearchResult == null || lastSearchResult.Count == 0)
                await ReplyAsync("You need to search first.");
            if (lastSearchResult.Count < number)
                await ReplyAsync("The search result doesn't have so many tracks.");

            var audio = lastSearchResult[number - 1];
            string pausedMsg = player.Status == PlayerStatus.Paused ? "\nThe player is paused, use the resume command to continue." : string.Empty;

            AudioTrack track = new AudioTrack
            {
                Audio = audio,
                Player = player,
                User = Context.User
            };

            if (player.CurrentTrack != null)
            {
                player.Queue.Enqueue(track);
                await ReplyAsync($"{track.Audio.Title} has been queued." + pausedMsg);
            }
            else
            {
                await player.PlayAsync(track);
                await ReplyAsync($"Now Playing: {track.Audio.Title}" + pausedMsg);
            }
        }
    }
}
