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
                    //await Clients.Clients

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

                        _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "SimbirSoft", OwnerId = id });
                        _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "Room1", OwnerId = id });
                        _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "Room2", OwnerId = id });

                        _store.appDbContext.SaveChanges();
                    }
                    else if (message.StartsWith("//room create "))
                    {
                        try
                        {
                            string nameRoom = message.Replace("//room create ", "");
                            _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = nameRoom, OwnerId = identityUser.Id });
                            _store.appDbContext.SaveChanges();

                            string answer = "Вы создали комнату " + nameRoom;
                            await Clients.Caller.SendAsync("ReceiveMessage", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                        // TODO после создания комнаты надо в нее сразу зайти
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
                                _store.appDbContext.SaveChanges();

                                string answer = "Вы удалили комнату " + nameRoom;
                                await Clients.Caller.SendAsync("ReceiveMessage", "", answer);
                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                        // TODO после удаления переадресация в главную комнату
                    }
                    else if (message.StartsWith("//room enter "))
                    {
                        try
                        {
                            string nameRoom = message.Replace("//room enter ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(nameRoom) == identityUser.Id ||
                                _store.CheckUserMemberRoom(nameRoom, identityUser.Id))
                            {
                                // TODO описать вхождение

                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//room rename "))
                    {
                        try
                        {
                            string newNameRoom = message.Replace("//room rename ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(room) == identityUser.Id)
                            {
                                _store.RenameRoom(room, newNameRoom);

                                // Возможно проверку условий надо полностью перенести в store
                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    // TODO допуск к команде общий, хотя это и протеворечит общей логике
                    else if (message.StartsWith("//room connect "))
                    {
                        try
                        {
                            string nameRoom = message.Replace("//room connect ", "");

                            string userId = identityUser.Id;
                            string roomId = _store.FindRoomIdByRoomName(nameRoom);
                            ChatUser chatUser = new ChatUser() { ChatId = roomId, UserId = userId };
                            _store.appDbContext.ChatUsers.Add(chatUser);
                            _store.appDbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//room disconnect "))
                    {
                        try
                        {
                            string nameRoom = message.Replace("//room disconnect ", "");

                            string userId = identityUser.Id;
                            string roomId = _store.FindRoomIdByRoomName(nameRoom);

                            _store.RemoveRoomUser(userId, roomId);

                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//user kick off "))
                    {
                        try
                        {
                            string nameUser = message.Replace("//user kick off ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(room) == identityUser.Id)
                            {
                                string userId = _userManager.FindByNameAsync(user).Id.ToString();
                                string roomId = _store.FindRoomIdByRoomName(room);

                                _store.RemoveRoomUser(userId, roomId);

                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    else if (message.StartsWith("//user welcome "))
                    {
                        try
                        {
                            string nameUser = message.Replace("//user welcome ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(room) == identityUser.Id)
                            {
                                string userId = _userManager.FindByNameAsync(user).Id.ToString();
                                string roomId = _store.FindRoomIdByRoomName(room);

                                _store.AddRoomUser(userId, roomId);

                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveMessage", "", ex.Message);
                        }
                    }
                    // //private room -     создать приватную комнату на двоих
                    // //help -             список доступных команд

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
            _store.appDbContext.ChatMessages.Add(message);
            _store.appDbContext.SaveChanges();
        }

    }
}