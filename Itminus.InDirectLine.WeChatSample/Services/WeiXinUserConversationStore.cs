
using System.Collections.Generic;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Models;

namespace Itminus.InDirectLine.WeChatBotSample.Services
{

    public class InMemoryWeiXinUserConversationStore : IWeixinUserConversationStore
    {
        private Dictionary<string,ConversationInfo> dict = new Dictionary<string, ConversationInfo>();

        public InMemoryWeiXinUserConversationStore()
        {
        }

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

        public Task<bool> StoreAsync(string userId, DirectLineConversation conversation , string watermark = "")
        {
            var r= this.dict.TryAdd(userId,new ConversationInfo{
                DirectLineConversation = conversation,
                Watermark = watermark,
            });
            return Task.FromResult(r);
        }
    }

    public interface IWeixinUserConversationStore
    {
        Task<ConversationInfo> GetConversationAsync(string userId);

        Task<bool> StoreAsync(string userId, DirectLineConversation conversation , string watermark="");
    }

    public class ConversationInfo
    {
        public DirectLineConversation DirectLineConversation{get;set;}
        public string Watermark {get;set;} = "";
    }

}