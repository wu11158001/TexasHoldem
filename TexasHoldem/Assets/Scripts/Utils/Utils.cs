using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

        //相加
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

    /// <summary>
    /// 字串乘法
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <returns></returns>
    public static string StringMultiplication(string v1, string v2)
    {
        if (string.IsNullOrEmpty(v1) || string.IsNullOrEmpty(v2))
        {
            return "0";//若有一個為空字符串，則結果為0
        }

        int m = v1.Length;
        int n = v2.Length;
        int[] result = new int[m + n];

        //進行乘法運算
        for (int i = m - 1; i >= 0; i--)
        {
            for (int j = n - 1; j >= 0; j--)
            {
                int num1 = v1[i] - '0';
                int num2 = v2[j] - '0';
                int mul = num1 * num2;
                int sum = mul + result[i + j + 1];

                result[i + j] += sum / 10; //進位
                result[i + j + 1] = sum % 10;
            }
        }

        //將結果轉為字符串
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < result.Length; i++)
        {
            int digit = result[i];
            sb.Append(digit);
        }

        //移除前導零
        int startIndex = 0;
        while (startIndex <= sb.Length - 1 && sb[startIndex] == '0')
        {
            startIndex++;
        }

        return sb.ToString().Substring(startIndex);
    }
}
