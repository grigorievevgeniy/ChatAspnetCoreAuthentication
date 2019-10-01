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
        public ApplicationDbContext _applicationDbContext;

        public ApplicationStore(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        internal void AddMessage(ChatMessage message)
        {
            _applicationDbContext.ChatMessages.Add(message);
            _applicationDbContext.SaveChanges();
        }

        internal void RemoveChatRoomsByName(string nameRoom)
        {
            ChatRoom room = FindRoomByRoomName(nameRoom);
            _applicationDbContext.ChatRooms.Remove(room);
        }

        internal ChatRoom FindRoomByRoomName(string nameRoom)
        {
            return _applicationDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault();
        }

        internal string FindRoomIdByRoomName(string nameRoom)
        {
            ChatRoom room = _applicationDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault();
            return room.RoomId;
        }

        internal string FindOwnerIdByRoomName(string nameRoom)
        {
            ChatRoom room = _applicationDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault();
            return room.OwnerId;
        }

        internal bool CheckUserMemberRoom(string nameRoom, string UserId)
        {
            string RoomId = _applicationDbContext.ChatRooms.Where(x => x.RoomName == nameRoom).FirstOrDefault().ToString();

            ChatUser chatUser = _applicationDbContext.ChatUsers.Where(x => x.UserId == UserId && x.ChatId == RoomId).FirstOrDefault();

            if (chatUser == null)
                return false;

            return true;
        }
    }
}
