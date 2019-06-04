using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using BonusBot.Helpers;
using Discord.Commands;
using Victoria;

namespace AudioAssembly
{
    partial class AudioModule
    {
        [Command("shuffle"), AudioProviso(createPlayerIfNeeded: false)]
        [Alias("mix")]
        public async Task ShuffleAsync()
        {
            player.Queue.Shuffle();
            await ReplyAsync("The queue got shuffled.");
        }

        [Command("NowPlaying"), AudioProviso]
        [Alias("playing")]
        public async Task<RuntimeResult> NowPlayingAsync()
        {
            if (player.CurrentTrack is null)
            {
                return Reply("There is no track playing right now.");
            }

            var track = player.CurrentTrack.Audio;
            var thumb = await track.FetchThumbnailAsync();
            var embed = EmbedHelper.DefaultEmbed
                .WithAuthor($"Now Playing {track.Title}", thumb, $"{track.Uri}")
                .WithThumbnailUrl(thumb)
                .AddField("Author", track.Author, true)
                .AddField("Length", track.Length, true)
                .AddField("Position", track.Position, true)
                .AddField("Streaming?", track.IsStream, true);

            return Reply(embed);
        }

        [Command("Lyrics"), AudioProviso]
        public async Task<RuntimeResult> LyricsAsync()
        {
            if (player.CurrentTrack is null)
            {
                return Reply("There is no track playing right now.");
            }

            var audio = player.CurrentTrack.Audio;
            var lyrics = await audio.FetchLyricsAsync();
            var thumb = await audio.FetchThumbnailAsync();

            var embed = EmbedHelper.DefaultEmbed
                .WithImageUrl(thumb)
                .WithDescription(lyrics)
                .WithAuthor($"Lyrics For {audio.Title}", thumb);

            return Reply(embed);
        }

        [Command("Queue"), AudioProviso(playerNeeded: false)]
        public Task Queue()
        {
            var queue = player?.Queue;
            return ReplyAsync((queue is null || queue.Count is 0) ?
                "No tracks in queue." : queue.ToString());
        }
    }
}
