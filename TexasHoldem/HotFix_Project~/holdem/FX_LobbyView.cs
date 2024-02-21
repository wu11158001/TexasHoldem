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

        private static Button Back_Btn, QuickStart_Btn, CreateRoom_Btn, BlindCancel_Btn, BlindConfirm_Btn, Setting_Btn;
        private static Transform RoomList_Tr, RoomListSample_Tr, CreateRoom_Tr;
        private static Dropdown SelectBlind_Dd;

        private static Timer updateRoomTimer;

        private static List<GameObject> roomList = new List<GameObject>();

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Back_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Back_Btn");
            QuickStart_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "QuickStart_Btn");
            CreateRoom_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "CreateRoom_Btn");
            BlindCancel_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "BlindCancel_Btn");
            BlindConfirm_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "BlindConfirm_Btn");
            Setting_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Setting_Btn");
            SelectBlind_Dd = FindConponent.FindObj<Dropdown>(thisView.view.transform, "SelectBlind_Dd");
            RoomList_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "RoomList_Tr");
            RoomListSample_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "RoomListSample_Tr");
            CreateRoom_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "CreateRoom_Tr");

            RoomListSample_Tr.gameObject.SetActive(false);
            CreateRoom_Tr.gameObject.SetActive(false);
        }

        private static void OnEnable()
        {
            updateRoomTimer = new Timer(SendUpdateRoom, null, 0, 5000);
        }

        private static void Start()
        {
            //返回按鈕
            Back_Btn.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowLoadingView(ViewType.LobbyView, true);
            });

            //設置按鈕
            Setting_Btn.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowToolView(ViewType.SettingView);
            });

            //快速開局
            QuickStart_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();

                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.Room;
                pack.ActionCode = ActionCode.QuickJoinRoom;

                thisView.view.SendRequest(pack);
            });

            //創建房間
            CreateRoom_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();

                CreateRoom_Tr.gameObject.SetActive(true);
            });

            //關閉大小盲View
            BlindCancel_Btn.onClick.AddListener(() =>
            {
                CreateRoom_Tr.gameObject.SetActive(false);
            });

            //大小盲確認
            BlindConfirm_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();

                string bigBlind = "";
                switch(SelectBlind_Dd.value)
                {
                    case 0:
                        bigBlind = "50";
                        break;
                    case 1:
                        bigBlind = "100";
                        break;
                    case 2:
                        bigBlind = "2000";
                        break;

                }

                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.Room;
                pack.ActionCode = ActionCode.CreateRoom;

                RoomPack roomPack = new RoomPack();
                roomPack.RoomBigBlind = bigBlind;

                pack.RoomPack.Add(roomPack);
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
                GameObject obj = GameObject.Instantiate(RoomListSample_Tr.gameObject);
                RoomListView roomView = obj.GetComponent<RoomListView>();
                roomView.transform.SetParent(RoomList_Tr);
                roomView.gameObject.SetActive(true);
                roomView.transform.localScale = Vector3.one;
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
                        UIManager.Instance.ShowTip("籌碼不足!!!");
                    }
                    break;

                //創建房間
                case ActionCode.CreateRoom:
                    if (pack.ReturnCode == ReturnCode.Succeed)
                    {
                        CreateRoom_Tr.gameObject.SetActive(false);
                        UIManager.Instance.ShowLoadingView(ViewType.HoldemGameView);
                    }
                    else
                    {
                        UIManager.Instance.ShowTip("籌碼不足!!!");
                    }
                    break;
            }
        }
    }
}
