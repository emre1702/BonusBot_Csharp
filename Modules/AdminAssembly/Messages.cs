using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace AdminAssembly
{
    partial class AdminModule
    {
        [Command("DeleteMessages")]
        [Alias("DeleteMessage", "DelMessages", "DelMessage", "MessagesDelete", "MessageDelete", "MessagesDel", "MessageDel", "DelMsg", "MsgDel", "DeleteMsg", "MsgDelete")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task<RuntimeResult> DeleteMessagesAsync(int limit = 1)
        {
            var messages = await Context.Channel.GetMessagesAsync(limit).FlattenAsync();
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            return Reply($"Deleted {messages.Count()} messages.");
        }

        [Command("DeleteMessages")]
        [Alias("DeleteMessage", "DelMessages", "DelMessage", "MessagesDelete", "MessageDelete", "MessagesDel", "MessageDel", "DelMsg", "MsgDel", "DeleteMsg", "MsgDelete")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public async Task<RuntimeResult> DeleteMessagesAsync(string userMention, int limit = 1)
        {
            if (!MentionUtils.TryParseUser(userMention, out ulong userId))
                return Reply("Please mention a user.");

            var messagesToDelete = new List<IMessage>();
            IEnumerable<IMessage> messages;
            IMessage lastMessage = null;
            do
            {
                if (lastMessage == null)
                    messages = await Context.Channel.GetMessagesAsync().FlattenAsync();
                else
                    messages = await Context.Channel.GetMessagesAsync(lastMessage, Direction.Before).FlattenAsync();
                if (!messages.Any())
                    break;
                lastMessage = messages.Last();
                messages = messages.Where(m => m.Author.Id == userId);
                int amountFetched = messages.Count();
                if (amountFetched == 0)
                    continue;
                int missingAmount = limit - messagesToDelete.Count;
                if (amountFetched > missingAmount)
                    messages = messages.Take(missingAmount);

                messagesToDelete.AddRange(messages);
            }
            while (messagesToDelete.Count < limit);

            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messages);
            return Reply($"Deleted {messages.Count()} messages of the user.");
        }
    }
}
