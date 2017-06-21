using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using vk2tg.Data.Models.VK;

namespace vk2tg.Services
{

    public class VkService
    {
        private readonly string _accessTokens = ConfigurationManager.AppSettings["AccessTokens"];
        private readonly List<string> _tokens;
        private readonly DataService _dataService = new DataService();
        private const string UrlFormat = "https://api.vk.com/method/{0}?access_token={1}&{2}";
        private static int LastRequestTime = 0;
        private const int Timeout = 333;

        public VkService()
        {
            _tokens = _accessTokens.Split(';').ToList();
        }

        public async Task<string> GetVideoInfo(long ownerId, long videoId, string accessKey)
        {
            const string method = "video.get";
            const Method httpMethod = Method.GET;
            var parameters = $"owner_id={ownerId}&videos={ownerId}_{videoId}_{accessKey}";

            foreach(var token in EnumerateTokens())
            {
                var video = await GetListResult<VideoGetResponse>(method, token, parameters, httpMethod);
                return video == null || !video.Any()
                    ? null
                    : video.First().player;
            }
            return null;
        }

        public async Task<GroupsResponse> GetGroupInfo(string groupName)
        {
            const string method = "groups.getById";
            const Method httpMethod = Method.GET;
            var parameters = $"group_id={groupName}";

            foreach(var token in EnumerateTokens())
            {
                var content = await ExecuteRequest(method, token, parameters, httpMethod);
                var result = JsonConvert.DeserializeObject<GroupsGetResponse>(content);
                if(result?.response != null && result.response.Count == 1)
                {
                    return result.response.First();
                }

                await _dataService.AddErrorLog(content);
                //                var error = JsonConvert.DeserializeObject<ErrorResponse>(content);
                Trace.WriteLine(content);
                _tokens.Remove(token);
            }

            return null;
        }

        public async Task<List<WallPost>> GetPosts(long subscriptionId, long lastPostId, int count = 5)
        {
            const string method = "wall.get";
            const Method httpMethod = Method.GET;
            var parameters = $"owner_id={subscriptionId}&count={count + 1}&filter=owner";

            foreach(var token in EnumerateTokens())
            {
                var newPosts = await GetListResult<WallPost>(method, token, parameters, httpMethod);
                return newPosts != null && newPosts.Any()
                    ? newPosts.Where(p => p.id > lastPostId && p.marked_as_ads != 1).OrderByDescending(p => p.id).Take(count).OrderBy(p => p.id).ToList()
                    : new List<WallPost>();
            }

            return new List<WallPost>();
        }

        private async Task<List<T>> GetListResult<T>(string method, string token, string parameters, Method httpMethod)
        {
            var content = await ExecuteRequest(method, token, parameters, httpMethod);
            var result = JsonConvert.DeserializeObject<VkResult>(content);
            if(result?.Response != null && result.Response.Count > 1)
            {
                result.Response.RemoveAt(0);
                var t = result.Response.Select(r => JsonConvert.DeserializeObject<T>(r.ToString())).ToList();
                return t;
            }

            await _dataService.AddTraceLog($"GetListResult. Method:{method} token:{token} parameters:{parameters}");
            await _dataService.AddTraceLog($"GetListResult. result?.Response?.Count:{result?.Response?.Count}");
            Trace.WriteLine(content);

            var error = JsonConvert.DeserializeObject<ErrorResponse>(content);
            if(error.error.error_code != 15) //Access denied
            {
                _tokens.Remove(token);
            }

            return null;
        }

        private IEnumerable<string> EnumerateTokens()
        {
            var index = 0;
            while(_tokens.Count != 0)
            {
                yield return _tokens[index++ % _tokens.Count];
            }
        }

        private async Task<string> ExecuteRequest(string method, string token, string parameters, Method httpMethod)
        {
            var difference = Timeout + LastRequestTime - DateTime.Now.Millisecond;
            if(difference > 0)
            {
                await Task.Delay(difference);
            }

            var client = new RestClient(string.Format(UrlFormat, method, token, parameters));
            var request = new RestRequest(httpMethod);
            var response = client.Execute(request);
            var content = response.Content;
            LastRequestTime = DateTime.Now.Millisecond;
            return content;
        }
    }
}