
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Itminus.InDirectLine.Core.Models;
using Itminus.InDirectLine.Core.Services;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Itminus.InDirectLine.WeChatBotSample.Services
{
    public class InDirectLineClient
    {
        private readonly HttpClient _httpClient;

        public InDirectLineClient(HttpClient httpClient, IOptions<InDirectLineOptions> opts)
        {
            this._httpClient = httpClient;
            this._httpClient.BaseAddress = new Uri(opts.Value.ServiceUrl);
        }

        public async Task<string> GenerateTokenAsync()
        {
            var endpoint = "/v3/directline/tokens/generate";
            var req = new HttpRequestMessage(HttpMethod.Post,new Uri( endpoint);
            req.Content = new StringContent("");
            var resp = await this._httpClient.SendAsync(req);
            var json= await resp.Content.ReadAsStringAsync();
            var x = JsonConvert.DeserializeObject<DirectLineConversation>(json);
            return x.Token;
        }

        public async Task<string> RefreshTokenAsync(string token)
        {
            var endpoint =  "/v3/directline/tokens/refresh";
            var req = new HttpRequestMessage(HttpMethod.Post,new Uri(endpoint));
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent("");
            var resp = await this._httpClient.SendAsync(req);
            var json= await resp.Content.ReadAsStringAsync();
            var x = JsonConvert.DeserializeObject<DirectLineConversation>(json);
            return x.Token;
        }


        public async Task<DirectLineConversation> StartConversationAsync(string token)
        {
            var endpoint = "/v3/directline/conversations";
            var req = new HttpRequestMessage(HttpMethod.Post, new Uri( endpoint));
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
            req.Content = new StringContent("");
            var resp = await this._httpClient.SendAsync(req);
            var jsonStr= await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DirectLineConversation>(jsonStr);

            return result;
        }

        public async Task<Core.Models.ActivitySet> RetrieveActivitySetAsync(string conversationId, string token)
        {
            var endpoint = $"/v3/directline/conversations/{conversationId}/activities";
            var req = new HttpRequestMessage(HttpMethod.Get,new Uri(endpoint));
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
            var resp = await this._httpClient.SendAsync(req);
            var jsonStr = await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<Core.Models.ActivitySet>(jsonStr);
            return result;
        }


        public async Task<ResourceResponse> SendActivity(string conversationId, Activity activity , string token)
        {
            var endpoint = $"/v3/directline/conversations/{conversationId}/activities";
            var req = new HttpRequestMessage(HttpMethod.Post, new Uri(endpoint));
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",token);
            req.Content = new StringContent(JsonConvert.SerializeObject(activity),Encoding.UTF8, "application/json") ;
            var resp= await this._httpClient.SendAsync(req);
            var jsonStr = await resp.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResourceResponse>(jsonStr);
            return result;
        }




    }
}