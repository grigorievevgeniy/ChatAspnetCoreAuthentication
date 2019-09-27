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
        public string RoomId { get; set; }
        public string RoomName { get; set; }

        public string OwnerId { get; set; }

    }
}
