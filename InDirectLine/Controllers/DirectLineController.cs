
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itminus.InDirectLine.Services.IDirectLineConnections;
using Itminus.InDirectLine.Models;
using Itminus.InDirectLine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

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
                StreamUrl= $"ws://localhost:3000/v3/directline/conversations/{conversationId}/stream?t=RCurR_XV9ZA.cwA..."
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
                StreamUrl= $"ws://localhost:3000/v3/directline/conversations/{conversationId}/stream?t=RCurR_XV9ZA.cwA..."
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

            var activitySet= new ActivitySet{
                Activities = new List<Activity>(){ activity, },
                Watermark = 0,
            };
            var message = JsonConvert.SerializeObject(activitySet);
            // notify the client 
            await this._connectionManager.SendAsync(conversationId,message);

            var statusCode=await this._helper.AddActivityToConversationAsync(conversationId,activity);

            return new OkObjectResult(new ResourceResponse{
                Id = activity.Id,
            });
        }

        /// <summary>
        /// process multiple files uploading 
        ///     seems that the single file uploading (body as a file stream) is not used by webchat
        /// </summary>
        /// <param name="conversationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost("v3/[controller]/conversations/{conversationId}/upload")]
        public async Task<IActionResult> ReceiveAttachmentsFromClient([FromRoute]string conversationId,[FromQuery]string userId,IList<IFormFile> file)
        {
            if(!Request.HasFormContentType){
                return BadRequest(new{
                    Message = "must be multipart/form-data"
                });
            }
            var serviceUrl = this._inDirectlineOption.ServiceUrl;
            IList<Attachment> attachments = file.Select(f => new Attachment(){
                ContentUrl = serviceUrl+"/attachments/"+f.Name,
                ContentType = Request.ContentType,
                Name = f.Name,
            }).ToList();
            var activity = this._helper.CreateAttachmentActivity(serviceUrl,conversationId, attachments);
            var statusCode = await this._helper.AddActivityToConversationAsync(conversationId, activity as Activity);

            return new OkObjectResult(new ResourceResponse{
                Id = activity.Id,
            });
        }

    }

}