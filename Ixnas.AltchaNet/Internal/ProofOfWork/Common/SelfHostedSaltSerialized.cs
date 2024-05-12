using System;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Common
{
    [Serializable]
    internal class SelfHostedSaltSerialized
    {
        public long T { get; set; } // Timestamp
        public int R { get; set; } // Random number
    }
}
