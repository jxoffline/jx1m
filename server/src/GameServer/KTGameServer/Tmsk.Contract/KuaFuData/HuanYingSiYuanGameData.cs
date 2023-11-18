using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KF.Contract.Interface;

namespace KF.Contract.Data.HuanYingSiYuan
{
    [Serializable]
    public class HuanYingSiYuanGameData : IGameData
    {
        public int ZhanDouLi;

        public object Clone()
        {
            return new HuanYingSiYuanGameData()
                {
                    ZhanDouLi = ZhanDouLi,
                };
        }

        public static int GetZhanDouLi(IGameData gameData)
        {
            int zhanDouLi = 0;
            if (null != gameData)
            {
                HuanYingSiYuanGameData huanYingSiYuanGameData = gameData as HuanYingSiYuanGameData;
                if (null != huanYingSiYuanGameData)
                {
                    zhanDouLi = huanYingSiYuanGameData.ZhanDouLi;
                }
            }

            return zhanDouLi;
        }
    }
}
