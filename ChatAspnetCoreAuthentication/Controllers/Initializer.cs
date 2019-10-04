using Microsoft.AspNetCore.Identity;
using ChatAspnetCoreAuthentication.Models;
using System.Threading.Tasks;
using ChatAspnetCoreAuthentication.Data;
using SignalRChat.Hubs;

namespace ChatAspnetCoreAuthentication.Controllers
{
    public class Initializer
    {

        // начальную инициализацию можно перенести в метод Seed => Migrations
        public static async Task InitializeAsync(UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager, ApplicationDbContext applicationDbContext)
        {

            #region Добавление ролей
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
            if (await roleManager.FindByNameAsync("block") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("block"));
            }
            #endregion

            #region Добавление пользователей и их ролей
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
            #endregion

            #region Добавление групп SignalR
            ApplicationStore applicationStore = new ApplicationStore(applicationDbContext);
            
            // TODO возможно юзер менеджер и не нужен, проверить позже.
            //ChatHub chatHub = new ChatHub(applicationStore);

            ChatHub chatHub = new ChatHub(applicationStore, userManager);
            chatHub.SeedSignalRGroup();
            #endregion
        }
    }
}