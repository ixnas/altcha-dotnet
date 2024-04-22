namespace Altcha.Net
{
    public interface IAltchaServiceBuilder
    {
        IAltchaService Build();
        IAltchaServiceBuilder AddStore(IAltchaChallengeStore store);
        IAltchaServiceBuilder AddKey(byte[] key);
        IAltchaServiceBuilder AddInMemoryStore();
        IAltchaServiceBuilder SetComplexity(int min, int max);
    }
}
