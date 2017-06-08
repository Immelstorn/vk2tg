using System;

namespace vk2tg.Data.Models
{
    public class Log
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public Subscription Subscription { get; set; }
        public string Link { get; set; }
        public long PostId { get; set; }
    }
}