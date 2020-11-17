using BonusBot.Common.Attributes;

namespace BonusBot.Common.Entities
{
    public class BaseEntity
    {
        [NotConfigurableProperty]
        public string Id { get; set; }
    }
}
