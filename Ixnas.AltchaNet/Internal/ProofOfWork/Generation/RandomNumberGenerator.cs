namespace Ixnas.AltchaNet.Internal.ProofOfWork.Generation
{
    internal class RandomNumberGenerator
    {
        public int Max => _complexity.Max;
        private readonly AltchaComplexityCounterRange _complexity;

        public RandomNumberGenerator(AltchaComplexityCounterRange complexity)
        {
            _complexity = complexity;
        }

        public int Generate(AltchaComplexityCounterRange complexityOverride)
        {
            var complexity = complexityOverride ?? _complexity;
            var min = complexity.Min;
            var max = complexity.Max;

            return System.Security.Cryptography.RandomNumberGenerator.GetInt32(min, max + 1);
        }
    }
}
