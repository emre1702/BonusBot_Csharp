using Discord;
using Discord.WebSocket;

namespace Common.Handlers
{
    public class ModuleEventsHandler
    {
        public delegate void GitHubWebHookSettingChangedDelegate(SocketGuild guild);

        #pragma warning disable 67
        public static event GitHubWebHookSettingChangedDelegate GitHubWebHookSettingChanged;
        #pragma warning restore 67

        public static void OnGitHubWebHookSettingChanged(SocketGuild guild)
        {
            GitHubWebHookSettingChanged?.Invoke(guild);
        }
    }
}
