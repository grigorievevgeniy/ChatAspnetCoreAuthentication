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
        List<ResponseByCommandStore> listResponse = new List<ResponseByCommandStore>();

        public CommandStore(ApplicationStore appStore)
        {
            this.appStore = appStore;
        }

        public List<ResponseByCommandStore> Command(ChatData dataFromClient)
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

            return listResponse;
        }

        private List<ResponseByCommandStore> Start()
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

            listResponse.Add(new ResponseByCommandStore("Caller", dataFromServer));

            return listResponse;
        }

        private List<ResponseByCommandStore> Block()
        {
            ChatData dataFromServer;
            string blockUser = "";
            IdentityUser identityBlockUser;

            //Проверка роли
            if (!appStore._userManager.IsInRoleAsync(identityUser, "admin").Result &&
                !appStore._userManager.IsInRoleAsync(identityUser, "moderator").Result)
            {
                listResponse.Add(new ResponseByCommandStore("Caller", new ChatData() { SystemMessage = "У Вас не достаточно прав, " +
                    "для блокировки пользователей необходимо иметь статус администратора или модератора."}));

                return listResponse;
            }

            try
            {
                blockUser = dataFromClient.Message.Replace("//block ", "");
                identityBlockUser = appStore._userManager.FindByNameAsync(blockUser).Result;
                if (identityBlockUser == null)
                {
                    listResponse.Add(new ResponseByCommandStore("Caller", new ChatData()
                    {
                        SystemMessage = "Пользователь с таким именем не найден"
                    }));

                    return listResponse;
                }
                appStore._userManager.AddToRoleAsync(identityBlockUser, "block");

                dataFromServer = new ChatData()
                {
                    SystemMessage = "Вы заблокировали пользователя " + blockUser,
                };

                appStore.AddMessage(new ChatMessage()
                {
                    SenderId = identityUser.Id,
                    Text = dataFromServer.SystemMessage,
                });
            }
            catch (Exception ex)
            {
                dataFromServer = new ChatData()
                {
                    SystemMessage =
                    "Ошибка. Нет такого пользователя, у Вас не достаточно прав или системная ошибка\r\n" +
                    ex.Message
                };
            }
            // Сообщение для инициатора команды
            listResponse.Add(new ResponseByCommandStore("Caller", dataFromServer));
            // Сообщение для заблокированного пользователя
            listResponse.Add(new ResponseByCommandStore("User", blockUser, new ChatData() { SystemMessage = "Вас заблокировал пользователь " + dataFromClient.User }));

            return listResponse;
        }
    }
}
