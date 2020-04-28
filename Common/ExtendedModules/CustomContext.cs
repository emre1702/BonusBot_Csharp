using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using BonusBot.Common.Helpers;
using System;

namespace BonusBot.Common.ExtendedModules
{
    public sealed class CustomContext : SocketCommandContext
    {
        public new SocketGuildUser User { get; }
        public SocketUser SocketUser { get; }

        public CustomContext(DiscordSocketClient client, SocketUserMessage msg) : base(client, msg)
        {
            User = msg.Author.CastTo<SocketGuildUser>();
            SocketUser = msg.Author;
            //SocketUser = base.User;
        }

        public async ValueTask<IUser> GetUserAsync(ulong id)
        {
            return Guild.GetUser(id).CastTo<IUser>() ?? (await Client.Rest.GetUserAsync(id)).CastTo<IUser>();
        }
    }
}
