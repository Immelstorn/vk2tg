using System.Collections.Generic;

namespace vk2tg.Data.Models.Telegraph
{
    public class TelegraphResult
    {
        public string path { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string author_name { get; set; }
        public List<TelegraphContent> content { get; set; }
        public int views { get; set; }
        public bool can_edit { get; set; }
    }
}