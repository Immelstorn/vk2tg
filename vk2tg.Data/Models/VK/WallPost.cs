using System.Collections.Generic;

namespace vk2tg.Data.Models.VK
{
    public class WallPost
    {
        public long id { get; set; }
        public int from_id { get; set; }
        public int to_id { get; set; }
        public int date { get; set; }
        public int marked_as_ads { get; set; }
        public string post_type { get; set; }
        public string text { get; set; }
        public int signer_id { get; set; }
        public int is_pinned { get; set; }
        public Media media { get; set; }
        public Attachment attachment { get; set; }
        public List<Attachment> attachments { get; set; }
        public PostSource post_source { get; set; }
        public Comments comments { get; set; }
        public Likes likes { get; set; }
        public Reposts reposts { get; set; }
        public int online { get; set; }
        public int reply_count { get; set; }
        public int? copy_post_date { get; set; }
        public string copy_post_type { get; set; }
        public int? copy_owner_id { get; set; }
        public int? copy_post_id { get; set; }
    }
}