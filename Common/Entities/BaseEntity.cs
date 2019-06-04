using BonusBot.Common.Attributes;

namespace BonusBot.Common.Entities
{
    public class BaseEntity
    {
        [NotConfigurableProperty]
        public object Id { get; set; }
    }
}
