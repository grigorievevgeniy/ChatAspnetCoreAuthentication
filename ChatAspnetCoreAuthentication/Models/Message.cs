using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Models
{
    public class Message
    {
        public string Text { get; set; }
        //public User User { get; set; }
        public string NameUser { get; set; }
        public DateTime Time { get; set; }
    }
}
