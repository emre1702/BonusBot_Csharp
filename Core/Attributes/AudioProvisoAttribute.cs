using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Helpers;
using Victoria;

namespace BonusBot.Attributes
{
    public sealed class AudioProvisoAttribute : PreconditionAttribute
    {
        public bool PlayerCheck { get; }

        public AudioProvisoAttribute(bool playerCheck = default)
        {
            PlayerCheck = playerCheck;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ctx = context.CastTo<CustomContext>();
            var lavaSocket = services.GetRequiredService<LavaSocketClient>();
            var player = lavaSocket.GetPlayer(ctx.Guild.Id);

            if (ctx.User.VoiceChannel is null)
                return Task.FromResult(PreconditionResult.FromError("You're not connected to a voice channel."));


            if (PlayerCheck && player is null)
                return Task.FromResult(PreconditionResult.FromError("There is no player created for this guild."));

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
