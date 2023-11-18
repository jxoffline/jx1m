using GameServer.KiemThe.Entities;
using GameServer.VLTK.Core.Activity.X2ExpEvent;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Các hàm hỗ trợ
        /// </summary>
        private static class MonsterUtilities
        {
            #region Máu

            /// <summary>
            /// Trả về sinh lực tối đa theo cấp độ quái
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            public static int GetMaxHPByLevel(int level)
            {
                // Nếu như đây là chế độ test
                if (KTGlobal.IsTestModel)
                {
                    return 1;
                }

                /// Trả về giá trị trong khoảng tương ứng cấp độ
                int GetData(int nLevel, int param1, int param2, float multiply)
                {
                    return (int)(multiply * (param2 * nLevel + param1));
                }

                /// Dưới cấp 10
                if (level <= 10)
                {
                    int param1 = 50;
                    int param2 = 3;
                    return GetData(level, param1, param2, 1f);
                }
                /// Từ cấp 11 đến cấp 20
                else if (level > 10 && level <= 20)
                {
                    int param1 = 122;
                    int param2 = 8;
                    return GetData(level - 10, param1, param2, 1f);
                }
                /// Từ cấp 21 đến cấp 30
                else if (level > 20 && level <= 30)
                {
                    int param1 = 220;
                    int param2 = 30;
                    return GetData(level - 20, param1, param2, 1f);
                }
                /// Từ cấp 31 đến cấp 40
                else if (level > 30 && level <= 40)
                {
                    int param1 = 900;
                    int param2 = 35;
                    return GetData(level - 30, param1, param2, 1f);
                }
                /// Từ cấp 41 đến cấp 50
                else if (level > 40 && level <= 50)
                {
                    int param1 = 1450;
                    int param2 = 60;
                    return GetData(level - 40, param1, param2, 1f);
                }
                /// Từ cấp 51 đến cấp 60
                else if (level > 50 && level <= 60)
                {
                    int param1 = 2100;
                    int param2 = 60;
                    return GetData(level - 50, param1, param2, 1.1f);
                }
                /// Từ cấp 61 đến cấp 70
                else if (level > 60 && level <= 70)
                {
                    int param1 = 5900;
                    int param2 = 60;
                    return GetData(level - 60, param1, param2, 1.1f);
                }
                /// Từ cấp 71 đến cấp 80
                else if (level > 70 && level <= 80)
                {
                    int param1 = 7000;
                    int param2 = 60;
                    return GetData(level - 70, param1, param2, 1.1f);
                }
                /// Từ cấp 81 đến cấp 90
                else if (level > 80 && level <= 90)
                {
                    int param1 = 10100;
                    int param2 = 60;
                    return GetData(level - 80, param1, param2, 1.1f);
                }
                /// Từ cấp 91 đến cấp 100
                else if (level > 90 && level <= 100)
                {
                    int param1 = 12900;
                    int param2 = 60;
                    return GetData(level - 90, param1, param2, 1.1f);
                }
                /// Từ cấp 101 đến cấp 110
                else if (level > 100 && level <= 110)
                {
                    int param1 = 13000;
                    int param2 = 60;
                    return GetData(level - 100, param1, param2, 1.1f);
                }
                /// Từ cấp 111 đến cấp 120
                else if (level > 110 && level <= 120)
                {
                    int param1 = 15000;
                    int param2 = 60;
                    return GetData(level - 110, param1, param2, 1.1f);
                }
                /// Từ cấp 121 đến cấp 130
                else if (level > 120 && level <= 130)
                {
                    int param1 = 17000;
                    int param2 = 60;
                    return GetData(level - 120, param1, param2, 1.1f);
                }
                /// Từ cấp 131 đến cấp 140
                else if (level > 130 && level <= 140)
                {
                    int param1 = 19000;
                    int param2 = 60;
                    return GetData(level - 130, param1, param2, 1.1f);
                }
                /// Từ cấp 141 đến cấp 150
                else if (level > 140 && level <= 150)
                {
                    int param1 = 22000;
                    int param2 = 60;
                    return GetData(level - 140, param1, param2, 1.1f);
                }
                /// Từ cấp 150 trở lên
                else if (level > 150)
                {
                    int param1 = 25000;
                    int param2 = 60;
                    return GetData(level - 150, param1, param2, 1.1f);
                }

                /// Toác
                return 0;
            }

            #endregion Máu

            #region Chính xác

            /// <summary>
            /// Chính xác theo cấp
            /// </summary>
            private static readonly Dictionary<int, int> hitByLevels = new Dictionary<int, int>()
            {
                { 1, 30 },
                { 10, 70 },
                { 100, 700 },
                { 150, 1050 },
                { 200, 1250 },
            };

            /// <summary>
            /// Trả về chính xác theo cấp độ quái
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            public static int GetHitByLevel(int level)
            {
                /// Giá trị Min
                KeyValuePair<int, int> minRange = default;
                /// Giá trị Max
                KeyValuePair<int, int> maxRange = default;
                /// Duyệt danh sách
                foreach (KeyValuePair<int, int> pair in MonsterUtilities.hitByLevels)
                {
                    /// Nếu nhỏ hơn cấp độ quái
                    if (pair.Key <= level)
                    {
                        /// Cập nhật giá trị nhỏ nhất
                        minRange = pair;
                    }
                    /// Nếu lớn hơn
                    else
                    {
                        /// Cập nhật giá trị lớn nhất
                        maxRange = pair;
                        /// Thoát
                        break;
                    }
                }

                /// Nếu nằm ngoài phạm vi
                if (maxRange.Equals(default))
                {
                    /// Trả về kết quả min
                    return minRange.Value;
                }

                /// Độ lệch cấp độ khoảng
                int diffLevel = maxRange.Key - minRange.Key;
                /// Độ lệch giá trị khoảng
                int diffValue = maxRange.Value - minRange.Value;
                /// % độ lệch so với cấp hiện tại
                float diffPercent = (level - minRange.Key) / (float)diffLevel;
                /// Giá trị tương ứng
                int value = (int)(minRange.Value + diffPercent * diffValue);

                /// Trả về kết quả
                return value;
            }

            #endregion Chính xác

            #region Né tránh

            /// <summary>
            /// Né tránh theo cấp
            /// </summary>
            private static readonly Dictionary<int, int> dodgeByLevels = new Dictionary<int, int>()
            {
                { 1, 5 },
                { 10, 5 },
                { 11, 10 },
                { 100, 200 },
                { 150, 300 },
                { 200, 350 },
            };

            /// <summary>
            /// Trả về né tránh theo cấp độ quái
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            public static int GetDodgeByLevel(int level)
            {
                /// Giá trị Min
                KeyValuePair<int, int> minRange = default;
                /// Giá trị Max
                KeyValuePair<int, int> maxRange = default;
                /// Duyệt danh sách
                foreach (KeyValuePair<int, int> pair in MonsterUtilities.dodgeByLevels)
                {
                    /// Nếu nhỏ hơn cấp độ quái
                    if (pair.Key <= level)
                    {
                        /// Cập nhật giá trị nhỏ nhất
                        minRange = pair;
                    }
                    /// Nếu lớn hơn
                    else
                    {
                        /// Cập nhật giá trị lớn nhất
                        maxRange = pair;
                        /// Thoát
                        break;
                    }
                }

                /// Nếu nằm ngoài phạm vi
                if (maxRange.Equals(default))
                {
                    /// Trả về kết quả min
                    return minRange.Value;
                }

                /// Độ lệch cấp độ khoảng
                int diffLevel = maxRange.Key - minRange.Key;
                /// Độ lệch giá trị khoảng
                int diffValue = maxRange.Value - minRange.Value;
                /// % độ lệch so với cấp hiện tại
                float diffPercent = (level - minRange.Key) / (float)diffLevel;
                /// Giá trị tương ứng
                int value = (int)(minRange.Value + diffPercent * diffValue);

                /// Trả về kết quả
                return value;
            }

            #endregion Né tránh

            #region Sát thương Min

            /// <summary>
            /// Sát thương Min theo cấp
            /// </summary>
            private static readonly Dictionary<int, int> minDamageByLevels = new Dictionary<int, int>()
            {
                { 1, 1 },
            };

            /// <summary>
            /// Trả về sát thương Min theo cấp độ của quái
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            public static int GetMinDamageByLevel(int level)
            {
                /// Giá trị Min
                KeyValuePair<int, int> minRange = default;
                /// Giá trị Max
                KeyValuePair<int, int> maxRange = default;
                /// Duyệt danh sách
                foreach (KeyValuePair<int, int> pair in MonsterUtilities.minDamageByLevels)
                {
                    /// Nếu nhỏ hơn cấp độ quái
                    if (pair.Key <= level)
                    {
                        /// Cập nhật giá trị nhỏ nhất
                        minRange = pair;
                    }
                    /// Nếu lớn hơn
                    else
                    {
                        /// Cập nhật giá trị lớn nhất
                        maxRange = pair;
                        /// Thoát
                        break;
                    }
                }

                /// Nếu nằm ngoài phạm vi
                if (maxRange.Equals(default))
                {
                    /// Trả về kết quả min
                    return minRange.Value;
                }

                /// Độ lệch cấp độ khoảng
                int diffLevel = maxRange.Key - minRange.Key;
                /// Độ lệch giá trị khoảng
                int diffValue = maxRange.Value - minRange.Value;
                /// % độ lệch so với cấp hiện tại
                float diffPercent = (level - minRange.Key) / (float)diffLevel;
                /// Giá trị tương ứng
                int value = (int)(minRange.Value + diffPercent * diffValue);

                /// Trả về kết quả
                return value;
            }

            #endregion Sát thương Min

            #region Sát thương Max

            /// <summary>
            /// Sát thương Max theo cấp
            /// </summary>
            private static readonly Dictionary<int, int> maxDamageByLevels = new Dictionary<int, int>()
            {
                { 1, 10 },
            };

            /// <summary>
            /// Trả về sát thương Max theo cấp độ của quái
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            public static int GetMaxDamageByLevel(int level)
            {
                /// Giá trị Min
                KeyValuePair<int, int> minRange = default;
                /// Giá trị Max
                KeyValuePair<int, int> maxRange = default;
                /// Duyệt danh sách
                foreach (KeyValuePair<int, int> pair in MonsterUtilities.maxDamageByLevels)
                {
                    /// Nếu nhỏ hơn cấp độ quái
                    if (pair.Key <= level)
                    {
                        /// Cập nhật giá trị nhỏ nhất
                        minRange = pair;
                    }
                    /// Nếu lớn hơn
                    else
                    {
                        /// Cập nhật giá trị lớn nhất
                        maxRange = pair;
                        /// Thoát
                        break;
                    }
                }

                /// Nếu nằm ngoài phạm vi
                if (maxRange.Equals(default))
                {
                    /// Trả về kết quả min
                    return minRange.Value;
                }

                /// Độ lệch cấp độ khoảng
                int diffLevel = maxRange.Key - minRange.Key;
                /// Độ lệch giá trị khoảng
                int diffValue = maxRange.Value - minRange.Value;
                /// % độ lệch so với cấp hiện tại
                float diffPercent = (level - minRange.Key) / (float)diffLevel;
                /// Giá trị tương ứng
                int value = (int)(minRange.Value + diffPercent * diffValue);

                /// Trả về kết quả
                return value;
            }

            #endregion Sát thương Max

            #region Sinh lực phục hồi mỗi 5 giây

            /// <summary>
            /// Sinh lực phục hồi mỗi 5 giây theo cấp
            /// </summary>
            public static readonly Dictionary<int, int> lifeReplenishByLevels = new Dictionary<int, int>()
            {
                { 1, 5 },
                { 10, 10 },
                { 20, 65 },
                { 60, 250 },
                { 100, 750 },
                { 150, 750 },
                { 200, 750 },
            };

            /// <summary>
            /// Trả về sinh lực phục hồi mỗi 5 giây theo cấp độ của quái
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            public static int GetLifeReplenishByLevel(int level)
            {
                /// Giá trị Min
                KeyValuePair<int, int> minRange = default;
                /// Giá trị Max
                KeyValuePair<int, int> maxRange = default;
                /// Duyệt danh sách
                foreach (KeyValuePair<int, int> pair in MonsterUtilities.lifeReplenishByLevels)
                {
                    /// Nếu nhỏ hơn cấp độ quái
                    if (pair.Key <= level)
                    {
                        /// Cập nhật giá trị nhỏ nhất
                        minRange = pair;
                    }
                    /// Nếu lớn hơn
                    else
                    {
                        /// Cập nhật giá trị lớn nhất
                        maxRange = pair;
                        /// Thoát
                        break;
                    }
                }

                /// Nếu nằm ngoài phạm vi
                if (maxRange.Equals(default))
                {
                    /// Trả về kết quả min
                    return minRange.Value;
                }

                /// Độ lệch cấp độ khoảng
                int diffLevel = maxRange.Key - minRange.Key;
                /// Độ lệch giá trị khoảng
                int diffValue = maxRange.Value - minRange.Value;
                /// % độ lệch so với cấp hiện tại
                float diffPercent = (level - minRange.Key) / (float)diffLevel;
                /// Giá trị tương ứng
                int value = (int)(minRange.Value + diffPercent * diffValue);

                /// Trả về kết quả
                return value;
            }

            #endregion Sinh lực phục hồi mỗi 5 giây

            #region Sát thương ngũ hành

            /// <summary>
            /// Sát thương ngũ hành theo cấp
            /// </summary>
            public static Dictionary<int, int> seriesDamageByLevels = new Dictionary<int, int>()
            {
                { 1, 1 },
                { 9, 2 },
                { 10, 2 },
                { 30, 5 },
                { 60, 10 },
                { 100, 50 },
                { 150, 50 },
                { 200, 50 },
            };

            /// <summary>
            /// Trả về sát thương ngũ hành theo cấp độ của quái
            /// </summary>
            /// <param name="level"></param>
            /// <param name="damageType"></param>
            /// <param name="series"></param>
            /// <returns></returns>
            public static int GetSeriesDamageByLevel(int level, DAMAGE_TYPE damageType, KE_SERIES_TYPE series)
            {
                /// Giá trị Min
                KeyValuePair<int, int> minRange = default;
                /// Giá trị Max
                KeyValuePair<int, int> maxRange = default;
                /// Duyệt danh sách
                foreach (KeyValuePair<int, int> pair in MonsterUtilities.seriesDamageByLevels)
                {
                    /// Nếu nhỏ hơn cấp độ quái
                    if (pair.Key <= level)
                    {
                        /// Cập nhật giá trị nhỏ nhất
                        minRange = pair;
                    }
                    /// Nếu lớn hơn
                    else
                    {
                        /// Cập nhật giá trị lớn nhất
                        maxRange = pair;
                        /// Thoát
                        break;
                    }
                }

                /// Nếu nằm ngoài phạm vi
                if (maxRange.Equals(default))
                {
                    /// Trả về kết quả min
                    return minRange.Value;
                }

                /// Độ lệch cấp độ khoảng
                int diffLevel = maxRange.Key - minRange.Key;
                /// Độ lệch giá trị khoảng
                int diffValue = maxRange.Value - minRange.Value;
                /// % độ lệch so với cấp hiện tại
                float diffPercent = (level - minRange.Key) / (float)diffLevel;
                /// Giá trị tương ứng
                int value = (int)(minRange.Value + diffPercent * diffValue);

                /// Hệ số nhân ngũ hành
                float seriesMultiply = 1f;
                /// Xem ngũ hành là gì
                switch (series)
                {
                    /// Kim
                    case KE_SERIES_TYPE.series_metal:
                        {
                            /// Nếu không phải sát thương vật công
                            if (damageType != DAMAGE_TYPE.damage_physics && damageType != DAMAGE_TYPE.damage_magic)
                            {
                                /// Giảm hệ số
                                seriesMultiply = 0.5f;
                            }
                            break;
                        }
                    /// Mộc
                    case KE_SERIES_TYPE.series_wood:
                        {
                            /// Nếu không phải sát thương độc công
                            if (damageType != DAMAGE_TYPE.damage_poison)
                            {
                                /// Giảm hệ số
                                seriesMultiply = 0.5f;
                            }
                            break;
                        }
                    /// Thủy
                    case KE_SERIES_TYPE.series_water:
                        {
                            /// Nếu không phải sát thương băng công
                            if (damageType != DAMAGE_TYPE.damage_cold)
                            {
                                /// Giảm hệ số
                                seriesMultiply = 0.5f;
                            }
                            break;
                        }
                    /// Hỏa
                    case KE_SERIES_TYPE.series_fire:
                        {
                            /// Nếu không phải sát thương hỏa công
                            if (damageType != DAMAGE_TYPE.damage_fire)
                            {
                                /// Giảm hệ số
                                seriesMultiply = 0.5f;
                            }
                            break;
                        }
                    /// Thổ
                    case KE_SERIES_TYPE.series_earth:
                        {
                            /// Nếu không phải sát thương lôi công
                            if (damageType != DAMAGE_TYPE.damage_light)
                            {
                                /// Giảm hệ số
                                seriesMultiply = 0.5f;
                            }
                            break;
                        }
                }

                /// Nhân hệ số ngũ hành vào kết quả
                value = (int)(value * seriesMultiply);

                /// Trả về kết quả
                return value;
            }

            #endregion Sát thương ngũ hành

            #region Kháng ngũ hành

            /// <summary>
            /// Kháng ngũ hành theo cấp
            /// </summary>
            public static readonly Dictionary<int, int> resistanceByLevels = new Dictionary<int, int>()
            {
                { 1, 20 },
                { 9, 28 },
                { 10, 65 },
                { 100, 245 },
                { 150, 245 },
                { 200, 245 },
            };

            /// <summary>
            /// Trả về kháng ngũ hành theo cấp độ của quái
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            public static int GetSeriesResistanceByLevel(int level)
            {
                /// Giá trị Min
                KeyValuePair<int, int> minRange = default;
                /// Giá trị Max
                KeyValuePair<int, int> maxRange = default;
                /// Duyệt danh sách
                foreach (KeyValuePair<int, int> pair in MonsterUtilities.resistanceByLevels)
                {
                    /// Nếu nhỏ hơn cấp độ quái
                    if (pair.Key <= level)
                    {
                        /// Cập nhật giá trị nhỏ nhất
                        minRange = pair;
                    }
                    /// Nếu lớn hơn
                    else
                    {
                        /// Cập nhật giá trị lớn nhất
                        maxRange = pair;
                        /// Thoát
                        break;
                    }
                }

                /// Nếu nằm ngoài phạm vi
                if (maxRange.Equals(default))
                {
                    /// Trả về kết quả min
                    return minRange.Value;
                }

                /// Độ lệch cấp độ khoảng
                int diffLevel = maxRange.Key - minRange.Key;
                /// Độ lệch giá trị khoảng
                int diffValue = maxRange.Value - minRange.Value;
                /// % độ lệch so với cấp hiện tại
                float diffPercent = (level - minRange.Key) / (float)diffLevel;
                /// Giá trị tương ứng
                int value = (int)(minRange.Value + diffPercent * diffValue);

                /// Trả về kết quả
                return value;
            }

            #endregion Kháng ngũ hành

            #region Kinh nghiệm cơ bản

            /// <summary>
            /// Kinh nghiệm theo cấp
            /// </summary>
            private static readonly Dictionary<int, int> expByLevels = new Dictionary<int, int>()
            {
                { 1, 50 },
                { 9, 50 },

                { 10, 100 },
                { 19, 100 },

                { 20, 150 },
                { 29, 150 },

                { 30, 200 },
                { 39, 200 },

                { 40, 300 },
                { 49, 300 },

                { 50, 400 },
                { 59, 400 },

                { 60, 500 },
                { 69, 500 },

                { 70, 650 },
                { 79, 650 },

                { 80, 800 },
                { 89, 800 },

                { 90, 850 },
                { 99, 850 },

                { 100, 900 },
                { 109, 900 },

                { 110, 950 },
                { 119, 950 },

                { 120, 1000 },
                { 129, 1000 },

                { 130, 1100 },
                { 139, 1100 },

                { 140, 1250 },
                { 150, 1250 },

                { 151, 1350 },
                { 200, 1350 },
            };

            /// <summary>
            /// Trả về kinh nghiệm theo cấp độ của quái
            /// </summary>
            /// <param name="level"></param>
            /// <returns></returns>
            public static int GetExpByLevel(int level)
            {
                /// Giá trị Min
                KeyValuePair<int, int> minRange = default;
                /// Giá trị Max
                KeyValuePair<int, int> maxRange = default;
                /// Duyệt danh sách
                foreach (KeyValuePair<int, int> pair in MonsterUtilities.expByLevels)
                {
                    /// Nếu nhỏ hơn cấp độ quái
                    if (pair.Key <= level)
                    {
                        /// Cập nhật giá trị nhỏ nhất
                        minRange = pair;
                    }
                    /// Nếu lớn hơn
                    else
                    {
                        /// Cập nhật giá trị lớn nhất
                        maxRange = pair;
                        /// Thoát
                        break;
                    }
                }

                /// Nếu nằm ngoài phạm vi
                if (maxRange.Equals(default))
                {
                    /// Trả về kết quả min
                    return minRange.Value;
                }

                /// Độ lệch cấp độ khoảng
                int diffLevel = maxRange.Key - minRange.Key;
                /// Độ lệch giá trị khoảng
                int diffValue = maxRange.Value - minRange.Value;
                /// % độ lệch so với cấp hiện tại
                float diffPercent = (level - minRange.Key) / (float)diffLevel;
                /// Giá trị tương ứng
                int value = (int)(minRange.Value + diffPercent * diffValue);

                /// Trả về kết quả
                return value;
            }

            #endregion Kinh nghiệm cơ bản

            #region Kinh nghiệm khi giết quái

            /// <summary>
            /// Tính toán lượng kinh nghiệm nhận được khi người chơi giết quái
            /// </summary>
            /// <param name="nExp"></param>
            /// <param name="nSelfLevel"></param>
            /// <param name="nTarLevel"></param>
            /// <returns></returns>
            public static int CalculatePlayerExpByMonsterLevel(int nExp, int nSelfLevel, int nTarLevel)
            {
                /// Độ lệch cấp
                int nSubLevel = nSelfLevel - nTarLevel;
                /// Lượng kinh nghiệm nhận được
                int nGetExp;

                /// Nếu cấp bản thân mà trên 100
                if (nSelfLevel >= 100)
                {
                    /// Nếu mà Con quái có cấp >= 90 thì thôi không giảm kinh nghiệm
                    if (nTarLevel >= 90)
                    {
                        nGetExp = nExp;
                    }
                    /// Cấp dưới thì không có gì
                    else
                    {
                        nGetExp = 1;
                    }
                }
                /// Nếu mà đánh quái > 13 level thì kinh nghiệm nhận được là 1
                else if (nSubLevel < -13)
                {
                    nGetExp = 1;
                }
                /// Nếu đánh quái > 5 cấp thì kinh nghiệm bị chia ra
                else if (nSubLevel < -5)
                {
                    nGetExp = nExp * (14 + nSubLevel) / 10;
                }
                /// Nếu đánh quái <= 5 cấp thì kinh nghiệm giữ nguyên
                else if (nSubLevel <= 5)
                {
                    nGetExp = nExp;
                }
                /// Nếu cấp bản thân mà > 20 cấp quái thì kinh nghiệm bị chia ra
                else if (nSubLevel <= 20)
                {
                    nGetExp = nExp * (30 - nSubLevel) / 25;
                }
                /// Các trường hợp còn lại kinh nghiệm / 2.5
                else
                {
                    nGetExp = nExp * 2 / 5;
                }

                /// Nếu kinh nghiệm chia ra nhỏ hơn 0 thì thiết lập là 1
                if (nGetExp <= 0)
                {
                    nGetExp = 1;
                }

                /// Kiểm tra xem sự kiện có mở hay không
                if (ExpMutipleEvent.IsOpen())
                {
                    /// Tăng tỷ lệ kinh nghiệm nhận được
                    nGetExp = (int)(nGetExp * ExpMutipleEvent.GetRate() * ServerConfig.Instance.ExpRate);
                }
                else
                {
                    nGetExp = (int)(nGetExp  * ServerConfig.Instance.ExpRate);
                }

                /// Trả về kết quả
                return nGetExp;
            }

            #endregion Kinh nghiệm khi giết quái
        }
    }
}