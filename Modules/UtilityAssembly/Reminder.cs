using System;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using Discord;
using Discord.Commands;

namespace UtilityAssembly
{
    partial class UtilityModule
    {
        [Command("reminder")]
        [Alias("SetReminder", "AddReminder", "ReminderSet", "ReminderAdd")]
        [RequireContext(ContextType.DM | ContextType.Group | ContextType.Guild)]
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
                Id = new Guid().ToString(),
                ExpiresOn = dateTimeOffset.Value,
                Content = content
            };
            if (!Context.IsPrivate && Context.User.GetPermissions(Context.Channel as IGuildChannel).ManageMessages)
            {
                reminderEntity.ChannelId = Context.Channel.Id;
                reminderEntity.GuildId = Context.Guild.Id;
                _databaseHandler.Save(reminderEntity);
                await ReplyAsync($"Reminder is set for {dateTimeOffset.Value}");
            }
            else
            {
                reminderEntity.UserId = Context.SocketUser.Id;
                foreach (var guild in Context.Client.Guilds)
                {
                    if (guild.GetUser(reminderEntity.UserId) is { })
                        reminderEntity.GuildId = guild.Id;
                }
                _databaseHandler.Save(reminderEntity);
                if (Context.IsPrivate)
                    await ReplyAsync($"Reminder is set for {dateTimeOffset.Value}");
                else
                    await Context.SocketUser.SendMessageAsync($"Reminder is set for {dateTimeOffset.Value}");
            }
        }
    }
}
