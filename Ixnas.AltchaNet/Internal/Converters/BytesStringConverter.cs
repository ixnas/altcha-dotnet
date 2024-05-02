using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Ixnas.AltchaNet.Internal.Converters
{
    internal class BytesStringConverter
    {
        public string GetHexStringFromBytes(byte[] bytes)
        {
            return BitConverter.ToString(bytes)
                               .Replace("-", string.Empty)
                               .ToLower();
        }

        public Result<byte[]> GetByteArrayFromHexString(string hexString)
        {
            var isValidHexString = Regex.Match(hexString, "^([A-Fa-f0-9][A-Fa-f0-9]){1,}$")
                                        .Success;
            if (!isValidHexString)
                return new Result<byte[]>();

            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < hexString.Length; i += 2)
            {
                var @string = hexString.Substring(i, 2);
                var @byte = Convert.ToByte(@string, 16);
                bytes[i / 2] = @byte;
            }

            return new Result<byte[]>
            {
                Success = true,
                Value = bytes
            };
        }

        public byte[] GetByteArrayFromUtf8String(string utf8String)
        {
            return Encoding.UTF8.GetBytes(utf8String);
        }
    }
}
