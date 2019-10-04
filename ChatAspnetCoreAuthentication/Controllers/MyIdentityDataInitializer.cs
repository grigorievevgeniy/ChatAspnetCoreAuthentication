using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using SignalRChat.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Controllers
{
    public class MyIdentityDataInitializer
    {
        public static void SeedData (UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            SeedRoles(roleManager);
            SeedUsers(userManager);
            //SeedSignalRGroupsAsync(applicationDbContext, userManager, applicationStore);
        }

        public static void SeedUsers (UserManager<IdentityUser> userManager)
        {
            if (userManager.FindByNameAsync("user1").Result == null)
            {
                IdentityUser user = new IdentityUser();
                user.UserName = "user1";
                user.Email = "user1@localhost";

                IdentityResult result = userManager.CreateAsync
                (user, "password_goes_here").Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "NormalUser").Wait();
                }
            }


            if (userManager.FindByNameAsync("user2").Result == null)
            {
                IdentityUser user = new IdentityUser();
                user.UserName = "user2";
                user.Email = "user2@localhost";

                IdentityResult result = userManager.CreateAsync
                (user, "password_goes_here").Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(user, "Administrator").Wait();
                }
            }
        }

        public static void SeedRoles (RoleManager<IdentityRole> roleManager)
        {
            {
                if (!roleManager.RoleExistsAsync("NormalUser").Result)
                {
                    IdentityRole role = new IdentityRole();
                    role.Name = "NormalUser";
                    IdentityResult roleResult = roleManager.
                    CreateAsync(role).Result;
                }

                if (!roleManager.RoleExistsAsync("Administrator").Result)
                {
                    IdentityRole role = new IdentityRole();
                    role.Name = "Administrator";
                    IdentityResult roleResult = roleManager.
                    CreateAsync(role).Result;
                }
            }
        }

        //public static async void SeedSignalRGroupsAsync(ApplicationDbContext applicationDbContext,
        //    //IGroupManager groupManager,
        //    UserManager<IdentityUser> userManager,
        //    ApplicationStore applicationStore)
        //{
        //    List<Room> groups = new List<Room>();

        //    foreach (var item in applicationDbContext.ChatRooms)
        //    {
        //        groups.Add(new Room() { Name = item.RoomName, Id = item.RoomId });
        //    }

        //    ChatHub chatHub = new ChatHub(applicationStore, userManager);

        //    foreach (var item in applicationDbContext.ChatUsers)
        //    {
        //        for (int i = 0; i < groups.Count; i++)
        //        {
        //            if (item.ChatId == groups[i].Id)
        //            {
        //                try
        //                {
        //                    await chatHub.Groups.AddToGroupAsync(item.UserId, groups[i].Name);
        //                }
        //                catch (Exception ex)
        //                {

        //                    string ss = ex.Message;
        //                }

        //                //await groupManager.AddToGroupAsync(item.UserId, groups[i].Name);
        //            }
        //        }
        //    }

        //}

        class Room
        {
            public string Name { get; set; }
            public string Id { get; set; }
        }

    }
}
