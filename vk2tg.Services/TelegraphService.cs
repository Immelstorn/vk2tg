﻿using System.Configuration;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
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
        private const string VideoTemplate = "<a href='{0}'>Video: {0}</a>";
        private const string AudioTemplate = "<a href='{0}'>{1} - {2}</a>";
        private const string UnsupportedAttachment = "Unsupported attachment";
        private const string AuthorUrlTemplate = "https://vk.com/{0}?w=wall{1}_{2}";
        
        public string CreatePage(WallPost post, string groupName, string groupPrettyName)
        {
            var doc = new HtmlDocument();
            var htmlBuilder = new StringBuilder(post.text);
            var vkService = new VkService();
            if (post.attachments != null && post.attachments.Any())
            {
                foreach(var attachment in post.attachments)
                {
                    switch(attachment.type)
                    {
                        case "photo":
                            htmlBuilder.AppendLine("<br>");

                            var imgurResult = UploadPhoto(attachment.photo.src_big);
                            htmlBuilder.AppendLine(string.Format(ImgTemplate,
                                                                     string.IsNullOrEmpty(imgurResult?.Link) 
                                                                         ? attachment.photo.src_big 
                                                                         : imgurResult.Link));

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

        private static IImage UploadPhoto(string url)
        {
            var imgur = new ImgurClient(ConfigurationManager.AppSettings["ImgurClientId"], ConfigurationManager.AppSettings["ImgurClientSecret"]);
            var endpoint = new ImageEndpoint(imgur);
            var result = endpoint.UploadImageUrlAsync(url).Result;
            return result;
        }

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