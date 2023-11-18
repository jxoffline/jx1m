using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract.Data;

namespace KF.Remoting
{
    class CoupleArenaJoinDataUtil
    {
        /// <summary>
        /// 匹配序列号，仅仅中心使用
        /// </summary>
        private uint CurrJoinSeq = 1;

        /// <summary>
        /// key: join seq ---> join data
        /// </summary>
        private Dictionary<uint, CoupleArenaJoinData> JoinDataDict = new Dictionary<uint, CoupleArenaJoinData>();

        /// <summary>
        /// key: roleid ---> join seq
        /// </summary>
        private Dictionary<int, uint> RoleJoinSeqDict = new Dictionary<int, uint>();

        public CoupleArenaJoinData Create()
        {
            CoupleArenaJoinData joinData = new CoupleArenaJoinData();
            joinData.JoinSeq = CurrJoinSeq;
            CurrJoinSeq++;
            return joinData;
        }

        public List<CoupleArenaJoinData> GetJoinList()
        {
            return JoinDataDict.Values.ToList();
        }

        public void Reset()
        {
            this.JoinDataDict.Clear();
            this.RoleJoinSeqDict.Clear();
        }

        public void AddJoinData(CoupleArenaJoinData joinData)
        {
            if (joinData == null) return;

            this.JoinDataDict.Add(joinData.JoinSeq, joinData);
            this.RoleJoinSeqDict[joinData.RoleId1] = joinData.JoinSeq;
            this.RoleJoinSeqDict[joinData.RoleId2] = joinData.JoinSeq;
        }

        public void DelJoinData(CoupleArenaJoinData joinData)
        {
            if (joinData == null) return;

            if (this.JoinDataDict.Remove(joinData.JoinSeq))
            {
                this.RoleJoinSeqDict.Remove(joinData.RoleId1);
                this.RoleJoinSeqDict.Remove(joinData.RoleId2);
            }
        }

        public CoupleArenaJoinData GetJoinData(int roleId)
        {
            uint joinSeq;
            if (this.RoleJoinSeqDict.TryGetValue(roleId, out joinSeq))
            {
                CoupleArenaJoinData joinData;
                if (this.JoinDataDict.TryGetValue(joinSeq, out joinData))
                    return joinData;
            }

            return null;
        }
    }
}
