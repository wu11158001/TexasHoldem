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

        private static Sprite[] avatarSpriteList;
        private static Image[] userAvatars_Img, userMasks;
        private static Text[] userNickNames_Txt, userChips_Txt, userCountDown_Txt;
        private static Dictionary<string, int> userDic = new Dictionary<string, int>();

        private static string userName;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Exit_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Exit_Btn");

            Transform Seat = FindConponent.FindObj<Transform>(thisView.view.transform, "Seat_Tr");
            userAvatars_Img = new Image[Seat.childCount];
            userNickNames_Txt = new Text[Seat.childCount];
            userChips_Txt = new Text[Seat.childCount];
            userMasks = new Image[Seat.childCount];
            userCountDown_Txt = new Text[Seat.childCount];
            for (int i = 0; i < Seat.childCount; i++)
            {
                userAvatars_Img[i] = FindConponent.FindObj<Image>(Seat.GetChild(i), "Avatar_Img");
                userNickNames_Txt[i] = FindConponent.FindObj<Text>(Seat.GetChild(i), "NickName_Txt");
                userChips_Txt[i] = FindConponent.FindObj<Text>(Seat.GetChild(i), "Chips_Txt");
                userMasks[i] = FindConponent.FindObj<Image>(Seat.GetChild(i), "DountDown_Img");
                userCountDown_Txt[i] = FindConponent.FindObj<Text>(Seat.GetChild(i), "userCountDown_Txt");
            }

            ABManager.Instance.LoadSprite("entry", "AvatarList", (avatars) =>
            {
                avatarSpriteList = avatars;
            });
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

        private static void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                SendUpdateRoomInfo();
            }
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
                    int computerSeat = 1;
                    userAvatars_Img[computerSeat].sprite = avatarSpriteList[Convert.ToInt32(pack.ComputerPack.Avatar)];
                    userNickNames_Txt[computerSeat].text = pack.ComputerPack.NickName;
                    userChips_Txt[computerSeat].text = pack.ComputerPack.Chips;
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
            }
        }
    }
}
