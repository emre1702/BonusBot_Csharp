using Discord.Commands;
using BonusBot.Common.ExtendedModules;
using BonusBot.Common.Handlers;
using BonusBot.Common.Entities;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using Discord;
using System.Reflection;
using System;
using BonusBot.Common.Helpers;
using BonusBot.Common.Attributes;
using System.Collections.Generic;

namespace BotGuildSettingsAssembly
{
    [RequireContext(ContextType.Guild)]
    [Group("config")]
    [RequireUserPermission(GuildPermission.Administrator | GuildPermission.ManageGuild, ErrorMessage = "Only users with 'Administrator' right are allowed!")]
    public sealed class BotGuildSettingsModule : CommandBase
    {
        private readonly DatabaseHandler _databaseHandler;
        private readonly RoleReactionHandler _roleReactionHandler;
        private GuildEntity _guildEntity;

        public BotGuildSettingsModule(DatabaseHandler databaseHandler, RoleReactionHandler roleReactionHandler)
        {
            _databaseHandler = databaseHandler;
            _roleReactionHandler = roleReactionHandler;
        }


        protected override void BeforeExecute(CommandInfo command)
        {
            _guildEntity = _databaseHandler.Get<GuildEntity>(Context.Guild.Id);
            base.BeforeExecute(command);
        }

        protected override void AfterExecute(CommandInfo command)
        {
            _databaseHandler.Save(_guildEntity);
            _roleReactionHandler.InitForGuild(Context.Guild);
            base.AfterExecute(command);
        }

        [Command("help")]
        public Task Start()
        {
            var builder = new StringBuilder();
            builder.AppendLine("With the config command you are able to configurate the guild settings.");
            builder.AppendLine("To do this, use 'config [SETTING] [VALUE]'.");
            builder.AppendLine("Example: '!config Prefix ?'");
            builder.AppendLine("Possible settings are:");
            builder.AppendJoin(", ", GetAvailablePropertyNames());

            var notSetted = GetNotSettedProperties();
            if (notSetted.Any())
            {
                builder.AppendLine("\n");
                builder.AppendLine("Not set settings are:");
                builder.AppendJoin(", ", notSetted);
            }
            return ReplyAsync(builder.ToString());
        }

        [Command]
        public Task Set(string setting, [Remainder] string value)
        {
            PropertyInfo prop = _guildEntity.GetType().GetProperty(setting, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite || prop.GetCustomAttribute<NotConfigurablePropertyAttribute>() != null)
                return ReplyAsync("The setting does not exist. Use 'config help' for all possible settings.");

            object setValue = SetStringToType(value.Trim(), prop.PropertyType);
            if (setValue == null)
                return ReplyAsync($"Invalid value! The type has to be {prop.PropertyType.Name}.");

            prop.SetValue(_guildEntity, setValue, null);
            return ReplyAsync("Setting changed successfully.");

            //todo restart GitHubListener after changing a github setting
        }

        [Command("get")]
        [Priority(1)]
        public Task Get(string setting)
        {
            PropertyInfo prop = _guildEntity.GetType().GetProperty(setting, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite || prop.GetCustomAttribute<NotConfigurablePropertyAttribute>() != null)
                return ReplyAsync("The setting does not exist. Use 'config help' for all possible settings.");

            return ReplyAsync(prop.GetValue(_guildEntity).ToString());
        }

        private IEnumerable<string> GetAvailablePropertyNames()
        {
            return _guildEntity.GetType()
                .GetProperties()
                .Where(p => p.CanWrite
                    && p.GetCustomAttribute<NotConfigurablePropertyAttribute>() == null)
                 .Select(p => $"{p.Name} ({p.PropertyType.Name})" );
        }

        private IEnumerable<string> GetNotSettedProperties()
        {
            return _guildEntity.GetType().GetProperties()
                .Where(p => p.CanWrite
                    && p.PropertyType != typeof(bool)   // false is default
                    && p.GetCustomAttribute<NotConfigurablePropertyAttribute>() == null
                    && IsDefault(p.GetValue(_guildEntity)))
                .Select(p => $"{p.Name} ({p.PropertyType.Name})");
        }

        private bool IsDefault(object value)
        {
            if (value == null)
                return true;
            Type type = value.GetType();
            if (!type.IsValueType)
                return false;
            if (Nullable.GetUnderlyingType(type) != null)
                return false;
            object defaultValue = Activator.CreateInstance(type);
            return value.Equals(defaultValue);
        }

        private object SetStringToType(string value, Type newType)
        {
            switch (newType)
            {
                case Type uintType when uintType == typeof(uint):
                    if (!uint.TryParse(value, out uint uintResult))
                        return null;
                    return uintResult;
                case Type intType when intType == typeof(int):
                    if (!int.TryParse(value, out int intResult))
                        return null;
                    return intResult;
                case Type ulongType when ulongType == typeof(ulong):
                    if (!ulong.TryParse(value, out ulong ulongResult))
                        return null;
                    return ulongResult;
                case Type charType when charType == typeof(char):
                    if (value.Length != 1)
                        return null;
                    return value[0];
                case Type boolType when boolType == typeof(bool):
                    if (value != "1" && value != "0" && !value.Equals("true", StringComparison.CurrentCultureIgnoreCase) && !value.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                        return null;
                    return value == "1" || value.Equals("true", StringComparison.CurrentCultureIgnoreCase);
            }
            return value;
        }
    }
}
