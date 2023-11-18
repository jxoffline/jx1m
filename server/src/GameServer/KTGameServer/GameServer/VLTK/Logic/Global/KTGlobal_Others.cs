using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region IP
        /// <summary>
        /// Trả về địa chỉ IP của người chơi tương ứng
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string GetIPAddress(KPlayer client)
        {
            try
            {
                if (client.ClientSocket == null)
                {
                    return "";
                }
                return ((IPEndPoint) client.ClientSocket.RemoteEndPoint).Address.ToString();
            }
            catch (Exception) { }
            /// Toác
            return "";
        }

        /// <summary>
        /// Lấy ra địa chỉ IP hiện tại
        /// </summary>
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
        #endregion

        #region Dialog

        /// <summary>
        /// Thời gian DELAY khi thực hiện Click vào NPC Dialog hoặc ItemDialog
        /// </summary>
        public const long DialogClickDelay = 500;

        /// <summary>
        /// Thời gian DELAY khi thực hiện Click vào vật phẩm để dùng
        /// </summary>
        public const long ItemClickDelay = 500;

        #endregion Dialog

        #region Thời gian check mail

        /// <summary>
        /// Giới hạn mỗi lần thao tác với thư
        /// </summary>
        public const long LimitMailCheckTick = 500;

        #endregion

        #region Record

        /// <summary>
        /// Lấy ra giá trị đã đánh dấu
        /// Cái này sử dụng để đánh dấu tháng này người chơi đã nhận cái gì chưa
        /// Hoặc sử dụng trong rất nhiều trường hợp khác cần đánh dấu
        /// Ví dụ : Cần ghi tháng 2 Người chơi đã nhận quà tháng tiêu dao cốc
        /// Thì tháng 2 là KEY | MarkType : Là Tiêu Dao Cốc
        /// </summary>
        /// <param name="client"></param>
        /// <param name="MarkType"></param>
        /// <param name="MarkType"></param>
        /// <returns></returns>
        public static int GetMarkValue(KPlayer client, string MarkKey, int MarkType)
        {
            string CMDBUILD = client.RoleID + "#" + MarkKey + "#" + MarkType;

            string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_KT_GETMARKVALUE, CMDBUILD, GameManager.LocalServerId);

            if (null == dbFields || dbFields.Length != 2)
            {
                LogManager.WriteLog(LogTypes.Error, "[GetMarkValue][" + client.RoleID + "][" + MarkKey + "] Lấy giá trị đánh dấu bị lỗi");
            }
            else
            {
                return Int32.Parse(dbFields[1]);
            }

            return -1;
        }

        /// <summary>
        /// Hàm này để ghi vào 1 giá trị vào DB theo KEY và TYPE
        /// Ví dụ muốn đánh tháng 2 người chơi ABC đã nhận Thưởng TIÊU DAO CỐC
        /// MarkKEy là tháng 2,MarkType là Tiêu dao cốc,MarkValue truyền vào 1 để thể hiện là đã nhận 1 lần
        /// NẾu muốn cho nhận nhiều lần thì MARKValue có thể đảm nhận việc này
        /// Nếu MarkKey Và MarkType đã tồn tại trong DB thì MarkValue mới sẽ được thay thế MarkValue cũ
        /// </summary>
        /// <param name="client"></param>
        /// <param name="MarkKey"></param>
        /// <param name="MarkType"></param>
        /// <param name="MarkValue"></param>
        public static bool UpdateMarkValue(KPlayer client, string MarkKey, int MarkType, int MarkValue)
        {
            string CMDBUILD = client.RoleID + "#" + MarkKey + "#" + MarkValue + "#" + MarkType;

            string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_KT_UPDATEMARKVALUE, CMDBUILD, GameManager.LocalServerId);

            if (null == dbFields || dbFields.Length != 2)
            {
                LogManager.WriteLog(LogTypes.Error, "[UpdateMarkValue][" + client.RoleID + "][" + MarkKey + "] Lấy giá trị đánh dấu bị lỗi");
            }
            else
            {
                if (Int32.Parse(dbFields[1]) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Hàm này để ghi lại tích lũy bất kỳ của nhân vật trong thời điểm thực hiện lệnh này
        /// Hàm này có thể sử dụng để ghi lại nhật ký của người chơi khi đi tiêu dao cốc
        /// MarkTYpe là key của TIÊU DAO CỐC
        /// MarkVALUE là giá trị điểm đạt được của nhân vật
        /// Khi đọc ra chỉ cần chuyền vào TIMER RANGER từ A tới B hệ thống sẽ SUM toàn bộ VALUE này và trả về cho hệ thống
        /// </summary>
        /// <param name="client"></param>
        /// <param name="MarkType"></param>
        /// <param name="MarkValue"></param>
        /// <returns></returns>
        public static bool AddRecoreByType(KPlayer client, int MarkType, int MarkValue)
        {
            string CMDBUILD = client.RoleID + "|" + client.RoleName + "|" + MarkType + "|" + MarkValue;
            string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_KT_ADD_RECORE_BYTYPE, CMDBUILD, GameManager.LocalServerId);
            if (null == dbFields || dbFields.Length != 2)
            {
                LogManager.WriteLog(LogTypes.Error, "[AddRecoreByType][" + client.RoleID + "][" + MarkType + "] Lấy giá trị đánh dấu bị lỗi :" + MarkValue);
            }
            else
            {
                if (Int32.Parse(dbFields[1]) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Hàm lấy ra tổng tích lũy theo 1 khoảng thời gian
        /// Ví dụ tháng này người chơi A được bao nhiêu điểm thi đấu
        /// </summary>
        /// <param name="client"></param>
        /// <param name="MarkKey"></param>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <returns></returns>
        public static int GetRecoreByType(KPlayer client, int MarkKey, DateTime Start, DateTime End)
        {
            string StartTime = Start.ToString("yyyy-MM-dd HH:mm:ss");
            string EndTime = End.ToString("yyyy-MM-dd HH:mm:ss");
            string CMDBUILD = client.RoleID + "|" + MarkKey + "|" + StartTime + "|" + EndTime;

            string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_KT_GET_RECORE_BYTYPE, CMDBUILD, GameManager.LocalServerId);
            if (null == dbFields || dbFields.Length != 2)
            {
                LogManager.WriteLog(LogTypes.Error, "[GetMarkValue][" + client.RoleID + "][" + MarkKey + "] Lấy giá trị đánh dấu bị lỗi");
            }
            else
            {
                return Int32.Parse(dbFields[1]);
            }

            return -1;
        }

        /// <summary>
        /// Lấy ra top X người chơi theo Markey đã đánh dấu trong 1 khoảng thời gian
        /// Ví dụ lấy ra top 100 thằng có điểm tích lũy cao nhất trong TIÊU DAO CỐC
        /// MarkKey : Tiêu dao cốc
        /// Start : Thời gian bắt đầu của tháng trước đó
        /// End : Thời gian kết thúc của tháng trươc đó
        /// LitmitCount : Lấy ra bao nhiêu thằng => 100 nếu lấy 100 thằng
        /// ZONEID : ZONEID hiện tại
        /// </summary>
        /// <param name="MarkKey"></param>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <param name="LitmitCount"></param>
        /// <param name="ZoneID"></param>
        /// <returns></returns>
        public static List<RecoreRanking> GetRankByMarkAndTimeRanger(int MarkKey, DateTime Start, DateTime End, int LitmitCount, int ZoneID)
        {
            string StartTime = Start.ToString("yyyy-MM-dd HH:mm:ss");
            string EndTime = End.ToString("yyyy-MM-dd HH:mm:ss");

            string CMDBUILD = MarkKey + "#" + LitmitCount + "#" + StartTime + "#" + EndTime;

            byte[] bytesData = null;
            byte[] ByteSendToDB = Encoding.ASCII.GetBytes(CMDBUILD);

            TCPProcessCmdResults result = Global.ReadDataFromDb((int) TCPGameServerCmds.CMD_KT_GETRANK_RECORE_BYTYPE, ByteSendToDB, ByteSendToDB.Length, out bytesData, ZoneID);

            if (TCPProcessCmdResults.RESULT_FAILED != result)
            {
                //Get đồ từ DB ra trả về client
                List<RecoreRanking> goodsDataList = DataHelper.BytesToObject<List<RecoreRanking>>(bytesData, 6, bytesData.Length - 6);
                return goodsDataList;
            }
            return null;
        }

        #endregion

        #region Chức vị bang hội
        /// <summary>
        /// Trả về tên chức vụ trong bang
        /// </summary>
        /// <param name="guildRank"></param>
        /// <returns></returns>
        public static string GetGuildRankName(int guildRank)
        {
            if (guildRank <= (int) GuildRank.Member)
            {
                return "Thành viên";
            }
            else if (guildRank == (int) GuildRank.Master)
            {
                return "Bang chủ";
            }
            else if (guildRank == (int) GuildRank.ViceMaster)
            {
                return "Phó bang chủ";
            }
            else if (guildRank == (int) GuildRank.Ambassador)
            {
                return "Trưởng lão";
            }
            else if (guildRank == (int) GuildRank.ViceAmbassador)
            {
                return "Đường chủ";
            }
            else if (guildRank == (int) GuildRank.Elite)
            {
                return "Tinh anh";
            }
            else
            {
                return "Không rõ";
            }
        }
        #endregion
    }
}
