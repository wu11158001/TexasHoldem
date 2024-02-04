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
        private static Image UserAvatar_Img;
        private static Text UserNickName_Txt, UserCash_Txt;

        private static Sprite[] avatarList;
        private static Image[] othersAvatar_Img;
        private static Text[] othersNickName_Txt;
        private static Dictionary<string, int> otherUserDic = new Dictionary<string, int>();

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Exit_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Exit_Btn");
            UserAvatar_Img = FindConponent.FindObj<Image>(thisView.view.transform, "UserAvatar_Img");
            UserNickName_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "UserNickName_Txt");
            UserCash_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "UserCash_Txt");
            Transform OtherUser = FindConponent.FindObj<Transform>(thisView.view.transform, "OtherUser");
            othersAvatar_Img = new Image[OtherUser.childCount];
            othersNickName_Txt = new Text[OtherUser.childCount];
            for (int i = 0; i < OtherUser.childCount; i++)
            {
                othersAvatar_Img[i] = FindConponent.FindObj<Image>(OtherUser.GetChild(i), "Avatar_Img");
                othersNickName_Txt[i] = FindConponent.FindObj<Text>(OtherUser.GetChild(i), "NickName_Txt");
            }

            ABManager.Instance.LoadSprite("entry", "AvatarList", (avatars) =>
            {
                avatarList = avatars;
            });
        }

        private static void OnEnable()
        {
            SendGetUserInfo();
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
        /// 發送獲取用戶訊息
        /// </summary>
        private static void SendGetUserInfo()
        {
            if (avatarList != null)
            {
                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.User;
                pack.ActionCode = ActionCode.GetUserInfo;
                thisView.view.SendRequest(pack);
            }                
        }

        /// <summary>
        /// 發送更新房間玩家訊息
        /// </summary>
        private static void SendUpdateRoomInfo()
        {
            MainPack pack = new MainPack();
            pack.ActionCode = ActionCode.UpdateRoomUserInfo;
            pack.RequestCode = RequestCode.Game;
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
                    UserNickName_Txt.text = pack.UserInfoPack[0].NickName;
                    UserCash_Txt.text = pack.UserInfoPack[0].Cash;
                    UserAvatar_Img.sprite = avatarList[Convert.ToInt32(pack.UserInfoPack[0].Avatar)];

                    //SendUpdateRoomInfo();
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
                    foreach (var user in pack.UserInfoPack)
                    {
                        if (user.NickName != UserNickName_Txt.text)
                        {
                            if (!otherUserDic.ContainsKey(user.NickName))
                            {
                                for (int i = 0; i < othersAvatar_Img.Length; i++)
                                {
                                    if (!otherUserDic.ContainsValue(i))
                                    {
                                        otherUserDic.Add(user.NickName, i);
                                        othersAvatar_Img[i].sprite = avatarList[Convert.ToInt32(user.Avatar)];                                        
                                        othersNickName_Txt[i].text = user.NickName;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}
