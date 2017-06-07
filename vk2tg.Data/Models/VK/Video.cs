namespace vk2tg.Data.Models.VK
{
    public class Video
    {
        public int vid { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
        public string description { get; set; }
        public int date { get; set; }
        public int views { get; set; }
        public string image { get; set; }
        public string image_big { get; set; }
        public string image_small { get; set; }
        public string image_xbig { get; set; }
        public string access_key { get; set; }
        public string platform { get; set; }
    }
}