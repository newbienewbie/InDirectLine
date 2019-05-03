
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itminus.InDirectLine.InDirectLine.Services.IDirectLineConnections;
using Itminus.InDirectLine.Models;
using Itminus.InDirectLine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Controllers{

    [ApiController]
    public class DirectLineController : Controller
    {
        private ILogger<DirectLineController> _logger;
        private readonly DirectLineHelper _helper;
        private readonly IDirectLineConnectionManager _connectionManager;
        private InDirectLineOptions _inDirectlineOption;

        public DirectLineController(ILogger<DirectLineController> logger, IOptions<InDirectLineOptions> opt, DirectLineHelper helper, IDirectLineConnectionManager connectionManager)
        {
            this._logger= logger;
            this._helper = helper;
            this._connectionManager = connectionManager;
            this._inDirectlineOption = opt.Value;
        }

        [HttpGet("v3/[controller]")]
        public IActionResult Index()
        {
            return new OkResult();
        }

        /// <summary>
        /// Create a new conversation
        /// </summary>
        /// <returns></returns>
        [HttpPost("v3/[controller]/conversations")]
        public async Task<IActionResult> Conversations()
        {
            var result= await this._helper.CreateNewConversation();
            var conversationId = result.Activity.Conversation.Id;
            return new OkObjectResult(new DirectLineConversation{
                ConversationId = conversationId,
                ExpiresIn= this._inDirectlineOption.ExpiresIn,
                Token = Request.Headers["Authentication"],
                StreamUrl= $"http://localhost:3000/v3/[controller]/conversations/{conversationId}/stream?t=RCurR_XV9ZA.cwA..."
            });
        }


        //
        [HttpGet("v3/[controller]/conversations/{conversationId}")]
        public IActionResult ConversationInfo(string conversationId)
        {
            return new OkObjectResult(new DirectLineConversation{
                ConversationId = conversationId,
                ExpiresIn= this._inDirectlineOption.ExpiresIn,
                Token = Request.Headers["Authentication"],
                StreamUrl= $"http://localhost:3000/v3/directline/conversations/{conversationId}/stream?t=RCurR_XV9ZA.cwA..."
            });
        }

        //
        [HttpGet("v3/[controller]/conversations/{conversationId}/activities")]
        public async Task<IActionResult> ShowActivitiesToClient([FromRoute]string conversationId, [FromQuery]string watermark)
        {

            int normailizedWatermark=0; 
            try{
                normailizedWatermark= int.Parse(watermark);
            }catch(Exception e){
                normailizedWatermark = 0;
            }
            //get converation by id
            var activitySet = await _helper.GetActivitySetFromConversationHistoryAsync(conversationId,normailizedWatermark);
            var message = JsonConvert.SerializeObject(activitySet);
            await this._connectionManager.SendAsync(conversationId,message);
            if(activitySet == null){
                return BadRequest(new {
                    Message = "Conversation doesn't exist",
                });
            }

            return new OkObjectResult(activitySet);
        }

        [HttpPost("v3/[controller]/conversations/{conversationId}/activities")]
        public async Task<IActionResult> SendActivityToBot([FromRoute]string conversationId, [FromBody] Activity activity)
        {
            var conversationExists = await this._helper.ConversationHistoryExistsAsync(conversationId);
            if(!conversationExists){
                return BadRequest(new {
                    Message = $"Conversation with id={conversationId} doesn't exist",
                });
            }

            // create a Id for activity
            activity.Id = Guid.NewGuid().ToString();
            activity.ChannelId = "emulator";
            activity.ServiceUrl = this._inDirectlineOption.ServiceUrl;
            activity.Conversation = new ConversationAccount{
                Id = conversationId,
            };

            var statusCode=await this._helper.AddActivityToConversationAsync(conversationId,activity);

            return new OkObjectResult(new ResourceResponse{
                Id = activity.Id,
            });
        }



    }

}