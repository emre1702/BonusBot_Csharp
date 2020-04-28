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
