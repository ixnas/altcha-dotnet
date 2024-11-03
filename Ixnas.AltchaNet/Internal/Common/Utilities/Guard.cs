using System;

namespace Ixnas.AltchaNet.Internal.Common.Utilities
{
    internal static class Guard
    {
        public static void NotNull(object argument)
        {
            NotNull<ArgumentNullException>(argument);
        }

        public static void NotNullOrWhitespace(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
                throw new ArgumentNullException();
        }

        public static void NotNull<TException>(object argument)
            where TException : Exception, new()
        {
            if (argument == null)
                throw new TException();
        }
    }
}
