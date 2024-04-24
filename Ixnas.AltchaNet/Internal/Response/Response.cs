using System;

namespace Ixnas.AltchaNet.Internal.Response
{
    [Serializable]
    internal class Response
    {
        public string Algorithm { get; set; }
        public string Challenge { get; set; }
        public int Number { get; set; }
        public string Salt { get; set; }
        public string Signature { get; set; }
    }
}
