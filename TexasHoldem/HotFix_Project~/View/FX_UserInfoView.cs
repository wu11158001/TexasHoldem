using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TexasHoldemProtobuf;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix_Project
{
    class FX_UserInfoView : FX_BaseView
    {
        private static FX_BaseView thisView;

        private static GameObject ReviseNickName_Obj;
        private static Button NickName_Btn, Cancel_Btn, Confirm_Btn;
        private static Text NickName_Txt, Cash_Txt;
        private static InputField NickName_IF;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            ReviseNickName_Obj = FindConponent.FindObj<Transform>(thisView.view.transform, "ReviseNickName_Obj").gameObject;
            NickName_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "NickName_Btn");
            Cancel_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Cancel_Btn");
            Confirm_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Confirm_Btn");
            NickName_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "NickName_Txt");
            Cash_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Cash_Txt");
            NickName_IF = FindConponent.FindObj<InputField>(thisView.view.transform, "NickName_IF");
        }

        private static void OnEnable()
        {
            ReviseNickName_Obj.SetActive(false);

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.User;
            pack.ActionCode = ActionCode.GetUserInfo;
            thisView.view.SendRequest(pack);
        }

        private static void Start()
        {
            NickName_Btn.onClick.AddListener(() =>
            {
                NickName_IF.text = "";
                NickName_IF.Select();
                ReviseNickName_Obj.SetActive(true);
            });

            Cancel_Btn.onClick.AddListener(() =>
            {
                ReviseNickName_Obj.SetActive(false);
            });

            Confirm_Btn.onClick.AddListener(() =>
            {
                if (NickName_IF.text == NickName_Txt.text)
                {
                    UIManager.Instance.ShowTip("與當前暱稱相同。");
                    return;
                }

                if (!string.IsNullOrEmpty(NickName_IF.text))
                {
                    MainPack pack = new MainPack();
                    pack.RequestCode = RequestCode.User;
                    pack.ActionCode = ActionCode.ReviseNickName;

                    UserInfoPack userInfoPack = new UserInfoPack();
                    userInfoPack.NickName = NickName_IF.text;

                    pack.UserInfoPack.Add(userInfoPack);
                    thisView.view.SendRequest(pack);
                }
                else
                {
                    UIManager.Instance.ShowTip("請輸入暱稱!!!");
                }
            });
        }

        /// <summary>
        /// 處理協議
        /// </summary>
        /// <param name="pack"></param>
        private static void HandleRequest(MainPack pack)
        {
            switch (pack.ActionCode)
            {
                case ActionCode.GetUserInfo:
                    NickName_Txt.text = pack.UserInfoPack[0].NickName;
                    Cash_Txt.text = pack.UserInfoPack[0].Cash;
                    Debug.Log($"頭像:{pack.UserInfoPack[0].Avatar}");
                    break;

                case ActionCode.ReviseNickName:
                    if (pack.ReturnCode == ReturnCode.Succeed)
                    {
                        ReviseNickName_Obj.SetActive(false);
                        NickName_Txt.text = pack.UserInfoPack[0].NickName;
                    }
                    else if (pack.ReturnCode == ReturnCode.Duplicated)
                    {
                        UIManager.Instance.ShowTip("暱稱重複!!!");
                    }
                    else
                    {
                        UIManager.Instance.ShowTip("修改暱稱失敗!!!");
                    }
                    break;
            }
        }
    }
}
