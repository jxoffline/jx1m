using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using System;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Quản lý lửa trại
        /// </summary>
        public static class FireCampManager
        {
            /// <summary>
            /// Res ID đống củi
            /// </summary>
            private const int FireWoodResID = 20021;

            /// <summary>
            /// Tên đống củi
            /// </summary>
            private const string FireWoodName = "Đống củi";

            /// <summary>
            /// Thời gian tồn tại đống củi
            /// </summary>
            private const int FireWoodLifeTime = 30000;

            /// <summary>
            /// Thời gian đốt củi
            /// </summary>
            private const int FireWoodCollectTicks = 2000;

            /// <summary>
            /// Thời gian cho phép người chơi mở đống củi khi chủ nhân không mở
            /// </summary>
            private const int FireWoodFreeOpenAfterTicks = 60000;

            /// <summary>
            /// ID Res lửa trại
            /// </summary>
            private const int FireCampResID = 20022;

            /// <summary>
            /// Tên lửa trại
            /// </summary>
            private const string FireCampName = "Lửa trại";

            /// <summary>
            /// Thời gian đốt củi
            /// </summary>
            private const int FireCampLifeTime = 300000;

            /// <summary>
            /// Thời gian nhận kinh nghiệm lửa trại
            /// </summary>
            private const int FireCampExpTicks = 5000;

            /// <summary>
            /// Bán kính lửa trại
            /// </summary>
            private const int FireCampRadius = 500;

            /// <summary>
            /// ID buff rượu
            /// </summary>
            private const int WineBuff = 378;

            /// <summary>
            /// Kinh nghiệm cơ bản của lửa trại
            /// </summary>
            private const int FireCampBaseExp = 2500;

            /// <summary>
            /// Tạo đống củi tương ứng
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="mapCode"></param>
            /// <param name="copySceneID"></param>
            /// <param name="posX"></param>
            /// <param name="posY"></param>
            /// <param name="onDestroyed"></param>
            public static void CreateFireWood(KPlayer owner, int mapCode, int copySceneID, int posX, int posY, Action onDestroyed = null)
            {
                /// Dữ liệu
                GrowPointXML fireWoodData = new GrowPointXML()
                {
                    CollectTick = FireCampManager.FireWoodCollectTicks,
                    Name = FireCampManager.FireWoodName,
                    ResID = FireCampManager.FireWoodResID,
                    RespawnTime = -100,
                    InteruptIfTakeDamage = false,
                    ScriptID = -1,
                };
                /// Tạo đống củi
                GrowPoint fireWood = KTGrowPointManager.Add(mapCode, copySceneID, fireWoodData, posX, posY, FireCampManager.FireWoodLifeTime);
                /// Kiểm tra điều kiện
                fireWood.ConditionCheck = (player) =>
                {
                    /// Nếu có người chơi giết
                    if (owner != null)
                    {
                        /// Nếu không phải thành viên nhóm và chưa đến giờ hết tác dụng giữ quyền tổ đội
                        if (player != owner && !KTGlobal.IsTeamMate(player, owner) && KTGlobal.GetCurrentTimeMilis() - fireWood.InitTicks < FireCampManager.FireWoodFreeOpenAfterTicks)
                        {
                            /// Thời gian còn lại
                            int secLeft = (int)((FireCampManager.FireWoodFreeOpenAfterTicks - (KTGlobal.GetCurrentTimeMilis() - fireWood.InitTicks)) / 1000);
                            /// Thông báo
                            KTPlayerManager.ShowNotification(player, string.Format("Bạn không phải chủ nhân của đống củi này, {0} giây sau mới có thể thao tác!", secLeft));
                            /// Toác
                            return false;
                        }
                    }

                    /// OK
                    return true;
                };
                /// Sự kiện mở thành công
                fireWood.GrowPointCollectCompleted = (player) =>
                {
                    /// Tạo lửa trại
                    FireCampManager.CreateFireCamp(owner, mapCode, copySceneID, posX, posY, onDestroyed);
                };
                /// Sự kiện hết thời gian
                fireWood.OnTimeout = () =>
                {
                    /// Thực thi sự kiện bị hủy
                    onDestroyed?.Invoke();
                };
            }

            /// <summary>
            /// Tạo lửa trại tương ứng
            /// </summary>
            /// <param name="owner"></param>
            /// <param name="mapCode"></param>
            /// <param name="copySceneID"></param>
            /// <param name="posX"></param>
            /// <param name="posY"></param>
            /// <param name="onDestroyed"></param>
            public static void CreateFireCamp(KPlayer owner, int mapCode, int copySceneID, int posX, int posY, Action onDestroyed = null)
            {
                int MapLevel = 0;

                GameMap GameMap = KTMapManager.Find(mapCode);
                if (GameMap != null)
                {
                    MapLevel = GameMap.MapLevel;
                }
                /// Tạo lửa trại
                KDynamicArea fireCamp = KTDynamicAreaManager.Add(mapCode, copySceneID, FireCampManager.FireCampName, FireCampManager.FireCampResID, posX, posY, FireCampManager.FireCampLifeTime, FireCampManager.FireCampExpTicks, FireCampManager.FireCampRadius, -1);
                /// Sự kiện người chơi ở trong phạm vi
                fireCamp.OnStayTick = (go) =>
                {
                    /// Nếu không phỉa người chơi
                    if (!(go is KPlayer player))
                    {
                        /// Bỏ qua
                        return;
                    }

                    /// Nếu không cùng nhóm
                    if (owner != player && !KTGlobal.IsTeamMate(owner, player))
                    {
                        /// Bỏ qua
                        return;
                    }

                    /// Thực thi nhận kinh nghiệm lửa trại
                    FireCampManager.ProcessFireCampExp(player, MapLevel);
                };
                /// Sự kiện hết thời gian
                fireCamp.OnTimeout = () =>
                {
                    /// Thực thi sự kiện bị hủy
                    onDestroyed?.Invoke();
                };
            }

            /// <summary>
            /// Trả về kinh nghiệm lửa trại nhận được theo cấp độ bản đồ tương ứng
            /// </summary>
            /// <param name="mapLevel"></param>
            /// <returns></returns>
            private static int GetExpByMapLevel(int mapLevel)
            {
                if(GameManager.IsKuaFuServer)
                {
                    return 15000;
                }    

                if (mapLevel < 20)
                {
                    return 200;
                }
                else if (mapLevel >= 20 && mapLevel < 40)
                {
                    return 1000;
                }
                else if (mapLevel >= 40 && mapLevel < 60)
                {
                    return 2500;
                }
                else if (mapLevel >= 60 && mapLevel < 80)
                {
                    return 5000;
                }
                else if (mapLevel >= 80 && mapLevel < 100)
                {
                    return 7500;
                }
                else if (mapLevel >= 100 && mapLevel < 120)
                {
                    return 10000;
                }
                else if (mapLevel >= 120 && mapLevel < 140)
                {
                    return 12500;
                }
                else if (mapLevel >= 140 && mapLevel < 160)
                {
                    return 15000;
                }
                else if (mapLevel >= 160 && mapLevel < 180)
                {
                    return 17500;
                }
                else if (mapLevel >= 180 && mapLevel < 200)
                {
                    return 20000;
                }

                return 0;
            }

            /// <summary>
            /// Thực thi nhận kinh nghiệm lửa trại
            /// </summary>
            /// <param name="player"></param>
            private static void ProcessFireCampExp(KPlayer player, int MapLevel)
            {
                /// Kinh nghiệm cơ bản nhận được
                int expGain = FireCampManager.GetExpByMapLevel(MapLevel);
                /// Cộng thêm tỷ lệ từ các Buff hỗ trợ
                expGain += expGain * player.m_nExpEnhancePercent / 100;
                expGain += expGain * player.m_nExpAddtionP / 100;

                /// Nếu có rượu thì tăng x2 kinh nghiệm
                if (player.Buffs.HasBuff(FireCampManager.WineBuff))
                {
                    expGain *= 2;
                }

                /// Thực thi nhận kinh nghiệm cho người chơi
                KTPlayerManager.AddExp(player, expGain);
            }
        }
    }
}