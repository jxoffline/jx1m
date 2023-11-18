using GameDBServer.Logic;
using GameDBServer.Logic.Pet;
using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace GameDBServer.DB
{
    /// <summary>
    /// Code lại cho đỡ toác
    /// </summary>
    public class DBRoleInfo
    {
        /// <summary>
        /// RoleiD
        /// </summary>
        public int RoleID
        {
            get;
            set;
        }

        private object _MoneyLock = new object();

        public object GetMoneyLock
        {
            get { return _MoneyLock; }
        }

        public string UserID
        {
            get;
            set;
        }

        public string RoleName
        {
            get;
            set;
        }

        public int RoleSex
        {
            get;
            set;
        }

        /// <summary>
        /// ID phái
        /// </summary>
        public int Occupation
        {
            get;
            set;
        }

        /// <summary>
        /// ID nhánh
        /// </summary>
        public int SubID
        {
            get;
            set;
        }

        public int Level
        {
            get;
            set;
        }

        public int RolePic
        {
            get;
            set;
        }

        public int GuildID
        {
            get;
            set;
        }

        public int Money1
        {
            get;
            set;
        }

        public int Money2
        {
            get;
            set;
        }

        public long Experience
        {
            get;
            set;
        }

        public int PKMode
        {
            get;
            set;
        }

        public int PKValue
        {
            get;
            set;
        }

        public string Position
        {
            get;
            set;
        }

        public string RegTime
        {
            get;
            set;
        }

        public long LastTime
        {
            get;
            set;
        }

        public int BagNum
        {
            get;
            set;
        }

        public string MainQuickBarKeys
        {
            get;
            set;
        }

        public string OtherQuickBarKeys
        {
            get;
            set;
        }

        /// <summary>
        /// Danh sách vật phẩm dùng nhanh
        /// </summary>
        public string QuickItems { get; set; }

        /// <summary>
        /// Mật khẩu cấp 2
        /// </summary>
        public string SecondPassword { get; set; }

        public int LoginNum
        {
            get;
            set;
        }

        public int LeftFightSeconds
        {
            get;
            set;
        }

        public int ServerLineID
        {
            get;
            set;
        }

        public int TotalOnlineSecs
        {
            get;
            set;
        }

        public int AntiAddictionSecs
        {
            get;
            set;
        }

        public long LogOffTime
        {
            get;
            set;
        }

        public int YinLiang
        {
            get;
            set;
        }

        public int MainTaskID
        {
            get;
            set;
        }

        public int PKPoint
        {
            get;
            set;
        }

        /// <summary>
        /// ID gia tộc
        /// </summary>
        public int FamilyID { get; set; }

        /// <summary>
        /// Tên gia tộc
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Chức vị trong gia tộc
        /// </summary>
        public int FamilyRank { get; set; }

        /// <summary>
        /// Uy danh
        /// </summary>
        public int Prestige { get; set; }

        public int KillBoss
        {
            get;
            set;
        }

        public int CZTaskID
        {
            get;
            set;
        }

        public int LoginDayID
        {
            get;
            set;
        }

        public int LoginDayNum
        {
            get;
            set;
        }

        public int ZoneID
        {
            get;
            set;
        }

        public string GuildName
        {
            get;
            set;
        }

        public int GuildRank
        {
            get;
            set;
        }

        public int RoleGuildMoney
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        public int LastMailID
        {
            get;
            set;
        }

        public long OnceAwardFlag
        {
            get;
            set;
        }

        public int Gold
        {
            get;
            set;
        }

        public int IsFlashPlayer
        {
            get;
            set;
        }

        public int AdmiredCount
        {
            get;
            set;
        }

        public long store_yinliang
        {
            get;
            set;
        }

        public int store_money
        {
            get;
            set;
        }

        /// <summary>
        /// Role Prammenter
        /// </summary>
        public ConcurrentDictionary<string, RoleParamsData> RoleParamsDict { get; set; } = new ConcurrentDictionary<string, RoleParamsData>();

        /// <summary>
        /// Danh sách nhiệm vụ đã hoàn thành
        /// </summary>
        public List<OldTaskData> OldTasks
        {
            get;
            set;
        }

        /// <summary>
        /// Danh sách nhiệm vụ
        /// </summary>
        public ConcurrentDictionary<int, TaskData> DoingTaskList
        {
            get;
            set;
        }

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public ConcurrentDictionary<int, GoodsData> GoodsDataList
        {
            get;
            set;
        }


        /// <summary>
        /// Danh sách vật phẩm thằng này vứt ra đất chỉ cache khi bật gamedb.Chạy lại là mất
        /// </summary>
        public ConcurrentDictionary<int, GoodsData> DropDataList
        {
            get;
            set;
        }
        /// <summary>
        /// List vật phẩm sử dụng có giới hạn
        /// </summary>
        public List<GoodsLimitData> GoodsLimitDataList
        {
            get;
            set;
        }

        /// <summary>
        /// Danh sách bạn bè
        /// </summary>
        public List<FriendData> FriendDataList
        {
            get;
            set;
        }

        /// <summary>
        /// Danh sách Pet
        /// </summary>
        public ConcurrentDictionary<int, PetData> PetList { get; set; } = new ConcurrentDictionary<int, PetData>();

        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        public ConcurrentDictionary<int, SkillData> SkillDataList
        {
            get;
            set;
        }

        /// <summary>
        /// Danh sách Buff
        /// </summary>
        public ConcurrentDictionary<int, BufferData> BufferDataList
        {
            get;
            set;
        }


        public RoleWelfare RoleWelfare
        {
            get;
            set;
        }

        private long _LastReferenceTicks = DateTime.Now.Ticks / 10000;

        public long LastReferenceTicks
        {
            get { return _LastReferenceTicks; }
            set { _LastReferenceTicks = value; }
        }

        public string LastIP
        {
            get;
            set;
        }

        public List<int> GroupMailRecordList
        {
            get;
            set;
        }

        public Dictionary<int, Dictionary<int, SevenDayItemData>> SevenDayActDict
        {
            get;
            set;
        }

        public long BanTradeToTicks
        {
            get;
            set;
        }

        public Dictionary<int, SpecActInfoDB> SpecActInfoDict
        {
            get;
            set;
        }

        public static void DBTableRow2RoleInfo(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd, int index)
        {
            dbRoleInfo.RoleID = Convert.ToInt32(cmd.Table.Rows[index]["rid"]);
            dbRoleInfo.UserID = cmd.Table.Rows[index]["userid"].ToString();
            dbRoleInfo.RoleName = cmd.Table.Rows[index]["rname"].ToString();
            dbRoleInfo.RoleSex = Convert.ToInt32(cmd.Table.Rows[index]["sex"]);
            dbRoleInfo.Occupation = Convert.ToInt32(cmd.Table.Rows[index]["occupation"]);
            dbRoleInfo.SubID = Convert.ToInt32(cmd.Table.Rows[index]["sub_id"]);
            dbRoleInfo.Level = Convert.ToInt32(cmd.Table.Rows[index]["level"]);
            dbRoleInfo.RolePic = Convert.ToInt32(cmd.Table.Rows[index]["pic"]);
            dbRoleInfo.Money1 = Convert.ToInt32(cmd.Table.Rows[index]["money1"]);
            dbRoleInfo.Money2 = Convert.ToInt32(cmd.Table.Rows[index]["money2"]);
            dbRoleInfo.Experience = Convert.ToInt64(cmd.Table.Rows[index]["experience"]);
            dbRoleInfo.PKMode = Convert.ToInt32(cmd.Table.Rows[index]["pkmode"]);
            dbRoleInfo.PKValue = Convert.ToInt32(cmd.Table.Rows[index]["pkvalue"]);
            dbRoleInfo.Position = cmd.Table.Rows[index]["position"].ToString();
            dbRoleInfo.RegTime = cmd.Table.Rows[index]["regtime"].ToString();
            dbRoleInfo.LastTime = DataHelper.ConvertToTicks(cmd.Table.Rows[index]["lasttime"].ToString());
            dbRoleInfo.BagNum = Convert.ToInt32(cmd.Table.Rows[index]["bagnum"]);
            dbRoleInfo.MainQuickBarKeys = cmd.Table.Rows[index]["main_quick_keys"].ToString();
            dbRoleInfo.OtherQuickBarKeys = cmd.Table.Rows[index]["other_quick_keys"].ToString();
            dbRoleInfo.QuickItems = cmd.Table.Rows[index]["quick_items"] == null ? "" : cmd.Table.Rows[index]["quick_items"].ToString();
            dbRoleInfo.SecondPassword = cmd.Table.Rows[index]["second_password"] == null ? "" : cmd.Table.Rows[index]["second_password"].ToString();
            dbRoleInfo.LoginNum = Convert.ToInt32(cmd.Table.Rows[index]["loginnum"].ToString());
            dbRoleInfo.LeftFightSeconds = Convert.ToInt32(cmd.Table.Rows[index]["leftfightsecs"].ToString());
            dbRoleInfo.TotalOnlineSecs = Convert.ToInt32(cmd.Table.Rows[index]["totalonlinesecs"].ToString());
            dbRoleInfo.AntiAddictionSecs = Convert.ToInt32(cmd.Table.Rows[index]["antiaddictionsecs"].ToString());
            dbRoleInfo.LogOffTime = DataHelper.ConvertToTicks(cmd.Table.Rows[index]["logofftime"].ToString());
            dbRoleInfo.YinLiang = Convert.ToInt32(cmd.Table.Rows[index]["yinliang"].ToString());
            dbRoleInfo.MainTaskID = Convert.ToInt32(cmd.Table.Rows[index]["maintaskid"].ToString());
            dbRoleInfo.PKPoint = Convert.ToInt32(cmd.Table.Rows[index]["pkpoint"].ToString());
            dbRoleInfo.KillBoss = Convert.ToInt32(cmd.Table.Rows[index]["killboss"].ToString());
            dbRoleInfo.CZTaskID = Convert.ToInt32(cmd.Table.Rows[index]["cztaskid"].ToString());
            dbRoleInfo.LoginDayID = Convert.ToInt32(cmd.Table.Rows[index]["logindayid"].ToString());
            dbRoleInfo.LoginDayNum = Convert.ToInt32(cmd.Table.Rows[index]["logindaynum"].ToString());
            dbRoleInfo.ZoneID = Convert.ToInt32(cmd.Table.Rows[index]["zoneid"].ToString());
            dbRoleInfo.UserName = cmd.Table.Rows[index]["username"].ToString();
            dbRoleInfo.LastMailID = Convert.ToInt32(cmd.Table.Rows[index]["lastmailid"].ToString());
            dbRoleInfo.OnceAwardFlag = Convert.ToInt64(cmd.Table.Rows[index]["onceawardflag"].ToString());
            dbRoleInfo.Gold = Convert.ToInt32(cmd.Table.Rows[index]["money2"].ToString());
            dbRoleInfo.IsFlashPlayer = Convert.ToInt32(cmd.Table.Rows[index]["isflashplayer"].ToString());
            dbRoleInfo.AdmiredCount = Convert.ToInt32(cmd.Table.Rows[index]["admiredcount"].ToString());
            dbRoleInfo.store_yinliang = Convert.ToInt64(cmd.Table.Rows[index]["store_yinliang"]);
            dbRoleInfo.store_money = Convert.ToInt32(cmd.Table.Rows[index]["store_money"]);
            dbRoleInfo.BanTradeToTicks = Convert.ToInt64(cmd.Table.Rows[index]["ban_trade_to_ticks"].ToString());

            //KT ADD 8/9/2021

            dbRoleInfo.FamilyID = Convert.ToInt32(cmd.Table.Rows[index]["familyid"].ToString());
            dbRoleInfo.FamilyName = cmd.Table.Rows[index]["familyname"].ToString();
            dbRoleInfo.FamilyRank = Convert.ToInt32(cmd.Table.Rows[index]["familyrank"].ToString());

            dbRoleInfo.Prestige = Convert.ToInt32(cmd.Table.Rows[index]["roleprestige"].ToString());
            dbRoleInfo.RoleGuildMoney = Convert.ToInt32(cmd.Table.Rows[index]["guildmoney"]);
            dbRoleInfo.GuildRank = Convert.ToInt32(cmd.Table.Rows[index]["guildrank"].ToString());
            dbRoleInfo.GuildID = Convert.ToInt32(cmd.Table.Rows[index]["guildid"]);
            dbRoleInfo.GuildName = cmd.Table.Rows[index]["guildname"].ToString();
        }

        public static void DBTableRow2RoleInfo_Params(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd, bool normalOnly)
        {
            if (cmd.Table.Rows.Count > 0)
            {
                ConcurrentDictionary<string, RoleParamsData> dict = dbRoleInfo.RoleParamsDict;
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    RoleParamsData roleParamsData = new RoleParamsData()
                    {
                        ParamName = cmd.Table.Rows[i]["pname"].ToString(),
                        ParamValue = cmd.Table.Rows[i]["pvalue"].ToString(),
                    };

                    roleParamsData.ParamType = RoleParamNameInfo.GetRoleParamType(roleParamsData.ParamName, roleParamsData.ParamValue);
                    if (roleParamsData.ParamType.Type > 0 && normalOnly)
                    {
                        continue;
                    }

                    dict[roleParamsData.ParamName] = roleParamsData;
                }
            }
        }

        /// <summary>
        /// Truy vấn tên nhân vật theo ID
        /// </summary>
        /// <param name="dbManager"></param>
        /// <param name="roleID"></param>
        public static string QueryRoleNameByRoleID(DBManager dbManager, int roleID)
        {
            /// Tên nhân vật tương ứng
            string roleName = "";

            /// Thông tin nhân vật được cache
            DBRoleInfo roleInfo = dbManager.GetDBRoleInfo(roleID);
            /// Nếu đã được Cache
            if (roleInfo != null)
            {
                roleName = roleInfo.RoleName;
            }
            /// Nếu chưa được Cache
            else
            {
                MySQLConnection conn = null;

                try
                {
                    conn = dbManager.DBConns.PopDBConnection();
                    string queryString = string.Format("SELECT rname FROM t_roles WHERE rid = {0}", roleID);

                    MySQLCommand cmd = new MySQLCommand(queryString, conn);
                    MySQLDataReader reader = cmd.ExecuteReaderEx();

                    if (reader.Read())
                    {
                        roleName = reader["rname"].ToString();
                    }

                    cmd.Dispose();
                    cmd = null;
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.TeamBattle, ex.ToString());
                }
                finally
                {
                    if (null != conn)
                    {
                        dbManager.DBConns.PushDBConnection(conn);
                    }
                }
            }

            /// Trả về kết quả
            return roleName;
        }

        public static void DBTableRow2RoleInfo_ParamsEx(DBRoleInfo dbRoleInfo, int roleId)
        {
            using (MySqlUnity conn = new MySqlUnity())
            {
                string cmdText = string.Format("select * from t_roleparams_long where rid={0};", roleId);
                DataTable dataTable = conn.ExecuteReader(cmdText).GetSchemaTable();
                if (dataTable.Rows.Count > 0)
                {
                    ConcurrentDictionary<string, RoleParamsData> dict = dbRoleInfo.RoleParamsDict;
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        DataRow dataRow = dataTable.Rows[i];
                        int idx = Convert.ToInt32(dataRow["idx"].ToString());
                        int columnCount = dataRow.ItemArray.Length;
                        for (int columnIndex = 2; columnIndex < columnCount; columnIndex++)
                        {
                            RoleParamType roleParamType = RoleParamNameInfo.GetRoleParamType(idx, columnIndex - 2);
                            if (null != roleParamType)
                            {
                                RoleParamsData roleParamsData = new RoleParamsData()
                                {
                                    ParamName = roleParamType.ParamName,
                                    ParamValue = dataRow[columnIndex].ToString(),
                                    ParamType = roleParamType,
                                };

                                dict[roleParamsData.ParamName] = roleParamsData;
                            }
                        }
                    }
                }

                cmdText = string.Format("select * from t_roleparams_char where rid={0};", roleId);
                dataTable = conn.ExecuteReader(cmdText).GetSchemaTable();
                if (dataTable.Rows.Count > 0)
                {
                    ConcurrentDictionary<string, RoleParamsData> dict = dbRoleInfo.RoleParamsDict;
                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        DataRow dataRow = dataTable.Rows[i];
                        int idx = Convert.ToInt32(dataRow["idx"].ToString());
                        int columnCount = dataRow.ItemArray.Length;
                        for (int columnIndex = 2; columnIndex < columnCount; columnIndex++)
                        {
                            RoleParamType roleParamType = RoleParamNameInfo.GetRoleParamType(idx, columnIndex - 2);
                            if (null != roleParamType)
                            {
                                RoleParamsData roleParamsData = new RoleParamsData()
                                {
                                    ParamName = roleParamType.ParamName,
                                    ParamValue = dataRow[columnIndex].ToString(),
                                    ParamType = roleParamType,
                                };

                                dict[roleParamsData.ParamName] = roleParamsData;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Datable To Role Info
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="cmd"></param>
        public static void DBTableRow2RoleInfo_OldTasks(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd)
        {
            if (cmd.Table.Rows.Count > 0)
            {
                List<OldTaskData> oldTasks = new List<OldTaskData>(cmd.Table.Rows.Count);
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    string TaskClass = cmd.Table.Rows[i]["taskclass"].ToString();

                    int taskint = 0;

                    if (TaskClass.Length > 0)
                    {
                        taskint = Convert.ToInt32(cmd.Table.Rows[i]["taskclass"].ToString());
                    }

                    oldTasks.Add(new OldTaskData()
                    {
                        TaskID = Convert.ToInt32(cmd.Table.Rows[i]["taskid"].ToString()),
                        DoCount = Convert.ToInt32(cmd.Table.Rows[i]["count"].ToString()),
                        TaskClass = taskint,
                    });
                }

                dbRoleInfo.OldTasks = oldTasks;
            }
        }

        /// <summary>
        /// Datable To Row RoleInfo
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="cmd"></param>
        public static void DBTableRow2RoleInfo_DoingTasks(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd)
        {
            if (cmd.Table.Rows.Count > 0)
            {
                dbRoleInfo.DoingTaskList = new ConcurrentDictionary<int, TaskData>();
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    TaskData task = new TaskData()
                    {
                        DbID = Convert.ToInt32(cmd.Table.Rows[i]["id"].ToString()),
                        DoingTaskID = Convert.ToInt32(cmd.Table.Rows[i]["taskid"].ToString()),
                        DoingTaskVal1 = Convert.ToInt32(cmd.Table.Rows[i]["value1"].ToString()),
                        DoingTaskVal2 = Convert.ToInt32(cmd.Table.Rows[i]["value2"].ToString()),
                        DoingTaskFocus = Convert.ToInt32(cmd.Table.Rows[i]["focus"].ToString()),
                        AddDateTime = DataHelper.ConvertToTicks(cmd.Table.Rows[i]["addtime"].ToString()),
                        TaskType = Convert.ToInt32(cmd.Table.Rows[i]["starlevel"].ToString()),
                    };
                    dbRoleInfo.DoingTaskList[task.DbID] = task;
                }
            }
        }

        /// <summary>
        /// Đọc toàn bộ GOODs CỦA NGƯỜI CHƠI LƯU RA LIST
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="cmd"></param>
        /// <param name="index"></param>
        public static void DBTableRow2RoleInfo_Goods(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd)
        {
            if (cmd.Table.Rows.Count > 0)
            {
                dbRoleInfo.GoodsDataList = new ConcurrentDictionary<int, GoodsData>();
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    string otherPramenter = cmd.Table.Rows[i]["otherpramer"].ToString();

                    byte[] Base64Decode = Convert.FromBase64String(otherPramenter);

                    Dictionary<ItemPramenter, string> _OtherParams = DataHelper.BytesToObject<Dictionary<ItemPramenter, string>>(Base64Decode, 0, Base64Decode.Length);

                    GoodsData goodsData = new GoodsData()
                    {
                        Id = Convert.ToInt32(cmd.Table.Rows[i]["Id"].ToString()),
                        GoodsID = Convert.ToInt32(cmd.Table.Rows[i]["goodsid"].ToString()),
                        Using = Convert.ToInt32(cmd.Table.Rows[i]["isusing"].ToString()),
                        Forge_level = Convert.ToInt32(cmd.Table.Rows[i]["forge_level"].ToString()),
                        Starttime = cmd.Table.Rows[i]["starttime"].ToString(),
                        Endtime = cmd.Table.Rows[i]["endtime"].ToString(),
                        Site = Convert.ToInt32(cmd.Table.Rows[i]["site"].ToString()),
                        Props = cmd.Table.Rows[i]["Props"].ToString(),
                        GCount = Convert.ToInt32(cmd.Table.Rows[i]["gcount"].ToString()),
                        Binding = Convert.ToInt32(cmd.Table.Rows[i]["binding"].ToString()),
                        BagIndex = Convert.ToInt32(cmd.Table.Rows[i]["bagindex"].ToString()),
                        Strong = Convert.ToInt32(cmd.Table.Rows[i]["strong"].ToString()),
                        Series = Convert.ToInt32(cmd.Table.Rows[i]["series"].ToString()),
                        OtherParams = _OtherParams

                        //TODO : ĐỌC NGŨ HÀNH + TIỀN GIAO BÁN LƯU RA
                    };

                    dbRoleInfo.GoodsDataList[goodsData.Id] = goodsData;
                }
            }
        }

        /// <summary>
        /// Đọc dữ liệu Pets
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="cmd"></param>
        /// <param name="index"></param>
        public static void DBTableRow2RoleInfo_Pets(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd)
        {
            if (cmd.Table.Rows.Count > 0)
            {
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    /// Tạo mới
                    PetData pet = new PetData();
                    pet.ID = Convert.ToInt32(cmd.Table.Rows[i]["id"].ToString());
                    pet.RoleID = Convert.ToInt32(cmd.Table.Rows[i]["role_id"].ToString());
                    pet.ResID = Convert.ToInt32(cmd.Table.Rows[i]["res_id"].ToString());
                    pet.Name = DataHelper.Base64Decode(cmd.Table.Rows[i]["name"].ToString());
                    pet.Level = Convert.ToInt32(cmd.Table.Rows[i]["level"].ToString());
                    pet.Exp = Convert.ToInt32(cmd.Table.Rows[i]["exp"].ToString());
                    pet.Enlightenment = Convert.ToInt32(cmd.Table.Rows[i]["enlightenment"].ToString());
                    pet.Skills = new Dictionary<int, int>();
                    pet.Equips = new Dictionary<int, int>();
                    pet.Str = Convert.ToInt32(cmd.Table.Rows[i]["str"].ToString());
                    pet.Dex = Convert.ToInt32(cmd.Table.Rows[i]["dex"].ToString());
                    pet.Sta = Convert.ToInt32(cmd.Table.Rows[i]["sta"].ToString());
                    pet.Int = Convert.ToInt32(cmd.Table.Rows[i]["ene"].ToString());
                    pet.RemainPoints = Convert.ToInt32(cmd.Table.Rows[i]["remain_points"].ToString());
                    pet.Joyful = Convert.ToInt32(cmd.Table.Rows[i]["joyful"].ToString());
                    pet.Life = Convert.ToInt32(cmd.Table.Rows[i]["life"].ToString());
                    pet.HP = Convert.ToInt32(cmd.Table.Rows[i]["hp"].ToString());

                    /// Thông tin kỹ năng
                    string skillInfoString = cmd.Table.Rows[i]["skills"].ToString();
                    /// Nếu tồn tại
                    if (!string.IsNullOrEmpty(skillInfoString))
                    {
                        /// Các trường
                        string[] skillInfos = skillInfoString.Split('|');
                        /// Duyệt danh sách các trường
                        foreach (string skillInfo in skillInfos)
                        {
                            /// Chia nhỏ
                            string[] fields = skillInfo.Split('_');
                            /// ID kỹ năng
                            int skillID = Convert.ToInt32(fields[0]);
                            /// Cấp độ
                            int level = Convert.ToInt32(fields[1]);
                            /// Thêm vào danh sách
                            pet.Skills[skillID] = level;
                        }
                    }

                    /// Thông tin trang bị
                    string equipInfoString = cmd.Table.Rows[i]["equips"].ToString();
                    /// Nếu tồn tại
                    if (!string.IsNullOrEmpty(equipInfoString))
                    {
                        /// Các trường
                        string[] equipInfos = equipInfoString.Split('|');
                        /// Duyệt danh sách các trường
                        foreach (string equipInfo in equipInfos)
                        {
                            /// Chia nhỏ
                            string[] fields = equipInfo.Split('_');
                            /// Vị trí
                            int equipPos = Convert.ToInt32(fields[0]);
                            /// ID trang bị
                            int equipID = Convert.ToInt32(fields[1]);
                            /// Thêm vào danh sách
                            pet.Equips[equipPos] = equipID;
                        }
                    }

                    /// Thêm vào danh sách
                    dbRoleInfo.PetList[pet.ID] = pet;
                }
            }
        }

        /// <summary>
        /// Đọc dữ liệu kỹ năng
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="cmd"></param>
        /// <param name="index"></param>
        public static void DBTableRow2RoleInfo_Skills(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd)
        {
            if (cmd.Table.Rows.Count > 0)
            {
                dbRoleInfo.SkillDataList = new ConcurrentDictionary<int, SkillData>();
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    SkillData skillData = new SkillData()
                    {
                        DbID = Convert.ToInt32(cmd.Table.Rows[i]["Id"].ToString()),
                        SkillID = Convert.ToInt32(cmd.Table.Rows[i]["skillid"].ToString()),
                        SkillLevel = Convert.ToInt32(cmd.Table.Rows[i]["skilllevel"].ToString()),
                        LastUsedTick = Convert.ToInt64(cmd.Table.Rows[i]["lastusedtick"].ToString()),
                        Cooldown = Convert.ToInt32(cmd.Table.Rows[i]["cooldowntick"].ToString()),
                        Exp = Convert.ToInt32(cmd.Table.Rows[i]["exp"].ToString()),
                    };
                    dbRoleInfo.SkillDataList[skillData.SkillID] = skillData;
                }
            }
        }

        /// <summary>
        /// Đọc ra danh sách các buff thoe nhân vật
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="cmd"></param>
        /// <param name="index"></param>
        public static void DBTableRow2RoleInfo_Buffers(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd)
        {
            if (cmd.Table.Rows.Count > 0)
            {
                dbRoleInfo.BufferDataList = new ConcurrentDictionary<int, BufferData>();
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    BufferData buff = new BufferData()
                    {
                        BufferID = Convert.ToInt32(cmd.Table.Rows[i]["bufferid"].ToString()),
                        StartTime = Convert.ToInt64(cmd.Table.Rows[i]["starttime"].ToString()),
                        BufferSecs = Convert.ToInt64(cmd.Table.Rows[i]["buffersecs"].ToString()),
                        CustomProperty = cmd.Table.Rows[i]["custom_property"].ToString(),
                        BufferVal = Convert.ToInt64(cmd.Table.Rows[i]["bufferval"].ToString()),
                    };
                    dbRoleInfo.BufferDataList[buff.BufferID] = buff;
                }
            }
        }

        /// <summary>
        /// Thành tích cá nhân
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="cmd"></param>
        /// <param name="index"></param>
        public static void DBTableRow2RoleInfo_RoleWelfare(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd, int RoleID)
        {
            /// Tạo mới
            dbRoleInfo.RoleWelfare = new RoleWelfare();

            /// Nếu có dữ liệu fill vào Object
            if (cmd.Table.Rows.Count > 0)
            {
                dbRoleInfo.RoleWelfare.lastdaylogin = Convert.ToInt32(cmd.Table.Rows[0]["lastdaylogin"].ToString());

                dbRoleInfo.RoleWelfare.logincontinus = Convert.ToInt32(cmd.Table.Rows[0]["logincontinus"].ToString());

                dbRoleInfo.RoleWelfare.sevenday_continus_step = Convert.ToInt32(cmd.Table.Rows[0]["sevenday_continus_step"].ToString());

                dbRoleInfo.RoleWelfare.sevenday_continus_note = cmd.Table.Rows[0]["sevenday_continus_note"].ToString();

                dbRoleInfo.RoleWelfare.sevendaylogin_note = cmd.Table.Rows[0]["sevendaylogin_note"].ToString();

                dbRoleInfo.RoleWelfare.sevendaylogin_step = cmd.Table.Rows[0]["sevendaylogin_step"].ToString();

                dbRoleInfo.RoleWelfare.createdayid = Convert.ToInt32(cmd.Table.Rows[0]["createdayid"].ToString());

                dbRoleInfo.RoleWelfare.logindayid = Convert.ToInt32(cmd.Table.Rows[0]["logindayid"].ToString());

                dbRoleInfo.RoleWelfare.loginweekid = Convert.ToInt32(cmd.Table.Rows[0]["loginweekid"].ToString());

                dbRoleInfo.RoleWelfare.online_step = cmd.Table.Rows[0]["online_step"].ToString();

                dbRoleInfo.RoleWelfare.levelup_step = cmd.Table.Rows[0]["levelup_step"].ToString();

                dbRoleInfo.RoleWelfare.monthid = Convert.ToInt32(cmd.Table.Rows[0]["monthid"].ToString());

                dbRoleInfo.RoleWelfare.checkpoint = cmd.Table.Rows[0]["checkpoint"].ToString();

                dbRoleInfo.RoleWelfare.fist_recharge_step = Convert.ToInt32(cmd.Table.Rows[0]["fist_recharge_step"].ToString());

                dbRoleInfo.RoleWelfare.totarechage_step = cmd.Table.Rows[0]["totarechage_step"].ToString();

                dbRoleInfo.RoleWelfare.totalconsume_step = cmd.Table.Rows[0]["totalconsume_step"].ToString();

                dbRoleInfo.RoleWelfare.day_rechage_step = cmd.Table.Rows[0]["day_rechage_step"].ToString();

                dbRoleInfo.RoleWelfare.RoleID = RoleID;
            }

        }

        /// <summary>
        /// Truy vấn danh sách bạn bè
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="roleID"></param>
        public static void DBTableRow2RoleInfo_Friends(DBRoleInfo dbRoleInfo, int roleID)
        {
            dbRoleInfo.FriendDataList = new List<FriendData>();
            string str = string.Format("SELECT Id, myid, otherid, relationship, friendType FROM t_friends WHERE myid = {0} OR otherid = {1}", roleID, roleID);
            GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", str), EventLevels.Important);
            MySQLConnection connection = DBManager.getInstance().DBConns.PopDBConnection();
            try
            {
                MySQLCommand command = new MySQLCommand(str, connection);
                MySQLDataReader reader = command.ExecuteReaderEx();
                while (reader.Read())
                {
                    int myID = Convert.ToInt32(reader["myid"].ToString());
                    int otherID = Convert.ToInt32(reader["otherid"].ToString());

                    /// Nếu bản thân là myid
                    if (roleID == myID)
                    {
                        dbRoleInfo.FriendDataList.Add(new FriendData()
                        {
                            DbID = Convert.ToInt32(reader["Id"].ToString()),
                            OtherRoleID = otherID,
                            Relationship = Convert.ToInt32(reader["relationship"].ToString()),
                            FriendType = Convert.ToInt32(reader["friendType"].ToString()),
                        });
                    }
                    /// Nếu bản thân là otherid
                    else
                    {
                        dbRoleInfo.FriendDataList.Add(new FriendData()
                        {
                            DbID = Convert.ToInt32(reader["Id"].ToString()),
                            OtherRoleID = myID,
                            Relationship = Convert.ToInt32(reader["relationship"].ToString()),
                            FriendType = Convert.ToInt32(reader["friendType"].ToString()),
                        });
                    }
                }
                command.Dispose();
                command = null;
            }
            finally
            {
                // trar keets noois
                DBManager.getInstance().DBConns.PushDBConnection(connection);
            }
        }

        /// <summary>
        /// Query Role By RoleName
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="strRoleName"></param>
        /// <returns></returns>
        public static int QueryRoleID_ByRolename(MySQLConnection conn, String strRoleName)
        {
            List<Tuple<int, string>> idList = QueryRoleIdList_ByRolename_IgnoreDbCmp(conn, strRoleName);

            int roleId = -1;
            if (idList != null)
            {
                var tuple = idList.Find(_t => _t.Item2 == strRoleName);
                roleId = tuple != null ? tuple.Item1 : -1;
            }

            return roleId;
        }

        /// <summary>
        /// Querry Role By RoleName
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="rolename"></param>
        /// <returns></returns>
        public static List<Tuple<int, string>> QueryRoleIdList_ByRolename_IgnoreDbCmp(MySQLConnection conn, string rolename)
        {
            List<Tuple<int, string>> resultList = new List<Tuple<int, string>>();

            string sql = string.Format("SELECT rid,rname FROM t_roles where rname='{0}'", rolename);
            GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", sql), EventLevels.Important);
            MySQLCommand cmd = new MySQLCommand(sql, conn);

            MySQLDataReader reader = cmd.ExecuteReaderEx();
            while (reader.Read())
            {
                int oneRoleId = Convert.ToInt32(reader["rid"].ToString());
                string oneRolename = reader["rname"].ToString();

                resultList.Add(new Tuple<int, string>(oneRoleId, oneRolename));
            }

            cmd.Dispose();
            cmd = null;

            return resultList;
        }

        /// <summary>
        /// Thông tin thư [5/17/2014 LiaoWei]
        /// </summary>
        /// <param name="dbRoleInfo"></param>
        /// <param name="cmd"></param>
        /// <param name="index"></param>
        public static void DBTableRow2RoleInfo_GMailInfo(DBRoleInfo dbRoleInfo, MySQLSelectCommand cmd)
        {
            if (cmd.Table.Rows.Count > 0)
            {
                dbRoleInfo.GroupMailRecordList = new List<int>();
                for (int i = 0; i < cmd.Table.Rows.Count; i++)
                {
                    dbRoleInfo.GroupMailRecordList.Add(Convert.ToInt32(cmd.Table.Rows[i]["gmailid"].ToString()));
                }
            }
        }

        /// <summary>
        /// Truy vấn thông tin nhân vật từ DB
        /// </summary>
        /// <param name="bUseIsdel">Có phải nhân vật chưa bị xóa không</param>
        public bool Query(MySQLConnection conn, int roleID, bool bUseIsdel = true)
        {
            LogManager.WriteLog(LogTypes.Info, string.Format("Load role data from DB: {0}", roleID));

            MySQLSelectCommand cmd = null;

            if (bUseIsdel)
            {
                cmd = new MySQLSelectCommand(conn,
                     new string[] { "rid", "userid", "rname", "sex", "occupation", "sub_id", "level", "pic", "money1", "money2", "experience", "pkmode", "pkvalue", "position", "regtime", "lasttime", "bagnum", "main_quick_keys", "other_quick_keys", "loginnum", "leftfightsecs", "totalonlinesecs", "antiaddictionsecs", "logofftime", "yinliang", "maintaskid", "pkpoint", "killboss", "cztaskid", "logindayid", "logindaynum", "zoneid", "guildname", "guildrank", "guildid", "guildmoney", "username", "lastmailid", "onceawardflag", "banchat", "banlogin", "isflashplayer", "admiredcount", "store_yinliang", "store_money", "ban_trade_to_ticks", "familyid", "familyname", "familyrank", "roleprestige", "quick_items", "second_password" },
                     new string[] { "t_roles" }, new object[,] { { "rid", "=", roleID }, { "isdel", "=", 0 } }, null, new string[,] { { "level", "desc" } }, true, 0, 4, false);
            }
            else
            {
                cmd = new MySQLSelectCommand(conn,
                    new string[] { "rid", "userid", "rname", "sex", "occupation", "sub_id", "level", "pic", "money1", "money2", "experience", "pkmode", "pkvalue", "position", "regtime", "lasttime", "bagnum", "main_quick_keys", "other_quick_keys", "loginnum", "leftfightsecs", "totalonlinesecs", "antiaddictionsecs", "logofftime", "yinliang", "maintaskid", "pkpoint", "killboss", "cztaskid", "logindayid", "logindaynum", "zoneid", "guildname", "guildrank", "guildid", "guildmoney", "username", "lastmailid", "onceawardflag", "banchat", "banlogin", "isflashplayer", "admiredcount", "store_yinliang", "store_money", "ban_trade_to_ticks", "familyid", "familyname", "familyrank", "roleprestige", "quick_items", "second_password" },
                     new string[] { "t_roles" }, new object[,] { { "rid", "=", roleID } }, null, new string[,] { { "level", "desc" } }, true, 0, 4, false);
            }

            if (cmd.Table.Rows.Count <= 0)
            {
                return false;
            }

            DBRoleInfo.DBTableRow2RoleInfo(this, cmd, 0);

            if (GameDBManager.Flag_Splite_RoleParams_Table == 0)
            {
                cmd = new MySQLSelectCommand(conn,
                     new string[] { "pname", "pvalue" },
                     new string[] { "t_roleparams" }, new object[,] { { "rid", "=", roleID } }, null, null);

                DBRoleInfo.DBTableRow2RoleInfo_Params(this, cmd, true);
            }
            else
            {
                cmd = new MySQLSelectCommand(conn,
                     new string[] { "pname", "pvalue" },
                     new string[] { "t_roleparams_2" }, new object[,] { { "rid", "=", roleID } }, null, null);

                DBRoleInfo.DBTableRow2RoleInfo_Params(this, cmd, false);

                DBRoleInfo.DBTableRow2RoleInfo_ParamsEx(this, roleID);
            }

            cmd = new MySQLSelectCommand(conn,
                 new string[] { "rid", "taskid", "count", "taskclass" },
                 new string[] { "t_taskslog" }, new object[,] { { "rid", "=", roleID } }, null, null);

            DBRoleInfo.DBTableRow2RoleInfo_OldTasks(this, cmd);

            cmd = new MySQLSelectCommand(conn,
                 new string[] { "Id", "rid", "taskid", "focus", "value1", "value2", "addtime", "starlevel" },
                 new string[] { "t_tasks" }, new object[,] { { "rid", "=", roleID }, { "isdel", "=", 0 } }, null, null);

            DBRoleInfo.DBTableRow2RoleInfo_DoingTasks(this, cmd);

            //Cầu hình đọc ra vật phẩm trong DB
            cmd = new MySQLSelectCommand(conn,
                     new string[] { "id", "goodsid", "isusing", "forge_level", "starttime", "endtime", "site", "Props", "gcount", "binding", "bagindex", "strong", "series", "otherpramer" },
                     new string[] { "t_goods" }, new object[,] { { "rid", "=", roleID }, { "gcount", ">", 0 } }, null, new string[,] { { "id", "asc" } });

            /// Load Ra List GoodData
            DBRoleInfo.DBTableRow2RoleInfo_Goods(this, cmd);

            DBRoleInfo.DBTableRow2RoleInfo_Friends(this, roleID);

            /// Pet
            cmd = new MySQLSelectCommand(conn,
                     new string[] { "id", "role_id", "res_id", "name", "level", "exp", "skills", "enlightenment", "equips", "str", "dex", "sta", "ene", "hp", "remain_points", "joyful", "life" },
                     new string[] { "t_pet" }, new object[,] { { "role_id", "=", roleID } }, null, null);

            DBRoleInfo.DBTableRow2RoleInfo_Pets(this, cmd);

            cmd = new MySQLSelectCommand(conn,
                     new string[] { "Id", "skillid", "skilllevel", "lastusedtick", "cooldowntick", "exp" },
                     new string[] { "t_skills" }, new object[,] { { "rid", "=", roleID } }, null, null);

            DBRoleInfo.DBTableRow2RoleInfo_Skills(this, cmd);

            cmd = new MySQLSelectCommand(conn,
                     new string[] { "bufferid", "starttime", "buffersecs", "bufferval", "custom_property" },
                     new string[] { "t_buffer" }, new object[,] { { "rid", "=", roleID } }, null, null);

            DBRoleInfo.DBTableRow2RoleInfo_Buffers(this, cmd);

            // Liên quan tới thành tích
            cmd = new MySQLSelectCommand(conn,
                     new string[] { "lastdaylogin", "logincontinus", "sevenday_continus_step", "sevenday_continus_note", "sevendaylogin_note", "sevendaylogin_step", "createdayid", "logindayid", "loginweekid", "online_step", "levelup_step", "monthid", "checkpoint", "fist_recharge_step", "totarechage_step", "totalconsume_step", "day_rechage_step" },
                     new string[] { "t_welfare" }, new object[,] { { "rid", "=", roleID } }, null, null);

            DBRoleInfo.DBTableRow2RoleInfo_RoleWelfare(this, cmd, roleID);

            cmd = new MySQLSelectCommand(conn,
                     new string[] { "roleid", "gmailid" },
                     new string[] { "t_rolegmail_record" }, new object[,] { { "roleid", "=", roleID } }, null, null);

            DBRoleInfo.DBTableRow2RoleInfo_GMailInfo(this, cmd);

            cmd = null;

            return true;
        }
    }
}