using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itminus.InDirectLine.InDirectLine.Services.IDirectLineConnections
{
    public delegate Task SendHandler(string conversationId,ArraySegment<byte> seg);
    public delegate Task RegisterConversationConnectionHandler(string conversationId, IDirectLineConnection connection);
    public interface IDirectLineConnectionManager
    {
        Task RegisterConnectionAsync(string conversationId, IDirectLineConnection connection);
        Task SendAsync(string conversationId,string txt);

        /// <summary>
        /// invoked only by internal
        /// </summary>
        event RegisterConversationConnectionHandler OnRegister;
        /// <summary>
        /// invoked only by internal
        /// </summary>
        event SendHandler OnSend;
    }

    public class DirectLineConnectionManager : IDirectLineConnectionManager
    {
        public async Task SendAsync(string conversationId,string txt)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(txt);
            var seg = new ArraySegment<byte>(bytes);
            await this.OnSend?.Invoke(conversationId,seg);
        }

        public async Task RegisterConnectionAsync(string conversationId, IDirectLineConnection connection)
        {
            await this.OnRegister?.Invoke(conversationId,connection);
        }

        public event SendHandler OnSend;
        public event RegisterConversationConnectionHandler OnRegister;
    }

}
