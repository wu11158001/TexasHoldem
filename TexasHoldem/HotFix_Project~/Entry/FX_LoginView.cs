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
        private static InputField Acc_IF, Psw_IF, Psw2_IF;
        private static Button Send_Btn, Switch_Btn, Setting_Btn, Eye1_Btn, Eye2_Btn;
        private static Image Eye1_Img, Eye2_Img;
        private static Transform Tip_Tr;

        private static Sprite[] eyeList;

        /// <summary>
        /// 模式
        /// </summary>
        private enum ModeType
        {
            login,      //登入
            logon,      //註冊
        }
        private static ModeType currentMode;

        private static string localSaveAcc = "Holdem_Account";
        private static string localSavePsw = "Holdem_Password";

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Title_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Title_Txt");
            Send_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Send_Txt");
            Switch_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Switch_Txt");
            Acc_IF = FindConponent.FindObj<InputField>(thisView.view.transform, "Acc_IF");
            Psw_IF = FindConponent.FindObj<InputField>(thisView.view.transform, "Psw_IF");
            Psw2_IF = FindConponent.FindObj<InputField>(thisView.view.transform, "Psw2_IF");
            Send_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Send_Btn");
            Switch_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Switch_Btn");
            Eye1_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Eye1_Btn");
            Eye2_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Eye2_Btn");
            Setting_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Setting_Btn");
            Eye1_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Eye1_Btn");
            Eye2_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Eye2_Btn");
            Tip_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "Tip_Tr");

            ABManager.Instance.LoadSprite("entry", "Eyes", (eyes) =>
            {
                eyeList = eyes;
            });
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
                AudioManager.Instance.PlayButtonClick();
                ClickSend();
            });

            //切換按鈕
            Switch_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();
                ModeType modeType = currentMode == ModeType.login ? ModeType.logon : ModeType.login;
                SwichMode(modeType);
            });

            //顯示/影藏密碼
            Eye1_Btn.onClick.AddListener(() =>
            {
                Psw_IF.contentType = Psw_IF.contentType == InputField.ContentType.Password ?
                                     InputField.ContentType.Standard :
                                     InputField.ContentType.Password;
                string temp = Psw_IF.text;
                Psw_IF.text = "";
                Psw_IF.text = temp;


                Eye1_Img.sprite = Psw_IF.contentType == InputField.ContentType.Password ?
                                 eyeList[0] :
                                 eyeList[1];
            });

            Eye2_Btn.onClick.AddListener(() =>
            {
                Psw2_IF.contentType = Psw2_IF.contentType == InputField.ContentType.Password ?
                                     InputField.ContentType.Standard :
                                     InputField.ContentType.Password;
                string temp = Psw2_IF.text;
                Psw2_IF.text = "";
                Psw2_IF.text = temp;


                Eye2_Img.sprite = Psw2_IF.contentType == InputField.ContentType.Password ?
                                 eyeList[0] :
                                 eyeList[1];
            });

            //設置按鈕
            Setting_Btn.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowToolView(ViewType.SettingView);
            });

            Acc_IF.Select();

            //自動登入
            AutoLogin();            
        }

        private static void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Acc_IF.isFocused)
                {
                    Psw_IF.Select();
                }
                else if (Psw_IF.isFocused)
                {
                    if (currentMode == ModeType.logon)
                    {
                        Psw2_IF.Select();
                    }
                    else
                    {
                        Acc_IF.Select();
                    }
                }
                else
                {
                    Acc_IF.Select();
                }
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                AudioManager.Instance.PlayButtonClick();
                ClickSend();
            }
        }

        /// <summary>
        /// 按下發送
        /// </summary>
        private static void ClickSend()
        {
            if (string.IsNullOrEmpty(Acc_IF.text) || string.IsNullOrEmpty(Psw_IF.text))
            {
                UIManager.Instance.ShowTip("帳號/密碼 不可為空!");
                return;
            }

            if (Acc_IF.text.Length < 6 || Acc_IF.text.Length > 12 ||
                Psw_IF.text.Length < 6 || Psw_IF.text.Length > 12 ||
                !Utils.IsAlphaNumeric(Acc_IF.text) || !Utils.IsAlphaNumeric(Psw_IF.text))
            {
                UIManager.Instance.ShowTip("帳號/密碼需為6~12位數字或英文字母");
                return;
            }

            if (currentMode == ModeType.logon)
            {
                if (Psw_IF.text != Psw2_IF.text)
                {
                    UIManager.Instance.ShowTip("密碼輸入不相同。");
                    return;
                }
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
            Psw_IF.contentType = InputField.ContentType.Password;
            Psw2_IF.contentType = InputField.ContentType.Password;
            Psw2_IF.gameObject.SetActive(modeType == ModeType.logon);
            if (eyeList != null && eyeList.Length > 0)
            {
                Eye1_Img.sprite = eyeList[0];
                Eye2_Img.sprite = eyeList[0];
            }

            Tip_Tr.gameObject.SetActive(modeType == ModeType.logon);
            Title_Txt.text = modeType == ModeType.login ? "登入" : "註冊";
            Send_Txt.text = modeType == ModeType.login ? "登入" : "註冊";
            Switch_Txt.text = modeType == ModeType.login ? "註冊" : "登入";
        }

        /// <summary>
        /// 自動登入
        /// </summary>
        /// <returns></returns>
        private static void AutoLogin()
        {
            string acc = PlayerPrefs.GetString(localSaveAcc, "");
            string psw = PlayerPrefs.GetString(localSavePsw, "");

            if (!string.IsNullOrEmpty(acc) && !string.IsNullOrEmpty(psw))
            {
                Debug.Log($"帳號:{acc} / 密碼:{psw} 自動登入!");
                UIManager.Instance.WaitViewSwitch(true);

                MainPack pack = new MainPack();
                pack.ActionCode = ActionCode.Login;
                pack.RequestCode = RequestCode.User;

                LoginPack loginPack = new LoginPack();
                loginPack.Account = acc;
                loginPack.Password = psw;

                pack.LoginPack = loginPack;
                thisView.view.SendRequest(pack);
            }
        }

        /// <summary>
        /// 儲存登入資料
        /// </summary>
        /// <param name="pack"></param>
        private static void SaveLoginData(MainPack pack)
        {
            Debug.Log($"本地儲存登入資料。");

            PlayerPrefs.SetString(localSaveAcc, pack.LoginPack.Account);
            PlayerPrefs.SetString(localSavePsw, pack.LoginPack.Password);
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
                    SaveLoginData(pack);
                    UIManager.Instance.ShowLoadingView(ViewType.ModeView);
                }
                else if (pack.ActionCode == ActionCode.Logon)
                {
                    //註冊
                    SaveLoginData(pack);

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
