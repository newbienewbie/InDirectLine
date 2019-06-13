
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

namespace Itminus.InDirectLine.Core.Controllers{

    [ApiController]
    public class TokensController : Controller
    {
        private ILogger<TokensController> _logger;
        private readonly DirectLineHelper _helper;
        private readonly IDirectLineConnectionManager _connectionManager;
        private readonly TokenBuilder _tokenBuilder;
        private InDirectLineSettings _inDirectlineSettings;

        public TokensController(ILogger<TokensController> logger, IOptions<InDirectLineSettings> opt, DirectLineHelper helper, IDirectLineConnectionManager connectionManager,TokenBuilder tokenBuilder)
        {
            this._logger= logger;
            this._helper = helper;
            this._connectionManager = connectionManager;
            this._tokenBuilder = tokenBuilder;
            this._inDirectlineSettings = opt.Value;
        }


        [HttpPost("v3/directline/[controller]/generate")]
        public IActionResult Generate([FromBody]TokenCreationPayload payload)
        {
            // according to https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-direct-line-3-0-authentication?view=azure-bot-service-4.0#generate-token-versus-start-conversation
            //     we don't start a conversation , we just issue a new token that is valid for specific conversation
            var userId = payload.UserId;
            var conversationId = Guid.NewGuid().ToString();
            var claims = new List<Claim>();
            claims.Add(new Claim(TokenBuilder.ClaimTypeConversationID,conversationId));
            var expiresIn = this._inDirectlineSettings.TokenExpiresIn;
            var token =  this._tokenBuilder.BuildToken( userId, claims, expiresIn);
            return new OkObjectResult(new DirectLineConversation{
                ConversationId = conversationId,
                Token = token,
                ExpiresIn = expiresIn,
            });
        }


        [HttpPost("v3/directline/[controller]/refresh")]
        [Authorize(AuthenticationSchemes=InDirectLineDefaults.AuthenticationSchemeName)]
        public IActionResult Refresh()
        {
            var conversationId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == TokenBuilder.ClaimTypeConversationID)?.Value;
            if(string.IsNullOrEmpty(conversationId)){
                return BadRequest("there's no valid conversationID");
            }
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            if(string.IsNullOrEmpty(userId))
            {
                return BadRequest("there's no valid userId");
            }
            var claims = new List<Claim>();
            claims.Add(new Claim(TokenBuilder.ClaimTypeConversationID, conversationId));
            var expiresIn = this._inDirectlineSettings.TokenExpiresIn;
            var token =  this._tokenBuilder.BuildToken(userId, claims, expiresIn);
            return new OkObjectResult(new DirectLineConversation{
                ConversationId = conversationId,
                Token = token,
                ExpiresIn = expiresIn,
            });
        }


    }

}