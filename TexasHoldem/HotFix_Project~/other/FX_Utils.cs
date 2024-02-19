using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotFix_Project
{
    class FX_Utils
    {
        private static FX_Utils instance;
        public static FX_Utils Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FX_Utils();
                }
                return instance;
            }
        }

        private FX_Utils() { }

        /// <summary>
        /// 設定籌碼數字
        /// </summary>
        /// <param name="chips"></param>
        /// <returns></returns>
        public string SetChipsStr(string chips)
        {
            StringBuilder sb = new StringBuilder();

            int count = 0;
            for (int i = chips.Length - 1; i >= 0; i--)
            {
                sb.Insert(0, chips[i]);

                count++;
                if (count == 3 && i != 0)
                {
                    sb.Insert(0, ",");
                    count = 0;
                }
            }

            return sb.ToString();
        }
    }
}
