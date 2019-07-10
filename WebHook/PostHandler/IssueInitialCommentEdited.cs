using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using WebHook.Entity.GitHub;

namespace WebHook.PostHandler
{
    class IssueInitialCommentEdited
    {
        public static List<EmbedBuilder> Handle(Base o)
        {
            var builder = new EmbedBuilder()
                   .WithAuthor(o.Sender.Username, o.Sender.AvatarUrl, o.Sender.UserUrl)
                   .WithColor(195, 206, 26)
                   .WithTitle(o.Issue.Title)
                   .WithUrl(o.Issue.HtmlUrl)
                   .WithDescription(Handler.SplitForEmbedDescription(GetBody(o)).First())
                   .WithFooter("Issue text changed");

            return new List<EmbedBuilder> { builder };
        }

        private static string GetBody(Base o)
        {
            var diffMatchPatch = new DiffMatchPatch.DiffMatchPatch();
            var differents = diffMatchPatch.DiffMain(o.Changes.Body.From, o.Issue.Body);
            diffMatchPatch.DiffCleanupSemantic(differents);
            //diffMatchPatch.DiffCleanupEfficiency(differents);

            return diffMatchPatch.DiffEmbedBody(differents);

            //return "```diff\n" + diffMatchPatch.DiffEmbedBody(differents) + "```";

            /* var strBuilder = new StringBuilder();
             if (differents.Any(d => d.operation == DiffMatchPatch.Operation.INSERT))
             {
                 strBuilder.AppendLine("Added:");
                 foreach (var diff in differents.Where(d => d.operation == DiffMatchPatch.Operation.INSERT).Select(d => d.text))
                 {
                     strBuilder.AppendLine(diff);
                 }
             }

             if (differents.Any(d => d.operation == DiffMatchPatch.Operation.DELETE))
             {
                 if (strBuilder.Length > 0)
                     strBuilder.AppendLine();
                 strBuilder.AppendLine("Removed:");
                 foreach (var diff in differents.Where(d => d.operation == DiffMatchPatch.Operation.DELETE).Select(d => d.text))
                 {
                     strBuilder.AppendLine(diff);
                 }
             }

             return strBuilder.ToString();*/
        }
    }
}
