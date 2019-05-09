using System.Collections.Generic;
using Itminus.InDirectLine.Core.Utils;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Core.Models
{

    public class ActivitySet
    {
        [JsonProperty("activities")]
        [JsonConverter(typeof(ActivityNullPropsProcessingConverter))]
        public IList<Activity> Activities {get;set;}
        [JsonProperty("watermark")]
        public int Watermark {get;set;}
    }


}