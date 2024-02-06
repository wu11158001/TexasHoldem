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

        private static Button Logout_Btn, Holdem_Btn;
        private static GameObject Download_Obj, DownLoadProgress_Obj;
        private static Image Progress_Img;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            Logout_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Logout_Btn");
            Holdem_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Holdem_Btn");
            DownLoadProgress_Obj = FindConponent.FindObj<Transform>(thisView.view.transform, "DownLoadProgress_Obj").gameObject;            
            Download_Obj = FindConponent.FindObj<Transform>(thisView.view.transform, "Download_Obj").gameObject;            
            Progress_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Progress_Img");

            DownLoadProgress_Obj.SetActive(false);

            ABManager.Instance.CheckAB("holdem", SwitchDownloadObj);
        }

        private static void Start()
        {
            Logout_Btn.onClick.AddListener(() =>
            {
                MainPack pack = new MainPack();
                pack.RequestCode = RequestCode.User;
                pack.ActionCode = ActionCode.Logout;
                thisView.view.SendRequest(pack);
            });

            Holdem_Btn.onClick.AddListener(() =>
            {
                ABManager.Instance.CheckAB("holdem", ClickHoldemBtn);         
            });
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
            Download_Obj.SetActive(!isdownload);
        }

        /// <summary>
        /// 檢測資源
        /// </summary>
        /// <param name="size"></param>
        private static void CheckResources(long size)
        {
            Debug.Log($"下載資源大小:" + size);
            double megabytes = (double)size / (1024 * 1024);
            DownLoadProgress_Obj.SetActive(true);

            UIManager.Instance.ShowConfirmView(() =>
            {
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
            Progress_Img.fillAmount = progress;
            if (progress == 100)
            {
                ABManager.Instance.CheckAB("holdem", SwitchDownloadObj);
                DownLoadProgress_Obj.SetActive(false);
            }
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
