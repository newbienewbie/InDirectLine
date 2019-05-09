
using System.Collections.Generic;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Models;
using Itminus.InDirectLine.Core.Services.IDirectLineConnections;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Core.Services.IDirectLineConnections
{
    public static class IDirectLineConnectionManagerExtension
    {
        public static async Task SendActivitySetAsync(this IDirectLineConnectionManager mgr, string conversationId, Activity activity)
        {
            var activitySet= new ActivitySet{
                Activities = new List<Activity>(){ activity, },
                Watermark = 0,
            };
            var message = JsonConvert.SerializeObject(activitySet);
            // notify the client 
            await mgr.SendAsync(conversationId,message);
        }
    }

}