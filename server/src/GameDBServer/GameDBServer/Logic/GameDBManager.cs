using System.Collections.Generic;

//using Task.Tool;

namespace GameDBServer.Logic
{
    /// <summary>
    /// Quản lý GameDB
    /// </summary>
    public class GameDBManager
    {
        /// <summary>
        /// Thống kê loại
        /// </summary>
        public const int StatisticsMode = 3;

        /// <summary>
        /// ID khu vực
        /// </summary>
        public static int ZoneID { get; set; } = 1;

        public static int HttpPort { get; set; } = 1;

        /// <summary>
        /// Tên DB
        /// </summary>
        public static string DBName { get; set; } = "kt_gamedb";

        /// <summary>
        /// Sự kiện lịch sử SQL
        /// </summary>
        public static ServerEvents SystemServerSQLEvents = new ServerEvents() { EventRootPath = "SQLs", EventPreFileName = "sql" };

        /// <summary>
        /// Cấu hình GameDB
        /// </summary>
        public static GameConfig GameConfigMgr = new GameConfig();



        public static int DBAutoIncreaseStepValue = 1000000;

        public static int Guild_FamilyIncreaseStepValue = 100000;

        public const int KuaFuServerIdStartValue = 9000;


        public const int ServerLineIdAllIncludeKuaFu = 0;


        public const int ServerLineIdAllLineExcludeSelf = -1000;




        public static bool Flag_t_goods_delete_immediately = true;

        public static bool IsUsingQueeItem = false;


        public static int Flag_Splite_RoleParams_Table = 0;


        public static int Flag_Query_Total_UserMoney_Minute = 60;

        public static Dictionary<string, int> IPRange2AutoIncreaseStepDict = new Dictionary<string, int>()
            {
                { "101.251", 1000000 },
                { "192.168", 0 },
            };
    }
}