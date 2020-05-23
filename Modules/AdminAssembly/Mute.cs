using System;
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

        [Command("mute")]
        [Alias("TimeMute", "TMute", "MuteTime", "MuteT", "PermaMute", "PMute", "MuteP",
            "MutePerma", "PermanentMute", "MutePermanent", "UMute", "MuteU", "UnMute", "MuteUn",
            "RemoveMute", "MuteRemove", "DeleteMute", "MuteDelete", "DelMute", "MuteDel")]
        [RequireUserPermission(GuildPermission.MuteMembers)]
        public async Task MuteMember(string targetMention, string time, [Remainder] string reason)
        {
            var targetSocketUser = await GetMentionedUser(targetMention, "mute [@user] [time] [reason]");
            if (targetSocketUser == null)
                return;

            string muteId = $"{targetSocketUser.Id}-Mute";

            var previousMute = _databaseHandler.Get<CaseEntity>(muteId);
            if (previousMute != null)
            {
                await ReplyAsync("Previous mute removed:");
                await ReplyAsync(embed: previousMute.ToEmbed(Context.Client));
                _databaseHandler.Delete(previousMute);
            }

            if (!GetTime(time, out DateTimeOffset? dateTimeOffset, out bool isPerma))
            {
                await ReplyAsync(
@"Invalid time: 'mute [@user] [time] [reason]'. 
Please use with X as number:
'Xs' or 'Xsec' for seconds,
'Xm' or 'Xmin' for minutes,
'Xh', 'Xst' for hours,
'Xd', 'Xt' for days,
'-1', '-', 'perma', 'permamute' or 'never' for perma,
'0', 'unmute', 'no' or 'stop' for unmute.");
                return;
            }

            if (!dateTimeOffset.HasValue && !isPerma)   // unmute
            {
                if (previousMute != null && (targetSocketUser is SocketGuildUser target))
                    _rolesHandler.ChangeRolesToUnmute(target);
                await ReplyAsync($"Unmuted user {targetSocketUser.Username}.");
            }
            else
            {
                if (previousMute == null && (targetSocketUser is SocketGuildUser target))
                    _rolesHandler.ChangeRolesToMute(target);
                var caseEntity = new CaseEntity
                {
                    Id = muteId.ToString(),
                    UserId = targetSocketUser.Id,
                    CaseType = isPerma ? CaseType.Mute : CaseType.TempMute,
                    CreatedOn = DateTimeOffset.Now,
                    GuildId = Context.Guild.Id,
                    ModeratorId = Context.User.Id,
                    Reason = reason,
                };
                if (dateTimeOffset.HasValue)
                    caseEntity.ExpiresOn = dateTimeOffset.Value;
                _databaseHandler.Save(caseEntity);
                await ReplyAsync($"Mute saved for user {targetSocketUser.Username}. Use 'muteinfo [@user]' or 'info [@user]' for informations.");
            }
        }

        [Command("muteinfo")]
        [Alias("infomute")]
        public Task MuteInfo(string targetStr)
        {
            var user = GetUser(targetStr);
            if (user == null)
                return ReplyAsync("The user doesn't exist: 'muteinfo [@user]'");

            string muteId = $"{user.Id}-Mute";
            var mute = _databaseHandler.Get<CaseEntity>(muteId);
            if (mute == null)
                return ReplyAsync("The user is not muted.");

            return ReplyAsync(mute.ToEmbedBuilder(Context.Client));
        }


    }
}
