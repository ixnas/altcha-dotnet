using Ixnas.AltchaNet.Exceptions;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the complexity for generating ALTCHA challenges.
    ///     Tweaks the computational effort required to solve a challenge.
    /// </summary>
    public readonly struct AltchaComplexity
    {
        /// <summary>
        ///     Minimum complexity.
        /// </summary>
        public int Min { get; }
        /// <summary>
        ///     Maximum complexity.
        /// </summary>
        public int Max { get; }

        /// <summary>
        ///     Constructs complexity from a specified range.
        /// </summary>
        /// <param name="min">Minimum complexity.</param>
        /// <param name="max">Maximum complexity.</param>
        /// <exception cref="InvalidComplexityException">Thrown when attempting to set invalid complexity ranges.</exception>
        public AltchaComplexity(int min, int max)
        {
            if (min < 0 || max < 0 || min > max)
                throw new InvalidComplexityException();
            Min = min;
            Max = max;
        }
    }
}
