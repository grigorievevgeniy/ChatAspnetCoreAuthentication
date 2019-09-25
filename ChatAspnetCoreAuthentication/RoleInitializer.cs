using Microsoft.AspNetCore.Identity;
using ChatAspnetCoreAuthentication.Models;
using System.Threading.Tasks;
 
namespace ChatAspnetCoreAuthentication
{
    public class RoleInitializer
    {
        public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {

            // Добавление ролей
            if (await roleManager.FindByNameAsync("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }
            if (await roleManager.FindByNameAsync("moderator") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("moderator"));
            }
            if (await roleManager.FindByNameAsync("user") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("user"));
            }

            // Добавление пользователей и их ролей
            if (await userManager.FindByNameAsync("admin@simbirsoft.com") == null)
            {
                IdentityUser identityUser = new IdentityUser { Email = "admin@simbirsoft.com", UserName = "admin@simbirsoft.com" };
                IdentityResult result = await userManager.CreateAsync(identityUser, "qweqwe");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(identityUser, "admin");
                }
            }
            if (await userManager.FindByNameAsync("moderator@simbirsoft.com") == null)
            {
                IdentityUser identityUser = new IdentityUser { Email = "moderator@simbirsoft.com", UserName = "moderator@simbirsoft.com" };
                IdentityResult result = await userManager.CreateAsync(identityUser, "qweqwe");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(identityUser, "moderator");
                }
            }
            if (await userManager.FindByNameAsync("user@simbirsoft.com") == null)
            {
                IdentityUser identityUser = new IdentityUser { Email = "user@simbirsoft.com", UserName = "user@simbirsoft.com" };
                IdentityResult result = await userManager.CreateAsync(identityUser, "qweqwe");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(identityUser, "user");
                }
            }
        }
    }
}