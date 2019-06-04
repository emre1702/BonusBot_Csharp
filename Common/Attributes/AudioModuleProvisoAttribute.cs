using System;
using System.Linq;
using System.Threading.Tasks;
using BonusBot.Common.Defaults;
using BonusBot.Common.Entities;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Handlers;
using BonusBot.Common.Helpers;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BonusBot.Common.Attributes
{
    public sealed class AudioModuleProvisoAttribute : PreconditionAttribute
    {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ctx = context.CastTo<CustomContext>();
            var databaseHandler = services.GetRequiredService<DatabaseHandler>();
            var guildEntity = databaseHandler.Get<GuildEntity>(ctx.Guild.Id);

            if (guildEntity.AudioCommandChannelId != default)
                if (ctx.Channel.Id != guildEntity.AudioCommandChannelId)
                    return Task.FromResult(PreconditionResult.FromError($"Only allowed in {ctx.Guild.GetTextChannel(guildEntity.AudioCommandChannelId)?.Name ?? "?"} channel."));

            if (guildEntity.AudioBotUserRoleId != default)
                if (!ctx.User.Roles.Any(r => r.Id == guildEntity.AudioBotUserRoleId))
                    return Task.FromResult(PreconditionResult.FromError($"Only allowed with the {ctx.Guild.GetRole(guildEntity.AudioBotUserRoleId)?.Name ?? "?"} role."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
