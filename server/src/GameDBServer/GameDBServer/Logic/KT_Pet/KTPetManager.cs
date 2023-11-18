using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameDBServer.Logic.Pet
{
    /// <summary>
    /// Quản lý Pet
    /// </summary>
    public class KTPetManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý Pet
        /// </summary>
        public static KTPetManager Instance { get; private set; }

        /// <summary>
        /// Quản lý Pet
        /// </summary>
        private KTPetManager(DBManager dBManager)
        {
            /// Lưu lại đối tượng quản lý DB
            this.Database = dBManager;
        }
        #endregion

        #region Init
        /// <summary>
        /// Quản lý DB
        /// </summary>
        private readonly DBManager Database;

        /// <summary>
        /// Khởi tạo quản lý Pet
        /// </summary>
        public static void Init(DBManager dBManager)
        {
            KTPetManager.Instance = new KTPetManager(dBManager);
        }
        #endregion

        #region Database connection
        /// <summary>
        /// Cập nhật thông tin Res pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="resID"></param>
        private bool DB_UpdatePetData_ResID(int petID, int resID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET res_id = {1} WHERE id = {0}", petID, resID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin tên pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="name"></param>
        private bool DB_UpdatePetData_Name(int petID, string name)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET name = '{1}' WHERE id = {0}", petID, DataHelper.Base64Encode(name));

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin cấp độ và kinh nghiệm pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        private bool DB_UpdatePetData_LevelAndExp(int petID, int level, int exp)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET level = {1}, exp = {2} WHERE id = {0}", petID, level, exp);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin lĩnh ngộ pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="enlightenment"></param>
        private bool DB_UpdatePetData_Enlightenment(int petID, int enlightenment)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET enlightenment = {1} WHERE id = {0}", petID, enlightenment);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin kỹ năng pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="skills"></param>
        private bool DB_UpdatePetData_Skills(int petID, string skills)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET skills = '{1}' WHERE id = {0}", petID, skills);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin trang bị pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="equips"></param>
        private bool DB_UpdatePetData_Equips(int petID, string equips)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET equips = '{1}' WHERE id = {0}", petID, equips);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }


        private bool DB_UpdatePetRoleID(int petID, int RoleID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET role_id = {0} WHERE id = {1}", RoleID, petID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Xóa pet khỏi DB
        /// </summary>
        /// <param name="petID"></param>
        private bool DB_DeletePet(int petID)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("DELETE FROM t_pet WHERE id = {0}", petID);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Thêm Pet vào DB
        /// </summary>
        /// <param name="petData"></param>
        /// <returns></returns>
        private int DB_AddPet(PetData petData)
        {
            /// ID được thêm vào
            int insertedID = -1;

            MySQLConnection conn = null;

            try
            {
                /// Chuỗi mã hóa kỹ năng
                List<string> skillsStringList = new List<string>();
                /// Nếu tồn tại kỹ năng
                if (petData.Skills != null)
                {
                    /// Duyệt danh sách
                    foreach (KeyValuePair<int, int> pair in petData.Skills)
                    {
                        /// Thêm vào chuỗi
                        skillsStringList.Add(string.Format("{0}_{1}", pair.Key, pair.Value));
                    }
                }

                /// Chuỗi mã hóa trang bị
                List<string> equipsStringList = new List<string>();
                /// Nếu tồn tại kỹ năng
                if (petData.Equips != null)
                {
                    /// Duyệt danh sách
                    foreach (KeyValuePair<int, int> pair in petData.Equips)
                    {
                        /// Thêm vào chuỗi
                        equipsStringList.Add(string.Format("{0}_{1}", pair.Key, pair.Value));
                    }
                }


                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("INSERT INTO t_pet(role_id, res_id, name, level, exp, enlightenment, skills, equips, str, dex, sta, ene, remain_points, hp, joyful, life) VALUES({0}, {1}, '{2}', {3}, {4}, {5}, '{6}', '{7}', {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15})", petData.RoleID, petData.ResID, DataHelper.Base64Encode(petData.Name), petData.Level, petData.Exp, petData.Enlightenment, string.Join("|", skillsStringList), string.Join("|", equipsStringList), petData.Str, petData.Dex, petData.Sta, petData.Int, petData.RemainPoints, petData.HP, petData.Joyful, petData.Life);

                /// Đoạn này thực hiện thêm vào DB
                {
                    MySQLCommand cmd = new MySQLCommand(queryString, conn);
                    int affectedRows = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    cmd = null;

                    /// Toác
                    if (affectedRows < 0)
                    {
                        return -1;
                    }
                }

                /// Đoạn này thực hiện truy vấn ID cuối cùng được thêm vào
                {
                    MySQLCommand cmd = new MySQLCommand("SELECT LAST_INSERT_ID() AS inserted_id", conn);
                    MySQLDataReader reader = cmd.ExecuteReaderEx();
                    if (reader.Read())
                    {
                        /// ID được thêm vào
                        insertedID = Convert.ToInt32(reader["inserted_id"].ToString());
                    }
                    cmd.Dispose();
                    cmd = null;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }

            /// Trả về kết quả
            return insertedID;
        }

        /// <summary>
        /// Cập nhật thông tin thuộc tính pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="str"></param>
        /// <param name="dex"></param>
        /// <param name="sta"></param>
        /// <param name="intel"></param>
        /// <param name="remainPoints"></param>
        private bool DB_UpdatePetData_Attributes(int petID, int str, int dex, int sta, int intel, int remainPoints)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET str = {1}, dex = {2}, sta = {3}, ene = {4}, remain_points = {5} WHERE id = {0}", petID, str, dex, sta, intel, remainPoints);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin độ vui vẻ pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="joyful"></param>
        private bool DB_UpdatePetData_Joyful(int petID, int joyful)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET joyful = {1} WHERE id = {0}", petID, joyful);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin tuổi thọ pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="life"></param>
        private bool DB_UpdatePetData_Life(int petID, int life)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET joyful = {1} WHERE id = {0}", petID, life);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin sinh lực pet vào DB
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="hp"></param>
        private bool DB_UpdatePetData_HP(int petID, int hp)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET hp = {1} WHERE id = {0}", petID, hp);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin pet vào DB trước khi thoát Game
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="hp"></param>
        /// <param name="joyful"></param>
        /// <param name="life"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        private bool DB_UpdatePetData_BeforeQuitGame(int petID, int hp, int joyful, int life, int level, int exp)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this.Database.DBConns.PopDBConnection();
                string queryString = string.Format("UPDATE t_pet SET hp = {1}, joyful = {2}, life = {3}, level = {4}, exp = {5} WHERE id = {0}", petID, hp, joyful, life, level, exp);

                MySQLCommand cmd = new MySQLCommand(queryString, conn);
                int affectedRows = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cmd = null;
                /// OK
                return affectedRows == 1;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Pet, ex.ToString());
                /// Toác
                return false;
            }
            finally
            {
                if (null != conn)
                {
                    this.Database.DBConns.PushDBConnection(conn);
                }
            }
        }
        #endregion

        #region TCP-Core
        /// <summary>
        /// Xử lý gói tin truy vấn thông tin Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessGetPetList(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Danh sách Pet
                List<PetData> pets = dbRoleInfo.PetList.Values.ToList();

                /// Chuỗi byte kết quả
                byte[] resultBytes = DataHelper.ObjectToBytes<List<PetData>>(pets);
                /// Trả về kết quả
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, resultBytes, 0, resultBytes.Length, nID);
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
        /// Xử lý gói tin cập nhật cấp độ và kinh nghiệm Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateLevelAndExp(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Cấp độ pet
                int petLevel = int.Parse(fields[2]);
                /// Kinh nghiệm pet
                int petExp = int.Parse(fields[3]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_LevelAndExp(petID, petLevel, petExp);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.Level = petLevel;
                        petData.Exp = petExp;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật ResID Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateResID(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// ID Res
                int resID = int.Parse(fields[2]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_ResID(petID, resID);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.ResID = resID;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật lĩnh ngộ Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateEnlightenment(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Lĩnh ngộ pet
                int enlightenment = int.Parse(fields[2]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_Enlightenment(petID, enlightenment);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.Enlightenment = enlightenment;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật tên Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateName(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Tên pet
                string petName = fields[2];

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_Name(petID, petName);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.Name = petName;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật danh sách kỹ năng Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateSkills(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Thông tin kỹ năng
                string skills = fields[2];

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_Skills(petID, skills);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        /// Nếu chưa có skill
                        if (petData.Skills == null)
                        {
                            /// Tạo mới
                            petData.Skills = new Dictionary<int, int>();
                        }

                        /// Làm rỗng danh sách kỹ năng
                        petData.Skills.Clear();

                        /// Thông tin kỹ năng
                        string skillInfoString = skills;
                        /// Nếu tồn tại
                        if (!string.IsNullOrEmpty(skillInfoString))
                        {
                            /// Các trường
                            string[] skillInfos = skillInfoString.Split('|');
                            /// Duyệt danh sách các trường
                            foreach (string skillInfo in skillInfos)
                            {
                                /// Chia nhỏ
                                string[] _fields = skillInfo.Split('_');
                                /// ID kỹ năng
                                int skillID = Convert.ToInt32(_fields[0]);
                                /// Cấp độ
                                int level = Convert.ToInt32(_fields[1]);
                                /// Thêm vào danh sách
                                petData.Skills[skillID] = level;
                            }
                        }
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật danh sách trang bị Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateEquips(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Thông tin trang bị
                string equips = fields[2];

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_Equips(petID, equips);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        /// Thông tin trang bị
                        string equipInfoString = equips;
                        /// Nếu tồn tại
                        if (!string.IsNullOrEmpty(equipInfoString))
                        {
                            /// Các trường
                            string[] equipInfos = equipInfoString.Split('|');
                            /// Duyệt danh sách các trường
                            foreach (string equipInfo in equipInfos)
                            {
                                /// Chia nhỏ
                                string[] _fields = equipInfo.Split('_');
                                /// Vị trí
                                int equipPos = Convert.ToInt32(_fields[0]);
                                /// ID trang bị
                                int equipID = Convert.ToInt32(_fields[1]);
                                /// Thêm vào danh sách
                                petData.Equips[equipPos] = equipID;
                            }
                        }
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin xóa Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessDeletePet(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_DeletePet(petID);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Xóa khỏi Cache
                    dbRoleInfo.PetList.TryRemove(petID, out _);

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin thêm Pet cho người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessAddPet(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            PetData petData = null;

            try
            {
                petData = DataHelper.BytesToObject<PetData>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                /// Toác
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet data is null, CMD={0}", (TCPGameServerCmds)nID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(petData.RoleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, petData.RoleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                int petID = this.DB_AddPet(petData);
                /// Toác
                if (petID == -1)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Thiết lập ID
                    petData.ID = petID;
                    /// Thêm vào danh sách
                    dbRoleInfo.PetList[petID] = petData;

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", petID), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật thuộc tính Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateAttributes(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 7)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Sức mạnh
                int str = int.Parse(fields[2]);
                /// Thân pháp
                int dex = int.Parse(fields[3]);
                /// Ngoại công
                int sta = int.Parse(fields[4]);
                /// Nội công
                int intel = int.Parse(fields[5]);
                /// Tiềm năng
                int remainPoints = int.Parse(fields[6]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_Attributes(petID, str, dex, sta, intel, remainPoints);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.Str = str;
                        petData.Dex = dex;
                        petData.Sta = sta;
                        petData.Int = intel;
                        petData.RemainPoints = remainPoints;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật độ vui vẻ Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateJoyful(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Độ vui vẻ pet
                int joyful = int.Parse(fields[2]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_Joyful(petID, joyful);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.Joyful = joyful;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật giá trị tuổi thọ Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateLife(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Tuổi thọ pet
                int life = int.Parse(fields[2]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_Life(petID, life);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.Life = life;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật giá trị sinh lực Pet của người chơi tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateHP(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Sinh lực pet
                int hp = int.Parse(fields[2]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_HP(petID, hp);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.HP = hp;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Xử lý gói tin cập nhật giá trị Pet của người chơi tương ứng trước khi thoát Game
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults ProcessPetUpdateBeforeQuitGame(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 7)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);
                /// ID pet
                int petID = int.Parse(fields[1]);
                /// Sinh lực pet
                int hp = int.Parse(fields[2]);
                /// Độ vui vẻ pet
                int joyful = int.Parse(fields[3]);
                /// Tuổi thọ pet
                int life = int.Parse(fields[4]);
                /// Cấp độ pet
                int level = int.Parse(fields[5]);
                /// Kinh nghiệm pet
                int exp = int.Parse(fields[6]);

                /// DBRoleInfo
                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                /// Toác
                if (dbRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetData_BeforeQuitGame(petID, hp, joyful, life, level, exp);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    if (dbRoleInfo.PetList.TryGetValue(petID, out PetData petData))
                    {
                        petData.HP = hp;
                        petData.Joyful = joyful;
                        petData.Life = life;
                        petData.Level = level;
                        petData.Exp = exp;
                    }

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Chuyển pet cho 1 thằng khác
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public TCPProcessCmdResults MovePetToOtherRole(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                int toClient = int.Parse(fields[0]);
                /// ID pet
                int fromClient = int.Parse(fields[1]);

                int PetDBID = int.Parse(fields[2]);

                /// DBRoleInfo
                DBRoleInfo dbtoClientRoleInfo = dbMgr.GetDBRoleInfo(toClient);

                DBRoleInfo dbfromClientRoleInfo = dbMgr.GetDBRoleInfo(fromClient);
                /// Toác
                if (dbtoClientRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, toClient));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (dbfromClientRoleInfo == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, dbfromClientRoleInfo));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Cập nhật vào DB
                bool ret = this.DB_UpdatePetRoleID(PetDBID, toClient);
                /// Toác
                if (!ret)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}:{1}", -1,PetDBID), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// OK
                else
                {
                    dbfromClientRoleInfo.PetList.TryGetValue(PetDBID, out PetData _PETDATA);
                    if(_PETDATA!=null)
                    {
                        // Add cho thawngf thuws 2 con pet nayf
                        dbtoClientRoleInfo.PetList.TryAdd(PetDBID, _PETDATA);
                    }

                    /// Xóa con pet này đi
                    dbfromClientRoleInfo.PetList.TryRemove(PetDBID, out _);

                    /// Gửi gói tin thành công
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}:{1}", 0,PetDBID), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        #endregion
    }
}
