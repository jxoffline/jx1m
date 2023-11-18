using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Data
{
    public class KFSpreadData
    {
        public int ServerID = 0;
        public int ZoneID = 0;
        public int RoleID = 0;

        //public string SpreadCode = "";
        //public string VerifyCode = "";

        public int CountRole = 0;
        public int CountVip = 0;
        public int CountLevel = 0;

        public DateTime LogTime = DateTime.Now;

        public void UpdateLogtime()
        {
            LogTime = DateTime.Now;
        }
    }

    public class KFSpreadVerifyData
    {
        public string CUserID = "";
        public int CServerID = 0;
        public int CZoneID = 0;
        public int CRoleID = 0;

        public int PZoneID = 0;
        public int PRoleID = 0;

        public int IsVip = 0;
        public int IsLevel = 0;

        //public string VerifyCode = "";

        public string Tel = "";
        public int TelCode = 0;

        public DateTime LogTime = DateTime.Now;
        public DateTime TelTime = DateTime.Now;

        public void UpdateLogTime() { LogTime = DateTime.Now; }
    }

    public class KFSpreadTelTotal
    {
        public string Tel = "";
        public int Count = 0;
        public DateTime LogTime = DateTime.Now;
        public bool IsStop = false;

        public void UpdateLogTime() {  LogTime = DateTime.Now; }
        public void AddCount() { Count++; }
    }

    public class KFSpreadRoleTotal
    {
        public int CServerID = 0;
        public int CZoneID = 0;
        public int CRoleID = 0;

        public int Count = 0;
        public DateTime LogTime = DateTime.Now;
        public bool IsStop = false;

        public void UpdateLogTime() { LogTime = DateTime.Now; }
        public void AddCount() { Count++; }
    }

    public class KFSpreadKey : IEquatable<KFSpreadKey>
    {
        int RoleId;
        int ZoneId;

        public static KFSpreadKey Get(int zoneId, int roleId)
        {
            return new KFSpreadKey(zoneId, roleId);
        }

        private KFSpreadKey(int zoneId, int roleId)
        {
            ZoneId = zoneId;
            RoleId = roleId;
        }

        public bool Equals(KFSpreadKey other)
        {
            return RoleId == other.RoleId && ZoneId == other.ZoneId;
        }

        public override int GetHashCode()
        {
            return RoleId;
        }

        public override bool Equals(object other)
        {
            KFSpreadKey obj = other as KFSpreadKey;
            if (null == obj)
            {
                return false;
            }

            return RoleId == obj.RoleId && ZoneId == obj.ZoneId;
        }
    }

}

//推广数据 KFSpreadData
//验证数据 KFSpreadVerifyData
//
//
