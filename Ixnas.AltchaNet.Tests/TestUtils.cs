using System.Collections.Generic;
using System.Text.Json;
using Ixnas.AltchaNet.Tests.Abstractions;

namespace Ixnas.AltchaNet.Tests
{
    internal static class TestUtils
    {
        public readonly static Dictionary<CommonServiceType, CommonServiceFactory> ServiceFactories =
            new Dictionary<CommonServiceType, CommonServiceFactory>
            {
                [CommonServiceType.Default] = new CommonDefaultServiceFactory(),
                [CommonServiceType.Api] = new CommonApiServiceFactory()
            };
        public readonly static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static byte[] GetKey()
        {
            var list = new List<byte>(64);
            for (byte i = 0; i < 64; i++)
                list.Add(i);

            return list.ToArray();
        }

        /// <summary>
        ///     Fake API secret with same format as a real one
        /// </summary>
        public static string GetApiSecret()
        {
            return "csec_bc52da57bed2f40a4ad9dfe146436543d5e533edbf6babcf";
        }
    }
}
