
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Itminus.InDirectLine.Core.Models;
using Itminus.InDirectLine.WeChat.Models;

namespace Itminus.InDirectLine.WeChat.Services
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

        public Task<bool> StoreAsync(string userId, DirectLineConversation conversation , string watermark = "" )
        {
            this.dict[userId] = new ConversationInfo{
                DirectLineConversation = conversation,
                Watermark = watermark,
                TokenExpiredAt = DateTime.Now.AddSeconds(conversation.ExpiresIn),
            };
            return Task.FromResult(true);
        }
    }




}