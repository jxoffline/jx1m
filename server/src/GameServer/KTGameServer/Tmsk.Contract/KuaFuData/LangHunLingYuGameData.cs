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
    public class LangHunLingYuGameData : IGameData
    {
        public int ZhanDouLi;
        public string RoleName;

        public object Clone()
        {
            return new LangHunLingYuGameData()
                {
                    ZhanDouLi = ZhanDouLi,
                };
        }
    }

    [Serializable]
    public class LangHunLingYuCityDataEx : IGameData
    {
        public int CityId;
        public int CityLevel;
        public long[] Site = new long[Consts.LangHunLingYuCitySiteCount];
        public int GameId;

        public object Clone()
        {
            LangHunLingYuCityDataEx obj = new LangHunLingYuCityDataEx()
                {
                    GameId = GameId,
                    CityId = CityId,
                    CityLevel = CityLevel,
                };
            Array.Copy(Site, obj.Site, Site.Length);
            return obj;
        }
    }

    [Serializable]
    public class LangHunLingYuBangHuiDataEx : IGameData
    {
        public int Bhid;
        public string BhName;
        public int ZoneId;
        public int Level;

        public object Clone()
        {
            return new LangHunLingYuBangHuiDataEx()
                {
                    Bhid = Bhid,
                    BhName = BhName,
                    ZoneId = ZoneId,
                    Level = Level,
                };
        }
    }

    /// <summary>
    /// 狼魂领域圣域城主历史数据
    /// </summary>
    [Serializable]
    public class LangHunLingYuKingHist
    {
        // 城主角色ID
        public int rid;

        // 膜拜次数
        public int AdmireCount;

        // 获得时间
        public DateTime CompleteTime;

        // 圣域城主相关数据(ProtoBuf序列化RoleDataEx)
        public byte[] CityOwnerRoleData;
    }

    /// <summary>
    /// 勇者战场角色信息
    /// </summary>
    [Serializable]
    public class LangHunLingYuStatisticalData
    {
        /// <summary>
        /// 场次ID
        /// </summary>
        public int GameId;

        public int CityId;

        public int[] SiteBhids = new int[Consts.LangHunLingYuCitySiteCount];

        public DateTime CompliteTime;

        // 圣域城主 角色ID
        public int rid;

        // 圣域城主 RoleDataEx信息
        public byte[] CityOwnerRoleData;
    }

    [Serializable]
    public class LangHunLingYuResultData
    {
        public int Result;
        public LangHunLingYuCityDataEx CityDataEx;
        public List<LangHunLingYuBangHuiDataEx> BangHuiDataExList;
    }

    [Serializable]
    public class CitySiteData
    {
        public int CityId;
        public int Site;
    }

    /// <summary>
    /// 城池单个等级包含的城池详细信息
    /// </summary>
    [Serializable]
    public class CityLevelCacheData
    {
        public Dictionary<int, LangHunLingYuCityDataEx> CityDataDict = new Dictionary<int, LangHunLingYuCityDataEx>();
        public List<LangHunLingYuCityDataEx> CityDataList = new List<LangHunLingYuCityDataEx>();
    }

    [Serializable]
    public class BangHuiCitySiteData
    {
        public int Bhid;
        public CitySiteData[] CitySiteArray = new CitySiteData[10];
    }
}
