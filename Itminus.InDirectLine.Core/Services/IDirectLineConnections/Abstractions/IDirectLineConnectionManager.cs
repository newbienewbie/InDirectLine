using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Core.Services.IDirectLineConnections
{

    public interface IDirectLineConnectionManager
    {
        Task<IDirectLineConnection> GetConnectionAsync(string conversationId);

        Task RegisterConnectionAsync(string conversationId, IDirectLineConnection connection);

        Task RemoveConnectionAsync(string conversationId);
        Task SendAsync(string conversationId,string txt);

    }


}
