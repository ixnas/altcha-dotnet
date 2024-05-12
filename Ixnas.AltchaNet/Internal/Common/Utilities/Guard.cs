using System;

namespace Ixnas.AltchaNet.Internal.Common.Utilities
{
    internal static class Guard
    {
        public static void NotNull(object argument)
        {
            if (argument == null)
                throw new ArgumentNullException();
        }

        public static void NotNullOrWhitespace(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentNullException();
        }
    }
}
