using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TexasHoldemProtobuf;

namespace HotFix_Project
{
    class FX_ModeView :FX_BaseView
    {
        private static FX_BaseView thisView;

        private static Button Logout_Btn, Holdem_Btn, Setting_Btn;
        private static Transform Download_Tr, DownLoadProgress_Tr;
        private static Image Progress_Img;
        private static Text Progress_Txt;

        private static float currProccess;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Logout_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Logout_Btn");
            Holdem_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Holdem_Btn");
            Setting_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Setting_Btn");
            DownLoadProgress_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "DownLoadProgress_Tr");            
            Download_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "Download_Tr");            
            Progress_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Progress_Img");
            Progress_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Progress_Txt");

            DownLoadProgress_Tr.gameObject.SetActive(false);

            ABManager.Instance.CheckAB("holdem", SwitchDownloadObj);
        }

        private static void Start()
        {
            //登出按鈕
            Logout_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();

                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.User;
                pack.ActionCode = ActionCode.Logout;
                thisView.view.SendRequest(pack);
            });

            //設置按鈕
            Setting_Btn.onClick.AddListener(() =>
            {
                UIManager.Instance.ShowToolView(ViewType.SettingView);
            });

            //德州撲克進入
            Holdem_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();
                ABManager.Instance.CheckAB("holdem", ClickHoldemBtn);         
            });
        }

        private static void Update()
        {
            //下載進度
            if (DownLoadProgress_Tr.gameObject.activeSelf)
            {
                if (Progress_Img.fillAmount < currProccess)
                {
                    Progress_Img.fillAmount += 1 * Time.deltaTime;
                }
                Progress_Txt.text = $"{(Progress_Img.fillAmount * 100).ToString("F0")}%";

                if (Progress_Img.fillAmount == 1)
                {
                    ABManager.Instance.CheckAB("holdem", SwitchDownloadObj);
                    DownLoadProgress_Tr.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 按下德州撲克
        /// </summary>
        /// <param name="isdownload"></param>
        private static void ClickHoldemBtn(bool isdownload)
        {
            if (isdownload)
            {
                UIManager.Instance.ShowLoadingView(ViewType.LobbyView);
            }
            else
            {
                ABManager.Instance.GetABSize("holdem", CheckResources);
            }
        }

        /// <summary>
        /// 下載物件顯示開關
        /// </summary>
        /// <param name="isdownload"></param>
        private static void SwitchDownloadObj(bool isdownload)
        {
            Download_Tr.gameObject.SetActive(!isdownload);
        }

        /// <summary>
        /// 檢測資源
        /// </summary>
        /// <param name="size"></param>
        private static void CheckResources(long size)
        {
            Debug.Log($"下載資源大小:" + size);
            double megabytes = (double)size / (1024 * 1024);

            UIManager.Instance.ShowConfirmView(() =>
            {
                Debug.Log($"開始下載資源 holdem");

                Download_Tr.gameObject.SetActive(false);
                DownLoadProgress_Tr.gameObject.SetActive(true);
                Progress_Img.fillAmount = 0;
                ABManager.Instance.DownloadAB("holdem", DownloadProgress);
            },
            $"下載熱更資源: {megabytes.ToString("F2")} M"
            );
        }

        /// <summary>
        /// 下載進度
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="finishedCallBack"></param>
        private static void DownloadProgress(float progress)
        {
            Debug.Log($"下載進度:{progress}%");
            currProccess = progress; 
        }

        /// <summary>
        /// 處理協議
        /// </summary>
        /// <param name="pack"></param>
        private static void HandleRequest(MainPack pack)
        {
            if (pack.ReturnCode == ReturnCode.Succeed)
            {
                UIManager.Instance.ShowLoadingView(ViewType.LoginView);
            }            
        }
    }
}
