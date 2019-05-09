using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Itminus.InDirectLine.Core.Services.IDirectLineConnections
{
    public class DirectLineConnectionManager : IDirectLineConnectionManager
    {
        private ConcurrentDictionary<string, IDirectLineConnection> _sockets = new ConcurrentDictionary<string, IDirectLineConnection>();

        public async Task SendAsync(string conversationId,string txt)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(txt);
            var seg = new ArraySegment<byte>(bytes);
            var conn = await GetConnectionAsync(conversationId);
            if(conn!=null){
                await conn.SendAsync(seg);
            }
            else{
                // ....
            }
        }

        public Task RegisterConnectionAsync(string conversationId, IDirectLineConnection connection)
        {
            if(string.IsNullOrEmpty(conversationId)){ throw new ArgumentException($"{nameof(conversationId)} cannot be null or empty!") ;}
            if(connection == null) throw new ArgumentNullException(nameof(connection));
            _sockets.TryAdd(conversationId, connection);
            return Task.CompletedTask;
        }

        public Task<IDirectLineConnection> GetConnectionAsync(string conversationId)
        {
            var conn =  _sockets.FirstOrDefault(kvp => kvp.Key == conversationId).Value ;
            return Task.FromResult(conn);
        }

        public async Task RemoveConnectionAsync(string conversationId)
        {
            var flag=_sockets.TryRemove(conversationId, out var conn);
            await conn?.CloseAsync(WebSocketCloseStatus.NormalClosure,"closed by IConnectionMananger");
        }

    }

}
