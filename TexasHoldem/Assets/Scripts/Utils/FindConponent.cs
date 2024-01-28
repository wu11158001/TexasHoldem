using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FindConponent
{
    public static T FindObj<T>(this Transform searchObj, string searchName) where T : Component
    {
        for (int i = 0; i < searchObj.childCount; i++)
        {
            if (searchObj.GetChild(i).childCount > 0)
            {
                var obj = searchObj.GetChild(i).FindObj<Transform>(searchName);
                if (obj != null) return obj.GetComponent<T>();
            }

            if (searchObj.GetChild(i).name == searchName)
            {
                return searchObj.GetChild(i).GetComponent<T>();
            }
        }

        return default;
    }
}
