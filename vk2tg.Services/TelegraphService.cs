﻿using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using vk2tg.Data.Models.Telegraph;
using vk2tg.Data.Models.VK;

namespace vk2tg.Services
{
    public class TelegraphService
    {
        private const string TelegraphUrl = "https://api.telegra.ph/createPage";
        private static readonly string TelegraphAccessToken = ConfigurationManager.AppSettings["TelegraphAccessToken"];
        private const string ImgTemplate = "<img src='{0}'/>";
        private const string HrefTemplate = "<a href='{0}'>{1}</a>";
        private const string VideoTemplate = "<a href='{0}'>Video: {0}</a>";
        private const string AudioTemplate = "<a href='{0}'>{1} - {2}</a>";
        private const string UnsupportedAttachment = "Unsupported attachment";
        private const string AuthorUrlTemplate = "https://vk.com/{0}?w=wall{1}_{2}";
        private static readonly DataService _dataService = new DataService();


        public async Task<string> CreatePage(WallPost post, string groupName, string groupPrettyName)
        {
            var doc = new HtmlDocument();
            var htmlBuilder = new StringBuilder(post.text);
            var vkService = new VkService();

            if (post.attachments != null && post.attachments.Any())
            {

                foreach (var attachment in post.attachments)
                {

                    switch (attachment.type)
                    {
                        case "photo":

                            htmlBuilder.AppendLine("<br>");
                            htmlBuilder.AppendLine(await UploadImageAndGetHtml(attachment.photo.src_big));
                            break;
                        case "video":
                            var embedLink = vkService.GetVideoInfo(attachment.video.owner_id, attachment.video.vid, attachment.video.access_key);
                            htmlBuilder.AppendLine("<br>");
                            htmlBuilder.AppendLine(string.Format(VideoTemplate, embedLink));
                            break;
                        case "audio":
                            htmlBuilder.AppendLine("<br>");
                            htmlBuilder.AppendLine(string.Format(AudioTemplate, attachment.audio.url, attachment.audio.artist, attachment.audio.title));
                            break;
                        case "link":
                            htmlBuilder.AppendLine("<br>");
                            htmlBuilder.AppendLine(await UploadImageAndGetHtml(attachment.link.image_big ?? attachment.link.image_src));
                            htmlBuilder.AppendLine(string.Format(HrefTemplate, attachment.link.url, attachment.link.title));
                            break;
                        default:
                            htmlBuilder.AppendLine("<br>");
                            htmlBuilder.AppendLine(UnsupportedAttachment);
                            break;
                    }
                }
            }

            doc.LoadHtml(htmlBuilder.ToString());
            var jArray = new JArray();
            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                jArray.Add(DomToNode(node));
            }
            var json = jArray.ToString();


            var client = new RestClient(TelegraphUrl);
            var request = new RestRequest(Method.POST);
            request.AddParameter("access_token", TelegraphAccessToken);
            request.AddParameter("title", groupPrettyName);
            request.AddParameter("author_name", groupPrettyName);
            request.AddParameter("author_url", string.Format(AuthorUrlTemplate, groupName, post.from_id, post.id));
            request.AddParameter("content", json);
            var response = client.Execute(request);
            var content = response.Content;
            var result = JsonConvert.DeserializeObject<TelegraphResponse>(content);
            if (result.ok != "false")
            {
                return result.result.url;
            }

            return null;
        }

        private async Task<string> UploadImageAndGetHtml(string imgUrl)
        {
            await _dataService.AddTraceLog(imgUrl);
            var uploadResult = imgUrl == null ? null : await UploadToCloudinary(imgUrl);
            return string.Format(ImgTemplate,
                                 string.IsNullOrEmpty(uploadResult)
                                     ? imgUrl
                                     : uploadResult);
        }

//        private static void GetImgurLimit()
//        {
//            var client = new RestClient("https://api.imgur.com/3/credits");
//            var request = new RestRequest();
//            request.AddHeader("Authorization", "Client-ID " + ConfigurationManager.AppSettings["ImgurClientId"]);
//            var response = client.Execute(request);
//            var content = response.Content;
//            _dataService.AddTraceLogSync(content);
//        }

        private async Task<string> UploadToCloudinary(string url)
        {
            var account = new Account(ConfigurationManager.AppSettings["CloudinaryCloud"], ConfigurationManager.AppSettings["CloudinaryApiKey"], ConfigurationManager.AppSettings["CloudinaryApiSecret"]);

            var cloudinary = new Cloudinary(account);
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(url)
            };
            var uploadResult = await cloudinary.UploadAsync(uploadParams);
            await _dataService.AddTraceLog(uploadResult.Uri.AbsoluteUri);

            return uploadResult.Uri.AbsoluteUri;
        }

//        private static IImage UploadPhoto(string url)
//        {
//            var imgur = new ImgurClient(ConfigurationManager.AppSettings["ImgurClientId"], ConfigurationManager.AppSettings["ImgurClientSecret"]);
//            var endpoint = new ImageEndpoint(imgur);
//            var limit = endpoint.ApiClient.RateLimit;
//            GetImgurLimit();
//            try
//            {
//                var result = endpoint.UploadImageUrlAsync(url).Result;
//                return result;
//            }
//            catch(ImgurException e)
//            {
//                _dataService.AddErrorLogSync(e);
//                if(e.InnerException != null)
//                {
//                    _dataService.AddErrorLogSync(e);
//                }
//            }
//
//            return null;
//        }

        private static object DomToNode(HtmlNode node)
        {
            dynamic nodeElement = new JObject();
            if (node.NodeType == HtmlNodeType.Text)
            {
                return node.InnerText;
            }

            if (node.NodeType != HtmlNodeType.Element)
            {
                return new JObject();
            }

            nodeElement.tag = node.Name.ToLower();
            foreach (var attr in node.Attributes)
            {
                if (attr.Name == "href" || attr.Name == "src")
                {
                    if (nodeElement.attrs == null)
                    {
                        nodeElement.attrs = new JObject();
                    }
                    nodeElement.attrs[attr.Name] = attr.Value;
                }
            }

            if (node.ChildNodes.Count > 0)
            {
                nodeElement.children = new JArray();
                foreach (var child in node.ChildNodes)
                {
                    nodeElement.children.Add(DomToNode(child));
                }
            }

            return nodeElement;
        }
    }
}