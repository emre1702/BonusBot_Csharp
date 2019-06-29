using System.Collections.Generic;
using Discord;
using Discord.WebSocket;

namespace WebHook.Entity
{
    public class GuildWebHookSettings
    {
        public SocketGuild Guild;

        #nullable enable
        public Dictionary<PostType, ITextChannel?> OutputChannel = new Dictionary<PostType, ITextChannel?>();
        public ITextChannel? ErrorOutputChannel;

        public bool DeleteNeedTestingAfterLabelRemove;
        public bool DeleteHelpWantedAfterLabelRemove;

        public string? BugIssueTitlePrefix;
        public string? SuggestionIssueTitlePrefix;
        #nullable restore
    }
}
