using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace vk2tg.Data.Models
{
    public class User
    {
       
        public int Id { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}