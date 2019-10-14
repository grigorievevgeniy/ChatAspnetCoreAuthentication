using ChatAspnetCoreAuthentication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Models
{
    public class ChatCommand
    {
        public delegate Task<ChatData> Command(ChatData data, ApplicationStore appStore);

        public string Name { get; set; }
        public string Description { get; set; }
        public Command RunCommand { get; set; }
    }
}
