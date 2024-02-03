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

        private static Transform ReviseNickName_Tr, AvatarSample_Tr;
        private static Button NickName_Btn, Cancel_Btn, Confirm_Btn, Avatar_Btn;
        private static Text NickName_Txt, Cash_Txt;
        private static InputField NickName_IF;
        private static RectTransform AvatarList_Rt;
        private static Image Avatar_Img;

        private static float avatarListWidth;
        private static Vector2 avatarItemSize;

        private static List<Sprite> avatarList = new List<Sprite>();

        private static void Init(BaseView baseView, GameObject viewObj)
        {
            thisView = new FX_BaseView().SetObj(baseView, viewObj);

            ReviseNickName_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "ReviseNickName_Tr");
            AvatarSample_Tr = FindConponent.FindObj<Transform>(thisView.view.transform, "AvatarSample_Tr");
            AvatarList_Rt = FindConponent.FindObj<RectTransform>(thisView.view.transform, "AvatarList_Rt");
            NickName_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "NickName_Btn");
            Cancel_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Cancel_Btn");
            Confirm_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Confirm_Btn");
            Avatar_Btn = FindConponent.FindObj<Button>(thisView.view.transform, "Avatar_Btn");
            NickName_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "NickName_Txt");
            Cash_Txt = FindConponent.FindObj<Text>(thisView.view.transform, "Cash_Txt");
            NickName_IF = FindConponent.FindObj<InputField>(thisView.view.transform, "NickName_IF");
            Avatar_Img = FindConponent.FindObj<Image>(thisView.view.transform, "Avatar_Img");

            AvatarSample_Tr.gameObject.SetActive(false);

            LoadAvatar();   
        }

        private static void OnEnable()
        {
            ReviseNickName_Tr.gameObject.SetActive(false);
            AvatarList_Rt.gameObject.SetActive(false);

            MainPack pack = new MainPack();
            pack.RequestCode = RequestCode.User;
            pack.ActionCode = ActionCode.GetUserInfo;
            thisView.view.SendRequest(pack);
        }

        private static void Start()
        {
            NickName_Btn.onClick.AddListener(() =>
            {
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
                ClickConfirm();

            });

            Avatar_Btn.onClick.AddListener(() =>
            {
                if (AvatarList_Rt.gameObject.activeSelf)
                {
                    AvatarList_Rt.gameObject.SetActive(false);
                }
                else
                {
                    AvatarList_Rt.gameObject.SetActive(true);
                    AvatarList_Rt.sizeDelta = new Vector2(0, avatarItemSize.y);
                }                
            });
        }

        private static void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                ClickConfirm();
            }

            if (AvatarList_Rt.gameObject.activeSelf)
            {
                if (AvatarList_Rt.rect.width < avatarListWidth)
                {
                    float width = AvatarList_Rt.rect.width + 10;
                    AvatarList_Rt.sizeDelta = new Vector2(width, avatarItemSize.y);
                }
            }
        }

        /// <summary>
        /// 頭像載入
        /// </summary>
        private static void LoadAvatar()
        {
            HorizontalLayoutGroup group = AvatarList_Rt.GetComponent<HorizontalLayoutGroup>();
            avatarItemSize = AvatarSample_Tr.GetComponent<RectTransform>().rect.size;
            AvatarList_Rt.sizeDelta = new Vector2(0, avatarItemSize.y);
            for (int i = 0; i < 5; i++)
            {
                int currentIndex = i;
                ABManager.Instance.GetAB<Sprite>("entry", $"Avatar{i}", (avatar) =>
                {
                    avatarList.Add(avatar);

                    GameObject avatarObj = GameObject.Instantiate<GameObject>(AvatarSample_Tr.gameObject);
                    avatarObj.transform.SetParent(AvatarList_Rt);
                    avatarObj.SetActive(true);
                    avatarListWidth += avatarItemSize.x + group.spacing;
                    Image img = FindConponent.FindObj<Image>(avatarObj.transform, "Item_Img");
                    img.sprite = avatar;
                    avatarObj.GetComponent<Button>().onClick.AddListener(delegate
                    {
                        MainPack pack = new MainPack();
                        pack.RequestCode = RequestCode.User;
                        pack.ActionCode = ActionCode.ReviseAvatar;

                        UserInfoPack userInfoPack = new UserInfoPack();
                        userInfoPack.Avatar = currentIndex.ToString();

                        pack.UserInfoPack.Add(userInfoPack);
                        thisView.view.SendRequest(pack);

                        Debug.LogError("發送頭像" + currentIndex.ToString());
                    });
                });
            }
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
                    Cash_Txt.text = pack.UserInfoPack[0].Cash;
                    Avatar_Img.sprite = avatarList[Convert.ToInt32(pack.UserInfoPack[0].Avatar)];
                    break;

                //修改暱稱
                case ActionCode.ReviseNickName:
                    if (pack.ReturnCode == ReturnCode.Succeed)
                    {
                        ReviseNickName_Tr.gameObject.SetActive(false);
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

                //修改頭像
                case ActionCode.ReviseAvatar:
                    if (pack.ReturnCode == ReturnCode.Succeed)
                    {
                        AvatarList_Rt.gameObject.SetActive(false);
                        Avatar_Img.sprite = avatarList[Convert.ToInt32(pack.UserInfoPack[0].Avatar)];
                    }
                    else
                    {
                        UIManager.Instance.ShowTip("更換頭像失敗!!!");
                    }
                    break;
            }
        }
    }
}
