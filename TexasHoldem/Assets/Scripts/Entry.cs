using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class Entry : UnitySingleton<Entry>
{
    public bool isDeleteAssetBundle;

    public override void Awake()
    {
        base.Awake();

        GameObject audioManaager = new GameObject();
        audioManaager.name = "AudioManager";
        audioManaager.AddComponent<AudioManager>();
        gameObject.AddComponent<ViewABName>();
        gameObject.AddComponent<ILRuntimeManager>();
        gameObject.AddComponent<ABManager>();
        gameObject.AddComponent<UIManager>();

        Init();
    }

    async private void Init()
    {
        await ABManager.Instance.Init();
        await ILRuntimeManager.Instance.Init();

        Debug.Log("開始連線...");
        gameObject.AddComponent<ClientManager>();
        gameObject.AddComponent<RequestManager>();

        await UIManager.Instance.CreateToolsView();
        await UIManager.Instance.ShowView(ViewType.LoginView);

        AudioManager.Instance.PlayBGM();
    }

    private void OnDestroy()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        Debug.Log("移除AB資源");
    }
}
