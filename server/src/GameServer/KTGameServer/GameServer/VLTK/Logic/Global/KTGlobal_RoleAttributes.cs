using GameServer.KiemThe.Core.Repute;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Entities.Player;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.StallManager;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Role Data
        /// <summary>
        /// Sử dụng sysn dữ liệu cho client
        /// Duy nhất 1 lần tại ProcessInitGameCmd
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static RoleData GetMyselfRoleData(KPlayer client)
        {
            RoleData roleData = new RoleData()
            {
                RoleID = client.RoleID,
                RoleName = client.RoleName,
                RoleSex = client.RoleSex,
                FactionID = client.m_cPlayerFaction.GetFactionId(),

                Level = client.m_Level,
                GuildID = client.GuildID,
                FamilyID = client.FamilyID,
                Money = client.Money,
                BoundMoney = client.BoundMoney,
                MaxExperience = client.m_nNextLevelExp,
                Experience = client.m_Experience,
                PKMode = client.PKMode,
                PKValue = client.PKValue,
                MapCode = client.MapCode,
                RoleDirection = client.RoleDirection,
                PosX = client.PosX,
                PosY = client.PosY,

                MaxHP = client.m_CurrentLifeMax,
                CurrentHP = client.m_CurrentLife,

                MaxMP = client.m_CurrentManaMax,
                CurrentMP = client.m_CurrentMana,

                CurrentStamina = client.m_CurrentStamina,
                MaxStamina = client.m_CurrentStaminaMax,

                TaskDataList = client.TaskDataList,
                RolePic = client.RolePic,
                BagNum = KTGlobal.MaxBagItemCount,
                GoodsDataList = client.GoodsData.FindAll(x => x.Site == 0 || x.Using >= 0),
                MainQuickBarKeys = client.MainQuickBarKeys,
                OtherQuickBarKeys = client.OtherQuickBarKeys,
                QuickItems = client.QuickItems,

                Token = client.Token,
                BoundToken = client.BoundToken,

                TeamID = client.TeamID,
                TeamLeaderRoleID = KTTeamManager.GetTeamLeader(client.TeamID) == null ? -1 : KTTeamManager.GetTeamLeader(client.TeamID).RoleID,

                SkillDataList = client.SkillDataList,

                BufferDataList = client.BufferDataList,
                ZoneID = client.ZoneID,

                GuildRank = client.GuildRank,
                FamilyRank = client.FamilyRank,

                StoreMoney = client.StoreMoney,

                MoveSpeed = client.GetCurrentRunSpeed(),
                Camp = client.Camp,

                // Fill giá trị tinh lực hoạt lực
                MakePoint = client.GetMakePoint(),
                GatherPoint = client.GetGatherPoint(),

                WorldHonor = client.WorldHonor,
                TotalValue = client.GetTotalValue(),
                Prestige = client.Prestige,

                IsRiding = client.IsRiding,

                /// Kỹ năng sống
                LifeSkills = client.GetLifeSkills(),

                /// Danh hiệu
                Title = !string.IsNullOrEmpty(client.TempTitle) ? client.TempTitle : client.Title,
                GuildTitle = string.IsNullOrEmpty(client.GuildTitle) ? client.FamilyTitle : client.GuildTitle,
                SpecialTitleID = client.SpecialTitleID,

                // Tiền bang hội có bao nhiêu
                GuildMoney = client.RoleGuildMoney,
                Repute = client.GetRepute(),

                QuestInfo = client.GetQuestInfo(),

                NPCTaskStateList = TaskManager.getInstance().GetNPCTaskState(client),

                StallName = "",

                AttackSpeed = client.GetCurrentAttackSpeed(),
                CastSpeed = client.GetCurrentCastSpeed(),

                SkillPoint = client.GetCurrentSkillPoints(),

                GMAuth = KTGMCommandManager.IsGM(client) ? 1 : 0,

                OfficeRank = client.OfficeRank,

                SelfTitles = client.RoleTitles.Keys.ToList(),
                SelfCurrentTitleID = client.CurrentRoleTitleID,
                m_wStrength = client.GetCurStrength(),
                m_wDexterity = client.GetCurDexterity(),
                m_wEnergy = client.GetCurEnergy(),
                m_wVitality = client.GetCurEnergy(),

                CurrentPetID = client.CurrentPet == null ? -1 : client.CurrentPet.RoleID,
                Pets = client.PetList,
            };

            /// Thiết lập hệ thống và Auto
            try
            {
                roleData.AutoSettings = Global.GetRoleParamsStringWithNullFromDB(client, RoleParamName.AutoSettings);
            }
            catch (Exception)
            {
                roleData.AutoSettings = "";
            }
            try
            {
                roleData.SystemSettings = Global.GetRoleParamsStringWithNullFromDB(client, RoleParamName.SystemSettings);
            }
            catch (Exception)
            {
                roleData.SystemSettings = "";
            }
            return roleData;
        }

        /// <summary>
        /// Trả về thông tin người chơi khác, phục vụ cho soi thông tin trang bị
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static RoleData GetOtherPlayerRoleData(KPlayer client)
        {
            /// Tạo mới đối tượng
            RoleData roleData = new RoleData()
            {
                RoleID = client.RoleID,
                RoleName = client.RoleName,
                RoleSex = client.RoleSex,
                FactionID = client.m_cPlayerFaction.GetFactionId(),
                Level = client.m_Level,
                RolePic = client.RolePic,
                GoodsDataList = client.GoodsData.FindAll(x => x.Using >= 0),
                ZoneID = client.ZoneID,
            };

            return roleData;
        }

        /// <summary>
        /// Danh sách trang bị cơ bản của nhân vật
        /// </summary>
        public class RoleMiniEquipID
        {
            /// <summary>
            /// ID áo
            /// </summary>
            public int ArmorID { get; set; }

            /// <summary>
            /// ID mũ
            /// </summary>
            public int HelmID { get; set; }

            /// <summary>
            /// ID vũ khí
            /// </summary>
            public int WeaponID { get; set; }

            /// <summary>
            /// Cấp cường hóa vũ khí
            /// </summary>
            public int WeaponEnhanceLevel { get; set; }

            /// <summary>
            /// Ngũ hành vũ khí
            /// </summary>
            public int WeaponSeries { get; set; }

            /// <summary>
            /// ID phi phong
            /// </summary>
            public int MantleID { get; set; }

            /// <summary>
            /// ID ngựa
            /// </summary>
            public int HorseID { get; set; }

            /// <summary>
            /// ID mặt nạ
            /// </summary>
            public int MaskID { get; set; }
        }

        /// <summary>
        /// Trả về dữ liệu trang bị nhân vật Mini, dùng trong hiển thị nhân vật khác
        /// </summary>
        /// <param name="client"></param>
        /// <returns>ID áo, ID mũ, ID vũ khí, cấp cường hóa vũ khí, ngũ hành vũ khí, ID phi phong, ID ngựa, ID mặt nạ</returns>
        public static RoleMiniEquipID GetRoleEquipDataMini(KPlayer client)
        {
            int armorID = -1, helmID = -1, weaponID = -1, weaponEnhanceLevel = 0, weaponSeries = 0, mantleID = -1, horseID = -1, maskID = -1;
            /// Danh sách vị trí cần truy vấn
            KE_EQUIP_POSITION[] listEquipPos = new KE_EQUIP_POSITION[]
            {
                KE_EQUIP_POSITION.emEQUIPPOS_BODY,
                KE_EQUIP_POSITION.emEQUIPPOS_HEAD,
                KE_EQUIP_POSITION.emEQUIPPOS_WEAPON,
                KE_EQUIP_POSITION.emEQUIPPOS_MANTLE,
                KE_EQUIP_POSITION.emEQUIPPOS_HORSE,
                KE_EQUIP_POSITION.emEQUIPPOS_MASK,
            };

            bool isRiding = false;
            List<GoodsData> equips = client.GoodsData.FindAll(x => listEquipPos.Any(y => x.Using == (int) y));

            int count = equips.Count;
            for (int i = 0; i < count; i++)
            {
                GoodsData itemGD = equips[i];
                switch (itemGD.Using)
                {
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                    {
                        armorID = itemGD.GoodsID;
                        break;
                    }
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                    {
                        helmID = itemGD.GoodsID;
                        break;
                    }
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                    {
                        weaponID = itemGD.GoodsID;
                        weaponEnhanceLevel = itemGD.Forge_level;
                        weaponSeries = itemGD.Series;
                        break;
                    }
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                    {
                        mantleID = itemGD.GoodsID;
                        break;
                    }
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE:
                    {
                        horseID = itemGD.GoodsID;
                        break;
                    }
                    case (int) KE_EQUIP_POSITION.emEQUIPPOS_MASK:
                    {
                        maskID = itemGD.GoodsID;
                        break;
                    }
                }
            }

            /// Trả về kết quả
            return new RoleMiniEquipID()
            {
                ArmorID = armorID,
                HelmID = helmID,
                WeaponID = weaponID,
                WeaponEnhanceLevel = weaponEnhanceLevel,
                WeaponSeries = weaponSeries,
                MantleID = mantleID,
                HorseID = horseID,
                MaskID = maskID,
            };
        }

        /// <summary>
        /// Chuyển dữ liệu nhân vật thành đối tượng RoleDataMini
        /// </summary>
        /// <param name="roleDataEx"></param>
        /// <param name="includeBuffData"></param>
        /// <returns></returns>
        public static RoleDataMini ClientToRoleDataMini(KPlayer client, bool includeBuffData = true)
        {
            RoleMiniEquipID miniEquip = KTGlobal.GetRoleEquipDataMini(client);
            int armorID = miniEquip.ArmorID;
            int helmID = miniEquip.HelmID;
            int weaponID = miniEquip.WeaponID;
            int weaponEnhanceLevel = miniEquip.WeaponEnhanceLevel;
            int weaponSeries = miniEquip.WeaponSeries;
            int mantleID = miniEquip.MantleID;
            int horseID = miniEquip.HorseID;
            int maskID = miniEquip.MaskID;
            bool isRiding = client.IsRiding;

            RoleDataMini roleData = new RoleDataMini()
            {
                ZoneID = client.ZoneID,
                RoleID = client.RoleID,
                RoleName = client.RoleName,
                CurrentDir = (int) client.CurrentDir,
                HP = client.m_CurrentLife,
                MaxHP = client.m_CurrentLifeMax,
                FactionID = client.m_cPlayerFaction.GetFactionId(),
                Level = client.m_Level,
                PosX = (int) client.CurrentPos.X,
                PosY = (int) client.CurrentPos.Y,
                RoleSex = client.RoleSex,
                BufferDataList = includeBuffData ? client.Buffs.ToBufferData() : null,
                MoveSpeed = client.GetCurrentRunSpeed(),
                AttackSpeed = client.GetCurrentAttackSpeed(),
                CastSpeed = client.GetCurrentCastSpeed(),

                ArmorID = armorID,
                HelmID = helmID,
                WeaponID = weaponID,
                HorseID = horseID,
                IsRiding = isRiding,
                MantleID = mantleID,
                WeaponEnhanceLevel = weaponEnhanceLevel,
                WeaponSeries = weaponSeries,
                AvartaID = client.RolePic,
                MaskID = maskID,

                MapCode = client.CurrentMapCode,
                TeamLeaderID = client.TeamLeader != null ? client.TeamLeader.RoleID : -1,
                TeamID = client.TeamID,

                PKMode = client.PKMode,
                PKValue = client.PKValue,
                Camp = client.Camp,

                StallName = StallManager.GetStallName(client.RoleID, false),

                Title = string.IsNullOrEmpty(client.TempTitle) ? client.Title : client.TempTitle,
                GuildTitle = !string.IsNullOrEmpty(client.GuildTitle) ? client.GuildTitle : client.FamilyTitle,
                SpecialTitle = client.SpecialTitleID,

                GuildID = client.GuildID,
                GuildName = client.GuildName,
                GuildRank = client.GuildRank,
                OfficeRank = client.OfficeRank,

                FamilyID = client.FamilyID,
                FamilyName = client.FamilyName,
                FamilyRank = client.FamilyRank,

                TotalValue = client.GetTotalValue(),

                SelfCurrentTitleID = client.CurrentRoleTitleID,
            };
            return roleData;
        }
        #endregion

        #region Kinh nghiệm

        /// <summary>
        /// Tổng số EXP max sẽ share cho bọn đồng đội
        /// </summary>
        public const int KD_MAX_TEAMATE_EXP_SHARE = 60;

        #endregion Kinh nghiệm

        #region Khoảng cách chống BUG tốc chạy

        /// <summary>
        /// Khoảng cách lệch tối đa cho phép Client và Server
        /// <para>Tính cả Delay packet, etc...</para>
        /// </summary>
        public const float MaxClientServerMoveDistance = 100f;

        /// <summary>
        /// Thời gian Delay packet gửi từ Client về Server chấp nhận được
        /// </summary>
        public const long MaxClientPacketDelayAllowed = 500;

        #endregion Khoảng cách chống BUG tốc chạy

        #region Tỷ thí

        /// <summary>
        /// Thời gian tỷ thí tối đa
        /// </summary>
        public const long ChallengeMaxTimeTick = 600000;

        /// <summary>
        /// Kết thúc tỷ thí phục hồi sinh, nội, thể lực % tương ứng
        /// </summary>
        public const int ChallengeFinishHPMPStaminaReplenishPercent = 20;

        #endregion Tỷ thí

        #region Hồi sinh

        /// <summary>
        /// Hồi sinh sẽ phục hồi % sinh lực
        /// </summary>
        public const int DefaultReliveHPPercent = 20;

        /// <summary>
        /// Hồi sinh sẽ phục hồi % nội lực
        /// </summary>
        public const int DefaultReliveMPPercent = 20;

        /// <summary>
        /// Hồi sinh sẽ phục hồi % thể lực
        /// </summary>
        public const int DefaultReliveStaminaPercent = 20;

        #endregion Hồi sinh

        #region Phát hiện bẫy và tàng hình

        /// <summary>
        /// Khoảng cách cấp độ tối đa để có thể tự thấy đối phương ẩn thân mà không cần trạng thái phát hiện ẩn thân
        /// </summary>
        public const int DiffLevelToSeeInvisibleState = 20;

        /// <summary>
        /// Khoảng cách về cấp độ để có thể tự phát hiện bẫy mà không cần có trạng thái phát hiện bẫy
        /// </summary>
        public const int DiffLevelToDetectTrap = 9;

        #endregion Phát hiện bẫy và tàng hình

        #region Giãn cách thay đổi trạng thái cưỡi

        /// <summary>
        /// Thời gian giãn cách thay đổi trạng thái cưỡi ngựa liên tục
        /// </summary>
        public const long TickHorseStateChange = 5000;

        #endregion Giãn cách thay đổi trạng thái cưỡi

        #region PK

        /// <summary>
        /// Thời gian miễn nhiễm sát thương của đối phương sau tỷ thí
        /// </summary>
        public const long ChallengeImmuneToEachOtherDamagesTick = 2000;

        /// <summary>
        /// Thời gian giãn cách khi chuyển trạng thái PK từ đánh nhau sang luyện công
        /// </summary>
        public const long TimeCooldownChangingPKModeToFight = 60000;

        /// <summary>
        /// Trị PK tối đa sau đó sẽ bị chuyển vào nhà lao tự động
        /// </summary>
        public const int MaxPKValueToForceSendToJail = 10;

        /// <summary>
        /// Thời gian giãn cách để xóa trạng thái tuyên chiến kể từ lần cuối tấn công
        /// </summary>
        public const long ActiveFightPKTick = 600000;

        /// <summary>
        /// ID bản đồ nhà lao
        /// </summary>
        public const int JailMapCode = 90;

        /// <summary>
        /// Tọa độ X nhà lao
        /// </summary>
        public const int JailPosX = 4585;

        /// <summary>
        /// Tọa độ Y nhà lao
        /// </summary>
        public const int JailPosY = 2462;


        public static List<PKPunish> LstPKPunish = new List<PKPunish>();

        public static PKPunish GetPkConfig(int PkValue)
        {
            var find = LstPKPunish.Where(x => x.PKValue == PkValue).FirstOrDefault();
            if (find != null)
            {
                return find;
            }
            return null;
        }

        public static void LoadPKPunish()
        {
            string Files = KTGlobal.GetDataPath("Config/KT_Setting/PKPunish.xml");

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<PKPunish>));
                LstPKPunish = serializer.Deserialize(stream) as List<PKPunish>;
            }
        }

        #endregion PK

        #region Vinh dự tài phú
        /// <summary>
        /// Trả về vinh dự tài phú tương ứng
        /// </summary>
        /// <param name="ValuInput"></param>
        /// <returns></returns>
        public static int GetRankHonor(long ValuInput)
        {
            int FinalRank = (int) (ValuInput / 10000);

            if (FinalRank >= 300 && FinalRank < 600)
            {
                return 1;
            }
            else if (FinalRank >= 600 && FinalRank < 1000)
            {
                return 2;
            }
            else if (FinalRank >= 1000 && FinalRank < 2000)
            {
                return 3;
            }
            else if (FinalRank >= 2000 && FinalRank < 4000)
            {
                return 4;
            }
            else if (FinalRank >= 4000 && FinalRank < 7500)
            {
                return 5;
            }
            else if (FinalRank >= 7500 && FinalRank < 15000)
            {
                return 6;
            }
            else if (FinalRank >= 15000 && FinalRank < 38000)
            {
                return 7;
            }
            else if (FinalRank >= 38000 && FinalRank < 75000)
            {
                return 8;
            }
            else if (FinalRank >= 75000 && FinalRank < 180000)
            {
                return 9;
            }
            else if (FinalRank >= 180000)
            {
                return 10;
            }

            return 0;
        }
        #endregion

        #region Danh vọng
        /// <summary>
        /// Thêm danh vọng tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="PointType"></param>
        /// <param name="experience"></param>
        public static void AddRepute(KPlayer player, int PointType, long experience)
        {
            var FindPoint = player.GetRepute().Where(x => x.DBID == PointType).FirstOrDefault();

            if (FindPoint != null)
            {
                int LevelCur = FindPoint.Level;

                int _Class = FindPoint.Class;

                int _Camp = FindPoint.Camp;

                Camp _CampFind = ReputeManager._ReputeConfig.Camp.Where(x => x.Id == _Camp).FirstOrDefault();
                if (_CampFind != null)
                {
                    Class _ClassFind = _CampFind.Class.Where(x => x.Id == _Class).FirstOrDefault();
                    if (_ClassFind != null)
                    {
                        var FindLevel = _ClassFind.Level.Where(x => x.Id == LevelCur).FirstOrDefault();

                        if (FindLevel != null)
                        {
                            // get ra max level của danh vọng này
                            int MaxLevel = _ClassFind.Level.Max(x => x.Id);
                            int nNeedExp = FindLevel.LevelUp;

                            if (FindPoint.Level < MaxLevel && FindPoint.Exp + experience >= nNeedExp)
                            {
                                experience = (FindPoint.Exp + experience) - nNeedExp;

                                FindPoint.Exp = 0;
                                FindPoint.Level = FindPoint.Level + 1;

                                AddRepute(player, PointType, experience);
                            }
                            else
                            {
                                if (FindPoint.Level < MaxLevel)
                                {
                                    FindPoint.Exp += (int) experience;
                                }
                                else
                                {
                                    FindPoint.Exp = 0;
                                }
                            }

                            if (player.IsOnline())
                            {
                                ReputeInfo _Info = new ReputeInfo();
                                _Info.DBID = PointType;
                                _Info.Exp = FindPoint.Exp;
                                _Info.Level = FindPoint.Level;

                                player.SendPacket<ReputeInfo>((int) TCPGameServerCmds.CMD_KT_UPDATE_REPUTE, _Info);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thiết lập giá trị danh vọng tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="PointType"></param>
        /// <param name="Level"></param>
        /// <param name="experience"></param>
        public static void SetRepute(KPlayer player, int PointType, int Level, long experience)
        {
            var FindPoint = player.GetRepute().Where(x => x.DBID == PointType).FirstOrDefault();

            if (FindPoint != null)
            {
                FindPoint.Level = Level;
                FindPoint.Exp = (int) experience;

                if (player.IsOnline())
                {
                    ReputeInfo _Info = new ReputeInfo();
                    _Info.DBID = PointType;
                    _Info.Exp = FindPoint.Exp;
                    _Info.Level = FindPoint.Level;

                    player.SendPacket<ReputeInfo>((int) TCPGameServerCmds.CMD_KT_UPDATE_REPUTE, _Info);
                }
            }
        }
        #endregion
    }
}
