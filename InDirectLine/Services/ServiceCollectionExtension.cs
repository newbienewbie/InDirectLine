using System;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.InDirectLine.Services
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInDirectLine(this IServiceCollection services,Action<InDirectLineOptions> configurer)
        {
            services.AddHttpClient();
            services.Configure<InDirectLineOptions>(configurer);
            services.AddSingleton<IConversationHistoryStore, InMemoryConversationHistoryStore>();
            return services.AddScoped<DirectLineHelper>();
        }
    }
}