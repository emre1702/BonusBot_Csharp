using System.Linq;
using BonusBot.Common.Entities;
using Discord.WebSocket;

namespace BonusBot.Common.Handlers
{
    public class RolesHandler
    {
        private DatabaseHandler _databaseHandler;

        public RolesHandler(DatabaseHandler databaseHandler)
        {
            _databaseHandler = databaseHandler;
        }

        public void ChangeRolesToMute(SocketGuildUser user)
        {
            var guildEntity = _databaseHandler.Get<GuildEntity>(user.Guild.Id);
            string muteRolesSuffix = guildEntity.RoleForMutedSuffix;

            var userRoleNames = user.Roles.Select(r => r.Name).ToHashSet();
            var giveMuteRoles = user.Guild.Roles.Where(r =>
                r.Id == guildEntity.MuteRoleId  // or it's the muteRole
                || (muteRolesSuffix != default 
                && r.Name.EndsWith(muteRolesSuffix)     // e.g. Rolename ends with "Muted" (muteRoleSuffix)
                && userRoleNames.Contains(r.Name.Substring(0, r.Name.Length - muteRolesSuffix.Length)))); // e.g. User got the same role without "Muted" (muteRoleSuffix)    
            user.AddRolesAsync(giveMuteRoles);

            var giveMuteRoleNamesHashset = giveMuteRoles.Select(r => r.Name.Substring(0, r.Name.Length - muteRolesSuffix.Length)).ToHashSet();
            var removeNotMuteRoles = user.Roles.Where(r => giveMuteRoleNamesHashset.Contains(r.Name));
            user.RemoveRolesAsync(removeNotMuteRoles);
        }

        public void ChangeRolesToUnmute(SocketGuildUser user)
        {
            var guildEntity = _databaseHandler.Get<GuildEntity>(user.Guild.Id);
            string muteRolesSuffix = guildEntity.RoleForMutedSuffix;
            if (muteRolesSuffix == default)
                return;

            var removeMuteRoles = user.Roles.Where(r => r.Id == guildEntity.MuteRoleId || r.Name.EndsWith(muteRolesSuffix));
            user.RemoveRolesAsync(removeMuteRoles);

            var addNotMuteRoleNames = removeMuteRoles.Select(r => r.Name.Substring(0, r.Name.Length - muteRolesSuffix.Length));
            var addNotMuteRoles = user.Guild.Roles.Where(r => addNotMuteRoleNames.Contains(r.Name));
            user.AddRolesAsync(addNotMuteRoles);
        }

        public SocketRole GetRole(SocketGuildUser user, ulong roleId)
        {
            var role = user.Guild.GetRole(roleId);
            if (role == null)
                return null;
            return GetRole(user, role.Name);
        }

        public SocketRole GetRole(SocketGuildUser user, string roleName)
        {
            bool isMuted = IsUserMuted(user);

            if (isMuted)
            {
                string muteRolesSuffix = _databaseHandler.Get<GuildEntity>(user.Guild.Id).RoleForMutedSuffix;
                var muteRole = user.Guild.Roles.FirstOrDefault(r => r.Name == roleName + muteRolesSuffix);
                if (muteRole != null)
                    return muteRole;
            }
            return user.Guild.Roles.FirstOrDefault(r => r.Name == roleName);
        }

        public bool IsUserMuted(SocketGuildUser user)
        {
            ulong muteRoleId = _databaseHandler.Get<GuildEntity>(user.Guild.Id).MuteRoleId;
            return user.Roles.Any(r => r.Id == muteRoleId);
        }
    }
}
