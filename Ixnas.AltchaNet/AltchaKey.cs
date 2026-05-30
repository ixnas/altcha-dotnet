using Ixnas.AltchaNet.Exceptions;
using Ixnas.AltchaNet.Internal;
using Ixnas.AltchaNet.Internal.Common.Utilities;

namespace Ixnas.AltchaNet
{
    /// <summary>
    ///     Represents the secret key that is used for generating and validating challenges.
    /// </summary>
#if NET8_0_OR_GREATER
    public sealed record AltchaKey
#else
    public sealed class AltchaKey
#endif
    {
        internal byte[] Bytes { get; private set; }

        private AltchaKey()
        {
        }

        /// <summary>
        ///     Creates a key from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array to generate a key from.</param>
        /// <returns>A new AltchaKey instance.</returns>
        /// <exception cref="InvalidKeyException">Thrown when attempting to set a key of an invalid size.</exception>
        public static AltchaKey FromBytes(byte[] bytes)
        {
            Guard.NotNull(bytes);
            if (bytes.Length < Defaults.RequiredKeySize)
                throw new InvalidKeyException();
            return new AltchaKey
            {
                Bytes = bytes
            };
        }

        internal static AltchaKey FromBytesApiKey(byte[] bytes)
        {
            return new AltchaKey
            {
                Bytes = bytes
            };
        }
    }
}
