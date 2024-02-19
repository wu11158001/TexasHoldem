using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using TexasHoldemServer.Controller;
using TexasHoldemProtobuf;
using TexasHoldemServer.SqlData;
using MySql.Data.MySqlClient;

namespace TexasHoldemServer.Servers
{
    class Server
    {
        private Socket socket = null;

        //存放所有連接的客戶端
        private List<Client> clientList = new List<Client>();
        public List<Client> GetClientList { get { return clientList; } }

        private ControllerManager controllerManager;

        //存放所有房間
        private List<Room> roomList = new List<Room>();
        public int GetRoomCount { get { return roomList.Count; } }
        private int roomName;

        public Server(int port)
        {
            controllerManager = new ControllerManager(this);

            //Socket初始化
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //綁定
            socket.Bind(new IPEndPoint(IPAddress.Any, port));
            //監聽
            socket.Listen(0);
            //開始接收
            StartAccect();
        }

        /// <summary>
        /// 開始接收
        /// </summary>
        void StartAccect()
        {
            socket.BeginAccept(AccectCallBack, null);
        }
        /// <summary>
        /// 接收CallBack
        /// </summary>
        void AccectCallBack(IAsyncResult iar)
        {
            Socket client = socket.EndAccept(iar);
            clientList.Add(new Client(client, this));
            StartAccect();
        }

        /// <summary>
        /// 處理請求
        /// </summary>
        /// <param name="pack"></param>
        /// <param name="client"></param>
        public void HandleRequest(MainPack pack, Client client)
        {
            controllerManager.HandleRequest(pack, client);
        }

        /// <summary>
        /// 移除客戶端
        /// </summary>
        /// <param name="client"></param>
        public void RemoveClient(Client client)
        {
            clientList.Remove(client);
        }

        /// <summary>
        /// 進入遊戲初始數值
        /// </summary>
        /// <param name="client"></param>
        /// <param name="initChips"></param>
        /// <param name="srat"></param>
        private void InitGameInfo(Client client, string initChips, int srat)
        {
            client.UserInfo.Chips = initChips;
            client.UserInfo.GameSeat = srat;
            client.UserInfo.handPoker = new int[2];
            client.UserInfo.BetChips = "0";
            client.UserInfo.GameState = UserGameState.StateNone;
        }

        /// <summary>
        /// 快速開局
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <param name="initChips"></param>
        /// <param name="bigBlindValue"></param>
        /// <returns></returns>
        public MainPack QuickJoinRoom(Client client, MainPack pack, string initChips, string bigBlindValue)
        {
            foreach (Room r in roomList)
            {
                if (r.GetRoomInfo.CurrCount < r.GetRoomInfo.MaxCount)
                {
                    InitGameInfo(client, initChips, r.GetSeatInfo());
                    r.Join(client);
                    pack.RoomPack.Add(r.GetRoomInfo);

                    foreach (UserInfoPack p in r.GetRoomUserInfo())
                    {
                        pack.UserInfoPack.Add(p);
                    }

                    ComputerPack computerPack = new ComputerPack();
                    computerPack.NickName = r.ComputerInfo.NickName;
                    computerPack.Avatar = r.ComputerInfo.Avatar;
                    computerPack.Chips = r.ComputerInfo.Chips;
                    pack.ComputerPack = computerPack;

                    pack.ReturnCode = ReturnCode.Succeed;
                    return pack;
                }
            }

            //沒有找到房間創建房間
            return CreateRoom(client, pack, initChips, bigBlindValue);
        }

        /// <summary>
        /// 創建房間
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <param name="initChips"></param>
        /// <param name="bigBlindValue"></param>
        /// <returns></returns>
        public MainPack CreateRoom(Client client, MainPack pack, string initChips, string bigBlindValue)
        {
            try
            {
                RoomPack roomPack = new RoomPack();
                roomPack.RoomName = (roomName++).ToString();
                roomPack.MaxCount = 4;
                pack.RoomPack.Add(roomPack);

                Room room = new Room(this, client, roomPack, initChips, bigBlindValue);
                roomList.Add(room);

                InitGameInfo(client, initChips, 0);

                foreach (UserInfoPack p in room.GetRoomUserInfo())
                {
                    pack.UserInfoPack.Add(p);
                }

                ComputerPack computerPack = new ComputerPack();
                computerPack.NickName = room.ComputerInfo.NickName;
                computerPack.Avatar = room.ComputerInfo.Avatar;
                computerPack.Chips = room.ComputerInfo.Chips;
                pack.ComputerPack = computerPack;

                pack.ReturnCode = ReturnCode.Succeed;
                return pack;
            }
            catch (Exception)
            {
                pack.ReturnCode = ReturnCode.Fail;
                return pack;
            }
        }

        /// <summary>
        /// 刷新房間
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack UpdateRoom(Client client, MainPack pack)
        {
            try
            {                
                foreach (Room room in roomList)
                {
                    pack.RoomPack.Add(room.GetRoomInfo);
                }

                pack.ReturnCode = ReturnCode.Succeed;
            }
            catch (Exception)
            {
                pack.ReturnCode = ReturnCode.Fail;
            }

            return pack;
        }

        /// <summary>
        /// 加入房間
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <param name="initChips"></param>
        /// <returns></returns>
        public MainPack JoinRoom(Client client, MainPack pack, string initChips)
        {
            foreach (Room r in roomList)
            {
                if (r.GetRoomInfo.RoomName.Equals(pack.RoomPack[0].RoomName))
                {
                    if (r.GetRoomInfo.CurrCount < r.GetRoomInfo.MaxCount)
                    {
                        //可以加入房間
                        InitGameInfo(client, initChips, r.GetSeatInfo());
                        r.Join(client);

                        pack.RoomPack.Add(r.GetRoomInfo);
                        foreach (UserInfoPack user in r.GetRoomUserInfo())
                        {
                            pack.UserInfoPack.Add(user);
                        }
                        pack.ReturnCode = ReturnCode.Succeed;
                        return pack;
                    }
                    else
                    {
                        //無法加入房間
                        pack.ReturnCode = ReturnCode.Fail;
                        return pack;
                    }
                }
            }

            //沒有此房間
            pack.ReturnCode = ReturnCode.NotRoom;
            return pack;
        }

        /// <summary>
        /// 離開房間
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        /// <returns></returns>
        public MainPack ExitRoom(Client client, MainPack pack)
        {
            if (client.GetRoom == null)
            {
                pack.ReturnCode = ReturnCode.Fail;
                return null;
            }

            client.GetRoom.Exit(this, client);
            pack.ReturnCode = ReturnCode.Succeed;
            return pack;
        }

        /// <summary>
        /// 移除房間
        /// </summary>
        /// <param name="room"></param>
        public void RemoveRoom(Room room)
        {
            roomList.Remove(room);
        }
    }
}
