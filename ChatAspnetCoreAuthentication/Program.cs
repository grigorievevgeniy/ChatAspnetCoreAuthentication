using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using ChatAspnetCoreAuthentication.Models;
using ChatAspnetCoreAuthentication.Controllers;
using ChatAspnetCoreAuthentication.Data;

namespace ChatAspnetCoreAuthentication
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var host = BuildWebHost(args);

            using (var scope = host.Services.CreateScope())
            {
                Task t;
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                //    ApplicationDbContext applicationDbContext = new ApplicationDbContext(options =>
                //options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                    t = Initializer.InitializeAsync(userManager, rolesManager);
                    t.Wait();

                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args).UseStartup<Startup>().Build();


    }
}