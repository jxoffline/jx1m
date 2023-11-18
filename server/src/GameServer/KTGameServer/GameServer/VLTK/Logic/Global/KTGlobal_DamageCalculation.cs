using GameServer.KiemThe.Entities;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region PKRegular

        public const int PKDamageRate = 25;

        public const int NpcPKDamageRate = 750;

        public const int EnmityExpLimitPercent = -50;

        public const int FightExpLimitPercent = -50;

        public const int PKStateChangeLimitTime = 180;

        #endregion PKRegular

        #region Algorithm Properties
        /// <summary>
        /// Danh sách cường hóa sát thương theo vũ khí tương ứng
        /// </summary>
        public static readonly int[] m_arPhysicsEnhance = new int[(int) KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_NUM];

        /// <summary>
        /// Tỉ lệ chính xác nhỏ nhất
        /// </summary>
        public const int HitPercentMin = 5;

        /// <summary>
        /// TỈ lệ chính xác tối đa
        /// </summary>
        public const int HitPercentMax = 95;

        /// <summary>
        /// Tỉ lệ dính phải trạng thái base
        /// </summary>
        public const int StateBaseRateParam = 250;

        /// <summary>
        /// Thời gian tồn tại trạng thái base
        /// </summary>
        public const int StateBaseTimeParam = 250;

        /// <summary>
        /// Tỉ lệ dính phải trạng thái base
        /// </summary>
        public const int DeadlyStrikeBaseRate = 2000;

        /// <summary>
        /// Thời gian tồn tại trạng thái base
        /// </summary>
        public const int DeadlyStrikeDamagePercent = 180;

        /// <summary>
        /// Bỏ qua kháng max
        /// </summary>
        public const int DefenceMaxPercent = 85;

        /// <summary>
        /// Bỏ qua kháng min
        /// </summary>
        public const int IngoreResistMaxP = 95;

        /// <summary>
        /// Max ngũ hành tương khắc damage
        /// </summary>
        public const int SeriesTrimMax = 95;

        /// <summary>
        /// Chí mạng % damage thêm vào tối đa
        /// </summary>
        public const int FatallyStrikePercent = 50; 

        public const int SeriesTrimParam1 = 1;
        public const int SeriesTrimParam2 = 8;
        public const int SeriesTrimParam3 = 200;
        public const int SeriesTrimParam4 = 0;
        public const int SeriesTrimParam5 = 10;

        /// <summary>
        /// Tỉ lệ thiệt hại khi bị suy yếu
        /// </summary>
        public const int WeakDamagePercent = 50;

        /// <summary>
        /// Tốc dộ sẽ làm chậm
        /// </summary>
        public const int SlowAllPercent = 50;

        /// <summary>
        /// Dame bị bỏng tính thêm
        /// </summary>
        public const int BurnDamagePercent = 150;

        /// <summary>
        /// Thời gian nộ tính bằng giây
        /// </summary>
        public const int AngerTime = 10;
        #endregion

        #region Ngũ hành tương khắc
        /// <summary>
        /// Tương sinh
        /// </summary>
        public static readonly int[] g_nAccrueSeries = new int[(int) KE_SERIES_TYPE.series_num];

        /// <summary>
        /// Tương khắc
        /// </summary>
        public static readonly int[] g_nConquerSeries = new int[(int) KE_SERIES_TYPE.series_num];

        /// <summary>
        /// Được sinh
        /// </summary>
        public static readonly int[] g_nAccruedSeries = new int[(int) KE_SERIES_TYPE.series_num];

        /// <summary>
        /// Bị khắc
        /// </summary>
        public static readonly int[] g_nConqueredSeries = new int[(int) KE_SERIES_TYPE.series_num];

        /// <summary>
        /// Lấy ra vật hệ tương sinh của ngũ hành nhập vào
        /// </summary>
        /// <param name="nDesSeries"></param>
        /// <returns></returns>
        public static KE_SERIES_TYPE AccrueSeriesSourceRequest(int nDesSeries)
        {
            return (KE_SERIES_TYPE) KTGlobal.g_nAccrueSeries[nDesSeries];
        }

        /// <summary>
        /// Cập nhật dữ liệu ngũ hành tương khắc
        /// </summary>
        public static void LoadAccrueSeries()
        {
            KTGlobal.g_nAccrueSeries[(int) KE_SERIES_TYPE.series_none] = (int) KE_SERIES_TYPE.series_none;
            KTGlobal.g_nConquerSeries[(int) KE_SERIES_TYPE.series_none] = (int) KE_SERIES_TYPE.series_none;
            KTGlobal.g_nAccruedSeries[(int) KE_SERIES_TYPE.series_none] = (int) KE_SERIES_TYPE.series_none;
            KTGlobal.g_nConqueredSeries[(int) KE_SERIES_TYPE.series_none] = (int) KE_SERIES_TYPE.series_none;
            KTGlobal.g_nAccrueSeries[(int) KE_SERIES_TYPE.series_metal] = (int) KE_SERIES_TYPE.series_water;
            KTGlobal.g_nConquerSeries[(int) KE_SERIES_TYPE.series_metal] = (int) KE_SERIES_TYPE.series_wood;
            KTGlobal.g_nAccruedSeries[(int) KE_SERIES_TYPE.series_metal] = (int) KE_SERIES_TYPE.series_earth;
            KTGlobal.g_nConqueredSeries[(int) KE_SERIES_TYPE.series_metal] = (int) KE_SERIES_TYPE.series_fire;
            KTGlobal.g_nAccrueSeries[(int) KE_SERIES_TYPE.series_wood] = (int) KE_SERIES_TYPE.series_fire;
            KTGlobal.g_nConquerSeries[(int) KE_SERIES_TYPE.series_wood] = (int) KE_SERIES_TYPE.series_earth;
            KTGlobal.g_nAccruedSeries[(int) KE_SERIES_TYPE.series_wood] = (int) KE_SERIES_TYPE.series_water;
            KTGlobal.g_nConqueredSeries[(int) KE_SERIES_TYPE.series_wood] = (int) KE_SERIES_TYPE.series_metal;
            KTGlobal.g_nAccrueSeries[(int) KE_SERIES_TYPE.series_water] = (int) KE_SERIES_TYPE.series_wood;
            KTGlobal.g_nConquerSeries[(int) KE_SERIES_TYPE.series_water] = (int) KE_SERIES_TYPE.series_fire;
            KTGlobal.g_nAccruedSeries[(int) KE_SERIES_TYPE.series_water] = (int) KE_SERIES_TYPE.series_metal;
            KTGlobal.g_nConqueredSeries[(int) KE_SERIES_TYPE.series_water] = (int) KE_SERIES_TYPE.series_earth;
            KTGlobal.g_nAccrueSeries[(int) KE_SERIES_TYPE.series_fire] = (int) KE_SERIES_TYPE.series_earth;
            KTGlobal.g_nConquerSeries[(int) KE_SERIES_TYPE.series_fire] = (int) KE_SERIES_TYPE.series_metal;
            KTGlobal.g_nAccruedSeries[(int) KE_SERIES_TYPE.series_fire] = (int) KE_SERIES_TYPE.series_wood;
            KTGlobal.g_nConqueredSeries[(int) KE_SERIES_TYPE.series_fire] = (int) KE_SERIES_TYPE.series_water;
            KTGlobal.g_nAccrueSeries[(int) KE_SERIES_TYPE.series_earth] = (int) KE_SERIES_TYPE.series_metal;
            KTGlobal.g_nConquerSeries[(int) KE_SERIES_TYPE.series_earth] = (int) KE_SERIES_TYPE.series_water;
            KTGlobal.g_nAccruedSeries[(int) KE_SERIES_TYPE.series_earth] = (int) KE_SERIES_TYPE.series_fire;
            KTGlobal.g_nConqueredSeries[(int) KE_SERIES_TYPE.series_earth] = (int) KE_SERIES_TYPE.series_wood;
        }

        /// <summary>
        /// Kiểm tra 2 ngũ hành tương ứng có tương sinh không
        /// </summary>
        /// <param name="nSrcSeries"></param>
        /// <param name="nDesSeries"></param>
        /// <returns></returns>
        public static bool g_IsAccrue(int nSrcSeries, int nDesSeries)
        {
            return KTGlobal.g_InternalIsAccrueConquer(KTGlobal.g_nAccrueSeries, nSrcSeries, nDesSeries);
        }

        /// <summary>
        /// Kiểm tra 2 ngũ hành tương ứng có tương khắc không
        /// </summary>
        /// <param name="nSrcSeries"></param>
        /// <param name="nDesSeries"></param>
        /// <returns></returns>
        public static bool g_IsConquer(int nSrcSeries, int nDesSeries)
        {
            return KTGlobal.g_InternalIsAccrueConquer(KTGlobal.g_nConquerSeries, nSrcSeries, nDesSeries);
        }

        /// <summary>
        /// Kiểm tra tính tương sinh hay tương khắc của ngũ hành tương ứng
        /// </summary>
        /// <param name="pAccrueConquerTable"></param>
        /// <param name="nSrcSeries"></param>
        /// <param name="nDesSeries"></param>
        /// <returns></returns>
        private static bool g_InternalIsAccrueConquer(int[] pAccrueConquerTable, int nSrcSeries, int nDesSeries)
        {
            if (nSrcSeries < (int) KE_SERIES_TYPE.series_none || nSrcSeries >= (int) KE_SERIES_TYPE.series_num)
            {
                return false;
            }

            return nDesSeries == pAccrueConquerTable[nSrcSeries];
        }
        #endregion

        #region Loại kỹ năng
        /// <summary>
        /// Trả về ID loại kỹ năng tương ứng
        /// </summary>
        /// <param name="skillStyle"></param>
        /// <returns></returns>
        public static int GetSkillStyleDef(string skillStyle)
        {
            switch (skillStyle)
            {
                case "meleephysicalattack":
                    return 1;

                case "rangephysicalattack":
                    return 2;

                case "rangemagicattack":
                    return 3;

                case "aurarangemagicattack":
                    return 4;

                case "defendcurse":
                    return 5;

                case "attackcurse":
                    return 6;

                case "fightcurse":
                    return 7;

                case "curse":
                    return 8;

                case "trap":
                    return 9;

                case "initiativeattackassistant":
                    return 10;

                case "initiativedefendassistant":
                    return 11;

                case "initiativefightassistant":
                    return 12;

                case "stolenassistant":
                    return 13;

                case "initiativeattackassistantally":
                    return 14;

                case "initiativedefendassistantally":
                    return 15;

                case "initiativefightassistantally":
                    return 16;

                case "specialattack":
                    return 17;

                case "disable":
                    return 18;

                case "assistant":
                    return 19;

                case "nonpcskill":
                    return 20;
            }
            return -1;
        }
        #endregion

        #region Tốc chạy và tốc đánh

        #region Tốc chạy

        /// <summary>
        /// Chuyển tốc độ di chuyển sang dạng lưới Pixel
        /// </summary>
        /// <param name="moveSpeed"></param>
        /// <returns></returns>
        public static int MoveSpeedToPixel(int moveSpeed)
        {
            return moveSpeed * 15;
        }

        #endregion Tốc chạy

        #region Tốc đánh

        /// <summary>
        /// Kiểm tra đối tượng đã kết thúc thực thi động tác xuất chiêu chưa
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="attackSpeed"></param>
        /// <returns></returns>
        public static bool FinishedUseSkillAction(GameServer.Logic.GameObject obj, int attackSpeed)
        {
            /// Tổng thời gian
            float frameDuration = KTGlobal.AttackSpeedToFrameDuration(attackSpeed);
            /// Tổng thời gian đã qua
            long tick = KTGlobal.GetCurrentTimeMilis() - obj.LastAttackTicks;
            /// Tổng thời gian yêu cầu
            long requireTick = (long) (frameDuration * 1000);

            //if (tick < requireTick)
            //{
            //    LogManager.WriteLog(LogTypes.RolePosition, string.Format("Toác VKL => Tick = {0}, Require Tick = {1}", tick, requireTick));
            //}

            /// Trả ra kết quả
            return tick >= requireTick;
        }

        /// <summary>
        /// Thời gian thực hiện động tác xuất chiêu tối thiểu
        /// </summary>
        public const float MinAttackActionDuration = 0.2f;

        /// <summary>
        /// Thời gian thực hiện động tác xuất chiêu tối đa
        /// </summary>
        public const float MaxAttackActionDuration = 0.8f;

        /// <summary>
        /// Thời gian cộng thêm giãn cách giữa các lần ra chiêu
        /// </summary>
        public const float AttackSpeedAdditionDuration = 0.1f;

        /// <summary>
        /// Tốc đánh tối thiểu
        /// </summary>
        public const int MinAttackSpeed = 0;

        /// <summary>
        /// Tốc đánh tối đa
        /// </summary>
        public const int MaxAttackSpeed = 100;

        /// <summary>
        /// Chuyển tốc độ đánh sang thời gian thực hiện động tác xuất chiêu
        /// </summary>
        /// <param name="attackSpeed"></param>
        /// <returns></returns>
        public static float AttackSpeedToFrameDuration(int attackSpeed)
        {
            /// Nếu tốc đánh nhỏ hơn tốc tối thiểu
            if (attackSpeed < KTGlobal.MinAttackSpeed)
            {
                attackSpeed = KTGlobal.MinAttackSpeed;
            }
            /// Nếu tốc đánh vượt quá tốc tối đa
            if (attackSpeed > KTGlobal.MaxAttackSpeed)
            {
                attackSpeed = KTGlobal.MaxAttackSpeed;
            }

            /// Tỷ lệ % so với tốc đánh tối đa
            float percent = attackSpeed / (float) KTGlobal.MaxAttackSpeed;

            /// Thời gian thực hiện động tác xuất chiêu
            float animationDuration = KTGlobal.MinAttackActionDuration + (KTGlobal.MaxAttackActionDuration - KTGlobal.MinAttackActionDuration) * (1f - percent);

            /// Trả về kết quả
            return animationDuration;
        }

        #endregion Tốc đánh

        #endregion Tốc chạy và tốc đánh
    }
}
