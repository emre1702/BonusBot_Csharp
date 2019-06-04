using BonusBot.Common.Attributes;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Handlers;
using Discord.Commands;
using Victoria;
using Victoria.Entities;

namespace AudioAssembly.SearchPlay
{
    [RequireContext(ContextType.Guild)]
    [AudioModuleProviso]
    [Group("search")]
    [Alias("YoutubeSearch", "ytSearch", "searchYt", "searchYoutube")]
    public sealed partial class AudioSearchModule : CommandBase
    {
        private LavaPlayer player;
        private readonly LavaSocketClient _lavaSocketClient;
        private readonly LavaRestClient _lavaRestClient;
        private readonly TrackHandler _trackHandler;

        public AudioSearchModule(LavaSocketClient lavaSocketClient, LavaRestClient lavaRestClient, TrackHandler trackHandler)
        {
            _lavaSocketClient = lavaSocketClient;
            _lavaRestClient = lavaRestClient;
            _trackHandler = trackHandler;
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            base.BeforeExecute(command);
        }
    }
}
