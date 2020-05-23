using System;
using System.Linq;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;

namespace AdminAssembly
{
    partial class AdminModule
    {
        [Command("roleban")]
        [Alias("TRoleBan", "TimeRoleBan", "RoleBanT", "RoleBanTime", "PermaRoleBan", "PermanentRoleBan", "RoleBanPerma", "RoleBanPermanent", "PRoleBan", "RoleBanP",
            "RemoveRoleBan", "RoleBanRemove", "DeleteRoleBan", "RoleBanDelete", "URoleBan", "UnRoleBan", "RoleBanU")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task RoleBanUser(string targetStr, string roleStr, string time, [Remainder] string reason)
        {
            var target = await GetMentionedUser(targetStr, null, false);
            if (target == null)
            {
                await ReplyAsync("Target could not be found.");
                return;
            }
            var targetGuildUser = Context.Guild.GetUser(target.Id);
            if (targetGuildUser != null && targetGuildUser.Hierarchy > Context.User.Hierarchy)
            {
                await ReplyAsync("The target got a higher rank than you.");
                return;
            }

            if (!GetTime(time, out DateTimeOffset? dateTimeOffset, out bool isPerma))
            {
                await ReplyAsync(
@"Invalid time: 'roleban [@user] [role] [time] [reason]'.
Please use with X as number:
'Xs' or 'Xsec' for seconds,
'Xm' or 'Xmin' for minutes,
'Xh', 'Xst' for hours,
'Xd', 'Xt' for days,
'-1', '-', 'perma' or 'never' for perma,
'0', 'unban', 'no' or 'stop' for unban.");
                return;
            }

            var role = GetRole(roleStr);
            if (role is null)
            {
                await ReplyAsync("Role could not be found.");
                return;
            }
            SocketRole otherRole = null;

            var guildEntity = _databaseHandler.Get<GuildEntity>(Context.Guild.Id);
            string muteRolesSuffix = guildEntity.RoleForMutedSuffix;
            if (muteRolesSuffix != default)
            {
                var mutedOrUnmutedOtherRoleName = role.Name.EndsWith("Muted")
                    ? role.Name.Substring(0, role.Name.Length - muteRolesSuffix.Length)
                    : role.Name + muteRolesSuffix;

                otherRole = GetRole(mutedOrUnmutedOtherRoleName);

                if (!otherRole.Name.EndsWith("Muted"))
                {
                    var roleTemp = role;
                    role = otherRole;
                    otherRole = roleTemp;
                }
            }

            string rolebanId = $"{target.Id}-{role.Id}-RoleBan";

            var previousRoleBan = _databaseHandler.Get<CaseEntity>(rolebanId);
            if (previousRoleBan != null)
            {
                await ReplyAsync("Previous roleban removed:");
                await ReplyAsync(embed: previousRoleBan.ToEmbed(Context.Client));
                _databaseHandler.Delete(previousRoleBan);
            }

            if (dateTimeOffset.HasValue || isPerma)   // not unban
            {
                var dmChannel = await target.GetOrCreateDMChannelAsync();
                await dmChannel?.SendMessageAsync(
                    @$"You've got a role ban in {Context.Guild.Name} Discord server for the role {role.Name} by {Context.User.Username}.
Reason: {reason}
Expires: {(isPerma ? "never" : dateTimeOffset.Value.ToString())}");

                //await Context.Guild.AddBanAsync(target, 0, reason);
                var caseEntity = new CaseEntity
                {
                    Id = rolebanId,
                    UserId = target.Id,
                    CaseType = isPerma ? CaseType.RoleBan : CaseType.TempRoleBan,
                    CreatedOn = DateTimeOffset.Now,
                    GuildId = Context.Guild.Id,
                    ModeratorId = Context.User.Id,
                    Reason = reason,
                };
                if (dateTimeOffset.HasValue)
                    caseEntity.ExpiresOn = dateTimeOffset.Value;
                _databaseHandler.Save(caseEntity);

                try
                {
                    await targetGuildUser.RemoveRoleAsync(role);
                    if (otherRole is { })
                    {
                        await targetGuildUser.RemoveRoleAsync(otherRole);
                    }
                }
                catch
                {
                    // ignored
                }

                await ReplyAsync($"Role ban saved for {target.Username}. Use 'rolebaninfo [@user] [roleName]' for informations.");
            }
            else
            {
                await ReplyAsync($"The users {target.Username} role ban for {role.Name} got removed.");
                var dmChannel = await target.GetOrCreateDMChannelAsync();
                dmChannel?.SendMessageAsync($"Your role ban for {role.Name} got removed in {Context.Guild.Name} Discord server by {Context.User.Username}. Reason: {reason}");
            }

        }

        [Command("rolebaninfo")]
        [Alias("infoban")]
        public async Task<RuntimeResult> RoleBanInfo(string targetStr, string roleStr)
        {
            var user = await GetMentionedUser(targetStr, null, false);
            if (user == null)
                return Reply("The user doesn't exist: 'rolebaninfo @user [role]'");

            var role = GetRole(roleStr);
            if (role is null)
                return Reply("The role doesn't exist.");

            string rolebanId = $"{user.Id}-{role.Id}-RoleBan";
            var roleban = _databaseHandler.Get<CaseEntity>(rolebanId);
            if (roleban == null)
                return Reply("The user is not banned for this role.");

            return Reply(roleban.ToEmbedBuilder(Context.Client));
        }


        private SocketRole GetRole(string roleStr)
        {
            if (ulong.TryParse(roleStr, out ulong roleId))
            {
                var possibleRole = Context.Guild.GetRole(roleId);
                if (possibleRole is { })
                    return possibleRole;
            }

            return Context.Guild.Roles.FirstOrDefault(r =>
                r.Mention.Equals(roleStr, StringComparison.CurrentCultureIgnoreCase)
                || r.Name.Equals(roleStr, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
