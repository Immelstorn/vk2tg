namespace vk2tg.Data.Models.VK
{
    public class Photo
    {
        public int pid { get; set; }
        public int aid { get; set; }
        public int owner_id { get; set; }
        public int user_id { get; set; }
        public string src { get; set; }
        public string src_big { get; set; }
        public string src_small { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string text { get; set; }
        public int created { get; set; }
        public int post_id { get; set; }
        public string access_key { get; set; }
        public string src_xbig { get; set; }
        public string src_xxbig { get; set; }
        public string src_xxxbig { get; set; }
        public double lat { get; set; }
        public double @long { get; set; }
    }
}