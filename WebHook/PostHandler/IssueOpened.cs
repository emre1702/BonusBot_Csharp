using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class IssueOpened
    {
        public static List<EmbedBuilder> Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(0, 150, 0)
                   .WithTitle(o.Issue.Title)
                   .WithUrl(o.Issue.HtmlUrl)
                   .WithDescription(Handler.SplitForEmbedDescription(o.Issue.Body).First())
                   .WithFooter("New issue");

            return new List<EmbedBuilder> { builder };
        }
    }
}
