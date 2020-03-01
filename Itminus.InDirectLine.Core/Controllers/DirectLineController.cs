
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Services.IDirectLineConnections;
using Itminus.InDirectLine.Core.Models;
using Itminus.InDirectLine.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Itminus.InDirectLine.Core.Utils;

namespace Itminus.InDirectLine.Core.Controllers{

    [ApiController]
    public class DirectLineController : Controller
    {
        private ILogger<DirectLineController> _logger;
        private readonly DirectLineHelper _helper;
        private readonly IDirectLineConnectionManager _connectionManager;
        private readonly TokenBuilder _tokenBuilder;
        private readonly IWebHostEnvironment _env;
        private InDirectLineSettings _inDirectlineSettings;

        public DirectLineController(ILogger<DirectLineController> logger, IOptions<InDirectLineSettings> opt, DirectLineHelper helper, IDirectLineConnectionManager connectionManager,TokenBuilder tokenBuilder, IWebHostEnvironment env)
        {
            this._logger= logger;
            this._helper = helper;
            this._connectionManager = connectionManager;
            this._tokenBuilder = tokenBuilder;
            this._env = env;
            this._inDirectlineSettings = opt.Value;
        }

        [HttpGet("v3/[controller]")]
        public IActionResult Index()
        {
            return new OkResult();
        }

        /// <summary>
        /// Create a new conversation
        /// If there's no conversation associated with the current User's token, create a new conversation
        /// </summary>
        /// <returns></returns>
        [HttpPost("v3/[controller]/conversations")]
        [Authorize(AuthenticationSchemes=InDirectLineDefaults.AuthenticationSchemeName)]
        public async Task<IActionResult> Conversations()
        {
            var userId= HttpContext.User?.Claims.FirstOrDefault(c => c.Type== ClaimTypes.Name)?.Value;
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId cannot be null!");
            }
            var conversationId = HttpContext.User?.Claims.FirstOrDefault(c => c.Type== TokenBuilder.ClaimTypeConversationID)?.Value;
            var result= await this._helper.CreateNewConversationWithId(userId, conversationId);
            // make sure the conversationId is created if null or empty
            conversationId = result.Activity.Conversation.Id;

            var dlConversation = this.GetDirectLineConversation(userId, conversationId);
            return new OkObjectResult(dlConversation);
        }

        //
        [HttpGet("v3/[controller]/conversations/{conversationId}")]
        [Authorize(Policy="MatchConversation", AuthenticationSchemes=InDirectLineDefaults.AuthenticationSchemeName)]
        public IActionResult ConversationInfo(string conversationId)
        {
            var userId= HttpContext.User?.Claims.FirstOrDefault(c => c.Type== ClaimTypes.Name)?.Value;
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId cannot be null!");
            }
            var dl = this.GetDirectLineConversation(userId, conversationId);
            return new OkObjectResult(dl);
        }

        private DirectLineConversation GetDirectLineConversation(string userId, string conversationId)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(TokenBuilder.ClaimTypeConversationID, conversationId));
            var expiresIn = this._inDirectlineSettings.TokenExpiresIn ;
            var token = this._tokenBuilder.BuildToken(userId,claims,expiresIn);

            var mustBeConnectedIn = this._inDirectlineSettings.StreamUrlMustBeConnectedIn;
            var streamUrlToken = this._tokenBuilder.BuildToken(userId,claims,mustBeConnectedIn);

            // we should not use ServiceUrl here because 
            //     - Service URL is exposed to Bot Message Endpont only, 
            //     - it might be different from the one exposed to client
            // so we should get the StreamURL from current URL
            var origin = $"{this.HttpContext.Request.Scheme}://{HttpContext.Request.Host}"; // note the Host has already included the PORT
            origin = UtilsEx.GetWebSocketOrigin(origin);
            this._logger.LogInformation($"the origin for streamUrl is {origin}");
            return new DirectLineConversation{
                ConversationId = conversationId,
                ExpiresIn= expiresIn,
                Token = token,
                StreamUrl= $"{origin}/v3/directline/conversations/{conversationId}/stream?t={streamUrlToken}"
            };
        }

        //
        [HttpGet("v3/[controller]/conversations/{conversationId}/activities")]
        [Authorize(Policy="MatchConversation", AuthenticationSchemes=InDirectLineDefaults.AuthenticationSchemeName)]
        public async Task<IActionResult> ShowActivitiesToClient([FromRoute]string conversationId, [FromQuery]string watermark)
        {

            int normailizedWatermark=0; 
            try{
                normailizedWatermark= int.Parse(watermark);
            }catch(Exception ){
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
        [Authorize(Policy="MatchConversation", AuthenticationSchemes=InDirectLineDefaults.AuthenticationSchemeName)]
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
            activity.ChannelId = InDirectLineDefaults.ChannelId;
            activity.ServiceUrl = this._inDirectlineSettings.ServiceUrl;
            activity.Conversation = new ConversationAccount{
                Id = conversationId,
            };
            // always uses the server receiving time 
            activity.Timestamp = DateTime.Now;

            // notify the client 
            await this._connectionManager.SendActivitySetAsync(conversationId,activity);
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
        [Authorize(Policy="MatchConversation", AuthenticationSchemes=InDirectLineDefaults.AuthenticationSchemeName)]
        [HttpPost("v3/[controller]/conversations/{conversationId}/upload")]
        public async Task<IActionResult> ReceiveAttachmentsFromClient([FromRoute]string conversationId,[FromQuery]string userId,[FromForm]IList<IFormFile> file)
        {
            if(!Request.HasFormContentType){
                return BadRequest(new{
                    Message = "must be multipart/form-data"
                });
            }
            var serviceUrl = this._inDirectlineSettings.ServiceUrl;
            IList<Attachment> attachments = new List<Attachment>(); 
            foreach(var f in file)
            {
                var attachment= await this.HandleAttachment(f,conversationId);
                attachments.Add(attachment);
            }

            Activity activity=null;
            var activityStream = Request.Form.Files["activity"]?.OpenReadStream();
            if(activityStream==null){
                activity=this._helper.CreateAttachmentActivity(serviceUrl,conversationId,userId,attachments) as Activity ;
            }else{
                var json = await new StreamReader(activityStream).ReadToEndAsync();
                activity = JsonConvert.DeserializeObject<Activity>(json);
            }
            activity.Id = Guid.NewGuid().ToString();
            activity.ServiceUrl = serviceUrl;
            activity.Attachments = attachments;
            activity.Conversation = new ConversationAccount{Id = conversationId};

            await this._connectionManager.SendActivitySetAsync(conversationId,activity);
            var statusCode = await this._helper.AddActivityToConversationAsync(conversationId, activity);

            return new OkObjectResult(new ResourceResponse{
                Id = activity.Id,
            });
        }


        /// <summary>
        /// copy attachment to required directory & construct an instance of Attachment
        /// </summary>
        /// <param name="file"></param>
        /// <param name="subdirectory"></param>
        /// <returns></returns>
        private async Task<Attachment> HandleAttachment(IFormFile file,string subdirectory)
        {
            var pathSegs = new string[]{
                this._env.ContentRootPath , 
                this._inDirectlineSettings.Attachments.BaseDirectoryForUploading,
                subdirectory,
            };
            var destDir = Path.Combine(pathSegs);

            if(!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);

            var contentUrlBaseStr= new string[]{ 
                    this._inDirectlineSettings.ServiceUrl,
                    this._inDirectlineSettings.Attachments.BaseUrlForDownloading,
                    subdirectory,
                }
                .Select(s => s.Trim('/'))
                .Aggregate((state, current)=>{
                    return $"{state}/{current}";
                });
            var contentUrlBase=new Uri(contentUrlBaseStr);

            using(var f = System.IO.File.OpenWrite(Path.Combine(destDir,file.FileName)))
            {
                await file.CopyToAsync(f);
                return new Attachment(){
                    ContentUrl = new Uri($"{contentUrlBase}/{file.FileName}").AbsoluteUri,
                    ContentType = Request.ContentType,
                    Name = file.FileName,
                };
            }

        }

    }

}