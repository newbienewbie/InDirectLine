using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Core.Utils
{
    public static class HttpClientExtensions
    {

        public static async Task<HttpResponseMessage> SendJsonAsync<TPayload>(this HttpClient client, string url , TPayload payload)
        {
            var req = new HttpRequestMessage(HttpMethod.Post,url);
            var str= JsonConvert.SerializeObject(payload);
            req.Content = new StringContent( str , Encoding.UTF8, "application/json");

            var resp = await client.SendAsync(req);
            return resp;
        }


    }
}