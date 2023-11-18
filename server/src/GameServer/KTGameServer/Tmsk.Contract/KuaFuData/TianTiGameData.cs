using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KF.Contract.Interface;
using ProtoBuf;

namespace KF.Contract.Data
{
    [Serializable]
    public class TianTiGameData : IGameData
    {
        public int ZhanDouLi;
        public string RoleName;

        public object Clone()
        {
            return new TianTiGameData()
                {
                    ZhanDouLi = ZhanDouLi,
                };
        }
    }

    /// <summary>
    /// 天梯角色信息
    /// </summary>
    [Serializable]
    public class TianTiRoleInfoData
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public int RoleId;

        /// <summary>
        /// 角色名
        /// </summary>
        public string RoleName;

        /// <summary>
        /// 区号
        /// </summary>
        public int ZoneId;

        /// <summary>
        /// 段位ID
        /// </summary>
        public int DuanWeiId;

        /// <summary>
        /// 段位积分
        /// </summary>
        public int DuanWeiJiFen;

        /// <summary>
        /// 段位排名
        /// </summary>
        public int DuanWeiRank;

        /// <summary>
        /// 战力
        /// </summary>
        public int ZhanLi;

        /// <summary>
        /// 保存天梯相关数据(ProtoBuf序列化TianTiPaiHangRoleData)
        /// </summary>
        public byte[] TianTiPaiHangRoleData;

        /// <summary>
        /// 玩家竞技数据镜像，众神争霸使用
        /// </summary>
        public byte[] PlayerJingJiMirrorData;
    }

    [Serializable]
    public class TianTiRankData
    {
        public DateTime ModifyTime;
        public int MaxPaiMingRank;
        public List<TianTiRoleInfoData> TianTiRoleInfoDataList;
        public List<TianTiRoleInfoData> TianTiMonthRoleInfoDataList;
    }
}
