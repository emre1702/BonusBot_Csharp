using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class IssueOpened
    {
        public static Embed Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(0, 150, 0)
                   .WithTitle("[OPENED] " + o.Issue.Title)
                   .WithUrl(o.Issue.Url)
                   .WithDescription(o.Issue.Body);

            return builder.Build();
        }
    }
}
