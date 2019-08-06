using System;
using System.Linq;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace AdminAssembly
{
    partial class AdminModule
    {
        [Command("ban")]
        [Alias("TBan", "TimeBan", "BanT", "BanTime", "PermaBan", "PermanentBan", "BanPerma", "BanPermanent", "PBan", "BanP",
            "RemoveBan", "BanRemove", "DeleteBan", "BanDelete", "UBan", "UnBan", "BanU")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task BanUser(string targetStr, string time, [Remainder] string reason)
        {
            var target = await GetBannedUser(targetStr);
            if (target == null)
            {
                await ReplyAsync("User could not be found.");
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
@"Invalid time: 'ban [@user] [time] [reason]'.
Please use with X as number:
'Xs' or 'Xsec' for seconds,
'Xm' or 'Xmin' for minutes,
'Xh', 'Xst' for hours,
'Xd', 'Xt' for days,
'-1', '-', 'perma' or 'never' for perma,
'0', 'unban', 'no' or 'stop' for unban.");
                return;
            }

            string banId = $"{target.Id}-Ban";

            var previousBan = _databaseHandler.Get<CaseEntity>(banId);
            if (previousBan != null)
            {
                await ReplyAsync("Previous ban removed:");
                await ReplyAsync(embed: previousBan.ToEmbed(Context.Client));
                _databaseHandler.Delete(previousBan);
            }

            // Gives exception if there is no ban
            try
            {
                await Context.Guild.RemoveBanAsync(target);
            }
            catch
            {
                // ignored
            }

            if (dateTimeOffset.HasValue || isPerma)   // not unban
            {
                var dmChannel = await target.GetOrCreateDMChannelAsync();
                await dmChannel?.SendMessageAsync($"You've been banned in TDS-V Discord server by {Context.User.Username}. Reason: {reason}");
                await dmChannel?.SendMessageAsync($"Expires: {(isPerma ? "never" : dateTimeOffset.Value.ToString())}");

                await Context.Guild.AddBanAsync(target, 0, reason);
                var caseEntity = new CaseEntity
                {
                    Id = banId,
                    UserId = target.Id,
                    CaseType = isPerma ? CaseType.Ban : CaseType.TempBan,
                    CreatedOn = DateTimeOffset.Now,
                    GuildId = Context.Guild.Id,
                    ModeratorId = Context.User.Id,
                    Reason = reason,
                };
                if (dateTimeOffset.HasValue)
                    caseEntity.ExpiresOn = dateTimeOffset.Value;
                _databaseHandler.Save(caseEntity);
                await ReplyAsync($"Ban saved for {target.Username}. Use 'baninfo [@user]' or 'info [@user]' for informations.");
            }
            else
            {
                await ReplyAsync($"The user {target.Username} got unbanned.");
                var dmChannel = await target.GetOrCreateDMChannelAsync();
                dmChannel?.SendMessageAsync($"You got unbanned in TDS-V Discord server by {Context.User.Username}. Reason: {reason}");
            }

        }

        [Command("baninfo")]
        [Alias("infoban")]
        public async Task<RuntimeResult> BanInfo(string targetStr)
        {
            var user = await GetBannedUser(targetStr);
            if (user == null)
                return Reply("The user doesn't exist: 'baninfo @user'");

            string banId = $"{user.Id}-Ban";
            var ban = _databaseHandler.Get<CaseEntity>(banId);
            if (ban == null)
                return Reply("The user is not banned.");

            return Reply(ban.ToEmbedBuilder(Context.Client));
        }

        private async Task<IUser> GetBannedUser(string targetStr)
        {
            
            IUser target = await GetMentionedUser(targetStr, null, false, false);
            if (target != null)
                return target;

            var bans = await Context.Guild.GetBansAsync();
            var targetBans = bans.Where(b =>
                b.User.Id.ToString() == targetStr
                || b.User.Username.Equals(targetStr, StringComparison.CurrentCultureIgnoreCase)
                || b.User.Discriminator.Equals(targetStr, StringComparison.CurrentCultureIgnoreCase)
                || $"{b.User.Username}#{b.User.Discriminator}".Equals(targetStr, StringComparison.CurrentCultureIgnoreCase));

            if (!targetBans.Any())
                return null;

            var byId = targetBans.Where(b => b.User.Id.ToString() == targetStr);
            if (byId.Any())
                return byId.First().User;

            var byUniqueName = targetBans.Where(b => $"{b.User.Username}#{b.User.Discriminator}".Equals(targetStr, StringComparison.CurrentCultureIgnoreCase));
            if (byUniqueName.Any())
                return byUniqueName.First().User;

            var byDiscriminator = targetBans.Where(b => b.User.Discriminator.Equals(targetStr, StringComparison.CurrentCultureIgnoreCase));
            if (byDiscriminator.Any())
                return byDiscriminator.First().User;

            var byName = targetBans.Where(b => b.User.Username.Equals(targetStr, StringComparison.CurrentCultureIgnoreCase));
            return byName.First().User;
        }

    }
}
