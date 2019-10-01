using ChatAspnetCoreAuthentication.Data;
using ChatAspnetCoreAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication
{
    public class ApplicationStore
    {
        public ApplicationDbContext appDbContext;

        public ApplicationStore(ApplicationDbContext applicationDbContext)
        {
            appDbContext = applicationDbContext;
        }

        internal void AddMessage(ChatMessage message)
        {
            appDbContext.ChatMessages.Add(message);
            appDbContext.SaveChanges();
        }

        internal void RemoveChatRoomsByName(string nameRoom)
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

        internal void AddRoomUser(string userId, string roomId)
        {
            appDbContext.ChatUsers.Add(new ChatUser() { ChatId = roomId, UserId = userId });
            appDbContext.SaveChanges();
        }
    }
}
