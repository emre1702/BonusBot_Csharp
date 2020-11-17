using System;

namespace BonusBot.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotConfigurablePropertyAttribute : Attribute
    {
    }
}
