using System.Collections.Generic;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Models
{
    public class ActivitySet
    {
        public IList<Activity> Activities {get;set;}
        public int Watermark {get;set;}
    }


}