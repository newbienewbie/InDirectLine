using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Itminus.InDirectLine.InDirectLine.Services.IDirectLineConnections
{
    public interface IDirectLineConnection
    {
        Task<(bool,ArraySegment<byte>)> ReceiveAsync();
        Task SendAsync(ArraySegment<byte> buffer);
        Task CloseAsync(string reason);
        bool Avaiable { get; }
    }

    public class WebSocketDirectLineConnection : IDirectLineConnection
    {
        public WebSocketDirectLineConnection(WebSocket webSocket)
        {
            WebSocket = webSocket;
        }

        public WebSocket WebSocket { get; }

        public bool Avaiable
        {
            get {
                return this.WebSocket.State != WebSocketState.Open
                    && this.WebSocket.State != WebSocketState.Connecting
                    ;
            }
        }

        public async Task CloseAsync(string reason)
        {
            await this.WebSocket.CloseAsync(
                WebSocketCloseStatus.Empty,
                reason,
                CancellationToken.None
            );
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
                false,
                CancellationToken.None
            );
        }

        public Task SendAsync()
        {
            throw new NotImplementedException();
        }
    }
}
