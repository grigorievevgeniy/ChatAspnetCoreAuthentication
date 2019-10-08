using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Models
{
    public class ChatCommand
    {
        delegate void RunCommand();

        public string Name { get; set; }
        public string Description { get; set; }
        public RunCommand RunCommand { get; set; }

    }
}
