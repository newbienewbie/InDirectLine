using System;
using Itminus.InDirectLine.Authorization;
using Itminus.InDirectLine.Middlewares;
using Itminus.InDirectLine.Services.IDirectLineConnections;
using Itminus.InDirectLine.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.InDirectLine.Services
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
                options.AddPolicy("CORS-InDirectLine",
                builder =>
                {
                    builder.WithOrigins(botEndPointUri);
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });
            });
            services.AddScoped<WebSocketConnectionMiddleware>();
            // services.AddHostedService<DirectLineConnectionHostedService>();
            return services.AddScoped<DirectLineHelper>();
        }
    }
}