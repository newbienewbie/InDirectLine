using System.Reflection;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class ShouldSerializeContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        property.ShouldSerialize = instance => {
            if(member is PropertyInfo pi){
                var pv =  pi.GetValue(instance);
                if(pv == null) return false;
            }
            return true;
        };
        return property;

    }
}