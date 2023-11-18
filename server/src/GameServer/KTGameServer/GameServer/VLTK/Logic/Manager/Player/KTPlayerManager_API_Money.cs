using GameServer.Core.Executor;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Tools;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        #region Bạc trong kho
        /// <summary>
        /// Thêm bạc trong kho cho người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="addMoney"></param>
        /// <param name="strFrom"></param>
        /// <param name="writeLog"></param>
        /// <returns></returns>
        public static bool AddUserStoreMoney(KPlayer client, int addMoney, string strFrom, bool writeLog = true)
        {
            /// Toác
            if (0 == addMoney)
            {
                /// Bỏ qua
                return true;
            }

            long oldMoney = client.StoreMoney;

            lock (client.StoreMoneyMutex)
            {
                if (addMoney < 0 && oldMoney < Math.Abs(addMoney))
                {
                    return false;
                }

                /// Gói tin gửi lên GameDB
                string strcmd = string.Format("{0}:{1}", client.RoleID, addMoney);
                string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_ADD_STORE_MONEY, strcmd, client.ServerId);
                /// Nếu DB không trả ra kết quả
                if (null == dbFields || dbFields.Length != 2)
                {
                    return false;
                }

                /// Nếu số lượng bạc trong kho hiện có < 0 thì toác
                if (Convert.ToInt32(dbFields[1]) < 0)
                {
                    return false;
                }

                /// Cập nhật lại số bạc trong kho
                client.StoreMoney = Convert.ToInt32(dbFields[1]);

                if (0 != addMoney)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "ADD";

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldMoney + "";
                        string OtherPram_2 = client.BoundMoney + "";
                        string OtherPram_3 = MoneyType.StoreMoney + "";
                        string OtherPram_4 = "NONE";

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.StoreMoney, addMoney, client.StoreMoney, strFrom);
                }
            }

            /// Thông báo số bạc trong kho thay đổi
            KT_TCPHandler.NotifySelfUserStoreMoneyChange(client);

            /// Sự kiện gì đó
            KTGlobal.AddRoleStoreMoneyEvent(client, oldMoney);

            return true;
        }
        #endregion

        #region Đồng khóa
        /// <summary>
        /// Trừ đồng khóa của người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="subBoundToken"></param>
        /// <param name="strFrom"></param>
        /// <returns></returns>
        public static bool SubBoundToken(KPlayer client, int subBoundToken, string strFrom, bool writeLog = true)
        {
            int oldBoundToken = client.BoundToken;

            //Lock lại tiền trước khi thao tác với nó
            lock (client.BoundTokenMutex)
            {
                if (client.BoundToken < subBoundToken)
                {
                    return false; //Nếu số tiền không đủ thì chim cút
                }

                // Tiền trước đó
                int oldValue = client.BoundToken;
                // Sau khi trừ còn bao nhiêu
                client.BoundToken -= subBoundToken;

                //CMD SEND TO DB
                string strcmd = string.Format("{0}:{1}", client.RoleID, -subBoundToken);
                string[] dbFields = null;

                try
                {
                    dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEBoundToken_CMD, strcmd, client.ServerId);
                }
                catch (Exception ex)
                {
                    DataHelper.WriteExceptionLogEx(ex, string.Format("Nếu toạch thì return FALSE"));

                    return false;
                }

                if (null == dbFields)
                    return false;
                if (dbFields.Length != 2)
                {
                    client.BoundToken = oldValue;
                    return false;
                }

                if (Convert.ToInt32(dbFields[1]) < 0)
                {
                    client.BoundToken = oldValue;
                    return false;
                }

                client.BoundToken = Convert.ToInt32(dbFields[1]);

                if (0 != subBoundToken)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "SUB";

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldValue + "";
                        string OtherPram_2 = client.BoundToken + "";
                        string OtherPram_3 = MoneyType.DongKhoa + "";
                        string OtherPram_4 = "-" + subBoundToken;

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.DongKhoa, -subBoundToken, client.BoundToken, strFrom);
                }
            }

            // THỰC HIỆN NOTIFY TỚI NGƯỜI CHƠI
            KT_TCPHandler.NotifySelfTokenChange(client);

            return true;
        }

        /// <summary>
        /// Thêm đồng khóa cho người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="addBoundToken"></param>
        /// <param name="strFrom"></param>
        /// <returns></returns>
        public static bool AddBoundToken(KPlayer client, int addBoundToken, string strFrom, bool writeLog = true)
        {
            int oldBoundToken = client.BoundToken;
            lock (client.BoundTokenMutex)
            {
                if (oldBoundToken >= KTGlobal.Max_Role_Money)
                {
                    return false;
                }

                if (oldBoundToken + addBoundToken > KTGlobal.Max_Role_Money)
                {
                    return false;
                }

                if (0 == addBoundToken)
                {
                    return true;
                }

                string strcmd = string.Format("{0}:{1}", client.RoleID, addBoundToken);
                string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEBoundToken_CMD, strcmd, client.ServerId);
                if (null == dbFields)
                    return false;
                if (dbFields.Length != 2)
                {
                    return false;
                }
                if (Convert.ToInt32(dbFields[1]) < 0)
                {
                    return false;
                }

                client.BoundToken = Convert.ToInt32(dbFields[1]);

                if (0 != addBoundToken)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "" + addBoundToken;

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldBoundToken + "";
                        string OtherPram_2 = client.BoundToken + "";
                        string OtherPram_3 = MoneyType.DongKhoa + "";
                        string OtherPram_4 = "NONE";

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.DongKhoa, addBoundToken, client.BoundToken, strFrom);
                }
            }

            KT_TCPHandler.NotifySelfTokenChange(client);

            return true;
        }
        #endregion

        #region Đồng thường
        /// <summary>
        /// Thêm đồng cho người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="addMoney"></param>
        /// <param name="strFrom"></param>
        /// <returns></returns>
        public static bool AddToken(KPlayer client, int addMoney, string strFrom, ActivityTypes result = ActivityTypes.None, bool writeLog = true)
        {
            lock (client.TokenMutex)
            {
                int oldMoney = client.Token;

                string strcmd = string.Format("{0}:{1}:{2}:{3}", client.RoleID, addMoney, (int) result, strFrom);
                string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEToken_CMD, strcmd, client.ServerId);
                if (null == dbFields)
                    return false;
                if (dbFields.Length != 3)
                {
                    return false;
                }

                if (Convert.ToInt32(dbFields[1]) < 0)
                {
                    return false;
                }

                client.Token = Convert.ToInt32(dbFields[1]);
                int nTotalMoney = Convert.ToInt32(dbFields[2]);

                if (0 != addMoney)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "ADD";

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldMoney + "";
                        string OtherPram_2 = client.Token + "";
                        string OtherPram_3 = MoneyType.Dong + "";
                        string OtherPram_4 = addMoney + "";

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.Dong, addMoney, client.Money, strFrom);
                }
            }

            KT_TCPHandler.NotifySelfTokenChange(client);

            return true;
        }

        /// <summary>
        /// Trừ đồng của người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="subMoney"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool SubToken(KPlayer client, int subMoney, string strFrom, int type = 1, bool writeLog = true,bool UpdateConsume =false)
        {
            lock (client.TokenMutex)
            {
                subMoney = Math.Abs(subMoney);
                if (client.Token < subMoney)
                {
                    return false;
                }

                int oldValue = client.Token;
                client.Token -= subMoney;

                string strcmd = string.Format("{0}:{1}", client.RoleID, -subMoney);
                string[] dbFields = null;

                try
                {
                    dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEToken_CMD, strcmd, client.ServerId);
                }
                catch (Exception ex)
                {
                    DataHelper.WriteExceptionLogEx(ex, string.Format("CMD_DB_UPDATEToken_CMD Faild"));

                    return false;
                }

                if (null == dbFields)
                    return false;
                if (dbFields.Length != 3)
                {
                    client.Token = oldValue;
                    return false;
                }

                if (Convert.ToInt32(dbFields[1]) < 0)
                {
                    client.Token = oldValue;
                    return false;
                }

                client.Token = Convert.ToInt32(dbFields[1]);

                if(UpdateConsume)
                {
                    /// ghi lại tiêu phí
                    Global.SaveConsumeLog(client, subMoney, type);
                }    
             

                if (0 != subMoney)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "SUB";

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldValue + "";
                        string OtherPram_2 = client.Token + "";
                        string OtherPram_3 = MoneyType.Dong + "";
                        string OtherPram_4 = "-" + subMoney;

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.Dong, -subMoney, client.Money, strFrom);
                }
            }

            KT_TCPHandler.NotifySelfTokenChange(client);

            return true;
        }
        #endregion

        #region Bạc thường
        /// <summary>
        /// Thêm bạc cho người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="addMoney"></param>
        /// <param name="strFrom"></param>
        /// <param name="writeToDB"></param>
        /// <returns></returns>
        public static bool AddMoney(KPlayer client, int addMoney, string strFrom, bool writeToDB = true, bool writeLog = true, int exchangeID = -1)
        {
            lock (client.GetMoneyLock)
            {
                int oldMoney = client.Money;
                /// Nếu số bạc đang có vượt quá ngưỡng thì tự thêm vào kho
                if (oldMoney >= KTGlobal.Max_Role_Money)
                {
                    return KTPlayerManager.AddUserStoreMoney(client, addMoney, strFrom);
                }

                /// Nếu số bạc mang theo vượt quá ngưỡng thì tự thêm vào kho
                if (oldMoney + addMoney > KTGlobal.Max_Role_Money)
                {
                    int newValue = Math.Max(0, oldMoney + addMoney - KTGlobal.Max_Role_Money);
                    addMoney = Math.Max(0, KTGlobal.Max_Role_Money - oldMoney);
                    /// Thêm vào kho
                    KTPlayerManager.AddUserStoreMoney(client, newValue, strFrom);
                }

                if (0 == addMoney)
                {
                    return true;
                }

                if (writeToDB)
                {
                    string strcmd = string.Format("{0}:{1}", client.RoleID, client.Money + addMoney);

                    Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEMoney_CMD,
                        strcmd,
                        client.ServerId);

                    long nowTicks = TimeUtil.NOW();
                    Global.SetLastDBCmdTicks(client, (int) TCPGameServerCmds.CMD_DB_UPDATEMoney_CMD, nowTicks);
                }

                client.Money = client.Money + addMoney;
                KT_TCPHandler.NotifySelfMoneyChange(client);
                if (0 != addMoney)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "ADD";

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldMoney + "";
                        string OtherPram_2 = client.Money + "";
                        string OtherPram_3 = MoneyType.Bac + "";
                        string OtherPram_4 = "" + addMoney;
                        if (exchangeID != -1)
                        {
                            OtherPram_4 = exchangeID + "";
                        }

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.Bac, addMoney, client.Money, strFrom);
                }
            }

            return true;
        }

        /// <summary>
        /// Trừ bạc của người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="subMoney"></param>
        /// <param name="strFrom"></param>
        /// <returns></returns>
        public static bool SubMoney(KPlayer client, int subMoney, string strFrom, bool writeLog = true, int exchangeID = -1)
        {
            lock (client.GetMoneyLock)
            {
                int oldMoney = client.Money;

                if (client.Money - subMoney < 0)
                {
                    return false;
                }

                string strcmd = string.Format("{0}:{1}", client.RoleID, client.Money - subMoney);
                Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEMoney_CMD,
                    strcmd,
                     client.ServerId);

                long nowTicks = TimeUtil.NOW();
                Global.SetLastDBCmdTicks(client, (int) TCPGameServerCmds.CMD_DB_UPDATEMoney_CMD, nowTicks);

                client.Money = client.Money - subMoney;
                KT_TCPHandler.NotifySelfMoneyChange(client);
                if (0 != subMoney)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "SUB";

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldMoney + "";
                        string OtherPram_2 = client.Money + "";
                        string OtherPram_3 = MoneyType.Bac + "";

                        string OtherPram_4 = "-" + subMoney;
                        if (exchangeID != -1)
                        {
                            OtherPram_4 = exchangeID + "";
                        }

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.Bac, -subMoney, client.Money, strFrom);
                }
            }
            return true;
        }
        #endregion

        #region Bạc khóa
        /// <summary>
        /// Thêm Bạc khóa cho người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="addBoundMoney"></param>
        /// <returns></returns>
        public static bool AddBoundMoney(KPlayer client, int addBoundMoney, string strFrom, bool writeLog = false)
        {
            int oldBoundMoney = client.BoundMoney;

            lock (client.BoundMoneyMutex)
            {
                string strcmd = string.Format("{0}:{1}", client.RoleID, addBoundMoney);
                string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEUSERBoundMoney_CMD, strcmd, client.ServerId);
                if (null == dbFields)
                    return false;
                if (dbFields.Length != 2)
                {
                    return false;
                }

                if (Convert.ToInt32(dbFields[1]) < 0)
                {
                    return false;
                }

                client.BoundMoney = Convert.ToInt32(dbFields[1]);

                if (0 != addBoundMoney)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "ADD";

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldBoundMoney + "";
                        string OtherPram_2 = client.BoundMoney + "";
                        string OtherPram_3 = MoneyType.BacKhoa + "";
                        string OtherPram_4 = addBoundMoney + "";

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.BacKhoa, addBoundMoney, client.BoundMoney, strFrom);
                }
            }

            KT_TCPHandler.NotifySelfMoneyChange(client);

            KTGlobal.AddRoleBoundMoneyEvent(client, oldBoundMoney);

            return true;
        }

        /// <summary>
        /// Trừ bạc khóa của người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="subBoundMoney"></param>
        /// <param name="strFrom"></param>
        /// <returns></returns>
        public static bool SubBoundMoney(KPlayer client, int subBoundMoney, string strFrom = "", bool writeLog = false)
        {
            int oldBoundMoney = client.BoundMoney;

            lock (client.BoundMoneyMutex)
            {
                if (client.BoundMoney < subBoundMoney)
                {
                    return false;
                }

                string strcmd = string.Format("{0}:{1}", client.RoleID, -subBoundMoney);
                string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEUSERBoundMoney_CMD, strcmd, client.ServerId);
                if (null == dbFields)
                    return false;
                if (dbFields.Length != 2)
                {
                    return false;
                }

                if (Convert.ToInt32(dbFields[1]) < 0)
                {
                    return false;
                }

                client.BoundMoney = Convert.ToInt32(dbFields[1]);

                if (0 != subBoundMoney)
                {
                    if (writeLog)
                    {
                        int RoleID = client.RoleID;
                        string AccountName = client.strUserID;
                        string RecoreType = "MONEY";
                        string RecoreDesc = strFrom;
                        string Source = "SYSTEM";
                        string Taget = client.RoleName;
                        string OptType = "SUB";

                        int ZONEID = client.ZoneID;

                        string OtherPram_1 = oldBoundMoney + "";
                        string OtherPram_2 = client.BoundMoney + "";
                        string OtherPram_3 = MoneyType.BacKhoa + "";
                        string OtherPram_4 = "-" + subBoundMoney;

                        //Thực hiện việc ghi LOGS vào DB
                        GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                    }

                    EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.BacKhoa, -subBoundMoney, client.BoundMoney, strFrom);
                }
            }

            KT_TCPHandler.NotifySelfMoneyChange(client);

            KTGlobal.AddRoleBoundMoneyEvent(client, oldBoundMoney);

            return true;
        }
        #endregion

        #region Bạc bang hội
        /// <summary>
        /// Trừ bạc bang hội
        /// </summary>
        /// <param name="client"></param>
        /// <param name="subBoundToken"></param>
        /// <param name="strFrom"></param>
        /// <returns></returns>
        public static bool SubGuildMoney(KPlayer client, int subBoundToken, string strFrom, bool writeLog = false)
        {
            if (client.RoleGuildMoney < subBoundToken)
            {
                return false; //Nếu số tiền không đủ thì chim cút
            }
            // Nếu ko có bang thì cũng cho toạch luôn
            if (client.GuildID <= 0)
            {
                return false;
            }
            // Tiền trước đó
            int oldValue = client.RoleGuildMoney;
            // Sau khi trừ còn bao nhiêu
            client.RoleGuildMoney -= subBoundToken;

            //CMD SEND TO DB
            string strcmd = string.Format("{0}:{1}", client.RoleID, -subBoundToken);
            string[] dbFields = null;

            try
            {
                dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEBANGGONG_CMD, strcmd, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteExceptionLogEx(ex, string.Format("Nếu toạch thì return FALSE"));

                return false;
            }

            // nếu lỗi DB thì set lại tiền = tiền trước đó
            if (null == dbFields)
            {
                client.RoleGuildMoney = oldValue;
                return false;
            }

            if (dbFields.Length != 2)
            {
                client.RoleGuildMoney = oldValue;
                return false;
            }

            if (Convert.ToInt32(dbFields[1]) < 0)
            {
                if (Convert.ToInt32(dbFields[1]) == -3)
                {
                    KTPlayerManager.ShowNotification(client, "Số lượng muốn rút đã vượt quá số % lợi tức thiết lập của bang chủ");
                }

                client.RoleGuildMoney = oldValue;
                return false;
            }

            client.RoleGuildMoney = Convert.ToInt32(dbFields[1]);

            if (0 != subBoundToken)
            {
                if (writeLog)
                {
                    int RoleID = client.RoleID;
                    string AccountName = client.strUserID;
                    string RecoreType = "MONEY";
                    string RecoreDesc = strFrom;
                    string Source = "SYSTEM";
                    string Taget = client.RoleName;
                    string OptType = "SUB";

                    int ZONEID = client.ZoneID;

                    string OtherPram_1 = oldValue + "";
                    string OtherPram_2 = client.RoleGuildMoney + "";
                    string OtherPram_3 = MoneyType.GuildMoney + "";
                    string OtherPram_4 = "-" + subBoundToken;

                    //Thực hiện việc ghi LOGS vào DB
                    GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                }
            }
            //Notify về client số bang cống hiện tại còn
            KT_TCPHandler.NotifySelfMoneyChange(client);

            return true;
        }

        /// <summary>
        /// Thêm bạc bang hội
        /// </summary>
        /// <param name="client"></param>
        /// <param name="AddGuildMoney"></param>
        /// <param name="strFrom"></param>
        /// <param name="writeLog"></param>
        /// <returns></returns>
        public static bool AddGuildMoney(KPlayer client, int AddGuildMoney, string strFrom, bool writeLog = false)
        {
            int oldGuildMoney = client.RoleGuildMoney;

            if (oldGuildMoney >= KTGlobal.Max_Role_Money)
            {
                return false;
            }

            if (oldGuildMoney + AddGuildMoney > KTGlobal.Max_Role_Money)
            {
                return false;
            }

            if (0 == AddGuildMoney)
            {
                return true;
            }

            string strcmd = string.Format("{0}:{1}", client.RoleID, AddGuildMoney);

            string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATEBANGGONG_CMD, strcmd, client.ServerId);
            if (null == dbFields)
                return false;
            if (dbFields.Length != 2)
            {
                return false;
            }
            if (Convert.ToInt32(dbFields[1]) < 0)
            {
                return false;
            }

            client.RoleGuildMoney = Convert.ToInt32(dbFields[1]);

            if (0 != AddGuildMoney)
            {
                if (writeLog)
                {
                    int RoleID = client.RoleID;
                    string AccountName = client.strUserID;
                    string RecoreType = "MONEY";
                    string RecoreDesc = strFrom;
                    string Source = "SYSTEM";
                    string Taget = client.RoleName;
                    string OptType = "" + AddGuildMoney;

                    int ZONEID = client.ZoneID;

                    string OtherPram_1 = oldGuildMoney + "";
                    string OtherPram_2 = client.RoleGuildMoney + "";
                    string OtherPram_3 = MoneyType.GuildMoney + "";
                    string OtherPram_4 = "NONE";

                    //Thực hiện việc ghi LOGS vào DB
                    GameManager.logDBCmdMgr.WriterLogs(RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZONEID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4, client.ServerId);
                }

                // EventLogManager.AddMoneyEvent(client, OpTypes.AddOrSub, OpTags.None, MoneyType.GuildMoney, AddGuildMoney, client.BoundToken, strFrom);
            }

            KTGlobal.SendDefaultChat(client, KTGlobal.CreateStringByColor("Tài sản bang hội cá nhân gia tăng: " + AddGuildMoney + "", ColorType.Blue));

            // GameManager.ClientMgr.NotifySelfTokenChange(sl, pool, client);

            return true;
        }
        #endregion
    }
}
