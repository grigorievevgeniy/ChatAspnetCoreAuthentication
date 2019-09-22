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
        public string SenderId { get; set; }
        public string Text { get; set; }

        //public ChatMessage(string user, string message)
        //{
        //    SenderId = user;
        //    Text = message;
        //}
        //public ChatMessage()
        //{
        //}
    }
}
