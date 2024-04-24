using System;
using Ixnas.AltchaNet.Internal.Serialization;

namespace Ixnas.AltchaNet.Internal.Salt
{
    internal class TimestampedSaltGenerator : ISaltGenerator
    {
        [Serializable]
        internal class TimestampedSalt
        {
            public long T { get; set; }
            public int R { get; set; }
        }

        private readonly IJsonSerializer _serializer;

        public TimestampedSaltGenerator(IJsonSerializer serializer)
        {
            _serializer = serializer;
        }

        public string Generate()
        {
            return GetTimestampAndGuid();
        }

        private string GetTimestampAndGuid()
        {
            var timestampedSalt = new TimestampedSalt
            {
                T = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                R = new Random().Next(1000, 9999)
            };
            return _serializer.ToBase64Json(timestampedSalt);
        }
    }
}
