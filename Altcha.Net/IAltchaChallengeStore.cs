using System.Threading.Tasks;

namespace Altcha.Net
{
    public interface IAltchaChallengeStore
    {
        Task Store(string challenge);
        Task<bool> Exists(string challenge);
    }
}
