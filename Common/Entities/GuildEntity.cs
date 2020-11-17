using BonusBot.Common.Attributes;
using Common.Attributes;
using Common.Interfaces;
using LiteDB;

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
        public ulong MemberRoleId { get; set; }
        public ulong GermanRoleId { get; set; }
        public ulong TurkishRoleId { get; set; }
        public ulong DevFeedRoleId { get; set; }
        public ulong TesterRoleId { get; set; }
        public ulong AudioBotUserRoleId { get; set; }
        public ulong AudioCommandChannelId { get; set; }
        public ulong TagsManagerRoleId { get; set; }
        public ulong RolesRequestChannelId { get; set; }
        public ulong AudioInfoChannelId { get; set; }
#pragma warning disable CA1056 // URI-like properties should not be strings
        public string GitHubWebHookListenToUrl { get; set; }
#pragma warning restore CA1056 // URI-like properties should not be strings
        public string WelcomeMessage { get; set; }
        public string UserLeftMessage { get; set; }
        public bool UseRolesCommandSystem { get; set; }
        public string RolesRequestInfoText { get; set; }


        public ulong SupportRequestCategoryId { get; set; }
        public ulong SupportRequestChannelInfoId { get; set; }
        public string SupportRequestInfo { get; set; }
        public uint SupportRequestMinTitleLength { get; set; }
        public uint SupportRequestMinTextLength { get; set; }
        public uint SupportRequestMaxTitleLength { get; set; }
        public uint SupportRequestMaxTextLength { get; set; }

        public ulong SupporterRoleId { get; set; }
        public ulong AdministratorRoleId { get; set; }


        [GitHubWebHookSettingPropertyAttribute]
        public ulong GitHubWebHookPushChannelId { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public ulong GitHubWebHookIssueOpenedChannelId { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public ulong GitHubWebHookIssueClosedChannelId { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public ulong GitHubWebHookIssueCommentChannelId { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public ulong GitHubWebHookIssueInitialCommentEditedChannelId { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public ulong GitHubWebHookIssueNeedTestingChannelId { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public ulong GitHubWebHookIssueHelpWantedChannelId { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public ulong GitHubWebHookErrorOutputChannelId { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public bool GitHubWebHookIssueNeedTestingDeleteAfterRemove { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public bool GitHubWebHookIssueHelpWantedDeleteAfterRemove { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public string GitHubWebHookIssueBugTitlePrefix { get; set; }
        [GitHubWebHookSettingPropertyAttribute]
        public string GitHubWebHookIssueSuggestionTitlePrefix { get; set; }

        [NotConfigurableProperty]
        public uint LastPlayerVolume { get; set; }

        [BsonIgnore]
        public IGitHubListener GithubListener { get; set; }
    }
}
