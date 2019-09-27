using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Models
{
    public class ChatMessage
    {
        [Key]
        public string MessageId { get; set; }

        public string RoomId { get; set; }

        public string SenderId { get; set; }
        public string Text { get; set; }

        public DateTime Time { get; set; } = DateTime.Now;

        public bool Deleted { get; set; } = false;
        // хотя итак по умолчанию false...

        // Несколько усложняет задачу
        // TODO реализовать позже, кто удалил
        //public string DeletedId { get; set; }
    }
}
