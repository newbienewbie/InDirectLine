using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Services.IDirectLineConnections;
using Itminus.InDirectLine.Core.Models;
using Itminus.InDirectLine.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Core.Controllers
{

    [ApiController]
    public class ConversationsController : Controller
    {
        private readonly ILogger<ConversationsController> _logger;
        private readonly IOptions<InDirectLineSettings> _settings;
        private readonly DirectLineHelper _helper;
        private readonly IDirectLineConnectionManager _connectionManager;

        public ConversationsController(ILogger<ConversationsController> logger, IOptions<InDirectLineSettings> opt, DirectLineHelper helper, IDirectLineConnectionManager connectionManager)
        {
            this._logger = logger;
            this._settings = opt;
            this._helper = helper;
            this._connectionManager = connectionManager;
        }

        [HttpGet("v3/[controller]")]
        public IActionResult Index()
        {
            // todo
            return Ok();
        }

        [HttpPost("v3/[controller]/{conversationId}/activities")]
        public async Task<IActionResult> ReceiveActivityFromBotAsync([FromRoute] string conversationId,[FromBody]Activity activity)
        {
            activity.Id = Guid.NewGuid().ToString();
            activity.From = new ChannelAccount{
                Id = "id",
                Name = "Bot",
            };
            // always uses the server receiving time 
            //     it prevents messages displaying in incorrect order
            activity.Timestamp = DateTime.Now;
            var conversationExists = await this._helper.ConversationHistoryExistsAsync(conversationId);
            if(!conversationExists){
                return BadRequest(new{
                    Message = $"Conversation with id={conversationId} doesn't exist!"
                });
            }
            await this._helper.AddActivityToConversationHistoryAsync(conversationId,activity);
            this._logger.LogInformation("messages from bot received: \r\nConversationId={0}\r\nActivity.Id={1}\tActivityType={2}\tMessageText={3}",conversationId,activity.Id,activity.Type,activity.Text);
            return new OkResult();
        }

        [HttpPost("v3/[controller]/{conversationId}/activities/{replyTo}")]
        public async Task<IActionResult> ReceiveActiviyFromBotAsync([FromRoute] string conversationId,[FromRoute] string replyTo, Activity activity)
        {
            activity.Id = Guid.NewGuid().ToString();
            activity.From = new ChannelAccount{
                Id = "id",
                Name = "Bot",
            };
            // always uses the server receiving time 
            //     it prevents messages displaying in incorrect order
            activity.Timestamp = DateTime.Now;
            var conversationExists = await this._helper.ConversationHistoryExistsAsync(conversationId);
            if(!conversationExists){
                return BadRequest(new{
                    Message = $"Conversation with id={conversationId} doesn't exist!"
                });
            }
            await this._helper.AddActivityToConversationHistoryAsync(conversationId,activity);
            await this._connectionManager.SendActivitySetAsync(conversationId,activity);
            this._logger.LogInformation($"A message from bot received: \r\nConversationId={conversationId.ToString()}\r\n ReplyTo={replyTo}\r\nActivity.Id={activity.Id}\tActivityType={activity.Type}\tMessageText={activity.Text}");
            return new OkResult();
        }

        [HttpPost("v3/[controller]/{conversationId}/members")]
        public IActionResult ConversationMembers([FromRoute] string conversationId)
        {
            // not implemented
            return new OkResult();
        }

        [HttpPost("v3/[controller]/{conversationId}/activities/{activityId}/members")]
        public IActionResult ActivityMembers([FromRoute] string conversationId)
        {
            // not implemented
            return new OkResult();
        }


    }

}