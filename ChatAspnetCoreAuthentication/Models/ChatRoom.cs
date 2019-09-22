using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Models
{
    public class ChatRoom
    {
        [Key]
        public string ChatId { get; set; }
        public string ChatName { get; set; }

    }
}
