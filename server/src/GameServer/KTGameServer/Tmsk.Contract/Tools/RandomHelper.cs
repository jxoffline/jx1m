using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Tools.Tools
{
    public class RandomHelper
    {
        /// <summary>
        /// 全局的随机数对象
        /// </summary>
        private static Random GlobalRand = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// 获取全局的随机数
        /// </summary>
        /// <param name="minV"></param>
        /// <param name="MaxV"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int minV, int maxV)
        {
            if (minV == maxV) return minV; //如果相等，则直接返回
            if (minV > maxV) return maxV;

            int ret = minV;
            lock (GlobalRand)
            {
                ret = GlobalRand.Next(minV, maxV);
            }

            return ret;
        }
    }
}
