using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TexasHoldemProtobuf;

namespace HotFix_Project
{
    class FX_LobbyView : FX_BaseView
    {
        private static FX_BaseView thisView;

        private static Button QuickStart_Btn;
        private static Transform RoomList_Tr;
        private static GameObject RoomListSample_Obj;

        private static Timer updateRoomTimer;

        private static List<GameObject> roomList = new List<GameObject>();

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            QuickStart_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "QuickStart_Btn");
            RoomList_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "RoomList_Tr");
            RoomListSample_Obj = FindConponent.FindObj<Transform>(thisView.view.transform, "RoomListSample_Obj").gameObject;

            RoomListSample_Obj.SetActive(false);
        }

        private static void OnEnable()
        {
            updateRoomTimer = new Timer(SendUpdateRoom, null, 0, 5000);
        }

        private static void Start()
        {
            QuickStart_Btn.onClick.AddListener(() =>
            {
                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.Room;
                pack.ActionCode = ActionCode.QuickJoinRoom;

                thisView.view.SendRequest(pack);
            });
        }

        private static void OnDisable()
        {
            updateRoomTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 發送刷新房間
        /// </summary>
        /// <param name="state"></param>
        private static void SendUpdateRoom(object state)
        {
            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.Room;
            pack.ActionCode = ActionCode.UpdateRoom;
            thisView.view.SendRequest(pack);
        }

        /// <summary>
        /// 刷新房間列表
        /// </summary>
        /// <param name="pack"></param>
        private static void UpdateRoomList(MainPack pack)
        {
            foreach (var room in roomList)
            {
                GameObject.Destroy(room);
            }

            foreach (var room in pack.RoomPack)
            {
                GameObject obj = GameObject.Instantiate(RoomListSample_Obj);
                RoomListView roomView = obj.GetComponent<RoomListView>();
                roomView.transform.SetParent(RoomList_Tr);
                roomView.gameObject.SetActive(true);
                roomView.SetRoomInfo(room);
                roomList.Add(obj);
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
                //刷新房間
                case ActionCode.UpdateRoom:
                    if (pack.ReturnCode == ReturnCode.Succeed)
                    {
                        UpdateRoomList(pack);
                    }
                    else
                    {
                        Debug.LogError("刷新房間錯誤!!!");
                    }
                    break;

                //快速開局
                case ActionCode.QuickJoinRoom:
                    if (pack.ReturnCode == ReturnCode.Succeed)
                    {
                        UIManager.Instance.ShowLoadingView(ViewType.HoldemGameView);
                    }
                    else
                    {
                        Debug.LogError("快速開局錯誤!!!");
                    }
                    break;
            }
        }
    }
}
