using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TexasHoldemProtobuf;

namespace HotFix_Project
{
    class FX_HoldemGameView :FX_BaseView
    {
        private static FX_BaseView thisView;

        private static Button Exit_Btn, Abort_Btn, Pass_Btn, Follow_Btn, Add_Btn;
        private static Transform Seat_Tr, BetChips_Tr, BetChipsSample, PointTarget, OperateButton_Tr;
        private static Text TotalBetChips_Txt, AddChips_Txt;
        private static Slider Add_Sl;
        private static Image[] pokersPool = new Image[5];

        private static Sprite[] avatarSpriteList;
        private static Sprite[] pokerSpiteList;

        private static int computerSeat = 1;
        private static string computerName = "-1";
        
        private static string localUserName;
        private static string currBetValue;

        /// <summary>
        /// 用戶訊息(座位, (頭像,暱稱,籌碼,遮罩,倒數, 行動文字))
        /// </summary>
        private static Dictionary<int, (Image, Text, Text, Image, Text, Text)> userInfoDic = new Dictionary<int, (Image, Text, Text, Image, Text, Text)>();

        /// <summary>
        /// 用戶位置(暱稱, 座位)
        /// </summary>
        private static Dictionary<string, int> seatDic = new Dictionary<string, int>();

        /// <summary>
        /// 手牌物件(座位, (父物件,手牌1,手牌2,蓋牌圖片物件))
        /// </summary>
        private static Dictionary<int, (Transform, Image, Image, Transform)> handPokerDic = new Dictionary<int, (Transform, Image, Image, Transform)>();

        /// <summary>
        /// 下注籌碼(座位, (下注籌碼物件, 下注籌碼文字))
        /// </summary>
        private static Dictionary<int, (Transform, Text)> betShipsDic = new Dictionary<int, (Transform, Text)>();

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Exit_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Exit_Btn");
            Abort_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Abort_Btn");
            Pass_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Pass_Btn");
            Follow_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Follow_Btn");
            Add_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Add_Btn");
            Add_Sl = FindConponent.FindObj<Slider>(thisView.view.transform, "Add_Sl");
            BetChips_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "BetChips_Tr");
            BetChipsSample = FindConponent.FindObj<Transform>(thisView.view.transform, "BetChipsSample");
            OperateButton_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "OperateButton_Tr");
            PointTarget = FindConponent.FindObj<Transform>(thisView.view.transform, "PointTarget");
            TotalBetChips_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "TotalBetChips_Txt");
            AddChips_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "AddChips_Txt");
            Seat_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "Seat_Tr");

            for (int i = 0; i < Seat_Tr.childCount; i++)
            {
                //用戶訊息
                Image Avatar_Img = FindConponent.FindObj<Image>(Seat_Tr.GetChild(i), "Avatar_Img");
                Text NickName_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "NickName_Txt");
                Text Chips_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "Chips_Txt");
                Image Mask_Img = FindConponent.FindObj<Image>(Seat_Tr.GetChild(i), "Mask_Img");
                Text CountDown_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "CountDown_Txt");
                Text Action_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "Action_Txt");
                userInfoDic.Add(i, (Avatar_Img, NickName_Txt, Chips_Txt, Mask_Img, CountDown_Txt, Action_Txt));

                //手牌
                Transform Poker_Tr = FindConponent.FindObj<Transform>(Seat_Tr.GetChild(i), "Poker_Tr");
                Image Poker1 = FindConponent.FindObj<Image>(Poker_Tr, "Poker1");
                Image Poker2 = FindConponent.FindObj<Image>(Poker_Tr, "Poker2");
                Transform Fold_Img = FindConponent.FindObj<Transform>(Poker_Tr, "Fold_Img");
                handPokerDic.Add(i, (Poker_Tr, Poker1, Poker2, Fold_Img));

                Transform BetShips_Tr = FindConponent.FindObj<Transform>(Seat_Tr.GetChild(i), "BetShips_Tr");
                Text BetShips_Txt = FindConponent.FindObj<Text>(BetShips_Tr, "BetShips_Txt");
                betShipsDic.Add(i, (BetShips_Tr, BetShips_Txt));
            }

            for (int i = 0; i < pokersPool.Length; i++)
            {
                pokersPool[i] = FindConponent.FindObj<Image>(thisView.view.transform, $"PokerPool{i + 1}_Img");
            }

            //圖像資源
            ABManager.Instance.LoadSprite("entry", "AvatarList", (avatars) =>
            {
                avatarSpriteList = avatars;
            });
            ABManager.Instance.LoadSprite("holdem", "PokerList", (poker) =>
            {
                pokerSpiteList = poker;
            });
        }

        private static void OnEnable()
        {
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.User;
            pack.ActionCode = ActionCode.GetUserInfo;
            thisView.view.SendRequest(pack);

            //初始狀態
            BetChipsSample.gameObject.SetActive(false);
            OperateButton_Tr.gameObject.SetActive(false);
            foreach (var item in handPokerDic.Values)
            {
                item.Item1.gameObject.SetActive(false);
            }
            foreach (var item in betShipsDic)
            {
                item.Value.Item1.gameObject.SetActive(false);
            }
            foreach (var item in userInfoDic)
            {
                item.Value.Item4.gameObject.SetActive(false);
                item.Value.Item5.text = "";                
                item.Value.Item6.text = "";
                item.Value.Item6.gameObject.SetActive(false);
            }
        }

        private static void Start()
        {
            //離開按鈕
            Exit_Btn.onClick.AddListener(() =>
            {
                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.Game;
                pack.ActionCode = ActionCode.ExitRoom;                

                thisView.view.SendRequest(pack);
            });

            //棄牌按鈕
            Abort_Btn.onClick.AddListener(() =>
            {
                SendUserGameAction(UserGameState.Abort, "0");
            });

            //過牌按鈕
            Pass_Btn.onClick.AddListener(() =>
            {
                SendUserGameAction(UserGameState.Pass, "0");
            });

            //跟注按鈕
            Follow_Btn.onClick.AddListener(() =>
            {
                SendUserGameAction(UserGameState.Follow, currBetValue);
            });

            //加注按鈕
            Add_Btn.onClick.AddListener(() =>
            {
                SendUserGameAction(UserGameState.Add, Add_Sl.value.ToString());
            });

            //加注滑條
            Add_Sl.onValueChanged.AddListener((value) =>
            {
                AddChips_Txt.text = value.ToString();
            });
        }

        /// <summary>
        /// 發送更新房間玩家訊息
        /// </summary>
        private static void SendUpdateRoomInfo()
        {
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Game;
            pack.ActionCode = ActionCode.UpdateRoomUserInfo;
            pack.SendModeCode = SendModeCode.RoomBroadcast;
            thisView.view.SendRequest(pack);
        }

        /// <summary>
        /// 發送玩家遊戲行動
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="betValue"></param>
        private static void SendUserGameAction(UserGameState gameState, string betValue)
        {
            OperateButton_Tr.gameObject.SetActive(false);

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Game;
            pack.ActionCode = ActionCode.UserGameAction;

            GameActionPack gameActionPack = new GameActionPack();
            gameActionPack.UserGameState = gameState;
            gameActionPack.BetValue = betValue;

            pack.GameActionPack = gameActionPack;
            thisView.view.SendRequest(pack);
        }

        /// <summary>
        /// 更新電腦玩家
        /// </summary>
        /// <param name="computerPack"></param>
        private static void UpdateComputer(ComputerPack computerPack)
        {
            betShipsDic[computerSeat].Item1.gameObject.SetActive(true);

            userInfoDic[computerSeat].Item1.sprite = avatarSpriteList[Convert.ToInt32(computerPack.Avatar)];
            userInfoDic[computerSeat].Item2.text = computerPack.NickName;

            userInfoDic[computerSeat].Item3.text = computerPack.Chips;
            betShipsDic[computerSeat].Item2.text = computerPack.BetChips;
        }

        /// <summary>
        /// 遊戲階段
        /// </summary>
        /// <param name="pack"></param>
        private static void GameStage(MainPack pack)
        {
            TotalBetChips_Txt.text = $"總籌碼:{pack.GameProcessPack.TotalBetChips}";
            currBetValue = pack.GameProcessPack.CurrBet;

            //下注籌碼
            foreach (var betChips in pack.GameProcessPack.BetShips)
            {
                int seat = betChips.Key == computerName ? computerSeat : seatDic[betChips.Key];
                betShipsDic[seat].Item1.gameObject.SetActive(true);
                betShipsDic[seat].Item2.text = betChips.Value;
            }

            //籌碼
            foreach (var chips in pack.GameProcessPack.UserChips)
            {
                int seat = chips.Key == computerName ? computerSeat : seatDic[chips.Key];
                userInfoDic[seat].Item3.text = chips.Value;
            }

            UpdateComputer(pack.ComputerPack);

            switch (pack.GameProcessPack.GameProcess)
            {                
                //選擇大小盲
                case GameProcess.SetBlind:
                    int smallSeat = 0, bigSeat = 0;
                    //小盲
                    if (seatDic.ContainsKey(pack.GameProcessPack.SmallBlinder))
                    {
                        smallSeat = seatDic[pack.GameProcessPack.SmallBlinder];
                    }
                    else
                    {
                        if(pack.GameProcessPack.SmallBlinder == computerName)
                        {
                            smallSeat = computerSeat;
                        }
                    }
                    //大盲
                    if (seatDic.ContainsKey(pack.GameProcessPack.BigBlinder))
                    {
                        bigSeat = seatDic[pack.GameProcessPack.BigBlinder];
                    }
                    else
                    {
                        if (pack.GameProcessPack.BigBlinder == computerName)
                        {
                            bigSeat = computerSeat;
                        }
                    }

                    int smallBlindVluue = Convert.ToInt32(pack.GameProcessPack.BigBlindValue) / 2;
                    int bigBlindVluue = Convert.ToInt32(pack.GameProcessPack.BigBlindValue);
                    CreateBetShips(smallSeat, smallBlindVluue.ToString());
                    CreateBetShips(bigSeat, bigBlindVluue.ToString());
                    break;

                //翻牌前
                case GameProcess.Preflop:
                    //手牌
                    foreach (var user in pack.GameProcessPack.HandPoker)
                    {
                        if (handPokerDic.ContainsKey(seatDic[user.Key]))
                        {
                            handPokerDic[seatDic[user.Key]].Item1.gameObject.SetActive(true);

                            if (user.Key == localUserName)
                            {
                                //本地玩家
                                int[] handPoker = user.Value.Values.ToArray();
                                
                                handPokerDic[seatDic[user.Key]].Item2.sprite = pokerSpiteList[handPoker[0]];
                                handPokerDic[seatDic[user.Key]].Item3.sprite = pokerSpiteList[handPoker[1]];
                                handPokerDic[seatDic[user.Key]].Item4.gameObject.SetActive(false);
                            }
                            else
                            {
                                //其他用戶
                                handPokerDic[seatDic[user.Key]].Item2.gameObject.SetActive(false);
                                handPokerDic[seatDic[user.Key]].Item3.gameObject.SetActive(false);
                                handPokerDic[seatDic[user.Key]].Item4.gameObject.SetActive(true);                                
                            }
                        }

                        //電腦
                        handPokerDic[computerSeat].Item1.gameObject.SetActive(true);
                        handPokerDic[computerSeat].Item2.gameObject.SetActive(false);
                        handPokerDic[computerSeat].Item3.gameObject.SetActive(false);
                        handPokerDic[computerSeat].Item4.gameObject.SetActive(true);
                    }

                    SetOperateButton(pack);
                    break;

                //翻牌
                case GameProcess.Flop:
                    for (int i = 0; i < 3; i++)
                    {
                        int result = pack.GameProcessPack.Result[i];
                        pokersPool[i].sprite = pokerSpiteList[result];
                    }
                    break;

                //轉排
                case GameProcess.Turn:
                    break;

                //河牌
                case GameProcess.River:
                    break;
            }
        }

        /// <summary>
        /// 設定操作按鈕
        /// </summary>
        private static void SetOperateButton(MainPack pack)
        {
            OperateButton_Tr.gameObject.SetActive(pack.GameProcessPack.ActionUser == localUserName);

            string maxBetStr = pack.GameProcessPack.BetShips.Values.Max();

            int currBet = Convert.ToInt32(pack.GameProcessPack.CurrBet);
            int selfBet = Convert.ToInt32(betShipsDic[seatDic[localUserName]].Item2);

            Abort_Btn.gameObject.SetActive(true);
            Pass_Btn.gameObject.SetActive(selfBet == currBet);
            Follow_Btn.gameObject.SetActive(currBet > 0);
            Add_Btn.gameObject.SetActive(true);

            string minAddValue = pack.GameProcessPack.CurrBet == "0" ? 
                                 pack.GameProcessPack.BigBlindValue : 
                                 Utils.StringAddition(pack.GameProcessPack.TotalBetChips, pack.GameProcessPack.BigBlindValue);
            Add_Sl.minValue = float.Parse(minAddValue);
            Add_Sl.maxValue = float.Parse(userInfoDic[seatDic[localUserName]].Item3.text);
        }

        /// <summary>
        /// 產生下注籌碼
        /// </summary>
        /// <param name="seat"></param>
        /// <param name="shipsValus"></param>
        private static void CreateBetShips(int seat, string shipsValus)
        {
            Transform obj = GameObject.Instantiate(BetChipsSample);
            obj.SetParent(BetChips_Tr);
            obj.position = Seat_Tr.GetChild(seat).position;
            obj.gameObject.SetActive(true);
            obj.GetComponent<BetChipsAction>().SetChipsValue(shipsValus, PointTarget);
        }

        /// <summary>
        /// 處理協議
        /// </summary>
        /// <param name="pack"></param>
        private static void HandleRequest(MainPack pack)
        {
            switch (pack.ActionCode)
            {
                //離開
                case ActionCode.ExitRoom:
                    UIManager.Instance.ShowLoadingView(ViewType.LobbyView, true);
                    break;

                //更新用戶訊息
                case ActionCode.GetUserInfo:
                    localUserName = pack.UserInfoPack[0].NickName;
                    SendUpdateRoomInfo();
                    break;
            }
        }

        /// <summary>
        /// 處理廣播訊息
        /// </summary>
        /// <param name="pack"></param>
        private static void HandleBroadcast(MainPack pack)
        {
            switch (pack.ActionCode)
            {
                //更新房間訊息
                case ActionCode.UpdateRoomUserInfo:
                    //玩家
                    foreach (var user in pack.UserInfoPack)
                    {
                        if (!seatDic.ContainsKey(user.NickName))
                        {
                            //座位
                            int seat = 0;
                            if (user.NickName != localUserName)
                            {
                                for (int i = 2; i < userInfoDic.Count; i++)
                                {
                                    if (!seatDic.ContainsValue(i))
                                    {
                                        seat = i;
                                        break;
                                    }
                                }
                            }

                            seatDic.Add(user.NickName, seat);
                            userInfoDic[seat].Item1.sprite = avatarSpriteList[Convert.ToInt32(user.Avatar)];
                            userInfoDic[seat].Item2.text = user.NickName;
                            userInfoDic[seat].Item3.text = user.Chips;
                        }
                    }

                    //電腦玩家
                    if (!seatDic.ContainsKey(computerName))
                    {
                        UpdateComputer(pack.ComputerPack);                       
                        seatDic.Add(computerName, computerSeat);
                    }
                    

                    //發起開始遊戲指令                    
                    if (pack.UserInfoPack.Count == 1)
                    {
                        pack = new MainPack();
                        pack.RequestCode = RequestCode.Game;
                        pack.ActionCode = ActionCode.StartGame;
                        pack.SendModeCode = SendModeCode.RoomBroadcast;

                        thisView.view.SendRequest(pack);
                    }
                    break;

                //其他用戶離開房間
                case ActionCode.OtherUserExitRoom:
                    if (seatDic.ContainsKey(pack.UserInfoPack[0].NickName))
                    {
                        int seat = seatDic[pack.UserInfoPack[0].NickName];
                        userInfoDic[seat].Item1.sprite = null;
                        userInfoDic[seat].Item2.text = "";
                        userInfoDic[seat].Item3.text = "";
                        seatDic.Remove(pack.UserInfoPack[0].NickName);
                    }
                    break;

                //遊戲階段
                case ActionCode.GameStage:
                    GameStage(pack);
                    break;

                //更新玩家行動
                case ActionCode.UserGameAction:
                    string betValue = pack.GameActionPack.BetValue;
                    string nickName = pack.GameActionPack.ActionNickName;
                    string totalBet = pack.GameProcessPack.TotalBetChips;
                    string showActionStr = "";
                    switch (pack.GameActionPack.UserGameState)
                    {
                        case UserGameState.StateNone:
                            break;
                        case UserGameState.Abort:
                            showActionStr = "棄牌";
                            userInfoDic[seatDic[nickName]].Item4.gameObject.SetActive(true);
                            userInfoDic[seatDic[nickName]].Item4.fillAmount = 1;
                            userInfoDic[seatDic[nickName]].Item5.text = "棄牌";
                            break;
                        case UserGameState.Pass:
                            showActionStr = "過牌";
                            userInfoDic[seatDic[nickName]].Item4.gameObject.SetActive(true);
                            userInfoDic[seatDic[nickName]].Item4.fillAmount = 1;
                            userInfoDic[seatDic[nickName]].Item5.text = "過牌";
                            break;
                        case UserGameState.Follow:
                            showActionStr = "跟注";
                            CreateBetShips(seatDic[nickName], betValue);
                            break;
                        case UserGameState.Add:
                            showActionStr = "加注";
                            CreateBetShips(seatDic[nickName], betValue);
                            break;
                        default:
                            break;
                    }
                    userInfoDic[seatDic[nickName]].Item6.gameObject.SetActive(true);
                    userInfoDic[seatDic[nickName]].Item6.text = showActionStr;
                    foreach (var user in pack.GameProcessPack.UserChips)
                    {
                        userInfoDic[seatDic[user.Key]].Item3.text = user.Value;
                    }
                    TotalBetChips_Txt.text = totalBet;
                    UpdateComputer(pack.ComputerPack);
                    break;
            }
        }
    }
}
