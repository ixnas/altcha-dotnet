using Ixnas.AltchaNet.Exceptions;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the counter range for the complexity of generating ALTCHA challenges.
    /// </summary>
    public sealed record AltchaComplexityCounterRange
    {
        internal int Min { get; }
        internal int Max { get; }

        /// <summary>
        ///     Constructs counter range from a minimum and maximum value.
        /// </summary>
        /// <param name="min">Minimum counter.</param>
        /// <param name="max">Maximum counter.</param>
        /// <exception cref="InvalidComplexityException">Thrown when attempting to set invalid counter ranges.</exception>
        public AltchaComplexityCounterRange(int min, int max)
        {
            if (min < 0 || max < 0 || min > max)
                throw new InvalidComplexityException();
            Min = min;
            Max = max;
        }
    }
}
