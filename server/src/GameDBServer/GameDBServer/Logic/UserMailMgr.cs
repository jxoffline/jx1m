using GameDBServer.Core;
using GameDBServer.DB;
using System;
using System.Collections.Generic;

namespace GameDBServer.Logic
{
    public class UserMailManager
    {
        private const int OverdueDays = 15;

        private const long ClearOverdueMailInterval = (long)(1.42857 * TimeUtil.DAY);

        private static long LastScanMailTicks = DateTime.Now.Ticks / 10000;

        private static long LastClearMailTicks = DateTime.Now.Ticks / 10000;

        public static void ScanLastMails(DBManager dbMgr)
        {
            long nowTicks = DateTime.Now.Ticks / 10000;
            if (nowTicks - LastScanMailTicks < (30 * 1000))
            {
                return;
            }

            LastScanMailTicks = nowTicks;

            Dictionary<int, int> lastMailDict = DBQuery.ScanLastMailIDListFromTable(dbMgr);

            if (null != lastMailDict && lastMailDict.Count > 0)
            {
                String gmCmd = "", mailIDsToDel = "";

                foreach (var item in lastMailDict)
                {
                    DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(item.Key);
                    if (null != dbRoleInfo)
                    {
                        if (gmCmd.Length > 0)
                        {
                            gmCmd += "_";
                        }

                        dbRoleInfo.LastMailID = item.Value;
                        gmCmd += String.Format("{0}|{1}", dbRoleInfo.RoleID, item.Value);
                    }
                    else
                    {
                        DBWriter.UpdateRoleLastMail(dbMgr, item.Key, item.Value);
                    }

                    if (mailIDsToDel.Length > 0)
                    {
                        mailIDsToDel += ",";
                    }

                    mailIDsToDel += item.Value;
                }

                if (gmCmd.Length > 0)
                {
                    //添加GM命令消息
                    string gmCmdData = string.Format("-notifymail {0}", gmCmd);
                    ChatMsgManager.AddGMCmdChatMsg(-1, gmCmdData);
                }

                if (mailIDsToDel.Length >= 0)
                {
                    DBWriter.DeleteLastScanMailIDs(dbMgr, lastMailDict);
                }
            }
        }

        public static void ClearOverdueMails(DBManager dbMgr)
        {
            long nowTicks = DateTime.Now.Ticks / 10000;

            if (nowTicks - LastClearMailTicks < ClearOverdueMailInterval)
            {
                return;
            }

            LastClearMailTicks = nowTicks;

            DBWriter.ClearOverdueMails(dbMgr, DateTime.Now.AddDays(-OverdueDays));
        }
    }
}