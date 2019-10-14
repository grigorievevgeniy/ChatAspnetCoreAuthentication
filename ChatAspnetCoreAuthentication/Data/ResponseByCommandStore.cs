using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Data
{
    public class ResponseByCommandStore
    {
        public string Receiver;
        public string NameReceiver;
        public ChatData ChatData;

        public ResponseByCommandStore(string receiver, ChatData chatData)
        {
            Receiver = receiver;
            ChatData = chatData;
        }

        public ResponseByCommandStore(string receiver, string nameReceiver, ChatData chatData)
        {
            Receiver = receiver;
            NameReceiver = nameReceiver;
            ChatData = chatData;
        }
    }
}
