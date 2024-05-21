using Ixnas.AltchaNet.Internal.Common.Salt;

namespace Ixnas.AltchaNet.Internal.Solving
{
    internal class ExpirySaltValidator : SaltValidator
    {
        private readonly SaltParser _saltParser;

        public ExpirySaltValidator(SaltParser saltParser)
        {
            _saltParser = saltParser;
        }

        public bool IsValid(string salt)
        {
            var parsedSalt = _saltParser
                .Parse(salt);
            return !parsedSalt.Value.HasExpired();
        }
    }
}
