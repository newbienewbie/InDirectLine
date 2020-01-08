using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Itminus.InDirectLine.Core.Services.IDirectLineConnections
{
    public class WebSocketDirectLineConnection : IDirectLineConnection
    {
        public WebSocketDirectLineConnection(WebSocket webSocket, TaskCompletionSource<object> tcs)
        {
            WebSocket = webSocket;
            Tcs = tcs;
        }

        public WebSocket WebSocket { get; }
        public TaskCompletionSource<object> Tcs { get; }

        public bool Avaiable
        {
            get {
                return this.WebSocket.State == WebSocketState.Open
                    || this.WebSocket.State == WebSocketState.Connecting
                    ;
            }
        }

        public async Task CloseAsync(object status,string reason)
        {
            if(status is WebSocketCloseStatus s){
                await this.WebSocket.CloseAsync(
                    s,
                    reason,
                    CancellationToken.None
                );
            }else{
                await this.WebSocket.CloseAsync(
                    WebSocketCloseStatus.Empty,
                    reason,
                    CancellationToken.None
                );
            }
            this.Tcs.SetResult(status);
        }

        public async Task<(bool,ArraySegment<byte>)> ReceiveAsync()
        {
            var buffer = new byte[1024*4];
            var seg= new ArraySegment<byte>(buffer);
            var result = await this.WebSocket.ReceiveAsync(seg, CancellationToken.None);
            if (!result.CloseStatus.HasValue) {
                return (true,seg);
            }
            else {
                await this.WebSocket.CloseAsync(
                    result.CloseStatus.Value,
                    result.CloseStatusDescription,
                    CancellationToken.None
                );
                return (false,seg);
            }
        }

        public async Task SendAsync(ArraySegment<byte> buffer)
        {
            await this.WebSocket.SendAsync(
                buffer,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

    }
}
