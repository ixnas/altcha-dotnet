namespace Ixnas.AltchaNet.AspNetCoreExample.Data;

internal class VerifiedChallenge
{
    public int Id { get; set; }
    public string Challenge { get; set; } = string.Empty;
    public DateTime ExpiryUtc { get; set; }
}
