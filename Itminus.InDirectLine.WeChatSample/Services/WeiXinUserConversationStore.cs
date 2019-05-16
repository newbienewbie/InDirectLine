
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector.DirectLine;

namespace Itminus.InDirectLine.WeChatBotSample.Services
{

    public class InMemoryWeiXinUserConversationStore : IWeixinUserConversationStore
    {
        private Dictionary<string,ConversationInfo> dict;

        public Task<ConversationInfo> GetConversationAsync(string userId)
        {
            var x = dict.TryGetValue(userId,out var info);
            if(x) {
                return Task.FromResult(info);
            }
            else 
            {
                return Task.FromResult<ConversationInfo>(null);
            }
        }

        public Task<bool> StoreAsync(string userId, Conversation conversation , string watermark = "")
        {
            var r= this.dict.TryAdd(userId,new ConversationInfo{
                Conversation = conversation,
                Watermark = watermark,
            });
            return Task.FromResult(r);
        }
    }

    public interface IWeixinUserConversationStore
    {
        Task<ConversationInfo> GetConversationAsync(string userId);

        Task<bool> StoreAsync(string userId, Conversation conversation , string watermark="");
    }

    public class ConversationInfo
    {
        public Conversation Conversation {get;set;}
        public string Watermark {get;set;} = "";
    }

}