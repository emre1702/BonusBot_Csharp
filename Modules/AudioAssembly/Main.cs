using Discord.Commands;
using BonusBot.Common.Attributes;
using BonusBot.Common.ExtendedModules;
using Victoria;
using BonusBot.Common.Handlers;
using Discord;

namespace AudioAssembly
{
    [RequireContext(ContextType.Guild)]
    [AudioModuleProviso]
    [RequireBotPermission(GuildPermission.Speak & GuildPermission.Connect)]
    public sealed partial class AudioModule : CommandBase
    {
        private LavaPlayer player;
        private readonly LavaSocketClient _lavaSocketClient;
        private readonly LavaRestClient _lavaRestClient;
        private readonly DatabaseHandler _databaseHandler;

        public AudioModule(LavaSocketClient lavaSocketClient, LavaRestClient lavaRestClient, DatabaseHandler databaseHandler)
        {
            _lavaSocketClient = lavaSocketClient;
            _lavaRestClient = lavaRestClient;
            _databaseHandler = databaseHandler;
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            player = _lavaSocketClient.GetPlayer(Context.Guild.Id);
            base.BeforeExecute(command);
        }
    }
}
