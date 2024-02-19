using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasHoldemProtobuf;
using TexasHoldemServer.Servers;
using TexasHoldemServer.Tools;

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
            string multiple = "400";

            return Utils.StringMultiplication(Bigblind, multiple);
        }

        /// <summary>
        /// 判斷用戶金幣是否足夠
        /// </summary>
        /// <param name="client"></param>
        /// <param name="initChips"></param>
        /// <returns></returns>
        private bool JudgeUserCashIsEnough(Client client, string initChips)
        {
            Dictionary<string, string> dataDic = client.GetMySql.GetData(client.GetMySqlConnection,
                                                            "userdata",
                                                            "account",
                                                            client.UserInfo.Account,
                                                            new string[] { "cash" });

            if (Convert.ToInt64(dataDic["cash"]) < Convert.ToInt64(initChips))
            {
                return false;
            }

            return true;
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
        /// 創建房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack CreateRoom(Server server, Client client, MainPack pack)
        {
            string bigBlind = pack.RoomPack[0].RoomBigBlind;
            string initChips = GetInitChips(client, bigBlind);

            if (!JudgeUserCashIsEnough(client, initChips))
            {
                pack.ReturnCode = ReturnCode.Fail;
                return pack;
            }

            return server.CreateRoom(client, pack, initChips, bigBlind);
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

            if (!JudgeUserCashIsEnough(client, initChips))
            {
                pack.ReturnCode = ReturnCode.Fail;
                return pack;
            }

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
