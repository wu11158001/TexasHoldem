using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix_Project
{
    class FX_BaseView
    {
        public BaseView view = null;
        public GameObject obj = null;

        public virtual FX_BaseView SetObj(BaseView baseView, GameObject viewObj)
        {
            view = baseView;
            obj = viewObj;

            return this;
        }
    }
}
