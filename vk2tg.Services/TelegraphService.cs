using System.Configuration;
using System.Linq;
using System.Text;
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
        private const string VideoTemplate = "<a href='{0}'>Video: {0}</a>";
        private const string UnsupportedAttachment = "Unsupported attachment";
        private const string AuthorUrlTemplate = "https://vk.com/{0}?w=wall{1}_{2}";
        
        public string CreatePage(WallPost post, string groupName)
        {
            var doc = new HtmlDocument();
            var htmlBuilder = new StringBuilder(post.text);
            var vkService = new VkService();
            if (post.attachments.Any())
            {
                foreach(var attachment in post.attachments)
                {
                    switch(attachment.type)
                    {
                        case "photo":
                            htmlBuilder.AppendLine("<br>");
                            htmlBuilder.AppendLine(string.Format(ImgTemplate, attachment.photo.src_big));
                            break;
                        case "video":
                            var embedLink = vkService.GetVideoInfo(attachment.video.owner_id, attachment.video.vid, attachment.video.access_key);
                            htmlBuilder.AppendLine("<br>");
                            htmlBuilder.AppendLine(string.Format(VideoTemplate, embedLink));
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
            request.AddParameter("title", groupName);
            request.AddParameter("author_name", groupName);
            request.AddParameter("author_url", string.Format(AuthorUrlTemplate, groupName, post.from_id, post.id));
            request.AddParameter("content", json);
            var response = client.Execute(request);
            var content = response.Content;
            var result = JsonConvert.DeserializeObject<TelegraphResponse>(content);
            if (result.ok)
            {
                return result.result.url;
            }

            return null;
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