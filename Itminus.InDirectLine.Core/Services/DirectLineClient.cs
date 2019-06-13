
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Models;
using Itminus.InDirectLine.Core.Services;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.Core.Services
{
    public class InDirectLineClient
    {
        private readonly HttpClient _httpClient;

        public InDirectLineClient(HttpClient httpClient, IOptions<InDirectLineSettings> settings)
        {
            this._httpClient = httpClient;
            this._httpClient.BaseAddress = new Uri(settings.Value.ServiceUrl);
        }

        public async Task<DirectLineConversation> GenerateTokenAsync(TokenCreationPayload payload)
        {
            if(payload ==null)
            {
                throw new ArgumentNullException(nameof(payload));
            }
            var endpoint = "/v3/directline/tokens/generate";
            var req = new HttpRequestMessage(HttpMethod.Post,endpoint);
            req.Content = new StringContent(
                JsonConvert.SerializeObject(payload), 
                Encoding.UTF8, 
                "application/json"
            );
            var resp = await this._httpClient.SendAsync(req);
            var json= await resp.Content.ReadAsStringAsync();
            var x = JsonConvert.DeserializeObject<DirectLineConversation>(json);
            return x;
        }

        public async Task<DirectLineConversation> RefreshTokenAsync(string token)
        {
            var endpoint =  "/v3/directline/tokens/refresh";
            var req = new HttpRequestMessage(HttpMethod.Post,endpoint);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent("");
            var resp = await this._httpClient.SendAsync(req);
            var json= await resp.Content.ReadAsStringAsync();
            var x = JsonConvert.DeserializeObject<DirectLineConversation>(json);
            return x;
        }


        public async Task<DirectLineConversation> StartConversationAsync(string token)
        {
            var endpoint = "/v3/directline/conversations";
            var req = new HttpRequestMessage(HttpMethod.Post,endpoint);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
            req.Content = new StringContent("");
            var resp = await this._httpClient.SendAsync(req);
            var jsonStr= await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DirectLineConversation>(jsonStr);

            return result;
        }

        public async Task<Core.Models.ActivitySet> RetrieveActivitySetAsync(string conversationId, string watermark ,string token)
        {
            var endpoint = $"/v3/directline/conversations/{conversationId}/activities?watermark={watermark}";
            var req = new HttpRequestMessage(HttpMethod.Get,endpoint);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
            var resp = await this._httpClient.SendAsync(req);
            var jsonStr = await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Core.Models.ActivitySet>(jsonStr);
            return result;
        }


        public async Task<ResourceResponse> SendActivityAsync(string conversationId, Activity activity , string token)
        {
            var endpoint = $"/v3/directline/conversations/{conversationId}/activities";
            var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
            req.Content = new StringContent(JsonConvert.SerializeObject(activity),Encoding.UTF8, "application/json") ;
            var resp= await this._httpClient.SendAsync(req);
            var jsonStr = await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResourceResponse>(jsonStr);
            return result;
        }




    }
}