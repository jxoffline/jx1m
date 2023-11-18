using ProtoBuf;
using System.Collections.Concurrent;

namespace GameDBServer.DB
{
    [ProtoContract]
    public class RoleMiniInfo
    {
        [ProtoMember(1)]
        public long roleId;

        [ProtoMember(2)]
        public int zoneId;

        [ProtoMember(3)]
        public string userId;
    }
    /// <summary>
    /// Cache lại role data mini
    /// </summary>
    public static class CacheManager
    {
        private static ConcurrentDictionary<long, RoleMiniInfo> roleMiniInfoDict = new ConcurrentDictionary<long, RoleMiniInfo>();

        public static RoleMiniInfo GetRoleMiniInfo(long rid)
        {
            RoleMiniInfo roleMiniInfo;
            if (roleMiniInfoDict.TryGetValue(rid, out roleMiniInfo))
            {
                return roleMiniInfo;
            }

            roleMiniInfo = DBQuery.QueryRoleMiniInfo(rid);
            if (null != roleMiniInfo)
            {
                roleMiniInfoDict[rid] = roleMiniInfo;
            }

            return roleMiniInfo;
        }

        public static void AddRoleMiniInfo(long rid, int zoneId, string userId)
        {
            RoleMiniInfo roleMiniInfo;
            if (!roleMiniInfoDict.TryGetValue(rid, out roleMiniInfo))
            {
                roleMiniInfo = new RoleMiniInfo()
                {
                    roleId = rid,
                    zoneId = zoneId,
                    userId = userId,
                };
            }
        }
    }
}