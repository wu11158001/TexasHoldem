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

        private static Button Leave_Btn;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Leave_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Leave_Btn");
        }

        private static void OnEnable()
        {
            SendUpdateRoomInfo();
        }

        private static void Start()
        {
            Leave_Btn.onClick.AddListener(() =>
            {
                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.ExitRoom;
                pack.RequestCode = RequestCode.Game;

                thisView.view.SendRequest(pack);
            });
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
            if (pack.ReturnCode == ReturnCode.Succeed)
            {
                switch (pack.ActionCode)
                {
                    case ActionCode.ExitRoom:
                        UIManager.Instance.ShowView(ViewType.LobbyView);
                        break;
                }
            }
            else
            {
                Debug.LogError($"協議出錯:{pack.ActionCode}");
            }
        }

        /// <summary>
        /// 接收廣播訊息
        /// </summary>
        /// <param name="pack"></param>
        private static void ReciveBroadcast(MainPack pack)
        {
            foreach (var user in pack.RoomUserInfoPack)
            {
                Debug.Log(user.AccountName);
            }
        }
    }
}
