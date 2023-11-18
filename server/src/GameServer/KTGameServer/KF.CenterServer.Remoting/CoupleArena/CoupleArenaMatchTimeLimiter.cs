using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Tmsk.Contract;

namespace KF.Remoting
{
    class CoupleArenaMatchTimeLimiter
    {
        /// <summary>
        ///
        /// </summary>
        private Dictionary<long, Dictionary<long, int>> TimesDict = new Dictionary<long, Dictionary<long, int>>();

        /// <summary>
        /// 获取夫妻间匹配次数
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public int GetMatchTimes(int a1, int a2, int b1, int b2)
        {
            long min, max;
            GetUnionCouple2(a1, a2, b1, b2, out min, out max);
            Dictionary<long, int> dict = null;
            int times = 0;
            if (TimesDict.TryGetValue(min, out dict) && dict.TryGetValue(max, out times))
                return times;
            return 0;
        }

        /// <summary>
        /// 增加夫妻间匹配次数
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        public void AddMatchTimes(int a1, int a2, int b1, int b2, int times = 1)
        {
            long min, max;
            GetUnionCouple2(a1, a2, b1, b2, out min, out max);
            Dictionary<long, int> dict = null;
            if (!TimesDict.TryGetValue(min, out dict))
                TimesDict[min] = dict = new Dictionary<long, int>();

            if (dict.ContainsKey(max)) dict[max] += times;
            else dict.Add(max, times);
        }

        /// <summary>
        /// 重置夫妻间匹配次数
        /// </summary>
        public void Reset()
        {
            this.TimesDict.Clear();
        }

        private void GetUnionCouple2(int a1, int a2, int b1, int b2, out long min, out long max)
        {
            long l1 = GetUnionCouple(a1, a2);
            long l2 = GetUnionCouple(b1, b2);
            min = Math.Min(l1, l2);
            max = Math.Max(l1, l2);
        }

        private long GetUnionCouple(int a1, int a2)
        {
            int min = Math.Min(a1, a2);
            int max = Math.Max(a1, a2);

            long v = min;
            v = v << 32;
            v = v | (uint)max;

            return v;
        }
    }
}
