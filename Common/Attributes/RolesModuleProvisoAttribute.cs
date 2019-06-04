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
    public sealed class RolesModuleProvisoAttribute : PreconditionAttribute
    {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ctx = context.CastTo<CustomContext>();
            var databaseHandler = services.GetRequiredService<DatabaseHandler>();
            var guildEntity = databaseHandler.Get<GuildEntity>(ctx.Guild.Id);

            if (guildEntity.RolesRequestChannelId != default) 
                if (ctx.Channel.Id != guildEntity.RolesRequestChannelId)
                    return Task.FromResult(PreconditionResult.FromError($"Only allowed in {ctx.Guild.GetChannel(guildEntity.RolesRequestChannelId)?.Name ?? "?"} channel."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
