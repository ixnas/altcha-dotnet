namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class RandomNumberGenerator
    {
        public int Generate(AltchaComplexityCounterRange complexity)
        {
            var min = complexity.Min;
            var max = complexity.Max;
            return System.Security.Cryptography.RandomNumberGenerator.GetInt32(min, max + 1);
        }
    }
}
