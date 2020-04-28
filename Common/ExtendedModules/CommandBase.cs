using System;
using System.Globalization;
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

        protected bool GetTime(string time, out DateTimeOffset? dateTimeOffset, out bool isPerma)
        {
            dateTimeOffset = null;
            isPerma = false;
            switch (time)
            {
                #region Seconds
                case string _ when time.EndsWith("s", true, CultureInfo.CurrentCulture):    // seconds
                    if (!double.TryParse(time[0..^1], out double seconds))
                        return false;
                    dateTimeOffset = DateTimeOffset.Now.AddSeconds(seconds);
                    return true;
                case string _ when time.EndsWith("sec", true, CultureInfo.CurrentCulture):    // seconds
                    if (!double.TryParse(time[0..^3], out double secs))
                        return false;
                    dateTimeOffset = DateTimeOffset.Now.AddSeconds(secs);
                    return true;
                #endregion

                #region Minutes
                case string _ when time.EndsWith("m", true, CultureInfo.CurrentCulture):    // minutes
                    if (!double.TryParse(time[0..^1], out double minutes))
                        return false;
                    dateTimeOffset = DateTimeOffset.Now.AddMinutes(minutes);
                    return true;
                case string _ when time.EndsWith("min", true, CultureInfo.CurrentCulture):    // minutes
                    if (!double.TryParse(time[0..^3], out double mins))
                        return false;
                    dateTimeOffset = DateTimeOffset.Now.AddMinutes(mins);
                    return true;
                #endregion

                #region Hours
                case string _ when time.EndsWith("h", true, CultureInfo.CurrentCulture):    // hours
                    if (!double.TryParse(time[0..^1], out double hours))
                        return false;
                    dateTimeOffset = DateTimeOffset.Now.AddHours(hours);
                    return true;
                case string _ when time.EndsWith("st", true, CultureInfo.CurrentCulture):    // hours
                    if (!double.TryParse(time[0..^2], out double hours2))
                        return false;
                    dateTimeOffset = DateTimeOffset.Now.AddHours(hours2);
                    return true;
                #endregion

                #region Days
                case string _ when time.EndsWith("d", true, CultureInfo.CurrentCulture):    // days
                case string _ when time.EndsWith("t", true, CultureInfo.CurrentCulture):    // days
                    if (!double.TryParse(time[0..^1], out double days))
                        return false;
                    dateTimeOffset = DateTimeOffset.Now.AddDays(days);
                    return true;
                #endregion

                #region Perma
                case string _ when IsPerma(time):       // perma
                    isPerma = true;
                    return true;
                #endregion

                #region Unmute
                case string _ when IsUnmute(time):       // unmute
                    return true;
                #endregion

                default:
                    return false;

            };
        }

        private bool IsPerma(string time)
        {
            return time == "-1"
                || time == "-"
                || time.Equals("perma", StringComparison.CurrentCultureIgnoreCase)
                || time.Equals("permamute", StringComparison.CurrentCultureIgnoreCase)
                || time.Equals("permaban", StringComparison.CurrentCultureIgnoreCase)
                || time.Equals("never", StringComparison.CurrentCultureIgnoreCase);
        }

        private bool IsUnmute(string time)
        {
            return time == "0"
                || time.Equals("unmute", StringComparison.CurrentCultureIgnoreCase)
                || time.Equals("unban", StringComparison.CurrentCultureIgnoreCase)
                || time.Equals("stop", StringComparison.CurrentCultureIgnoreCase)
                || time.Equals("no", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
