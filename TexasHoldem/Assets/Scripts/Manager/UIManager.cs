using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Events;

public class UIManager : UnitySingleton<UIManager>
{
    private Transform canvas_Tr;

    private Dictionary<ViewType, BaseView> viewDic = new Dictionary<ViewType, BaseView>();
    private Stack<BaseView> viewStack = new Stack<BaseView>();

    //工具類View
    private ViewType[] toolsViewType = new ViewType[] 
    {
        ViewType.TipView, 
        ViewType.LoadingView,
        ViewType.WaitView,
        ViewType.ConfirmView,
        ViewType.SettingView,
    };
    private Dictionary<ViewType, BaseView> toolsViewDic = new Dictionary<ViewType, BaseView>();

    public override void Awake()
    {
        base.Awake();

        canvas_Tr = GameObject.FindObjectOfType<Canvas>().transform;  
    }

    /// <summary>
    /// 創建工具View
    /// </summary>
    async public Task CreateToolsView()
    {
        for (int i = 0; i < toolsViewType.Length; i++)
        {
            BaseView view = await CreateView(toolsViewType[i]);
            InitView(view, toolsViewType[i]);
            toolsViewDic.Add(toolsViewType[i], view);

            await Task.Delay(1);
        }

        Debug.Log("工具類View 創建完成。");
    }

    /// <summary>
    /// 顯示工具View
    /// </summary>
    /// <param name="viewType"></param>
    /// <returns></returns>
    public BaseView ShowToolView(ViewType viewType)
    {
        BaseView view = null;
        if (toolsViewDic.ContainsKey(viewType))
        {
            view = toolsViewDic[viewType];
            view.gameObject.SetActive(true);
            view.gameObject.transform.SetSiblingIndex(100);
        }
        else
        {
            Debug.LogError($"{viewType} 資源不存在!!!");
        }

        return view;
    }

    /// <summary>
    /// 顯示提示
    /// </summary>
    /// <param name="str"></param>
    public void ShowTip(string str)
    {
        BaseView view = ShowToolView(ViewType.TipView);
        if (view != null)
        {
            ((TipView)view).ShowTip(str);
        }      
    }

    /// <summary>
    /// 顯示載入畫面
    /// </summary>
    /// <param name="nextViewType"></param>
    /// <param name="isBack"></param>
    /// <returns></returns>
    async public Task ShowLoadingView(ViewType nextViewType, bool isBack = false)
    {
        BaseView view = ShowToolView(ViewType.LoadingView);
        ((LoadingView)view).OpenLoading();

        if (isBack)
        {
            BackView();
            ((LoadingView)view).CloseLoading();
        }
        else
        {
            
            if (view != null)
            {  
                BaseView nextView = await ShowView(nextViewType);
                nextView.gameObject.SetActive(false);
                ((LoadingView)view).CloseLoading(nextView.gameObject);
            }
        }
    }

    /// <summary>
    /// 顯示確認視窗
    /// </summary>
    /// <param name="confirmCallBack"></param>
    /// <param name="str"></param>
    /// <param name="isHaveCancel"></param>
    /// <returns></returns>
    public BaseView ShowConfirmView(UnityAction confirmCallBack, string str, bool isHaveCancel = true)
    {
        BaseView view = ShowToolView(ViewType.ConfirmView);
        if (view != null)
        {
            ((ConfirmView)view).SetConfirmView(confirmCallBack, str, isHaveCancel);
        }

        return view;
    }

    /// <summary>
    /// 等待畫面開關
    /// </summary>
    /// <param name=""></param>
    public void WaitViewSwitch(bool isOpen)
    {
        BaseView view = ShowToolView(ViewType.WaitView);
        ((WaitView)view).gameObject.SetActive(isOpen);
    }

    /// <summary>
    /// 顯示View
    /// </summary>
    /// <param name="viewType"></param>
    /// <returns></returns>
    async public Task<BaseView> ShowView(ViewType viewType)
    {
        BaseView view = null;
        if (viewStack.Count > 0)
        {
            view = viewStack.Peek();
            view.gameObject.SetActive(false);
        }
        
        if (viewDic.ContainsKey(viewType))
        {
            view = viewDic[viewType];
        }
        else
        {
            view = await CreateView(viewType);
            InitView(view, viewType);
        }

        view.gameObject.SetActive(true);
        viewStack.Push(view);
        return view;
    }

    //初始化View
    private void InitView(BaseView view, ViewType viewType)
    {
        view.gameObject.SetActive(false);      
        view.name = viewType.ToString();
        RectTransform rt = view.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.eulerAngles = Vector3.zero;
        rt.offsetMax = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    /// <summary>
    /// 創建View
    /// </summary>
    /// <param name="viewType"></param>
    /// <returns></returns>
    async private Task<BaseView> CreateView(ViewType viewType)
    {       
        if (viewDic.ContainsKey(viewType))
        { 
            return viewDic[viewType];
        }
        else
        {
            TaskCompletionSource<BaseView> tcs = new TaskCompletionSource<BaseView>();
            string abName = ViewABName.Instance.GetAbName(viewType);
            await ABManager.Instance.GetAB<GameObject>(abName, viewType.ToString(), (obj) =>
            {
                GameObject viewObj = Instantiate(obj);
                viewObj.transform.SetParent(canvas_Tr);
                viewObj.SetActive(false);
                BaseView baseView = viewObj.GetComponent<BaseView>();
                viewDic.Add(viewType, baseView);

                tcs.SetResult(baseView);
            });

            return await tcs.Task;
        }
    }

    /// <summary>
    /// 返回前個View
    /// </summary>
    public void BackView()
    {
        BaseView view = null;
        if (viewStack.Count > 0)
        {
            view = viewStack.Pop();
            view.gameObject.SetActive(false);
        }

        if (viewStack.Count > 0)
        {
            view = viewStack.Peek();
            view.gameObject.SetActive(true);
        }
    }
}
