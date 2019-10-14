using ChatAspnetCoreAuthentication.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatAspnetCoreAuthentication.Data
{
    public class CommandStore
    {
        ApplicationStore appStore;
        ChatData dataFromClient;
        IdentityUser identityUser;

        public CommandStore(ApplicationStore appStore)
        {
            this.appStore = appStore;
        }

        public ChatData Command(ChatData dataFromClient)
        {
            this.dataFromClient = dataFromClient;
            identityUser = appStore._userManager.FindByNameAsync(dataFromClient.User).Result;


            if (dataFromClient.Message.StartsWith("//start"))
            {
                return Start();
            }
            else if (dataFromClient.Message.StartsWith("//block"))
            {
                return Block();
            }
            else if (dataFromClient.Message.StartsWith("//unblock"))
            {
            }
            else if (dataFromClient.Message.StartsWith("//appoint moderator"))
            {
            }
            else if (dataFromClient.Message.StartsWith("//disrank moderator"))
            {
            }
            // TODO этот блок может убрать вообще, а может дополнить...
            else if (dataFromClient.Message.StartsWith("//si"))
            {
            }
            else if (dataFromClient.Message.StartsWith("//room create "))
            {
            }
            else if (dataFromClient.Message.StartsWith("//room remove "))
            {
            }
            else if (dataFromClient.Message.StartsWith("//room enter "))
            {
            }
            // TODO для переименовки комнаты, надо в ней находиться
            else if (dataFromClient.Message.StartsWith("//room rename "))
            {
            }
            // TODO допуск к команде общий, хотя это и протеворечит общей логике
            else if (dataFromClient.Message.StartsWith("//room connect "))
            {
            }
            else if (dataFromClient.Message.StartsWith("//room disconnect "))
            {
            }
            else if (dataFromClient.Message.StartsWith("//user kick off "))
            {
            }
            else if (dataFromClient.Message.StartsWith("//user welcome "))
            {
            }
            else if (dataFromClient.Message.StartsWith("//find message "))
            {
            }



            return null;
        }

        private ChatData Start()
        {
            ChatData dataFromServer;

            try
            {
                dataFromServer = new ChatData()
                {
                    User = dataFromClient.User,
                    SystemMessage = "Выберите доступную комнату, создайте новую или воспользуйтесь помошником команд \r\n//help.",
                    ListAvailableRooms = appStore.GetAllRoomsForUser(identityUser),
                    ListAllRooms = appStore.GetAllRooms(),
                    ListAllUsers = appStore.GetAllUsers()
                };

                appStore.AddMessage(new ChatMessage()
                {
                    SenderId = identityUser.Id,
                    Text = dataFromServer.SystemMessage,
                });
            }
            catch (Exception ex)
            {
                dataFromServer = new ChatData() { SystemMessage = ex.Message };
            }

            return dataFromServer;
        }

        private ChatData Block()
        {
            try
            {
                string blockUser = dataFromClient.Message.Replace("//block ", "");
                IdentityUser identityBlockUser = appStore._userManager.FindByNameAsync(blockUser).Result;
                appStore._userManager.AddToRoleAsync(identityBlockUser, "block");

                ChatData dataFromServer = new ChatData()
                {
                    SystemMessage = "Вы заблокировали пользователя " + blockUser,
                };

                //foreach (var item in connectionMapping.GetConnections(nameUser2))
                //    await Clients.Client(item).SendAsync("ReceiveData",
                //        new ChatData() { SystemMessage = "Вас заблокировал пользователь " + dataFromClient.User });

                appStore.AddMessage(new ChatMessage()
                {
                    SenderId = identityUser.Id,
                    Text = dataFromServer.SystemMessage,
                });
            }
            catch (Exception ex)
            {
                new ChatData()
                {
                    SystemMessage =
                    "Ошибка. Нет такого пользователя или у Вас не достаточно прав.\r\n" +
                    ex.Message
                };
            }

            return null;
        }
    }
}
