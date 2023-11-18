using GameDBServer.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameDBServer.Logic.SystemParameters
{
    /// <summary>
    /// Quản lý các tham biến hệ thống
    /// </summary>
    public static class SystemGlobalParametersManager
    {
        /// <summary>
        /// Thời gian Update lần trước
        /// </summary>
        private static long LastUpdateTick = 0;

        /// <summary>
        /// Danh sách tham biến hệ thống
        /// </summary>
        private static ConcurrentDictionary<int, string> SystemGlobalParameters = new ConcurrentDictionary<int, string>();

        /// <summary>
        /// Cập nhật giá trị tham biến hệ thống
        /// </summary>
        /// <param name="dbMgr"></param>
        public static void UpdateSystemGlobalParameters(DBManager dbMgr)
        {
            /// Tick hiện tại
            long nowTicks = DateTime.Now.Ticks / 10000;
            /// Nếu chưa đến thời gian
            if (nowTicks - LastUpdateTick < (10 * 1000))
            {
                return;
            }
            /// Cập nhật thời điểm Update
            SystemGlobalParametersManager.LastUpdateTick = nowTicks;

            /// Duyệt danh sách các tham biến hệ thống
            List<int> keys = SystemGlobalParametersManager.SystemGlobalParameters.Keys.ToList();
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!SystemGlobalParametersManager.SystemGlobalParameters.TryGetValue(key, out string value))
                {
                    continue;
                }
                /// Thực hiện lưu vào DB
                DBQuery.SaveSystemGlobalValue(dbMgr, key, value);
            }
        }

        /// <summary>
        /// Truy vấn toàn bộ các tham biến hệ thống từ DB
        /// </summary>
        /// <param name="dbMgr"></param>
        public static void QuerySystemGlobalParameters(DBManager dbMgr)
        {
            /// Thiết lập danh sách theo truy vấn từ DB
            SystemGlobalParametersManager.SystemGlobalParameters = DBQuery.GetSystemGlobalValues(dbMgr);
        }

        /// <summary>
        /// Trả về giá trị biến toàn cục hệ thống có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetSystemGlobalParameter(int id)
        {
            if (SystemGlobalParametersManager.SystemGlobalParameters.TryGetValue(id, out string value))
            {
                return value;
            }
            return "";
        }

        /// <summary>
        /// Thiết lập giá trị biến toàn cục hệ thống có ID tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetSystemGlobalParameter(int id, string value)
        {
            SystemGlobalParametersManager.SystemGlobalParameters[id] = value;
        }
    }
}