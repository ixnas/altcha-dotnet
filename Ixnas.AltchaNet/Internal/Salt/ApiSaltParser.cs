namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class ApiSaltParser : TimestampedSaltParser
    {
        public Result<TimestampedSalt> Parse(string salt)
        {
            if (salt == null)
                return new Result<TimestampedSalt>();
            var parseResult = ApiSalt.Parse(salt);
            return new Result<TimestampedSalt>
            {
                Success = parseResult.Success,
                Value = parseResult.Value
            };
        }
    }
}
