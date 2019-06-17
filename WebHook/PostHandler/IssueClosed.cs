using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class IssueClosed
    {
        public static Embed Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(100, 0, 0)
                   .WithTitle("[CLOSED] " + o.Issue.Title)
                   .WithUrl(o.Issue.Url)
                   .WithDescription(o.Issue.Body);

            return builder.Build();
        }
    }
}
