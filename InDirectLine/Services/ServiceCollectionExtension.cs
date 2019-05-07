using System;
using Itminus.InDirectLine.Authorization;
using Itminus.InDirectLine.Services.IDirectLineConnections;
using Microsoft.AspNetCore.Authorization;
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
            services.AddSingleton<IDirectLineConnection,WebSocketDirectLineConnection>();
            services.AddSingleton<IDirectLineConnectionManager,DirectLineConnectionManager>();
            services.AddSingleton<TokenBuilder>();
            services.AddAuthorization(opt =>{
                opt.AddPolicy("MatchConversation",pb => pb.Requirements.Add(new MatchConversationAuthzRequirement()));
            });
            services.AddSingleton<IAuthorizationHandler,MatchConversationAuthzHandler>();
            // services.AddHostedService<DirectLineConnectionHostedService>();
            return services.AddScoped<DirectLineHelper>();
        }
    }
}