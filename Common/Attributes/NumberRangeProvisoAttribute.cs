using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace BonusBot.Common.Attributes
{
    public sealed class NumberRangeProvisoAttribute : ParameterPreconditionAttribute
    {
        public int Min { get; }
        public int Max { get; }

        public NumberRangeProvisoAttribute(int min = int.MinValue, int max = int.MaxValue)
        {
            Min = min;
            Max = max;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            int val = Convert.ToInt32(value);
            return Min >= 0 && val <= Max
                ? Task.FromResult(PreconditionResult.FromSuccess()) 
                : Task.FromResult(PreconditionResult.FromError($"The value for {parameter.Name} has to be between {Min} and {Max}."));
        }
    }
}
