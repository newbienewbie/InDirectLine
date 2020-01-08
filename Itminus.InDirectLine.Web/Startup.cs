using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Itminus.InDirectLine.Core;
using Itminus.InDirectLine.Core.Authentication;
using Itminus.InDirectLine.Core.Services;
using Microsoft.Extensions.Hosting;
using Itminus.InDirectLine.Core.Controllers;

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

            services.AddInDirectLine(Configuration.GetSection("DirectLine").Get<InDirectLineSettings>());
            services.AddAuthentication()
                .AddInDirectLine(Configuration.GetSection("Jwt").Get<InDirectLineAuthenticationOptions>());
            services.AddAuthorization();
            services.AddControllers().AddNewtonsoftJson().AddApplicationPart(typeof(DirectLineController).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
            //app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseStaticFiles();
            app.UseInDirectLineCors();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseInDirectLineCore();
            app.UseInDirectLineUploadsStatic();

            app.UseEndpoints(endpoints=>
            {
                endpoints.MapControllers();
            });
        }
    }
}
