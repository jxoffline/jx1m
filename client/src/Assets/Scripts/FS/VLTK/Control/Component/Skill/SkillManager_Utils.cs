using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Factory;
using FS.VLTK.Logic;
using FS.VLTK.Logic.Settings;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Các phương thức hỗ trợ
    /// </summary>
    public static partial class SkillManager
    {
        /// <summary>
        /// Phạm vi tìm mục tiêu
        /// </summary>
        public const int FindEnemyRange = 800;

        /// <summary>
        /// Trả về lượng máu hiện tại của mục tiêu
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static int GetTargetTrueHP(GSprite target)
        {
            /// Nếu không có dữ liệu
            if (target == null)
            {
                return 0;
            }

            /// Nếu là người chơi khác
            if (Global.Data.OtherRoles.TryGetValue(target.RoleID, out RoleData rd))
            {
                return rd.CurrentHP;
            }
            /// Nếu là quái
            else if (Global.Data.SystemMonsters.TryGetValue(target.RoleID, out MonsterData md))
            {
                return md.HP;
            }
            /// Nếu là pet
            else if (Global.Data.SystemPets.TryGetValue(target.RoleID, out PetDataMini pd))
            {
                return pd.HP;
            }
            /// Nếu là xe tiêu
            else if (Global.Data.TraderCarriages.TryGetValue(target.RoleID, out TraderCarriageData td))
            {
                return td.HP;
            }
            /// Mặc định thì = 0
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Trả về danh sách kẻ địch xung quanh theo khoảng cách từ gần tới xa
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static List<GSprite> FindNearestEnemies(GSprite sprite, int maxDistance = -1)
        {
            /// Tìm danh sách mục tiêu là kẻ địch
            List<GSprite> listEnemies = KTObjectsManager.Instance.FindObjects<GSprite>((target) =>
            {
                /// Mục tiêu không phù hợp
                if (!SkillManager.IsValidTarget(target))
                {
                    return false;
                }
                /// Nếu khoảng cách quá xa
                if (maxDistance > 0 && Vector2.Distance(target.PositionInVector2, sprite.PositionInVector2) > maxDistance)
                {
                    return false;
                }

                /// OK
                return true;
            }).ToList();

            listEnemies.Sort((o1, o2) =>
            {
                /// Khoảng cách hiện tại đến mục tiêu
                float distanceO1 = Vector2.Distance(o1.PositionInVector2, sprite.PositionInVector2);
                float distanceO2 = Vector2.Distance(o2.PositionInVector2, sprite.PositionInVector2);

                return (int) (distanceO1 - distanceO2);
            });

            return listEnemies;
        }

        public static List<GSprite> FindNearestEnemies(Vector2 CenterPoint, int maxDistance = -1)
        {
            /// Tìm danh sách mục tiêu là kẻ địch
            List<GSprite> listEnemies = KTObjectsManager.Instance.FindObjects<GSprite>((target) =>
            {
                /// Mục tiêu không phù hợp
                if (!SkillManager.IsValidTarget(target))
                {
                    return false;
                }
                /// Nếu khoảng cách quá xa
                if (maxDistance > 0 && Vector2.Distance(target.PositionInVector2, CenterPoint) > maxDistance)
                {
                    return false;
                }

                /// OK
                return true;
            }).ToList();

            listEnemies.Sort((o1, o2) =>
            {
                /// Khoảng cách hiện tại đến mục tiêu
                float distanceO1 = Vector2.Distance(o1.PositionInVector2, CenterPoint);
                float distanceO2 = Vector2.Distance(o2.PositionInVector2, CenterPoint);

                return (int) (distanceO1 - distanceO2);
            });

            return listEnemies;
        }

        /// <summary>
        /// Tìm kẻ địch gần nhất thỏa mãn điều kiện
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static GSprite FindNearestEnemy(GSprite sprite, Predicate<GSprite> predicate, int maxRange = -1)
        {
            /// Mục tiêu
            GSprite target = null;
            /// Nếu đang mở Auto
            if (KTAutoFightManager.Instance.IsAutoFighting)
            {
                /// Nếu là chế độ Farm
                if (!KTAutoAttackSetting.Config.EnableAutoPK)
                {
                    /// Nếu mở chế độ đánh quanh điểm
                    if (KTAutoAttackSetting.Config.Farm.FarmAround)
                    {
                        /// Phạm vi
                        maxRange = Math.Max(maxRange, KTAutoAttackSetting.Config.Farm.ScanRange);
                    }
                }
            }

            /// Tìm danh sách mục tiêu là kẻ địch
            List<GSprite> listEnemies = SkillManager.FindNearestEnemies(sprite, maxRange);

            /// Nếu là chế độ chỉ giết người
            if (KTAutoAttackSetting.Config.EnableAutoPK)
            {
                /// Nếu ưu tiên khắc hệ và máu ít
                if (KTAutoAttackSetting.Config.PK.SeriesConquarePriority && KTAutoAttackSetting.Config.PK.LowHPTargetPriority)
                {
                    listEnemies = listEnemies.Where(x => x.SpriteType == GSpriteTypes.Other).OrderBy(x => (KTGlobal.g_IsConquer(sprite.RoleData.FactionID, x.RoleData.FactionID))).ThenByDescending(x => x.HP).ToList();
                }
                /// Nếu ưu tiên mỗi khắc hệ
                else if (KTAutoAttackSetting.Config.PK.SeriesConquarePriority && !KTAutoAttackSetting.Config.PK.LowHPTargetPriority)
                {
                    listEnemies = listEnemies.Where(x => x.SpriteType == GSpriteTypes.Other).OrderBy(x => (KTGlobal.g_IsConquer(sprite.RoleData.FactionID, x.RoleData.FactionID))).ToList();
                }
                /// Nếu ưu tiên mỗi mục tiêu ít máu
                else if (!KTAutoAttackSetting.Config.PK.SeriesConquarePriority && KTAutoAttackSetting.Config.PK.LowHPTargetPriority)
                {
                    listEnemies = listEnemies.Where(x => x.SpriteType == GSpriteTypes.Other).OrderBy(x => x.HP).ToList();
                }
            }
            /// Nếu là chế độ train
            else
            {
                if (KTAutoAttackSetting.Config.Farm.LowHPTargetPriority)
                {
                    listEnemies = listEnemies.Where(x => x.SpriteType == GSpriteTypes.Monster || x.SpriteType == GSpriteTypes.Pet).OrderBy(x => x.HP).ToList();
                }
            }

            /// Duyệt danh sách
            for (int i = 0; i < listEnemies.Count; i++)
            {
                /// Nếu thỏa mãn điều kiện
                if ((predicate == null || predicate.Invoke(listEnemies[i])))
                {
                    target = listEnemies[i];
                    break;
                }
            }

            /// Xóa mảng
            listEnemies.Clear();
            listEnemies = null;

            /// Trả về mục tiêu
            return target;
        }

        /// <summary>
        /// Tìm quái xung quanh điểm cắm cọc
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="CenterPoint"></param>
        /// <param name="predicate"></param>
        /// <param name="maxRange"></param>
        /// <returns></returns>
        public static GSprite FindNearestEnemy(GSprite sprite, Vector2 centerPoint, Predicate<GSprite> predicate, int maxRange = -1)
        {
            /// Mục tiêu
            GSprite target = null;
            /// Nếu đang mở Auto
            if (KTAutoFightManager.Instance.IsAutoFighting)
            {
                /// Nếu mở chế độ đánh quanh điểm
                if (KTAutoAttackSetting.Config.Farm.FarmAround)
                {
                    /// Phạm vi
                    maxRange = Math.Max(maxRange, KTAutoAttackSetting.Config.Farm.ScanRange);
                }
            }
            /// Tìm danh sách mục tiêu là kẻ địch
            List<GSprite> listEnemies = SkillManager.FindNearestEnemies(centerPoint, maxRange);

            /// Nếu là chế độ chỉ giết người
            if (KTAutoAttackSetting.Config.EnableAutoPK)
            {
                /// Nếu ưu tiên khắc hệ và máu ít
                if (KTAutoAttackSetting.Config.PK.SeriesConquarePriority && KTAutoAttackSetting.Config.PK.LowHPTargetPriority)
                {
                    listEnemies = listEnemies.Where(x => x.SpriteType == GSpriteTypes.Other).OrderBy(x => (KTGlobal.g_IsConquer(sprite.RoleData.FactionID, x.RoleData.FactionID))).ThenByDescending(x => x.HP).ToList();
                }
                /// Nếu ưu tiên mỗi khắc hệ
                else if (KTAutoAttackSetting.Config.PK.SeriesConquarePriority && !KTAutoAttackSetting.Config.PK.LowHPTargetPriority)
                {
                    listEnemies = listEnemies.Where(x => x.SpriteType == GSpriteTypes.Other).OrderBy(x => (KTGlobal.g_IsConquer(sprite.RoleData.FactionID, x.RoleData.FactionID))).ToList();
                }
                /// Nếu ưu tiên mỗi mục tiêu ít máu
                else if (!KTAutoAttackSetting.Config.PK.SeriesConquarePriority && KTAutoAttackSetting.Config.PK.LowHPTargetPriority)
                {
                    listEnemies = listEnemies.Where(x => x.SpriteType == GSpriteTypes.Other).OrderBy(x => x.HP).ToList();
                }
            }
            /// Nếu là chế độ train
            else
            {
                if (KTAutoAttackSetting.Config.Farm.LowHPTargetPriority)
                {
                    listEnemies = listEnemies.Where(x => x.SpriteType == GSpriteTypes.Monster || x.SpriteType == GSpriteTypes.Pet).OrderBy(x => x.HP).ToList();
                }
            }

            /// Duyệt danh sách
            for (int i = 0; i < listEnemies.Count; i++)
            {
                /// Nếu thỏa mãn điều kiện
                if ((predicate == null || predicate.Invoke(listEnemies[i])))
                {
                    target = listEnemies[i];
                    break;
                }
            }

            /// Xóa mảng
            listEnemies.Clear();
            listEnemies = null;

            /// Trả về mục tiêu
            return target;
        }

        /// <summary>
        /// Tìm kẻ địch gần nhất
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static GSprite FindNearestEnemy(GSprite sprite)
        {
            return SkillManager.FindNearestEnemy(sprite, null);
        }

        /// <summary>
        /// Mục tiêu có phù hợp để tấn công không
        /// <para>Áp dụng cho Leader</para>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="targetMustBeEnemy"></param>
        /// <param name="targetMustAlive"></param>
        /// <returns></returns>
        public static bool IsValidTarget(GSprite target, bool targetMustBeEnemy = true, bool targetMustAlive = true)
        {
            /// Nếu không có mục tiêu
            if (target == null)
            {
                return false;
            }

            /// Nếu yêu cầu mục tiêu phải còn sống nhưng thực tế mục tiêu đã chết
            if (targetMustAlive && (target.IsDeath || SkillManager.GetTargetTrueHP(target) <= 0))
            {
                return false;
            }
            /// Nếu yêu cầu mục tiêu đã chết nhưng thực tế mục tiêu còn sống
            else if (!targetMustAlive && !(target.IsDeath || SkillManager.GetTargetTrueHP(target) <= 0))
            {
                return false;
            }

            /// Nếu trạng thái PK không phù hợp
            if (targetMustBeEnemy && !KTGlobal.CanAttack(target))
            {
                return false;
            }
            else if (!targetMustBeEnemy && KTGlobal.CanAttack(target))
            {
                return false;
            }

            /// Nếu thỏa mãn
            return true;
        }
    }
}