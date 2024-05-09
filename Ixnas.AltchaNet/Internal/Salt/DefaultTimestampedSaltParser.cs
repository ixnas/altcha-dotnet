using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class DefaultTimestampedSaltParser : TimestampedSaltParser
    {
        private readonly JsonSerializer _serializer;

        public DefaultTimestampedSaltParser(JsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public Result<TimestampedSalt> Parse(string salt)
        {
            if (salt == null)
                return new Result<TimestampedSalt>();
            var parsed = DefaultTimestampedSalt.Parse(_serializer, salt);
            return new Result<TimestampedSalt>
            {
                Success = parsed.Success,
                Value = parsed.Value
            };
        }
    }
}
