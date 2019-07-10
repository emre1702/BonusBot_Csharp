using System.Collections.Generic;
using System.Linq;
using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class IssueCommented
    {
        public static List<EmbedBuilder> Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(255, 238, 22)
                   .WithTitle(o.Issue.Title)
                   .WithUrl(o.Comment.HtmlUrl)
                   .WithDescription(Handler.SplitForEmbedDescription(o.Comment.Body).First())
                   .WithFooter("Issue commented");

            return new List<EmbedBuilder> { builder };
        }
    }
}
