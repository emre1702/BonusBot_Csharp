using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BonusBot.Common.ExtendedModules
{
    public class CommandBase : ModuleBase<CustomContext>
    {
        protected Task<IUserMessage> ReplyAsync(string message)
        {
            return base.ReplyAsync(message);
        }

        protected Task<IUserMessage> ReplyAsync(EmbedBuilder embed)
        {
            if (!embed.Color.HasValue)
                embed.WithColor(171, 31, 242);
            return base.ReplyAsync(embed: embed.Build());
        }

        protected CustomResult Reply(string message)
        {
            return new CustomResult(message, null);
        }

        protected CustomResult Reply(EmbedBuilder embed)
        {
            if (!embed.Color.HasValue)
                embed.WithColor(171, 31, 242);
            return new CustomResult(null, embed.Build());
        }

        protected (string, string) UseQuotationMarksForFirstText(string firstText, string secondText)
        {
            char quotationMark = firstText[0];
            if (quotationMark != '"' && quotationMark != '\'')
                return (firstText, secondText);

            if (firstText[^1] == quotationMark)
                return (firstText, secondText);

            var quotationMarkEndIndex = secondText.IndexOf(quotationMark);
            if (quotationMarkEndIndex < 0)
                return (firstText, secondText);

            firstText = firstText.Substring(1) + secondText.Substring(0, quotationMarkEndIndex);

            if (secondText.Length == quotationMarkEndIndex)
                secondText = string.Empty;
            else 
                secondText = secondText.Substring(quotationMarkEndIndex + 1);

            return (firstText, secondText);
        }

        protected string RemoveQuotationMarksAtStartAndEnd(string text)
        {
            char quotationMark = text[0];
            if (quotationMark != '"' && quotationMark != '\'')
                return text;
            if (text[^1] != quotationMark)
                return text;

            return text[1..^0];
        }
    }
}
