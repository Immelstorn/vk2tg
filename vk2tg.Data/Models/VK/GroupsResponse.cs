namespace vk2tg.Data.Models.VK
{
    public class GroupsResponse
    {
        public long gid { get; set; }
        public string name { get; set; }
        public string screen_name { get; set; }
        public int is_closed { get; set; }
        public string type { get; set; }
        public int is_admin { get; set; }
        public int is_member { get; set; }
        public string photo { get; set; }
        public string photo_medium { get; set; }
        public string photo_big { get; set; }
    }
}