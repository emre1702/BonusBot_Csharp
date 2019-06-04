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
    }
}
