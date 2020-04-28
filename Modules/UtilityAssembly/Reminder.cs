using System;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using Discord.Commands;

namespace UtilityAssembly
{
    partial class UtilityModule
    {
        [Command("reminder")]
        [Alias("SetReminder", "AddReminder", "ReminderSet", "ReminderAdd")]
        public async Task CreateReminder(string time, [Remainder] string content)
        {
            if (!GetTime(time, out DateTimeOffset? dateTimeOffset, out bool isPerma)
                || /* Unmute */ !dateTimeOffset.HasValue
                || /* Perma */ isPerma)
            {
                await ReplyAsync(
@"Invalid time: 'reminder [afterTime] [content]'.
Please use with X as number:
'Xs' or 'Xsec' for seconds,
'Xm' or 'Xmin' for minutes,
'Xh', 'Xst' for hours,
'Xd', 'Xt' for days,
'-1', '-', 'perma' or 'never' for perma,
'0', 'unban', 'no' or 'stop' for unban.");
                return;
            }

            var reminderEntity = new ReminderEntity
            {
                Id = new Guid(),
                ExpiresOn = dateTimeOffset.Value,
                GuildId = Context.Guild.Id, 
                Content = content
            };
            if (Context.IsPrivate)
                reminderEntity.UserId = Context.User.Id;
            else 
                reminderEntity.ChannelId = Context.Channel.Id;

            _databaseHandler.Save(reminderEntity);
            await ReplyAsync($"Reminder is set for {dateTimeOffset.Value}");
        }
    }
}
