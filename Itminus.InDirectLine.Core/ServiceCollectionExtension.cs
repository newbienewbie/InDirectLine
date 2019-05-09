using System;
using Itminus.InDirectLine.Core.Authorization;
using Itminus.InDirectLine.Core.Middlewares;
using Itminus.InDirectLine.Core.Services;
using Itminus.InDirectLine.Core.Services.IDirectLineConnections;
using Itminus.InDirectLine.Core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.InDirectLine.Core
{
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection AddInDirectLine(this IServiceCollection services,IConfiguration directlineConfig)
        {
            services.AddHttpClient();
            services.Configure<InDirectLineOptions>(directlineConfig);
            services.AddSingleton<IConversationHistoryStore, InMemoryConversationHistoryStore>();
            services.AddSingleton<IDirectLineConnection,WebSocketDirectLineConnection>();
            services.AddSingleton<IDirectLineConnectionManager,DirectLineConnectionManager>();
            services.AddSingleton<TokenBuilder>();
            services.AddAuthorization(opt =>{
                opt.AddPolicy("MatchConversation",pb => pb.Requirements.Add(new MatchConversationAuthzRequirement()));
            });

            var botEndPointUri= UtilsEx.GetOrigin(directlineConfig["BotEndPoint"]);
            services.AddSingleton<IAuthorizationHandler,MatchConversationAuthzHandler>();
            services.AddCors(options =>
            {
                options.AddPolicy(
                    InDirectLineDefaults.CorsPolicyNames,
                    builder =>{
                        builder.WithOrigins(botEndPointUri);
                        builder.AllowAnyOrigin();
                        builder.AllowAnyHeader();
                        builder.AllowAnyMethod();
                    }
                );
            });
            services.AddScoped<WebSocketConnectionMiddleware>();
            return services.AddScoped<DirectLineHelper>();
        }
    }
}