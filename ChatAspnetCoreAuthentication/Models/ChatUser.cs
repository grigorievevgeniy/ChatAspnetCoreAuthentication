using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Models
{
    public class ChatUser
    {
        [Key(Order = 1)]
        public string ChatId { get; set; }
        [Key(Order = 2)]
        public string UserId { get; set; }

    }
}
