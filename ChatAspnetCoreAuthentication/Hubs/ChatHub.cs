using ChatAspnetCoreAuthentication;
using ChatAspnetCoreAuthentication.Controllers;
using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace SignalRChat.Hubs
{
    [Authorize]
    //[Authorize(Roles = "admin")]
    public class ChatHub : Hub
    {
        private ApplicationStore _store;
        //WorkWithRoles workWithRoles;
        UserManager<IdentityUser> _userManager;

        public ChatHub(ApplicationStore store, UserManager<IdentityUser> userManager)
        {
            _store = store;
            _userManager = userManager;
        }

        public async Task SendMessage(string user, string message, string room)
        {
            IdentityUser identityUser = await _userManager.FindByNameAsync(user);

            if (!await _userManager.IsInRoleAsync(identityUser, "block"))
            {
                if (!message.StartsWith("//"))
                {
                    await Clients.All.SendAsync("ReceiveMessage", user, message);

                    string SId = identityUser.Id;
                    string RId = _store.FindRoomIdByRoomName(room);

                    AddMessage(new ChatMessage() { SenderId = SId, Text = message, RoomId = RId });
                }
                else
                {
                    if(message.StartsWith("//block") && await ChechRoleAdminModeratorAsync(identityUser))
                    {
                        try
                        {
                            string nameUser2 = message.Replace("//block ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.AddToRoleAsync(identityUser2, "block");

                            string answer = "Вы заблокировали пользователя " + nameUser2;
                            await Clients.Caller.SendAsync("ReceiveMessage", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//unblock") && await ChechRoleAdminModeratorAsync(identityUser))
                    {
                        try
                        {
                            string nameUser2 = message.Replace("//unblock ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.RemoveFromRoleAsync(identityUser2, "block");

                            string answer = "Вы разблокировали пользователя " + nameUser2;
                            await Clients.Caller.SendAsync("ReceiveMessage", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//appoint moderator") && await _userManager.IsInRoleAsync(identityUser, "admin"))
                    {
                        try
                        {
                            string nameUser2 = message.Replace("//appoint moderator ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.AddToRoleAsync(identityUser2, "moderator");

                            string answer = "Вы назначили модератором " + nameUser2;
                            await Clients.Caller.SendAsync("ReceiveMessage", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//disrank moderator") && await _userManager.IsInRoleAsync(identityUser, "admin"))
                    {
                        try
                        {
                            string nameUser2 = message.Replace("//disrank moderator ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.RemoveFromRoleAsync(identityUser2, "moderator");

                            string answer = "Вы разжаловали модератора " + nameUser2;
                            await Clients.Caller.SendAsync("ReceiveMessage", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//si"))
                    {
                        IdentityUser identityUser2 = await _userManager.FindByNameAsync("admin@simbirsoft.com");

                        string id = identityUser2.Id;

                        _store._applicationDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "SimbirSoft", OwnerId = id });
                        _store._applicationDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "Room1", OwnerId = id });
                        _store._applicationDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "Room2", OwnerId = id });

                        _store._applicationDbContext.SaveChanges();
                    }
                    else if (message.StartsWith("//room create "))
                    {
                        try
                        {
                            string nameRoom = message.Replace("//room create ", "");
                            _store._applicationDbContext.ChatRooms.Add(new ChatRoom() { RoomName = nameRoom, OwnerId = identityUser.Id });
                            _store._applicationDbContext.SaveChanges();

                            string answer = "Вы создали комнату " + nameRoom;
                            await Clients.Caller.SendAsync("ReceiveMessage", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//room remove "))
                    {
                        try
                        {
                            string nameRoom = message.Replace("//room remove ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(nameRoom) == identityUser.Id)
                            {
                                _store.RemoveChatRoomsByName(nameRoom);
                                _store._applicationDbContext.SaveChanges();

                                string answer = "Вы удалили комнату " + nameRoom;
                                await Clients.Caller.SendAsync("ReceiveMessage", "", answer);
                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }

                    }
                }

            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveMessage", user, "Вы заблокированны и не можете отправлять сообщения. Обратитесь к модератору или администратору.");
            }


        }

        private async Task<bool> ChechRoleAdminModeratorAsync(IdentityUser identityUser)
        {
            if (await _userManager.IsInRoleAsync(identityUser, "admin") || 
                await _userManager.IsInRoleAsync(identityUser, "moderator"))
            {
                return true;
            }

            return false;
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