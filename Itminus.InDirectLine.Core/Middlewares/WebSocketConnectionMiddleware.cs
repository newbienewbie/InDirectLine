using Itminus.InDirectLine.Core.Services;
using Itminus.InDirectLine.Core.Services.IDirectLineConnections;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Itminus.InDirectLine.Core.Middlewares
{
    public class WebSocketConnectionMiddleware : IMiddleware
    {
        private readonly IDirectLineConnectionManager _connectionManager;
        private readonly ILogger<WebSocketConnectionMiddleware> _logger;

        public WebSocketConnectionMiddleware(IDirectLineConnectionManager connectionManager,ILogger<WebSocketConnectionMiddleware> logger)
        {
            this._connectionManager = connectionManager;
            this._logger = logger;
        }


        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var reg = new Regex(@"^/v3/directline/conversations/(?<conversationId>.*)/stream");
            var path = context.Request.Path;
            var m=reg.Match(path);
            if (m.Success)
            {
                var conversaionId = m.Groups["conversationId"].Value;
                if (context.WebSockets.IsWebSocketRequest)
                {
                    if(context.User==null)
                    {
                        this._logger.LogInformation($"token={context.Request.Query["t"]} is not valid when trying to establish a websocket connection");
                        context.Response.StatusCode = 401;
                        return;
                    }
                    var token = context.Request.Query["t"].FirstOrDefault();
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    // register connection 
                    var conn = new WebSocketDirectLineConnection(webSocket);
                    await this._connectionManager.RegisterConnectionAsync(conversaionId,conn);
                    await ProcessWebSocketAsync(webSocket,conversaionId);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next(context);
            }
        }

        private async Task OnConnect(string conversationId)
        {
            var s = "{\"activities\":\"[]\",\"watermark\":\"initial-watermark\"}";
            await this._connectionManager.SendAsync(conversationId,s);
        }

        private async Task ProcessWebSocketAsync(WebSocket webSocket,string conversationId)
        {
            if(webSocket.State == WebSocketState.Open){
                await OnConnect(conversationId);

                var buffer = new byte[1024 * 4];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                // loop untill the close status is set
                while (!result.CloseStatus.HasValue)
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    // there's no need to hanle message
                }
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        }

    }
}
