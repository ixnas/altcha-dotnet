using System;

namespace Ixnas.AltchaNet.Exceptions
{
    /// <summary>
    ///     Base class for any exception within the ALTCHA library.
    /// </summary>
    public class AltchaException : Exception
    {
    }

    /// <summary>
    ///     Thrown when the API secret provided has an invalid format.
    /// </summary>
    public class InvalidApiSecretException : AltchaException
    {
    }

    /// <summary>
    ///     Thrown when attempting to set invalid complexity ranges.
    /// </summary>
    public class InvalidComplexityException : AltchaException
    {
    }

    /// <summary>
    ///     Thrown when attempting to set a zero or negative expiry time.
    /// </summary>
    public class InvalidExpiryException : AltchaException
    {
    }

    /// <summary>
    ///     Thrown when attempting to set an invalid key, probably because of an invalid key size.
    /// </summary>
    public class InvalidKeyException : AltchaException
    {
    }

    /// <summary>
    ///     Thrown when attempting to build an AltchaService without specifying the algorithm to be used.
    /// </summary>
    public class MissingAlgorithmException : AltchaException
    {
    }

    /// <summary>
    ///     Thrown when attemtping to build an AltchaApiService without specifying the API secret to be used.
    /// </summary>
    public class MissingApiSecretException : AltchaException
    {
    }

    /// <summary>
    ///     Thrown when attempting to build an AltchaService or AltchaApiService without specifying the store to be used.
    /// </summary>
    public class MissingStoreException : AltchaException
    {
    }
}
