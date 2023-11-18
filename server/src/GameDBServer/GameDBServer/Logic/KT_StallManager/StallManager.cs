using GameDBServer.Core;
using GameDBServer.Data;
using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace GameDBServer.Logic.StallManager
{
    /// <summary>
    /// Quản lý việc mua bán của nhân vạt
    /// </summary>
    public class StallManager
    {
        /// <summary>
        /// Khởi tạo 1 lớp static ảo
        /// </summary>
        private static StallManager instance = new StallManager();

        /// <summary>
        /// Toàn bộ dữ liệu bán hàng của người chơi
        /// </summary>
        public ConcurrentDictionary<int, StallData> TotalStallData = new ConcurrentDictionary<int, StallData>();

        /// <summary>
        /// Game DB
        /// </summary>
        public DBManager _Database = null;

        /// <summary>
        /// Lấy ra install của thằng này
        /// </summary>
        /// <returns></returns>
        public static StallManager getInstance()
        {
            return instance;
        }

        public void Setup(DBManager _Db)
        {
            this._Database = _Db;
            // Loading all stall
            this.LoadingAllStall();
        }

        /// <summary>
        /// Khi một thằng mới tinh bsan cái gì đó
        /// </summary>
        /// <param name="Info"></param>
        /// <param name="_MiniInfo"></param>
        /// <returns></returns>
        public StallData StallDataFromRoleOnline(int RoleID, MiniStallData _MiniInfo)
        {
            DBRoleInfo Info = this._Database.GetDBRoleInfo(RoleID);

            if (Info == null)
            {
                return null;
            }

            StallData _Stall = new StallData();

            _Stall.AddDateTime = _MiniInfo.AddDateTime;
            _Stall.StallID = _MiniInfo.StallID;
            _Stall.Start = _MiniInfo.Start;
            _Stall.StallName = _MiniInfo.StallName;

            //Thông tin bản đồ
            _Stall.MapCode = _MiniInfo.MapCode;
            _Stall.PosX = _MiniInfo.PosX;
            _Stall.PosY = _MiniInfo.PosY;
            //Nhân vật
            _Stall.RoleName = _MiniInfo.RoleName;
            _Stall.RoleID = _MiniInfo.RoleID;
            _Stall.ListResID = _MiniInfo.ListResID;

            // Có phải bot ko
            _Stall.IsBot = _MiniInfo.IsBot;
            //Vật phẩm
            _Stall.GoodsPriceDict = _MiniInfo.GoodsPriceDict;
            // khởi tạo 1 list vật phẩm rỗng
            _Stall.GoodsList = new List<GoodsData>();

            if (_Stall.GoodsPriceDict != null)
            {
                foreach (int KEY in _Stall.GoodsPriceDict.Keys)
                {
                    if (Info.GoodsDataList.TryGetValue(KEY, out GoodsData _Good))
                    {
                        // add vật phẩm vào list nhé
                        _Stall.GoodsList.Add(_Good);
                    }
                }
            }

            return _Stall;
        }

        /// <summary>
        /// Add một vật phẩm j đó lên giỏ hàng
        /// </summary>
        /// <param name="ItemDbID"></param>
        /// <param name="ItemPrince"></param>
        public bool AddItemToStall(int RoleID, int ItemDbID, int ItemPrince)
        {
            TotalStallData.TryGetValue(RoleID, out StallData _STALLDATA);
            //Nếu thằng này đã có sạp hàng rồi thì ok
            if (_STALLDATA != null)
            {
                DBRoleInfo otherDbRoleInfo = this._Database.GetDBRoleInfo(RoleID);
                if (otherDbRoleInfo != null)
                {
                    otherDbRoleInfo.GoodsDataList.TryGetValue(ItemDbID, out GoodsData _VALUE);

                    if (_VALUE != null)
                    {
                        // Add vật phẩm này vào trong list StallData
                        _STALLDATA.GoodsList.Add(_VALUE);

                        if(_STALLDATA.GoodsPriceDict==null)
                        {
                            _STALLDATA.GoodsPriceDict = new Dictionary<int, int>();
                        }    
                        // Add vào đây vật phẩm và giá
                        _STALLDATA.GoodsPriceDict.Add(ItemDbID, ItemPrince);

                        if (UpdateStallToGameDB(_STALLDATA))
                        {
                            return true;
                        }
                       
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Xóa một vật phẩm khỏi stall
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="ItemDbID"></param>
        /// <returns></returns>
        public bool RemoveItemInStall(int RoleID, int ItemDbID)
        {
            bool isDel = false;
            TotalStallData.TryGetValue(RoleID, out StallData _STALLDATA);
            //Nếu thằng này đã có sạp hàng rồi thì ok
            if (_STALLDATA != null)
            {
                if (_STALLDATA.GoodsPriceDict.ContainsKey(ItemDbID))
                {
                    lock (_STALLDATA)
                    {
                        for (int i = 0; i < _STALLDATA.GoodsList.Count; i++)
                        {
                            if (_STALLDATA.GoodsList[i].Id == ItemDbID)
                            {
                                _STALLDATA.GoodsPriceDict.Remove(ItemDbID);

                                _STALLDATA.GoodsList.RemoveAt(i);

                                if(UpdateStallToGameDB(_STALLDATA))
                                {
                                    isDel = true;
                                }    
                               
                                break;
                            }
                        }
                    }
                }
                else
                {
                    return isDel;
                }
            }

            return isDel;
        }

        public StallData ConvertDataFromDb(MiniStallData _MiniInfo)
        {
            StallData _Stall = new StallData();

            _Stall.AddDateTime = _MiniInfo.AddDateTime;
            _Stall.StallID = _MiniInfo.StallID;
            _Stall.Start = _MiniInfo.Start;
            _Stall.StallName = _MiniInfo.StallName;

            //Thông tin bản đồ
            _Stall.MapCode = _MiniInfo.MapCode;
            _Stall.PosX = _MiniInfo.PosX;
            _Stall.PosY = _MiniInfo.PosY;
            //Nhân vật
            _Stall.RoleName = _MiniInfo.RoleName;
            _Stall.RoleID = _MiniInfo.RoleID;
            _Stall.ListResID = _MiniInfo.ListResID;

            // Có phải bot ko
            _Stall.IsBot = _MiniInfo.IsBot;
            //Vật phẩm
            _Stall.GoodsPriceDict = _MiniInfo.GoodsPriceDict;
            // khởi tạo 1 list vật phẩm rỗng
            _Stall.GoodsList = new List<GoodsData>();


            if (_Stall.GoodsPriceDict != null)
            {

                string ORADD = "";

                foreach (int KEY in _Stall.GoodsPriceDict.Keys)
                {
                    ORADD += "id = " + KEY + " or ";
                }
                // nếu có vật phẩm trong dict
                if (ORADD.Length > 0)
                {
                    ORADD = ORADD.Substring(0, ORADD.Length - 3);

                    using (MySqlUnity Sql = new MySqlUnity())
                    {
                        string CMD = "Select * from t_goods where gcount > 0 and (" + ORADD + ")";

                        DataTable _Table = Sql.ReadSqlToTable(CMD);

                        List<GoodsData> Data = new List<GoodsData>();

                        for (int i = 0; i < _Table.Rows.Count; i++)
                        {
                            string otherPramenter = _Table.Rows[i]["otherpramer"].ToString();

                            byte[] Base64Decode = Convert.FromBase64String(otherPramenter);

                            Dictionary<ItemPramenter, string> _OtherParams = DataHelper.BytesToObject<Dictionary<ItemPramenter, string>>(Base64Decode, 0, Base64Decode.Length);

                            GoodsData goodsData = new GoodsData()
                            {
                                Id = Convert.ToInt32(_Table.Rows[i]["Id"].ToString()),
                                GoodsID = Convert.ToInt32(_Table.Rows[i]["goodsid"].ToString()),
                                Using = Convert.ToInt32(_Table.Rows[i]["isusing"].ToString()),
                                Forge_level = Convert.ToInt32(_Table.Rows[i]["forge_level"].ToString()),
                                Starttime = _Table.Rows[i]["starttime"].ToString(),
                                Endtime = _Table.Rows[i]["endtime"].ToString(),
                                Site = Convert.ToInt32(_Table.Rows[i]["site"].ToString()),
                                Props = _Table.Rows[i]["Props"].ToString(),
                                GCount = Convert.ToInt32(_Table.Rows[i]["gcount"].ToString()),
                                Binding = Convert.ToInt32(_Table.Rows[i]["binding"].ToString()),
                                BagIndex = Convert.ToInt32(_Table.Rows[i]["bagindex"].ToString()),
                                Strong = Convert.ToInt32(_Table.Rows[i]["strong"].ToString()),
                                Series = Convert.ToInt32(_Table.Rows[i]["series"].ToString()),
                                OtherParams = _OtherParams

                                //TODO : ĐỌC NGŨ HÀNH + TIỀN GIAO BÁN LƯU RA
                            };

                            Data.Add(goodsData);
                        }


                        _Stall.GoodsList = Data;
                    }
                }
            }

            return _Stall;
        }

        /// <summary>
        /// Đọc ra toàn bộ sạp hàng từ gamedb
        /// </summary>
        public void LoadingAllStall()
        {
            MySQLConnection conn = null;

            int TotalHous = TimeUtil.GetOffsetHour(DateTime.Now);

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                // Câu truy vấn này lấy ra toàn bộ sạp hàng bắt đầu bày bán được 3 ngày thằng nào quá 3 ngày thì thôi sẽ ko dump lại cửa hàng nữa
                string cmdText = "Select * from t_stalldata where (" + TotalHous + " - StartSellDate < 72)";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    try
                    {
                        int RoleID = Convert.ToInt32(reader["RoleID"].ToString());

                        string Hex = reader["StallData"].ToString();

                        byte[] TotalData = DataHelper.HexString2Bytes(Hex);

                        /// lấy ra toàn bộ dữ liệu stall từ db
                        MiniStallData _Data = DataHelper.BytesToObject<MiniStallData>(TotalData, 0, TotalData.Length);

                        if(_Data!=null)
                        {
                            StallData _FULLDATA = ConvertDataFromDb(_Data);
                            if (_FULLDATA != null)
                            {
                                // Add thằng này vào Dict
                                TotalStallData.TryAdd(RoleID, _FULLDATA);
                            }
                        }
                        // Convert dữ liệu về
                      
                    }
                    catch(Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Error, "BUG:" + ex.ToString());
                    }
                }
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "BUG:" + ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Xóa stall của thằng này
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public bool RemoveStallData(int RoleID)
        {
            if (TotalStallData.TryRemove(RoleID, out StallData _OutStall))
            {
                string SqlScript = "Delete from t_stalldata where RoleID = " + RoleID + "";

                return DBWriter.ExecuteSqlScript(SqlScript);
            }

            return false;
        }
        public  MiniStallData ConverStallToMiniData(StallData Data)
        {
            if (Data != null)
            {
                MiniStallData MiniStallData = new MiniStallData();
                MiniStallData.StallID = Data.StallID;
                MiniStallData.RoleID = Data.RoleID;
                MiniStallData.StallName = Data.StallName;
                MiniStallData.IsBot = Data.IsBot;
                MiniStallData.GoodsPriceDict = Data.GoodsPriceDict;
                MiniStallData.AddDateTime = Data.AddDateTime;
                MiniStallData.Start = Data.Start;
                MiniStallData.ListResID = Data.ListResID;
                MiniStallData.MapCode = Data.MapCode;
                MiniStallData.PosX = Data.PosX;
                MiniStallData.PosY = Data.PosY;
                MiniStallData.RoleName = Data.RoleName;

                return MiniStallData;
            }
            return null;
        }

        /// <summary>
        /// Cập nhật vào db thông tin hàng hóa
        /// </summary>
        /// <param name="_InputData"></param>
        /// <returns></returns>
        public bool UpdateStallToGameDB(StallData _InputData)
        {

            MySQLConnection conn = null;

            bool Ret = false;

            int TotalHous = TimeUtil.GetOffsetHour(DateTime.Now);

            try
            {
                MiniStallData _StallData = ConverStallToMiniData(_InputData);

                conn = _Database.DBConns.PopDBConnection();
                //Convert dữ liệu stall về bytedata
                byte[] Data = DataHelper.ObjectToBytes<MiniStallData>(_StallData);

                string HexArray = DataHelper.Bytes2HexString(Data);

                Console.WriteLine(HexArray);

                string cmdText = "INSERT INTO t_stalldata (RoleID, StartSellDate,StallData) VALUES (" + _InputData.RoleID + ", " + TotalHous + ",'" + HexArray + "') ON DUPLICATE KEY UPDATE StartSellDate = " + TotalHous + ",StallData = '" + HexArray + "'";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                Ret = true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "BUG UpdateStallData:" + ex.ToString());
                Ret = false;
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return Ret;

        }
        /// <summary>
        /// Gói tin tạo 1 STALLDATA sau đó lưu trữ vào DB
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="userID"></param>
        /// <param name="realmoney"></param>
        /// <returns></returns>
        public bool CreteStallData(int RoleID, MiniStallData _StallData)
        {
            // Lấy ra StallData Full
            StallData _StallDataFull = this.StallDataFromRoleOnline(RoleID, _StallData);

            // Xem trước đó đã có gian hàng nào chưa
            if (TotalStallData.TryGetValue(RoleID, out StallData _Value))
            {
                TotalStallData[RoleID] = _StallDataFull;
            }
            else
            {
                // Nếu chưa có thì add mới vào
                TotalStallData.TryAdd(RoleID, _StallDataFull);
            }

            MySQLConnection conn = null;

            bool Ret = false;

            int TotalHous = TimeUtil.GetOffsetHour(DateTime.Now);

            try
            {
                conn = _Database.DBConns.PopDBConnection();
                //Convert dữ liệu stall về bytedata
                byte[] Data = DataHelper.ObjectToBytes<MiniStallData>(_StallData);

                string HexArray = DataHelper.Bytes2HexString(Data);

                Console.WriteLine(HexArray);

                string cmdText = "INSERT INTO t_stalldata (RoleID, StartSellDate,StallData) VALUES (" + RoleID + ", " + TotalHous + ",'" + HexArray + "') ON DUPLICATE KEY UPDATE StartSellDate = " + TotalHous + ",StallData = '" + HexArray + "'";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                cmd.ExecuteNonQuery();

                Ret = true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "BUG UpdateStallData:" + ex.ToString());
                Ret = false;
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return Ret;
        }

        #region NETWORK_ZONE

        public static TCPProcessCmdResults CMD_SPR_STALL_QUERRY(DBManager dbMgr, TCPOutPacketPool pool,
       int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                ConcurrentDictionary<int, StallData> _TotalStall = StallManager.getInstance().TotalStallData;

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<ConcurrentDictionary<int, StallData>>(_TotalStall, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Packet thực hiện toàn bộ các dữ liệu liên quan tới gamedb
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults CMD_SPR_STALL_UDPATE_DB(DBManager dbMgr, TCPOutPacketPool pool,
        int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                string Rep = "";

                StallDbAction _StallData = DataHelper.BytesToObject<StallDbAction>(data, 0, count);

                if (_StallData != null)
                {
                    // Nếu CMD là đã thêm mới
                    if (_StallData.Type == (int)StallCommand.UPDATE)
                    {
                        // Lấy ra dữ liệu mini của thằng này truyền vào
                        MiniStallData _StallDataMini = _StallData.MiniData;

                        int RoleID = _StallData.Fields[0];

                        if (StallManager.getInstance().CreteStallData(RoleID, _StallDataMini))
                        {
                            Rep = "0:" + (int)StallCommand.UPDATE;
                        }
                        else
                        {
                            Rep = "-1:" + (int)StallCommand.UPDATE;
                        }
                    } // Nếu cấu trúc gói tin là thêm mới 1 vật phẩm vào sạp hàng
                    else if (_StallData.Type == (int)StallCommand.INSERT_ITEM)
                    {
                        int RoleID = _StallData.Fields[0];
                        int ItemDbID = _StallData.Fields[1];
                        int Price = _StallData.Fields[2];
                        // Thêm mới 1 món đồ vào sạp hàng
                        if (StallManager.getInstance().AddItemToStall(RoleID, ItemDbID, Price))
                        {
                            Rep = "0:" + (int)StallCommand.INSERT_ITEM;
                        }
                        else
                        {
                            Rep = "-1:" + (int)StallCommand.INSERT_ITEM;
                        }
                    }
                    else if (_StallData.Type == (int)StallCommand.REMOVE_ITEM)
                    {
                        int RoleID = _StallData.Fields[0];
                        int ItemDbID = _StallData.Fields[1];

                        // Thêm mới 1 món đồ vào sạp hàng
                        if (StallManager.getInstance().RemoveItemInStall(RoleID, ItemDbID))
                        {
                            Rep = "0:" + (int)StallCommand.REMOVE_ITEM;
                        }
                        else
                        {
                            Rep = "-1:" + (int)StallCommand.REMOVE_ITEM;
                        }
                    }
                    else if (_StallData.Type == (int)StallCommand.DELETE_STALL)
                    {
                        int RoleID = _StallData.Fields[0];

                        // Thêm mới 1 món đồ vào sạp hàng
                        if (StallManager.getInstance().RemoveStallData(RoleID))
                        {
                            Rep = "0:" + (int)StallCommand.DELETE_STALL;
                        }
                        else
                        {
                            Rep = "-1:" + (int)StallCommand.DELETE_STALL;
                        }
                    }

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, Rep, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            var bytes = DataHelper.ObjectToBytes(0);
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        #endregion NETWORK_ZONE
    }
}