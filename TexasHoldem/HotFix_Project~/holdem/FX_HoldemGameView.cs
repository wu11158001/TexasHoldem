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

        private static Button Exit_Btn, Abort_Btn, Pass_Btn, Follow_Btn, Add_Btn, Setting_Btn, AllIn_Btn;
        private static Transform Seat_Tr, BetChips_Tr, BetChipsSample, PointTarget, OperateButton_Tr, AddBetValue_Tr, SidePot_Tr;
        private static Text TotalBetChips_Txt, AddChips_Txt, Add_Txt, Tip_Txt, Follow_Txt, SidePot_Txt, SideBackSample_Txt;
        private static Slider Add_Sl;
        private static Image[] pokersPool = new Image[5];
        private static Button[] betPercentButtons = new Button[4];

        private static Sprite[] avatarSpriteList;
        private static Sprite[] pokerSpiteList;

        private static int computerSeat = 1;
        private static string computerName = "-1";

        private static string localUserName;
        private static string currBetValue;
        private static bool isGetUserInfo;
        private static bool isAbort;
        private static float minBetValue;
        private static float maxBetValue;

        /// <summary>
        /// 用戶訊息(座位, (1頭像, 2暱稱, 3籌碼, 4遮罩, 5倒數, 6行動文字, 7贏家文字, 8玩家籌碼物件))
        /// </summary>
        private static Dictionary<int, (Image, Text, Text, Image, Text, Text, Text, Transform)> userInfoDic = new Dictionary<int, (Image, Text, Text, Image, Text, Text, Text, Transform)>();

        /// <summary>
        /// 用戶位置(1暱稱, 2座位)
        /// </summary>
        private static Dictionary<string, int> seatDic = new Dictionary<string, int>();

        /// <summary>
        /// 手牌物件(座位, (1父物件, 2手牌1, 3手牌2, 4蓋牌圖片物件, 5牌行文字))
        /// </summary>
        private static Dictionary<int, (Transform, Image, Image, Transform, Text)> handPokerDic = new Dictionary<int, (Transform, Image, Image, Transform, Text)>();

        /// <summary>
        /// 下注籌碼(座位, (1下注籌碼物件, 2下注籌碼文字))
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
            AllIn_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "AllIn_Btn");
            Setting_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Setting_Btn");
            Add_Sl = FindConponent.FindObj<Slider>(thisView.view.transform, "Add_Sl");
            BetChips_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "BetChips_Tr");
            BetChipsSample = FindConponent.FindObj<Transform>(thisView.view.transform, "BetChipsSample");
            OperateButton_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "OperateButton_Tr");
            AddBetValue_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "AddBetValue_Tr");
            PointTarget = FindConponent.FindObj<Transform>(thisView.view.transform, "PointTarget");
            TotalBetChips_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "TotalBetChips_Txt");
            AddChips_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "AddChips_Txt");
            Add_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Add_Txt");
            Tip_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Tip_Txt");
            Follow_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Follow_Txt");
            SidePot_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "SidePot_Txt");
            SideBackSample_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "SideBackSample_Txt");
            Seat_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "Seat_Tr");
            SidePot_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "SidePot_Tr");

            //下注百分比按鈕
            betPercentButtons[0] =FindConponent.FindObj<Button>(thisView.view.transform, "100Percent_Btn");
            betPercentButtons[1] = FindConponent.FindObj<Button>(thisView.view.transform, "75Percent_Btn");
            betPercentButtons[2] = FindConponent.FindObj<Button>(thisView.view.transform, "50Percent_Btn");
            betPercentButtons[3] = FindConponent.FindObj<Button>(thisView.view.transform, "25Percent_Btn");

            for (int i = 0; i < Seat_Tr.childCount; i++)
            {
                //用戶訊息
                Image Avatar_Img = FindConponent.FindObj<Image>(Seat_Tr.GetChild(i), "Avatar_Img");
                Text NickName_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "NickName_Txt");
                Text Chips_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "Chips_Txt");
                Image Mask_Img = FindConponent.FindObj<Image>(Seat_Tr.GetChild(i), "Mask_Img");
                Text CountDown_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "CountDown_Txt");
                Text Action_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "Action_Txt");
                Text Win_Txt = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "Win_Txt");
                Transform UserChips_Tr = FindConponent.FindObj<Transform>(Seat_Tr.GetChild(i), "UserChips_Tr");
                userInfoDic.Add(i, (Avatar_Img, NickName_Txt, Chips_Txt, Mask_Img, CountDown_Txt, Action_Txt, Win_Txt, UserChips_Tr));

                //手牌
                Transform Poker_Tr = FindConponent.FindObj<Transform>(Seat_Tr.GetChild(i), "Poker_Tr");
                Image Poker1 = FindConponent.FindObj<Image>(Poker_Tr, "Poker1");
                Image Poker2 = FindConponent.FindObj<Image>(Poker_Tr, "Poker2");
                Transform Fold_Img = FindConponent.FindObj<Transform>(Poker_Tr, "Fold_Img");
                Text CardShape_Txt = FindConponent.FindObj<Text>(Poker_Tr, "CardShape_Txt");
                handPokerDic.Add(i, (Poker_Tr, Poker1, Poker2, Fold_Img, CardShape_Txt));

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
            seatDic.Clear();
            Tip_Txt.text = "等待下一局...";
            TotalBetChips_Txt.text = "0";
            isGetUserInfo = false;

            foreach (var user in userInfoDic)
            {
                user.Value.Item1.gameObject.SetActive(false);
                user.Value.Item2.text = "";
                user.Value.Item3.text = "";
                user.Value.Item8.gameObject.SetActive(false);
            }

            GameInit();

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.User;
            pack.ActionCode = ActionCode.GetUserInfo;
            AddBetValue_Tr.gameObject.SetActive(false);
            thisView.view.SendRequest(pack);
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

            //設置按鈕
            Setting_Btn.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowToolView(ViewType.SettingView);
            });

            //棄牌按鈕
            Abort_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();

                isAbort = true;
                SendUserGameAction(UserGameState.Abort, betShipsDic[seatDic[localUserName]].Item2.text.Replace(",", ""));
            });

            //過牌按鈕
            Pass_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();

                SendUserGameAction(UserGameState.Pass, currBetValue);
            });

            //跟注按鈕
            Follow_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();
                SendUserGameAction(UserGameState.Follow, currBetValue);
            });

            //加注按鈕
            Add_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();
                if (AddBetValue_Tr.gameObject.activeSelf)
                {
                    SendUserGameAction(UserGameState.Add, Add_Sl.value.ToString());                    
                }
                else
                {
                    Add_Txt.text = "確定";
                    AddBetValue_Tr.gameObject.SetActive(true);
                }
            });

            //All In按鈕
            AllIn_Btn.onClick.AddListener(() =>
            {
                SendUserGameAction(UserGameState.AllIn, maxBetValue.ToString());
            });

            //加注滑條
            Add_Sl.onValueChanged.AddListener((value) =>
            {
                AddChips_Txt.text = Utils.Instance.SetChipsStr(value.ToString());
            });

            //加註百分比按鈕
            betPercentButtons[0].onClick.AddListener(() =>
            {
                Add_Sl.value = minBetValue + ((maxBetValue - minBetValue) * 1);
            });
            betPercentButtons[1].onClick.AddListener(() =>
            {
                Add_Sl.value = minBetValue + ((maxBetValue - minBetValue) * 0.75f);
            });
            betPercentButtons[2].onClick.AddListener(() =>
            {
                Add_Sl.value = minBetValue + ((maxBetValue - minBetValue) * 0.5f);
            });
            betPercentButtons[3].onClick.AddListener(() =>
            {
                Add_Sl.value = minBetValue + ((maxBetValue - minBetValue) * 0.25f);
            });
        }

        /// <summary>
        /// 遊戲重製
        /// </summary>
        private static void GameInit()
        {
            isAbort = false;
            BetChipsSample.gameObject.SetActive(false);
            SideBackSample_Txt.gameObject.SetActive(false);
            OperateButton_Tr.gameObject.SetActive(false);
            SidePot_Tr.gameObject.SetActive(false);
            SidePot_Txt.text = "";

            foreach (var item in handPokerDic.Values)
            {
                item.Item1.gameObject.SetActive(false);
                item.Item5.gameObject.SetActive(false);
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
                item.Value.Item7.gameObject.SetActive(false);
            }

            for (int i = 0; i < pokersPool.Length; i++)
            {
                pokersPool[i].sprite = null;
            }
        }

        /// <summary>
        /// 發送更新房間玩家訊息
        /// </summary>
        private static void SendUpdateRoomUserInfo()
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
            if (float.Parse(betValue) == maxBetValue)
            {
                gameState = UserGameState.AllIn;
            }

            Add_Txt.text = "加注";
            OperateButton_Tr.gameObject.SetActive(false);
            AddBetValue_Tr.gameObject.SetActive(false);

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Game;
            pack.ActionCode = ActionCode.ShowUserAction;

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

            userInfoDic[computerSeat].Item1.gameObject.SetActive(true);
            userInfoDic[computerSeat].Item1.sprite = avatarSpriteList[Convert.ToInt32(computerPack.Avatar)];
            userInfoDic[computerSeat].Item2.text = computerPack.NickName;
            Utils.Instance.ChipsChangeEffect(userInfoDic[computerSeat].Item3, computerPack.Chips);
            userInfoDic[computerSeat].Item8.gameObject.SetActive(true);

            Utils.Instance.ChipsChangeEffect(betShipsDic[computerSeat].Item2, computerPack.BetChips);
        }


        /// <summary>
        /// 設定操作按鈕
        /// </summary>
        private static void SetOperateButton(MainPack pack)
        {
            if (pack.ActionerPack.Actioner != localUserName || OperateButton_Tr.gameObject.activeSelf || isAbort) return;

            OperateButton_Tr.gameObject.SetActive(true);

            int currBet = Convert.ToInt32(pack.GameProcessPack.CurrBet);
            int selfBet = Convert.ToInt32(betShipsDic[seatDic[localUserName]].Item2.text.Replace(",", ""));

            minBetValue = float.Parse(pack.GameProcessPack.MinBetValue);
            maxBetValue = float.Parse(Utils.Instance.StringAddition(userInfoDic[seatDic[localUserName]].Item3.text.Replace(",", ""),
                                                                    betShipsDic[seatDic[localUserName]].Item2.text.Replace(",", "")));


            Abort_Btn.gameObject.SetActive(true);
            Pass_Btn.gameObject.SetActive(selfBet == currBet);
            Follow_Btn.gameObject.SetActive(selfBet < currBet && maxBetValue > minBetValue);
            Follow_Txt.text = $"跟注\n{currBet}";
            Add_Btn.gameObject.SetActive(maxBetValue > float.Parse(pack.GameProcessPack.CurrBet) && maxBetValue > minBetValue);
            AllIn_Btn.gameObject.SetActive(maxBetValue <= float.Parse(pack.GameProcessPack.CurrBet) || maxBetValue <= minBetValue);

            AddChips_Txt.text = pack.GameProcessPack.MinBetValue;
            Add_Sl.minValue = float.Parse(pack.GameProcessPack.MinBetValue);
            Add_Sl.maxValue = maxBetValue;
            Add_Sl.value = float.Parse(pack.GameProcessPack.MinBetValue);
        }

        /// <summary>
        /// 更新下注籌碼
        /// </summary>
        /// <param name="pack"></param>
        private static void UpdateBetShips(MainPack pack)
        {
            Utils.Instance.ChipsChangeEffect(TotalBetChips_Txt, pack.GameProcessPack.TotalBetChips);

            //下注籌碼
            foreach (var betChips in pack.GameProcessPack.BetShips)
            {
                int seat = betChips.Key == computerName ? computerSeat : seatDic[betChips.Key];
                betShipsDic[seat].Item1.gameObject.SetActive(true);
                Utils.Instance.ChipsChangeEffect(betShipsDic[seat].Item2, betChips.Value);
            }
            //擁有籌碼
            foreach (var chips in pack.GameProcessPack.UserChips)
            {
                int seat = chips.Key == computerName ? computerSeat : seatDic[chips.Key];
                Utils.Instance.ChipsChangeEffect(userInfoDic[seat].Item3, chips.Value);
            }
        }

        /// <summary>
        /// 產生籌碼物件
        /// </summary>
        /// <param name="seat"></param>
        /// <param name="shipsValus"></param>
        /// <param name="isBet">下注/獲勝籌碼</param>
        private static void CreateBetShips(int seat, string shipsValus, bool isBet = true)
        {
            Transform obj = GameObject.Instantiate(BetChipsSample);
            obj.SetParent(BetChips_Tr);
            obj.localScale = new Vector3(1, 1, 1);
            obj.GetComponent<RectTransform>().eulerAngles = Vector3.zero;           
            obj.gameObject.SetActive(true);
            obj.localPosition = isBet ? Seat_Tr.GetChild(seat).localPosition : PointTarget.localPosition;
            Transform target = isBet ? PointTarget : Seat_Tr.GetChild(seat);
            obj.GetComponent<BetChipsAction>().SetChipsValue(shipsValus, target);
        }

        /// <summary>
        /// 產生邊池結果
        /// </summary>
        /// <param name="nickName"></param>
        /// <param name="result"></param>
        private static void CreateSideResult(string nickName, string result)
        {
            Transform obj = GameObject.Instantiate(SideBackSample_Txt.transform);
            obj.SetParent(BetChips_Tr);
            obj.localScale = new Vector3(1, 1, 1);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.eulerAngles = Vector3.zero;
            obj.gameObject.SetActive(true);
            obj.localPosition = Seat_Tr.GetChild(seatDic[nickName]).localPosition;
            obj.GetComponent<SideBackAction>().SetSideBackInfo(result);
        }

        /// <summary>
        /// 設定牌池撲克
        /// </summary>
        /// <param name="result"></param>
        /// <param name="showCount"></param>
        private static void SetPoolPoker(List<int> result, int showCount)
        {
            //牌池                    
            for (int i = 0; i < showCount; i++)
            {
                int poker = result[i];
                pokersPool[i].sprite = pokerSpiteList[poker];
            }
        }

        /// <summary>
        /// 遊戲階段
        /// </summary>
        /// <param name="pack"></param>
        private static void GameStage(MainPack pack)
        {            
            currBetValue = pack.GameProcessPack.CurrBet;

            foreach (var user in pack.GameProcessPack.UserStates)
            {
                if (user.Value != UserGameState.Abort)
                {
                    userInfoDic[seatDic[user.Key]].Item4.gameObject.SetActive(false);

                    if (user.Value != UserGameState.AllIn)
                    {
                        userInfoDic[seatDic[user.Key]].Item6.text = "";
                    }
                }
                userInfoDic[seatDic[user.Key]].Item5.text = "";
            }
            userInfoDic[seatDic[computerName]].Item6.text = "";

            int[] handPoker;

            //用戶牌型
            foreach (var poker in pack.GameProcessPack.PokerShape)
            {
                if (handPokerDic.ContainsKey(seatDic[poker.Key]))
                {
                    handPokerDic[seatDic[poker.Key]].Item5.text = poker.Value;
                }                
            }

            switch (pack.GameProcessPack.GameProcess)
            {                
                //選擇大小盲
                case GameProcess.SetBlind:
                    Debug.Log("選擇大小盲");
                    Tip_Txt.text = "";
                    GameInit();

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

                    //開啟下注籌碼
                    foreach (var betChips in pack.GameProcessPack.BetShips)
                    {
                        int seat = betChips.Key == computerName ? computerSeat : seatDic[betChips.Key];
                        betShipsDic[seat].Item1.gameObject.SetActive(true);
                    }
                    betShipsDic[seatDic[computerName]].Item1.gameObject.SetActive(true);

                    int smallBlindVluue = Convert.ToInt32(pack.GameProcessPack.BigBlindValue) / 2;
                    int bigBlindVluue = Convert.ToInt32(pack.GameProcessPack.BigBlindValue);

                    AudioManager.Instance.PlaySound("ChipsMove");
                    CreateBetShips(smallSeat, smallBlindVluue.ToString());
                    CreateBetShips(bigSeat, bigBlindVluue.ToString());
                    break;

                //翻牌前
                case GameProcess.Preflop:
                    Debug.Log("翻牌前");
                    //手牌
                    foreach (var user in pack.GameProcessPack.HandPoker)
                    {
                        if (handPokerDic.ContainsKey(seatDic[user.Key]))
                        {
                            handPokerDic[seatDic[user.Key]].Item1.gameObject.SetActive(true);

                            if (user.Key == localUserName)
                            {
                                //本地玩家
                                handPoker = user.Value.Values.ToArray();
                                handPokerDic[seatDic[user.Key]].Item2.sprite = pokerSpiteList[handPoker[0]];
                                handPokerDic[seatDic[user.Key]].Item3.sprite = pokerSpiteList[handPoker[1]];
                                handPokerDic[seatDic[user.Key]].Item4.gameObject.SetActive(false);
                                handPokerDic[seatDic[user.Key]].Item5.gameObject.SetActive(true);
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
                    break;

                //翻牌
                case GameProcess.Flop:
                    Debug.Log("翻牌");
                    SetPoolPoker(pack.GameProcessPack.Result.ToList(), 3);                                
                    break;

                //轉排
                case GameProcess.Turn:
                    Debug.Log("轉排");
                    SetPoolPoker(pack.GameProcessPack.Result.ToList(), 4);

                    //用戶牌型
                    foreach (var user in pack.GameProcessPack.HandPoker)
                    {
                        if (user.Key == localUserName)
                        {
                            handPokerDic[seatDic[user.Key]].Item5.text = pack.GameProcessPack.PokerShape[user.Key];
                        }
                    }
                    break;

                //河牌
                case GameProcess.River:
                    Debug.Log("河牌");
                    SetPoolPoker(pack.GameProcessPack.Result.ToList(), 5);
                    break;

                //遊戲結果
                case GameProcess.GameResult:
                    Debug.Log($"遊戲結果");
                    SetPoolPoker(pack.GameProcessPack.Result.ToList(), 5);
                    AudioManager.Instance.PlaySound("ChipsMove");
                    foreach (var user in userInfoDic)
                    {
                        user.Value.Item6.text = "";
                    }
                    foreach (var winner in pack.GameProcessPack.Winners)
                    {
                        Debug.Log($"獲勝:{winner}/獲勝人數:{pack.GameProcessPack.Winners.Count()}人");
                        userInfoDic[seatDic[winner]].Item7.gameObject.SetActive(true);
                        userInfoDic[seatDic[winner]].Item7.text = "獲勝";
                        CreateBetShips(seatDic[winner], pack.GameProcessPack.WinChips, false);
                    }
                    //顯示手牌
                    foreach (var user in pack.GameProcessPack.HandPoker)
                    {
                        handPokerDic[seatDic[user.Key]].Item1.gameObject.SetActive(true);
                        handPokerDic[seatDic[user.Key]].Item2.gameObject.SetActive(true);
                        handPokerDic[seatDic[user.Key]].Item3.gameObject.SetActive(true);
                        handPokerDic[seatDic[user.Key]].Item4.gameObject.SetActive(false);

                        handPoker = user.Value.Values.ToArray();
                        handPokerDic[seatDic[user.Key]].Item2.sprite = pokerSpiteList[handPoker[0]];
                        handPokerDic[seatDic[user.Key]].Item3.sprite = pokerSpiteList[handPoker[1]];                        
                    }
                    foreach (var poker in pack.GameProcessPack.PokerShape)
                    {
                        handPokerDic[seatDic[poker.Key]].Item5.gameObject.SetActive(true);
                    }
                    foreach (var bet in betShipsDic)
                    {
                        bet.Value.Item1.gameObject.SetActive(false);
                    }
                    break;
            }
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
                    Debug.Log("離開房間");
                    UIManager.Instance.ShowLoadingView(ViewType.LobbyView, true);
                    break;

                //更新用戶訊息
                case ActionCode.GetUserInfo:
                    Debug.Log("更新用戶訊息");
                    localUserName = pack.UserInfoPack[0].NickName;
                    SendUpdateRoomUserInfo();
                    break;

                //強制離場
                case ActionCode.ForcedExit:
                    Tip_Txt.text = "籌碼不足，3秒後強制離場!!!";
                    break;
            }
        }

        /// <summary>
        /// 處理廣播訊息
        /// </summary>
        /// <param name="pack"></param>
        private static void HandleBroadcast(MainPack pack)
        {
            int seat = 0;
            switch (pack.ActionCode)
            {
                //更新房間訊息
                case ActionCode.UpdateRoomUserInfo:
                    Debug.Log("更新房間訊息");                    
                    //玩家
                    foreach (var user in pack.UserInfoPack)
                    {
                        if (!seatDic.ContainsKey(user.NickName))
                        {
                            //座位
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
                            else
                            {
                                seat = 0;
                            }

                            seatDic.Add(user.NickName, seat);
                            userInfoDic[seat].Item1.gameObject.SetActive(true);
                            userInfoDic[seat].Item1.sprite = avatarSpriteList[Convert.ToInt32(user.Avatar)];
                            userInfoDic[seat].Item2.text = user.NickName;
                            userInfoDic[seat].Item3.text = user.Chips;
                            userInfoDic[seat].Item8.gameObject.SetActive(true);
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

                    isGetUserInfo = true;
                    break;

                //其他用戶離開房間
                case ActionCode.OtherUserExitRoom:
                    if (seatDic.ContainsKey(pack.UserInfoPack[0].NickName))
                    {
                        seat = seatDic[pack.UserInfoPack[0].NickName];
                        userInfoDic[seat].Item1.gameObject.SetActive(false);
                        userInfoDic[seat].Item2.text = "";
                        userInfoDic[seat].Item3.text = "";
                        userInfoDic[seat].Item4.gameObject.SetActive(false);
                        userInfoDic[seat].Item8.gameObject.SetActive(false);
                        betShipsDic[seat].Item1.gameObject.SetActive(false);
                        handPokerDic[seat].Item4.gameObject.SetActive(false);
                        seatDic.Remove(pack.UserInfoPack[0].NickName);
                    }
                    break;

                //遊戲階段
                case ActionCode.GameStage:
                    if (!isGetUserInfo) return;

                    UpdateBetShips(pack);
                    UpdateComputer(pack.ComputerPack);
                    GameStage(pack);
                    break;

                //演出玩家行動
                case ActionCode.ShowUserAction:
                    if (!isGetUserInfo) return;

                    foreach (var user in userInfoDic)
                    {
                        user.Value.Item5.text = "";
                    }

                    currBetValue = pack.GameProcessPack.CurrBet;
                    string betValue = pack.GameActionPack.BetValue;
                    string actionUser = pack.GameActionPack.ActionNickName;
                    string showActionStr = "";
                    switch (pack.GameActionPack.UserGameState)
                    {
                        case UserGameState.StateNone:
                            break;
                        case UserGameState.Abort:
                            showActionStr = "棄牌";
                            userInfoDic[seatDic[actionUser]].Item4.gameObject.SetActive(true);
                            userInfoDic[seatDic[actionUser]].Item4.fillAmount = 1;
                            handPokerDic[seatDic[actionUser]].Item4.gameObject.SetActive(false);
                            break;

                        case UserGameState.Pass:
                            showActionStr = "過牌";
                            userInfoDic[seatDic[actionUser]].Item4.gameObject.SetActive(false);                            
                            break;

                        case UserGameState.Follow:
                            showActionStr = "跟注";
                            AudioManager.Instance.PlaySound("ChipsMove");
                            userInfoDic[seatDic[actionUser]].Item4.gameObject.SetActive(false);
                            CreateBetShips(seatDic[actionUser], betValue);
                            break;

                        case UserGameState.Add:
                            showActionStr = "加注";
                            AudioManager.Instance.PlaySound("ChipsMove");
                            userInfoDic[seatDic[actionUser]].Item4.gameObject.SetActive(false);
                            CreateBetShips(seatDic[actionUser], betValue);
                            break;

                        case UserGameState.AllIn:
                            showActionStr = "All In";
                            userInfoDic[seatDic[actionUser]].Item4.gameObject.SetActive(false);
                            AudioManager.Instance.PlaySound("ChipsMove");
                            CreateBetShips(seatDic[actionUser], betValue);
                            break;
                    }
                    Debug.Log($"{actionUser}:{showActionStr}");
                    
                    userInfoDic[seatDic[actionUser]].Item6.text = showActionStr;
                    if (actionUser == localUserName)
                    {
                        Tip_Txt.text = "";
                    }

                    //邊池
                    if (pack.GameProcessPack.SidePotValue != "" && 
                        pack.GameProcessPack.SidePotValue != "0")
                    {
                        SidePot_Tr.gameObject.SetActive(true);
                        Utils.Instance.ChipsChangeEffect(SidePot_Txt, pack.GameProcessPack.SidePotValue);
                    }

                    UpdateBetShips(pack);
                    UpdateComputer(pack.ComputerPack);
                    break;

                //行動者指令
                case ActionCode.ActionerOrder:
                    if (!isGetUserInfo) return;

                    TotalBetChips_Txt.text = $"{Utils.Instance.SetChipsStr(pack.GameProcessPack.TotalBetChips)}";
                    switch (pack.GameProcessPack.GameProcess)
                    {
                        case GameProcess.Flop:
                            SetPoolPoker(pack.GameProcessPack.Result.ToList(), 3);
                            break;
                        case GameProcess.Turn:
                            SetPoolPoker(pack.GameProcessPack.Result.ToList(), 4);
                            break;
                        case GameProcess.River:
                            SetPoolPoker(pack.GameProcessPack.Result.ToList(), 5);
                            break;
                        case GameProcess.GameResult:
                            SetPoolPoker(pack.GameProcessPack.Result.ToList(), 5);
                            break;
                        default:
                            break;
                    }
                    UpdateBetShips(pack);

                    seat = seatDic[pack.ActionerPack.Actioner];
                    SetOperateButton(pack);

                    foreach (var user in userInfoDic)
                    {
                        if (user.Key != seat)
                        {
                            user.Value.Item4.gameObject.SetActive(false);
                        }
                    }

                    userInfoDic[seat].Item4.gameObject.SetActive(true);
                    userInfoDic[seat].Item4.fillAmount = (8.0f - pack.ActionerPack.Countdown) / 8.0f;                   
                    userInfoDic[seat].Item5.text = pack.ActionerPack.Countdown.ToString("F0");
                    if (pack.ActionerPack.Actioner == localUserName && pack.ActionerPack.Countdown < 5)
                    {
                        Tip_Txt.text = "倒數結束棄牌!";
                    }

                    if (OperateButton_Tr.gameObject.activeSelf && pack.ActionerPack.Countdown <= 0)
                    {
                        OperateButton_Tr.gameObject.SetActive(false);
                        AddBetValue_Tr.gameObject.SetActive(false);
                        userInfoDic[seatDic[localUserName]].Item5.text = "";
                        userInfoDic[seatDic[localUserName]].Item6.text = "棄牌";
                    }
                    break;

                //邊池結果
                case ActionCode.SidePotResult:
                    Utils.Instance.ChipsChangeEffect(SidePot_Txt, "0");

                    //更新擁有籌碼
                    foreach (var chips in pack.GameProcessPack.UserChips)
                    {
                        seat = chips.Key == computerName ? computerSeat : seatDic[chips.Key];
                        Utils.Instance.ChipsChangeEffect(userInfoDic[seat].Item3, chips.Value);
                    }
                    Utils.Instance.ChipsChangeEffect(userInfoDic[computerSeat].Item3, pack.ComputerPack.Chips);

                    foreach (var winner in pack.SideWinPack.SideWinner)
                    {
                        if (Convert.ToInt32(winner.Value) > 0)
                        {
                            Debug.Log($"邊池獲勝者:{winner.Key}/{winner.Value}");
                            userInfoDic[seatDic[winner.Key]].Item7.gameObject.SetActive(true);
                            userInfoDic[seatDic[winner.Key]].Item7.text = "獲得邊池";
                            CreateBetShips(seatDic[winner.Key], winner.Value, false);
                        }                        
                    }

                    foreach (var back in pack.SideWinPack.SideBackChips)
                    {
                        if (Convert.ToInt32(back.Value) > 0)
                        {
                            Debug.Log($"邊池退回:{back.Key}/{back.Value}");

                            CreateSideResult(back.Key, back.Value);
                        }                        
                    }
                    break;
            }
        }
    }
}
