namespace Altcha.Net.Internal.Response
{
    internal class Response
    {
        public string Algorithm { get; set; }
        public string Challenge { get; set; }
        public int Number { get; set; }
        public string Salt { get; set; }
        public string Signature { get; set; }
    }
}
