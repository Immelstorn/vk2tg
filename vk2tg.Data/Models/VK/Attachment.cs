namespace vk2tg.Data.Models.VK
{
    public class Attachment
    {
        public string type { get; set; }
        public Link link { get; set; }
        public Photo photo { get; set; }
        public Video video { get; set; }
        public Audio audio { get; set; }
        public Page page { get; set; }
    }
}