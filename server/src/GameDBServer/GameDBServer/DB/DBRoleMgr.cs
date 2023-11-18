using GameDBServer.Core;
using GameDBServer.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameDBServer.DB
{
    /// <summary>
    /// Quản lý truy vấn thông tin nhân vật
    /// </summary>
    public class DBRoleMgr
    {
        /// <summary>
        /// Danh sách thông tin nhân vật
        /// </summary>
        private ConcurrentDictionary<int, MyWeakReference> DictRoleInfos = new ConcurrentDictionary<int, MyWeakReference>();

        /// <summary>
        /// Danh sách ID nhân vật theo tên
        /// </summary>
        private ConcurrentDictionary<string, int> DictRoleName2ID = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Trả về tổng số nhân vật
        /// </summary>
        /// <returns></returns>
        public int GetRoleInfoCount()
        {
            return DictRoleInfos.Count;
        }

        /// <summary>
        /// Tìm nhân vật theo ID
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public DBRoleInfo FindDBRoleInfo(int roleID)
        {
            DBRoleInfo dbRoleInfo = null;
            MyWeakReference weakRef = null;

            if (DictRoleInfos.Count > 0)
            {
                if (DictRoleInfos.TryGetValue(roleID, out weakRef))
                {
                    if (weakRef.IsAlive)
                    {
                        dbRoleInfo = weakRef.Target as DBRoleInfo;
                    }
                }
            }

            if (null != dbRoleInfo)
            {
                dbRoleInfo.LastReferenceTicks = DateTime.Now.Ticks / 10000;
            }

            return dbRoleInfo;
        }

        /// <summary>
        /// Tìm ID nhân vật theo tên
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        public int FindDBRoleID(string roleName)
        {
            int roleID = -1;
            if (!DictRoleName2ID.TryGetValue(roleName, out roleID))
            {
                return -1;
            }

            return roleID;
        }

        /// <summary>
        /// Thêm thông tin nhân vật tương ứng vào danh sách
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        public void AddDBRoleInfo(DBRoleInfo dbRoleInfo)
        {
            MyWeakReference weakRef = null;

            if (DictRoleInfos.TryGetValue(dbRoleInfo.RoleID, out weakRef))
            {
                weakRef.Target = dbRoleInfo;
            }
            else
            {
                DictRoleInfos.TryAdd(dbRoleInfo.RoleID, new MyWeakReference(dbRoleInfo));
            }

            string formatedRoleName = Global.FormatRoleName(dbRoleInfo);
            DictRoleName2ID[formatedRoleName] = dbRoleInfo.RoleID;
        }

        /// <summary>
        /// Xóa thông tin nhân vật tương ứng khỏi danh sách
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        public void RemoveDBRoleInfo(int roleID)
        {
            string formatedRoleName = null;
            MyWeakReference weakRef = null;

            if (DictRoleInfos.TryGetValue(roleID, out weakRef))
            {
                formatedRoleName = Global.FormatRoleName((weakRef.Target as DBRoleInfo));
                weakRef.Target = null;
            }

            if (null != formatedRoleName)
            {
                DictRoleName2ID.TryRemove(formatedRoleName, out int Value);
            }
        }

        /// <summary>
        /// Xóa toàn bộ thông tin nhân vật
        /// </summary>
        public void ClearAllDBroleInfo()
        {
            DictRoleInfos.Clear();

            DictRoleName2ID.Clear();
        }

        /// <summary>
        /// Xóa các nhân vật không Online sau khoảng tương ứng
        /// </summary>
        public void ReleaseIdleDBRoleInfos(int ticksSlot)
        {
            long nowTicks = DateTime.Now.Ticks / 10000;
            long needUpdateTicks = TimeUtil.NOW() - ticksSlot;
            Dictionary<DBRoleInfo, List<RoleParamsData>> dict = new Dictionary<DBRoleInfo, List<RoleParamsData>>();
            List<int> idleRoleIDList = new List<int>();

            // Duyệt toàn bộ các roelI Dhiện tịa
            foreach (var weakRef in DictRoleInfos.Values)
            {
                if (weakRef.IsAlive)
                {
                    DBRoleInfo dbRoleInfo = (weakRef.Target as DBRoleInfo);
                    if (null != dbRoleInfo)
                    {
                        List<RoleParamsData> updateList = null;

                        // Lock rolePram
                        if (null != dbRoleInfo.RoleParamsDict)
                        {
                            foreach (var roleParamData in dbRoleInfo.RoleParamsDict.Values)
                            {
                                if (roleParamData.UpdateFaildTicks > 0 && needUpdateTicks > roleParamData.UpdateFaildTicks)
                                {
                                    if (null == updateList)
                                    {
                                        if (!dict.TryGetValue(dbRoleInfo, out updateList))
                                        {
                                            updateList = new List<RoleParamsData>();
                                            dict.Add(dbRoleInfo, updateList);
                                        }
                                    }

                                    updateList.Add(roleParamData);
                                }
                            }
                        }

                        if (null == updateList)
                        {
                            if (dbRoleInfo.ServerLineID <= 0 && nowTicks - dbRoleInfo.LastReferenceTicks >= ticksSlot)
                            {
                                idleRoleIDList.Add(dbRoleInfo.RoleID);
                            }
                        }
                    }
                }
            }

            DBManager dbMgr = DBManager.getInstance();
            foreach (var kv in dict)
            {
                foreach (var paramData in kv.Value)
                {
                    Global.UpdateRoleParamByName(dbMgr, kv.Key, paramData.ParamName, paramData.ParamValue, paramData.ParamType);
                }
            }

            for (int i = 0; i < idleRoleIDList.Count; i++)
            {
                RemoveDBRoleInfo(idleRoleIDList[i]);
                LogManager.WriteLog(LogTypes.Info, string.Format("Release unused role: {0}", idleRoleIDList[i]));
            }
        }

        /// <summary>
        /// Xóa thông tin nhân vật theo ID
        /// </summary>
        public void ReleaseDBRoleInfoByID(int roleID)
        {
            DBRoleInfo dbRoleInfo = FindDBRoleInfo(roleID);
            if (null == dbRoleInfo) return;

            RemoveDBRoleInfo(dbRoleInfo.RoleID);
            // Logs lại xóa bỏ role id theo thời gian
            LogManager.WriteLog(LogTypes.Cache, string.Format("Release unused role: {0}", dbRoleInfo.RoleID));
        }

        /// <summary>
        /// Đổi tên nhân vật
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="zoneId"></param>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public void OnChangeName(int roleId, int zoneId, string oldName, string newName)
        {
            string fmtOldName = Global.FormatRoleName(zoneId, oldName);
            int _roleId = 0;
            if (DictRoleName2ID.TryGetValue(fmtOldName, out _roleId))
            {
                DictRoleName2ID.TryRemove(fmtOldName, out int value);
                DictRoleName2ID[Global.FormatRoleName(zoneId, newName)] = _roleId;
            }
        }
    }
}