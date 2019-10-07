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
        private readonly static ConnectionMapping<string> connectionMapping = new ConnectionMapping<string>();

        static List<UserConnection> uList = new List<UserConnection>();

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

                    await Clients.Group(dataFromClient.Room).SendAsync("ReceiveData", dataFromServer);

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
                    if (dataFromClient.Message.StartsWith("//start"))
                    {
                        try
                        {
                            ChatData dataFromServer = new ChatData()
                            {
                                User = dataFromClient.User,
                                SystemMessage = "Выберите доступную комнату или создайте новую.\r\n" + dataFromClient.Message,
                                ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                ListAllRooms = _store.GetAllRooms(),
                                ListAllUsers = _store.GetAllUsers()
                            };

                            //TODO Конечно этот метод должен исполняться во время коннекта
                            connectionMapping.Add(Context.User.Identity.Name, Context.ConnectionId);

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

                            foreach (var item in connectionMapping.GetConnections(nameUser2))
                                await Clients.Client(item).SendAsync("ReceiveData", 
                                    new ChatData() { SystemMessage = "Вас заблокировал пользователь " + dataFromClient.User });

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

                            foreach (var item in connectionMapping.GetConnections(nameUser2))
                                await Clients.Client(item).SendAsync("ReceiveData",
                                    new ChatData() { SystemMessage = "Вас разблокировал пользователь " + dataFromClient.User });

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });

                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

                            foreach (var item in connectionMapping.GetConnections(nameUser2))
                                await Clients.Client(item).SendAsync("ReceiveData",
                                    new ChatData() { SystemMessage = "Вас назначил модератором " + dataFromClient.User });

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

                            foreach (var item in connectionMapping.GetConnections(nameUser2))
                                await Clients.Client(item).SendAsync("ReceiveData",
                                    new ChatData() { SystemMessage = "Вас разжаловал из модераторов " + dataFromClient.User });

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

                                await Groups.AddToGroupAsync(Context.ConnectionId, nameRoom);
                                await Groups.RemoveFromGroupAsync(Context.ConnectionId, dataFromClient.Room);

                                dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы создали и вошли в комнату " + nameRoom + "\r\n",
                                    Room = nameRoom, // переход в комнату осуществляется по отправке имени комнаты
                                    ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                    ListAllRooms = _store.GetAllRooms(),
                                    // TODO Реализовать!!!
                                    ListMembers = "реализовать",
                                    ListAllUsers = _store.GetAllUsers()
                                };
                            }
                            else
                            {
                                dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Такая комната " + nameRoom + " уже есть.\r\n",
                                };
                            }

                            //await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            await Clients.Group(nameRoom).SendAsync("ReceiveData", dataFromServer);

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

                                //await Groups.AddToGroupAsync(Context.ConnectionId, nameRoom);
                                //await Groups.RemoveFromGroupAsync(Context.ConnectionId, dataFromClient.Room);

                                ChatData dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы удалили комнату " + nameRoom
                                    + "\r\nДля продолжения общения войдите в доступную комнату.",
                                    ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                    ListAllRooms = _store.GetAllRooms()
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
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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
                                await Groups.AddToGroupAsync(Context.ConnectionId, nameRoom);
                                await Groups.RemoveFromGroupAsync(Context.ConnectionId, dataFromClient.Room);

                                ChatData dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы вошли в комнату " + nameRoom + "\r\n",
                                    Room = nameRoom, // переход в комнату осуществляется по отправке имени комнаты
                                    ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                    ListAllRooms = _store.GetAllRooms(),
                                    // TODO Реализовать!!!
                                    ListMembers = "реализовать",
                                    ListAllUsers = _store.GetAllUsers()
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
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
                        }
                    }
                    // TODO для переименовки комнаты, надо в ней находиться
                    else if (dataFromClient.Message.StartsWith("//room rename "))
                    {
                        try
                        {
                            string newNameRoom = dataFromClient.Message.Replace("//room rename ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(dataFromClient.Room) == identityUser.Id)
                            {
                                _store.RenameRoom(dataFromClient.Room, newNameRoom);

                                //TODO интересно что будет если группу оставить со старым названием...
                                // по сути SignalR должно быть все равно как группа назвывается
                                //await Groups.AddToGroupAsync(Context.ConnectionId, newNameRoom);
                                //await Groups.RemoveFromGroupAsync(Context.ConnectionId, dataFromClient.Room);

                                var list = _userManager.Users;
                                string allUsers = "";
                                foreach (var item in list)
                                    allUsers += item.UserName + "\r\n";

                                ChatData dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы переименовали комнату в " + newNameRoom,
                                    Room = newNameRoom, // переход в комнату осуществляется по отправке имени комнаты
                                    ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                    ListAllRooms = _store.GetAllRooms()
                                };

                                await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                                // TODO у других пользователей тоже надо обновить списки комнат

                                AddMessage(new ChatMessage()
                                {
                                    SenderId = identityUser.Id,
                                    Text = dataFromServer.SystemMessage,
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

                            await Groups.AddToGroupAsync(Context.ConnectionId, nameRoom);
                            await Groups.RemoveFromGroupAsync(Context.ConnectionId, dataFromClient.Room);

                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = "Вы присоеденились и вошли в комнату " + nameRoom,
                                Room = nameRoom, // переход в комнату осуществляется по отправке имени комнаты
                                ListAvailableRooms = _store.GetAllRoomsForUser(identityUser),
                                // TODO
                                ListMembers = "Реализовать"
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            // TODO у других пользователей комнаты тоже надо обновить список участников

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

                            await Groups.RemoveFromGroupAsync(Context.ConnectionId, nameRoom);

                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = "Вы покинули (насовсем) комнату " + nameRoom
                                + "\r\nДля продожения общения войдите в доступную комнату.",
                                //Room = nameRoom, // TODO как сказать "клиенту" что он вне комнат
                                ListAvailableRooms = _store.GetAllRoomsForUser(identityUser)
                            };

                            await Clients.Caller.SendAsync("ReceiveData", dataFromServer);
                            // TODO у других пользователей комнаты тоже надо обновить список участников

                            AddMessage(new ChatMessage()
                            {
                                SenderId = identityUser.Id,
                                Text = dataFromServer.SystemMessage,
                            });
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//user kick off "))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//user kick off ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(dataFromClient.Room) == identityUser.Id)
                            {
                                string userId = _userManager.FindByNameAsync(nameUser2).Id.ToString();
                                string roomId = _store.FindRoomIdByRoomName(dataFromClient.Room);

                                _store.RemoveRoomUser(userId, roomId);

                                ChatData dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы удалили из комнаты пользователя " + nameUser2,
                                    // TODO
                                    ListMembers = "Реализовать"
                                };

                                await Clients.Caller.SendAsync("ReceiveData", dataFromServer);

                                foreach (var item in connectionMapping.GetConnections(nameUser2))
                                {
                                    await Clients.Client(item).SendAsync("ReceiveData",
                                        new ChatData() { SystemMessage = "Вас выгнали из комнаты " + dataFromClient.Room });
                                    await Groups.RemoveFromGroupAsync(item, dataFromClient.Room);
                                }

                                AddMessage(new ChatMessage()
                                {
                                    SenderId = identityUser.Id,
                                    Text = dataFromServer.SystemMessage,
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//user welcome "))
                    {
                        try
                        {
                            string nameUser2 = dataFromClient.Message.Replace("//user welcome ", "");

                            if (await _userManager.IsInRoleAsync(identityUser, "admin") ||
                                _store.FindOwnerIdByRoomName(dataFromClient.Room) == identityUser.Id)
                            {
                                string userId = _userManager.FindByNameAsync(dataFromClient.User).Id.ToString();
                                string roomId = _store.FindRoomIdByRoomName(dataFromClient.Room);

                                _store.AddRoomUser(userId, roomId);

                                ChatData dataFromServer = new ChatData()
                                {
                                    SystemMessage = "Вы пригласили в комнату пользователя " + nameUser2,
                                    // TODO
                                    ListMembers = "Реализовать"
                                };

                                await Clients.Caller.SendAsync("ReceiveData", dataFromServer);

                                foreach (var item in connectionMapping.GetConnections(nameUser2))
                                {
                                    await Clients.Client(item).SendAsync("ReceiveData",
                                        new ChatData() { SystemMessage = "Вас пригласили в комнату " + dataFromClient.Room });
                                    await Groups.RemoveFromGroupAsync(item, dataFromClient.Room);
                                }

                                AddMessage(new ChatMessage()
                                {
                                    SenderId = identityUser.Id,
                                    Text = dataFromServer.SystemMessage,
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
                        }
                    }
                    else if (dataFromClient.Message.StartsWith("//find message "))
                    {
                        try
                        {
                            string partText = dataFromClient.Message.Replace("//find message ", "");

                            ChatData dataFromServer = new ChatData()
                            {
                                SystemMessage = _store.FindMessageContainsText(partText)
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
                            await Clients.Caller.SendAsync("ReceiveData", new ChatData() { SystemMessage = ex.Message });
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

        public void SeedSignalRGroup()
        {
            List<Room> groups = new List<Room>();

            foreach (var item in _store.appDbContext.ChatRooms)
            {
                groups.Add(new Room() { Name = item.RoomName, Id = item.RoomId });
            }

            foreach (var item in _store.appDbContext.ChatUsers)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    if (item.ChatId == groups[i].Id)
                    {
                        try
                        {
                            Groups.AddToGroupAsync(item.UserId, groups[i].Name);

                        }
                        catch (Exception ex)
                        {

                            string aa = ex.Message;
                        }

                        break;
                    }
                }
            }
        }

        class Room
        {
            public string Name { get; set; }
            public string Id { get; set; }
        }

        class UserConnection
        {
            public string UserName { set; get; }
            public string ConnectionID { set; get; }
        }
    }
}