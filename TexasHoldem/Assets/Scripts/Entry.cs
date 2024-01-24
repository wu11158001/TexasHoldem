using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entry : UnitySingleton<Entry>
{
    public override void Awake()
    {
        base.Awake();

        gameObject.AddComponent<ABManager>();
        gameObject.AddComponent<UIManager>();
    }
}
