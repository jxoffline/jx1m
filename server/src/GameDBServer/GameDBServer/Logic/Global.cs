using GameDBServer.Core;
using GameDBServer.DB;
using GameDBServer.Logic.KTBan;
using GameDBServer.Logic.Pet;
using GameDBServer.Server;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace GameDBServer.Logic
{
    internal class Global
    {
        /// <summary>
        /// Trả về giờ hệ thống hiện tại dưới đơn vị Mili giây
        /// </summary>
        /// <returns></returns>
        public static long GetCurrentTimeMilis()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }


        #region Thư viện xử lý XML

        public static string GetXElementNodePath(XElement element)
        {
            try
            {
                string path = element.Name.ToString();
                element = element.Parent;
                while (null != element)
                {
                    path = element.Name.ToString() + "/" + path;
                    element = element.Parent;
                }

                return path;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static XAttribute GetSafeAttribute(XElement XML, string attribute)
        {
            try
            {
                XAttribute attrib = XML.Attribute(attribute);
                if (null == attrib)
                {
                    throw new Exception(string.Format("读取属性: {0} 失败, xml节点名: {1}", attribute, GetXElementNodePath(XML)));
                }

                return attrib;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("读取属性: {0} 失败, xml节点名: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        public static string GetSafeAttributeStr(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            return (string)attrib;
        }

        public static long GetSafeAttributeLong(XElement XML, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, attribute);
            string str = (string)attrib;
            if (null == str || str == "") return -1;

            try
            {
                return (long)Convert.ToDouble(str);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("读取属性: {0} 失败, xml节点名: {1}", attribute, GetXElementNodePath(XML)));
            }
        }

        public static XAttribute GetSafeAttribute(XElement XML, string root, string attribute)
        {
            try
            {
                XAttribute attrib = XML.Element(root).Attribute(attribute);
                if (null == attrib)
                {
                    throw new Exception(string.Format("读取属性: {0}/{1} 失败, xml节点名: {2}", root, attribute, GetXElementNodePath(XML)));
                }

                return attrib;
            }
            catch (Exception)
            {
                throw new Exception(string.Format("读取属性: {0}/{1} 失败, xml节点名: {2}", root, attribute, GetXElementNodePath(XML)));
            }
        }

        public static string GetSafeAttributeStr(XElement XML, string root, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, root, attribute);
            return (string)attrib;
        }

        public static long GetSafeAttributeLong(XElement XML, string root, string attribute)
        {
            XAttribute attrib = GetSafeAttribute(XML, root, attribute);
            string str = (string)attrib;
            if (null == str || str == "") return -1;

            try
            {
                return (long)Convert.ToDouble(str);
            }
            catch (Exception)
            {
                throw new Exception(string.Format("读取属性: {0}/{1} 失败, xml节点名: {2}", root, attribute, GetXElementNodePath(XML)));
            }
        }

        #endregion Thư viện xử lý XML

        #region Chuyển dữ liệu từ DB sang RoleDataEx

        //TODO xử lý nốt phần phúc lợi vào roledataex
        /// <summary>
        /// Chuyển dữ liệu từ DB sang RoleDataEx
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="roleDataEx"></param>
        public static void DBRoleInfo2RoleDataEx(DBRoleInfo dbRoleInfo, RoleDataEx roleDataEx)
        {
            /// Dữ liệu ban
            KTBanModel.BanUser banUser = KTBanManager.GetBanChatUserData(dbRoleInfo.RoleID);

            lock (dbRoleInfo)
            {
                roleDataEx.RoleID = dbRoleInfo.RoleID;
                roleDataEx.RoleName = dbRoleInfo.RoleName;
                roleDataEx.RoleSex = dbRoleInfo.RoleSex;
                roleDataEx.FactionID = dbRoleInfo.Occupation;
                roleDataEx.SubID = dbRoleInfo.SubID;
                roleDataEx.Level = dbRoleInfo.Level;
                roleDataEx.GuildID = dbRoleInfo.GuildID;
                //Bạc
                roleDataEx.Money = dbRoleInfo.Money1;
                // Bạc Khóa
                roleDataEx.BoundMoney = dbRoleInfo.Gold;
                // Đồng khóa
                roleDataEx.BoundToken = dbRoleInfo.YinLiang;
                // Bạc ở thủ khố
                roleDataEx.Store_Money = dbRoleInfo.store_money;

                roleDataEx.Experience = dbRoleInfo.Experience;
                roleDataEx.PKMode = dbRoleInfo.PKMode;
                roleDataEx.PKValue = dbRoleInfo.PKValue;

                string[] fileds = dbRoleInfo.Position.Split(':');
                if (fileds.Length == 4)
                {
                    roleDataEx.MapCode = Convert.ToInt32(fileds[0]);
                    roleDataEx.RoleDirection = Convert.ToInt32(fileds[1]);
                    roleDataEx.PosX = Convert.ToInt32(fileds[2]);
                    roleDataEx.PosY = Convert.ToInt32(fileds[3]);
                }

                roleDataEx.OldTasks = dbRoleInfo.OldTasks;
                roleDataEx.TaskDataList = dbRoleInfo.DoingTaskList?.Values.ToList();
                roleDataEx.RolePic = dbRoleInfo.RolePic;
                //Thử send all vật phẩm của nó ra xem thế nào
                roleDataEx.GoodsDataList = Global.GetGoodDataList(dbRoleInfo);
                roleDataEx.MainQuickBarKeys = dbRoleInfo.MainQuickBarKeys;
                roleDataEx.OtherQuickBarKeys = dbRoleInfo.OtherQuickBarKeys;
                roleDataEx.QuickItems = dbRoleInfo.QuickItems;
                roleDataEx.SecondPassword = dbRoleInfo.SecondPassword;
                roleDataEx.LoginNum = dbRoleInfo.LoginNum;

                roleDataEx.FriendDataList = dbRoleInfo.FriendDataList;

                roleDataEx.TotalOnlineSecs = Math.Max(0, dbRoleInfo.TotalOnlineSecs); //2011-05-31, 以前的会出现负数？兼容错误

                roleDataEx.SkillDataList = dbRoleInfo.SkillDataList?.Values.ToList();

                roleDataEx.RegTime = DataHelper.ConvertToTicks(dbRoleInfo.RegTime);

                roleDataEx.SaleGoodsDataList = Global.GetGoodsDataListBySite(dbRoleInfo, (int)SaleGoodsConsts.SaleGoodsID);

                roleDataEx.BufferDataList = dbRoleInfo.BufferDataList?.Values.ToList();

                roleDataEx.MainTaskID = dbRoleInfo.MainTaskID;
                roleDataEx.PKPoint = dbRoleInfo.PKPoint;

                roleDataEx.ZoneID = dbRoleInfo.ZoneID;

                /// Tên bang hội
                roleDataEx.GuildName = dbRoleInfo.GuildName;

                roleDataEx.GuildRank = dbRoleInfo.GuildRank;

                roleDataEx.RoleGuildMoney = dbRoleInfo.RoleGuildMoney;

                if (roleDataEx.GuildID > 0)
                {
                    roleDataEx.OfficeRank = 0;
                }

                roleDataEx.Prestige = dbRoleInfo.Prestige;

                roleDataEx.LastMailID = dbRoleInfo.LastMailID;

                roleDataEx.BanChatStartTime = banUser == null ? 0 : banUser.StartTime;
                roleDataEx.BanChatDuration = banUser == null ? 0 : banUser.Duration;

                roleDataEx.GoodsLimitDataList = dbRoleInfo.GoodsLimitDataList;
                roleDataEx.RoleParamsDict = dbRoleInfo.RoleParamsDict.ToDictionary(entry => entry.Key, entry => entry.Value);

                long ticks = DateTime.Now.Ticks / 10000;

                roleDataEx.LastOfflineTime = dbRoleInfo.LogOffTime;

                roleDataEx.GroupMailRecordList = dbRoleInfo.GroupMailRecordList;

                // Set dữ liệu cho ROLEWELFARE
                roleDataEx.RoleWelfare = dbRoleInfo.RoleWelfare;

                /// Set dữ liệu cho Pets
                roleDataEx.Pets = dbRoleInfo.PetList.Values.ToList();

                /// Danh sách các chức năng bị cấm
                roleDataEx.BannedList = KTBanManager.GetBanList(dbRoleInfo.RoleID);
            }
        }

        public static int SafeConvertToInt32(string str, int fromBase = 10)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str)) return 0;

            try
            {
                return Convert.ToInt32(str, fromBase);
            }
            catch (Exception)
            {
            }

            return 0;
        }

        public static long SafeConvertToInt64(string str, int fromBase = 10)
        {
            str = str.Trim();
            if (string.IsNullOrEmpty(str)) return 0;

            try
            {
                return Convert.ToInt64(str, fromBase);
            }
            catch (Exception)
            {
            }

            return 0;
        }

        public static double GetOffsetSecond(DateTime date)
        {
            TimeSpan ts = date - DateTime.Parse("2011-11-11");
            // 经过的毫秒数
            double temp = ts.TotalMilliseconds;
            return temp / 1000;
        }

        public static int GetOffsetDay(DateTime now)
        {
            TimeSpan ts = now - DateTime.Parse("2011-11-11");

            double temp = ts.TotalMilliseconds;
            int day = (int)(temp / 1000 / 60 / 60 / 24);
            return day;
        }

        public static string GetDayStartTime(DateTime now)
        {
            return Global.GetDateTimeString(now.Date);
        }

        public static string GetDateTimeString(DateTime now)
        {
            return now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        #endregion Chuyển dữ liệu từ DB sang RoleDataEx

        #region SocketProsecc

        public static string GetSocketRemoteEndPoint(Socket s)
        {
            try
            {
                return string.Format("{0} ", s.RemoteEndPoint);
            }
            catch (Exception)
            {
            }

            return "";
        }

        public static string GetDebugHelperInfo(Socket socket)
        {
            if (null == socket)
            {
                return "socket为null, 无法打印错误信息";
            }

            string ret = "";
            try
            {
                ret += string.Format("IP={0} ", GetSocketRemoteEndPoint(socket));
            }
            catch (Exception)
            {
            }

            return ret;
        }

        #endregion SocketProsecc

        #region Quản lý vật phẩm

        /// <summary>
        /// Trả về danh sách vật phẩm theo túi
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static List<GoodsData> GetGoodsDataListBySite(DBRoleInfo dbRoleInfo, int site)
        {
            List<GoodsData> goodsDataList = new List<GoodsData>();
            if (null == dbRoleInfo.GoodsDataList)
            {
                return goodsDataList;
            }

            /// Truy vấn
            goodsDataList = dbRoleInfo.GoodsDataList.Values.Where(x => x.Site == site).ToList();

            return goodsDataList;
        }

        public static List<GoodsData> GetGoodDataList(DBRoleInfo dbRoleInfo)
        {
            List<GoodsData> goodsDataList = new List<GoodsData>();
            if (null == dbRoleInfo.GoodsDataList)
            {
                return goodsDataList;
            }

            /// Truy vấn
            goodsDataList = dbRoleInfo.GoodsDataList.Values.ToList();

            return goodsDataList;
        }


        /// <summary>
        /// Trả về tổng số vật phẩm trong túi tương ứng
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static int GetGoodsDataCountBySite(DBRoleInfo dbRoleInfo, int site)
        {
            if (null == dbRoleInfo.GoodsDataList)
            {
                return 0;
            }

            int count = dbRoleInfo.GoodsDataList.Values.Where(x => x.Site == site).Count();
            return count;
        }

        /// <summary>
        /// Trả về vật phẩm theo DbID
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static GoodsData GetGoodsDataByDbID(DBRoleInfo dbRoleInfo, int goodsDbID)
        {
            if (dbRoleInfo.GoodsDataList.TryGetValue(goodsDbID, out GoodsData itemGD))
            {
                return itemGD;
            }
            return null;
        }

        public static GoodsData GetGoodDropByDBID(DBRoleInfo dbRoleInfo, int goodsDbID)
        {
            if (dbRoleInfo.DropDataList != null)
            {
                if (dbRoleInfo.DropDataList.TryGetValue(goodsDbID, out GoodsData itemGD))
                {
                    return itemGD;
                }
            }
            return null;
        }

        /// <summary>
        /// Trả về danh sách vật phẩm của thằng A đã Drops
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="goodsDbID"></param>
        /// <returns></returns>
        public static GoodsData GetDropItemDataDbID(DBRoleInfo dbRoleInfo, int goodsDbID)
        {
            // nếu mà dict này không null thì lấy ra vật phẩm xem có không
            if (dbRoleInfo.DropDataList != null)
            {
                if (dbRoleInfo.DropDataList.TryGetValue(goodsDbID, out GoodsData itemGD))
                {
                    return itemGD;
                }
            }
            return null;
        }

        #endregion Quản lý vật phẩm

        #region Cbeck online state

        public static int GetRoleOnlineState(DBRoleInfo dbRoleInfo)
        {
            if (null == dbRoleInfo) return 0;
            if (dbRoleInfo.ServerLineID <= 0) return 0;
            return 1;
        }

        public static bool IsGameServerClientOnline(int lineId)
        {
            GameServerClient client;
            client = LineManager.GetGameServerClient(lineId);
            if (null != client && null != client.CurrentSocket && client == TCPManager.getInstance().getClient(client.CurrentSocket))
            {
                return true;
            }

            return false;
        }

        #endregion Cbeck online state

        #region RoleName Format

        public static string FormatRoleName(DBRoleInfo dbRoleInfo)
        {
            return FormatRoleName(dbRoleInfo.ZoneID, dbRoleInfo.RoleName);
        }

        public static string FormatRoleName(int zoneID, string roleName)
        {
            //return string.Format(Global.GetLang("[{0}区]{1}"), zoneID, roleName);
            return roleName;
        }

        #endregion RoleName Format

        public static List<MailData> LoadUserMailItemDataList(DBManager dbMgr, int rid)
        {
            //先设置新邮件标志位0【没有新邮件】
            DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(rid);
            if (null != dbRoleInfo)
            {
                dbRoleInfo.LastMailID = 0;
            }

            return DBQuery.GetMailItemDataList(dbMgr, rid);
        }

        public static int LoadUserMailItemDataCount(DBManager dbMgr, int rid, int excludeReadState = 0, int limitCount = 1)
        {
            return DBQuery.GetMailItemDataCount(dbMgr, rid, excludeReadState, limitCount);
        }

        public static MailData LoadMailItemData(DBManager dbMgr, int rid, int mailID)
        {
            MailData mailData = DBQuery.GetMailItemData(dbMgr, rid, mailID);
            if (null != mailData)
            {
                if (mailData.IsRead != 1)
                {
                    //设置已读标志
                    DBWriter.UpdateMailHasReadFlag(dbMgr, mailID, rid);

                    mailData.IsRead = 1;
                    mailData.ReadTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            return mailData;
        }

        #region

        /// <summary>
        /// Cập nhật trạng thái có thể nhận vật phẩm đính kèm trong thư không
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="mailData"></param>
        /// <returns></returns>
        public static bool UpdateHasFetchMailGoodsStat(DBManager dbMgr, int rid, int mailID)
        {
            bool ret = false;

            ret = DBWriter.UpdateMailHasFetchGoodsFlag(dbMgr, mailID, rid, 0);
            DBWriter.UpdateMailHasReadFlag(dbMgr, mailID, rid);
            /// Xóa vật phẩm đính kèm
            DBWriter.DeleteMailGoodsList(dbMgr, mailID);
            return ret;
        }

        #endregion

        #region

        /// <summary>
        /// Xóa thư
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="mailData"></param>
        /// <returns></returns>
        public static bool DeleteMail(DBManager dbMgr, int rid, string mailIDs)
        {
            bool ret = false, result = false;

            string[] mailidArr = mailIDs.Split('|');

            foreach (var strID in mailidArr)
            {
                try
                {
                    int mailID = int.Parse(strID);
                    //删除邮件实体【只要邮件实体被成功删除，就算删除成功】
                    ret = DBWriter.DeleteMailDataItemExcludeGoodsList(dbMgr, mailID, rid);
                    if (ret)
                    {
                        //删除邮件临时表中的相关项
                        DBWriter.DeleteMailIDInMailTemp(dbMgr, mailID);
                        //删除附件【没有附件的也可以调用】
                        DBWriter.DeleteMailGoodsList(dbMgr, mailID);

                        //有一个成功就都成功
                        result = ret;
                    }
                }
                catch
                {
                }
            }

            return result;
        }

        #endregion

        #region

        /// <summary>
        /// Thêm thư
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="mailData"></param>
        /// <returns></returns>
        public static int AddMail(DBManager dbMgr, string[] fields, out int addGoodsCount)
        {
            int.TryParse(fields[0], out int senderrid);
            string senderrname = DataHelper.Base64Encode(fields[1]);
            int.TryParse(fields[2], out int receiverrid);
            string reveiverrname = DataHelper.Base64Encode(fields[3]);
            string subject = DataHelper.Base64Encode(fields[4]);
            string content = DataHelper.Base64Encode(fields[5]);
            int.TryParse(fields[6], out int boundMoney);
            int.TryParse(fields[7], out int boundToken);
            int.TryParse(fields[9], out int mailtype);
            string goodslist = fields[8];

            if (reveiverrname == "")
            {
                string uid = "";
                Global.GetRoleNameAndUserID(dbMgr, receiverrid, out reveiverrname, out uid);
            }

          
            senderrname = senderrname.Replace('$', ':');
            reveiverrname = reveiverrname.Replace('$', ':');
            subject = subject.Replace('$', ':');
            content = content.Replace('$', ':');

            int mailID = -1;

            addGoodsCount = 0;

            DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(senderrid);

            if (null != dbRoleInfo)
            {
                senderrname = Global.FormatRoleName(dbRoleInfo);
            }

            int hasFetchAttachment = !string.IsNullOrEmpty(goodslist) || boundMoney > 0 || boundToken > 0 ? 1 : 0;

          
            mailID = DBWriter.AddMailBody(dbMgr, senderrid, senderrname, receiverrid, reveiverrname, subject, content, hasFetchAttachment, boundMoney, 0, mailtype);

            
            if (mailID >= 0)
            {
               
                addGoodsCount = Global.AddMailGoods(dbMgr, mailID, goodslist.Split('|'));
           
                DBWriter.UpdateLastScanMailID(dbMgr, receiverrid, mailID);
            }

            return mailID;
        }

        /// <summary>
        /// Thêm vật phẩm đính kèm trong thư
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="mailData"></param>
        /// <returns></returns>
        private static int AddMailGoods(DBManager dbMgr, int mailid, string[] goodsArr)
        {
          
            if (null == goodsArr || goodsArr.Length <= 0)
            {
                return 0;
            }

            int addCount = 0;
            string[] goods = null;

            for (int n = 0; n < goodsArr.Length; n++)
            {
                goods = goodsArr[n].Split('_');
                if (8 != goods.Length)
                {
                    continue;
                }

                if (DBWriter.AddMailGoodsDataItem(dbMgr, mailid, int.Parse(goods[0]), int.Parse(goods[1]), goods[2], int.Parse(goods[3]), int.Parse(goods[4]), int.Parse(goods[5]), goods[6], int.Parse(goods[7])))
                {
                    addCount++;
                }
            }

            return addCount;
        }

        #endregion

        public static int TransMoneyToYuanBao(int money)
        {
            int moneyToYuanBao = GameDBManager.GameConfigMgr.GetGameConfigItemInt("money-to-yuanbao", 10);

            int yuanBao = money * moneyToYuanBao;

            return yuanBao;
        }

        public static string GetHuoDongKeyString(string fromDate, string toDate)
        {
            return string.Format("{0}_{1}", fromDate, toDate);
        }

        public static bool GetRoleNameAndUserID(DBManager dbMgr, int rid, out string maxLevelRoleName, out string userID)
        {
            maxLevelRoleName = "";
            userID = "";

            DBRoleInfo roleInfo = dbMgr.GetDBRoleInfo(rid);

            if (null != roleInfo)
            {
                maxLevelRoleName = Global.FormatRoleName(roleInfo);
                userID = roleInfo.UserID;
            }

            return true;
        }

        public static void LogAndExitProcess(string error)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            File.AppendAllText("error.log", error + "\r\n");
            Console.WriteLine(error);
            Console.WriteLine("本程序将自动退出");
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 30; i > 0; i--)
            {
                Console.Write("\b\b" + i.ToString("00"));
                Thread.Sleep(600);
            }
            Process.GetCurrentProcess().Kill();
        }

        /// <summary>
        /// Tăng Index ID
        /// </summary>
        /// <returns></returns>
        public static bool InitDBAutoIncrementValues(DBManager dbManger)
        {
            int baseValue = GameDBManager.ZoneID * GameDBManager.DBAutoIncreaseStepValue;

            if (baseValue < 0)
            {
                return false;
            }

            int baseGuild_FamillyID = GameDBManager.ZoneID * GameDBManager.Guild_FamilyIncreaseStepValue;

            if (baseGuild_FamillyID < 0)
            {
                return false;
            }

            int dbMaxValue = DBQuery.GetMaxRoleID(dbManger) + 1;
            int ret1 = DBWriter.ChangeTablesAutoIncrementValue(dbManger, "t_roles", Math.Max(baseValue, dbMaxValue));

            dbMaxValue = DBQuery.GetMaxMailID(dbManger) + 1;
            int ret2 = DBWriter.ChangeTablesAutoIncrementValue(dbManger, "t_mail", Math.Max(baseValue, dbMaxValue));

            // lấy ra giá trị max của FAMILYID hiện tại
            //dbMaxValue = DBQuery.GetMaxFamilyID(dbManger) + 1;
            //int ret3 = DBWriter.ChangeTablesAutoIncrementValue(dbManger, "t_family", Math.Max(baseGuild_FamillyID, dbMaxValue));

            // lấy ra giá trị max của GUILD hiện tại
            dbMaxValue = DBQuery.GetMaxGuildID(dbManger) + 1;
            int ret4 = DBWriter.ChangeTablesAutoIncrementValue(dbManger, "t_guild", Math.Max(baseGuild_FamillyID, dbMaxValue));

            if (0 != ret1)
            {
                System.Console.WriteLine("Error updating the t_roles self-growth field of the database table");
            }

            if (0 != ret2)
            {
                System.Console.WriteLine("Error updating the t_mail self-growth field of the database table");
            }

            //if (0 != ret3)
            //{
            //    System.Console.WriteLine("Error updating the t_family self-growth field of the database table");
            //}

            if (0 != ret4)
            {
                System.Console.WriteLine("Error updating the t_guild self-growth field of the database table");
            }

            if (0 != ret1 || 0 != ret2 || 0 != ret4)
            {
                return false;
            }

            return true;
        }

        #region Update Role Pram

        public static void UpdateRoleParamByName(DBManager dbMgr, DBRoleInfo dbRoleInfo, string name, string value, RoleParamType roleParamType = null)
        {
            if (roleParamType == null)
            {
                roleParamType = RoleParamNameInfo.GetRoleParamType(name, value);
            }

            bool saved = DBWriter.UpdateRoleParams(dbMgr, dbRoleInfo.RoleID, name, value, roleParamType);

            RoleParamsData roleParamsData = null;

            if (!dbRoleInfo.RoleParamsDict.TryGetValue(name, out roleParamsData))
            {
                roleParamsData = new RoleParamsData()
                {
                    ParamName = name,
                    ParamValue = value,
                    ParamType = roleParamType,
                };

                dbRoleInfo.RoleParamsDict[name] = roleParamsData;
            }
            else
            {
                roleParamsData.ParamValue = value;
            }

            if (saved)
            {
                roleParamsData.UpdateFaildTicks = 0;
            }
            else
            {
                roleParamsData.UpdateFaildTicks = TimeUtil.NOW();
            }
        }

        public static long ModifyRoleParamLongByName(DBManager dbMgr, DBRoleInfo dbRoleInfo, string name, long value, RoleParamType roleParamType = null)
        {
            value += Global.GetRoleParamsInt64(dbRoleInfo, name);
            UpdateRoleParamByName(dbMgr, dbRoleInfo, name, value.ToString(), roleParamType);
            return value;
        }

        public static string GetRoleParamByName(DBRoleInfo dbRoleInfo, string name)
        {
            RoleParamsData roleParamsData = null;
            if (dbRoleInfo.RoleParamsDict.TryGetValue(name, out roleParamsData))
            {
                return roleParamsData.ParamValue;
            }

            return null;
        }

        public static int GetRoleParamsInt32(DBRoleInfo dbRoleInfo, string name)
        {
            String valueString = Global.GetRoleParamByName(dbRoleInfo, name);

            if (null == valueString || valueString.Length <= 0)
            {
                return 0;
            }

            return Global.SafeConvertToInt32(valueString);
        }

        public static long GetRoleParamsInt64(DBRoleInfo dbRoleInfo, string name)
        {
            String valueString = Global.GetRoleParamByName(dbRoleInfo, name);

            if (null == valueString || valueString.Length <= 0)
            {
                return 0;
            }

            return Global.SafeConvertToInt64(valueString);
        }

        #endregion Update Role Pram

        #region Ghi lại tích tiêu

        /// <summary>
        /// Ghi lại Log tích tiêu
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults SaveConsumeLog(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit, CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int roleID = Global.SafeConvertToInt32(fields[0]);
                int money = Global.SafeConvertToInt32(fields[1]);
                int type = Global.SafeConvertToInt32(fields[2]);

                string datestr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi khi tìm kiếm người chơi，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int nRet = DBWriter.SaveConsumeLog(dbMgr, roleID, datestr, type, money);
                if (1 == nRet)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", nID);
                }
                else
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception e)
            {
            }
            return TCPProcessCmdResults.RESULT_DATA;
        }

        #endregion Ghi lại tích tiêu

        #region Trả về IP local

        public static string GetLocalAddressIPs()
        {
            string addressIP = "";

            try
            {
                foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                    {
                        if (addressIP == "")
                        {
                            addressIP = _IPAddress.ToString();
                        }
                        else
                        {
                            addressIP += "_" + _IPAddress.ToString();
                        }
                    }
                }
            }
            catch
            {
            }
            return addressIP;
        }

        #endregion Trả về IP local

        #region String Prosecc

        public static int[] StringArray2IntArray(string[] sa)
        {
            int[] da = new int[sa.Length];
            for (int i = 0; i < sa.Length; i++)
            {
                string str = sa[i].Trim();
                str = string.IsNullOrEmpty(str) ? "0" : str;
                da[i] = Convert.ToInt32(str);
            }

            return da;
        }

        public static List<GoodsData> ParseGoodsDataList(string strGoodIDs)
        {
            string[] fields = strGoodIDs.Split('|');

            List<GoodsData> goodsDataList = new List<GoodsData>();
            for (int i = 0; i < fields.Length; i++)
            {
                string[] sa = fields[i].Split(',');
                if (sa.Length != 7)
                {
                    LogManager.WriteLog(LogTypes.Warning, string.Format("ParseGMailGoodsDataList解析{0}中第{1}个的奖励项时失败, 物品配置项个数错误", strGoodIDs, i));
                    continue;
                }

                int[] goodsFields = Global.StringArray2IntArray(sa);

                GoodsData gmailData = new GoodsData()
                {
                    GoodsID = goodsFields[0],
                    GCount = goodsFields[1],
                    Forge_level = goodsFields[3],
                    Binding = goodsFields[2],
                };

                goodsDataList.Add(gmailData);
            }

            return goodsDataList;
        }

        #endregion String Prosecc
    }
}