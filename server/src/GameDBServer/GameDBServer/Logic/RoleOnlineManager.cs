using System;
using System.Collections.Generic;

namespace GameDBServer.Logic
{
    public class RoleOnlineManager
    {
        private static Dictionary<int, long> _RoleOlineTicksDict = new Dictionary<int, long>();



        public static void UpdateRoleOnlineTicks(int roleID)
        {
            long ticks = DateTime.Now.Ticks / 10000;
            lock (_RoleOlineTicksDict)
            {
                if (_RoleOlineTicksDict.ContainsKey(roleID))
                {
                    _RoleOlineTicksDict[roleID] = ticks;
                }
                else
                {
                    _RoleOlineTicksDict.Add(roleID, ticks);
                }
            }
        }

        public static void RemoveRoleOnlineTicks(int roleID)
        {
            lock (_RoleOlineTicksDict)
            {
                _RoleOlineTicksDict.Remove(roleID);
            }
        }


    }
}