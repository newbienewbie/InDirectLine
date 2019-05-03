using Itminus.InDirectLine.InDirectLine.Services.IDirectLineConnections;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Itminus.InDirectLine.InDirectLine
{
    public class WebSocketConnectionMiddleware : IMiddleware
    {
        private readonly IDirectLineConnectionManager _connectionManager;

        public WebSocketConnectionMiddleware(IDirectLineConnectionManager connectionManager)
        {
            this._connectionManager = connectionManager;
        }


        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var reg = new Regex(@"$/v3/directline/conversations/(?<conversationId>.*)/stream");
            var m=reg.Match(context.Request.Path);
            var conversaionId = m.Groups["conversationId"].Value;
            if (m.Success)
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    // register connection 
                    var conn = new WebSocketDirectLineConnection(webSocket);
                    await this._connectionManager.RegisterConnectionAsync(conversaionId,conn);
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

    }
}
