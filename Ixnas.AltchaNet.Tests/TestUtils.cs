namespace Ixnas.AltchaNet.Tests;

internal static class TestUtils
{
    public static byte[] GetKey()
    {
        var list = new List<byte>(64);
        for (byte i = 0; i < 64; i++)
            list.Add(i);

        return list.ToArray();
    }
}
