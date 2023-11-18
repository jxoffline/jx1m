using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace GameServer.VLTK.Core.GuildManager
{
    /// <summary>
    /// Quản lý bang hội mapping với gameDB
    /// </summary>
    public partial class GuildManager
    {
        public static int ITEM_REQUEST_CREATE_GUILD = 13220;

        private static GuildManager instance = new GuildManager();

        /// <summary>

        /// </summary>
        /// <returns></returns>
        public static GuildManager getInstance()
        {
            return instance;
        }

        public static string GUILD_CONFIG_FILE = "Config/KT_Guild/GuildConfig.xml";

        //Dict chứa toàn bộ thông tin cơ bản của bang hội
        //Thông tin về kỹ năng để tham chiếu skill
        public static ConcurrentDictionary<int, MiniGuildInfo> _TotalGuild = new ConcurrentDictionary<int, MiniGuildInfo>();

        /// <summary>
        /// Toàn bộ kỹ năng của bang hội
        /// </summary>
        public static GuildDef _GuildConfig = new GuildDef();

        /// <summary>
        /// Load toàn bộ TMP của kỹ năng bang
        /// </summary>
        public static void LoadGuildSkill()
        {
            string Files = KTGlobal.GetDataPath(GUILD_CONFIG_FILE);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(GuildDef));

                _GuildConfig = serializer.Deserialize(stream) as GuildDef;
            }
        }

        /// <summary>
        /// Kiểm tra xem guild có tồn tại kho
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public bool CheckGuildExist(int GuildID)
        {
            if (_TotalGuild.ContainsKey(GuildID))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy toàn bộ thông tin bang từ gamedatabase
        /// Gọi sau khi lấy game config
        /// Đảm bảo là game database đã khởi động xong
        /// </summary>
        public bool GetAllGuildInfo(int ZoneID = GameManager.LocalServerId)
        {
            byte[] bytesData = null;
            if (TCPProcessCmdResults.RESULT_FAILED == Global.RequestToDBServer3(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool, (int)TCPGameServerCmds.CMD_KT_GUILD_GETMINI_INFO, string.Format("{0}:{1}", ZoneID, 1), out bytesData, ZoneID))
            {
                return false;
            }

            if (null == bytesData || bytesData.Length <= 6)
            {
                return false;
            }

            Int32 length = BitConverter.ToInt32(bytesData, 0);

            //Đọc ra danh sách bang hội mà gamedb trả về
            ConcurrentDictionary<int, MiniGuildInfo> MiniGuildInfo = DataHelper.BytesToObject<ConcurrentDictionary<int, MiniGuildInfo>>(bytesData, 6, length - 2);

            if (MiniGuildInfo.Count > 0)
            {
                //lấy ra toàn bộ dữ liệu bang hội trả ra từ gamedb
                foreach (MiniGuildInfo Values in MiniGuildInfo.Values)
                {
                    // Nếu dict lưu trữ bang mà không chứa bagn này thì add bang này vào list
                    if (!_TotalGuild.ContainsKey(Values.GuildId))
                    {
                        //Nếu như bang này chưa tồn tại trong danh sách thì thêm vào
                        _TotalGuild.TryAdd(Values.GuildId, Values);
                    }
                }
            }

            // Rebuild lại task cho các bang hội
            foreach (int GuildID in _TotalGuild.Keys)
            {
                MiniGuildInfo _GUIDINFO = _TotalGuild[GuildID];

                if (_GUIDINFO != null)
                {
                    // Thực hiện reload lại kỹ năng cho bang này
                    _GUIDINFO.IntKMagicAttribs();
                    ReloadTaskOfGuild(GuildID);
                }
            }

            return true;
        }

        public void ReloadAllTaskNextDay()
        {
            // Rebuild lại task cho các bang hội
            foreach (int GuildID in _TotalGuild.Keys)
            {
                ReloadTaskOfGuild(GuildID);
            }
        }

        /// <summary>
        /// Attack thuộc tính bang hội vào 1 thằng nào đó
        /// </summary>
        /// <param name="client"></param>
        public static void AttackEffectGuildSkill(KPlayer client)
        {
            //MiniGuildInfo _Info = _TotalGuild.Where(x => x.GuildId == client.GuildID).FirstOrDefault();
            _TotalGuild.TryGetValue(client.GuildID, out MiniGuildInfo _Info);

            if (_Info != null)
            {
                List<KMagicAttrib> TotalSkillGuildEffect = _Info.SkillProbsKMagicAttribs;

                if (TotalSkillGuildEffect != null)
                {
                    if (TotalSkillGuildEffect.Count > 0)
                    {
                        LogManager.WriteLog(LogTypes.Guild, "ATTACK GUILD SKILL :" + TotalSkillGuildEffect.Count);

                        foreach (var VARIABLE in TotalSkillGuildEffect)
                        {
                            // Attack all thuộc tính của bang hội vào người chơi
                            KTAttributesModifier.AttachProperty(VARIABLE, client, false);
                        }
                    }
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Guild, "["+client.RoleID+"]["+client.RoleName+"] EFFECT NULL WHEN ATTACK :" + _Info.SkillNote);

                  
                    // Nếu như kỹ năng của bọn này mà có độ dài lớn hơn 0 thì thực hiện INIT lại cho cái bang này
                    if (_Info.SkillNote.Length > 0)
                    {                      
                        _Info.IntKMagicAttribs();
                    }
                }
            }
        }

        /// <summary>
        /// Hủy kích hoạt kỹ năng bang cho người chơi
        /// </summary>
        /// <param name="GuildID"></param>
        public static void DetackAllPlayerInGuild(int GuildID)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Info);
            // MiniGuildInfo _Info = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
            if (_Info != null)
            {
                // Lấy ra atribute hiện tại
                List<KMagicAttrib> TotalSkillGuildEffect = _Info.SkillProbsKMagicAttribs;
                if (TotalSkillGuildEffect != null)
                {
                    int index = 0;

                    if (TotalSkillGuildEffect.Count > 0)
                    {
                        /// Danh sách thành viên bang
                        List<KPlayer> guildMembers = KTPlayerManager.FindAll(x => x.GuildID == GuildID);
                        foreach (KPlayer gc in guildMembers)
                        {
                            foreach (var VARIABLE in TotalSkillGuildEffect)
                            {
                                // Detack all thuộc tính của bang hội khỏi người chơi
                                KTAttributesModifier.AttachProperty(VARIABLE, gc, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Kích hoạt toàn bộ kỹ năng bang cho người chơi
        /// </summary>
        /// <param name="GuildID"></param>
        public static void AttackAllPlayerInGuild(int GuildID)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Info);
            // MiniGuildInfo _Info = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
            if (_Info != null)
            {
                // Lấy ra atribute hiện tại
                List<KMagicAttrib> TotalSkillGuildEffect = _Info.SkillProbsKMagicAttribs;
                int index = 0;
                if (TotalSkillGuildEffect.Count > 0)
                {
                    List<KPlayer> guildMembers = KTPlayerManager.FindAll(x => x.GuildID == GuildID);
                    foreach (KPlayer gc in guildMembers)
                    {
                        if (gc.GuildID == GuildID)
                        {
                            foreach (var VARIABLE in TotalSkillGuildEffect)
                            {
                                // Detack all thuộc tính của bang hội khỏi người chơi
                                KTAttributesModifier.AttachProperty(VARIABLE, gc, false);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Trả về list kỹ năng của bang hội
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public List<SkillDef> GetTotalSkillDef(int GuildID)
        {
            List<SkillDef> _TotalSkill = new List<SkillDef>();

            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Info);
            // MiniGuildInfo _Info = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
            if (_Info != null)
            {
                string SkillNote = _Info.SkillNote;

                _TotalSkill = SkillDecode(SkillNote);
            }

            return _TotalSkill;
        }

        /// <summary>
        /// Lấy ra số lượt đi phụ bản của bang trong tuần
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public static int GetTotal_Copy_Scenes_This_Week(int GuildID)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Info);
            if (_Info != null)
            {
                return _Info.Total_Copy_Scenes_This_Week;
            }

            return -1;
        }

        /// <summary>
        /// Set số lượt đi phụ bản của bang
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="Num"></param>
        /// <returns></returns>
        public static bool SetTotal_Copy_Scenes_This_Week(int GuildID, int Num)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Info);
            if (_Info != null)
            {
                _Info.Total_Copy_Scenes_This_Week = Num;

                if (UpdateGuildResource(GuildID, GUILD_RESOURCE.JOINEVENT, Num + ""))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Funtion xử lý việc thăng cấp kỹ năng của bang hội
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="SkillID"></param>
        public static int LevelUpSkill(int GuildID, int SkillID)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Info);

            // MiniGuildInfo _Info = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
            if (_Info != null)
            {
                // Check xem kỹ năng này có trong tmp không
                var FindTmp = _GuildConfig.Skills.Where(x => x.ID == SkillID).FirstOrDefault();
                if (FindTmp != null)
                {
                    string SkillResource = GetGuildResource(GuildID, GUILD_RESOURCE.SKILL);

                    if (SkillResource == "-1")
                    {
                        return -100;
                    }

                    //Giải mã toàn bộ kỹ năng ra
                    List<SkillDef> TotalSkillDef = SkillDecode(SkillResource);

                    // Tìm xem chỗ kỹ năng vừa giãi mã có kỹ năng cần nâng cấp không
                    var FindData = TotalSkillDef.Where(x => x.SkillID == SkillID).FirstOrDefault();

                    // Nếu đã có kỹ năng này rồi
                    //  if (FindData != null)
                    {
                        int CurLevel = 0;

                        // Nếu như thằng này đã học kỹ năng này rồi thì lấy cấp độ trước đó
                        if (FindData != null)
                        {
                            CurLevel = FindData.Level;
                        }

                        if (CurLevel >= 10)
                        {
                            // Skill đã đạt cấp độ tối đa
                            return -1;
                        }

                        int NextLevel = CurLevel + 1;

                        if (_Info.GuilLevel < NextLevel)
                        {
                            return -200;
                        }

                        //TODO : Kiểm tra tài nguyên bang xem có đủ không để thực hiện
                        var LevelData = FindTmp.LevelData.Where(x => x.Level == NextLevel).FirstOrDefault();

                        if (LevelData != null)
                        {
                            string MoneyResource = GetGuildResource(GuildID, GUILD_RESOURCE.GUILD_MONEY);
                            if (MoneyResource == "-1")
                            {
                                return -100;
                            }
                            // Số bang cống yêu cầu để nâng cấp kỹ năng
                            int PointRequest = LevelData.PointUpdate;

                            int GuildMoney = Int32.Parse(MoneyResource);

                            if (GuildMoney < PointRequest)
                            {
                                // Thiếu tiền
                                return -2;
                            }

                            string ItemResource = GetGuildResource(GuildID, GUILD_RESOURCE.ITEM);
                            if (ItemResource == "-1")
                            {
                                return -100;
                            }
                            //Lấy ra số vật phẩm cần để nâng cấp kỹ năng
                            List<ItemRequest> TotalItemRequest = LevelData.ItemRequest;

                            // Giả mã đống kỹ năng kỹ năng
                            List<ItemRequest> TotalItemHave = ItemDecode(ItemResource);

                            // Check xem có đủ vật phẩm yêu cầu không
                            foreach (var _Item in TotalItemRequest)
                            {
                                int ITEMID = _Item.ItemID;
                                int ItemNum = _Item.ItemNum;

                                var FindResource = TotalItemHave.Where(x => x.ItemID == ITEMID).FirstOrDefault();
                                if (FindResource != null)
                                {
                                    if (FindResource.ItemNum < ItemNum)
                                    {
                                        return -3;
                                    }
                                }
                                else
                                {
                                    return -3;
                                }
                            }

                            // Nếu tất cả đều đủ thực hiện sub value và thực hiện update kỹ năng
                            // DETACK VÀ ATTACK LẠI TOÀN BỘ THUỘC TÍNH CHO AE TRONG BANG HỘI
                            int MoneyGuildLess = GuildMoney - PointRequest;

                            if (!UpdateGuildResource(GuildID, GUILD_RESOURCE.GUILD_MONEY, MoneyGuildLess + ""))
                            {
                                // Nếu có lỗi trong quá trình trừ tiền thì cho toác
                                return -4;
                            }

                            // Gán lại tiền cho bang hội
                            _Info.GuildMoney = MoneyGuildLess;
                            // Vòng for thứ 2 để trừ vật phẩm đi
                            foreach (var _Item in TotalItemRequest)
                            {
                                int ITEMID = _Item.ItemID;
                                int ItemNum = _Item.ItemNum;

                                var Find = TotalItemHave.Where(x => x.ItemID == ITEMID).FirstOrDefault();
                                int Less = Find.ItemNum - ItemNum;
                                //Thực hiện set lại cho item
                                Find.ItemNum = Less;
                            }
                            //TODO  : CHECK LẠI UPDATE TRÊN LINQ XEM HOẠT ĐỘNG ỔN KHÔNG | F9 TOTALITEMHAVE

                            string ItemEncode = GuildManager.ItemEncode(TotalItemHave);
                            if (!UpdateGuildResource(GuildID, GUILD_RESOURCE.ITEM, ItemEncode))
                            {
                                // Nếu có lỗi trong quá trình trừ tiền thì cho toác
                                return -4;
                            }

                            //Gán lại tiem cho bang hội
                            _Info.ItemStore = ItemEncode;
                            // Thực hiện nâng cấp kỹ năng cho nó
                            if (FindData != null)
                            {
                                FindData.Level = NextLevel;
                            }
                            else
                            {
                                // Thêm mới 1 kỹ năng
                                SkillDef _Skill = new SkillDef();
                                _Skill.Level = NextLevel;
                                _Skill.SkillID = SkillID;
                                TotalSkillDef.Add(_Skill);
                            }
                            // Mã hóa chỗ kỹ năng của nó rồi đút vào gameDb
                            string SkillEncode = GuildManager.SkillEncode(TotalSkillDef);
                            if (!UpdateGuildResource(GuildID, GUILD_RESOURCE.SKILL, SkillEncode))
                            {
                                // Nếu có lỗi trong quá trình trừ tiền thì cho toác
                                return -4;
                            }

                            string Notify = "Kỹ năng <color=red>[" + FindTmp.Name + "]</color> đã được tăng lên cấp <color=red>" + NextLevel + "</color>";
                            KTGlobal.SendGuildChat(GuildID, Notify, null, "");

                            // Thực hiện detack all hiệu ứng của cấp cu trước đó
                            DetackAllPlayerInGuild(GuildID);
                            // Set lại cho bang và attack hiệu ứng mới
                            _Info.SkillNote = SkillEncode;
                            _Info.IntKMagicAttribs();
                            // Attack lại chỉ số cho toàn bộ thành viên trong bang hội
                            AttackAllPlayerInGuild(GuildID);
                            // Thực hiện ATTACK VÀ DETACK LẠI TOÀN BỘ THUỘC TÍNH CŨ CỦA BANG

                            LogManager.WriteLog(LogTypes.Guild, "[SKILLUPDATE][" + GuildID + "] Update Skill ID : " + SkillID + " | OLDLEVEL :" + CurLevel + " | NEXTLEVEL :" + NextLevel);
                            // Return lại kết quả thực hiện thành công
                            return 1;
                        }
                    }
                }
            }

            return -100;
        }

        public List<MiniGuildInfo> GetTotalGuildInfo()
        {
            return _TotalGuild.Values.ToList();
        }

        public MiniGuildInfo _GetInfoGuildByGuildID(int GuildID)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Info);
            // MiniGuildInfo _Info = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
            if (_Info != null)
            {
                return _Info;
            }
            else
            {
                return null;
            }
        }

        public MiniGuildInfo _GetInfoGuildByGuildName(string GuildName)
        {
            var _Info = _TotalGuild.Values.Where(x => x.GuildName == GuildName).FirstOrDefault();

            // MiniGuildInfo _Info = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
            if (_Info != null)
            {
                return _Info;
            }
            else
            {
                return null;
            }
        }

        public MiniGuildInfo GetGuildWinHostCity()
        {
            // Lấy ra thằng thành chủ hiện tại
            var Find = _TotalGuild.Values.Where(x => x.IsMainCity == (int)GUILD_CITY_STATUS.HOSTCITY).FirstOrDefault();
            return Find;
        }

        /// <summary>
        /// Set điểm hoạt động tuần của thằng này
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Point"></param>
        public void SetWeekPoint(KPlayer client, int Point)
        {
            client.SetValueOfWeekRecore((int)WeekRecord.GuildWeekPoint, Point);
        }

        /// <summary>
        /// Lấy ra số điểm hoạt động tuần của nhân vật
        /// </summary>
        /// <param name="client"></param>
        /// <param name="Point"></param>
        public int GetWeekPoint(KPlayer client)
        {
            int WeekPoint = client.GetValueOfWeekRecore((int)WeekRecord.GuildWeekPoint);
            if (WeekPoint == -1)
            {
                WeekPoint = 0;
            }
            return WeekPoint;
        }

        /// <summary>
        /// Trả về thông tin bang hội kèm tích lũy của người chơi
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public GuildInfo GetGuildInfoByGuildID(int GuildID, KPlayer client)
        {
            GuildInfo guildInfo = null;

            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Info);

            if (_Info != null)
            {
                guildInfo = new GuildInfo();

                int GuildMoney = client.RoleGuildMoney;

                int WeekPoint = GetWeekPoint(client);

                guildInfo._GuildInfo = _Info;

                guildInfo.WeekPoint = WeekPoint;

                guildInfo.MoneyGuild = GuildMoney;

                var date = DateTime.Now;

                var nextSunday = date.AddDays(7 - (int)date.DayOfWeek);

                string Add = "<color=red>Lợi Tức Tuần:</color> " + KTGlobal.GetDisplayMoney(guildInfo._GuildInfo.MoneyBound) + "\n<color=red>Trả Lợi Tức :</color> " + nextSunday.ToShortDateString() + "\n<color=yellow>===================================</color>\n<color=green>Thông Báo Bang :</color>\n|";

                if (!_Info.GuildNotify.Contains("Lợi Tức Tuần"))
                {
                    guildInfo._GuildInfo.GuildNotify = Add + _Info.GuildNotify;
                }
            }

            return guildInfo;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public static int GuildLevelUp(int GuildID, KPlayer client)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo _Guild);

            //var _Guild = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();

            if (_Guild != null)
            {
                // Lấy ra số exp hiện tại
                int ExpHave = _Guild.GuildExp;

                int CurLevel = _Guild.GuilLevel;

                int NextLevel = CurLevel + 1;

                // Nếu vượt quá số cấp mà bang hội có
                if (NextLevel > _GuildConfig.LevelUps.Count)
                {
                    return -1;
                }

                var FindLevelNext = _GuildConfig.LevelUps.Where(x => x.Level == NextLevel).FirstOrDefault();

                if (FindLevelNext != null)
                {
                    // Lấy ra số exp yêu cầu
                    int ExpRequest = FindLevelNext.ExpRequest;

                    if (ExpHave < ExpRequest)
                    {
                        return -2;
                    }

                    //Lấy ra số vật phẩm cần để nâng cấp kỹ năng
                    List<ItemRequest> TotalItemRequest = FindLevelNext.ItemRequest;

                    string BuildAsk = "Để thăng cấp bang hội lên cấp <color=red>" + NextLevel + "</color> sẽ tiêu tốn :\n";

                    foreach (ItemRequest _AskTmp in TotalItemRequest)
                    {
                        ItemData _Item = ItemManager.GetItemTemplate(_AskTmp.ItemID);
                        if (_Item != null)
                        {
                            BuildAsk += "<color=green>" + _Item.Name + "</color> Số Lượng :" + "<color=red>" + _AskTmp.ItemNum + "</color>\n";
                        }
                    }

                    int PointRequest = FindLevelNext.PointUpdate;

                    BuildAsk += "Bang cống yêu cầu :<color=red>" + PointRequest + "</color>Điểm bạn có chắc chắn muốn thăng cấp bang?";

                    KTPlayerManager.ShowMessageBox(client, "Thông báo", BuildAsk, () =>
                    {
                        //Giải mã đống item hiện cso
                        List<ItemRequest> TotalItemHave = ItemDecode(_Guild.ItemStore);

                        // Check xem có đủ vật phẩm yêu cầu không
                        foreach (var _Item in TotalItemRequest)
                        {
                            int ITEMID = _Item.ItemID;
                            int ItemNum = _Item.ItemNum;

                            var FindResource = TotalItemHave.Where(x => x.ItemID == ITEMID).FirstOrDefault();
                            if (FindResource != null)
                            {
                                if (FindResource.ItemNum < ItemNum)
                                {
                                    KTPlayerManager.ShowMessageBox(client, "Thông báo", "Nguyên liệu trong bang cống không đủ");
                                    return;
                                }
                            }
                            else
                            {
                                KTPlayerManager.ShowMessageBox(client, "Thông báo", "Nguyên liệu trong bang cống không đủ");
                                return;
                            }
                        }

                        //Tiền bang hiện có

                        if (_Guild.GuildMoney < PointRequest)
                        {
                            KTPlayerManager.ShowMessageBox(client, "Thông báo", "Tiền trong bang cống không đủ để nâng cấp");
                            return;
                        }

                        // Nếu tất cả đều hợp lý thì thực hiện thăng cấp bang và trừ tài nguyên

                        int ExpLess = ExpHave - ExpRequest;
                        //Set lại EXP cho bang
                        _Guild.GuildExp = ExpLess;

                        int PointLess = _Guild.GuildMoney - PointRequest;
                        //Set Lại tiền cho bang
                        _Guild.GuildMoney = PointLess;

                        foreach (var _Item in TotalItemRequest)
                        {
                            int ITEMID = _Item.ItemID;
                            int ItemNum = _Item.ItemNum;

                            var FindResource = TotalItemHave.Where(x => x.ItemID == ITEMID).FirstOrDefault();
                            if (FindResource != null)
                            {
                                FindResource.ItemNum = (FindResource.ItemNum - ItemNum);
                            }
                        }

                        string Notify = "Cấp bang hội đã được tăng lên cấp <color=red>" + NextLevel + "</color>";
                        KTGlobal.SendGuildChat(GuildID, Notify, null, "");

                        string ItemEncode = GuildManager.ItemEncode(TotalItemHave);
                        //Set lại vật phẩm cho bang
                        _Guild.ItemStore = ItemEncode;

                        _Guild.GuilLevel = NextLevel;
                        //Update EXP vào DB
                        UpdateGuildResource(GuildID, GUILD_RESOURCE.EXP, _Guild.GuildExp + "");
                        //UPDATE TIỀN CỦA BANG
                        UpdateGuildResource(GuildID, GUILD_RESOURCE.GUILD_MONEY, _Guild.GuildMoney + "");
                        //UPDATE VẬT PHẨM
                        UpdateGuildResource(GuildID, GUILD_RESOURCE.ITEM, _Guild.ItemStore);
                        //UPDATE LEVEL
                        UpdateGuildResource(GuildID, GUILD_RESOURCE.LEVEL, _Guild.GuilLevel + "");

                        GuildInfo _GuildInfo = GuildManager.getInstance().GetGuildInfoByGuildID(client.GuildID, client);

                        client.SendPacket<GuildInfo>((int)TCPGameServerCmds.CMD_KT_GUILD_GETINFO, _GuildInfo);

                        KTPlayerManager.ShowMessageBox(client, "Thông báo", "Thăng cấp bang thành công!");
                        return;
                    }, true);
                }
            }

            return -10;
        }

        /// <summary>
        /// Update tiền cho thằng chủ thành
        /// </summary>
        /// <param name="Money"></param>
        public void AddBoundMoneyForHostCity(int Money)
        {
            var findhost = _TotalGuild.Values.Where(x => x.IsMainCity == 1).FirstOrDefault();
            if (findhost != null)
            {
                KTGlobal.UpdateGuildBoundMoney(Money, findhost.GuildId);
            }
        }

        /// <summary>
        /// Update tình trạng công ty
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="City"></param>
        /// <returns></returns>
        public bool UpdateCityStatus(int GuildID, int City)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo FindGuild);
            if (FindGuild != null)
            {
                // Set trạng thái thành chủ cho bang naỳ
                FindGuild.IsMainCity = City;

                // Update resource cho bang này
                return UpdateGuildResource(GuildID, GUILD_RESOURCE.CITYSTATUS, City + "");
            }
            return false;
        }

        /// <summary>
        /// Thao tác với quest
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="Cmd"></param>
        /// <returns></returns>
        public int GuildQuestCmd(int GuildID, int Cmd)
        {
            _TotalGuild.TryGetValue(GuildID, out MiniGuildInfo FindGuild);

            if (FindGuild != null)
            {
                if (FindGuild.Task != null)
                {
                    if (FindGuild.Task.TaskCountInDay >= 20)
                    {
                        return -10;
                    }
                }
                else
                {
                    return -1;
                }
                // Nếu là 0 tức là đổi nhiệm vụ tiêu tốn bang cống
                if (Cmd == 0)
                {
                    int CostNeed = _GuildConfig.ChangeQuestCost;

                    // var FindGuild = _TotalGuild.Where(x => x.GuildId == GuildID).FirstOrDefault();
                    if (FindGuild != null)
                    {
                        int MoneyHave = FindGuild.GuildMoney;

                        if (MoneyHave < CostNeed)
                        {
                            return -2;
                        }

                        int MoneyLess = FindGuild.GuildMoney - CostNeed;

                        FindGuild.GuildMoney = MoneyLess;

                        // Cập nhật tiền vào bang xong cho nó cái quest khác
                        UpdateGuildResource(GuildID, GUILD_RESOURCE.GUILD_MONEY, FindGuild.GuildMoney + "");

                        GiveTaskForGuild(GuildID, false);
                    }

                    return 1;
                }
                else if (Cmd == 1)
                {
                    GiveTaskForGuild(GuildID, true);
                    return 2;
                }
            }

            return -1;
        }
    }
}