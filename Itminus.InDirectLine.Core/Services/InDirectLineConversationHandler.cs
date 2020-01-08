using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Services.IDirectLineConnections;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Itminus.InDirectLine.Core.Services
{
    public class InDirectLineConversationHandler : ChannelServiceHandler
    {

        private readonly ILogger<InDirectLineConversationHandler> _logger;
        private readonly IDirectLineConnectionManager _connectionManager;
        private readonly DirectLineHelper _helper;

        public InDirectLineConversationHandler(ILogger<InDirectLineConversationHandler> logger, IDirectLineConnectionManager connectionManager ,DirectLineHelper helper , ICredentialProvider credentialProvider, AuthenticationConfiguration authConfiguration, IChannelProvider channelProvider = null) 

            : base(credentialProvider, authConfiguration, channelProvider)
        {
            this._logger = logger;
            this._connectionManager = connectionManager;
            this._helper = helper;
        }
        public virtual Task PrepareActivity(Activity activity)
        {
            activity.Id = Guid.NewGuid().ToString();
            activity.From = new ChannelAccount{
                Id = "id",
                Name = "Bot",
            };
            return Task.CompletedTask;
        }

        protected override async Task<ResourceResponse> OnReplyToActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {

            await this.PrepareActivity(activity);
            var conversationExists = await this._helper.ConversationHistoryExistsAsync(conversationId);
            if(!conversationExists){
                var resource = new IndirectLineConversationResourceResponse{
                    Id = activity.Id,
                    Status = 400,
                    Description = $"Conversation with id={conversationId} doesn't exist!" ,
                } ;
                return resource;
            }
            await this._helper.AddActivityToConversationHistoryAsync(conversationId,activity);
            await this._connectionManager.SendActivitySetAsync(conversationId,activity);
            this._logger.LogInformation("message from bot received: \r\nConversationId={0}\r\n ActivityId={1}\r\nActivity.Id={2}\tActivityType={3}\tMessageText={4}",conversationId, activityId,activity.Id,activity.Type,activity.Text);
            return new ResourceResponse{Id = activity.Id};
        }

        protected override async Task<ResourceResponse> OnSendToConversationAsync(ClaimsIdentity claimsIdentity, string conversationId, Activity activity, CancellationToken cancellationToken = default)
        {
            await this.PrepareActivity(activity);
            var conversationExists = await this._helper.ConversationHistoryExistsAsync(conversationId);
            if(!conversationExists){
                var resource = new IndirectLineConversationResourceResponse{
                    Id = activity.Id,
                    Status = 400,
                    Description = $"Conversation with id={conversationId} doesn't exist!" ,
                } ;
                return resource;
            }
            await this._helper.AddActivityToConversationHistoryAsync(conversationId,activity);
            this._logger.LogInformation("message from bot received: \r\nConversationId={0}\r\nActivity.Id={1}\tActivityType={2}\tMessageText={3}",conversationId,activity.Id,activity.Type,activity.Text);
            return new ResourceResponse{Id = activity.Id};
        }

        protected override async Task<ResourceResponse> OnUpdateActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, Activity activity, CancellationToken cancellationToken = default)
        {
            await this.PrepareActivity(activity);
            var conversationExists = await this._helper.ConversationHistoryExistsAsync(conversationId);
            if(!conversationExists){
                var resource = new IndirectLineConversationResourceResponse{
                    Id = activity.Id,
                    Status = 400,
                    Description = $"Conversation with id={conversationId} doesn't exist!" ,
                } ;
                return resource;
            }
            this._logger.LogWarning($"attempt to invoke {nameof(OnUpdateActivityAsync)} method, but this method is not allowed");
            return new IndirectLineConversationResourceResponse{
                Id = activity.Id,
                Status = 400,
                Description = "{Update Activity} is not allowed",
            };
        }

        protected override Task OnDeleteActivityAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            this._logger.LogWarning($"attempt to invoke {nameof(OnDeleteActivityAsync)} method, but this method is not implemented");
            return Task.CompletedTask;
        }

        protected override Task<ConversationResourceResponse> OnCreateConversationAsync(ClaimsIdentity claimsIdentity, ConversationParameters parameters, CancellationToken cancellationToken = default)
        {
            this._logger.LogWarning($"attempt to invoke {nameof(OnDeleteActivityAsync)} method, but this method is not implemented");
            var resp = new ConversationResourceResponse{};
            return Task.FromResult(resp);
        }

        protected override Task OnDeleteConversationMemberAsync(ClaimsIdentity claimsIdentity, string conversationId, string memberId, CancellationToken cancellationToken = default)
        {
            return base.OnDeleteConversationMemberAsync(claimsIdentity, conversationId, memberId, cancellationToken);
        }

        protected override Task<IList<ChannelAccount>> OnGetActivityMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, string activityId, CancellationToken cancellationToken = default)
        {
            this._logger.LogWarning($"attempt to invoke {nameof(OnGetActivityMembersAsync)} method, but this method is not implemented");
            IList<ChannelAccount> result = new List<ChannelAccount>();
            return Task.FromResult(result);
        }

        protected override Task<IList<ChannelAccount>> OnGetConversationMembersAsync(ClaimsIdentity claimsIdentity, string conversationId, CancellationToken cancellationToken = default)
        {
            // In the case of Teams, the conversationId parameter might actually be the TeamId and not the conversationId.
            // In Teams it is only the conversationId when it is a one on one conversation or a group chat.
            //return base.OnGetConversationMembersAsync(claimsIdentity, conversationId, cancellationToken);
            this._logger.LogWarning($"attempt to invoke {nameof(OnGetConversationMembersAsync)} method, but this method is not implemented");
            IList<ChannelAccount> result = new List<ChannelAccount>();
            return Task.FromResult(result);
        }

        protected override Task<ConversationsResult> OnGetConversationsAsync(ClaimsIdentity claimsIdentity, string conversationId, string continuationToken = null, CancellationToken cancellationToken = default)
        {
            //return base.OnGetConversationsAsync(claimsIdentity, conversationId, continuationToken, cancellationToken);
            this._logger.LogWarning($"attempt to invoke {nameof(OnGetConversationsAsync)} method, but this method is not implemented");
            var result = new ConversationsResult { };
            return Task.FromResult(result);
        }

        internal class IndirectLineConversationResourceResponse: ResourceResponse
        {
            public int Status {get;set;}
            public string Description{get;set;}
        }


    }

}