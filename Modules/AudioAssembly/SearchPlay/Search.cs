using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using Discord.Commands;
using Victoria.Entities;
using SearchResult = Victoria.Entities.SearchResult;

namespace AudioAssembly.SearchPlay
{
    partial class AudioSearchModule
    {
        [Command]
        [Alias("youtube", "yt", "y")]
        [Priority(2)]
        public async Task SearchYoutubeAsync([Remainder] string query)
        {
            await SearchYoutubeAsync(10, query);
        }

        [Command]
        [Alias("youtube", "yt", "y")]
        [Priority(1)]
        public async Task SearchYoutubeAsync([NumberRangeProviso(1, 20)] int amount, [Remainder] string query)
        {
            var search = await _lavaRestClient.SearchYouTubeAsync(query);
            await HandleSearch(search, amount);
        }

        [Command("SoundCloud")]
        [Alias("sc", "s")]
        [Priority(4)]
        public async Task SearchSoundcloudAsync([Remainder] string query)
        {
            await SearchSoundcloudAsync(10, query);
        }

        [Command("SoundCloud")]
        [Alias("sc", "s")]
        [Priority(3)]
        public async Task SearchSoundcloudAsync([NumberRangeProviso(1, 20)] int amount, [Remainder] string query)
        {
            var search = await _lavaRestClient.SearchSoundcloudAsync(query);
            await HandleSearch(search, amount);
        }

        private async Task HandleSearch(SearchResult search, int amount)
        {
            if (search.LoadType == LoadType.NoMatches ||
                search.LoadType == LoadType.LoadFailed)
            {
                await ReplyAsync("Nothing found.");
            }

            await ReplyAsync($"Found amount: {Math.Min(search.Tracks.Count(), amount)}");

            var builder = new StringBuilder();
            int i = 0;
            _trackHandler.LastSearchResult = search.Tracks.Take(amount).ToArray();
            foreach (var track in _trackHandler.LastSearchResult)
            {
                builder.AppendLine($"{++i}. {track.Title} - {track.Author}");
            }
            await ReplyAsync(builder.ToString());
        }
    }
}
