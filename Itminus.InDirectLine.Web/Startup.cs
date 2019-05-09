using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itminus.InDirectLine.Middlewares;
using Itminus.InDirectLine.Services;
using Itminus.InDirectLine.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

namespace Itminus.InDirectLine
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var directlineConfig=Configuration.GetSection("DirectLine");
            services.AddInDirectLine(directlineConfig);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthentication(opt =>{
                    opt.DefaultAuthenticateScheme= JwtBearerDefaults.AuthenticationScheme;
                    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opt =>{
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                    opt.Events=new JwtBearerEvents{
                        OnMessageReceived = ctx =>{
                            if (ctx.HttpContext.WebSockets.IsWebSocketRequest && ctx.Request.Query.ContainsKey("t"))
                            {
                                ctx.Token = ctx.Request.Query["t"]; 
                            }
                            return Task.CompletedTask;
                        },
                    };
                });
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("CORS-InDirectLine");
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            var botEndPoint = Configuration["DirectLine:BotEndPoint"];
            var botOrigin = UtilsEx.GetOrigin(botEndPoint);
            webSocketOptions.AllowedOrigins.Add(botOrigin);
            webSocketOptions.AllowedOrigins.Add("*");
            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<WebSocketConnectionMiddleware>();



            var baseDirOfAttachments = Path.Combine(env.ContentRootPath, Configuration["DirectLine:Attachments:BaseDirectoryForUploading"]);
            var requestPath = Configuration["DirectLine:Attachments:BaseUrlForDownloading"];
            requestPath = requestPath.StartsWith("/")? requestPath : "/"+requestPath;
            if(!Directory.Exists( baseDirOfAttachments))
                Directory.CreateDirectory(baseDirOfAttachments);

            var fileProvider = new PhysicalFileProvider(baseDirOfAttachments){ };  //todo : file filter
            var so = new StaticFileOptions(){
                RequestPath= requestPath,
                FileProvider = fileProvider,
            };
            app.UseStaticFiles(so);


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "area",
                    template: "{area}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
