using System;
using System.Linq;
using System.Threading.Tasks;
using BonusBot.Common.Entities;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Handlers;
using BonusBot.Common.Helpers;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace BonusBot.Common.Attributes
{
    public class TagManageProvisoAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ctx = context.CastTo<CustomContext>();
            var databaseHandler = services.GetRequiredService<DatabaseHandler>();
            ulong tagManagerRoleId = databaseHandler.Get<GuildEntity>(ctx.Guild.Id).TagsManagerRoleId;
            
            if (tagManagerRoleId == default)
                return Task.FromResult(PreconditionResult.FromSuccess());

            if (ctx.User.Roles.Any(r => r.Id == tagManagerRoleId))
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError("You are not allowed to do that."));
        }
    }
}
