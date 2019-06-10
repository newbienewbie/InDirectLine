using System;
using Itminus.InDirectLine.Core.Models;

namespace Itminus.InDirectLine.WeChat.Models
{
    public class ConversationInfo
    {
        /// <summary>
        /// DirectLineConversation : conversationId, token, etc.
        /// </summary>
        /// <value></value>
        public DirectLineConversation DirectLineConversation{get;set;}


        public string Watermark {get;set;} = "";

        /// <summary>
        /// Token expired time
        /// </summary>
        /// <value></value>
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
