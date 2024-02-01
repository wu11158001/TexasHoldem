using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasHoldemProtobuf;
using TexasHoldemServer.Servers;

namespace TexasHoldemServer.Controller
{
    class GameController : BaseController
    {
        public GameController()
        {
            requestCode = RequestCode.Game;
        }

        /// <summary>
        /// 更新房間玩家訊息
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack UpdateRoomUserInfo(Server server, Client client, MainPack pack)
        {
            client.GetRoom.UpdateRoomUserInfo(null, pack);
            return null;
        }

        public MainPack ExitRoom(Server server, Client client, MainPack pack)
        {
            return server.ExitRoom(client, pack);
        }
    }
}