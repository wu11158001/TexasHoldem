using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using TexasHoldemProtobuf;

namespace TexasHoldemServer.Servers
{
    class Room
    {
        private Server server;

        //房間內所有客戶端
        private List<Client> clientList = new List<Client>();

        //房間訊息
        private RoomPack roomInfo;
        public RoomPack GetRoomInfo
        {
            get
            {
                roomInfo.CurrCount = clientList.Count;
                return roomInfo;
            }
        }

        public Room(Server server, Client client, RoomPack pack)
        {
            this.server = server;
            roomInfo = pack;
            clientList.Add(client);
            client.GetRoom = this;
        }

        /// <summary>
        /// 獲取房間玩家訊息
        /// </summary>
        /// <returns></returns>
        public RepeatedField<RoomUserInfoPack> GetRoomUserInfo()
        {
            RepeatedField<RoomUserInfoPack> pack = new RepeatedField<RoomUserInfoPack>();
            foreach (Client c in clientList)
            {
                RoomUserInfoPack roomUserInfo = new RoomUserInfoPack();
                roomUserInfo.AccountName = c.UserInfo.Account;
                pack.Add(roomUserInfo);
            }

            return pack;
        }

        /// <summary>
        /// 廣播
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public void Broadcast(Client client, MainPack pack)
        {
            foreach (Client c in clientList)
            {
                //排除的cliemt
                if (c.Equals(client)) continue;

                c.Send(pack);
            }
        }

        /// <summary>
        /// 添加客戶端
        /// </summary>
        /// <param name="client"></param>
        public void Join(Client client)
        {
            clientList.Add(client);

            client.GetRoom = this;

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.UpdateRoomUserInfo;

            //賦值
            foreach (RoomUserInfoPack user in GetRoomUserInfo())
            {
                pack.RoomUserInfoPack.Add(user);
            }

            Broadcast(client, pack);
        }

        /// <summary>
        /// 更新房間玩家訊息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        public void UpdateRoomUserInfo(Client client, MainPack pack)
        {
            //賦值
            foreach (RoomUserInfoPack user in GetRoomUserInfo())
            {
                pack.RoomUserInfoPack.Add(user);
            }

            Broadcast(client, pack);
        }

        /// <summary>
        /// 離開房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        public void Exit(Server server, Client client)
        {
            MainPack pack = new MainPack();

            //房間只剩1人關閉房間
            if (clientList.Count == 1)
            {
                client.GetRoom = null;
                server.RemoveRoom(this);
                return;
            }

            clientList.Remove(client);
            client.GetRoom = null;
            pack.ActionCode = ActionCode.UpdateRoomUserInfo;

            //賦值
            foreach (RoomUserInfoPack user in GetRoomUserInfo())
            {
                pack.RoomUserInfoPack.Add(user);
            }

            Broadcast(client, pack);
        }
    }
}
