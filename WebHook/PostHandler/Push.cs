using System.Collections.Generic;
using System.Text;
using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    static class Push
    {
        public static List<EmbedBuilder> Handle(Base o)
        {
            var embedBuilders = new List<EmbedBuilder>();
            var builder = GetInitBuilder(o);
            //.WithTimestamp(DateTimeOffset.Parse(o.HeadCommit.Timestamp).ToLocalTime());
            embedBuilders.Add(builder);
            var strBuilder = new StringBuilder();
            foreach (var commit in o.Commits)
            {
                string msg = $"[`{commit.Id.Substring(0, 7)}`]({commit.Url}) {commit.Message}";
                if (strBuilder.Length + msg.Length + 2 >= Handler.MAX_DESCRIPTION_LENGTH)
                {
                    builder.WithDescription(strBuilder.ToString());
                    builder = GetInitBuilder(o);
                    embedBuilders.Add(builder);
                    strBuilder.Clear();
                }
                strBuilder.AppendLine(msg);
            }
            builder.WithDescription(strBuilder.ToString());

            return embedBuilders;
        }

        private static EmbedBuilder GetInitBuilder(Base o)
        {
            return new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(0, 0, 150)
                   .WithTitle($"[{o.Repository.Name}:{o.Branch}] {o.Commits.Length} new commit(s).")
                   .WithUrl(o.HeadCommit.Url)
                   .WithFooter("Changes pushed");
        }
    }
}
