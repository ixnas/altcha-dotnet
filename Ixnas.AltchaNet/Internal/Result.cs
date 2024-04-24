namespace Ixnas.AltchaNet.Internal
{
    internal class Result<T>
    {
        public bool Success { get; set; }
        public T Value { get; set; }
    }
}
