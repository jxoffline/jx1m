using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Core.Executor;
using KF.Contract.Data;
using Tmsk.Contract;

namespace KF.Remoting.Data
{
    public static class Global
    {
        public static readonly int UninitGameId = -1111;

        private static DateTime _NowTime = TimeUtil.NowDateTime();
        public static DateTime NowTime { get { return _NowTime; } }

        public static void UpdateNowTime(DateTime nowTime)
        {
            _NowTime = nowTime;
        }

        #region 全局的随机数

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

        #endregion //全局的随机数
    }
}
