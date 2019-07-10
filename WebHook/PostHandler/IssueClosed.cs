using System.Collections.Generic;
using System.Linq;
using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class IssueClosed
    {
        public static List<EmbedBuilder> Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(100, 0, 0)
                   .WithTitle(o.Issue.Title)
                   .WithUrl(o.Issue.HtmlUrl)
                   .WithDescription(Handler.SplitForEmbedDescription(o.Issue.Body).First())
                   .WithFooter("Issue closed");

            return new List<EmbedBuilder>
            {
                builder
            };
        }
    }
}
