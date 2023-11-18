using GameServer.Logic;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Thiết lập cơ bản của người chơi
    /// </summary>
    public class KPlayerSetting
    {
        /// <summary>
        /// Thuộc tính cơ bản cộng thêm khi thăng cấp
        /// </summary>
        public class KLEVEL_SETTING
        {
            /// <summary>
            /// Cấp độ
            /// </summary>
            public int nLevel { get; set; }

            /// <summary>
            /// Kinh nghiệm thăng cấp
            /// </summary>
            public long nExp { get; set; }

            /// <summary>
            /// Điểm tiềm năng
            /// </summary>
            public int dwPotential { get; set; }

            /// <summary>
            /// Sức
            /// </summary>
            public int dwStrength { get; set; }

            /// <summary>
            /// Thân
            /// </summary>
            public int dwDexterity { get; set; }

            /// <summary>
            /// Ngoại
            /// </summary>
            public int dwVitality { get; set; }

            /// <summary>
            /// Nội
            /// </summary>
            public int dwEnergy { get; set; }

            /// <summary>
            /// Điểm kỹ năng
            /// </summary>
            public int dwFightSkillPoint { get; set; }

            /// <summary>
            /// Sinh lực
            /// </summary>
            public int nLife { get; set; }

            /// <summary>
            /// Nội lực
            /// </summary>
            public int nMana { get; set; }

            /// <summary>
            /// Thể lực
            /// </summary>
            public int nStamina { get; set; }

            /// <summary>
            /// May mắn
            /// </summary>
            public int nLuck { get; set; }

            /// <summary>
            /// Hoạt lực
            /// </summary>
            public int dwGatherPoint { get; set; }

            /// <summary>
            /// Tinh lực
            /// </summary>
            public int dwMakePoint { get; set; }

            /// <summary>
            /// Tốc độ đi bộ
            /// </summary>
            public int nSpeedWalk { get; set; }

            /// <summary>
            /// Tốc độ chạy
            /// </summary>
            public int nSpeedRun { get; set; }

            /// <summary>
            /// % tốc độ di chuyển
            /// </summary>
            public int nMoveSpeedAddP { get; set; }

            /// <summary>
            /// Tốc độ xuất chiêu hệ ngoại công
            /// </summary>
            public int nAttackSpeed { get; set; }

            /// <summary>
            /// Tốc độ xuất chiêu hệ nội công
            /// </summary>
            public int nCastSpeed { get; set; }

            /// <summary>
            /// Chính xác
            /// </summary>
            public int nAttackRate { get; set; }

            /// <summary>
            /// Vật công ngoại
            /// </summary>
            public int nDamagePhysics { get; set; }

            /// <summary>
            /// Vật công nội
            /// </summary>
            public int nDamageMagic { get; set; }

            /// <summary>
            /// Né tránh
            /// </summary>
            public int nDefence { get; set; }

            /// <summary>
            /// Sát thương cơ bản của Nộ khí
            /// </summary>
            public int nBaseAngerATK { get; set; }

            /// <summary>
            /// Kháng ngũ hành
            /// </summary>
            public float[] fResist { get; set; } = new float[(int)KE_SERIES_TYPE.series_num];

            /// <summary>
            /// Cường hóa ngũ hành tương khắc
            /// </summary>
            public float fResistConquer { get; set; }

            /// <summary>
            /// Nhược hóa ngũ hành tương khắc
            /// </summary>
            public float fResistBeConquer { get; set; }

            /// <summary>
            /// Kinh nghiệm khi nhận thưởng
            /// </summary>
            public int nBaseAwardExp { get; set; }

            /// <summary>
            /// Hiệu suất phục hồi tinh hoạt lực
            /// </summary>
            public int nProductivity { get; set; }

            /// <summary>
            /// Số tiền có thể mang theo tối đa
            /// </summary>
            public int nMaxCarryMoney { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XML Node
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static KLEVEL_SETTING Parse(XElement xmlNode)
            {
                return new KLEVEL_SETTING()
                {
                    nLevel = int.Parse(xmlNode.Attribute("Level").Value),
                    nExp = long.Parse(xmlNode.Attribute("ExpNeed").Value),
                    dwPotential = int.Parse(xmlNode.Attribute("RemainPoint").Value),
                    dwStrength = int.Parse(xmlNode.Attribute("Str").Value),
                    dwDexterity = int.Parse(xmlNode.Attribute("Dex").Value),
                    dwVitality = int.Parse(xmlNode.Attribute("Sta").Value),
                    dwEnergy = int.Parse(xmlNode.Attribute("Int").Value),
                    dwFightSkillPoint = int.Parse(xmlNode.Attribute("SkillRemainPoint").Value),
                    nLife = int.Parse(xmlNode.Attribute("HP").Value),
                    nMana = int.Parse(xmlNode.Attribute("MP").Value),
                    nStamina = int.Parse(xmlNode.Attribute("Vitality").Value),
                    nLuck = int.Parse(xmlNode.Attribute("Luck").Value),
                    dwGatherPoint = int.Parse(xmlNode.Attribute("Energy").Value),
                    dwMakePoint = int.Parse(xmlNode.Attribute("Potential").Value),
                    nSpeedWalk = int.Parse(xmlNode.Attribute("SpeedWalk").Value),
                    nSpeedRun = int.Parse(xmlNode.Attribute("SpeedRun").Value),
                    nAttackSpeed = int.Parse(xmlNode.Attribute("PAtkSpeed").Value),
                    nCastSpeed = int.Parse(xmlNode.Attribute("MAtkSpeed").Value),
                    nAttackRate = int.Parse(xmlNode.Attribute("Hit").Value),
                    nDefence = int.Parse(xmlNode.Attribute("Dodge").Value),
                    nDamagePhysics = int.Parse(xmlNode.Attribute("PAtk").Value),
                    nDamageMagic = int.Parse(xmlNode.Attribute("MAtk").Value),
                    fResist = new float[] {
                        float.Parse(xmlNode.Attribute("Def").Value),
                        float.Parse(xmlNode.Attribute("Def").Value),
                        float.Parse(xmlNode.Attribute("PoisonRes").Value),
                        float.Parse(xmlNode.Attribute("IceRes").Value),
                        float.Parse(xmlNode.Attribute("FireRes").Value),
                        float.Parse(xmlNode.Attribute("LightningRes").Value),
                    },
                    nBaseAwardExp = int.Parse(xmlNode.Attribute("BaseExp").Value),
                    nMoveSpeedAddP = int.Parse(xmlNode.Attribute("MoveSpeedPercent").Value),
                    nBaseAngerATK = int.Parse(xmlNode.Attribute("Rage").Value),
                    nMaxCarryMoney = int.Parse(xmlNode.Attribute("MaxCarryGold").Value),
                };
            }
        }

        /// <summary>
        /// Chỉ số cộng theo nhánh môn phái
        /// </summary>
        protected class KROUTE_SETTING
        {
            /// <summary>
            /// ID phái
            /// </summary>
            public int nFactionID { get; set; }

            /// <summary>
            /// Sức tương đương Vật công ngoại
            /// </summary>
            public float fStrength2DamagePhysics { get; set; }

            /// <summary>
            /// Thân tương đương Chính xác
            /// </summary>
            public float fDexterity2AttackRate { get; set; }

            /// <summary>
            /// Thân tương đương Né tránh
            /// </summary>
            public float fDexterity2Defence { get; set; }

            /// <summary>
            /// Thân tương đương Vật công ngoại
            /// </summary>
            public float fDexterity2DamagePhysics { get; set; }

            /// <summary>
            /// Ngoại tương đương Sinh lực tối đa
            /// </summary>
            public float fVitality2Life { get; set; }

            /// <summary>
            /// Nội tương đương Vật công nội
            /// </summary>
            public float fEnergy2Mana { get; set; }

            /// <summary>
            /// Nội tương đương Nội lực tối đa
            /// </summary>
            public float fEnergy2DamageMagic { get; set; }

            /// <summary>
            /// Sức cộng được mỗi cấp
            /// </summary>
            public float fStrengthPerLevel { get; set; }

            /// <summary>
            /// Thân cộng được mỗi cấp
            /// </summary>
            public float fDexterityPerLevel { get; set; }

            /// <summary>
            /// Ngoại cộng được mỗi cấp
            /// </summary>
            public float fVitalityPerLevel { get; set; }

            /// <summary>
            /// Nội cộng được mỗi cấp
            /// </summary>
            public float fEnergyPerLevel { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XML Node
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static KROUTE_SETTING Parse(XElement xmlNode)
            {
                return new KROUTE_SETTING()
                {
                    nFactionID = int.Parse(xmlNode.Attribute("FactionID").Value),
                    fStrength2DamagePhysics = int.Parse(xmlNode.Attribute("StrToPAtk").Value),
                    fDexterity2AttackRate = int.Parse(xmlNode.Attribute("DexToHit").Value),
                    fDexterity2Defence = int.Parse(xmlNode.Attribute("DexToDodge").Value),
                    fDexterity2DamagePhysics = int.Parse(xmlNode.Attribute("DexToPAtk").Value),
                    fVitality2Life = int.Parse(xmlNode.Attribute("StaToHP").Value),
                    fEnergy2Mana = int.Parse(xmlNode.Attribute("IntToMP").Value),
                    fEnergy2DamageMagic = int.Parse(xmlNode.Attribute("IntToMAtk").Value),
                    fStrengthPerLevel = int.Parse(xmlNode.Attribute("StrPerLevel").Value),
                    fDexterityPerLevel = int.Parse(xmlNode.Attribute("DexPerLevel").Value),
                    fVitalityPerLevel = int.Parse(xmlNode.Attribute("StaPerLevel").Value),
                    fEnergyPerLevel = int.Parse(xmlNode.Attribute("IntPerLevel").Value),
                };
            }
        }

        /// <summary>
        /// Danh sách thuộc tính cơ bản theo cấp
        /// </summary>
        protected static List<KLEVEL_SETTING> s_vLevelSetting = new List<KLEVEL_SETTING>();

        /// <summary>
        /// Danh sách chỉ số cộng theo nhánh môn phái
        /// </summary>
        protected static List<KROUTE_SETTING> s_mpRouteSetting = new List<KROUTE_SETTING>();

        /// <summary>
        /// Cấp độ tối đa
        /// </summary>
        protected static int ms_nMaxPlayerLevel;

        /// <summary>
        /// Chấp nhận nhân vật cấp cao Login
        /// </summary>
        protected static bool ms_bAllowHighLevelLogin;

        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Init()
        {
            KPlayerSetting.s_vLevelSetting.Clear();
            KPlayerSetting.s_mpRouteSetting.Clear();

            {
                XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Setting/RoleLevelUpAttributes.xml");
                foreach (XElement node in xmlNode.Elements())
                {
                    KLEVEL_SETTING levelUpAttributes = KLEVEL_SETTING.Parse(node);
                    KPlayerSetting.s_vLevelSetting.Add(levelUpAttributes);
                }
            }

            {
                XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Faction/FactionAttributes.xml");
                foreach (XElement node in xmlNode.Elements())
                {
                    KROUTE_SETTING factionAttributes = KROUTE_SETTING.Parse(node);
                    KPlayerSetting.s_mpRouteSetting.Add(factionAttributes);
                }
            }

            KPlayerSetting.ms_nMaxPlayerLevel = KPlayerSetting.s_vLevelSetting.Count;
        }

        /// <summary>
        /// Cấp độ tối đa của nhân vật
        /// </summary>
        /// <returns></returns>
        public static int GetMaxPlayerLevel()
        {
            return KPlayerSetting.ms_nMaxPlayerLevel;
        }

        /// <summary>
        /// Chấp nhận người chơi cấp cao đăng nhập
        /// </summary>
        /// <returns></returns>
        public static bool GetAllowHighLevelLogin()
        {
            return ms_bAllowHighLevelLogin;
        }

        /// <summary>
        /// Trả về tổng giá trị Tiềm năng có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelPotential(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.dwPotential);
        }

        /// <summary>
        /// Trả về tổng giá trị Kinh nghiệm có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static long GetLevelExp(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nExp);
        }

        /// <summary>
        /// Trả về tổng giá trị Sức có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelStrength(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.dwStrength);
        }

        /// <summary>
        /// Trả về tổng giá trị Thân có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelDexterity(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.dwDexterity);
        }

        /// <summary>
        /// Trả về tổng giá trị Ngoại có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelVitality(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.dwVitality);
        }

        /// <summary>
        /// Trả về tổng giá trị Nội có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelEnergy(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.dwEnergy);
        }

        /// <summary>
        /// Trả về tổng giá trị Sức tối đa có thể cộng đến cấp hiện tại
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetLevelMaxStrength(KPlayer player)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == player.m_cPlayerFaction.GetFactionId()).FirstOrDefault();
            if (routeSetting != null)
            {
                return player.m_Level * (int) routeSetting.fStrengthPerLevel;
            }
            return 0;
        }

        /// <summary>
        /// Trả về tổng giá trị Thân tối đa có thể cộng đến cấp hiện tại
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetLevelMaxDexterity(KPlayer player)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == player.m_cPlayerFaction.GetFactionId()).FirstOrDefault();
            if (routeSetting != null)
            {
                return player.m_Level * (int) routeSetting.fDexterityPerLevel;
            }
            return 0;
        }

        /// <summary>
        /// Trả về tổng giá trị Ngoại tối đa có thể cộng đến cấp hiện tại
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetLevelMaxVitality(KPlayer player)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == player.m_cPlayerFaction.GetFactionId()).FirstOrDefault();
            if (routeSetting != null)
            {
                return player.m_Level * (int) routeSetting.fVitalityPerLevel;
            }
            return 0;
        }

        /// <summary>
        /// Trả về tổng giá trị Nội tối đa có thể cộng đến cấp hiện tại
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetLevelMaxEnergy(KPlayer player)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == player.m_cPlayerFaction.GetFactionId()).FirstOrDefault();
            if (routeSetting != null)
            {
                return player.m_Level * (int) routeSetting.fEnergyPerLevel;
            }
            return 0;
        }

        /// <summary>
        /// Trả về tổng giá trị Điểm kỹ năng có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelFightSkillPoint(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.dwFightSkillPoint);
        }

        /// <summary>
        /// Trả về tổng giá trị Sinh lực tối đa có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelLife(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nLife);
        }

        /// <summary>
        /// Trả về tổng giá trị Nội lực tối đa có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelMana(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nMana);
        }

        /// <summary>
        /// Trả về tổng giá trị Thể lực tối đa có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelStamina(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nStamina);
        }

        /// <summary>
        /// Trả về tổng giá trị May mắn có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelLuck(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nLuck);
        }

        /// <summary>
        /// Trả về tổng giá trị Hoạt lực có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelGatherPoint(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.dwGatherPoint);
        }

        /// <summary>
        /// Trả về tổng giá trị Tinh lực có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelMakePoint(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.dwMakePoint);
        }

        /// <summary>
        /// Trả về tổng giá trị Tốc độ đi bộ có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelSpeedWalk(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nSpeedWalk);
        }

        /// <summary>
        /// Trả về tổng giá trị Tốc chạy có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelSpeedRun(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nSpeedRun);
        }

        /// <summary>
        /// Trả về tổng giá trị % Tốc chạy có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelMoveSpeedAddP(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nMoveSpeedAddP);
        }

        /// <summary>
        /// Trả về tổng giá trị Tốc độ xuất chiêu hệ ngoại công có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelAttackSpeed(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nAttackSpeed);
        }


        public static int GetBaseExpLevel(int PlayerLevel)
        {
            var find = KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel == PlayerLevel).FirstOrDefault();
            if(find!=null)
            {
                return find.nBaseAwardExp;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Trả về tổng giá trị Tốc độ xuất chiêu hệ nội công có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelCastSpeed(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nCastSpeed);
        }

        /// <summary>
        /// Trả về tổng giá trị Chính xác có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelAttackRate(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nAttackRate);
        }

        /// <summary>
        /// Trả về tổng giá trị Vật công ngoại có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelDamagePhysics(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nDamagePhysics);
        }

        /// <summary>
        /// Trả về tổng giá trị Vật công nội có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelDamageMagic(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nDamageMagic);
        }

        /// <summary>
        /// Trả về tổng giá trị Né tránh có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelDefence(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nDefence);
        }

        /// <summary>
        /// Trả về tổng giá trị Sát thương cơ bản của Nộ khí có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelBaseAngerATK(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nBaseAngerATK);
        }

        /// <summary>
        /// Trả về tổng giá trị Số bạc có thể mang theo tối đa có được tới cấp hiện tại
        /// </summary>
        /// <param name="nPlayerLevel"></param>
        /// <returns></returns>
        public static int GetLevelMaxCarryMoney(int nPlayerLevel)
        {
            return KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel <= nPlayerLevel).Sum(x => x.nMaxCarryMoney);
        }

        /// <summary>
        /// Trả về giá trị điểm kháng theo cấp độ
        /// </summary>
        /// <param name="eResistSeries"></param>
        /// <param name="nPlayerLevel"></param>
        /// <param name="ePlayerSeries"></param>
        /// <returns></returns>
        public static int GetLevelResist(KE_SERIES_TYPE eResistSeries, int nPlayerLevel, KE_SERIES_TYPE ePlayerSeries)
        {
            if (nPlayerLevel < 0 || (int)nPlayerLevel >= KPlayerSetting.s_vLevelSetting.Count || eResistSeries < KE_SERIES_TYPE.series_none || eResistSeries >= KE_SERIES_TYPE.series_num || ePlayerSeries < KE_SERIES_TYPE.series_none || ePlayerSeries >= KE_SERIES_TYPE.series_num)
            {
                return 0;
            }

            KLEVEL_SETTING levelSetting = KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel == nPlayerLevel).FirstOrDefault();
            if (levelSetting == null)
            {
                return 0;
            }

           // System.Console.WriteLine("eResistSeries = " + eResistSeries + " - Array Length = " + levelSetting.fResist.Length);
            float fResult = levelSetting.fResist[(int)eResistSeries];
            return (int)fResult;
        }

        /// <summary>
        /// Lấy giá trị Sức chuyển qua vật công ngoại
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="nStrength"></param>
        /// <returns></returns>
        public static int GetStrength2DamagePhysics(int byFactionId, int nStrength)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == byFactionId).FirstOrDefault();
            if (routeSetting == null)
            {
                return 0;
            }

            return (int)(routeSetting.fStrength2DamagePhysics * nStrength);
        }

        /// <summary>
        /// Lấy giá trị Thân chuyển qua chính xác
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="nDexterity"></param>
        /// <returns></returns>
        public static int GetDexterity2AttackRate(int byFactionId, int nDexterity)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == byFactionId).FirstOrDefault();
            if (routeSetting == null)
            {
                return 0;
            }

            return (int)(routeSetting.fDexterity2AttackRate * nDexterity);
        }

        /// <summary>
        /// Lấy giá trị Thân chuyển qua né tránh
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="nDexterity"></param>
        /// <returns></returns>
        public static int GetDexterity2Defence(int byFactionId, int nDexterity)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == byFactionId).FirstOrDefault();
            if (routeSetting == null)
            {
                return 0;
            }

            return (int)(routeSetting.fDexterity2Defence * nDexterity);
        }

        /// <summary>
        /// Lấy giá trị Thân chuyển qua vật công ngoại
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="nDexterity"></param>
        /// <returns></returns>
        public static int GetDexterity2DamagePhysics(int byFactionId, int nDexterity)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == byFactionId).FirstOrDefault();
            if (routeSetting == null)
            {
                return 0;
            }

            return (int)(routeSetting.fDexterity2DamagePhysics * nDexterity);
        }

        /// <summary>
        /// Lấy giá trị Ngoại chuyển qua Sinh lực tối đa
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="nVitality"></param>
        /// <returns></returns>
        public static int GetVitality2Life(int byFactionId, int nVitality)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == byFactionId).FirstOrDefault();
            if (routeSetting == null)
            {
                return 0;
            }

            return (int)(routeSetting.fVitality2Life * nVitality);
        }

        /// <summary>
        /// Lấy giá trị Nội chuyển qua Nội lực tối đa
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="nEnergy"></param>
        /// <returns></returns>
        public static int GetEnergy2Mana(int byFactionId, int nEnergy)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == byFactionId).FirstOrDefault();
            if (routeSetting == null)
            {
                return 0;
            }

            return (int)(routeSetting.fEnergy2Mana * nEnergy);
        }

        /// <summary>
        /// Lấy giá trị Nội chuyển qua vật công nội
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <param name="nEnergy"></param>
        /// <returns></returns>
        public static int GetEnergy2DamageMagic(int byFactionId, int nEnergy)
        {
            KROUTE_SETTING routeSetting = KPlayerSetting.s_mpRouteSetting.Where(x => x.nFactionID == byFactionId).FirstOrDefault();
            if (routeSetting == null)
            {
                return 0;
            }

            return (int)(routeSetting.fEnergy2DamageMagic * nEnergy);
        }


        /// <summary>
        /// Hàm trả về level settings
        /// </summary>
        /// <param name="Level"></param>
        /// <returns></returns>
        public static KLEVEL_SETTING GetLevelSetting(int Level)
        {
            KLEVEL_SETTING levelfind = KPlayerSetting.s_vLevelSetting.Where(x => x.nLevel == Level).FirstOrDefault();
           

            if(levelfind!=null)
            {
                return levelfind;
            }
            else
            {
                return null;
            }
        }

        public static long GetNeedExpUpExp(int Level)
        {
           var find = KPlayerSetting.s_vLevelSetting.Where(x=>x.nLevel == Level).FirstOrDefault();

            if(find!=null)
            {
                return find.nExp;
            }
            else
            {
                return 0;
            }
            
        }

        public static int GetMaxLevel()
        {
            int MAX = KPlayerSetting.s_vLevelSetting.Max(x => x.nLevel);

            return MAX;
        }

        /// <summary>
        /// Thiết lập cấp độ tối đa
        /// </summary>
        /// <param name="nMaxLevel"></param>
        /// <returns></returns>
        public static bool SetMaxPlayerLevel(int nMaxLevel)
        {
            if (nMaxLevel < 1 || nMaxLevel > s_vLevelSetting.Count)
            {
                return false;
            }
            KPlayerSetting.ms_nMaxPlayerLevel = nMaxLevel;

            return true;
        }

        /// <summary>
        /// Thiết lập chấp nhận người chơi cấp cao hơn giới hạn đăng nhập
        /// </summary>
        /// <param name="bAllow"></param>
        /// <returns></returns>
        public bool SetAllowHighLevelLogin(bool bAllow)
        {
            KPlayerSetting.ms_bAllowHighLevelLogin = bAllow;
            return true;
        }
    }
}