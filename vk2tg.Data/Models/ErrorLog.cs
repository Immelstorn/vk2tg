using System;

namespace vk2tg.Data.Models
{
    public class ErrorLog
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}