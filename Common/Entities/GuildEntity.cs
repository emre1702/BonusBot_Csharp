using BonusBot.Common.Attributes;
using Common.Attributes;

namespace BonusBot.Common.Entities
{
    public sealed class GuildEntity : BaseEntity
    {
        public char Prefix { get; set; }
        public ulong MuteRoleId { get; set; }
        public string RoleForMutedSuffix { get; set; }
        public ulong LogChannelId { get; set; }
        public ulong WelcomeChannelId { get; set; }
        public ulong UserLeftLogChannelId { get; set; }
        public ulong GermanRoleId { get; set; }
        public ulong TurkishRoleId { get; set; }
        public ulong AudioBotUserRoleId { get; set; }
        public ulong AudioCommandChannelId { get; set; }
        public ulong TagsManagerRoleId { get; set; }
        public ulong RolesRequestChannelId { get; set; }
        public ulong AudioInfoChannelId { get; set; }
        public string GitHubWebHookListenToUrl { get; set; }
        public string WelcomeMessage { get; set; }
        public string UserLeftMessage { get; set; }
        public bool UseRolesCommandSystem { get; set; }

        [GitHubWebHookSettingProperty]
        public ulong GitHubWebHookPushChannelId { get; set; }
        [GitHubWebHookSettingProperty]
        public ulong GitHubWebHookIssueOpenedChannelId { get; set; }
        [GitHubWebHookSettingProperty]
        public ulong GitHubWebHookIssueClosedChannelId { get; set; }
        [GitHubWebHookSettingProperty]
        public ulong GitHubWebHookIssueCommentChannelId { get; set; }
        [GitHubWebHookSettingProperty]
        public ulong GitHubWebHookIssueInitialCommentEditedChannelId { get; set; }
        [GitHubWebHookSettingProperty]
        public ulong GitHubWebHookIssueNeedTestingChannelId { get; set; }
        [GitHubWebHookSettingProperty]
        public ulong GitHubWebHookIssueHelpWantedChannelId { get; set; }
        [GitHubWebHookSettingProperty]
        public ulong GitHubWebHookErrorOutputChannelId { get; set; }
        [GitHubWebHookSettingProperty]
        public bool GitHubWebHookIssueNeedTestingDeleteAfterRemove { get; set; }
        [GitHubWebHookSettingProperty]
        public bool GitHubWebHookIssueHelpWantedDeleteAfterRemove { get; set; }
        [GitHubWebHookSettingProperty]
        public string GitHubWebHookIssueBugTitlePrefix { get; set; }
        [GitHubWebHookSettingProperty]
        public string GitHubWebHookIssueSuggestionTitlePrefix { get; set; }

        [NotConfigurableProperty]
        public uint LastPlayerVolume { get; set; }
    }
}
