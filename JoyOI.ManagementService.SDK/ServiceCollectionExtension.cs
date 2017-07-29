using JoyOI.ManagementService.SDK;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddJoyOIManagementService(this IServiceCollection self)
        {
            return self.AddSingleton<ManagementServiceClient>();
        }
    }
}
