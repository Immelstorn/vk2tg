namespace vk2tg.Data.Models.VK
{
    public class VideoGetResponse
    {
        public int vid { get; set; }
        public int owner_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int duration { get; set; }
        public string link { get; set; }
        public int date { get; set; }
        public int views { get; set; }
        public string image { get; set; }
        public string image_medium { get; set; }
        public int comments { get; set; }
        public string player { get; set; }
    }
}