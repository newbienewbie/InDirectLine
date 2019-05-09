using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Models;
using Microsoft.Bot.Schema;

namespace Itminus.InDirectLine.Core.Services
{
    public class InMemoryConversationHistoryStore : IConversationHistoryStore
    {
        private ConcurrentDictionary<string,IList<Activity>> _history;
        public InMemoryConversationHistoryStore()
        {
            if(_history == null)
                _history = new ConcurrentDictionary<string,IList<Activity>>();
        }

        public async Task CreateConversationIfNotExistsAsync(string conversationId)
        {
            var exists = await ConversationExistsAsync(conversationId);
            if(!exists){
                _history[conversationId] = new List<Activity>();
            } else {

            }
        }

        public Task<bool> ConversationExistsAsync(string conversationId)
        {
            if(!_history.ContainsKey(conversationId))
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        public Task AddActivityAsync(string conversationId, Activity activity)
        {
            var exists = _history.TryGetValue(conversationId,out var conversation);
            if(!exists){
                _history[conversationId] = new List<Activity>();
            }
            _history[conversationId].Add(activity);
            return Task.CompletedTask;
        }


        public Task<ActivitySet> GetActivitySetAsync(string conversationId,int watermark)
        {
            var exists = _history.TryGetValue(conversationId,out var conversation);
            if(!exists){
                return null;
            }
            var _activities = conversation?.Skip(watermark).ToList();
            var count = _activities== null ? 0 : _activities.Count();
            var result = new ActivitySet{
                Activities = _activities ?? new List<Activity>(),
                Watermark =  watermark + count,
            };
            return Task.FromResult(result);
        }
    }

}
