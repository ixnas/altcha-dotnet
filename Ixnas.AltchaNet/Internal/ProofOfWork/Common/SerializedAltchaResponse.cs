using System;

namespace Ixnas.AltchaNet.Internal.ProofOfWork.Common
{
    [Serializable]
    internal class SerializedAltchaResponse
    {
        public string Challenge { get; set; }
        public int Number { get; set; }
        public string Salt { get; set; }
        public string Signature { get; set; }
        public string Algorithm { get; set; }
    }
}
