using System;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class RandomNumberGenerator
    {
        public int Max { get; }
#if !NET8_0_OR_GREATER
        private readonly System.Security.Cryptography.RandomNumberGenerator _generator =
            System.Security.Cryptography.RandomNumberGenerator.Create();
#endif
        private readonly int _min;

        public RandomNumberGenerator(int min, int max)
        {
            _min = min;
            Max = max;
        }

        public int Generate()
        {
#if NET8_0_OR_GREATER
            return System.Security.Cryptography.RandomNumberGenerator.GetInt32(_min, Max + 1);
#else
            // Algorithm based on https://gist.github.com/niik/1017834
            var diff = (long)Max + 1 - _min;
            var upperBound = uint.MaxValue / diff * diff;

            var buffer = new byte[sizeof(uint)];
            uint number;
            do
            {
                _generator.GetBytes(buffer);
                number = BitConverter.ToUInt32(buffer, 0);
            }
            while (number >= upperBound);

            return (int)(_min + number % diff);
#endif
        }
    }
}
