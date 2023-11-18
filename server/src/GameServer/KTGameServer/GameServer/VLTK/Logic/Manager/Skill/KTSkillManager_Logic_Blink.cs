using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Logic kỹ năng Blink và khinh công
    /// </summary>
    public static partial class KTSkillManager
    {
        /// <summary>
        /// Thực hiện kỹ năng dịch chuyển nhanh đến vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        private static UnityEngine.Vector2 DoSkillBlinkToPosition(SkillLevelRef skill, GameObject caster, PropertyDictionary skillPd)
        {
            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            int range = skill.Data.AttackRadius;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_param1_v))
            {
                range = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_param1_v).nValue[0];
            }
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                range = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            if (range <= 0)
            {
                return casterPos;
            }

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);
            /// Dịch đến vị trí chỉ định
            UnityEngine.Vector2 destPos = KTMath.MoveTowardByDirection(casterPos, caster.CurrentDir, range);
            /// Vị trí không có vật cản trên đường đi
            destPos = KTGlobal.FindLinearNoObsPoint(gameMap, casterPos, destPos, caster.CurrentCopyMapID);

            /// Vector chỉ hướng
            UnityEngine.Vector2 finalDirVector = destPos - casterPos;

            /// Hướng quay của đối tượng
            float rotationAngle = KTMath.GetAngle360WithXAxis(finalDirVector);
            caster.CurrentDir = KTMath.GetDirectionByAngle360(rotationAngle);

            /// Thời gian dịch
            float animationDuration = 0.2f;

            /// Gửi gói tin về Client thực hiện biểu diễn hiệu ứng
            KT_TCPHandler.SendBlinkToPosition(caster, (int) destPos.x, (int) destPos.y, animationDuration);

            /// Thực hiện đổi vị trí luôn
            caster.CurrentPos = new System.Windows.Point((int) destPos.x, (int) destPos.y);
            /// Cập nhật vị trí đối tượng vào Map
            gameMap.Grid.MoveObject((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y, caster);
            ///// Thực hiện gọi hàm cập nhật di chuyển
            //if (caster is KPlayer)
            //{
            //    ClientManager.DoSpriteMapGridMove(caster as KPlayer);
            //}
            /// Thực hiện kỹ năng tan biến
            KTSkillManager.BulletVanished(skill, caster, destPos, null, finalDirVector);

            return destPos;
        }

        /// <summary>
        /// Thực hiện kỹ năng dịch chuyển nhanh qua lại giữa các kẻ địch xung quanh
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="firstTarget">Mục tiêu đầu tiên</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        private static void DoSkillBlinkTowardTargets(SkillLevelRef skill, GameObject caster, GameObject firstTarget, PropertyDictionary skillPd)
        {
            /// Số mục tiêu tối đa
            int maxTarget = 1;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_missile_hitcount))
            {
                maxTarget = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_missile_hitcount).nValue[0];
            }

            /// Cự thi thi triển
            int range = skill.Data.AttackRadius;
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                range = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            /// Mục tiêu tiếp theo
            GameObject nextTarget = firstTarget;

            /// Đánh dấu các mục tiêu đã chạm
            List<GameObject> touchTargets = new List<GameObject>();
            /// Danh sách mục tiêu xung quanh
            List<GameObject> nearByTargets = KTGlobal.GetNearByEnemies(caster, range);

            /// <summary>
            /// Hàm đệ quy gọi liên tục để tấn công các mục tiêu xung quanh
            /// </summary>
            void DoLogic()
            {
                /// Nếu đã vượt quá số mục tiêu được chạm
                if (touchTargets.Count >= maxTarget)
                {
                    return;
                }

                /// Nếu không có mục tiêu tiếp theo
                if (nextTarget == null)
                {
                    /// Chọn 1 mục tiêu bất kỳ còn sống
                    GameObject newTarget = nearByTargets.Where(x => !x.IsDead() && !touchTargets.Contains(x)).FirstOrDefault();
                    /// Nếu có kẻ địch xung quanh
                    if (newTarget != null)
                    {
                        nextTarget = newTarget;
                        /// Thêm mục tiêu vào danh sách đã chọn
                        touchTargets.Add(newTarget);
                    }
                    /// Nếu không còn kẻ địch xung quanh thì ngừng
                    else
                    {
                        return;
                    }
                }

                /// Thực hiện Logic
                KTSkillManager.DoSkillBlinkToTargetPosition(skill, caster, nextTarget, skillPd, false, () => {
                    /// Thiết lập không có mục tiêu tiếp theo để tìm thêm
                    nextTarget = null;
                    /// Thực hiện gọi đệ quy
                    DoLogic();
                });
            }

            /// Thực hiện Logic
            DoLogic();
        }

        /// <summary>
        /// Thực hiện kỹ năng dịch chuyển nhanh đến vị trí chỉ định
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        /// <param name="ignoreTarget">Bỏ qua vị trí mục tiêu, dịch đến phía trước tối đa theo thông số của kỹ năng</param>
        /// <param name="callBack">Sự kiện khi hoàn tất</param>
        private static UnityEngine.Vector2 DoSkillBlinkToTargetPosition(SkillLevelRef skill, GameObject caster, GameObject target, PropertyDictionary skillPd, bool ignoreTargetPos, Action callBack = null)
        {
            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            int range = skill.Data.AttackRadius;
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                range = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }
            if (range <= 0)
            {
                return casterPos;
            }

            /// Dịch đến vị trí chỉ định
            UnityEngine.Vector2 destPos;

            /// Nếu có mục tiêu thì đến chỗ mục tiêu
            if (target != null)
            {
                destPos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);

                /// Nếu bỏ qua vị trí mục tiêu và tiếp tục dịch đến phía trước
                if (ignoreTargetPos)
                {
                    UnityEngine.Vector2 dirVector = destPos - casterPos;
                    destPos = KTMath.FindPointInVectorWithDistance(casterPos, dirVector, range);
                }
            }
            /// Nếu không có mục tiêu thì căn hướng kỹ năng dịch
            else
            {
                destPos = KTMath.MoveTowardByDirection(casterPos, caster.CurrentDir, range);
            }
            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);
            /// Vị trí không có vật cản trên đường đi
            destPos = KTGlobal.FindLinearNoObsPoint(gameMap, casterPos, destPos, caster.CurrentCopyMapID);

            /// Vector chỉ hướng
            UnityEngine.Vector2 finalDirVector = destPos - casterPos;

            /// Hướng quay của đối tượng
            float rotationAngle = KTMath.GetAngle360WithXAxis(finalDirVector);
            caster.CurrentDir = KTMath.GetDirectionByAngle360(rotationAngle);

            /// Thời gian dịch
            float animationDuration = 0.2f;

            /// Nếu có thiết lập tốc độ
            if (skill.Data.Params[2] > 0)
            {
                int velocity = skill.Data.Params[2];
                float distance = UnityEngine.Vector2.Distance(casterPos, destPos);
                animationDuration = distance / velocity;
            }

            /// Gửi gói tin về Client thực hiện biểu diễn hiệu ứng
            KT_TCPHandler.SendBlinkToPosition(caster, (int) destPos.x, (int) destPos.y, animationDuration);

            /// Thực hiện đổi vị trí luôn
            caster.CurrentPos = new System.Windows.Point((int) destPos.x, (int) destPos.y);
            /// Cập nhật vị trí đối tượng vào Map
            gameMap.Grid.MoveObject((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y, caster);
            /// Thực hiện kỹ năng tan biến
            KTSkillManager.BulletVanished(skill, caster, destPos, target, finalDirVector);

            /// Delay rồi mới thực thi Callback
            KTSkillManager.SetTimeout(0.2f, () =>
            {
                /// Thực hiện hàm Callback
                callBack?.Invoke();
            });

            return destPos;
        }

        /// <summary>
        /// Thực hiện kỹ năng dịch chuyển nhanh đến vị trí chỉ định, đồng thời gọi kỹ năng Tick trong quá trình bay
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="target">Mục tiêu</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        private static UnityEngine.Vector2 DoSkillBlinkToTargetPositionWithinFlySkill(SkillLevelRef skill, GameObject caster, GameObject target, PropertyDictionary skillPd)
        {
            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            int range = skill.Data.AttackRadius;
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                range = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }
            if (range <= 0)
            {
                return casterPos;
            }

            /// Cấu hình kỹ năng đi kèm không
            int flySkillID = skill.Data.BulletSkillID;
            /// Kiểm tra trong ProDict có kỹ năng đi kèm không
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_flyevent))
            {
                flySkillID = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_flyevent).nValue[0];
            }

            /// Kỹ năng bay
            SkillDataEx flySkill = KSkill.GetSkillData(flySkillID);

            /// Dịch đến vị trí chỉ định
            UnityEngine.Vector2 destPos;

            /// Nếu có mục tiêu thì đến chỗ mục tiêu
            if (target != null)
            {
                destPos = new UnityEngine.Vector2((float) target.CurrentPos.X, (float) target.CurrentPos.Y);
            }
            /// Nếu không có mục tiêu thì căn hướng kỹ năng dịch
            else
            {
                destPos = KTMath.MoveTowardByDirection(casterPos, caster.CurrentDir, range);
            }

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);
            /// Vị trí không có vật cản trên đường đi
            destPos = KTGlobal.FindLinearNoObsPoint(gameMap, casterPos, destPos, caster.CurrentCopyMapID);

            /// Vector chỉ hướng
            UnityEngine.Vector2 finalDirVector = destPos - casterPos;

            /// Hướng quay của đối tượng
            float rotationAngle = KTMath.GetAngle360WithXAxis(finalDirVector);
            caster.CurrentDir = KTMath.GetDirectionByAngle360(rotationAngle);

            /// Thời gian dịch
            float animationDuration = 0.2f;

            /// Nếu có thiết lập tốc độ
            if (skill.Data.Params[2] > 0)
            {
                int velocity = skill.Data.Params[2];
                float distance = UnityEngine.Vector2.Distance(casterPos, destPos);
                animationDuration = distance / velocity;
            }

            /// Gửi gói tin về Client thực hiện biểu diễn hiệu ứng
            KT_TCPHandler.SendBlinkToPosition(caster, (int) destPos.x, (int) destPos.y, animationDuration);

            /// Nếu có kỹ năng bay
            if (flySkill != null)
            {
                float tickDistance = 100f;
                float flyDistance = UnityEngine.Vector2.Distance(casterPos, destPos);
                float velocity = flyDistance / animationDuration;
                int totalPoints = (int) (flyDistance / tickDistance);
                UnityEngine.Vector2 tickDirVector = destPos - casterPos;
                UnityEngine.Vector2 lastTickPos = casterPos;
                for (int i = 1; i <= totalPoints; i++)
                {
                    UnityEngine.Vector2 tickPos = KTMath.FindPointInVectorWithDistance(lastTickPos, tickDirVector, tickDistance * i);
                    float time = UnityEngine.Vector2.Distance(lastTickPos, tickPos) / velocity;
                    KTSkillManager.SetTimeout(time, () => {
                        if (flySkill != null)
                        {
                            KTSkillManager.DoCallChildSkill(skill, flySkillID, caster, tickPos, null, target, tickDirVector);
                        }
                    });
                }
            }

            /// Thực hiện đổi vị trí luôn
            caster.CurrentPos = new System.Windows.Point((int) destPos.x, (int) destPos.y);
            /// Cập nhật vị trí đối tượng vào Map
            gameMap.Grid.MoveObject((int) caster.CurrentPos.X, (int) caster.CurrentPos.Y, caster);
            ///// Thực hiện gọi hàm cập nhật di chuyển
            //if (caster is KPlayer)
            //{
            //    ClientManager.DoSpriteMapGridMove(caster as KPlayer);
            //}
            /// Thực hiện kỹ năng tan biến
            KTSkillManager.BulletVanished(skill, caster, destPos, target, finalDirVector);

            return destPos;
        }

        /// <summary>
        /// Thực hiện kỹ năng khinh công theo hướng hiện tại
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="caster">Đối tượng xuất chiêu</param>
        /// <param name="skillPd">ProDict kỹ năng</param>
        private static UnityEngine.Vector2 DoSkillFlyToPosition(SkillLevelRef skill, GameObject caster, PropertyDictionary skillPd)
        {
            /// Vị trí hiện tại của đối tượng xuất chiêu
            UnityEngine.Vector2 casterPos = new UnityEngine.Vector2((float) caster.CurrentPos.X, (float) caster.CurrentPos.Y);

            int range = skill.Data.AttackRadius;
            /// Nếu có cự ly thi triển của kỹ năng
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                range = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            if (range <= 0)
            {
                return casterPos;
            }

            /// Dịch đến vị trí chỉ định
            UnityEngine.Vector2 destPos = KTMath.MoveTowardByDirection(casterPos, caster.CurrentDir, range);

            /// Bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(caster.CurrentMapCode);
            /// Vị trí không có vật cản trên đường đi
            destPos = KTGlobal.FindLinearNoObsPoint(gameMap, casterPos, destPos, caster.CurrentCopyMapID);

            /// Vector chỉ hướng
            UnityEngine.Vector2 finalDirVector = destPos - casterPos;

            /// Hướng quay của đối tượng
            float rotationAngle = KTMath.GetAngle360WithXAxis(finalDirVector);
            caster.CurrentDir = KTMath.GetDirectionByAngle360(rotationAngle);

            /// Tốc độ bay
            float velocity = KTGlobal.MoveSpeedToPixel(caster.GetCurrentRunSpeed());
            /// Thời gian dịch
            float animationDuration = UnityEngine.Vector2.Distance(casterPos, destPos) / velocity;

            /// Cập nhật động tác hiện tại của đối tượng
            caster.m_eDoing = KE_NPC_DOING.do_jump;

            /// Gửi gói tin về Client thực hiện biểu diễn hiệu ứng
            KT_TCPHandler.SendFlyToPosition(caster, (int) destPos.x, (int) destPos.y, animationDuration);

            /// Giảm chút thời gian để Client không bị Rollback
            animationDuration -= 0.5f;

            /// Thực hiện Blink
            caster.BlinkTo((int) destPos.x, (int) destPos.y, animationDuration, null, () => {
                /// Thực hiện kỹ năng tan biến
                KTSkillManager.BulletVanished(skill, caster, destPos, null, finalDirVector);

                /// Cập nhật động tác hiện tại của đối tượng
                caster.m_eDoing = KE_NPC_DOING.do_stand;
            });

            return destPos;
        }
    }
}
