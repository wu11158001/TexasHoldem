using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasHoldemProtobuf;
using TexasHoldemServer.Servers;

namespace TexasHoldemServer.Controller
{
    class RoomController : BaseController
    {
        public RoomController()
        {
            requestCode = RequestCode.Room;
        }

        /// <summary>
        /// 獲取初始籌碼
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Bigblind"></param>
        /// <returns></returns>
        private string GetInitChips(Client client, string Bigblind)
        {
            Dictionary<string, string> dataDic = client.GetMySql.GetData(client.GetMySqlConnection, "rule", "bigblind", Bigblind, new string[] { "initchips" });
            return dataDic["initchips"];
        }

        /// <summary>
        /// 刷新房間列表
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack UpdateRoom(Server server, Client client, MainPack pack)
        {
            return server.UpdateRoom(client, pack);
        }

        /// <summary>
        /// 快速開局
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack QuickJoinRoom(Server server, Client client, MainPack pack)
        {
            string initChips = GetInitChips(client, "50");

            return server.QuickJoinRoom(client, pack, initChips, "50");
        }

        /// <summary>
        /// 加入房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack JoinRoom(Server server, Client client, MainPack pack)
        {
            string initChips = GetInitChips(client, "50");

            return server.JoinRoom(client, pack, initChips);
        }
    }
}
