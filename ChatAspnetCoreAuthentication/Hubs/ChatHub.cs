using ChatAspnetCoreAuthentication;
using ChatAspnetCoreAuthentication.Controllers;
using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    [Authorize]
    //[Authorize(Roles = "admin")]
    public class ChatHub : Hub
    {
        private ApplicationStore _store;
        WorkWithRoles workWithRoles;
        UserManager<IdentityUser> _userManager;

        public ChatHub(ApplicationStore store, UserManager<IdentityUser> userManager)
        {
            _store = store;
            _userManager = userManager;
        }

        public async Task SendMessage(string user, string message)
        {
            IdentityUser identityUser = await _userManager.FindByNameAsync(user);

            if (!await _userManager.IsInRoleAsync(identityUser, "block"))
            {
                if (!message.StartsWith("//"))
                {
                    await Clients.All.SendAsync("ReceiveMessage", user, message);

                    AddMessage(new ChatMessage() { SenderId = user, Text = message });
                }
                else
                {
                    await Clients.Caller.SendAsync("ReceiveMessage", user, "Команды пока не реализованны");
                }

            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMessage", user, "Вы заблокированны и не можете отправлять сообщения. Обратитесь к модератору или администратору.");
            }


        }

        // TODO доработать оповещение новго пользователя
        public async Task NewUser(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }

        public void AddMessage(ChatMessage message)
        {
            _store._applicationDbContext.ChatMessages.Add(message);
            _store._applicationDbContext.SaveChanges();
        }

    }
}