using Newtonsoft.Json;
using System;

namespace Victoria.Entities
{
    public struct EqualizerBand
    {
        /// <summary>
        /// 15 bands (0-14) that can be changed.
        /// </summary>
        [JsonProperty("band")]
        public ushort Band { get; set; }

        /// <summary>
        /// Gain is the multiplier for the given band. The default value is 0. Valid values range from -0.25 to 1.0, 
        /// where -0.25 means the given band is completely muted, and 0.25 means it is doubled.
        /// </summary>
        [JsonProperty("gain")]
        public double Gain { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is not EqualizerBand other)
                return false;
            return Band == other.Band && Gain == other.Gain;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Band, Gain);
        }

        public static bool operator ==(EqualizerBand left, EqualizerBand right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EqualizerBand left, EqualizerBand right)
        {
            return !(left == right);
        }
    }
}