using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    class Handler
    {
        private readonly ITextChannel _outputToChannel;
        private readonly Action<string, LogSeverity, Exception> _logger;
        private readonly Dictionary<PostType, Func<Base, Embed>> _embedGetter = new Dictionary<PostType, Func<Base, Embed>>
        {
            [PostType.Commits] = Push.Handle,
            [PostType.IssueClosed] = IssueClosed.Handle,
            [PostType.IssueOpened] = IssueOpened.Handle
        };

        public Handler(ITextChannel outputToChannel, Action<string, LogSeverity, Exception> logger)
        {
            _outputToChannel = outputToChannel;
            _logger = logger;
        }

        public async Task HandleEchoPost(string content)
        {
            try
            {
                var o = JsonConvert.DeserializeObject<Base>(content);
                var postType = GetPostType(o);
                if (postType == PostType.Unknown)
                    return;
                if (!_embedGetter.ContainsKey(postType))
                    return;
                var embed = _embedGetter[postType](o);

                await _outputToChannel.SendMessageAsync(embed: embed);
            }
            catch (Exception ex)
            {
                _logger("Error at reading GitHub request", LogSeverity.Error, ex);
            }
        }

        private PostType GetPostType(Base content)
        {
            if (content.Action != null)
            {
                switch (content.Action)
                {
                    case "closed":
                        return PostType.IssueClosed;
                    case "opened":
                        return PostType.IssueOpened;
                }
            }

            if (content.Commits != null && content.Commits.Length > 0)
                return PostType.Commits;

            return PostType.Unknown;
        }
    }
}
