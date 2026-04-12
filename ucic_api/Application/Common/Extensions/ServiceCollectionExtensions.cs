using Application.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;

namespace Application.Common.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExternalApiService(this IServiceCollection services)
        {
            services.AddHttpClient<IExternalApiService, ExternalApiService>();
            return services;
        }
        
    }
} 