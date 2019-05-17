// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v0.4.6

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Itminus.InDirectLine.WeChatBotSample.Bots;
using Itminus.InDirectLine.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Authentication;
using Itminus.InDirectLine.WeChatBotSample.Services;
using Itminus.InDirectLine.Core.Services;
using Microsoft.Extensions.Options;

namespace Itminus.InDirectLine.WeChatBotSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the Bot Framework Adapter.
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, EchoBot>();

            var directlineConfig=Configuration.GetSection("DirectLine");
            var jwt = Configuration.GetSection("Jwt").Get<InDirectLineAuthenticationOptions>();
            services.AddInDirectLine(directlineConfig);
            services.AddAuthentication()
                .AddInDirectLine(jwt);
            
            services.AddHttpClient<InDirectLineClient>();
            services.AddSingleton<IWeixinUserConversationStore,InMemoryWeiXinUserConversationStore>();
            services.Configure<WeiXinOptions>(Configuration.GetSection("Weixin"));
            services.AddSingleton<WeixinHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseInDirectLineCors();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseInDirectLineCore();
            app.UseInDirectLineUploadsStatic();

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
