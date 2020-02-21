using Discord.Commands;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Handlers;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System;
using System.Globalization;
using System.Linq;

namespace AdminAssembly
{
    [RequireContext(ContextType.Guild)]
    public sealed partial class AdminModule : CommandBase
    {
        private readonly DatabaseHandler _databaseHandler;
        private readonly RolesHandler _rolesHandler;

        public AdminModule(DatabaseHandler databaseHandler, RolesHandler rolesHandler)
        {
            _databaseHandler = databaseHandler;
            _rolesHandler = rolesHandler;
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            base.BeforeExecute(command);
        }

        private async Task<SocketUser> GetMentionedUser(string mention, string usage, bool outputError = true, bool checkHistory = true)
        {
            if (Context.Message.MentionedUsers.Any())
            {
                var socketUser = Context.Message.MentionedUsers.First();
                var guildUser = Context.Guild.GetUser(socketUser.Id);
                if (guildUser != null)
                    return guildUser;
                else 
                    return socketUser;
            }

            if (!MentionUtils.TryParseUser(mention, out ulong userId))
            {
                if (outputError && usage is { })
                    await ReplyAsync($"Please mention (a) valid user with @: '{usage}'");
                if (!ulong.TryParse(mention, out userId))
                    return null;
            }

            var user = Context.Guild.GetUser(userId);
            if (user == null)
            {
                if (outputError && usage is { })
                    await ReplyAsync($"The user doesn't exist: '{usage}'");
                return null;
            }

            if (checkHistory && Context.User.Hierarchy < user.Hierarchy)
            {
                if (outputError)
                    await ReplyAsync("The target got a highter rank than you.");
                return null;
            }

            return user;
        }

        private bool GetTime(string time, out DateTimeOffset? dateTimeOffset, out bool isPerma)
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

        private SocketGuildUser GetUser(string targetStr)
        {
            SocketGuildUser target = null;
            if (MentionUtils.TryParseUser(targetStr, out ulong userId))
                target = Context.Guild.GetUser(userId);
            if (target != null)
                return target;

            var possibleTargets = Context.Guild.Users.Where(u =>
                u.Id.ToString() == targetStr
                || u.Username.Equals(targetStr, StringComparison.CurrentCultureIgnoreCase)
                || u.Discriminator.Equals(targetStr, StringComparison.CurrentCultureIgnoreCase)
                || $"{u.Username}#{u.Discriminator}".Equals(targetStr, StringComparison.CurrentCultureIgnoreCase));

            if (!possibleTargets.Any())
                return null;

            target = possibleTargets.Where(u => u.Id.ToString() == targetStr).FirstOrDefault();
            if (target != null)
                return target;

            target = possibleTargets.Where(u => $"{u.Username}#{u.Discriminator}".Equals(targetStr, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (target != null)
                return target;

            target = possibleTargets.Where(u => u.Discriminator.Equals(targetStr, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            if (target != null)
                return target;

            target = possibleTargets.Where(u => u.Username.Equals(targetStr, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            return target;
        }

    }
}


/*
namespace AdminAssembly
{
    partial class AdminModule
    {

    }
}
*/
