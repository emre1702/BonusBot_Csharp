using System;
using System.Threading.Tasks;
using BonusBot.Common.Attributes;
using Discord.Commands;

namespace AudioAssembly
{
    partial class AudioModule
    {
        [Command("position"), AudioProviso(createPlayerIfNeeded: false)]
        [Priority(1)]
        public Task InvalidPosition(int value)
        {
            return ReplyAsync("Invalid input.\nPlease use X% for percent, Xs for seconds or minutes:seconds for a specific time with X as number.");
        }

        [Command("position"), AudioProviso(createPlayerIfNeeded: false)]
        public async Task<RuntimeResult> Position(string position)
        {
            TimeSpan? pos = null;
            if (position.EndsWith('%'))
            {
                pos = HandlePercentPosition(position);
                if (!pos.HasValue)
                    return Reply("Wrong usage of percent position. Example for right usage: 'position 50%'");
            }
            else if (position.EndsWith('s'))
            {
                pos = HandleSecondPosition(position);
                if (!pos.HasValue)
                    return Reply("Wrong usage of second position. Example for right usage: 'position 30s'");
            }
            else if (position.EndsWith('m'))
            {
                pos = HandleMinutePosition(position);
                if (!pos.HasValue)
                    return Reply("Wrong usage of minute position. Example for right usage: 'position 30m'");
            }
            else if (position.Contains(':'))
            {
                pos = HandleTimePosition(position);
                if (!pos.HasValue)
                    return Reply("Wrong usage of time position. Example for right usage: 'position 2:12' or 'position 1:01:0'");
            }

            if (pos.HasValue)
            {
                double percentage = Math.Round(pos.Value.Divide(player.CurrentTrack.Audio.Length) * 100 * 100) / 100;
                if (pos > player.CurrentTrack.Audio.Length)
                    return Reply($"This position ({pos} - {percentage}%) is larger than the actual length of the current track!");
                await player.SeekAsync(pos.Value);
                return Reply($"Set position to {pos} ({percentage}%)");
            }
            return Reply("Invalid input.\nPlease use X% for percent, Xs for seconds, Xm for minutes or minutes:seconds for a specific time with X as number.");
        }

        private TimeSpan? HandlePercentPosition(string position)
        {
            position = position.Remove(position.Length - 1);
            if (!double.TryParse(position, out double number))
                return null;
            return player.CurrentTrack.Audio.Length.Multiply(number / 100);
        }

        private TimeSpan? HandleSecondPosition(string position)
        {
            position = position.Remove(position.Length - 1);
            if (!double.TryParse(position, out double number))
                return null;
            return TimeSpan.FromSeconds(number);
        }

        private TimeSpan? HandleMinutePosition(string position)
        {
            position = position.Remove(position.Length - 1);
            if (!double.TryParse(position, out double number))
                return null;
            return TimeSpan.FromMinutes(number);
        }

        private TimeSpan? HandleTimePosition(string position)
        {
            string[] splitted = position.Split(':');
            if (!int.TryParse(splitted[^1], out int seconds))
                return null;
            if (!int.TryParse(splitted[^2], out int minutes))
                return null;

            int hours = 0;
            if (splitted.Length > 2 && !int.TryParse(splitted[^3], out hours))
                return null;

            return new TimeSpan(hours, minutes, seconds);
        }
    }
}
