using GameDBServer.DB;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameDBServer.Logic
{
    /// <summary>
    /// Thiết lập hệ thống
    /// </summary>
    public class GameConfig
    {
        /// <summary>
        /// Danh sách thiết lập
        /// </summary>
        private Dictionary<string, string> _GameConfigDict = new Dictionary<string, string>();

        /// <summary>
        /// Đánh dấu đã khởi tạo chưa
        /// </summary>
        private bool Initialized = false;

        /// <summary>
        /// Khởi tạo
        /// </summary>
        /// <param name="init"></param>
        public void InitGameDBManagerFlags(bool init = false)
        {
            GameDBManager.Flag_t_goods_delete_immediately = GameDBManager.GameConfigMgr.GetGameConfigItemInt("flag_t_goods_delete_immediately", 1) > 0;
            GameDBManager.Flag_Query_Total_UserMoney_Minute = GameDBManager.GameConfigMgr.GetGameConfigItemInt("query_total_usermoney_minute", 60);

            if (!Initialized)
            {
                Initialized = true;
                GameDBManager.Flag_Splite_RoleParams_Table = GameDBManager.GameConfigMgr.GetGameConfigItemInt("opt_roleparams", 1);
            }
        }

        /// <summary>
        /// Tải thiết lập hệ thống từ DB
        /// </summary>
        public void LoadGameConfigFromDB(DBManager dbMgr)
        {
            _GameConfigDict = DBQuery.QueryGameConfigDict(dbMgr);
            if (null == _GameConfigDict)
            {
                _GameConfigDict = new Dictionary<string, string>();
            }

            InitGameDBManagerFlags();
        }

        /// <summary>
        /// Cập nhật thiết lập hệ thống
        /// </summary>
        public void UpdateGameConfigItem(string paramName, string paramValue)
        {
            lock (_GameConfigDict)
            {
                _GameConfigDict[paramName] = paramValue;
            }

            InitGameDBManagerFlags();
        }

        /// <summary>
        /// Lấy thông tin thiết lập hệ thống
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public string GetGameConifgItem(string paramName)
        {
            string paramValue = null;
            lock (_GameConfigDict)
            {
                if (!_GameConfigDict.TryGetValue(paramName, out paramValue))
                {
                    paramValue = null;
                }
            }

            return paramValue;
        }

        /// <summary>
        /// Lấy thông tin cấu hình hệ thống dạng String
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public string GetGameConfigItemStr(string paramName, string defVal)
        {
            string ret = GetGameConifgItem(paramName);
            if (string.IsNullOrEmpty(ret))
            {
                return defVal;
            }

            return ret;
        }

        /// <summary>
        /// Lấy thông tin cấu hình hệ thống dạng Int
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public int GetGameConfigItemInt(string paramName, int defVal)
        {
            string str = GetGameConifgItem(paramName);
            if (string.IsNullOrEmpty(str))
            {
                return defVal;
            }

            int ret = 0;

            try
            {
                ret = Convert.ToInt32(str);
            }
            catch (Exception)
            {
                ret = defVal;
            }

            return ret;
        }

        /// <summary>
        /// Lấy thông tin cấu hình hệ thống dạng Double
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public double GetGameConfigItemInt(string paramName, double defVal)
        {
            string str = GetGameConifgItem(paramName);
            if (string.IsNullOrEmpty(str))
            {
                return defVal;
            }

            double ret = 0.0;

            try
            {
                ret = Convert.ToDouble(str);
            }
            catch (Exception)
            {
                ret = defVal;
            }

            return ret;
        }

        /// <summary>
        /// Lấy thông tin TCP thiết lập hệ thống
        /// </summary>
        public TCPOutPacket GetGameConfigDictTCPOutPacket(TCPOutPacketPool pool, int cmdID)
        {
            TCPOutPacket tcpOutPacket = null;
            lock (_GameConfigDict)
            {
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<Dictionary<string, string>>(_GameConfigDict, pool, cmdID);
            }

            return tcpOutPacket;
        }
    }
}