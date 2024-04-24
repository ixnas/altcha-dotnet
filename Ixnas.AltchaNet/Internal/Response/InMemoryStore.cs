using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ixnas.AltchaNet.Internal.Response
{
    internal class InMemoryStore : IAltchaChallengeStore
    {
        private readonly List<string> _stored = new List<string>();

        public Task Store(string challenge)
        {
            _stored.Add(challenge);
            return Task.CompletedTask;
        }

        public Task<bool> Exists(string challenge)
        {
            var exists = _stored.Contains(challenge);
            return Task.FromResult(exists);
        }
    }
}
