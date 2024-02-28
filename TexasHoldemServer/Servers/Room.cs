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
        private Dictionary<string, int> allInDic;

        private int betCount;
        private int actionCount;
        private int maxActionCount;
        private string initChips;
        private int computerNameIndex;
        private int computerAction;         //0=不會再行動,1=可以行動

        private const string computerName = "-1";
        private readonly string[] shapeNames = new string[]
        {
            "皇家同花順",
            "同花順",
            "四條",
            "葫蘆",
            "同花",
            "順子",
            "三條",
            "兩對",
            "一對",
            "高牌"
        };

        CancellationTokenSource cts;
        CancellationToken token;

        public Room(Server server, Client client, RoomPack pack, string initChips, string bigBlindValue)
        {
            this.server = server;
            this.initChips = initChips;
            roomInfo = pack;
            clientList = new List<Client>();
            clientList.Add(client);
            client.GetRoom = this;

            cts = new CancellationTokenSource();
            token = cts.Token;

            allInDic = new Dictionary<string, int>();
            ComputerInfo = new ComputerData();
            ComputerInfo.NickName = "專業代打";
            ComputerInfo.Avatar = "0";
            ComputerInfo.Chips = initChips;
            ComputerInfo.BetChips = "0";
            computerNameIndex = 1;

            roomState = new RoomState();
            roomState.bigBlindValue = bigBlindValue;
            roomState.result = new int[5];
            roomState.handPoker = new Dictionary<string, int[]>();
            roomState.winners = new List<string>();
            roomState.pokerShape = new Dictionary<string, int>();
            roomState.actionUser = "-1";
            roomState.smallBlindIndex = -1;
            roomState.recodeBetValue = new int[2];

            sideWinInfo = new SideWinData();
            sideWinInfo.sideWinner = new Dictionary<string, string>();
            sideWinInfo.sideBackChips = new Dictionary<string, string>();
        }

        //房間訊息
        private RoomPack roomInfo;
        public RoomPack GetRoomInfo
        {
            get
            {
                roomInfo.CurrCount = clientList.Count;
                roomInfo.RoomBigBlind = roomState.bigBlindValue;
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
            Console.WriteLine($"{client.UserInfo.NickName}:加入房間");
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
            Console.WriteLine($"{client.UserInfo.NickName}:離開房間");

            //處於當前回合
            if (roomState.actionUser == client.UserInfo.NickName)
            {
                GameActionPack gameActionPack = new GameActionPack();
                gameActionPack.UserGameState = UserGameState.Abort;
                gameActionPack.BetValue = client.UserInfo.BetChips;

                MainPack exitPack = new MainPack();
                exitPack.GameActionPack = gameActionPack;
                UserAction(client, exitPack);
            }

            clientList.Remove(client);

            //關閉房間
            if (clientList.Count == 0)
            {
                Console.WriteLine("關閉房間!");
                cts?.Cancel();
                server.RemoveRoom(this);
                return;
            }

            client.GetRoom = null;

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
            public List<string> winners;                                        //贏家
            public string winChips;                                             //贏得籌碼值
            public Dictionary<string, int> pokerShape;                          //所有玩家/電腦牌型(暱稱, 牌型)
            public int[] recodeBetValue;                                        //紀錄前兩位下注籌碼
            public string sidePot;                                              //邊池
        }
        public RoomState roomState;

        /// <summary>
        /// 電腦玩家
        /// </summary>
        public class ComputerData
        {
            public string NickName { get; set; }
            public string Avatar { get; set; }           
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
            computerPack.BetChips = ComputerInfo.BetChips;
            computerPack.Chips = ComputerInfo.Chips;

            return computerPack;
        }

        /// <summary>
        /// 邊池獲勝
        /// </summary>
        public class SideWinData
        {
            public Dictionary<string, string> sideWinner;             //邊池獲勝者(暱稱, 籌碼)
            public Dictionary<string, string> sideBackChips;		   //邊池退回籌碼(暱稱, 籌碼)
        }
        public SideWinData sideWinInfo { get; set; }

        /// <summary>
        /// 獲取邊池獲勝者包
        /// </summary>
        /// <returns></returns>
        private SideWinPack GetSideWinPack()
        {
            SideWinPack sideWinPack = new SideWinPack();

            foreach (var win in sideWinInfo.sideWinner)
            {
                sideWinPack.SideWinner.Add(win.Key, win.Value);
            }

            foreach (var back in sideWinInfo.sideBackChips)
            {
                sideWinPack.SideBackChips.Add(back.Key, back.Value);
            }

            return sideWinPack;
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
            gameProcessPack.WinChips = roomState.winChips;
            gameProcessPack.MinBetValue = GetMinBetValue().ToString();
            gameProcessPack.SidePotValue = roomState.sidePot;
            
            foreach (var poker in roomState.handPoker)
            {
                IntList intList = new IntList();
                intList.Values.AddRange(poker.Value);

                for (int i = 0; i < clientList.Count; i++)
                {
                    if (clientList[i].UserInfo.NickName == poker.Key &&  
                        clientList[i].UserInfo.GameState != UserGameState.StateNone &&
                        clientList[i].UserInfo.GameState != UserGameState.Abort)
                    {
                        gameProcessPack.HandPoker.Add(poker.Key, intList);
                        break;
                    }
                }

                if (poker.Key == computerName)
                {
                    gameProcessPack.HandPoker.Add(poker.Key, intList);
                }
            }

            foreach (var user in clientList)
            {
                gameProcessPack.UserStates.Add(user.UserInfo.NickName, user.UserInfo.GameState);
                if (user.UserInfo.GameState != UserGameState.StateNone)
                {
                    gameProcessPack.BetShips.Add(user.UserInfo.NickName, user.UserInfo.BetChips);
                    gameProcessPack.UserChips.Add(user.UserInfo.NickName, user.UserInfo.Chips);
                }
            }

            foreach (var poker in roomState.pokerShape)
            {
                for (int i = 0; i < clientList.Count; i++)
                {
                    if (clientList[i].UserInfo.NickName == poker.Key && clientList[i].UserInfo.GameState != UserGameState.StateNone)
                    {
                        gameProcessPack.PokerShape.Add(poker.Key, shapeNames[poker.Value]);
                        break;
                    }
                }

                if (poker.Key == computerName)
                {
                    gameProcessPack.PokerShape.Add(poker.Key, shapeNames[poker.Value]);
                }
            }

            return gameProcessPack;
        }

        /// <summary>
        /// 獲取最小下注值
        /// </summary>
        /// <returns></returns>
        private int GetMinBetValue()
        {
            int minValue = 0;

            if (betCount == 0)
            {
                minValue = Convert.ToInt32(roomState.currBet) + Convert.ToInt32(roomState.bigBlindValue);
            }
            else if (betCount == 1)
            {
                minValue = Convert.ToInt32(roomState.currBet) * 2;
            }
            else
            {
                minValue = Convert.ToInt32(roomState.currBet) + (roomState.recodeBetValue[1] - roomState.recodeBetValue[0]);
            }

            return minValue;
        }

        /// <summary>
        /// 記錄下注值
        /// </summary>
        /// <param name="betValue"></param>
        private void SetRecodeBet(int betValue)
        {
            if (roomState.recodeBetValue[0] != 0 && roomState.recodeBetValue[1] != 0)
            {
                int temp = roomState.recodeBetValue[0];
                roomState.recodeBetValue[0] = roomState.recodeBetValue[1];
                roomState.recodeBetValue[1] = Convert.ToInt32(betValue);
            }
            else
            {
                for (int i = 0; i < roomState.recodeBetValue.Length; i++)
                {
                    if (roomState.recodeBetValue[i] == 0)
                    {
                        roomState.recodeBetValue[i] = Convert.ToInt32(betValue);
                    }
                }
            }
        }

        /// <summary>
        /// 重製下注紀錄
        /// </summary>
        private void InitRecodeBet()
        {
            for (int i = 0; i < roomState.recodeBetValue.Length; i++)
            {
                roomState.recodeBetValue[i] = 0;
            }
        }

        /// <summary>
        /// 開始遊戲
        /// </summary>
        /// <returns></returns>
        async public void StartGame()
        {
            //選擇大小盲
            if (!SetBlinder())
            {
                return;
            }
            BroadcastGameStage();

            //翻牌前
            roomState.gameProcess = GameProcess.Preflop;
            SetResult();
            await Task.Delay(2000);

            SetPokerShape(0);
            BroadcastGameStage();

            await Task.Delay(2000);

            computerAction = 1;
            actionCount = clientList.Count > 1 ? 2 : 0;
            betCount = 2;
            SetNextActionUser();
        }

        /// <summary>
        /// 設定玩家牌型
        /// </summary>
        /// <param name="poolCount"></param>
        private void SetPokerShape(int poolCount)
        {
            roomState.pokerShape.Clear();
            int shapeNum = 0;
            int[] poolResult = poolCount == 0 ? null : roomState.result.Take(poolCount).ToArray();

            foreach (var poker in roomState.handPoker)
            {
                shapeNum = PokerShape.Instance.GetCardShape(poker.Value, poolResult);
                if (!roomState.pokerShape.ContainsKey(poker.Key))
                {
                    roomState.pokerShape.Add(poker.Key, shapeNum);
                }                
            }
        }

        /// <summary>
        /// 設定大小盲
        /// </summary>
        private bool SetBlinder()
        {
            //初始化
            allInDic.Clear();
            ComputerInfo.BetChips = "0";
            foreach (var user in clientList)
            {
                user.UserInfo.BetChips = "0";
            }
            roomState.sidePot = "";
            roomState.gameProcess = GameProcess.SetBlind;
            roomState.currBet = roomState.bigBlindValue;
            roomState.winChips = "0";
            roomState.winners.Clear();
            roomState.totalBetChips = Utils.StringAddition((Convert.ToInt32(roomState.bigBlindValue) / 2).ToString(), roomState.bigBlindValue);
            roomState.recodeBetValue[0] = Convert.ToInt32(roomState.bigBlindValue) / 2;
            roomState.recodeBetValue[1] = Convert.ToInt32(roomState.bigBlindValue);

            sideWinInfo.sideBackChips.Clear();
            sideWinInfo.sideWinner.Clear();

            //電腦玩家籌碼不足
            if (Convert.ToInt64(ComputerInfo.Chips) < Convert.ToInt64(Utils.StringAddition((Convert.ToInt32(roomState.bigBlindValue) / 2).ToString(), roomState.bigBlindValue)))
            {
                computerNameIndex++;
                ComputerInfo.NickName = $"專業代打{computerNameIndex}";
                ComputerInfo.Chips = initChips.ToString();
            }

            //更改玩家遊戲狀態
            List<Client> roundPlayer = new List<Client>();
            foreach (var user in clientList)
            {
                roundPlayer.Add(user);
                user.UserInfo.GameState = UserGameState.Playing;
            }

            if (roundPlayer.Count == 0)
            {
                return false;
            }

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
            roomState.actionUser = roomState.bigBlinder;

            return true;

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
            maxActionCount = clientList.Count() + 1;
            int poker = 0;
            roomState.result = new int[5];
            roomState.handPoker.Clear();

            //52張撲克
            List<int> pokerList = new List<int>();
            for (int i = 0; i < 52; i++)
            {
                pokerList.Add(i);
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
            int[] ComputerInfoHandPoker = new int[2];
            for (int i = 0; i < 2; i++)
            {
                poker = Licensing();
                ComputerInfoHandPoker[i] = poker;                
                Console.Write($"{new string(i == 0 ? $"電腦手牌:" : "")}{poker}, "); 
            }
            roomState.handPoker.Add(computerName, ComputerInfoHandPoker);
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

            List<Client> playingList = GetPlayingUser(true);
            if (playingList.Count + computerAction == 0 && actionCount > playingList.Count + 1)
            {
                Console.WriteLine("設定下位行動玩家: 剩下一位玩家,設定遊戲結果!");

                await Task.Delay(1000);

                cts?.Cancel();
                SendGameResult();
                return;
            }

            //設定行動玩家
            if (roomState.actionUser == computerName)
            {
                if (!IsPeekNext(-1))
                {
                    return;
                }
            }
            else
            {                
                int currUserIndex = clientList.Select((v, i) => (v, i))
                                              .Where(name => name.v.UserInfo.NickName == roomState.actionUser)
                                              .FirstOrDefault()
                                              .i;

                Console.WriteLine($"前位行動玩家編號{currUserIndex}");

                if (!IsPeekNext(currUserIndex))
                {
                    if (roomState.actionUser != computerName)
                    {
                        return;
                    }
                    return;
                }
            }            

            SendActioner();

            //電腦行動
            ComputerAction();
        }

        /// <summary>
        /// 是否有下位行動者
        /// </summary>
        /// <param name="currUserIndex"></param>
        private bool IsPeekNext(int currUserIndex)
        {
            //無人能行動
            if (GetPlayingUser(true).Count() == 0 && computerAction == 0)
            {
                Console.WriteLine("是否有下位行動者:無人能行動判斷結果");
                cts?.Cancel();
                SendGameResult();
                return false;
            }

            string actioner = "";
            if (currUserIndex + 1 >= clientList.Count())
            {
                actioner = computerName;
            }
            else
            {
                if (clientList[currUserIndex + 1].UserInfo.GameState == UserGameState.StateNone ||
                    clientList[currUserIndex + 1].UserInfo.GameState == UserGameState.Abort ||
                    clientList[currUserIndex + 1].UserInfo.GameState == UserGameState.AllIn)
                {
                    return IsPeekNext(currUserIndex + 1);
                }

                actioner = clientList[currUserIndex + 1].UserInfo.NickName;
            }

            roomState.actionUser = actioner;
            Console.WriteLine("當前行動者:" + roomState.actionUser + "/行動數:" + actionCount + "/最大行動數:" + maxActionCount);

            if (isIntoNextRound())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 判斷是否進入下階段
        /// </summary>
        /// <returns></returns>
        private bool isIntoNextRound()
        {
            if (actionCount >= maxActionCount)
            {
                if (actionCount >= GetPlayingUser(true).Count())
                {
                    bool isSameBet = GetPlayingUser(true).Count() == 0 ?
                                     true :
                                     GetPlayingUser(false).All(user => user.UserInfo.BetChips == roomState.currBet);
                    bool isComputer = false;
                    if (ComputerInfo.Chips == "0" || ComputerInfo.BetChips == roomState.currBet)
                    {
                        isComputer = true;
                    }

                    if (isSameBet && isComputer)
                    {
                        NextRound();
                        return true;
                    }
                }

                actionCount = 0;
            }

            return false;
        }

        /// <summary>
        /// 發送行動者指令
        /// </summary>
        /// <returns></returns>
        private void SendActioner()
        {
            if (clientList.Count <= 0 )
            {
                return;
            }

            Client client = null;
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].UserInfo.NickName == roomState.actionUser)
                {
                    client = clientList[i];
                    break;
                }
            }
            if(client != null && client.UserInfo.GameState == UserGameState.AllIn)
            {
                GameActionPack gameActionPack = new GameActionPack();
                gameActionPack.UserGameState = UserGameState.AllIn;
                gameActionPack.BetValue = client.UserInfo.BetChips;
                MainPack abortpack = new MainPack();
                abortpack.ActionCode = ActionCode.ShowUserAction;
                abortpack.GameActionPack = gameActionPack;
                UserAction(client, abortpack);

                return;
            }

            if (roomState.actionUser == computerName && ComputerInfo.Chips == "0")
            {
                return;
            }

            cts?.Cancel();
            cts = new CancellationTokenSource();
            token = cts.Token;

            Task.Run(async () =>
            {
                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.ActionerOrder;

                ActionerPack actionerPack = new ActionerPack();
                actionerPack.Actioner = roomState.actionUser;

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

                //倒數結束棄牌                
                GameActionPack gameActionPack = new GameActionPack();
                gameActionPack.UserGameState = UserGameState.Abort;
                gameActionPack.BetValue = client.UserInfo.BetChips;
                MainPack abortpack = new MainPack();
                abortpack.ActionCode = ActionCode.ShowUserAction;
                abortpack.GameActionPack = gameActionPack;
                UserAction(client, abortpack);
            }, token);
        }

        /// <summary>
        /// 電腦行動
        /// </summary>
        async private void ComputerAction()
        {
            if (roomState.actionUser == computerName)
            {
                if (long.Parse(ComputerInfo.Chips) >= long.Parse(roomState.currBet))
                {
                    await Task.Delay(2000);
                }

                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.ShowUserAction;

                GameActionPack gameActionPack = new GameActionPack();
                gameActionPack.ActionNickName = computerName;

                int currBet = Convert.ToInt32(roomState.currBet);

                if (roomState.currBet == ComputerInfo.BetChips)
                {
                    //過牌                    
                    gameActionPack.UserGameState = UserGameState.Pass;
                    gameActionPack.BetValue = roomState.currBet;
                }
                else
                {
                    betCount++;
                    if (long.Parse(ComputerInfo.Chips) <= long.Parse(roomState.currBet))
                    {
                        //All In
                        computerAction = 0;

                        string allValue = Utils.StringAddition(ComputerInfo.BetChips, ComputerInfo.Chips);
                        gameActionPack.UserGameState = UserGameState.AllIn;
                        gameActionPack.BetValue = allValue;
                        roomState.totalBetChips = Utils.StringAddition(roomState.totalBetChips, ComputerInfo.Chips);
                        ComputerInfo.BetChips = allValue;
                        ComputerInfo.Chips = "0";
                        roomState.currBet = allValue;

                        roomState.recodeBetValue[0] = 0;
                        roomState.recodeBetValue[1] = Convert.ToInt32(allValue);

                        AddAllIn(computerName, allValue);

                        //無人能行動
                        if (GetPlayingUser(true).Count() == 0 && computerAction == 0)
                        {
                            Console.WriteLine("電腦行動:無人能行動判斷結果");
                            cts?.Cancel();
                            SendGameResult();
                            return;
                        }
                    }
                    else
                    {
                        //跟注                        
                        gameActionPack.UserGameState = UserGameState.Follow;
                        gameActionPack.BetValue = roomState.currBet;
                        string subtractChips = Utils.StringSubtract(roomState.currBet, ComputerInfo.BetChips);
                        ComputerInfo.Chips = Utils.StringSubtract(ComputerInfo.Chips, subtractChips);
                        roomState.totalBetChips = Utils.StringAddition(roomState.totalBetChips, subtractChips);
                        ComputerInfo.BetChips = roomState.currBet;

                        SetRecodeBet(Convert.ToInt32(ComputerInfo.BetChips));
                    }
                }

                pack.GameActionPack = gameActionPack;
                pack.GameProcessPack = GetGameProcessPack();
                pack.ComputerPack = GetComputerPack();

                Console.WriteLine($"電腦行動:{gameActionPack.UserGameState}");
                Broadcast(null, pack);

                cts?.Cancel();
                await Task.Delay(1000);

                actionCount++;

                SetNextActionUser();
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

            if (pack.GameActionPack.UserGameState == UserGameState.Add)
            {
                betCount++;
            }

            string betValue = pack.GameActionPack.BetValue;

            Dictionary<string, string> dataDic = client.GetMySql.GetData(client.GetMySqlConnection,
                                                                         "userdata", 
                                                                         "account",
                                                                         client.UserInfo.Account,
                                                                         new string[] { "cash" });

            if (dataDic != null)
            {
                string cash = Utils.StringSubtract(dataDic["cash"], Utils.StringSubtract(betValue, client.UserInfo.BetChips));

                //扣除金幣
                bool result = client.GetMySql.ReviseData(client.GetMySqlConnection,
                                                         "userdata",
                                                         "account",
                                                         client.UserInfo.Account,
                                                         new string[] { "cash" },
                                                         new string[] { cash }
                                                         );
            }            
         
            if (client.UserInfo.GameState == UserGameState.Abort)
            {
                maxActionCount--;
            }
            else
            {
                roomState.currBet = betValue;
                roomState.totalBetChips = Utils.StringAddition(roomState.totalBetChips, Utils.StringSubtract(betValue, client.UserInfo.BetChips));

                if (pack.GameActionPack.UserGameState == UserGameState.AllIn)
                {
                    maxActionCount--;

                    roomState.recodeBetValue[0] = 0;
                    roomState.recodeBetValue[1] = Convert.ToInt32(betValue);

                    roomState.currBet = betValue;
                    AddAllIn(client.UserInfo.NickName, betValue);
                }
                else
                {
                    SetRecodeBet(Convert.ToInt32(betValue));     
                }                
            }

            client.UserInfo.Chips = Utils.StringSubtract(client.UserInfo.Chips, Utils.StringSubtract(betValue, client.UserInfo.BetChips));
            client.UserInfo.BetChips = betValue;
            client.UserInfo.GameState = client.UserInfo.Chips == "0" ? UserGameState.AllIn : pack.GameActionPack.UserGameState;
            if (client.UserInfo.Chips == "0")
            {
                pack.GameActionPack.UserGameState = UserGameState.AllIn;
            }
            Console.WriteLine($"{client.UserInfo.NickName} 行動:{client.UserInfo.GameState}");            

            pack.GameProcessPack = GetGameProcessPack();
            pack.ComputerPack = GetComputerPack();
            pack.GameActionPack.ActionNickName = client.UserInfo.NickName;

            Broadcast(null, pack);

            await Task.Delay(3000);

            if (GetPlayingUser(false).Count == 0)
            {
                Console.WriteLine("玩家行動: 剩下一位玩家,設定遊戲結果!");

                await Task.Delay(1000);

                cts?.Cancel();
                SendGameResult();
                return;
            }

            actionCount++;
            SetNextActionUser();
        }      

        /// <summary>
        /// 添加All In玩家
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="betValue"></param>
        private void AddAllIn(string nickName, string betValue)
        {
            if (allInDic.ContainsKey(nickName))
            {
                allInDic.Remove(nickName);
            }
            allInDic.Add(nickName, Convert.ToInt32(betValue));

            if (allInDic.Count() > 1)
            {
                string potStr = "0";
                foreach (var allIn in allInDic)
                {
                    potStr = Utils.StringAddition(potStr, (Math.Abs(allIn.Value - Convert.ToInt32(roomState.currBet))).ToString());
                }

                roomState.totalBetChips = (Convert.ToInt32(roomState.totalBetChips) - Convert.ToInt32(potStr)).ToString();
                roomState.sidePot = potStr;
            }
        }

        /// <summary>
        /// 尋找結束回合玩家
        /// </summary>
        /// <param name="currUserIndex"></param>
        /// <returns></returns>
        private string PeekEndUser(int currUserIndex)
        {
            string endPlayer = "";
            if (currUserIndex + 1 >= clientList.Count)
            {
                endPlayer = computerName;
            }
            else
            {
                if (clientList[currUserIndex + 1].UserInfo.GameState == UserGameState.StateNone ||
                    clientList[currUserIndex + 1].UserInfo.GameState == UserGameState.Abort ||
                    clientList[currUserIndex + 1].UserInfo.GameState == UserGameState.AllIn)
                {
                    return PeekEndUser(currUserIndex + 1);
                }

                endPlayer = clientList[currUserIndex + 1].UserInfo.NickName;
            }

            return endPlayer;
        }

        /// <summary>
        /// 獲取遊戲中玩家
        /// </summary>
        /// <returns></returns>
        private List<Client> GetPlayingUser(bool isGetCanActionUser)
        {
            List<Client> playUserList = new List<Client>();
            if (!isGetCanActionUser)
            {
                //存活玩家
                foreach (var user in clientList)
                {
                    if (user.UserInfo.GameState != UserGameState.StateNone &&
                        user.UserInfo.GameState != UserGameState.Abort)
                    {
                        playUserList.Add(user);
                    }
                }
            }
            else
            {
                //可行動玩家
                foreach (var user in clientList)
                {
                    if (user.UserInfo.GameState != UserGameState.StateNone &&
                        user.UserInfo.GameState != UserGameState.Abort &&
                        user.UserInfo.GameState != UserGameState.AllIn)
                    {
                        playUserList.Add(user);
                    }
                }
            }            

            return playUserList;
        }

        /// <summary>
        /// 進入下一階段
        /// </summary>
        /// <returns></returns>
        async private void NextRound()
        {
            Console.WriteLine($"當前:{roomState.gameProcess}:進入下階段");

            cts?.Cancel();
            betCount = 0;
            actionCount = 0;
            InitRecodeBet();

            int currUserIndex = clientList.Select((v, i) => (v, i))
                                              .Where(name => name.v.UserInfo.NickName == roomState.actionUser)
                                              .FirstOrDefault()
                                              .i;

            //當前狀態
            switch (roomState.gameProcess)
            {
                //翻牌前
                case GameProcess.Preflop:
                    roomState.gameProcess = GameProcess.Flop;
                    SetPokerShape(3);
                    BroadcastGameStage();

                    await Task.Delay(2000);

                    IsGameResult();
                    break;

                //翻牌
                case GameProcess.Flop:
                    roomState.gameProcess = GameProcess.Turn;
                    SetPokerShape(4);
                    BroadcastGameStage();

                    await Task.Delay(2000);

                    IsGameResult();
                    break;

                //轉牌
                case GameProcess.Turn:
                    roomState.gameProcess = GameProcess.River;
                    SetPokerShape(5);
                    BroadcastGameStage();

                    await Task.Delay(2000);

                    IsGameResult();
                    break;

                //遊戲結果
                case GameProcess.River:
                    await Task.Delay(2000);

                    SendGameResult();
                    break;
            }
        }

        /// <summary>
        /// 判斷是否進入遊戲結果
        /// </summary>
        private bool IsGameResult()
        {
            if (GetPlayingUser(true).Count() != 0 && computerAction != 0)
            {
                ComputerAction();
                SendActioner();

                return false;
            }
            else
            {
                SendGameResult();                
                return true;
            }
        }

        /// <summary>
        /// 比對牌型
        /// </summary>
        /// <param name="judgePlayer"></param>
        /// <param name="minResult"></param>
        /// <returns></returns>
        private List<string> ComparingPoker(Dictionary<string, int> judgePlayer, int minResult)
        {
            List<string> winnersList = new List<string>();
            //比較相同結果玩家
            List<string> winnerList = judgePlayer.Where(kv => kv.Value == minResult)
                                                 .Select(kv => kv.Key)
                                                 .ToList();

            Dictionary<string, int[]> winDic = new Dictionary<string, int[]>();
            foreach (var win in winnerList)
            {
                winDic.Add(win, roomState.handPoker[win]);
            }

            if (minResult == 0)
            {
                //皇家桐花順
                foreach (var win in winnerList)
                {
                    winnersList.Add(win);
                }
            }
            else if (minResult == 1)
            {
                //同花順                    
                List<string> winList = PokerShape.Instance.CompareStraightSuit(winDic, roomState.result);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }
            else if (minResult == 2)
            {
                //4條
                List<string> winList = PokerShape.Instance.ComparePair(winDic, roomState.result, 4);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }
            else if (minResult == 3)
            {
                //葫蘆
                List<string> winList = PokerShape.Instance.CompareGourd(winDic, roomState.result);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }
            else if (minResult == 4)
            {
                //同花
                List<string> winList = PokerShape.Instance.CompareSameSuit(winDic, roomState.result);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }
            else if (minResult == 5)
            {
                //順子
                List<string> winList = PokerShape.Instance.CompareStraight(winDic, roomState.result);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }
            else if (minResult == 6)
            {
                //三條
                List<string> winList = PokerShape.Instance.ComparePair(winDic, roomState.result, 3);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }
            else if (minResult == 7)
            {
                //兩對
                List<string> winList = PokerShape.Instance.CompareDoublePair(winDic, roomState.result);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }
            else if (minResult == 8)
            {
                //一對
                List<string> winList = PokerShape.Instance.ComparePair(winDic, roomState.result, 2);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }
            else
            {
                //高牌
                List<string> winList = PokerShape.Instance.CompareHighCard(winDic, roomState.result);
                foreach (var win in winList)
                {
                    winnersList.Add(win);
                }
            }

            return winnersList;
        }

        /// <summary>
        /// 判斷遊戲結果
        /// </summary>
        private void JudgeGameResult()
        {
            List<Client> judgeList = new List<Client>();
            foreach (var user in clientList)
            {
                if (user.UserInfo.GameState != UserGameState.StateNone &&
                    user.UserInfo.GameState != UserGameState.Abort)
                {
                    judgeList.Add(user);
                }
            }
            if (judgeList.Count == 0)
            {
                //剩下一位玩家未棄牌
                roomState.winners.Add(computerName);
            }
            else
            {
                int minResult = roomState.pokerShape.Values.Min();
                Dictionary<string, int> surviveUser = new Dictionary<string, int>();
                for (int i = 0; i < clientList.Count; i++)
                {
                    if (clientList[i].UserInfo.GameState != UserGameState.Abort &&
                        clientList[i].UserInfo.GameState != UserGameState.StateNone)
                    {
                        string nickName = clientList[i].UserInfo.NickName;
                        surviveUser.Add(nickName, roomState.pokerShape[nickName]);
                    }
                }
                surviveUser.Add(computerName, roomState.pokerShape[computerName]);

                if (surviveUser.Count > 1)
                {
                    roomState.winners = ComparingPoker(surviveUser, minResult);
                }
                else
                {
                    //剩下1名玩家
                    roomState.winners.Add(surviveUser.Keys.FirstOrDefault());
                }                
            }

            //防呆(沒判斷到先給所有人平分)
            if (roomState.winners.Count == 0)
            {
                Console.WriteLine("判斷牌型大小有誤!!!!");
                roomState.winners.Add(computerName);
                foreach (var user in clientList)
                {
                    roomState.winners.Add(user.UserInfo.NickName);
                }
            }

            int winChips = Convert.ToInt32(roomState.totalBetChips) / roomState.winners.Count;
            roomState.winChips = winChips.ToString();

            //主池判斷
            Console.Write($"主池獲勝者:");
            foreach (var winner in roomState.winners)
            {
                Console.Write($"{winner} ,");

                if (winner == computerName)
                {
                    //電腦獲勝
                    ComputerInfo.Chips = Utils.StringAddition(ComputerInfo.Chips, winChips.ToString());
                }
                else
                {
                    //玩家獲勝
                    for (int i = 0; i < judgeList.Count; i++)
                    {
                        if (judgeList[i].UserInfo.NickName == winner)
                        {
                            ModifyUserCashData(judgeList[i], winChips.ToString());                            
                            break;
                        }
                    }
                }
            }
            Console.WriteLine("\n");
        }

        /// <summary>
        /// 發送邊池結果
        /// </summary>
        private void SendSidePotReult()
        {
            Dictionary<string, int> sideCompetitorDic = new Dictionary<string, int>();
            int mminSideValue = allInDic.Values.Min();

            foreach (var allIn in allInDic)
            {
                sideCompetitorDic.Add(allIn.Key, roomState.pokerShape[allIn.Key]);
                string backChips = (allIn.Value - mminSideValue).ToString();
                if (!sideWinInfo.sideBackChips.ContainsKey(allIn.Key))
                {
                    sideWinInfo.sideBackChips.Add(allIn.Key, backChips);
                }                
                Console.WriteLine($"邊池退回:{allIn.Key}/{backChips}");

                if (allIn.Key == computerName)
                {
                    ComputerInfo.Chips = Utils.StringAddition(ComputerInfo.Chips, backChips);
                }
                else
                {
                    Client c = clientList.Where(user => user.UserInfo.NickName == allIn.Key).FirstOrDefault();
                    ModifyUserCashData(c, backChips);
                }
            }

            List<string> aidePotWinner = ComparingPoker(sideCompetitorDic, sideCompetitorDic.Values.Min());

            int minSideValue = allInDic.Values.Min();
            int getSideValue = (minSideValue - Convert.ToInt32(roomState.currBet)) * (aidePotWinner.Count() - 1);
            Console.Write($"獲得籌碼:{getSideValue}/邊池獲勝者:");
            for (int i = 0; i < aidePotWinner.Count; i++)
            {
                Console.Write($"{aidePotWinner[i]} > ");
                if (aidePotWinner[i] == computerName)
                {
                    //電腦獲勝
                    ComputerInfo.Chips = Utils.StringAddition(ComputerInfo.Chips, getSideValue.ToString());
                }
                else
                {
                    //玩家獲勝
                    Client c = clientList.Where(user => user.UserInfo.NickName == aidePotWinner[i]).FirstOrDefault();
                    ModifyUserCashData(c, getSideValue.ToString());
                    if (!sideWinInfo.sideWinner.ContainsKey(aidePotWinner[i]))
                    {
                        sideWinInfo.sideWinner.Add(aidePotWinner[i], getSideValue.ToString());
                    }                    
                }
            }
            Console.WriteLine("\n");

            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.SidePotResult;
            pack.SideWinPack = GetSideWinPack();
            pack.GameProcessPack = GetGameProcessPack();
            pack.ComputerPack = GetComputerPack();
            Broadcast(null, pack);
        }

        /// <summary>
        /// 修改用戶金幣
        /// </summary>
        /// <param name="c"></param>
        /// <param name="addValue"></param>
        private void ModifyUserCashData(Client c, string addValue)
        {
            Dictionary<string, string> dataDic = c.GetMySql.GetData(c.GetMySqlConnection,
                                                                    "userdata", "account",
                                                                    c.UserInfo.Account,
                                                                    new string[] { "cash" }
                                                                    );

            if (dataDic != null)
            {
                string cash = Utils.StringAddition(dataDic["cash"], addValue);
                //修改資料庫金幣
                bool result = c.GetMySql.ReviseData(c.GetMySqlConnection,
                                                    "userdata",
                                                    "account",
                                                    c.UserInfo.Account,
                                                    new string[] { "cash" },
                                                    new string[] { cash }
                                                    );
                c.UserInfo.Chips = Utils.StringAddition(c.UserInfo.Chips, addValue);

                dataDic = c.GetMySql.GetData(c.GetMySqlConnection,
                                             "userdata", "account",
                                             c.UserInfo.Account,
                                             new string[] { "cash" }
                                             );
                c.UserInfo.Cash = dataDic["cash"];
            }     
            else
            {
                Console.WriteLine("修改資料庫金幣錯誤!!!");
            }
        }

        /// <summary>
        /// 發送遊戲結果
        /// </summary>
        async private void SendGameResult()
        {
            roomState.gameProcess = GameProcess.GameResult;
            JudgeGameResult();   
            BroadcastGameStage();

            //有邊池
            if (allInDic.Count() > 1)
            {
                await Task.Delay(1000);

                SendSidePotReult();                
            }

            await Task.Delay(1000);

            ForcedExit();

            await Task.Delay(3000);

            StartGame();
        }

        /// <summary>
        /// 強制離場判斷
        /// </summary>
        private void ForcedExit()
        {
            foreach (var user in clientList)
            {
                if (Convert.ToInt64(user.UserInfo.Chips) < Convert.ToInt32(roomState.bigBlindValue) * 3)
                {
                    //籌碼不足強制離場(不足大盲 * 3)
                    user.UserInfo.GameState = UserGameState.StateNone;

                    MainPack pack = new MainPack();
                    pack.ActionCode = ActionCode.ForcedExit;
                    user.Send(pack);

                    Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        pack.ActionCode = ActionCode.ExitRoom;
                        user.Send(pack);
                        server.ExitRoom(user, pack);
                    });
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
