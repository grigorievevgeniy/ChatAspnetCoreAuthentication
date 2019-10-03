using ChatAspnetCoreAuthentication;
using ChatAspnetCoreAuthentication.Controllers;
using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                // Если не команда, то отправить сообщение 
                if (!dataFromClient.Message.StartsWith("//"))
                {
                    ChatData dataFromServer = new ChatData()
                    {
                        User = dataFromClient.User,
                        Message = dataFromClient.Message,
                        Room = dataFromClient.Room
                    };

                    await Clients.All.SendAsync("ReceiveData", dataFromServer);

                    AddMessage(new ChatMessage()
                    {
                        SenderId = identityUser.Id,
                        Text = dataFromServer.Message,
                        RoomId = _store.FindRoomIdByRoomName(dataFromClient.Room)
                    });
                }
                else
                {
                    // Старт, загрузка комнат и пользователей
                    // TODO лучше вынести в отдельной окно (в клиенте)
                    if (dataFromClient.Message.StartsWith("//start"))
                    {
                        try
                        {
                            var list = _userManager.Users;
                            string allUsers = "";
                            foreach (var item in list)
                                allUsers += item.UserName + "\r\n";

                            ChatData dataFromServer = new ChatData()
                            {
                                User = dataFromClient.User,
                                SystemMessage = "Выберите доступную комнату или создайте новую.\r\n" + dataFromClient.Message,
                                ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                ListAllRooms = _store.GetAllRooms(),
                                ListAllUsers = allUsers
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });

                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = ex.Message,
                            };
                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//block") && await ChechRoleAdminModeratorAsync(identityUser))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//block ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.AddToRoleAsync(identityUser2, "block");

                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = "Вы заблокировали пользователя " + nameUser2,
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            // TODO добавить оповещение заблокированного пользователя

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = ex.Message,
                            };
                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//unblock") && await ChechRoleAdminModeratorAsync(identityUser))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//unblock ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.RemoveFromRoleAsync(identityUser2, "block");

                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = "Вы разблокировали пользователя " + nameUser2,
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            // TODO добавить оповещение разблокированного пользователя

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });

                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = ex.Message,
                            };
                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//appoint moderator") && await _userManager.IsInRoleAsync(identityUser, "admin"))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//appoint moderator ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.AddToRoleAsync(identityUser2, "moderator");

                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = "Вы назначили модератором пользователя " + nameUser2,
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            // TODO добавить оповещение назначенного пользователя

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = ex.Message,
                            };
                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//disrank moderator") && await _userManager.IsInRoleAsync(identityUser, "admin"))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//disrank moderator ", "");
                            IdentityUser identityUser2 = await _userManager.FindByNameAsync(nameUser2);
                            await _userManager.RemoveFromRoleAsync(identityUser2, "moderator");

                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = "Вы разжаловали модератора " + nameUser2,
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            // TODO добавить оповещение назначенного пользователя

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = ex.Message,
                            };
                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                    }
                    // TODO этот блок может убрать вообще, а может дополнить...
                    else if (dataFromClient.Message.StartsWith("//si"))
                    {
                        IdentityUser identityUser2 = await _userManager.FindByNameAsync("admin@simbirsoft.com");

                        string id = identityUser2.Id;

                        if(_store.CheckAvailabilityRoom("SimbirSoft"))
                            _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "SimbirSoft", OwnerId = id });
                        if (_store.CheckAvailabilityRoom("Room1"))
                            _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "Room1", OwnerId = id });
                        if (_store.CheckAvailabilityRoom("Room2"))
                            _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = "Room2", OwnerId = id });

                        _store.appDbContext.SaveChanges();

                        string RoomId;
                        RoomId = _store.appDbContext.ChatRooms.Where(x => x.RoomName == "SimbirSoft").FirstOrDefault().RoomId;
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
                            ChatData dataFromServer;

                            if (!_store.CheckAvailabilityRoom(nameRoom))
                            {
                                _store.appDbContext.ChatRooms.Add(new ChatRoom() { RoomName = nameRoom, OwnerId = identityUser.Id });
                                _store.appDbContext.SaveChanges();

                                string RoomId = _store.appDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault().RoomId;
                                _store.appDbContext.ChatUsers.Add(new ChatUser() { ChatId = RoomId, UserId = identityUser.Id });
                                _store.appDbContext.SaveChanges();

                                var list = _userManager.Users;
                                string allUsers = "";
                                foreach (var item in list)
                                    allUsers += item.UserName + "\r\n";

                                dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы создали и вошли в комнату " + nameRoom + "\r\n",
                                    Room = nameRoom, // переход в комнату осуществляется по отправке имени комнаты
                                    ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                    ListAllRooms = _store.GetAllRooms(),
                                    // TODO Реализовать!!!
                                    ListMembers = "реализовать",
                                    ListAllUsers = allUsers
                                };
                            }
                            else
                            {
                                dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Такая комната " + nameRoom + " уже есть.\r\n",
                                };
                            }

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = ex.Message,
                            };
                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
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

                                // TODO так же надо удалить все записи из таблицы ChatUser с этой комнатой
                                // Продумать ситуацию, что будет с пользователями которые в данный момент в этой комнате.
                                // Что делать с сообщениями в таблице из этой комнаты?

                                ChatData dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы удалили комнату " + nameRoom
                                    + "\r\nДля продолжения общения войдите в доступную комнату."
                                };

                                await Clients.Caller.SendAsync("ReceiveData", dataFromServer);

                                AddMessage(new ChatMessage()
                                {
                                    SenderId = identityUser.Id,
                                    Text = dataFromServer.SystemMessage,
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = ex.Message,
                            };
                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//room enter "))
                    {
                        string nameRoom = dataFromClient.Message.Replace("//room enter ", "");

                        try
                        {

                            // TODO возможна проверка на владельца комнаты лишняя
                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(nameRoom) == identityUser.Id ||
                                _store.CheckUserMemberRoom(nameRoom, identityUser.Id))
                            {
                                var list = _userManager.Users;
                                string allUsers = "";
                                foreach (var item in list)
                                    allUsers += item.UserName + "\r\n";

                                ChatData dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы вошли в комнату " + nameRoom + "\r\n",
                                    Room = nameRoom, // переход в комнату осуществляется по отправке имени комнаты
                                    ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                    ListAllRooms = _store.GetAllRooms(),
                                    // TODO Реализовать!!!
                                    ListMembers = "реализовать",
                                    ListAllUsers = allUsers
                                };

                                await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            }
                        }
                        catch (Exception ex)
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = ex.Message,
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