using System.Collections.Generic;
using Discord;
using Discord.WebSocket;

namespace WebHook.Entity
{
    public class GuildWebHookSettings
    {
        public SocketGuild Guild { get; set; }

#nullable enable
        public Dictionary<PostType, ITextChannel?> OutputChannel { get; set; } = new Dictionary<PostType, ITextChannel?>();
        public ITextChannel? ErrorOutputChannel { get; set; }

        public bool DeleteNeedTestingAfterLabelRemove { get; set; }
        public bool DeleteHelpWantedAfterLabelRemove { get; set; }

        public string? BugIssueTitlePrefix { get; set; }
        public string? SuggestionIssueTitlePrefix { get; set; }
#nullable restore
    }
}
