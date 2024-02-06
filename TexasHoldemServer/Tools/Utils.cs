using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldemServer.Tools
{
    public static class Utils
    {
        /// <summary>
        /// 字串加法
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static string StringAddition(string v1, string v2)
        {
            StringBuilder sb = new StringBuilder();
            Sum(v1.Length - 1, v2.Length - 1, false);
            return sb.ToString();

            // 相加
            void Sum(int index1, int index2, bool isCarry)
            {
                if (index1 < 0 && index2 < 0 && !isCarry)
                {
                    return;
                }

                int num1 = index1 >= 0 ? Convert.ToInt32(v1[index1].ToString()) : 0;
                int num2 = index2 >= 0 ? Convert.ToInt32(v2[index2].ToString()) : 0;

                int sum = num1 + num2 + (isCarry ? 1 : 0);

                sb.Insert(0, (sum % 10).ToString());

                Sum(index1 - 1, index2 - 1, sum / 10 >= 1);
            }
        }

        /// <summary>
        /// 字串減法
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static string StringSubtract(string v1, string v2)
        {
            StringBuilder sb = new StringBuilder();

            // 將兩個字串補零至相同長度
            int maxLength = Math.Max(v1.Length, v2.Length);
            v1 = v1.PadLeft(maxLength, '0');
            v2 = v2.PadLeft(maxLength, '0');

            Subtract(v1.Length - 1, v2.Length - 1, false);
            // 移除前導的零
            int startIdx = 0;
            while (startIdx < sb.Length - 1 && sb[startIdx] == '0')
            {
                startIdx++;
            }
            return sb.ToString().Substring(startIdx);

            // 相減
            void Subtract(int index1, int index2, bool isBorrow)
            {
                if (index1 < 0 && index2 < 0)
                {
                    return;
                }

                int num1 = Convert.ToInt32(v1[index1].ToString());
                int num2 = Convert.ToInt32(v2[index2].ToString());

                int difference = num1 - (num2 + (isBorrow ? 1 : 0));

                if (difference < 0)
                {
                    difference += 10;
                    isBorrow = true;
                }
                else
                {
                    isBorrow = false;
                }

                sb.Insert(0, difference.ToString());

                Subtract(index1 - 1, index2 - 1, isBorrow);
            }
        }
    }
}
