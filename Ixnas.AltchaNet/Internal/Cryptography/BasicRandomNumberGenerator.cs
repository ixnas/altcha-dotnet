using System;

namespace Ixnas.AltchaNet.Internal.Cryptography
{
    internal class BasicRandomNumberGenerator : IRandomNumberGenerator
    {
        public int Generate(int min, int max)
        {
            return new Random().Next(min, max + 1);
        }
    }
}
