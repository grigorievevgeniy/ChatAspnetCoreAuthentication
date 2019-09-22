using ChatAspnetCoreAuthentication;
using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private ApplicationStore _store;

        public ChatHub(ApplicationStore store)
        {
            _store = store;
        }

        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);

            AddMessage(new ChatMessage() { SenderId = user, Text = message });
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