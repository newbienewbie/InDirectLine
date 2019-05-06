using System.Collections.Generic;
using Itminus.InDirectLine.Utils;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Models
{
    public class ActivitySet
    {
        [JsonConverter(typeof(ActivityNullPropsProcessingConverter))]
        public IList<Activity> Activities {get;set;}
        public int Watermark {get;set;}
    }


}