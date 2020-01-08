using System;
using System.IO;
using Itminus.InDirectLine.Core.Middlewares;
using Itminus.InDirectLine.Core.Services;
using Itminus.InDirectLine.Core.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Itminus.InDirectLine.Core
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseInDirectLineCors(this IApplicationBuilder app)
        {
            app.UseCors(InDirectLineDefaults.CorsPolicyNames);
            return app;
        }

        public static IApplicationBuilder UseInDirectLineUploadsStatic(this IApplicationBuilder app)
        {
            var sp = app.ApplicationServices;
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var directLineOpt = sp.GetRequiredService<IOptions<InDirectLineSettings>>()?.Value;

            if(directLineOpt ==null)
            {
                throw new Exception("InDirectLineOptions cannot be null!");
            }

            var baseDirOfAttachments = Path.Combine(env.ContentRootPath, directLineOpt.Attachments.BaseDirectoryForUploading);
            var requestPath = directLineOpt.Attachments.BaseUrlForDownloading;
            requestPath = requestPath.StartsWith("/")? requestPath : "/"+requestPath;
            if(!Directory.Exists( baseDirOfAttachments))
                Directory.CreateDirectory(baseDirOfAttachments);

            var fileProvider = new PhysicalFileProvider(baseDirOfAttachments){ };  //todo : file filter
            var so = new StaticFileOptions(){
                RequestPath= requestPath,
                FileProvider = fileProvider,
            };
            app.UseStaticFiles(so);
            return app;
        }
        
        public static IApplicationBuilder UseInDirectLineCore(this IApplicationBuilder app){

            var sp = app.ApplicationServices;
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var directLineSettings = sp.GetRequiredService<IOptions<InDirectLineSettings>>()?.Value;
            if(directLineSettings ==null)
            {
                throw new Exception("InDirectLineOptions cannot be null!");
            }

            var webSocketOptions = new WebSocketOptions()
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
                ReceiveBufferSize = 4 * 1024
            };

            var botEndPoint = directLineSettings.BotEndpoint;
            var botOrigin = UtilsEx.GetOrigin(botEndPoint);
            webSocketOptions.AllowedOrigins.Add(botOrigin);
            webSocketOptions.AllowedOrigins.Add("*");
            app.UseWebSockets(webSocketOptions);
            app.UseMiddleware<WebSocketConnectionMiddleware>();
            return app;
        }
    }
}