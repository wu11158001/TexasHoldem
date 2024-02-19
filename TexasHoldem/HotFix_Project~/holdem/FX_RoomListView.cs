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
    class FX_RoomListView
    {
        private static FX_BaseView thisView;

        private static Text RoomName_Txt, Count_Txt, Blind_Txt;
        private static Image Count_Img;
        private static Button Join_Btn;

        private static string roomName;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            RoomName_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "RoomName_Txt");
            Count_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Count_Txt");
            Blind_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Blind_Txt");
            Count_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Count_Img");
            Join_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Join_Btn");
        }

        private static void Start()
        {
            Join_Btn.onClick.AddListener(() =>
            {
                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.JoinRoom;
                pack.RequestCode = RequestCode.Room;

                RoomPack roomPack = new RoomPack();
                roomPack.RoomName = roomName;

                pack.RoomPack.Add(roomPack);
                thisView.view.SendRequest(pack);
            });
        }

        /// <summary>
        /// 設定房間訊息
        /// </summary>
        /// <param name="roomPack"></param>
        private static void SetRoomInfo(RoomPack roomPack)
        {
            roomName = roomPack.RoomName;

            RoomName_Txt.text = $"{roomName}.";
            Count_Txt.text = $"{roomPack.CurrCount + 1} / {roomPack.MaxCount + 1}";
            Count_Img.fillAmount = (float)(roomPack.CurrCount + 1) / (float)(roomPack.MaxCount + 1);
            Blind_Txt.text = $"{Convert.ToInt32(roomPack.RoomBigBlind) / 2}/{roomPack.RoomBigBlind}";
        }

        /// <summary>
        /// 處理協議
        /// </summary>
        /// <param name="pack"></param>
        private static void HandleRequest(MainPack pack)
        {
            switch (pack.ReturnCode)
            {
                case ReturnCode.Succeed:
                    UIManager.Instance.ShowLoadingView(ViewType.HoldemGameView);
                    break;

                case ReturnCode.Fail:
                    UIManager.Instance.ShowTip("房間已滿!!!");
                    break;

                case ReturnCode.NotRoom:
                    UIManager.Instance.ShowTip("房間不存在!!!");
                    break;
            }
        }
    }
}
