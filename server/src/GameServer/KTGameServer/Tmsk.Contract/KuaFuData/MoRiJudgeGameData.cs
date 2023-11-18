using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract.Interface;

namespace KF.Contract.Data
{
    [Serializable]
    public class MoRiJudgeGameData : IGameData
    {
        public int ZhanDouLi;

        public object Clone()
        {
            return new TianTiGameData()
            {
                ZhanDouLi = ZhanDouLi,
            };
        }
    }
}
