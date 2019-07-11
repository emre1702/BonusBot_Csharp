using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;
using WebHook.Entity;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    class Handler
    {
        public const int MAX_DESCRIPTION_LENGTH = 2048;

        private readonly GuildWebHookSettings _settings;
        private readonly Action<string, LogSeverity, Exception> _logger;
        private readonly Dictionary<PostType, Func<Base, List<EmbedBuilder>>> _embedGetter = new Dictionary<PostType, Func<Base, List<EmbedBuilder>>>
        {
            [PostType.Push] = Push.Handle,
            [PostType.IssueClosed] = IssueClosed.Handle,
            [PostType.IssueOpened] = IssueOpened.Handle,
            [PostType.IssueCommented] = IssueCommented.Handle,
            [PostType.IssueInitialCommentEdited] = IssueInitialCommentEdited.Handle,
            [PostType.IssueNeedTestingAdded] = IssueLabelAdded.Handle,
            [PostType.IssueHelpWantedAdded] = IssueLabelAdded.Handle
        };


        public Handler(GuildWebHookSettings settings, Action<string, LogSeverity, Exception> logger)
        {
            _settings = settings;
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

                if (postType == PostType.IssueHelpWantedRemoved || postType == PostType.IssueNeedTestingRemoved)
                {
                    await CheckRemoveLabelAddedOutput(o, postType);
                    return;
                }
                if (!_settings.OutputChannel.ContainsKey(postType))
                    return;

                if (!_embedGetter.ContainsKey(postType))
                    return;
                var embedBuilders = _embedGetter[postType](o);

                if (postType == PostType.IssueOpened)
                {
                    foreach (var embedBuilder in embedBuilders)
                    {
                        if (!string.IsNullOrEmpty(_settings.BugIssueTitlePrefix) && embedBuilder.Title.StartsWith(_settings.BugIssueTitlePrefix))
                        {
                            embedBuilder.Color = new Color(150, 0, 0);
                        }
                        else if (!string.IsNullOrEmpty(_settings.SuggestionIssueTitlePrefix) && embedBuilder.Title.StartsWith(_settings.SuggestionIssueTitlePrefix))
                        {
                            embedBuilder.Color = new Color(0, 0, 150);
                        }
                    }
                }

                foreach (var embedBuilder in embedBuilders)
                {
                    await _settings.OutputChannel[postType].SendMessageAsync(embed: embedBuilder.Build());
                }
            }
            catch (Exception ex)
            {
                _logger("Error at reading GitHub request", LogSeverity.Error, ex);
            }
        }

        private async Task CheckRemoveLabelAddedOutput(Base o, PostType postType)
        {
            if (postType == PostType.IssueHelpWantedRemoved && _settings.DeleteHelpWantedAfterLabelRemove && _settings.OutputChannel.ContainsKey(PostType.IssueHelpWantedAdded))
            {
                var channel = _settings.OutputChannel[PostType.IssueHelpWantedAdded];
                await RemoveLabelAddedOutput(o, channel);
                return;
            }

            if (postType == PostType.IssueNeedTestingRemoved && _settings.DeleteNeedTestingAfterLabelRemove && _settings.OutputChannel.ContainsKey(PostType.IssueNeedTestingRemoved))
            {
                var channel = _settings.OutputChannel[PostType.IssueNeedTestingRemoved];
                await RemoveLabelAddedOutput(o, channel);
                return;
            }
        }

        private async Task RemoveLabelAddedOutput(Base o, ITextChannel channel)
        {
            var messages = await channel.GetMessagesAsync(1000).FlattenAsync();
            var msgId = messages.Where(m =>
            {
                if (m.Embeds.Count == 0)
                    return false;
                var embed = m.Embeds.First();
                if (!embed.Footer.HasValue)
                    return false;

                if (embed.Footer.Value.Text != o.Label.Name)
                    return false;

                return embed.Url == o.Issue.HtmlUrl;
            })
            .FirstOrDefault();
            if (msgId == default)
                return;
        }

        private PostType GetPostType(Base content)
        {
            if (content.Action != null)
            {
                switch (content.Action)
                {
                    case "opened":
                        return PostType.IssueOpened;
                    case "closed":
                        return PostType.IssueClosed;
                    case "created":
                        if (content.Comment != null)
                            return PostType.IssueCommented;
                        return PostType.Unknown;
                    case "edited":
                        if (content.Comment == null)
                            return PostType.IssueInitialCommentEdited;
                        return PostType.Unknown;
                    case "labeled":
                        if (content.Label.Name == "need testing")
                            return PostType.IssueNeedTestingAdded;
                        if (content.Label.Name == "help wanted")
                            return PostType.IssueHelpWantedAdded;
                        return PostType.Unknown;
                    case "unlabeled":
                        if (content.Label.Name == "need testing")
                            return PostType.IssueNeedTestingRemoved;
                        if (content.Label.Name == "help wanted")
                            return PostType.IssueHelpWantedRemoved;
                        return PostType.Unknown;
                }
            }

            if (content.Commits != null && content.Commits.Length > 0)
                return PostType.Push;

            return PostType.Unknown;
        }

        public static IEnumerable<string> SplitForEmbedDescription(string str)
        {
            var chunkLength = MAX_DESCRIPTION_LENGTH;
            if (String.IsNullOrEmpty(str)) throw new ArgumentException();
            if (chunkLength < 1) throw new ArgumentException();

            for (int i = 0; i < str.Length; i += chunkLength)
            {
                if (chunkLength + i > str.Length)
                    chunkLength = str.Length - i;

                yield return str.Substring(i, chunkLength);
            }
        }
    }
}
