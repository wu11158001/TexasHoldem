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

        private static Transform ReviseNickName_Tr, AvatarSample_Btn;
        private static Button NickName_Btn, Cancel_Btn, Confirm_Btn, Avatar_Btn;
        private static Text NickName_Txt, Chips_Txt;
        private static InputField NickName_IF;
        private static RectTransform AvatarList_Rt, AvatarList_Tr;
        private static Image Avatar_Img;

        private static bool isGetAvatar;
        private static bool AvatarListSwitch;
        private static GridLayoutGroup avatarGridLayout;
        private static int avatarWidthCound = 5;
        private static float initAvatarListHeight;
        private static Vector2 avatarItemSize;
        private static Vector2 avatarTargetSize;
        private static Vector2 avatarTempSize;

        private static Sprite[] avatarList;

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            ReviseNickName_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "ReviseNickName_Tr");
            AvatarList_Tr = FindConponent.FindObj<RectTransform>(thisView.view.transform, "AvatarList_Tr");
            AvatarSample_Btn = FindConponent.FindObj<Transform>(thisView.view.transform, "AvatarSample_Btn");
            AvatarList_Rt = FindConponent.FindObj<RectTransform>(thisView.view.transform, "AvatarList_Rt");
            NickName_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "NickName_Btn");
            Cancel_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Cancel_Btn");
            Confirm_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Confirm_Btn");
            Avatar_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Avatar_Btn");
            NickName_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "NickName_Txt");
            Chips_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Chips_Txt");
            NickName_IF = FindConponent.FindObj<InputField>(thisView.view.transform, "NickName_IF");
            Avatar_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Avatar_Img");

            AvatarSample_Btn.gameObject.SetActive(false);

            LoadAvatar();   
        }

        private static void Start()
        {
            NickName_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();
                ReviseNickName_Tr.gameObject.SetActive(true);
                NickName_IF.text = "";
                NickName_IF.Select();
            });

            Cancel_Btn.onClick.AddListener(() =>
            {
                ReviseNickName_Tr.gameObject.SetActive(false);
            });

            Confirm_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();
                ClickConfirm();
            });

            Avatar_Btn.onClick.AddListener(() =>
            {
                AudioManager.Instance.PlayButtonClick();
                if (AvatarList_Tr.gameObject.activeSelf)
                {
                    AvatarListSwitch = false;
                }
                else
                {
                    AvatarListSwitch = true;
                    AvatarList_Tr.gameObject.SetActive(true);
                    avatarTempSize = new Vector2(0, initAvatarListHeight);                    
                }                
            });

            ReviseNickName_Tr.gameObject.SetActive(false);
            AvatarList_Tr.gameObject.SetActive(false);

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.User;
            pack.ActionCode = ActionCode.GetUserInfo;
            thisView.view.SendRequest(pack);
        }

        private static void Update()
        {
            //修改暱稱確認
            if (ReviseNickName_Tr.gameObject.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
                {
                    AudioManager.Instance.PlayButtonClick();
                    ClickConfirm();
                }
            }

            //頭像列表開關
            if (AvatarListSwitch)
            {
                if (avatarTempSize.x < avatarTargetSize.x)
                {
                    avatarTempSize.x += 15;
                }
                else
                {
                    if (avatarTempSize.y < avatarTargetSize.y)
                    {
                        avatarTempSize.y += 10;
                    }
                }        
                AvatarList_Tr.sizeDelta = new Vector2(avatarTempSize.x, avatarTempSize.y);
            }

            if (AvatarList_Tr.gameObject.activeSelf && !AvatarListSwitch)
            {
                if (AvatarList_Tr.sizeDelta.y > initAvatarListHeight)
                {                    
                    avatarTempSize.y -= 10; 
                }
                else
                {
                    if (AvatarList_Tr.sizeDelta.x > 0)
                    {
                        avatarTempSize.x -= 15;
                    }
                    else
                    {
                        AvatarList_Tr.gameObject.SetActive(false);
                    }
                }
                AvatarList_Tr.sizeDelta = new Vector2(avatarTempSize.x, avatarTempSize.y);
            }
        }

        private static void OnDisable()
        {
            NickName_Btn.onClick.RemoveAllListeners();
            Cancel_Btn.onClick.RemoveAllListeners();
            Confirm_Btn.onClick.RemoveAllListeners();
            Avatar_Btn.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 頭像載入
        /// </summary>
        private static void LoadAvatar()
        {
            for (int i = 1; i < AvatarList_Rt.childCount; i++)
            {
                GameObject.Destroy(AvatarList_Rt.GetChild(i).gameObject);
            }

            HorizontalLayoutGroup group = AvatarList_Rt.GetComponent<HorizontalLayoutGroup>();
            avatarItemSize = AvatarSample_Btn.GetComponent<RectTransform>().rect.size;
            AvatarList_Rt.sizeDelta = new Vector2(0, avatarItemSize.y);
            ABManager.Instance.LoadSprite("entry", "AvatarList", (avatars) =>
            {
                avatarList = avatars;

                for (int i = 0; i < avatarList.Length; i++)
                {
                    int currentIndex = i;

                    GameObject avatarObj = GameObject.Instantiate<GameObject>(AvatarSample_Btn.gameObject);
                    avatarObj.transform.SetParent(AvatarList_Rt);
                    avatarObj.transform.localScale = Vector3.one;
                    avatarObj.SetActive(true);
                    Image img = FindConponent.FindObj<Image>(avatarObj.transform, "Item_Img");
                    img.sprite = avatarList[i];

                    //修改頭像
                    avatarObj.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        AudioManager.Instance.PlayButtonClick();

                        MainPack pack = new MainPack();
                        pack.RequestCode = RequestCode.User;
                        pack.ActionCode = ActionCode.ReviseUserInfo;

                        ReviseUserInfoPack reviseUserInfoPack = new ReviseUserInfoPack();
                        reviseUserInfoPack.ReviseName = "avatar";
                        reviseUserInfoPack.ReviseValue = currentIndex.ToString();

                        pack.ReviseUserInfoPack = reviseUserInfoPack;
                        thisView.view.SendRequest(pack);
                    });
                }

                //設定頭像列表目標大小
                avatarGridLayout = AvatarList_Rt.GetComponent<GridLayoutGroup>();
                avatarTargetSize = new Vector2(avatarGridLayout.spacing.x + (avatarGridLayout.cellSize.x + avatarGridLayout.spacing.x) * avatarWidthCound,
                                               avatarGridLayout.spacing.y + (avatarGridLayout.cellSize.y + avatarGridLayout.spacing.y) * ((avatarList.Length / avatarWidthCound) + 1));

                initAvatarListHeight = avatarGridLayout.cellSize.y + (avatarGridLayout.spacing.y * 2);
                AvatarList_Rt.sizeDelta = new Vector2(avatarTargetSize.x, avatarTargetSize.y);
            });
        }

        /// <summary>
        /// 按下修改暱稱確認
        /// </summary>
        private static void ClickConfirm()
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
                pack.ActionCode = ActionCode.ReviseUserInfo;

                ReviseUserInfoPack reviseUserInfoPack = new ReviseUserInfoPack();
                reviseUserInfoPack.ReviseName = "nickname";
                reviseUserInfoPack.ReviseValue = NickName_IF.text;

                pack.ReviseUserInfoPack = reviseUserInfoPack;
                thisView.view.SendRequest(pack);
            }
            else
            {
                UIManager.Instance.ShowTip("請輸入暱稱!!!");
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
                //更新用戶訊息
                case ActionCode.GetUserInfo:
                    NickName_Txt.text = pack.UserInfoPack[0].NickName;
                    Chips_Txt.text = Utils.Instance.SetChipsStr(pack.UserInfoPack[0].Cash);
                    Avatar_Img.sprite = avatarList[Convert.ToInt32(pack.UserInfoPack[0].Avatar)];
                    break;

                //修改用戶訊息
                case ActionCode.ReviseUserInfo:
                    if (pack.ReturnCode == ReturnCode.Succeed)
                    {
                        string reviseValue = pack.ReviseUserInfoPack.ReviseValue;
                        switch (pack.ReviseUserInfoPack.ReviseName)
                        {
                            case "avatar":
                                AvatarListSwitch = false;
                                Avatar_Img.sprite = avatarList[Convert.ToInt32(reviseValue)];
                                break;

                            case "nickname":
                                ReviseNickName_Tr.gameObject.SetActive(false);
                                NickName_Txt.text = reviseValue;
                                break;
                        }
                        
                    }
                    else if (pack.ReturnCode == ReturnCode.Duplicated)
                    {
                        switch (pack.ReviseUserInfoPack.ReviseName)
                        {
                            case "nickname":
                                UIManager.Instance.ShowTip("暱稱重複!!!");
                                break;
                        }                        
                    }
                    else
                    {
                        switch (pack.ReviseUserInfoPack.ReviseName)
                        {
                            case "avatar":
                                UIManager.Instance.ShowTip("修改暱稱失敗!!!");
                                break;

                            case "nickname":
                                UIManager.Instance.ShowTip("更換頭像失敗!!!");
                                break;
                        }                        
                    }
                    break;
            }
        }
    }
}
