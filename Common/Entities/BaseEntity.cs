using BonusBot.Common.Attributes;
using LiteDB;

namespace BonusBot.Common.Entities
{
    public class BaseEntity
    {
        [NotConfigurableProperty]
        public object Id { get; set; }
    }
}
