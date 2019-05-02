using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Models
{

    public class DirectLineConversation
    {
        public string ConversationId{get;set;}

        [JsonProperty("expires_in")]
        public int ExpiresIn{get;set;}
        public string StreamUrl{get;set;}
        public string Token{get;set;}
    }

}