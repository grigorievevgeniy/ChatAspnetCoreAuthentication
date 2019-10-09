using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication
{
    public class ApplicationStore
    {
        public ApplicationDbContext appDbContext;
        private UserManager<IdentityUser> _userManager;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IdentityUser identityUser;

        public ApplicationStore(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager)
        {
            appDbContext = applicationDbContext;
            _userManager = userManager;
        }

        //public ApplicationStore(ApplicationDbContext applicationDbContext)
        //{
        //    appDbContext = applicationDbContext;
        //}

        internal ChatData Start(ChatData dataFromClient)
        {
            try
            {
                IdentityUser user = _userManager.FindByEmailAsync(dataFromClient.User).Result;
                identityUser = _userManager.FindByNameAsync(dataFromClient.User).Result;
                //_userManager.Fi

                ChatData dataFromServer = new ChatData()
                {
                    User = dataFromClient.User,
                    SystemMessage = "Выберите доступную комнату, создайте новую или воспользуйтесь помошником команд //help.\r\n",
                    ListAvailableRooms = GetAllRoomsForUser(identityUser),
                    ListAllRooms = GetAllRooms(),
                    ListAllUsers = GetAllUsers()
                };

                AddMessage(new ChatMessage()
                {
                    SenderId = identityUser.Id,
                    Text = dataFromServer.SystemMessage,
                });

                //logger.Debug("Команда //start (вход в программу) от пользователя " + identityUser.UserName);

                return dataFromServer;
            }
            catch (Exception ex)
            {
                //logger.Debug("Команда //start (вход в программу) от пользователя " + identityUser.UserName);
                return new ChatData() { SystemMessage = ex.Message };
            }
        }

        internal void AddMessage(ChatMessage message)
        {
            appDbContext.ChatMessages.Add(message);
            appDbContext.SaveChanges();
        }

        internal void RemoveChatRoomAndChatUserByName(string nameRoom)
        {
            ChatRoom room = FindRoomByRoomName(nameRoom);
            List<ChatUser> chatUsers = appDbContext.ChatUsers.Where(x => x.ChatId == room.RoomId).ToList();

            foreach (var item in chatUsers)
            {
                appDbContext.ChatUsers.Remove(item);
            }

            appDbContext.ChatRooms.Remove(room);

            appDbContext.SaveChanges();
        }

        internal void RemoveChatRoomByName(string nameRoom)
        {
            ChatRoom room = FindRoomByRoomName(nameRoom);
            appDbContext.ChatRooms.Remove(room);
            appDbContext.SaveChanges();
        }

        internal ChatRoom FindRoomByRoomName(string nameRoom)
        {
            return appDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault();
        }

        internal string FindRoomIdByRoomName(string nameRoom)
        {
            ChatRoom room = appDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault();
            return room.RoomId;
        }

        internal string FindOwnerIdByRoomName(string nameRoom)
        {
            ChatRoom room = appDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault();
            return room.OwnerId;
        }

        internal bool CheckUserMemberRoom(string nameRoom, string UserId)
        {
            string RoomId = appDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault().ToString();

            ChatUser chatUser = appDbContext.ChatUsers.Where(x => x.UserId == UserId && x.ChatId == RoomId).FirstOrDefault();

            if (chatUser == null)
                return false;

            return true;
        }

        internal void RenameRoom(string oldName, string newName)
        {
            ChatRoom room = appDbContext.ChatRooms.Where(x => x.RoomName == oldName).FirstOrDefault();
            room.RoomName = newName;
            appDbContext.SaveChanges();
        }

        internal void RemoveRoomUser(string userId, string roomId)
        {
            ChatUser chatUser = appDbContext.ChatUsers.Where(x => x.ChatId == roomId && x.UserId == userId).FirstOrDefault();

            appDbContext.ChatUsers.Remove(chatUser);
            appDbContext.SaveChanges();

        }

        internal string GetAllUsers()
        {
            string allUsers = "";

            var list = _userManager.Users;
            foreach (var item in list)
                allUsers += item.UserName + "\r\n";

            return allUsers;
        }

        internal string GetAllRooms()
        {
            List<ChatRoom> chatRooms = new List<ChatRoom>();
            chatRooms = appDbContext.ChatRooms.ToList();

            string listNameRooms = "";

            for (int i = 0; i < chatRooms.Count; i++)
            {
                listNameRooms += chatRooms[i].RoomName + "\r\n";
            }

            return listNameRooms;
        }

        internal string GetAllRoomsForUser(IdentityUser identityUser)
        {
            List<ChatUser> chatUsers = new List<ChatUser>();
            chatUsers = appDbContext.ChatUsers.Where(x => x.UserId == identityUser.Id).ToList();

            string listNameRooms = "";

            for (int i = 0; i < chatUsers.Count; i++)
            {
                listNameRooms += appDbContext.ChatRooms.
                    Where(x => x.RoomId == chatUsers[i].ChatId).FirstOrDefault().RoomName 
                    + "\r\n";
            }

            return listNameRooms;
        }

        internal void AddRoomUser(string userId, string roomId)
        {
            appDbContext.ChatUsers.Add(new ChatUser() { ChatId = roomId, UserId = userId });
            appDbContext.SaveChanges();
        }

        internal bool CheckAvailabilityRoom(string nameRoom)
        {
            var r = appDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault();

            if (r == null)
                return false;

            return true;
        }

        internal string FindMessageContainsText(string partText)
        {
            var list = appDbContext.ChatMessages.Where(x => x.Text.Contains(partText)).ToList();
            string allMessages = "";
            foreach (var item in list)
            {
                IdentityUser user = _userManager.FindByIdAsync(item.SenderId).Result;
                allMessages = "User:" + user.UserName + "\r\n" 
                    + "DateTime:" + item.Time + "\r\n"
                    + "Text:" + item.Text + "\r\n\r\n" + allMessages;
            }
            return allMessages;
        }

        internal string GetAllRoomUsers(string nameRoom)
        {
            string roomId = FindRoomIdByRoomName(nameRoom);
            string roomUsers = "";

            var chatUsers = appDbContext.ChatUsers.Where(x => x.ChatId == roomId);

            foreach (var item in chatUsers)
            {
                IdentityUser identityUser3 = _userManager.FindByIdAsync(item.UserId).Result;
                roomUsers = identityUser3.UserName + "\r\n" + roomUsers;
            }

            return roomUsers;
        }
    }
}
