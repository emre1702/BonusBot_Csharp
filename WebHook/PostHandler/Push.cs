using System.Text;
using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class Push
    {
        public static EmbedBuilder Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(0, 0, 150)
                   .WithTitle($"[{o.Repository.Name}:{o.Branch}] {o.Commits.Length} new commit(s).")
                   .WithUrl(o.HeadCommit.Url)
                   .WithFooter("Changes pushed");
            //.WithTimestamp(DateTimeOffset.Parse(o.HeadCommit.Timestamp).ToLocalTime());

            var strBuilder = new StringBuilder();
            foreach (var commit in o.Commits)
            {
                strBuilder.AppendLine($"[`{commit.Id.Substring(0, 7)}`]({commit.Url}) {commit.Message}");
            }
            builder.WithDescription(strBuilder.ToString());

            return builder;
        }
    }
}
