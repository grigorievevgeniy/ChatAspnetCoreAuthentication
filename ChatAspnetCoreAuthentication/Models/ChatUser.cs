using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Models
{
    public class ChatUser
    {
        [Key]
        public string ChatId { get; set; }
        [Key]
        public string UserId { get; set; }

    }
}
