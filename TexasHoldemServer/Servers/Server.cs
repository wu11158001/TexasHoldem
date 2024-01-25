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
    }
}
