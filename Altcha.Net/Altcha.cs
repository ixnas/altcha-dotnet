using Altcha.Net.Internal;

namespace Altcha.Net
{
    public static class Altcha
    {
        public static IAltchaServiceBuilder CreateServiceBuilder()
        {
            return new ServiceBuilder();
        }
    }
}
