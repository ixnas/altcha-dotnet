using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Ixnas.AltchaNet.Internal.Common.Converters
{
    internal static class ByteConverter
    {
        public static string GetHexStringFromBytes(byte[] bytes)
        {
            return BitConverter.ToString(bytes)
                               .Replace("-", string.Empty)
                               .ToLower();
        }

        public static Result<byte[]> GetByteArrayFromHexString(string hexString)
        {
            var isValidHexString = Regex.Match(hexString, "^([A-Fa-f0-9][A-Fa-f0-9]){1,}$")
                                        .Success;
            if (!isValidHexString)
                return Result<byte[]>.Fail();

            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < hexString.Length; i += 2)
            {
                var @string = hexString.Substring(i, 2);
                var @byte = Convert.ToByte(@string, 16);
                bytes[i / 2] = @byte;
            }

            return Result<byte[]>.Ok(bytes);
        }

        public static byte[] GetByteArrayFromUtf8String(string utf8String)
        {
            return Encoding.UTF8.GetBytes(utf8String);
        }
    }
}
