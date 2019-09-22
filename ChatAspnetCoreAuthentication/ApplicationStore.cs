using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication
{
    public class ApplicationStore
    {
        public ApplicationStore(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public ApplicationDbContext _applicationDbContext;

        void AddMessage(ChatMessage message)
        {
            _applicationDbContext.ChatMessages.Add(message);
            _applicationDbContext.SaveChanges();
        }
    }
}
