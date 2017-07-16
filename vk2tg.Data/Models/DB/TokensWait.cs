using System;

namespace vk2tg.Data.Models.DB
{
    public class TokensWait
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime WaitUntil { get; set; }
    }
}