using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Itminus.InDirectLine.Core.Utils;
using System.Threading.Tasks;
using System.Net;
using Itminus.InDirectLine.Core.Models;

namespace Itminus.InDirectLine.Core.Services
{
    public class DirectLineHelper
    {
        private readonly InDirectLineSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly IConversationHistoryStore _history;




        /// <summary>
        /// Get the url of Bot Services Message EndPoint
        /// </summary>
        /// <returns></returns>
        public string GetBotMessageEndpointUrl()
        {
            return this._settings.BotEndpoint;
        }

        public DirectLineHelper(IOptions<InDirectLineSettings> settings , IHttpClientFactory clientFactory, IConversationHistoryStore history)
        {
            this._settings = settings?.Value?? throw new Exception($"the {nameof(settings)} must not be null!");
            this._httpClient = clientFactory.CreateClient();
            this._history = history;
        }


        internal async Task<CreateNewConversationResult> CreateNewConversationWithId(string userId, string conversationId)
        {
            if(string.IsNullOrEmpty(userId)){
                throw new ArgumentNullException(nameof(userId)); 
            }
            if(string.IsNullOrEmpty(conversationId)) {
                throw new ArgumentNullException(nameof(conversationId));
            }
            var membersAdded = new List<ChannelAccount>{
                new ChannelAccount{ Id = userId},
            };
            var MembersRemoved = new List<ChannelAccount>{};
            var activity= CreateNewConversationUpdateActivity(conversationId,membersAdded,MembersRemoved);
            // persist this conversation to history store
            await this._history.CreateConversationIfNotExistsAsync(activity.Conversation.Id);

            var botMessageEndpointUrl = GetBotMessageEndpointUrl();
            var resp = await _httpClient.SendJsonAsync(botMessageEndpointUrl,activity);
            return new CreateNewConversationResult{
                Activity = activity,
                StatusCode = resp.StatusCode,
            };
        }

        internal class CreateNewConversationResult{
            public IConversationUpdateActivity Activity {get;set;}
            public HttpStatusCode StatusCode{get;set;}
        }

        private IConversationUpdateActivity CreateNewConversationUpdateActivity(string conversationId, IList<ChannelAccount> membersAdded, IList<ChannelAccount> MembersRemoved)
        {
            conversationId = string.IsNullOrEmpty(conversationId)? Guid.NewGuid().ToString(): conversationId;
            var serviceUrl  = this._settings.ServiceUrl;

            var activity = new Activity{
                Type =  ActivityTypes.ConversationUpdate,
                ChannelId = InDirectLineDefaults.ChannelId,
                ServiceUrl = serviceUrl,
                Conversation = new ConversationAccount{ Id = conversationId, },
                Id= Guid.NewGuid().ToString(),
                MembersAdded= membersAdded,
                MembersRemoved= MembersRemoved,
                From = new ChannelAccount { 
                    Id = "offline-directline", 
                    Name = "Offline Directline Server"
                },
                Recipient = BotChannelAccount,
            };

            return activity.AsConversationUpdateActivity();
        }

        internal ChannelAccount BotChannelAccount {get;set;} = new ChannelAccount{
            Id = "InDirectLine.Bot",
            Name = "InDirectLine Bot",
        };

        public IMessageActivity CreateAttachmentActivity(string serviceUrl, string conversationId,string userId ,IList<Attachment> attachments )
        {
            var activity = new Activity{
                Type = ActivityTypes.Message,
                ChannelId = InDirectLineDefaults.ChannelId,
                ServiceUrl = serviceUrl,
                Conversation = new ConversationAccount{ Id = conversationId, },
                From = new ChannelAccount{
                    Id = userId,
                },
                Id= Guid.NewGuid().ToString(),
                Attachments = attachments,
            };
            return activity.AsMessageActivity();
        }

        public async Task<bool> ConversationHistoryExistsAsync(string conversationId)
        {
            var conversationExists = await this._history.ConversationExistsAsync(conversationId);
            return conversationExists;
        }

        public async Task<HttpStatusCode> AddActivityToConversationAsync(string conversationId, Activity activity)
        {
            await _history.AddActivityAsync(conversationId, activity);
            var botMessageEndpointUrl = GetBotMessageEndpointUrl();
            var resp = await this._httpClient.SendJsonAsync(botMessageEndpointUrl,activity);
            var content=await resp.Content.ReadAsStringAsync();
            return resp.StatusCode;
        }

        public async Task AddActivityToConversationHistoryAsync(string conversationId, Activity activity)
        {
            await _history.AddActivityAsync(conversationId, activity);
        }

        public async Task<ActivitySet> GetActivitySetFromConversationHistoryAsync(string conversationId, int watermark)
        {
            var res = await this._history.GetActivitySetAsync(conversationId,watermark);
            return res;
        }
    }
}