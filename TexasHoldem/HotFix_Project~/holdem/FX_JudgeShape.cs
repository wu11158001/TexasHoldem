using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix_Project
{
    class FX_JudgeShape
    {
        private static FX_JudgeShape instance;
        public static FX_JudgeShape Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FX_JudgeShape();
                }
                return instance;
            }
        }

        private FX_JudgeShape() { }

        /// <summary>
        /// 獲取牌型
        /// </summary>
        /// <param name="handPoker"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public int GetCardShape(List<int> handPoker, List<int> poolPoker)
        {            
            if (poolPoker == null)
            {
                return (handPoker[0] - 1) % 13 == (handPoker[1] - 1) % 13 ? 8 : 9;
            }
            else
            {
                //手牌+牌池
                List<int> pokersList = handPoker.Concat(poolPoker).ToList();
                pokersList.Sort();
                for (int i = 0; i < pokersList.Count; i++)
                {
                    pokersList[i] -= 1;
                }

                if (IsRoyalFlush(pokersList))
                {
                    //皇家桐花順
                    return 0;
                }
                else if(IsStraightSuit(pokersList))
                {
                    //同花順
                    return 1;
                }
                else if (IsPair(pokersList, 4))
                {
                    //4條
                    return 2;
                }
                else if (IsPair(pokersList, 3) && IsPair(pokersList, 2))
                {
                    //葫蘆
                    return 3;
                }
                else if (IsSameSuit(pokersList) != null)
                {
                    //同花
                    return 4;
                }
                else if (IsStraight(pokersList))
                {
                    //順子
                    return 5;
                }
                else if (IsPair(pokersList, 3))
                {
                    //三條
                    return 6;
                }
                else if (isDoublePair(pokersList))
                {
                    //兩對
                    return 7;
                }
                else if (IsPair(pokersList, 2))
                {
                    //一對
                    return 8;
                }

                return 9;
            }
        }

        /// <summary>
        /// 是否為皇家桐花順
        /// </summary>
        /// <param name="poker"></param>
        /// <returns></returns>
        private bool IsRoyalFlush(List<int> poker)
        {
            List<int> sameSuit = IsSameSuit(poker);
            if (sameSuit != null)
            {
                List<int> royalFlush = new List<int> { 9, 10, 11, 12, 0 };
                int count = sameSuit.Count(card => royalFlush.Contains(card % 13));
                return count >= 5;
            }

            return false;
        }

        /// <summary>
        /// 是否為同花順
        /// </summary>
        /// <param name="poker"></param>
        /// <returns></returns>
        private bool IsStraightSuit(List<int> poker)
        {
            List<int> sameSuit = IsSameSuit(poker);
            if (sameSuit != null)
            {
                return IsStraight(sameSuit);
            }

            return false;
        }

        /// <summary>
        /// 是否為順子
        /// </summary>
        /// <param name="poker"></param>
        /// <returns></returns>
        private bool IsStraight(List<int> poker)
        {
            List<int> tempList = new List<int>(poker);
            for (int i = 0; i < tempList.Count; i++)
            {
                tempList[i] = tempList[i] % 13;
            }
            tempList.Sort();

            int max = 0;
            int count = 1;
            for (int i = 1; i < tempList.Count; i++)
            {
                count++;
                if ((tempList[i - 1] % 13) + 1 != (tempList[i] % 13))
                {
                    count = 1;
                }

                if (max < count)
                {
                    max = count;
                }
            }

            return max >= 5;
        }

        /// <summary>
        /// 是否為同花
        /// </summary>
        /// <param name=""></param>
        /// <param name="poker"></param>
        /// <returns></returns>
        private List<int> IsSameSuit(List<int> poker)
        {
            for (int i = 0; i < poker.Count; i++)
            {
                int suit = poker[i] / 13;
                int count = poker.Count(card => card / 13 == suit);
                if (count >= 5)
                {
                    return poker.Select(x => x)
                                .Where(card => card / 13 == suit)
                                .ToList();
                }
            }

            return null;
        }

        /// <summary>
        /// 是否為對子
        /// </summary>
        /// <param name="poker"></param>
        /// <param name="pair">判斷對子數</param>
        /// <returns></returns>
        private bool IsPair(List<int> poker, int pair)
        {
            for (int i = 0; i < poker.Count; i++)
            {
                int num = poker[i] % 13;
                int count = poker.Count(card => card % 13 == num);
                if (count == pair)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否兩對
        /// </summary>
        /// <param name="poker"></param>
        /// <returns></returns>
        private bool isDoublePair(List<int> poker)
        {
            List<int> tempList = new List<int>(poker);

            int count = 0;
            for (int i = 0; i < tempList.Count - 1; i++)
            {
                int num = tempList[i] % 13;
                for (int j = 1; j < tempList.Count; j++)
                {
                    if (num == tempList[j] % 13)
                    {
                        count++;
                        tempList.Remove(tempList[j]);
                        break;
                    }
                }

                tempList.Remove(tempList[i]);
                i = 0;
            }

            return count >= 2;
        }
    }
}
