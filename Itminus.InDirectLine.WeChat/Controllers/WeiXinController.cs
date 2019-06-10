
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Itminus.InDirectLine.Core.Models;
using Itminus.InDirectLine.Core.Services;
using Itminus.InDirectLine.WeChat.Models;
using Itminus.InDirectLine.WeChat.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Senparc.NeuChar;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar.Helpers;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities;

namespace Itminus.InDirectLine.WeChat
{


    [ApiController]
    [Route("api/[controller]")]
    public class WeiXinController : Controller
    {
        private readonly WeixinHelper _helper;
        private readonly ILogger<WeiXinController> _logger;
        private readonly IWeixinUserConversationStore _ucstore;
        private readonly InDirectLineClient _directLineClient;
        private readonly InDirectLineOptions _opts;

        public WeiXinController(WeixinHelper helper, ILogger<WeiXinController> logger, IWeixinUserConversationStore ucstore, InDirectLineClient directLineClient,IOptions<InDirectLineOptions> opts)
        {
            this._helper = helper;
            this._logger = logger;
            this._ucstore = ucstore;
            this._opts = opts.Value;

            this._directLineClient = directLineClient;
        }


        [HttpGet("")]
        public string EchoStr([FromQuery]string signature, [FromQuery]string nonce, [FromQuery]string timestamp, [FromQuery]string echostr)
        {
            if (this._helper.IsMessageFromWeiXin(signature, nonce, timestamp))
            {
                return echostr;
            }
            else
            {
                return "Failed to authenticate the request";
            }
        }


        // WX Platform Will Post the Message to the Endpoint 
        [HttpPost("")]
        public async Task<string> Post([FromQuery]string signature, [FromQuery]string nonce, [FromQuery]string timestamp)
        {
           

            var msgFromWX = this._helper.IsMessageFromWeiXin(signature, nonce, timestamp);
            if (!msgFromWX) { return ""; }

            this._logger.LogInformation("Msg from WeiXin Server received");

            XDocument doc = XDocument.Load(Request.Body);
            this._logger.LogInformation("Msg from WeiXin Server received is: \r\n"+ doc.ToString());
            var requestMessage = RequestMessageFactory.GetRequestEntity(doc);

            var userId = requestMessage.FromUserName.Trim();
            var conversationInfo= await this.RetrieveConversationInfoAsync(userId);

            string respDoc = "";

            switch (requestMessage.MsgType)
            {
                case RequestMsgType.Text ://文字类型
                {
                    var strongRequestMessage = requestMessage as RequestMessageText;

                    var activity = new Activity{
                        From = new ChannelAccount(userId),
                        Text = strongRequestMessage.Content,
                        Type = ActivityTypes.Message,
                    };
                    var conversation = conversationInfo.DirectLineConversation;
                    await this._directLineClient.SendActivityAsync(conversation.ConversationId, activity,conversation.Token)
                        .ConfigureAwait(false);
                    var respActivities= await this._directLineClient.RetrieveActivitySetAsync(conversation.ConversationId, conversationInfo.Watermark, conversation.Token)
                        .ConfigureAwait(false);

                    conversationInfo.Watermark =respActivities.Watermark.ToString();

                    var reply= String.Join(
                        "\r\n",
                        respActivities.Activities
                            .Where(a => true)   // todo: where id == botId
                            .Select(a => a.Text)
                    );

                    var strongRespMessage=ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(strongRequestMessage); 
                    strongRespMessage.Content =reply;
                    respDoc= strongRespMessage.ConvertEntityToXmlString();
                    break;
                }
                case RequestMsgType.Location://地理位置
                    break;
                case RequestMsgType.Image://图片
                    break;
                case RequestMsgType.Voice://语音
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return respDoc;
        }


        private async Task<ConversationInfo> RetrieveConversationInfoAsync(string userId){

            var conversationInfo= await this._ucstore.GetConversationAsync(userId).ConfigureAwait(false);

            // brand new
            if(conversationInfo== null)
            {
                conversationInfo = await StartNewConversationAsync(userId).ConfigureAwait(false);
            }
            else if(!conversationInfo.Active)
            {
                conversationInfo = await StartNewConversationAsync(userId).ConfigureAwait(false);
            }
            else if(conversationInfo.ShouldRefresh)
            {
                var refreshed = await this._directLineClient.RefreshTokenAsync(conversationInfo.DirectLineConversation.Token)
                    .ConfigureAwait(false);
                await this._ucstore.StoreAsync(userId,refreshed,"");
                conversationInfo = await this._ucstore.GetConversationAsync(userId);
            }
            return conversationInfo;
        }

        private async Task<ConversationInfo> StartNewConversationAsync(string userId)
        {
            var initConversation= await this._directLineClient.GenerateTokenAsync()
                .ConfigureAwait(false);
            var directLineConversation = await this._directLineClient.StartConversationAsync(initConversation.Token)
                .ConfigureAwait(false);
            await this._ucstore.StoreAsync(userId, directLineConversation, "")
                .ConfigureAwait(false);
            return  await this._ucstore.GetConversationAsync(userId);
        }


    }

}