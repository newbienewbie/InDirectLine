
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
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

    public interface IWeixinUserConversationStore
    {
        Task<ConversationInfo> GetConversationAsync(string userId);

        Task<bool> StoreAsync(string userId, DirectLineConversation conversation , string watermark="");
    }

    public class ConversationInfo
    {
        public DirectLineConversation DirectLineConversation{get;set;}
        public string Watermark {get;set;} = "";
        public DateTime TokenExpiredAt {get;set;}


        /// <summary>
        /// if TokenExpiredAt - _Level1Span <= current time, 
        ///     that means the conversationInfo is invalid
        /// </summary>
        private static int _Level1Span=10;
        /// <summary>
        /// if TokenExpiredAt - _Level2Span <= current time, 
        ///      that means we need refresh
        /// </summary>
        private static int _Level2Span=300;


        /// <summary>
        /// indicates current conversation is active
        /// </summary>
        /// <value></value>
        public bool Active 
        {
            get{
                return DateTime.Now.AddSeconds(_Level1Span) <= TokenExpiredAt;
            }
        }

        /// <summary>
        /// indicates current conversation need be refreshed
        /// </summary>
        /// <value></value>
        public bool ShouldRefresh
        {   
            get{
                return DateTime.Now.AddSeconds(_Level2Span) >= TokenExpiredAt;
            }
        }
    }

}