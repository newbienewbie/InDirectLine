using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Itminus.InDirectLine.Core.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddInDirectLine(this AuthenticationBuilder builder, InDirectLineAuthenticationOptions options)
        {

            // a drity hack to configure options
            builder.Services.Configure<InDirectLineAuthenticationOptions>(opt =>{
                foreach(var pi in opt.GetType().GetProperties()) 
                {
                    var propValue = pi.GetValue(options);
                    pi.SetValue(opt,propValue);
                }
            });

            return builder.AddJwtBearer(InDirectLineDefaults.AuthenticationSchemeName, opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = options.Issuer,
                    ValidAudience = options.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key))
                };
                opt.Events = new JwtBearerEvents
                {
                    // dynamically get token by querystring if the request is a websocket request
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.HttpContext.WebSockets.IsWebSocketRequest && ctx.Request.Query.ContainsKey("t"))
                        {
                            ctx.Token = ctx.Request.Query["t"];
                        }
                        return Task.CompletedTask;
                    },
                };
            });
        }

    }


}