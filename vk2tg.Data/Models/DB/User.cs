using System;
using System.Collections.Generic;

namespace vk2tg.Data.Models.DB
{
    public class User
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}