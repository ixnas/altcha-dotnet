using System;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class RandomNumberGenerator
    {
        public int Max { get; }
        private readonly int _min;

        public RandomNumberGenerator(int min, int max)
        {
            _min = min;
            Max = max;
        }

        public int Generate()
        {
            return new Random().Next(_min, Max + 1);
        }
    }
}
