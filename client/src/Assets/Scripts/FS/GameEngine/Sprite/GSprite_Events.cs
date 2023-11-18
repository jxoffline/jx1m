using System;
using FS.GameEngine.Logic;
using static FS.VLTK.Entities.Enum;
using FS.VLTK.Logic;
using FS.VLTK;
using FS.VLTK.Loader;
using FS.VLTK.Entities.Config;

namespace FS.GameEngine.Sprite
{
    /// <summary>
    /// Các sự kiện của đối tượng
    /// </summary>
    public partial class GSprite
	{
        /// <summary>
        /// Thực hiện động tác đứng
        /// </summary>
        /// <param name="forceUpdate"></param>
        public void DoStand(bool forceUpdate)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Nếu hàng đợi rỗng, và là loại đứng mặc định
            if (!forceUpdate && this.QueueActions.Count <= 0 && this.IsDoingDefaultAction())
            {
                return;
            }

            /// Nếu chưa thực hiện xong động tác trước đó (khác đi bộ và chạy)
            if (this.doingAction != null && this.doingAction.Action != KE_NPC_DOING.do_run && this.doingAction.Action != KE_NPC_DOING.do_walk && !this.IsCurrentActionCompleted())
            {
                Action oldDone = this.doingAction.Done;
                this.doingAction.Done = () => {
                    oldDone?.Invoke();
                    this.DoStand(forceUpdate);
                };
                return;
            }

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Nếu buộc Update
            if (forceUpdate)
            {
                /// Thực thi động tác đứng
                this.AddAction(KE_NPC_DOING.do_stand, 0.5f);
            }
        }

        /// <summary>
        /// Thực hiện động tác đứng
        /// </summary>
        /// <param name="duration"></param>
        public void DoStand(float duration = -1, Action done = null)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Nếu hàng đợi rỗng, và là loại đứng mặc định
            if (this.QueueActions.Count <= 0 && this.IsDoingDefaultAction())
            {
                return;
            }

            /// Nếu chưa thực hiện xong động tác trước đó (khác đi bộ và chạy)
            if (this.doingAction != null && this.doingAction.Action != KE_NPC_DOING.do_run && this.doingAction.Action != KE_NPC_DOING.do_walk && !this.IsCurrentActionCompleted())
            {
                Action oldDone = this.doingAction.Done;
                this.doingAction.Done = () => {
                    oldDone?.Invoke();
                    this.DoStand(duration, done);
                };
                return;
            }

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Nếu không phải mặc định đứng
            if (duration != -1)
            {
                /// Thực thi động tác đứng
                this.AddAction(KE_NPC_DOING.do_stand, duration, null, done);
            }
        }

        /// <summary>
        /// Thực hiện động tác chạy
        /// </summary>
        public void DoRun()
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Nếu đang thực hiện động tác chạy thì bỏ qua
            if (this.doingAction != null && this.doingAction.Action == KE_NPC_DOING.do_run)
            {
                return;
            }

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác chạy
            this.AddAction(KE_NPC_DOING.do_run, -1);
        }

        /// <summary>
        /// Thực hiện động tác đi bộ
        /// </summary>
        public void DoWalk()
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Nếu đang thực hiện động tác chạy thì bỏ qua
            if (this.doingAction != null && this.doingAction.Action == KE_NPC_DOING.do_walk)
            {
                return;
            }

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác chạy
            this.AddAction(KE_NPC_DOING.do_walk, -1);
        }

        /// <summary>
        /// Thực thi động tác tấn công liên tiếp
        /// </summary>
        /// <param name="count"></param>
        public void DoAttackMultipleTimes(int count)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thời gian thực hiện động tác
            float framePlaySpeed = (KTGlobal.MaxAttackActionDuration - KTGlobal.AttackSpeedAdditionDuration) / count;

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác tương ứng
            this.AddAction(KE_NPC_DOING.do_manyattack, framePlaySpeed, null, null, count);
        }

        /// <summary>
        /// Thực thi động tác tấn công liên tiếp
        /// </summary>
        /// <param name="count"></param>
        public void DoSpecialAttackMultipleTimes(int count)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thời gian thực hiện động tác
            float framePlaySpeed = (KTGlobal.MaxAttackActionDuration - KTGlobal.AttackSpeedAdditionDuration) / count;

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác tương ứng
            this.AddAction(KE_NPC_DOING.do_runattackmany, framePlaySpeed, null, null, count);
        }

        /// <summary>
        /// Thực thi động tác tấn công thường
        /// </summary>
        /// <param name="isPhysical"></param>
        /// <param name="isSkillNoAffectAtkSpeed"></param>
        public void DoNormalAttack(bool isPhysical, bool isSkillNoAffectAtkSpeed)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thời gian thực hiện động tác
            float framePlaySpeed = KTGlobal.AttackSpeedToFrameDuration(isPhysical ? this.AttackSpeed : this.CastSpeed) - KTGlobal.AttackSpeedAdditionDuration;
            /// Nếu là kỹ năng không ảnh hưởng tốc đánh
            if (isSkillNoAffectAtkSpeed)
			{
                framePlaySpeed = 0.2f;
			}

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác tương ứng
            this.AddAction(KE_NPC_DOING.do_attack, framePlaySpeed);
        }

        /// <summary>
        /// Thực hiện tấn công đặc biệt
        /// </summary>
        /// <param name="isPhysical"></param>
        /// <param name="isSkillNoAffectAtkSpeed"></param>
        public void DoSpecialAttack(bool isPhysical, bool isSkillNoAffectAtkSpeed)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thời gian thực hiện động tác
            float framePlaySpeed = KTGlobal.AttackSpeedToFrameDuration(isPhysical ? this.AttackSpeed : this.CastSpeed) - KTGlobal.AttackSpeedAdditionDuration;
            /// Nếu là kỹ năng không ảnh hưởng tốc đánh
            if (isSkillNoAffectAtkSpeed)
            {
                framePlaySpeed = 0.2f;
            }

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác tương ứng
            this.AddAction(KE_NPC_DOING.do_rushattack, framePlaySpeed);
        }

        /// <summary>
        /// Thực thi động tác chạy tấn công
        /// </summary>
        public void DoRunAttack(float duration)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            float framePlaySpeed = duration;

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác tương ứng
            this.AddAction(KE_NPC_DOING.do_runattack, framePlaySpeed);
        }

        /// <summary>
        /// Thực thi động tác xuất chiêu nội công
        /// </summary>
        /// <param name="isPhysical"></param>
        public void DoMagic(bool isPhysical, bool isSkillNoAffectAtkSpeed)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thời gian thực hiện động tác
            float framePlaySpeed = KTGlobal.AttackSpeedToFrameDuration(isPhysical ? this.AttackSpeed : this.CastSpeed) - KTGlobal.AttackSpeedAdditionDuration;
            /// Nếu là kỹ năng không ảnh hưởng tốc đánh
            if (isSkillNoAffectAtkSpeed)
            {
                framePlaySpeed = 0.2f;
            }

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác tương ứng
            this.AddAction(KE_NPC_DOING.do_magic, framePlaySpeed);
        }

        /// <summary>
        /// Thực thi động tác bị thương
        /// </summary>
        public void DoHurt(float duration)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thời gian thực hiện động tác
            float framePlaySpeed = duration;

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác tương ứng
            this.AddAction(KE_NPC_DOING.do_hurt, framePlaySpeed);
        }

        /// <summary>
        /// Thực hiện khinh công
        /// </summary>
        /// <param name="duration"></param>
        public void DoFly(float duration)
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thời gian thực hiện động tác
            float framePlaySpeed = duration;

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác tương ứng
            this.AddAction(KE_NPC_DOING.do_jump, framePlaySpeed);
        }

        /// <summary>
        /// Thực hiện động tác ngồi
        /// </summary>
        public void DoSit()
        {
            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                return;
            }

            /// Thời gian thực hiện động tác
            float framePlaySpeed = KTGlobal.SitActionTime;

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Thực hiện động tác ngồi
            this.AddAction(KE_NPC_DOING.do_sit, framePlaySpeed);
        }

        /// <summary>
        /// Điều khiển từ bên ngoài thực hiện động tác chết
        /// </summary>
        public void DoDeath()
        {
            /// Nếu là xe tiêu
            if (this.SpriteType == GSpriteTypes.TraderCarriage)
            {
                this.TraderCarriageData.HP = 0;
                this.ComponentMonster.UpdateHP();

                /// Xóa khỏi Cache
                Global.Data.TraderCarriages.Remove(this.RoleID);
            }
            /// Nếu là Pet
            else if (this.SpriteType == GSpriteTypes.Pet)
            {
                this.PetData.HP = 0;
                this.ComponentMonster.UpdateHP();

                /// Xóa khỏi Cache
                Global.Data.SystemPets.Remove(this.RoleID);

                /// Nếu là Pet của bản thân
                if (Global.Data.RoleData.CurrentPetID == this.RoleID - (int) VLTK.Entities.Enum.ObjectBaseID.Pet)
                {
                    /// Xóa ID pet đang tham chiến
                    Global.Data.RoleData.CurrentPetID = -1;
                }
                
            }
            /// Nếu là quái
            else if (this.SpriteType == GSpriteTypes.Monster)
            {
                this.MonsterData.HP = 0;
                this.ComponentMonster.UpdateHP();
            }

            /// Nếu đã chết thì bỏ qua
            if (this.IsDeath)
            {
                //KTDebug.LogError("Already dead, nothing to do!");
                return;
            }

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Mở khóa Render
            this.UnlockRender();

            /// Thực thi động tác chết
            this.ProcessDie();

            /// Đánh dấu đối tượng đã chết
            this.IsDeath = true;

            /// Cập nhật thời gian Delay khi chết
            this.deathDelay = Global.GetMyTimer();

            /// Thực hiện chết
            this.ProcessDead();

            /// Ngừng di chuyển
            this.StopMove();
        }

        /// <summary>
        /// Điều khiển từ bên ngoài thực hiện động tác hồi sinh
        /// </summary>
        public void DoRevive()
        {
            /// Nếu chưa chết thì bỏ qua
            if (!this.IsDeath)
            {
                return;
            }

            /// Đánh dấu đối tượng đã chết
            this.IsDeath = false;

            /// Cập nhật thời gian Delay khi chết
            this.deathDelay = 0;

            /// Xóa toàn bộ động tác
            this.ClearActions();

            /// Mở khóa Render
            this.UnlockRender();

            /// Thực hiện động tác đứng
            this.AddAction(KE_NPC_DOING.do_stand, -1, null, null);

            /// Dừng thực thi StoryBoard
            this.StopMove();
        }
    }
}
