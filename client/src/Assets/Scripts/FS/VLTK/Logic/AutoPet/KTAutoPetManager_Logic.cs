using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using FS.VLTK.Entities;
using FS.VLTK.Entities.Config;
using FS.VLTK.Network;
using GameServer.VLTK.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Thực thi Logic Auto Pet
    /// </summary>
    public partial class KTAutoPetManager
    {
        private GSprite _Target;
        /// <summary>
        /// Mục tiêu nhắm tới
        /// </summary>
        public GSprite Target
        {
            get
            {
                return this._Target;
            }
            set
            {
                /// Nếu trùng với mục tiêu hiện tại
                if (value != null && value == this._Target)
                {
                    /// Bỏ qua
                    return;
                }
                
                /// Ngừng đuổi mục tiêu
                this.StopChaseTarget();
                /// Ngừng di chuyển
                this.Pet?.StopMove();

                /// Lưu lại
                this._Target = value;
            }
        }

        /// <summary>
        /// ID kỹ năng dùng lần trước
        /// </summary>
        public int LastUseSkillID { get; set; }

        /// <summary>
        /// Thời điểm dùng kỹ năng lần trước
        /// </summary>
        public long LastUseSkillTick { get; set; }

        /// <summary>
        /// Thời điểm dùng kỹ năng không ảnh hưởng tốc đánh lần trước
        /// </summary>
        public long LastUseSkillNoAffectAtkSpeedTick { get; set; }

        /// <summary>
        /// Đang đợi dùng kỹ năng
        /// </summary>
        public bool IsWaitingToUseSkill { get; set; } = false;

        /// <summary>
        /// Danh sách kỹ năng sử dụng của Pet
        /// </summary>
        private readonly Dictionary<int, long> petUsedSkillTicks = new Dictionary<int, long>();

        /// <summary>
        /// Đã kết thúc thực thi động tác xuất chiêu chưa
        /// </summary>
        private bool FinishedUseSkillAction
        {
            get
            {
                /// Nếu đang đợi dùng kỹ năng thì bỏ qua
                if (this.IsWaitingToUseSkill)
                {
                    return false;
                }

                /// Nếu vừa dùng kỹ năng không ảnh hưởng bởi tốc đánh
                if (KTGlobal.GetCurrentTimeMilis() - this.LastUseSkillNoAffectAtkSpeedTick < 100)
                {
                    return false;
                }

                /// Tốc độ xuất chiêu hệ ngoại công hiện tại
                int attackSpeed = this.Pet.AttackSpeed;
                /// Tốc độ xuất chiêu hệ nội công hiện tại
                int castSpeed = this.Pet.CastSpeed;

                /// Tổng thời gian
                float frameDuration = 0f;
                /// Kỹ năng lần trước
                if (Loader.Loader.Skills.TryGetValue(this.LastUseSkillID, out SkillDataEx skillData))
                {
                    /// Kỹ năng nội hay ngoại
                    bool isPhysical = skillData.IsPhysical;
                    frameDuration = KTGlobal.AttackSpeedToFrameDuration(isPhysical ? attackSpeed : castSpeed);
                }

                /// Trả ra kết quả
                return KTGlobal.GetCurrentTimeMilis() - this.LastUseSkillTick >= frameDuration * 1000;
            }
        }

        /// <summary>
        /// Luồng thực hiện đuổi theo mục tiêu
        /// </summary>
        private Coroutine chaseTargetCoroutine;

        /// <summary>
        /// Luồng thực hiện quay về với chủ nhân
        /// </summary>
        private Coroutine backToOwnerCoroutine;

        /// <summary>
        /// Kỹ năng có đang trong trạng thái Cooldown
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        private bool IsSkillCooldown(int skillID)
        {
            /// Thời gian phục hồi kỹ năng
            int skillCooldownTime = this.GetSkillCooldown(skillID);

            /// Thời gian phục hồi kỹ năng này
            if (!this.petUsedSkillTicks.TryGetValue(skillID, out long lastUsedTick) || KTGlobal.GetCurrentTimeMilis() - lastUsedTick >= skillCooldownTime)
            {
                /// Không Cooldown
                return false;
            }

            /// Toác
            return true;
        }

        /// <summary>
        /// Trả về thời gian phục hồi kỹ năng không
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        private int GetSkillCooldown(int skillID)
        {
            /// Không có pet
            if (this.Pet == null || this.Pet.PetData == null)
            {
                /// Toác
                return -1;
            }
            /// Thông tin kỹ năng
            if (!Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
            {
                /// Toác
                return -1;
            }
            /// Cấp 0
            if (!this.Pet.PetData.Skills.TryGetValue(skillID, out int level) || level <= 0 || level >= skillData.Properties.Count)
            {
                /// Toác
                return -1;
            }
            /// Trả về kết quả
            return SkillManager.GetSkillCooldown(skillData, level);
        }

        /// <summary>
        /// Đánh dấu lại thời điểm dùng kỹ năng của pet
        /// </summary>
        /// <param name="skillID"></param>
        public void PetUseSkillSuccess(int skillID)
        {
            /// Thêm vào danh sách
            this.petUsedSkillTicks[skillID] = KTGlobal.GetCurrentTimeMilis();
        }

        /// <summary>
        /// Thực thi Logic Auto pet
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoLogic()
        {
            /// Vị trí trước đó của chủ nhân
            Vector2 lastOwnerPos = Vector2.zero;

            /// Đợi
            WaitForSeconds wait = new WaitForSeconds(0.2f);
            /// Lặp liên tục
            while (true)
            {
                /// Nếu không có chủ nhân
                if (this.Owner == null)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }
                /// Nếu không có pet
                else if (this.Pet == null)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }
                /// Nếu đang đợi dùng kỹ năng
                else if (this.IsWaitingToUseSkill)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }
                /// Nếu không thể dùng kỹ năng
                else if (!this.Pet.CanDoLogic)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }
                /// Nếu đang di chuyển
                else if (this.Pet.IsMoving)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }
                /// Nếu chưa kết thúc động tác ra chiêu
                else if (!this.FinishedUseSkillAction)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }

                /// Nếu vị trí của chủ nhân thay đổi
                if (Vector2.Distance(lastOwnerPos, this.Owner.PositionInVector2) > 10f)
                {
                    /// Nếu đang đứng chơi hoặc khoảng cách quá xa so với chủ nhân
                    if (this.Target == null || !SkillManager.IsValidTarget(this.Target) || Vector2.Distance(this.Owner.PositionInVector2, this.Pet.PositionInVector2) >= KTAutoPetManager.ContinueWorkingOnDistanceToOwner)
                    {
                        /// Cập nhật vị trí mới của chủ nhân
                        lastOwnerPos = this.Owner.PositionInVector2;
                        /// Ngừng đuổi mục tiêu
                        this.StopChaseTarget();
                        /// Chạy đến bên cạnh chủ nhân
                        this.BackToOwner();
                        /// Tiếp tục luồng
                        goto COROUTINE_YIELD;
                    }
                }

                /// Khoảng cách quá xa so với chủ nhân
                if (Vector2.Distance(this.Owner.PositionInVector2, this.Pet.PositionInVector2) >= KTAutoPetManager.MaxDistanceToOwner)
                {
                    /// Ngừng đuổi mục tiêu
                    this.StopChaseTarget();
                    /// Chạy đến bên cạnh chủ nhân
                    this.BackToOwner();
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }

                /// Nếu đang đuổi mục tiêu
                if (this.chaseTargetCoroutine != null)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }

                /// ID kỹ năng được chọn
                int selectedSkillID = -1;
                /// Thời gian phục hồi lâu nhất
                int longestCooldown = -9999;
                /// Nếu có kỹ năng
                if (this.Pet.PetData.Skills != null)
                {
                    /// Duyệt danh sách kỹ năng
                    foreach (KeyValuePair<int, int> pair in this.Pet.PetData.Skills)
                    {
                        /// Thông tin kỹ năng
                        if (Loader.Loader.Skills.TryGetValue(pair.Key, out SkillDataEx skillData))
                        {
                            /// Nếu là kỹ năng bị động hoặc vòng sáng
                            if (skillData.Type == 4 || skillData.Type == 3)
                            {
                                /// Bỏ qua
                                continue;
                            }
                            /// Nếu kỹ năng đang cooldown
                            else if (this.IsSkillCooldown(pair.Key))
                            {
                                /// Bỏ qua
                                continue;
                            }

                            /// Thời gian phục hồi kỹ năng này
                            int skillCooldown = this.GetSkillCooldown(pair.Key);

                            /// Nếu nhỏ hơn thời gian phục hồi lâu nhất
                            if (skillCooldown <= longestCooldown)
                            {
                                /// Bỏ qua
                                continue;
                            }

                            /// Đánh dấu thời gian phục hồi lâu nhất
                            longestCooldown = skillCooldown;
                            /// Đánh dấu ID kỹ năng sẽ dùng
                            selectedSkillID = pair.Key;
                        }
                    }
                }

                /// Nếu không có kỹ năng
                if (selectedSkillID == -1)
                {
                    /// Chọn kỹ năng cơ bản đánh thường
                    selectedSkillID = Loader.Loader.PetConfig.BaseAttackSkillID;
                }

                /// Thực hiện dùng kỹ năng
                this.DoUseSkill(selectedSkillID);

                /// Nhãn tiếp tục luồng
                COROUTINE_YIELD:
                /// Đợi
                yield return wait;
            }
        }

        /// <summary>
        /// Thực hiện sử dụng kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="isSubPhrase"></param>
        private void DoUseSkill(int skillID, bool isSubPhrase = false)
        {
            /// Thông tin kỹ năng
            if (!Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
            {
                /// Toác
                return;
            }

            /// Ngừng StoryBoard
            this.Pet.StopMove();

            /// Nếu là kỹ năng không cần mục tiêu
            if (SkillManager.IsSkillNoNeedTarget(skillData))
            {
                /// Đánh dấu đang đợi dùng kỹ năng
                this.IsWaitingToUseSkill = true;
                /// Cập nhật thông tin dùng kỹ năng thành công
                this.PetUseSkillSuccess(skillID);
                /// Gửi yêu cầu dùng kỹ năng
                KT_TCPHandler.SendPetUseSkill(this.Pet.PetData.ID, skillID, -1, (int) this.Pet.Direction, this.Pet.PosX, this.Pet.PosY);
                /// Gửi yêu cầu dùng luôn
                return;
            }

            /// ProDict của kỹ năng
            PropertyDictionary skillPd;
            /// Nếu kỹ năng không tồn tại trong danh sách tức là kỹ năng cơ bản
            if (this.Pet.PetData.Skills == null || !this.Pet.PetData.Skills.TryGetValue(skillID, out int skillLevel))
            {
                /// Mặc định cấp 1
                skillLevel = 1;
            }
            skillPd = skillData.Properties[(byte) skillLevel];

            /// Phạm vi tấn công của kỹ năng
            int skillCastRange = skillData.AttackRadius;
            if (skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_attackradius))
            {
                skillCastRange = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_attackradius).nValue[0];
            }

            /// Mục tiêu
            GSprite target = this.Target;

            /// Nếu trước đó đã có mục tiêu, thì kiểm tra xem mục tiêu đó còn tồn tại không
            bool targetMustBeAlive = skillData.TargetType != "revivable";
            bool targetMustBeEnemy = skillData.TargetType != "revivable";
            /// Nếu mục tiêu cũ không tồn tại
            if (!SkillManager.IsValidTarget(target, targetMustBeEnemy, targetMustBeAlive))
            {
                /// Đánh dấu Target NULL
                target = null;
            }

            /// Nếu có mục tiêu
            if (target != null)
            {
                /// Thiết lập chọn mục tiêu cho pet
                if (targetMustBeEnemy)
                {
                    KTAutoPetManager.Instance.Target = target;
                }

                /// Vị trí hiện tại của pet
                Vector2 casterPos = this.Pet.PositionInVector2;

                /// Khoảng cách đến chỗ mục tiêu
                float distanceToTarget = Vector2.Distance(casterPos, target.PositionInVector2);

                /// Nếu mục tiêu nằm ngoài phạm vi đánh của kỹ năng thì tiến hành di chuyển đến chỗ mục tiêu
                if (distanceToTarget > skillCastRange + (isSubPhrase ? 100 : 0))
                {
                    /// Kiểm tra nếu pet đang bị khóa, không thể di chuyển
                    if (!this.Pet.CanPositiveMove)
                    {
                        /// Đánh dấu đang đợi dùng kỹ năng
                        this.IsWaitingToUseSkill = false;
                    }

                    /// Thực hiện đuổi mục tiêu
                    this.ChaseTarget(target, skillCastRange - 20, () => {
                        /// Gọi lại hàm sử dụng kỹ năng
                        this.DoUseSkill(skillID, true);
                    });
                }
                /// Nếu mục tiêu đã nằm trong phạm vi thì tiến hành sử dụng kỹ năng
                else
                {
                    /// Nếu vượt quá phạm vi thiết lập của kỹ năng thì xuất chiêu ở khoảng không
                    if (distanceToTarget > skillCastRange)
                    {
                        /// Bỏ đánh dấu mục tiêu
                        this.Target = null;
                    }

                    /// Đánh dấu đang đợi dùng kỹ năng
                    this.IsWaitingToUseSkill = true;

                    /// Cập nhật thông tin dùng kỹ năng thành công
                    this.PetUseSkillSuccess(skillID);
                    /// Gửi yêu cầu dùng kỹ năng
                    KT_TCPHandler.SendPetUseSkill(this.Pet.PetData.ID, skillID, target.RoleID, (int) this.Pet.Direction, this.Pet.PosX, this.Pet.PosY);
                }
            }
            /// Nếu không tìm thấy mục tiêu thì gửi yêu cầu dùng kỹ năng theo hướng
            else
            {
                /// Xóa mục tiêu được chọn
                this.Target = null;
            }
        }

        /// <summary>
        /// Ngừng đuổi theo mục tiêu
        /// </summary>
        private void StopChaseTarget()
        {
            /// Nếu đang có luồng đuổi mục tiêu
            if (this.chaseTargetCoroutine != null)
            {
                /// Ngừng luồng
                this.StopCoroutine(this.chaseTargetCoroutine);
                /// Thiết lập NULL
                this.chaseTargetCoroutine = null;
            }
        }

        /// <summary>
        /// Thực hiện đuổi theo mục tiêu
        /// </summary>
        /// <param name="target"></param>
        /// <param name="stopRadius"></param>
        /// <param name="complete"></param>
        private void ChaseTarget(GSprite target, int stopRadius, Action complete)
        {
            /// Ngừng luồng đuổi mục tiêu
            this.StopChaseTarget();
            /// Bắt đầu luồng mới
            this.chaseTargetCoroutine = this.StartCoroutine(this.DoChaseTarget(target, stopRadius, complete));
        }

        /// <summary>
        /// Thực hiện đuổi theo mục tiêu
        /// </summary>
        /// <param name="target"></param>
        /// <param name="stopRadius"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        private IEnumerator DoChaseTarget(GSprite target, int stopRadius, Action complete)
        {
            /// Bản thân
            GSprite pet = this.Pet;
            /// Thời gian nghỉ
            float deltaTime = 0.2f;
            /// Nghỉ đợi
            WaitForSeconds wait = new WaitForSeconds(deltaTime);

            /// Vị trí cũ của đối tượng
            Vector2 targetOldPos = Vector2.zero;

            /// Lặp liên tục
            while (true)
            {
                /// Vị trí của bản thân
                Vector2 leaderPos = pet.PositionInVector2;
                /// Vị trí của Target
                Vector2 targetPos = target.PositionInVector2;

                /// Khoảng cách đến mục tiêu
                float distanceToTarget = Vector2.Distance(leaderPos, targetPos);
                /// Nếu đủ gần
                if (distanceToTarget <= stopRadius)
                {
                    /// Thực thi sự kiện đuổi hoàn tất
                    complete?.Invoke();
                    /// Thoát lặp
                    break;
                }

                /// Nếu vị trí của đối tượng không đổi
                if (Vector2.Distance(targetOldPos, targetPos) <= 10f)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }
                /// Cập nhật vị trí của đối tượng
                targetOldPos = targetPos;

                /// Đợi
                bool waiting = true;

                /// Tìm đường
                this.FindPath(targetPos, () =>
                {
                    /// Bỏ đợi
                    waiting = false;
                }, () =>
                {
                    /// Bỏ đợi
                    waiting = false;
                });

                /// Chừng nào đang tạm ngưng thì thôi
                while (waiting)
                {
                    /// Bỏ qua Frame
                    yield return null;
                    /// Tiếp tục đợi
                    continue;
                }

                /// Tiếp tục luồng
                COROUTINE_YIELD:
                /// Nghỉ
                yield return wait;
            }

            /// Xóa luồng
            this.chaseTargetCoroutine = null;
        }

        /// <summary>
        /// Thực hiện tìm đường di chuyển
        /// </summary>
        /// <param name="toPos"></param>
        /// <param name="complete"></param>
        /// <param name="failed"></param>
        private void FindPath(Vector2 toPos, Action complete = null, Action failed = null)
        {
            /// Bản thân
            GSprite pet = this.Pet;
            /// Toác
            if (pet == null)
            {
                return;
            }

            /// Vị trí đầu cuối
            Vector2 fromVector = pet.PositionInVector2;
            Vector2 toVector = new Vector2((int) toPos.x, (int) toPos.y);

            /// Thực hiện tìm đường
            List<Vector2> paths = Global.Data.GameScene.FindPath(pet, ref fromVector, ref toVector);

            /// Nếu không có đường đi thì bỏ qua
            if (paths == null || paths.Count < 1)
            {
                /// Thực thi sự kiện
                failed?.Invoke();
                /// Thoát
                return;
            }

            /// Hàng đợi danh sách các điểm trên đường đi
            Queue<Vector2> queue = new Queue<Vector2>();
            /// Nạp tất cả các điểm vừa tìm được vào hàng đợi
            foreach (Vector2 pos in paths)
            {
                queue.Enqueue(pos);
            }

            /// Chuyển về dạng string
            string pathString = "";
            while (queue.Count > 0)
            {
                Vector2 node = queue.Dequeue();
                pathString += "|" + string.Format("{0}_{1}", (int) node.x, (int) node.y);
            }
            pathString = pathString.Substring(1);

            /// Thêm StoryBoard mới vào danh sách
            KTStoryBoard.Instance.AddOrUpdate(pet, pathString, complete);
        }

        /// <summary>
        /// Luồng thực hiện quay về với chủ nhân
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoBackToOwner()
        {
            /// Chọn vị trí bất kỳ xung quanh chủ nhân
            Vector2 randomPoint = KTMath.GetRandomPointAroundPos(Vector2.zero, KTAutoPetManager.IdleRadiusNearByOwner - 10);
            /// Nghỉ
            WaitForSeconds wait = new WaitForSeconds(0.2f);
            /// Lặp liên tục
            while (true)
            {
                /// Toác
                if (this.Pet == null || this.Owner == null)
                {
                    /// Thoát
                    break;
                }
                /// Nếu đã đến gần chủ nhân
                if (Vector2.Distance(this.Pet.PositionInVector2, this.Owner.PositionInVector2) <= KTAutoPetManager.IdleRadiusNearByOwner)
                {
                    /// Ngừng di chuyển
                    this.Pet.StopMove();
                    /// Thoát
                    break;
                }

                /// Vị trí đích
                Vector2 destPos = this.Owner.PositionInVector2 + randomPoint;

                /// Tìm đường đến chõ chủ nhân
                this.FindPath(destPos);

                /// Nghỉ
                yield return wait;
            }

            /// Hủy luồng
            this.backToOwnerCoroutine = null;
            /// Dừng đuổi mục tiêu
            this.StopChaseTarget();
        }

        /// <summary>
        /// Thực hiện quay về với chủ nhân
        /// </summary>
        private void BackToOwner()
        {
            /// Nếu đã tồn tại luồng
            if (this.backToOwnerCoroutine != null)
            {
                /// Ngừng luồng
                this.StopCoroutine(this.backToOwnerCoroutine);
            }
            /// Bắt đầu luồng mới
            this.backToOwnerCoroutine = this.StartCoroutine(this.DoBackToOwner());
        }
    }
}
