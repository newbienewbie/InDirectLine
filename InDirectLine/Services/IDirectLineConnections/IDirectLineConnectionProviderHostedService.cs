using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itminus.InDirectLine.InDirectLine.Services.IDirectLineConnections
{
    public interface IDirectLineConnectionsProvider
    {

        Task<IDirectLineConnection> GetConnectionAsync(string conversationId);

        Task<bool> RegisterConnectionAsync(string conversationId, IDirectLineConnection connection);

    }

}
