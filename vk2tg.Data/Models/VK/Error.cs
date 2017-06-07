using System.Collections.Generic;

namespace vk2tg.Data.Models.VK
{
    public class Error
    {
        public int error_code { get; set; }
        public string error_msg { get; set; }
        public List<RequestParam> request_params { get; set; }
    }
}