using FS.VLTK.Logic.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Thực hiện động tác
    /// </summary>
    public partial class Monster
    {
        /// <summary>
        /// Thời gian thực thi động tác hiện tại
        /// </summary>
        private float currentActionFrameSpeed = 0f;

        /// <summary>
        /// Tham biến khác
        /// </summary>
        private int otherParam = 0;

        #region Đứng
        /// <summary>
        /// Đứng ngay lập tức
        /// </summary>
        public void Stand()
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.NormalStand, this.Direction, KTGlobal.StandActionTime, int.MaxValue, 0f));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = 0f;
            this.otherParam = 0;
        }
        #endregion

        #region Đứng tấn công
        /// <summary>
        /// Đứng ngay lập tức
        /// </summary>
        public void FightStand()
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.FightStand, this.Direction, KTGlobal.StandActionTime, int.MaxValue, 0f));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = 0f;
            this.otherParam = 0;
        }
        #endregion

        #region Chạy
        /// <summary>
        /// Thực hiện động tác di chuyển
        /// </summary>
        public void Run()
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            /// Tốc độ thực thi động tác
            float framePlaySpeed = KTGlobal.MoveSpeedToFrameDuration(this.RefObject.MoveSpeed);

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.Run, this.Direction, framePlaySpeed, int.MaxValue));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = 0f;
            this.otherParam = 0;
        }
        #endregion

        #region Tấn công
        /// <summary>
        /// Thực hiện động tác tấn công
        /// </summary>
        /// <param name="framePlaySpeed"></param>
        public void Attack(float framePlaySpeed)
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.NormalAttack, this.Direction, framePlaySpeed, 0, 0, true, () => {
                this.RefObject.DoStand(true);
            }));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = framePlaySpeed;
            this.otherParam = 0;
        }
        #endregion

        #region Tấn công đặc biệt
        /// <summary>
        /// Thực hiện động tác tấn công
        /// </summary>
        /// <param name="framePlaySpeed"></param>
        public void SpecialAttack(float framePlaySpeed)
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.CritAttack, this.Direction, framePlaySpeed, 0, 0, true, () => {
                this.RefObject.DoStand(true);
            }));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = framePlaySpeed;
            this.otherParam = 0;
        }
        #endregion

        #region Chạy tấn công
        /// <summary>
        /// Thực hiện động tác di chuyển nhanh đến phía mục tiêu
        /// </summary>
        /// <param name="framePlaySpeed"></param>
        public void RunAttack(float framePlaySpeed)
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.RunAttack, this.Direction, framePlaySpeed, int.MaxValue));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = framePlaySpeed;
            this.otherParam = 0;
        }
        #endregion

        #region Tấn công liên tục nhiều lần
        /// <summary>
        /// Thực hiện động tác tấn công số lần cố định liên tiếp không ảnh hưởng tốc đánh
        /// </summary>
        /// <param name="framePlaySpeed"></param>
        /// <param name="count"></param>
        public void AttackMultipleTimes(float framePlaySpeed, int count)
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }

            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.NormalAttack, this.Direction, framePlaySpeed, count, 0, true, () => {
                this.RefObject.DoStand(true);
            }));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = framePlaySpeed;
            this.otherParam = count;
        }
        #endregion

        #region Tấn công đặc biệt liên tục nhiều lần
        /// <summary>
        /// Thực hiện động tác tấn công số lần cố định liên tiếp không ảnh hưởng tốc đánh
        /// </summary>
        /// <param name="framePlaySpeed"></param>
        /// <param name="count"></param>
        public void SpecialAttackMultipleTimes(float framePlaySpeed, int count)
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }

            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.CritAttack, this.Direction, framePlaySpeed, count, 0, true, () => {
                this.RefObject.DoStand(true);
            }));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = framePlaySpeed;
            this.otherParam = count;
        }
        #endregion

        #region Thọ thương
        /// <summary>
        /// Thực hiện động tác bị thọ thương
        /// </summary>
        /// <param name="framePlaySpeed"></param>
        public void Hurt(float framePlaySpeed)
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu đối tượng đã chết
            if (this.RefObject.IsDeath)
            {
                return;
            }
            /// Nếu thiết lập không hiện NPC
            else if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.Wound, this.Direction, framePlaySpeed, 0, 0, false));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = framePlaySpeed;
            this.otherParam = 0;
        }
        #endregion

        #region Chết
        /// <summary>
        /// Thực hiện động tác chết
        /// </summary>
        public void Die()
        {
            this.ActivateTrailEffect(false);
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu thiết lập không hiện NPC
            if (KTSystemSetting.HideNPC)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.Die, this.Direction, KTGlobal.DieActionTime));

            /// Cập nhật thời gian thực thi động tác
            this.currentActionFrameSpeed = 0f;
            this.otherParam = 0;
        }
        #endregion
    }
}
