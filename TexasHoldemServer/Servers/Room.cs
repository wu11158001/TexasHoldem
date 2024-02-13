using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using TexasHoldemProtobuf;
using TexasHoldemServer.Tools;

namespace TexasHoldemServer.Servers
{
    class Room
    {
        private Server server;

        //房間內所有客戶端
        private List<Client> clientList;

        private const string computerName = "-1";

        private int actionCount = 0;

        CancellationTokenSource cts;
        CancellationToken token;

        public Room(Server server, Client client, RoomPack pack, string initChips, string bigBlindValue)
        {
            this.server = server;
            roomInfo = pack;
            clientList = new List<Client>();
            clientList.Add(client);
            client.GetRoom = this;

            cts = new CancellationTokenSource();
            token = cts.Token;

            ComputerInfo = new ComputerData();
            ComputerInfo.NickName = "專業代打";
            ComputerInfo.Avatar = "0";
            ComputerInfo.Chips = initChips;
            ComputerInfo.HandPoker = new int[5];
            ComputerInfo.BetChips = "0";

            roomState = new RoomState();
            roomState.bigBlindValue = bigBlindValue;
            roomState.result = new int[5];
            roomState.handPoker = new Dictionary<string, int[]>();
            roomState.winners = new List<string>();
        }

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

        /// <summary>
        /// 獲取房間玩家訊息
        /// </summary>
        /// <returns></returns>
        public RepeatedField<UserInfoPack> GetRoomUserInfo()
        {
            RepeatedField<UserInfoPack> pack = new RepeatedField<UserInfoPack>();
            foreach (Client c in clientList)
            {
                UserInfoPack userInfoPack = new UserInfoPack();
                userInfoPack.NickName = c.UserInfo.NickName;
                userInfoPack.Avatar = c.UserInfo.Avatar;
                userInfoPack.Chips = c.UserInfo.Chips;
                userInfoPack.GameSeat = c.UserInfo.GameSeat;

                pack.Add(userInfoPack);
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
            pack.SendModeCode = SendModeCode.RoomBroadcast;
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
            foreach (UserInfoPack user in GetRoomUserInfo())
            {
                pack.UserInfoPack.Add(user);
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
            foreach (UserInfoPack user in GetRoomUserInfo())
            {
                pack.UserInfoPack.Add(user);
            }

            pack.ComputerPack = GetComputerPack();

            Broadcast(null, pack);
        }

        /// <summary>
        /// 離開房間
        /// </summary>
        /// <param name="server"></param>
        /// <param name="client"></param>
        public void Exit(Server server, Client client)
        {
            clientList.Remove(client);
            client.GetRoom = null;

            //關閉房間
            if (clientList.Count == 0)
            {
                cts?.Cancel();
                client.GetRoom = null;
                server.RemoveRoom(this);
                return;
            }

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.OtherUserExitRoom;
            pack.SendModeCode = SendModeCode.RoomBroadcast;

            UserInfoPack userInfoPack = new UserInfoPack();
            userInfoPack.NickName = client.UserInfo.NickName;

            pack.UserInfoPack.Add(userInfoPack);

            Broadcast(client, pack);
        }

        /// <summary>
        /// 獲取座位訊息
        /// </summary>
        /// <returns></returns>
        public int GetSeatInfo()
        {
            int seat = 0;
            while (true)
            {
                bool canSit = true;
                for (int i = 0; i < clientList.Count; i++)
                {
                    if (clientList[i].UserInfo.GameSeat == seat)
                    {
                        canSit = false;
                        break;
                    }
                }

                if (canSit)
                {
                    return seat;
                }

                seat++;
            }
        }

        /// <summary>
        /// 房間狀態
        /// </summary>
        public class RoomState
        {
            public GameProcess gameProcess;                                     //遊戲進程
            public string actionUser;                                           //行動玩家(暱稱)
            public string smallBlinder;                                         //小盲玩家(暱稱, -1=電腦)
            public string bigBlinder;                                           //大盲玩家(暱稱, -1=電腦)
            public string bigBlindValue;                                        //大盲籌碼
            public string totalBetChips;                                        //總下注籌碼
            public int[] result;                                                //牌面結果
            public Dictionary<string, int[]> handPoker;                         //所有玩家手牌(暱稱, 手牌)
            public int smallBlindIndex;                                         //小盲玩家編號
            public string currBet;                                              //當前下注籌碼
            public bool isFirstActioner;                                        //是否首位行動玩家
            public List<string> winners;                                        //贏家
        }
        public RoomState roomState;

        /// <summary>
        /// 電腦玩家
        /// </summary>
        public class ComputerData
        {
            public string NickName { get; set; }
            public string Avatar { get; set; }
            
            public int[] HandPoker { get; set; }
            public string BetChips { get; set; }
            public string Chips { get; set; }
        }
        public ComputerData ComputerInfo { get; set; }

        /// <summary>
        /// 獲取電腦玩家包
        /// </summary>
        /// <returns></returns>
        private ComputerPack GetComputerPack()
        {
            ComputerPack computerPack = new ComputerPack();
            computerPack.NickName = ComputerInfo.NickName;
            computerPack.Avatar = ComputerInfo.Avatar;

            computerPack.HandPoker.AddRange(ComputerInfo.HandPoker);
            computerPack.BetChips = ComputerInfo.BetChips;
            computerPack.Chips = ComputerInfo.Chips;

            return computerPack;
        }

        /// <summary>
        /// 獲取遊戲進程包
        /// </summary>
        /// <returns></returns>
        private GameProcessPack GetGameProcessPack()
        {
            GameProcessPack gameProcessPack = new GameProcessPack();
            gameProcessPack.GameProcess = roomState.gameProcess;
            gameProcessPack.ActionUser = roomState.actionUser;
            gameProcessPack.SmallBlinder = roomState.smallBlinder;
            gameProcessPack.BigBlinder = roomState.bigBlinder;
            gameProcessPack.BigBlindValue = roomState.bigBlindValue;
            gameProcessPack.TotalBetChips = roomState.totalBetChips;
            gameProcessPack.Result.AddRange(roomState.result);
            gameProcessPack.CurrBet = roomState.currBet;
            gameProcessPack.Winners.AddRange(roomState.winners);

            foreach (var poker in roomState.handPoker)
            {
                IntList intList = new IntList();
                intList.Values.AddRange(poker.Value);
                gameProcessPack.HandPoker.Add(poker.Key, intList);
            }

            foreach (var user in clientList)
            {
                gameProcessPack.BetShips.Add(user.UserInfo.NickName, user.UserInfo.BetChips);
                gameProcessPack.UserChips.Add(user.UserInfo.NickName, user.UserInfo.Chips);
            }

            return gameProcessPack;
        }

        /// <summary>
        /// 開始遊戲
        /// </summary>
        /// <returns></returns>
        async public void StartGame()
        {
            //選擇大小盲
            SetBlinder();
            BroadcastGameStage();

            //翻牌前
            roomState.gameProcess = GameProcess.Preflop;
            await Task.Delay(2000);

            SetResult();
            BroadcastGameStage();

            await Task.Delay(2000);
            SetNextActionUser();
        }

        /// <summary>
        /// 設定大小盲
        /// </summary>
        private void SetBlinder()
        {
            //初始化
            ComputerInfo.BetChips = "0";
            foreach (var user in clientList)
            {
                user.UserInfo.BetChips = "0";
            }

            actionCount = 2;
            roomState.gameProcess = GameProcess.SetBlind;
            roomState.actionUser = "-1";
            roomState.smallBlindIndex = -1;
            roomState.currBet = "0";
            roomState.isFirstActioner = true;
            roomState.winners.Clear();
            roomState.totalBetChips = Utils.StringAddition((Convert.ToInt32(roomState.bigBlindValue) / 2).ToString(), roomState.bigBlindValue);

            //小盲玩家
            roomState.smallBlindIndex = roomState.smallBlindIndex + 1 >= clientList.Count ? -1 : roomState.smallBlindIndex + 1;
            roomState.smallBlinder = roomState.smallBlindIndex == -1 ? computerName : clientList[roomState.smallBlindIndex].UserInfo.NickName;
            roomState.currBet = roomState.bigBlindValue;

            if (roomState.smallBlindIndex == -1)
            {
                ReviserComputer((Convert.ToInt32(roomState.bigBlindValue) / 2).ToString());
            }
            else
            {
                ReviseUserInfo(roomState.smallBlindIndex, (Convert.ToInt32(roomState.bigBlindValue) / 2).ToString());
            }

            //大盲玩家
            int bigBlinder = roomState.smallBlindIndex + 1 >= clientList.Count ? -1 : roomState.smallBlindIndex + 1;
            roomState.bigBlinder = bigBlinder == -1 ? computerName : clientList[bigBlinder].UserInfo.NickName;
            if (bigBlinder == -1)
            {
                ReviserComputer(roomState.bigBlindValue);
            }
            else
            {
                ReviseUserInfo(bigBlinder, roomState.bigBlindValue);
            }

            //修改電腦訊息
            void ReviserComputer(string blindValue)
            {
                ComputerInfo.Chips = Utils.StringSubtract(ComputerInfo.Chips, blindValue);
                ComputerInfo.BetChips = Utils.StringAddition(ComputerInfo.BetChips, blindValue);
            }

            //修改用戶訊息
            void ReviseUserInfo(int userIndex, string blindValue)
            {
                string chips = Utils.StringSubtract(clientList[userIndex].UserInfo.Chips, blindValue);
                clientList[userIndex].UserInfo.Chips = chips;

                clientList[userIndex].UserInfo.BetChips = Utils.StringAddition(clientList[userIndex].UserInfo.BetChips, blindValue);

                string cash = Utils.StringSubtract(clientList[userIndex].UserInfo.Cash, blindValue);
                clientList[userIndex].UserInfo.Cash = cash;
                bool result = clientList[userIndex].GetMySql.ReviseData(clientList[userIndex].GetMySqlConnection,
                                                    "userdata",
                                                    "account",
                                                    clientList[userIndex].UserInfo.Account,
                                                    new string[]{ "cash" },
                                                    new string[] { cash }
                                                    );
            }
        }

        /// <summary>
        /// 設定結果
        /// </summary>
        private void SetResult()
        {
            int poker = 0;
            roomState.result = new int[5];
            roomState.handPoker.Clear();

            //52張撲克
            List<int> pokerList = new List<int>();
            for (int i = 0; i < 52; i++)
            {
                pokerList.Add(i + 1);
            }

            //牌面結果
            for (int i = 0; i < 5; i++)
            {
                poker = new Random().Next(0, pokerList.Count);
                roomState.result[i] = pokerList[poker];
                Console.Write($"{new string(i == 0 ? "牌面結果:" : "")}{pokerList[poker]}, ");
                pokerList.RemoveAt(poker);
            }
            Console.WriteLine("\n");

            //玩家手牌
            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].UserInfo.handPoker = new int[2];
                for (int j = 0; j < 2; j++)
                {
                    poker = Licensing();
                    clientList[i].UserInfo.handPoker[j] = poker;
                    Console.Write($"{new string(j == 0 ? $"{clientList[i].UserInfo.NickName}手牌:" : "")}{poker}, ");
                }
                Console.WriteLine("\n");

                roomState.handPoker.Add(clientList[i].UserInfo.NickName, clientList[i].UserInfo.handPoker);
            }

            //電腦手牌
            ComputerInfo.HandPoker = new int[2];
            for (int i = 0; i < 2; i++)
            {
                poker = Licensing();
                ComputerInfo.HandPoker[i] = poker;
                Console.Write($"{new string(i == 0 ? $"電腦手牌:" : "")}{poker}, "); 
            }
            Console.WriteLine("\n");

            //發牌
            int Licensing()
            {
                int index = new Random().Next(0, pokerList.Count);
                int poker = pokerList[index];
                pokerList.RemoveAt(index);
                return poker;
            }
        }

        /// <summary>
        /// 設定下位行動玩家
        /// </summary>
        async private void SetNextActionUser()
        {
            if (clientList.Count <= 0)
            {
                return;
            }

            actionCount++;

            //設定行動玩家
            if (roomState.actionUser == computerName)
            {
                roomState.actionUser = clientList[0].UserInfo.NickName;
            }
            else
            {
                int currUser = clientList.Select((v, i) => (v, i))
                                         .Where(name => name.v.UserInfo.NickName == roomState.actionUser)
                                         .FirstOrDefault()
                                         .i;
                roomState.actionUser = currUser + 1 >= clientList.Count ? computerName : clientList[currUser].UserInfo.NickName;
            }
            Console.WriteLine($"當前行動玩家:{roomState.actionUser}");

            SendActioner();

            //電腦行動
            if (roomState.actionUser == computerName)
            {
                await Task.Delay(3000);

                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.ShowUserAction;

                GameActionPack gameActionPack = new GameActionPack();
                gameActionPack.ActionNickName = computerName;
                
                int currBet = Convert.ToInt32(roomState.currBet);

                string betValue = "";
                if (currBet > 0)
                {
                    if (roomState.currBet == ComputerInfo.BetChips)
                    {
                        //過牌
                        gameActionPack.UserGameState = UserGameState.Pass;
                    }
                    else
                    {
                        //跟注
                        betValue = roomState.currBet;
                        gameActionPack.UserGameState = UserGameState.Follow;
                        string subtractChips = Utils.StringSubtract(betValue, ComputerInfo.BetChips);
                        ComputerInfo.Chips = Utils.StringSubtract(ComputerInfo.Chips, subtractChips);
                        roomState.totalBetChips = Utils.StringAddition(roomState.totalBetChips, subtractChips);
                    }                    
                }
                else
                {
                    //加注
                    betValue = roomState.bigBlindValue;
                    gameActionPack.UserGameState = UserGameState.Add;
                    ComputerInfo.Chips = Utils.StringSubtract(ComputerInfo.Chips, betValue);
                    roomState.totalBetChips = Utils.StringAddition(roomState.totalBetChips, betValue);
                }
                ComputerInfo.BetChips = betValue != "" ? betValue : ComputerInfo.BetChips;
                roomState.currBet = betValue != "" ? betValue : roomState.currBet;
                gameActionPack.BetValue = betValue;

                pack.GameActionPack = gameActionPack;
                pack.GameProcessPack = GetGameProcessPack();
                pack.ComputerPack = GetComputerPack();
                Broadcast(null, pack);

                cts?.Cancel();
                await Task.Delay(1000);

                if (await IsNextRound() == false)
                {
                    SetNextActionUser();
                    SendActioner();
                }
            }
        }

        /// <summary>
        /// 玩家行動
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pack"></param>
        async public void UserAction(Client client, MainPack pack)
        {
            cts?.Cancel();

            string betValue = pack.GameActionPack.BetValue;
            string subtractCash = (Math.Abs(Convert.ToInt32( Utils.StringSubtract(betValue, client.UserInfo.BetChips)))).ToString();

            Dictionary<string, string> dataDic = client.GetMySql.GetData(client.GetMySqlConnection,
                                                                         "userdata", "account",
                                                                         client.UserInfo.Account,
                                                                         new string[] { "cash" });

            string cash = Utils.StringSubtract(dataDic["cash"], subtractCash);

            client.UserInfo.Chips = Utils.StringSubtract(client.UserInfo.Chips, subtractCash);
            client.UserInfo.BetChips = betValue;
            client.UserInfo.GameState = pack.GameActionPack.UserGameState;

            roomState.isFirstActioner = false;
            roomState.currBet = betValue;
            roomState.totalBetChips = Utils.StringAddition(roomState.totalBetChips, subtractCash);

            //扣除金幣
            if (betValue != "0")
            {
                bool result = client.GetMySql.ReviseData(client.GetMySqlConnection,
                                             "userdata",
                                             "account",
                                             client.UserInfo.Account,
                                             new string[] { "cash" },
                                             new string[] { cash }
                                             );
            }

            pack.GameProcessPack = GetGameProcessPack();
            pack.ComputerPack = GetComputerPack();
            pack.GameActionPack.ActionNickName = client.UserInfo.NickName;

            Broadcast(null, pack);

            await Task.Delay(3000);

            if (await IsNextRound() == false)
            {
                SetNextActionUser();
                BroadcastGameStage();
            }
        }

        /// <summary>
        /// 發送行動者指令
        /// </summary>
        /// <returns></returns>
        private void SendActioner()
        {
            if (clientList.Count <= 0)
            {
                return;
            }

            cts = new CancellationTokenSource();
            token = cts.Token;

            Task.Run(async() =>
            {
                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.ActionerOrder;

                ActionerPack actionerPack = new ActionerPack();
                actionerPack.Actioner = roomState.actionUser;
                actionerPack.IsFirstActioner = roomState.isFirstActioner;

                pack.GameProcessPack = GetGameProcessPack();

                for (int i = 8; i >= 0; i--)
                {
                    actionerPack.Countdown = (float)i;
                    pack.ActionerPack = actionerPack;
                    Broadcast(null, pack);

                    await Task.Delay(1000);

                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }, token);
        }

        /// <summary>
        /// 判斷是否進入下一階段
        /// </summary>
        /// <returns></returns>
        async private Task<bool> IsNextRound()
        {
            Console.WriteLine($"已行動人數:{actionCount}");
            if (actionCount < clientList.Count + 1)
            {
                return false;
            }

            string compareBet = ComputerInfo.BetChips;
            bool isNext = clientList.All(user => user.UserInfo.BetChips == compareBet);
            Console.WriteLine($"進入下階段:{isNext} : 當前:{roomState.gameProcess}");

            if (isNext)
            {
                actionCount = 0;
                roomState.isFirstActioner = true;

                //當前狀態
                switch (roomState.gameProcess)
                {
                    //翻牌前
                    case GameProcess.Preflop:
                        roomState.gameProcess = GameProcess.Flop;
                        BroadcastGameStage();

                        await Task.Delay(2000);

                        SetNextActionUser();
                        BroadcastGameStage();
                        break;

                    //翻牌
                    case GameProcess.Flop:
                        roomState.gameProcess = GameProcess.Turn;
                        BroadcastGameStage();

                        await Task.Delay(2000);

                        SetNextActionUser();
                        BroadcastGameStage();
                        break;

                    //轉牌
                    case GameProcess.Turn:
                        roomState.gameProcess = GameProcess.River;
                        BroadcastGameStage();

                        await Task.Delay(2000);

                        SetNextActionUser();
                        BroadcastGameStage();
                        break;

                    //遊戲結果
                    case GameProcess.River:
                        roomState.gameProcess = GameProcess.GameResult;
                        JudgeGameResult();
                        BroadcastGameStage();

                        await Task.Delay(5000);

                        StartGame();
                        break;
                }
            }

            return isNext;
        }

        /// <summary>
        /// 判斷遊戲結果
        /// </summary>
        private void JudgeGameResult()
        {
            roomState.winners.Add(clientList[0].UserInfo.NickName);

            int winChips = Convert.ToInt32(roomState.totalBetChips) / roomState.winners.Count;
            foreach (var winner in roomState.winners)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    if (clientList[i].UserInfo.NickName == winner)
                    {
                        //玩家獲勝
                        Dictionary<string, string> dataDic = clientList[i].GetMySql.GetData(clientList[i].GetMySqlConnection, 
                                                                                            "userdata", "account", 
                                                                                            clientList[i].UserInfo.Account,
                                                                                            new string[] { "cash"});
                     
                        string cash = Utils.StringAddition(dataDic["cash"], winChips.ToString());
                        //修改資料庫金幣
                        bool result = clientList[i].GetMySql.ReviseData(clientList[i].GetMySqlConnection,
                                             "userdata",
                                             "account",
                                             clientList[i].UserInfo.Account,
                                             new string[] { "cash" },
                                             new string[] { cash }
                                             );
                        clientList[i].UserInfo.Chips = Utils.StringAddition(clientList[i].UserInfo.Chips, winChips.ToString());
                        break;
                    }
                    else if(winner == computerName)
                    {
                        //電腦獲勝
                        ComputerInfo.Chips = Utils.StringAddition(ComputerInfo.Chips, winChips.ToString());
                    }
                }
            }            
        }

        /// <summary>
        /// 廣播遊戲階段
        /// </summary>
        private void BroadcastGameStage()
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.GameStage;

            pack.GameProcessPack = GetGameProcessPack();
            pack.ComputerPack = GetComputerPack();

            Broadcast(null, pack);
        }
    }
}
