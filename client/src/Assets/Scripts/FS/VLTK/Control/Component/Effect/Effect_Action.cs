using UnityEngine;
using FS.VLTK.Utilities.UnityComponent;
using FS.VLTK.Loader;
using System.Collections;
using FS.VLTK.Factory;
using FS.VLTK.Logic.Settings;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý động tác hiệu ứng
    /// </summary>
    public partial class Effect
    {
        #region Define
        /// <summary>
        /// Thân đối tượng
        /// </summary>
        [SerializeField]
        private GameObject Body;

        /// <summary>
        /// Gốc đối tượng
        /// </summary>
        [SerializeField]
        private GameObject Model;

        /// <summary>
        /// Đối tượng thực hiện động tác
        /// </summary>
        private new EffectAnimation2D animation = null;
        #endregion

        /// <summary>
        /// Thiết lập Sorting Order
        /// </summary>
        public void SortingOrderHandler()
        {
            Vector2 currentPos = this.gameObject.transform.localPosition;
            this.gameObject.transform.localPosition = new Vector3(currentPos.x, currentPos.y, currentPos.y / 10000);
        }


        /// <summary>
        /// Thiết lập hiển thị Model
        /// </summary>
        /// <param name="isVisible"></param>
        private void SetModelVisible(bool isVisible)
        {
            this.Body.gameObject.SetActive(isVisible);
        }


        /// <summary>
        /// Tạm dừng thực hiện tất cả động tác
        /// </summary>
        public void PauseAllActions()
        {
            this.animation.Pause();
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác
        /// </summary>
        public void ResumeActions()
        {
            /// Thực thi thiết lập
            this.ExecuteSetting();
            /// Nếu sau khi thực thi thiết lập trả ra kết quả ẩn đối tượng thì bỏ qua
            if (this.lastHideRole)
            {
                return;
            }

            /// Nếu có thiết lập ẩn hiệu ứng
            if ((this.Type == EffectType.Buff && KTSystemSetting.HideSkillBuffEffect) || (this.Type == EffectType.CastEffect && KTSystemSetting.HideSkillCastEffect))
            {
                return;
            }

            this.animation.Resume();
        }


        /// <summary>
        /// Luồng thực thi hiệu ứng Async
        /// </summary>
        private Coroutine actionCoroutine;

        /// <summary>
        /// Cập nhật động tác
        /// </summary>
        public void RefreshAction()
		{
            this.animation.Data = this.Data;
            /// Thiết lập vị trí
            this.Model.transform.localPosition = new Vector2(this.Data.PosX, this.Data.PosY);
		}

        /// <summary>
        /// Tiếp tục thực hiện động tác hiện tại
        /// </summary>
        public void Play()
        {
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu có thiết lập ẩn hiệu ứng
            if ((this.Type == EffectType.Buff && KTSystemSetting.HideSkillBuffEffect) || (this.Type == EffectType.CastEffect && KTSystemSetting.HideSkillCastEffect))
            {
                return;
            }

            this.DoPlayAnimation();

            if (this.animation != null && this.animation.IsPausing)
            {
                this.ResumeActions();
            }
        }

        /// <summary>
        /// Thực hiện hiệu ứng
        /// </summary>
        private void DoPlayAnimation()
        {
            if (!this.gameObject)
            {
                return;
            }
            else if (!this.gameObject.activeSelf)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(1f, (this.Data.Loop), 0f, false, () => {
                this.Destroy();
            }));
        }

        /// <summary>
        /// Hàm này gọi đến ngay khi đối tượng được tạo ra
        /// </summary>
        private void InitAction()
        {
            
        }

        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        public void Destroy()
        {
            this.StopAllCoroutines();
            this.Alpha = 1;
            this._Owner = null;
            this.ownerPlayer = null;
            this.ownerMonster = null;
            this.actionCoroutine = null;
            this.SetModelVisible(true);
            this.lastHideRole = false;
            this.Type = EffectType.None;
            this.Destroyed?.Invoke();
            this.Destroyed = null;
            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
    }
}
