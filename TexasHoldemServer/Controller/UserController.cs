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
            client.UserInfo.Avatar = pack.LoginPack.Avatar;
        }

        /// <summary>
        /// 註冊
        /// </summary>
        /// <returns></returns>
        public MainPack Logon(Server servers, Client client, MainPack pack)
        {
            string tableName = "userdata";
            string[] InsertNames = new string[] { "account", "password", "usercash" };
            string[] values = new string[] { pack.LoginPack.Account, pack.LoginPack.Password, "10000" };

            if (client.GetMySql.InsertData(client.GetMySqlConnection, tableName, InsertNames, values))
            {
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

        /// <summary>
        /// 登入
        /// </summary>
        /// <returns></returns>
        public MainPack Login(Server server, Client client, MainPack pack)
        {
            string tableName = "userdata";
            string[] searchNames = new string[] { "account", "password" };
            string[] values = new string[] { pack.LoginPack.Account, pack.LoginPack.Password };

            if (server.GetClientList.Any(list => list.UserInfo.Account == pack.LoginPack.Account))
            {
                pack.ReturnCode = ReturnCode.DuplicateLogin;
                Console.WriteLine(pack.LoginPack.Account + ": 重複登入!!!");
                return pack;
            }

            if (client.GetMySql.SearchData(client.GetMySqlConnection, tableName, searchNames, values))
            {
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
            return null;
        }
    }
}
