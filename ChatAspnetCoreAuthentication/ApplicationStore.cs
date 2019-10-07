using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Identity;
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

        public ApplicationStore(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager)
        {
            appDbContext = applicationDbContext;
            _userManager = userManager;
        }

        public ApplicationStore(ApplicationDbContext applicationDbContext)
        {
            appDbContext = applicationDbContext;
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
