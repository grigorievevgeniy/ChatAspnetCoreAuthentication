using ChatAspnetCoreAuthentication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication
{
    public class ApplicationStore
    {
        ApplicationDbContext _applicationDbContext;

        public ApplicationStore(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
    }
}
