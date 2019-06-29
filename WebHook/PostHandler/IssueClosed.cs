using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class IssueClosed
    {
        public static EmbedBuilder Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(100, 0, 0)
                   .WithTitle(o.Issue.Title)
                   .WithUrl(o.Issue.HtmlUrl)
                   .WithDescription(o.Issue.Body)
                   .WithFooter("Issue closed");

            return builder;
        }
    }
}
