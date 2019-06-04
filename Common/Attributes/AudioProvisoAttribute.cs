using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Helpers;
using Victoria;
using Discord;
using BonusBot.Common.Handlers;

namespace BonusBot.Common.Attributes
{
    public sealed class AudioProvisoAttribute : PreconditionAttribute
    {
        public bool PlayerNeeded { get; }
        public bool UserHasToBeInVoice { get; }
        public bool CreatePlayerIfNeeded { get; }

        public AudioProvisoAttribute(bool userHasToBeInVoice = default, bool playerNeeded = true, bool createPlayerIfNeeded = true)
        {
            PlayerNeeded = playerNeeded;
            UserHasToBeInVoice = userHasToBeInVoice;
            CreatePlayerIfNeeded = createPlayerIfNeeded;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var ctx = context.CastTo<CustomContext>();
            var lavaSocket = services.GetRequiredService<LavaSocketClient>();
            var player = lavaSocket.GetPlayer(ctx.Guild.Id);

            if (UserHasToBeInVoice && ctx.User.VoiceChannel is null)
                return PreconditionResult.FromError("You're not connected to a voice channel.");


            if (PlayerNeeded && player is null)
            {
                if (ctx.User.VoiceChannel is null)
                    return PreconditionResult.FromError("You're not connected to a voice channel.");
                if (!CreatePlayerIfNeeded)
                    return PreconditionResult.FromError("I'm not connected to a voice channel.");
                var newPlayerHandler = services.GetRequiredService<NewPlayerHandler>();
                await newPlayerHandler.ConnectAsync(ctx.User.VoiceChannel, ctx.Channel.CastTo<ITextChannel>());
            }
                
            return PreconditionResult.FromSuccess();
        }
    }
}
