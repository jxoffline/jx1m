using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract.Data;

namespace KF.Remoting
{
    class CoupleArenaJoinMatcher
    {
        /// <summary>
        /// key: union duanwei
        /// value: join data list
        /// </summary>
        private Dictionary<int, List<CoupleArenaJoinData>> JoinDatasDict = new Dictionary<int, List<CoupleArenaJoinData>>();

        /// <summary>
        /// global union duanwei
        /// </summary>
        private readonly int GlobalChannel = -1;

        public void AddJoinData(int duanweiType, int duanWeiLevel, CoupleArenaJoinData joinData)
        {
            AddJoinData(MakeChannel(duanweiType, duanWeiLevel), joinData);
        }

        public void AddGlobalJoinData(CoupleArenaJoinData joinData)
        {
            AddJoinData(GlobalChannel, joinData);
        }

        public IEnumerable<List<CoupleArenaJoinData>> GetAllMatch()
        {
            foreach (var channel in JoinDatasDict.Keys.ToList())
            {
                yield return GetJoinDataList(channel);
            }
        }

        private void AddJoinData(int channel, CoupleArenaJoinData joinData)
        {
            if (joinData == null) return;
            GetJoinDataList(channel).Add(joinData);
        }

        private List<CoupleArenaJoinData> GetJoinDataList(int unionDuanwei)
        {
            List<CoupleArenaJoinData> result;
            if (!JoinDatasDict.TryGetValue(unionDuanwei, out result))
            {
                result = new List<CoupleArenaJoinData>();
                JoinDatasDict.Add(unionDuanwei, result);
            }

            return result;
        }

        private int MakeChannel(int duanweiType, int duanweiLevel)
        {
            return duanweiType * 1000 + duanweiLevel;
        }
    }
}
