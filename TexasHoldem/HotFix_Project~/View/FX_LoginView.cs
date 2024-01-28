using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TexasHoldemProtobuf;

namespace HotFix_Project.View
{
    class FX_LoginView
    {
        private static BaseView view = null;
        private static GameObject obj = null;

        private static Text Title_Txt, Send_Txt;
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

        public static void Init(BaseView baseView, GameObject viewObj)
        {
            view = baseView;
            obj = viewObj;

            Title_Txt = FindConponent.FindObj<Text>(view.transform, "Title_Txt");
            Send_Txt = FindConponent.FindObj<Text>(view.transform, "Send_Txt");
            Acc_IF = FindConponent.FindObj<InputField>(view.transform, "Acc_IF");
            Psw_IF = FindConponent.FindObj<InputField>(view.transform, "Psw_IF");
            Send_Btn = FindConponent.FindObj<Button>(view.transform, "Send_Btn");
            Switch_Btn = FindConponent.FindObj<Button>(view.transform, "Switch_Btn");
        }

        public static void Awake()
        {
            SwichMode(ModeType.login);
        }

        public static void Start()
        {
            //發送按鈕
            Send_Btn.onClick.AddListener(() =>
            {
                if(string.IsNullOrEmpty(Acc_IF.text) || string.IsNullOrEmpty(Acc_IF.text))
                {
                    UIManager.Instance.ShowTip("帳號/密碼 不可為空!");
                    return;
                }

                MainPack pack = new MainPack();
                pack.ActionCode = currentMode == ModeType.login ? ActionCode.Login : ActionCode.Logon;
                pack.RequestCode = RequestCode.User;

                LoginPack loginPack = new LoginPack();
                loginPack.Account = Acc_IF.text;
                loginPack.Password = Psw_IF.text;

                pack.LoginPack = loginPack;
                RequestManager.Instance.Send(pack, ReviceLogin);
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

            switch (modeType)
            {
                //登入
                case ModeType.login:                    
                    Title_Txt.text = "登入";
                    Send_Txt.text = "登入";
                    break;

                //註冊
                case ModeType.logon:
                    Title_Txt.text = "註冊";
                    Send_Txt.text = "註冊";
                    break;
            }
        }

        /// <summary>
        /// 協議接收-登入
        /// </summary>
        /// <param name="pack"></param>
        private static void ReviceLogin(MainPack pack)
        {
            Debug.Log("登入" + pack.ReturnCode);
        }

        /// <summary>
        /// 協議接收-註冊
        /// </summary>
        /// <param name="pack"></param>
        private static void ReviceLogon(MainPack pack)
        {
            Debug.Log("註冊" + pack.ReturnCode);
        }
    }
}
