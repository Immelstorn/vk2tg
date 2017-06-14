using System.Collections.Generic;

namespace vk2tg.Data.Models.DB
{
    public class Subscription
    {
       
        public int Id { get; set; }
        public long SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public string SubscriptionPrettyName { get; set; }
        public long LastPostId { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}