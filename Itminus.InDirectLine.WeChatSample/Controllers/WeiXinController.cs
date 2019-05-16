
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Itminus.InDirectLine.WeChatBotSample.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Logging;
using Senparc.NeuChar;
using Senparc.NeuChar.Entities;
using Senparc.NeuChar.Helpers;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.Entities;

namespace Webbot.Wechat.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class WeiXinController : Controller
    {
        private readonly WeixinHelper _helper;
        private readonly ILogger<WeiXinController> _logger;
        private readonly IWeixinUserConversationStore _ucstore;
        private readonly DirectLineClient _directLineClient;

        public WeiXinController(WeixinHelper helper, ILogger<WeiXinController> logger, IWeixinUserConversationStore ucstore, DirectLineClient directLineClient)
        {
            this._helper = helper;
            this._logger = logger;
            this._ucstore = ucstore;
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
            var requestMessage = RequestMessageFactory.GetRequestEntity(doc);

            var userId = requestMessage.FromUserName.Trim();
            var conversationInfo= await this._ucstore.GetConversationAsync(userId);

            if(conversationInfo== null)
            {
                var conversation = await this._directLineClient.Conversations.StartConversationAsync().ConfigureAwait(false);
                await this._ucstore.StoreAsync(userId, conversation).ConfigureAwait(false);
            }

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
                    var conversation = conversationInfo.Conversation;
                    await this._directLineClient.Conversations.PostActivityAsync(conversation.ConversationId, activity)
                        .ConfigureAwait(false);
                    var respActivities= await this._directLineClient.Conversations.GetActivitiesAsync(conversation.ConversationId, conversationInfo.Watermark)
                        .ConfigureAwait(false);

                    conversationInfo.Watermark = respActivities.Watermark;

                    var reply= String.Join(
                        '|',
                        respActivities.Activities.Where(a => true)   // todo: where id == botId
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





    }

}