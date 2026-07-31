#if !NET8_0_OR_GREATER
using System;
#endif

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class RandomNumberGenerator
    {
#if !NET8_0_OR_GREATER
        private readonly System.Security.Cryptography.RandomNumberGenerator _generator =
            System.Security.Cryptography.RandomNumberGenerator.Create();
#endif

        public int Generate(AltchaComplexityCounterRange complexity)
        {
            var min = complexity.Min;
            var max = complexity.Max;

#if NET8_0_OR_GREATER
            return System.Security.Cryptography.RandomNumberGenerator.GetInt32(min, max + 1);
#else
            // Algorithm based on https://gist.github.com/niik/1017834
            var diff = (long)max + 1 - min;
            var upperBound = uint.MaxValue / diff * diff;

            var buffer = new byte[sizeof(uint)];
            uint number;
            do
            {
                _generator.GetBytes(buffer);
                number = BitConverter.ToUInt32(buffer, 0);
            }
            while (number >= upperBound);

            return (int)(min + number % diff);
#endif
        }
    }
}
