using Discord.Commands;
using BonusBot.Common.Attributes;
using BonusBot.Common.ExtendedModules;
using System.Threading.Tasks;
using BonusBot.Common.Handlers;
using BonusBot.Common.Entities;
using Discord.WebSocket;
using Discord;

namespace RolesAssembly
{
    [RequireContext(ContextType.Guild)]
    [RolesModuleProviso]
    public sealed class RolesModule : CommandBase
    {
        private readonly DatabaseHandler _databaseHandler;
        private GuildEntity _guildEntity;
        private readonly RolesHandler _rolesHandler;

        public RolesModule(DatabaseHandler databaseHandler, RolesHandler rolesHandler)
        {
            _databaseHandler = databaseHandler;
            _rolesHandler = rolesHandler;
        }

        protected override void BeforeExecute(CommandInfo command)
        {
            _guildEntity = _databaseHandler.Get<GuildEntity>(Context.Guild.Id);
            base.BeforeExecute(command);
        }

        [Command("german")]
        [Alias("deutsch", "almanca")]
        public async Task<RuntimeResult> RequestGermanRole()
        {
            return await GiveRole(_guildEntity.GermanRoleId);
        }

        [Command("turkish")]
        [Alias("türkisch", "türkce")]
        public async Task<RuntimeResult> RequestTurkishRole()
        {
            return await GiveRole(_guildEntity.TurkishRoleId);
        }

        private async Task<RuntimeResult> GiveRole(ulong roleId)
        {
            if (roleId == default)
                return Reply("This role is not available at this server.");
            var role = _rolesHandler.GetRole(Context.User, roleId);
            if (role == null)
                return Reply("The role doesn't exist at this server.");
            await Context.User.AddRoleAsync(role);
            return Reply($"You got the {role.Name} role.");
        }
    }
}
