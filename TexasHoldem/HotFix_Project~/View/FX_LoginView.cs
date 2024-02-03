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
    class FX_LoginView : FX_BaseView
    {
        private static FX_BaseView thisView;

        private static Text Title_Txt, Send_Txt, Switch_Txt;
        private static InputField Acc_IF, Psw_IF;
        private static Button Send_Btn, Switch_Btn;

        /// <summary>
        /// 模式
        /// </summary>
        private enum ModeType
        {
            login,      //登入
            logon,      //註冊
        }
        private static ModeType currentMode;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Title_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Title_Txt");
            Send_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Send_Txt");
            Switch_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Switch_Txt");
            Acc_IF = FindConponent.FindObj<InputField>(thisView.view.transform, "Acc_IF");
            Psw_IF = FindConponent.FindObj<InputField>(thisView.view.transform, "Psw_IF");
            Send_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Send_Btn");
            Switch_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Switch_Btn");
        }

        private static void OnEnable()
        {
            SwichMode(ModeType.login);
        }

        private static void Start()
        {
            //發送按鈕
            Send_Btn.onClick.AddListener(() =>
            {               
                if(string.IsNullOrEmpty(Acc_IF.text) || string.IsNullOrEmpty(Psw_IF.text))
                {
                    UIManager.Instance.ShowTip("帳號/密碼 不可為空!");
                    return;
                }

                UIManager.Instance.WaitViewSwitch(true);

                MainPack pack = new MainPack();
                pack.ActionCode = currentMode == ModeType.login ? ActionCode.Login : ActionCode.Logon;
                pack.RequestCode = RequestCode.User;

                LoginPack loginPack = new LoginPack();
                loginPack.Account = Acc_IF.text;
                loginPack.Password = Psw_IF.text;

                pack.LoginPack = loginPack;
                thisView.view.SendRequest(pack);
            });

            //切換按鈕
            Switch_Btn.onClick.AddListener(() =>
            {
                ModeType modeType = currentMode == ModeType.login ? ModeType.logon : ModeType.login;
                SwichMode(modeType);
            });
        }

        /// <summary>
        /// 切換模式
        /// </summary>
        /// <param name="modeType"></param>
        private static void SwichMode(ModeType modeType)
        {
            currentMode = modeType;
            Acc_IF.text = "";
            Psw_IF.text = "";

            Title_Txt.text = modeType == ModeType.login ? "登入" : "註冊";
            Send_Txt.text = modeType == ModeType.login ? "登入" : "註冊";
            Switch_Txt.text = modeType == ModeType.login ? "註冊" : "登入";
        }

        /// <summary>
        /// 處理協議
        /// </summary>
        /// <param name="pack"></param>
        private static void HandleRequest(MainPack pack)
        {
            UIManager.Instance.WaitViewSwitch(false);

            if (pack.ReturnCode == ReturnCode.Succeed)
            {
                if (pack.ActionCode == ActionCode.Login)
                {
                    //登入
                    UIManager.Instance.ShowLoadingView(ViewType.ModeView);
                }
                else if (pack.ActionCode == ActionCode.Logon)
                {
                    //註冊
                    UIManager.Instance.WaitViewSwitch(true);
                    UIManager.Instance.ShowTip("註冊完成。進入遊戲...");
                    AsyncFunc.DelayFunc(3000, () =>
                    {
                        UIManager.Instance.ShowLoadingView(ViewType.ModeView);
                        UIManager.Instance.WaitViewSwitch(false);
                    });                    
                }
            }
            else if (pack.ReturnCode == ReturnCode.Duplicated)
            {
                string tipStr = pack.ActionCode == ActionCode.Login ? "帳號已登入!!!" : "帳號已註冊!!!";
                UIManager.Instance.ShowTip(tipStr);
            }
            else
            {             
                string tipStr = pack.ActionCode == ActionCode.Login ? "登入失敗!!!" : "註冊失敗!!!";
                UIManager.Instance.ShowTip(tipStr);
            }
        }
    }
}
