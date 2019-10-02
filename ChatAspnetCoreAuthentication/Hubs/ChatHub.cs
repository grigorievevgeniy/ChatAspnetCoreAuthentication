using ChatAspnetCoreAuthentication;
using ChatAspnetCoreAuthentication.Controllers;
using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Linq;
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

        public async Task SendMessage(ChatData dataFromClient)
        {
            IdentityUser identityUser = await _userManager.FindByNameAsync(dataFromClient.User);

            if (!await _userManager.IsInRoleAsync(identityUser, "block"))
            {
                if (!dataFromClient.Message.StartsWith("//"))
                {
                    ChatData dataFromServer = new ChatData()
                    {
                        User = dataFromClient.User,
                        Message = dataFromClient.Message,
                        Room = dataFromClient.Room
                    };

                    await Clients.All.SendAsync("ReceiveData", dataFromServer);

                    string SId = identityUser.Id;
                    string RId = _store.FindRoomIdByRoomName(dataFromClient.Room);

                    AddMessage(new ChatMessage() { SenderId = SId, Text = dataFromServer.Message, RoomId = RId });
                }
                else
                {
                    if (dataFromClient.Message.StartsWith("//start"))
                    {
                        try
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                User = dataFromClient.User,
                                Message = dataFromClient.Message + "\r\nВыберите доступную комнату или создайте новую.",
                                ListRooms = _store.GetAllRoomForUser(identityUser)
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//block") && await ChechRoleAdminModeratorAsync(identityUser))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//block ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.AddToRoleAsync(identityUser2, "block");

                            string answer = "Вы заблокировали пользователя " + nameUser2;
                            await Clients.Caller.SendAsync("ReceiveData", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//unblock") && await ChechRoleAdminModeratorAsync(identityUser))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//unblock ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.RemoveFromRoleAsync(identityUser2, "block");

                            string answer = "Вы разблокировали пользователя " + nameUser2;
                            await Clients.Caller.SendAsync("ReceiveData", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//appoint moderator") && await _userManager.IsInRoleAsync(identityUser, "admin"))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//appoint moderator ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.AddToRoleAsync(identityUser2, "moderator");

                            string answer = "Вы назначили модератором " + nameUser2;
                            await Clients.Caller.SendAsync("ReceiveData", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//disrank moderator") && await _userManager.IsInRoleAsync(identityUser, "admin"))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//disrank moderator ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.RemoveFromRoleAsync(identityUser2, "moderator");

                            string answer = "Вы разжаловали модератора " + nameUser2;
                            await Clients.Caller.SendAsync("ReceiveData", "", answer);
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//si"))
                    {
                        IdentityUser identityUser2 = await _userManager.FindByNameAsync("admin@simbirsoft.com");

                        string id = identityUser2.Id;

                        _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "SimbirSoft", OwnerId = id });
                        _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "Room1", OwnerId = id });
                        _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "Room2", OwnerId = id });

                        _store.appDbContext.SaveChanges();

                        string RoomId = _store.appDbContext.ChatRooms.Where(x => x.RoomName == "SimbirSoft").FirstOrDefault().RoomId;
                        _store.appDbContext.ChatUsers.Add(new ChatUser() { ChatId = RoomId, UserId = id });
                        RoomId = _store.appDbContext.ChatRooms.Where(x => x.RoomName == "Room1").FirstOrDefault().RoomId;
                        _store.appDbContext.ChatUsers.Add(new ChatUser() { ChatId = RoomId, UserId = id });
                        RoomId = _store.appDbContext.ChatRooms.Where(x => x.RoomName == "Room2").FirstOrDefault().RoomId;
                        _store.appDbContext.ChatUsers.Add(new ChatUser() { ChatId = RoomId, UserId = id });

                        _store.appDbContext.SaveChanges();
                    }
                    else if (dataFromClient.Message.StartsWith("//room create "))
                    {
                        try
                        {
                            string nameRoom = dataFromClient.Message.Replace("//room create ", "");
                            _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = nameRoom, OwnerId = identityUser.Id });
                            _store.appDbContext.SaveChanges();

                            string RoomId = _store.appDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault().RoomId;
                            _store.appDbContext.ChatUsers.Add(new ChatUser() { ChatId = RoomId, UserId = identityUser.Id });
                            _store.appDbContext.SaveChanges();

                            ChatData dataFromServer = new ChatData()
                            {
                                User = dataFromClient.User,
                                Message = "Вы создали и вошли в комнату " + nameRoom + "\r\n",
                                Room = nameRoom
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                User = "Error",
                                Message = ex.Message,
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                        // TODO после создания комнаты надо в нее сразу зайти
                    }
                    else if (dataFromClient.Message.StartsWith("//room remove "))
                    {
                        try
                        {
                            string nameRoom = dataFromClient.Message.Replace("//room remove ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(nameRoom) == identityUser.Id)
                            {
                                _store.RemoveChatRoomsByName(nameRoom);
                                _store.appDbContext.SaveChanges();

                                string answer = "Вы удалили комнату " + nameRoom;
                                await Clients.Caller.SendAsync("ReceiveData", "", answer);
                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                        // TODO после удаления переадресация в главную комнату
                    }
                    else if (dataFromClient.Message.StartsWith("//room enter "))
                    {
                        string nameRoom = dataFromClient.Message.Replace("//room enter ", "");

                        try
                        {

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(nameRoom) == identityUser.Id ||
                                _store.CheckUserMemberRoom(nameRoom, identityUser.Id))
                            {
                                ChatData dataFromServer = new ChatData()
                                {
                                    User = dataFromClient.User,
                                    Message = "Вы вошли в комнату " + nameRoom + "\r\n",
                                };

                                await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            }
                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                User = "Error",
                                Message = ex.Message,
                                Room = nameRoom
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//room rename "))
                    {
                        try
                        {
                            string newNameRoom = dataFromClient.Message.Replace("//room rename ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(dataFromClient.Room) == identityUser.Id)
                            {
                                _store.RenameRoom(dataFromClient.Room, newNameRoom);

                                // Возможно проверку условий надо полностью перенести в store
                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    // TODO допуск к команде общий, хотя это и протеворечит общей логике
                    else if (dataFromClient.Message.StartsWith("//room connect "))
                    {
                        try
                        {
                            string nameRoom = dataFromClient.Message.Replace("//room connect ", "");

                            string userId = identityUser.Id;
                            string roomId = _store.FindRoomIdByRoomName(nameRoom);
                            ChatUser chatUser = new ChatUser() { ChatId = roomId, UserId = userId };
                            _store.appDbContext.ChatUsers.Add(chatUser);
                            _store.appDbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//room disconnect "))
                    {
                        try
                        {
                            string nameRoom = dataFromClient.Message.Replace("//room disconnect ", "");

                            string userId = identityUser.Id;
                            string roomId = _store.FindRoomIdByRoomName(nameRoom);

                            _store.RemoveRoomUser(userId, roomId);

                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//user kick off "))
                    {
                        try
                        {
                            string nameUser = dataFromClient.Message.Replace("//user kick off ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(dataFromClient.Room) == identityUser.Id)
                            {
                                string userId = _userManager.FindByNameAsync(dataFromClient.User).Id.ToString();
                                string roomId = _store.FindRoomIdByRoomName(dataFromClient.Room);

                                _store.RemoveRoomUser(userId, roomId);

                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//user welcome "))
                    {
                        try
                        {
                            string nameUser = dataFromClient.Message.Replace("//user welcome ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(dataFromClient.Room) == identityUser.Id)
                            {
                                string userId = _userManager.FindByNameAsync(dataFromClient.User).Id.ToString();
                                string roomId = _store.FindRoomIdByRoomName(dataFromClient.Room);

                                _store.AddRoomUser(userId, roomId);

                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", "", ex.Message);
                        }
                    }
                    // //private room -     создать приватную комнату на двоих
                    // //help -             список доступных команд

                }

            }
            else
            {
                await Clients.Caller.SendAsync("ReceiveData", dataFromClient.User, "Вы заблокированны и не можете отправлять сообщения. Обратитесь к модератору или администратору.");
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
            await Clients.All.SendAsync("ReceiveData", user, message);
        }

        public void AddMessage(ChatMessage message)
        {
            _store.appDbContext.ChatMessages.Add(message);
            _store.appDbContext.SaveChanges();
        }

    }
}