using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix_Project.View
{
    class FX_BaseView
    {
        protected static BaseView view = null;
        protected static GameObject obj = null;

        public virtual void SetObj(BaseView baseView, GameObject viewObj)
        {
            view = baseView;
            obj = viewObj;
        }
    }
}
