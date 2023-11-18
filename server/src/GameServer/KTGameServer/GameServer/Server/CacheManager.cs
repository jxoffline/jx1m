using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Logic;
using ProtoBuf;

namespace GameServer.Server
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

    public class CacheManager
    {
        private static Dictionary<long, RoleMiniInfo> roleMiniInfoDict = new Dictionary<long, RoleMiniInfo>();

        private static RoleMiniInfo GetRoleMiniInfo(long rid, int serverId)
        {
            RoleMiniInfo roleMiniInfo;
            lock (roleMiniInfoDict)
            {
                if (roleMiniInfoDict.TryGetValue(rid, out roleMiniInfo))
                {
                    return roleMiniInfo;
                }
            }

            roleMiniInfo = Global.SendToDB<RoleMiniInfo, long>((int)TCPGameServerCmds.CMD_DB_QUERY_ROLEMINIINFO, rid, serverId);
            if (null != roleMiniInfo && roleMiniInfo.roleId == rid)
            {
                lock (roleMiniInfoDict)
                {
                    roleMiniInfoDict[rid] = roleMiniInfo;
                }
            }

            return roleMiniInfo;
        }

        public static void OnInitGame(KPlayer client)
        {
            lock (roleMiniInfoDict)
            {
                RoleMiniInfo roleMiniInfo;
                if (!roleMiniInfoDict.TryGetValue(client.RoleID, out roleMiniInfo))
                {
                    roleMiniInfo = new RoleMiniInfo()
                    {
                        roleId = client.RoleID,
                        zoneId = client.ZoneID,
                        userId = client.strUserID,
                    };
                }
            }
        }

        public static int GetZoneIdByRoleId(long rid, int serverId)
        {
            RoleMiniInfo roleMiniInfo = GetRoleMiniInfo(rid, serverId);
            if (null != roleMiniInfo)
            {
                return roleMiniInfo.zoneId;
            }

            return 0;
        }

        public static string GetUserIdByRoleId(int rid, int serverId)
        {
            RoleMiniInfo roleMiniInfo = GetRoleMiniInfo(rid, serverId);
            if (null != roleMiniInfo)
            {
                return roleMiniInfo.userId;
            }

            return "";
        }
    }
}
