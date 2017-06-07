using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using vk2tg.Data.Models.VK;

namespace vk2tg.Services
{
    
    public class VkService
    {
        private readonly string _accessTokens = ConfigurationManager.AppSettings["AccessTokens"];
        private readonly List<string> _tokens;

        private const string UrlFormat = "https://api.vk.com/method/{0}?access_token={1}&{2}";

        public VkService()
        {
            _tokens = _accessTokens.Split(';').ToList();
        }

        public string GetVideoInfo(long ownerId, long videoId, string accessKey)
        {
            const string method = "video.get";
            const Method httpMethod = Method.GET;
            var parameters = $"owner_id={ownerId}&videos={ownerId}_{videoId}_{accessKey}";

            foreach(var token in EnumerateTokens())
            {
                var video = GetListResult<VideoGetResponse>(method, token, parameters, httpMethod);
                return video == null || !video.Any()
                    ? null
                    : video.First().player;
            }
            return null;
        }

        public GroupsResponse GetGroupInfo(string groupName)
        {
            const string method = "groups.getById";
            const Method httpMethod = Method.GET;
            var parameters = $"group_id={groupName}";

            foreach(var token in EnumerateTokens())
            {
                var content = ExecuteRequest(method, token, parameters, httpMethod);
                var result = JsonConvert.DeserializeObject<GroupsGetResponse>(content);
                if(result?.response != null && result.response.Count == 1)
                {
                    return result.response.First();
                }

                var error = JsonConvert.DeserializeObject<ErrorResponse>(content);
                Debug.WriteLine(error);
                _tokens.Remove(token);
            }

            return null;
        }

        public List<WallPost> GetPosts(long subscriptionId, long lastPostId, int count = 10)
        {
            const string method = "wall.get";
            const Method httpMethod = Method.GET;
            var parameters = $"owner_id={subscriptionId}&count={count}&filter=owner";

            foreach(var token in EnumerateTokens())
            {
                var newPosts = GetListResult<WallPost>(method, token, parameters, httpMethod);
                if(newPosts != null && newPosts.Any())
                {
                    return newPosts.Where(p => p.id > lastPostId).ToList();
                }
            }

            return new List<WallPost>();
        }

//        private T GetOneResult<T>(string method, string token, string parameters, Method httpMethod)
//        {
//            var content = ExecuteRequest(method, token, parameters, httpMethod);
//            var result = JsonConvert.DeserializeObject<T>(content);
//            if (result?.response != null && result.response.Count > 1)
//            {
//                return result;
//            }
//            else
//            {
//                var error = JsonConvert.DeserializeObject<ErrorResponse>(content);
//                Debug.WriteLine(error);
//                _tokens.Remove(token);
//            }
//
//            return null;
//        }

        private List<T> GetListResult<T>(string method, string token, string parameters, Method httpMethod)
        {
            var content = ExecuteRequest(method, token, parameters, httpMethod);
            var result = JsonConvert.DeserializeObject<VkResult>(content);
            if (result?.Response != null && result.Response.Count > 1)
            {
                result.Response.RemoveAt(0);
                var t = result.Response.Select(r => JsonConvert.DeserializeObject<T>(r.ToString())).ToList();
                return t;
            }

            var error = JsonConvert.DeserializeObject<ErrorResponse>(content);
            Debug.WriteLine(error);
            _tokens.Remove(token);

            return null;
        }

        private IEnumerable<string> EnumerateTokens()
        {
            var index = 0;
            while (_tokens.Count != 0)
            {
                yield return _tokens[index++ % _tokens.Count];
            }
        }

        private static string ExecuteRequest(string method, string token, string parameters, Method httpMethod)
        {
            var client = new RestClient(string.Format(UrlFormat, method, token, parameters));
            var request = new RestRequest(httpMethod);
            var response = client.Execute(request);
            var content = response.Content;
            return content;
        }
    }
}