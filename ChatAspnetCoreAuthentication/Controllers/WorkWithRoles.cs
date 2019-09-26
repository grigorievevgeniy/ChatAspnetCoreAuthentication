using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Controllers
{
    public class WorkWithRoles
    {
        public UserManager<IdentityUser> _userManager;
        public RoleManager<IdentityRole> _roleManager;

        public WorkWithRoles()
        {
        }

        public WorkWithRoles(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async void AddRoleUserForRigister(IdentityUser user)
        {
            await _userManager.AddToRoleAsync(user, "user");
        }



    }
}
