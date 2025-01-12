using Microsoft.AspNetCore.Hosting;

namespace CineVault.API.Extensions
{
    public static class WebHostEnvironmentExtensions
    {
        public static bool IsLocal(this IWebHostEnvironment env)
        {
            return env.EnvironmentName.Equals("Local", StringComparison.OrdinalIgnoreCase);
        }
    }
}
