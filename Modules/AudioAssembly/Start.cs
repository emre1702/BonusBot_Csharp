using System;
using System.Linq;
using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using Discord.Commands;
using Victoria.Entities;
using Victoria.Entities.Enums;
using SearchResult = Victoria.Entities.SearchResult;

namespace AudioAssembly
{
    partial class AudioModule
    {
        [Command("play"), AudioProviso]
        [Alias("yt", "youtube")]
        public async Task PlayAsync([Remainder] string query)
        {
            var search = await _lavaRestClient.SearchYouTubeAsync(query);
            if (search.LoadType == LoadType.NoMatches ||
                search.LoadType == LoadType.LoadFailed)
            {
                await ReplyAsync("Nothing found");
                return;
            }

            var audio = GetTrack(query, search);
            var track = new AudioTrack
            {
                Audio = audio,
                Player = player,
                TimeAdded = DateTime.Now,
                User = Context.User
            };

            await player.PlayAsync(track);
            await ReplyAsync($"Now Playing: {track.ToString()}");
        }

        [Command("queue"), AudioProviso]
        [Alias("ytqueue", "youtubequeue")]
        public async Task QueueAsync([Remainder] string query)
        {
            var search = await _lavaRestClient.SearchYouTubeAsync(query);
            if (search.LoadType == LoadType.NoMatches ||
                search.LoadType == LoadType.LoadFailed)
            {
                await ReplyAsync("Nothing found");
                return;
            }

            var audio = GetTrack(query, search);
            var track = new AudioTrack
            {
                Audio = audio,
                Player = player,
                TimeAdded = DateTime.Now,
                User = Context.User
            };

            player.Queue.Enqueue(track);
            await ReplyAsync($"{track.ToString()} has been queued.");
        }

        private LavaTrack GetTrack(string query, SearchResult search)
        {
            if (search.LoadType == LoadType.PlaylistLoaded)
            {
                int index = 0;
                int indexIndex = query.LastIndexOf("index=");
                if (indexIndex >= 0)
                {
                    string indexStr = query.Substring(indexIndex + "index=".Length);
                    if (int.TryParse(indexStr, out int theIndex))
                    {
                        index = theIndex;
                    }
                }
                index = Math.Min(index - 1, search.Tracks.Count());
                return search.Tracks.ElementAt(index);
            }
            else
            {
                return search.Tracks.First();
            }
        } 

        [Command("resume")]
        [Alias("unpause")]
        [AudioProviso(createPlayerIfNeeded: false)]
        public async Task ResumeAsync()
        {
            if (player.Status == PlayerStatus.Paused)
                await player.ResumeAsync();
        }

        [Command("current"), AudioProviso]
        [Alias("replay", "replaycurrent", "replaynow")]
        public async Task<RuntimeResult> ReplayCurrentAsync()
        {
            AudioTrack trackToReplay = null;

            if (player.CurrentTrack == null && player.PreviousTrack != null)
                trackToReplay = player.PreviousTrack;
            else if (player.CurrentTrack != null)
                trackToReplay = player.CurrentTrack;
            
            if (trackToReplay == null)
                return Reply("There is no track to replay.");

            await player.PlayAsync(trackToReplay);
            return Reply($"Replaying song: {trackToReplay.ToString()}");
        }

        [Command("previous"), AudioProviso] 
        [Alias("replayprevious", "replaydavor")]
        public async Task<RuntimeResult> ReplayPreviousAsync()
        {
            AudioTrack trackToReplay = null;

            if (player.PreviousTrack == null && player.CurrentTrack != null)
                trackToReplay = player.CurrentTrack;
            else if (player.PreviousTrack != null)
                trackToReplay = player.PreviousTrack;

            if (trackToReplay == null)
                return Reply("There is no track to replay.");

            await player.PlayAsync(trackToReplay);
            return Reply($"Replaying song: {trackToReplay.ToString()}");
        }
    }
}
