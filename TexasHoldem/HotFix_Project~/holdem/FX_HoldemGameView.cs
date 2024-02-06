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

        private static Button Exit_Btn;
        private static Transform Seat_Tr, BetChips_Tr, BetChipsSample, PointTarget;

        private static Sprite[] avatarSpriteList;
        private static Image[] userAvatars_Img, userMasks;
        private static Text[] userNickNames_Txt, userChips_Txt, userCountDown_Txt;

        private static int computerSeat = 1;
        private static string userName;
        private static Dictionary<string, int> userDic = new Dictionary<string, int>();

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Exit_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Exit_Btn");
            BetChips_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "BetChips_Tr");
            BetChipsSample = FindConponent.FindObj<Transform>(thisView.view.transform, "BetChipsSample");
            PointTarget = FindConponent.FindObj<Transform>(thisView.view.transform, "PointTarget");

            Seat_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "Seat_Tr");
            userAvatars_Img = new Image[Seat_Tr.childCount];
            userNickNames_Txt = new Text[Seat_Tr.childCount];
            userChips_Txt = new Text[Seat_Tr.childCount];
            userMasks = new Image[Seat_Tr.childCount];
            userCountDown_Txt = new Text[Seat_Tr.childCount];
            for (int i = 0; i < Seat_Tr.childCount; i++)
            {
                userAvatars_Img[i] = FindConponent.FindObj<Image>(Seat_Tr.GetChild(i), "Avatar_Img");
                userNickNames_Txt[i] = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "NickName_Txt");
                userChips_Txt[i] = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "Chips_Txt");
                userMasks[i] = FindConponent.FindObj<Image>(Seat_Tr.GetChild(i), "DountDown_Img");
                userCountDown_Txt[i] = FindConponent.FindObj<Text>(Seat_Tr.GetChild(i), "userCountDown_Txt");
            }

            ABManager.Instance.LoadSprite("entry", "AvatarList", (avatars) =>
            {
                avatarSpriteList = avatars;
            });

            BetChipsSample.gameObject.SetActive(false);
        }

        private static void OnEnable()
        {
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.User;
            pack.ActionCode = ActionCode.GetUserInfo;
            thisView.view.SendRequest(pack);
        }

        private static void Start()
        {
            Exit_Btn.onClick.AddListener(() =>
            {
                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.Game;
                pack.ActionCode = ActionCode.ExitRoom;                

                thisView.view.SendRequest(pack);
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
        /// 遊戲階段
        /// </summary>
        /// <param name="pack"></param>
        private static void GameStage(MainPack pack)
        {
            switch (pack.GameProcessPack.GameProcess)
            {
                //選擇大小盲
                case GameProcess.SetBlind:
                    int smallSeat = 0, bigSeat = 0;
                    //小盲
                    if (userDic.ContainsKey(pack.GameProcessPack.SmallBlinder))
                    {
                        smallSeat = userDic[pack.GameProcessPack.SmallBlinder];
                    }
                    else
                    {
                        if(pack.GameProcessPack.SmallBlinder == "-1")
                        {
                            smallSeat = computerSeat;
                        }
                    }
                    //大盲
                    if (userDic.ContainsKey(pack.GameProcessPack.BigBlinder))
                    {
                        bigSeat = userDic[pack.GameProcessPack.BigBlinder];
                    }
                    else
                    {
                        if (pack.GameProcessPack.BigBlinder == "-1")
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
                    break;

                //翻牌
                case GameProcess.Flop:
                    break;

                //轉排
                case GameProcess.Turn:
                    break;

                //河牌
                case GameProcess.River:
                    break;
            }
        }

        private static void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                CreateBetShips(0, "123");
            }  
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
            obj.name = seat.ToString();
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
                    userName = pack.UserInfoPack[0].NickName;
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
                        if (!userDic.ContainsKey(user.NickName))
                        {
                            //座位
                            int seat = 0;
                            if (user.NickName != userName)
                            {
                                for (int i = 2; i < userAvatars_Img.Length; i++)
                                {
                                    if (!userDic.ContainsValue(i))
                                    {
                                        seat = i;
                                        break;
                                    }
                                }
                            }

                            userDic.Add(user.NickName, seat);
                            userAvatars_Img[seat].sprite = avatarSpriteList[Convert.ToInt32(user.Avatar)];
                            userNickNames_Txt[seat].text = user.NickName;
                            userChips_Txt[seat].text = user.Chips;
                        }
                    }

                    //電腦玩家
                    userAvatars_Img[computerSeat].sprite = avatarSpriteList[Convert.ToInt32(pack.ComputerPack.Avatar)];
                    userNickNames_Txt[computerSeat].text = pack.ComputerPack.NickName;
                    userChips_Txt[computerSeat].text = pack.ComputerPack.Chips;

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
                    if (userDic.ContainsKey(pack.UserInfoPack[0].NickName))
                    {
                        int seat = userDic[pack.UserInfoPack[0].NickName];
                        userAvatars_Img[seat].sprite = null;
                        userNickNames_Txt[seat].text = "";
                        userDic.Remove(pack.UserInfoPack[0].NickName);
                    }
                    break;

                //遊戲階段
                case ActionCode.GameStage:
                    GameStage(pack);
                    break;
            }
        }
    }
}
