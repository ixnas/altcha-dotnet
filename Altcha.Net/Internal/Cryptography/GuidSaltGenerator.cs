using System;

namespace Altcha.Net.Internal.Cryptography
{
    internal class GuidSaltGenerator : ISaltGenerator
    {
        public string Generate()
        {
            return Guid.NewGuid()
                       .ToString();
        }
    }
}
