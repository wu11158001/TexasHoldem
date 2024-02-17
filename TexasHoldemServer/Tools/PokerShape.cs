using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasHoldemServer.Tools
{
    class PokerShape
    {
        private static PokerShape instance;
        public static PokerShape Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new PokerShape();
                }
                return instance;
            }
        }

        private PokerShape() { }

        /// <summary>
        /// 獲取牌型
        /// </summary>
        /// <param name="handPoker"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public int GetCardShape(int[] handPoker, int[] poolPoker)
        {
            if (poolPoker == null)
            {
                return handPoker[0] % 13 == handPoker[1] % 13 ? 8 : 9;
            }
            else
            {
                //手牌+牌池
                List<int> pokersList = handPoker.Concat(poolPoker).ToList();
                pokersList.Sort();

                if (IsRoyalFlush(pokersList))
                {
                    //皇家桐花順
                    return 0;
                }
                else if (IsStraightSuit(pokersList))
                {
                    //同花順
                    return 1;
                }
                else if (IsPair(pokersList, 4) != -1)
                {
                    //4條
                    return 2;
                }
                else if (IsPair(pokersList, 3) != -1 && IsPair(pokersList, 2) != -1)
                {
                    //葫蘆
                    return 3;
                }
                else if (IsSameSuit(pokersList) != null)
                {
                    //同花
                    return 4;
                }
                else if (IsStraight(pokersList).Count >= 5)
                {
                    //順子
                    return 5;
                }
                else if (IsPair(pokersList, 3) != -1)
                {
                    //三條
                    return 6;
                }
                else if (isDoublePair(pokersList).Count >= 2)
                {
                    //兩對
                    return 7;
                }
                else if (IsPair(pokersList, 2) != -1)
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
                return IsStraight(sameSuit).Count >= 5;
            }

            return false;
        }

        /// <summary>
        /// 是否為順子
        /// </summary>
        /// <param name="poker"></param>
        /// <returns></returns>
        private List<int> IsStraight(List<int> poker)
        {
            List<int> tempList = new List<int>(poker);
            for (int i = 0; i < tempList.Count; i++)
            {
                tempList[i] = tempList[i] % 13;
            }
            tempList = tempList.OrderByDescending(x => x).ToList();

            List<int> straightList = new List<int>();
            straightList.Add(tempList[0]);
            for (int i = 1; i < tempList.Count; i++)
            {
                int num = (tempList[i - 1] % 13) - 1;
                if (num >= 13)
                {
                    num = 0;
                }
                if (num != (tempList[i] % 13))
                {
                    straightList.Clear();
                }

                int addNum = tempList[i] % 13;
                if (addNum == 0)
                {
                    //將1最大化
                    addNum = 13;
                }
                straightList.Add(addNum);

                if (straightList.Count >= 5)
                {
                    return straightList;
                }
            }

            return straightList;
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
                    List<int> result = poker.Select(x => x)
                                            .Where(card => card / 13 == suit)
                                            .ToList();

                    return result.OrderByDescending(x => x).ToList(); ;
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
        private int IsPair(List<int> poker, int pair)
        {
            int pairNum = -1;
            for (int i = 0; i < poker.Count; i++)
            {
                int num = poker[i] % 13;
                int count = poker.Count(card => card % 13 == num);
                if (count == pair)
                {
                    return poker.GroupBy(x => x % 13)
                                .Where(g => g.Count() == pair)
                                .SelectMany(g => g)
                                .FirstOrDefault();
                }
            }

            return pairNum;
        }

        /// <summary>
        /// 是否兩對
        /// </summary>
        /// <param name="poker"></param>
        /// <returns></returns>
        private List<int> isDoublePair(List<int> poker)
        {
            List<int> tempList = new List<int>(poker);
            List<int> pairNum = new List<int>();

            for (int i = 0; i < tempList.Count - 1; i++)
            {
                int num = tempList[i] % 13;
                for (int j = 1; j < tempList.Count; j++)
                {
                    if (num == tempList[j] % 13)
                    {
                        int pokerNum = tempList[j] % 13;                        
                        tempList.Remove(tempList[j]);
                        if (pokerNum == 0) pokerNum = 13;
                        pairNum.Add(pokerNum);
                        break;
                    }
                }

                tempList.Remove(tempList[i]);
                i = -1;
            }

            return pairNum.OrderByDescending(x => x).Take(2).ToList();
        }

        /// <summary>
        /// 比較同花順
        /// </summary>
        /// <param name="handPokerList"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public List<string> CompareStraightSuit(Dictionary<string, int[]> handPokerList, int[] poolPoker)
        {
            int maxPoker = 0;
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (var handPoker in handPokerList)
            {
                List<int> poker = handPoker.Value.Concat(poolPoker).ToList();
                int max = IsStraight(IsSameSuit(poker)).Max();
                if (maxPoker < max)
                {
                    maxPoker = max;
                }
                dic.Add(handPoker.Key, max);
            }

            return dic.Where(x => x.Value == maxPoker)
                      .Select(x => x.Key)
                      .ToList();
        }

        /// <summary>
        /// 比較順子
        /// </summary>
        /// <param name="handPokerList"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public List<string> CompareStraight(Dictionary<string, int[]> handPokerList, int[] poolPoker)
        {
            int maxPoker = 0;
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (var handPoker in handPokerList)
            {
                List<int> poker = handPoker.Value.Concat(poolPoker).ToList();
                int max = IsStraight(poker).Max();
                if (maxPoker < max)
                {
                    maxPoker = max;
                }
                dic.Add(handPoker.Key, max);
            }

            return dic.Where(x => x.Value == maxPoker)
                      .Select(x => x.Key)
                      .ToList();
        }

        /// <summary>
        /// 比較對子
        /// </summary>
        /// <param name="handPokerList"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public List<string> ComparePair(Dictionary<string, int[]> handPokerList, int[] poolPoker, int paitCount)
        {
            int maxPoker = 0;
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (var handPoker in handPokerList)
            {
                List<int> poker = handPoker.Value.Concat(poolPoker).ToList();

                int num = IsPair(poker, paitCount) % 13;
                if (num == 0) num = 13;
                if (maxPoker < num) maxPoker = num;

                dic.Add(handPoker.Key, num);
            }

            return dic.Where(x => x.Value == maxPoker)
                      .Select(x => x.Key)
                      .ToList();
        }

        /// <summary>
        /// 比較葫蘆
        /// </summary>
        /// <param name="handPokerList"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public List<string> CompareGourd(Dictionary<string, int[]> handPokerList, int[] poolPoker)
        {
            int maxThire = 0;
            int maxTwo = 0;
            //暱稱(三條, 兩條)
            Dictionary<string, (int, int)> dic = new Dictionary<string, (int, int)>();
            foreach (var handPoker in handPokerList)
            {
                List<int> poker = handPoker.Value.Concat(poolPoker).ToList();

                int thire = IsPair(poker, 3) % 13;
                if (thire == 0) thire = 13;
                if (maxThire < thire) maxThire = thire;

                int two = IsPair(poker, 2) % 13;
                if (two == 0) two = 13;
                if (maxTwo < two) maxTwo = two;

                dic.Add(handPoker.Key, (thire, two));
            }

            return FindKeys(dic, maxThire, maxTwo);

            //找出Key
            List<string> FindKeys(Dictionary<string, (int, int)> pokerDic, int value1, int value2)
            {
                List<string> keysWithValue1 = pokerDic.Where(kv => kv.Value.Item1 == value1).Select(kv => kv.Key).ToList();

                if (keysWithValue1.Count > 1)
                {
                    // 如果有多個鍵擁有相同的value1，再比較value2
                    List<string> keysWithMatchingValue2 = keysWithValue1.Where(key => pokerDic[key].Item2 == value2).ToList();
                    return keysWithMatchingValue2;
                }
                else
                {
                    return keysWithValue1;
                }
            }
        }

        /// <summary>
        /// 比較同花
        /// </summary>
        /// <param name="handPokerList"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public List<string> CompareSameSuit(Dictionary<string, int[]> handPokerList, int[] poolPoker)
        {
            Dictionary<string, List<int>> dic = new Dictionary<string, List<int>>();
            foreach (var handPoker in handPokerList)
            {
                List<int> poker = handPoker.Value.Concat(poolPoker).ToList();
                List<int> suit = IsSameSuit(poker);
                dic.Add(handPoker.Key, new List<int>());
                for (int i = 0; i < suit.Count; i++)
                {
                    if (suit[i] == 0) suit[i] = 13;
                    dic[handPoker.Key].Add(suit[i]);
                }
            }

            return ToolGetMaxKey(dic, 5);
        }

        /// <summary>
        /// 比較兩對
        /// </summary>
        /// <param name="handPokerList"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public List<string> CompareDoublePair(Dictionary<string, int[]> handPokerList, int[] poolPoker)
        {
            Dictionary<string, List<int>> dic = new Dictionary<string, List<int>>();
            foreach (var handPoker in handPokerList)
            {
                List<int> poker = handPoker.Value.Concat(poolPoker).ToList();
                List<int> pait = isDoublePair(poker);
                dic.Add(handPoker.Key, pait);
            }

            return ToolGetMaxKey(dic, 2);
        }

        /// <summary>
        /// 比對高牌
        /// </summary>
        /// <param name="handPokerList"></param>
        /// <param name="poolPoker"></param>
        /// <returns></returns>
        public List<string> CompareHighCard(Dictionary<string, int[]> handPokerList, int[] poolPoker)
        {
            Dictionary<string, List<int>> dic = new Dictionary<string, List<int>>();
            foreach (var handPoker in handPokerList)
            {
                List<int> poker = handPoker.Value.Concat(poolPoker).ToList();
                for (int i = 0; i < poker.Count; i++)
                {
                    poker[i] = poker[i] % 13;
                    if (poker[i] == 0) poker[i] = 13;
                }
                dic.Add(handPoker.Key, poker.OrderByDescending(x => x).ToList());
            }

            return ToolGetMaxKey(dic, 5);
        }

        /// <summary>
        /// 工具類_降序排列中最大值的key
        /// </summary>
        /// <param name="pokerDic"></param>
        /// <param name="selectItem">預設0</param>
        /// <param name="retrunNum">遞迴次數</param>
        /// <returns></returns>
        private List<string> ToolGetMaxKey(Dictionary<string, List<int>> pokerDic, int retrunNum, int selectItem = 0)
        {
            if (selectItem >= retrunNum)
            {
                return pokerDic.Select(kv => kv.Key).ToList();
            }

            int max = -1;
            Dictionary<string, List<int>> findDic = new Dictionary<string, List<int>>();
            max = pokerDic.Values.Max(t => t[selectItem]);
            findDic = pokerDic.Where(kv => kv.Value[selectItem] == max)
                              .ToDictionary(kv => kv.Key, kv => kv.Value);

            if (findDic.Count > 1)
            {
                return ToolGetMaxKey(findDic, retrunNum, selectItem + 1);
            }

            return findDic.Select(kv => kv.Key).ToList();
        }
    }
}
