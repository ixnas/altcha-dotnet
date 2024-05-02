using System;

namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal class RandomNumberGenerator
    {
        public int Generate(int min, int max)
        {
            return new Random().Next(min, max + 1);
        }
    }
}
