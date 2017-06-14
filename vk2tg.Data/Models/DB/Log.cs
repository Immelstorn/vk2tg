using System;

namespace vk2tg.Data.Models.DB
{
    public class Log
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int SubscriptionId { get; set; }
        public string Link { get; set; }
        public long PostId { get; set; }
    }
}