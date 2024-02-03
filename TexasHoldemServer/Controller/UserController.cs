using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasHoldemProtobuf;
using TexasHoldemServer.Servers;

namespace TexasHoldemServer.Controller
{
    class UserController : BaseController
    {
        private string[] userDataNames = { "account", "password", "cash", "nickname", "avatar" };
        string tableName = "userdata";

        public UserController()
        {
            requestCode = RequestCode.User;
        }

        /// <summary>
        /// 設定用戶訊息
        /// </summary>
        private void SetUserInfo(Client client, MainPack pack)
        {
            client.UserInfo.Account = pack.LoginPack.Account;
            client.UserInfo.Password = pack.LoginPack.Password;
            client.UserInfo.NickName = pack.UserInfoPack[0].NickName;
            client.UserInfo.Avatar = pack.UserInfoPack[0].Avatar;
            client.UserInfo.Cash = pack.UserInfoPack[0].Cash;
        }

        /// <summary>
        /// 獲取用戶訊息
        /// </summary>
        /// <param name="servers"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack GetUserInfo(Server servers, Client client, MainPack pack)
        {
            UserInfoPack userInfoPack = new UserInfoPack();
            userInfoPack.NickName = client.UserInfo.NickName;
            userInfoPack.Avatar = client.UserInfo.Avatar;
            userInfoPack.Cash = client.UserInfo.Cash;

            pack.UserInfoPack.Add(userInfoPack);

            return pack;
        }

        /// <summary>
        /// 註冊
        /// </summary>
        /// <returns></returns>
        public MainPack Logon(Server servers, Client client, MainPack pack)
        {
            if (client.GetMySql.CheckData(client.GetMySqlConnection, tableName, new string[] { "account" }, new string[] { pack.LoginPack.Account }))
            {
                pack.ReturnCode = ReturnCode.Duplicated;
                return pack;
            }
            else
            {
                string initCash = "100000";
                string[] InsertNames = new string[] { "account", "password", "cash", "nickname", "avatar" };
                string[] values = new string[]
                {
                pack.LoginPack.Account,     //帳號
                pack.LoginPack.Password,    //密碼
                initCash,                   //初始金幣
                pack.LoginPack.Account,     //暱稱
                "0",                        //初始頭像
                };

                if (client.GetMySql.InsertData(client.GetMySqlConnection, tableName, userDataNames, values))
                {
                    UserInfoPack userInfoPack = new UserInfoPack();
                    userInfoPack.NickName = pack.LoginPack.Account;
                    userInfoPack.Avatar = "0";
                    userInfoPack.Cash = initCash;
                    pack.UserInfoPack.Add(userInfoPack);

                    pack.ReturnCode = ReturnCode.Succeed;
                    SetUserInfo(client, pack);

                    Console.WriteLine($"{pack.LoginPack.Account}: 註冊成功!");
                }
                else
                {
                    pack.ReturnCode = ReturnCode.Fail;
                    Console.WriteLine($"{pack.LoginPack.Account}: 註冊失敗!!!");
                }

                return pack;
            }
        }

        /// <summary>
        /// 登入
        /// </summary>
        /// <returns></returns>
        public MainPack Login(Server server, Client client, MainPack pack)
        {
            string[] searchNames = new string[] { "account", "password" };
            string[] values = new string[] { pack.LoginPack.Account, pack.LoginPack.Password };
            if (server.GetClientList.Any(list => list.UserInfo.Account == pack.LoginPack.Account))
            {
                pack.ReturnCode = ReturnCode.Duplicated;
                Console.WriteLine(pack.LoginPack.Account + ": 重複登入!!!");
                return pack;
            }

            if (client.GetMySql.CheckData(client.GetMySqlConnection, tableName, searchNames, values))
            {
                Dictionary<string, string> dataDic = client.GetMySql.GetData(client.GetMySqlConnection, tableName, "account", pack.LoginPack.Account, userDataNames);

                UserInfoPack userInfoPack = new UserInfoPack();
                userInfoPack.NickName = dataDic["nickname"];
                userInfoPack.Avatar = dataDic["avatar"];
                userInfoPack.Cash = dataDic["cash"];
                pack.UserInfoPack.Add(userInfoPack);
                pack.ReturnCode = ReturnCode.Succeed;

                SetUserInfo(client, pack);
                Console.WriteLine($"{pack.LoginPack.Account}: 登入!");
            }
            else
            {
                Console.WriteLine($"{pack.LoginPack.Account} : 登入失敗!");
                pack.ReturnCode = ReturnCode.Fail;
            }

            return pack;
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public MainPack Logout(Server server, Client client, MainPack pack)
        {
            Console.WriteLine(client.UserInfo.Account + ": 用戶登出");
            server.RemoveClient(client);            

            pack.ReturnCode = ReturnCode.Succeed;
            return pack;
        }

        /// <summary>
        /// 修改暱稱
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack ReviseNickName(Server server, Client client, MainPack pack)
        {
            string[] serchNames = { "nickName" };
            string[] reviseValue = { pack.UserInfoPack[0].NickName };
            if (client.GetMySql.CheckData(client.GetMySqlConnection, tableName, serchNames, reviseValue))
            {
                pack.ReturnCode = ReturnCode.Duplicated;
                return pack;
            }
            else
            {
                bool result = client.GetMySql.ReviseData(client.GetMySqlConnection,
                                                     tableName,
                                                     "account",
                                                     client.UserInfo.Account,
                                                     serchNames,
                                                     reviseValue
                                                     );

                pack.ReturnCode = result == true ? ReturnCode.Succeed : ReturnCode.Fail;
                return pack;
            }            
        }
    }
}
