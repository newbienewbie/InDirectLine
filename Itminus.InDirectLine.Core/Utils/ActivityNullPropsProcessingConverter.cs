using System;
using System.Reflection;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;



namespace Itminus.InDirectLine.Core.Utils
{

    public class ActivityNullPropsProcessingConverter : JsonConverter 
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return serializer.Deserialize(reader,objectType);
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.ContractResolver = new DefaultContractResolver 
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            serializer.Serialize(writer,value);
        }

    }

}