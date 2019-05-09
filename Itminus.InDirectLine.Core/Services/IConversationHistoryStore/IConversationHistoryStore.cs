
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Core.Services{


    public interface IConversationHistoryStore
    {

        Task<bool> ConversationExistsAsync(string conversationId);
        Task CreateConversationIfNotExistsAsync(string conversationId);
        Task AddActivityAsync(string conversationId, Activity activity);

        Task<ActivitySet> GetActivitySetAsync(string conversationId, int watermark);
    }


}