using System;
using System.Linq;
using System.Reflection;
using System.Text.Formatting;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using BonusBot.Common.Entities;
using Victoria;
using Discord.WebSocket;
using System.Globalization;

namespace BonusBot.Common.Helpers
{
    public static class ExtensionHelper
    {
        public static string SanitzeEntity(this string entity)
        {
            return entity.Replace("Entity", string.Empty);
        }

        public static string SanitizAssembly(this string module)
        {
            return module.Replace("Assembly", string.Empty);
        }

        public static string Replace(this string str, params object[] objs)
        {
            str = str.Replace("\\n", Environment.NewLine);
            foreach (var obj in objs)
            {
                switch (obj)
                {
                    case IUser user:
                        str = str.Replace("%u%", user.Mention);
                        str = str.Replace("%ud%", $"{user.Username}#{user.Discriminator}");
                        break;

                    case IGuild guild:
                        str = str.Replace("%g%", guild.Name);
                        if (guild is SocketGuild socketGuild)
                            str = str.Replace("%uc%", socketGuild.Users.Count.ToString());
                        break;

                    case ITextChannel channel:
                        str = str.Replace("%c%", channel.Mention);
                        break;

                    case IRole role:
                        str = str.Replace("%r%", role.Mention);
                        break;
                }
            }

            return str;
        }

        public static bool HasPrefixes(this IUserMessage message, ref int argpos, params char[] prefixes)
        {
            var content = message.Content;
            var shouldContinue = false;

            if (string.IsNullOrWhiteSpace(content))
                shouldContinue = false;

            foreach (var prefix in prefixes)
            {
                if (content[0] == prefix)
                {
                    shouldContinue = true;
                    break;
                }
            }

            argpos = shouldContinue ? 1 : 0;
            return shouldContinue;
        }

        public static IServiceCollection AddImplementedInterfaces(this IServiceCollection service, Assembly assembly,
            params Type[] interfaces)
        {
            if (assembly is null || interfaces.Length is 0)
                return service;

            foreach (var inter in interfaces)
            {
                var matches = assembly.GetTypes().Where(x => !x.IsAbstract && inter.IsAssignableFrom(x))
                    .ToArray();

                if (matches.Length is 0)
                    continue;

                foreach (var match in matches)
                    service.AddSingleton(match);
            }
            return service;
        }

        public static IServiceCollection AddServices(this IServiceCollection service, params Type[] types)
        {
            if (types.Length is 0)
                return service;

            foreach (var type in types)
            {
                service.AddSingleton(type);
            }

            return service;
        }

        public static bool IsTempCase(this CaseEntity caseEntity)
        {
            return caseEntity.CaseType == CaseType.TempBan
                   || caseEntity.CaseType == CaseType.TempMute
                   || caseEntity.CaseType == CaseType.TempBlock
                   || caseEntity.CaseType == CaseType.TempRoleBan;
        }

        public static double ConvertToMb(this long value)
        {
            return (double)value / 1024 / 1024;
        }

        public static T CastTo<T>(this object obj)
        {
            return obj is T val ? val : default;
        }

        public static T CastAs<T>(this object obj)
        {
            return (T)obj;
        }

        public static string ValueToString(this object value)
        {
            return value switch
            {
                LavaQueue queue => $"Queue with {queue.Count} items.",
                DateTime dateTime => dateTime.ToLocalTime().ToString(CultureInfo.DefaultThreadCurrentCulture),
                DateTimeOffset dateTimeOffset => dateTimeOffset.ToLocalTime().ToString(CultureInfo.DefaultThreadCurrentCulture),
                _ => value.ToString(),
            };
        }

        public static string ObjectToString(this object obj)
        {
            using var sf = new StringFormatter();
            var properties = obj.GetType().GetRuntimeProperties();

            var max = properties.Max(x => x.Name.Length) + 5;
            foreach (var property in properties)
            {
                var space = new string(' ', max - property.Name.Length);
                var value = property.GetValue(obj);                

                sf.Append($"{property.Name}:{space}{value.ValueToString()}\n");
            }

            return $"```ini\n===== [ {obj.GetType().Name} Information ] =====\n{sf}\n```";
        }
    }
}
