using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using FS.VLTK.Logic.Settings;
using FS.VLTK.Utilities.UnityComponent;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Control.Component.Skill
{
    /// <summary>
    /// Quản lý hiệu ứng
    /// </summary>
    public partial class ExplodeEffect
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
        private new BulletExplodeEffectAnimation2D animation = null;
        #endregion

        #region Private methods
        /// <summary>
        /// Khởi tạo động tác
        /// </summary>
        private void InitActions()
        {
            
        }

        /// <summary>
        /// Thiết lập Sorting Order
        /// </summary>
        private void SortingOrderHandler()
        {
            Vector2 currentPos = this.gameObject.transform.localPosition;
            this.gameObject.transform.localPosition = new Vector3(currentPos.x, currentPos.y, currentPos.y / 10000);
        }
        #endregion

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
            /// Nếu thiết lập không hiện hiệu ứng nổ
            if (KTSystemSetting.HideSkillExplodeEffect)
            {
                return;
            }

            this.animation.Resume();
        }

        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine actionCoroutine;

        /// <summary>
        /// Tiếp tục thực hiện trạng thái hiện tại
        /// </summary>
        private void ContinueCurrentState()
        {
            /// Thực thi thiết lập
            this.ExecuteSetting();
            /// Nếu sau khi thực thi thiết lập trả ra kết quả ẩn đối tượng thì bỏ qua
            if (this.lastHideRole)
            {
                this.Destroy();
                return;
            }

            this.RefreshAction();
            this.Play();
        }

        /// <summary>
        /// Làm mới động tác
        /// </summary>
        public void RefreshAction()
		{
            this.animation.ResID = this.ResID;
            /// Dịch vị trí gốc
            if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
            {
                this.Model.transform.localPosition = new Vector2(resData.ExplodePosX, resData.ExplodePosY);
            }
            else
            {
                this.Model.transform.localPosition = Vector2.zero;
            }
        }

        /// <summary>
        /// Thực hiện hiệu ứng
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

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }

            /// Nếu không có mục tiêu thì Play tại vị trí chỉ định
            if (this.Target == null)
            {
                this.gameObject.transform.localPosition = this.Position;
            }

            /// <summary>
            /// Thực hiện chạy hiệu ứng
            /// </summary>
            IEnumerator DoPlay()
            {
                if (this.Delay > 0)
				{
                    yield return new WaitForSeconds(this.Delay);
				}

                /// Thời gian thực thi hiệu ứng
                float animDuration = 0.5f;
                /// Thông tin Bullet
                if (Loader.Loader.BulletActionSetXML.ResDatas.TryGetValue(this.ResID, out BulletActionSetXML.BulletResData resData))
                {
                    animDuration = resData.ExplodeAnimDuration / 18f;
                    if (animDuration < 0.1f)
                    {
                        animDuration = 0.1f;
                    }
                }

                yield return this.animation.DoActionAsync(animDuration);
                yield return new WaitForSeconds(animDuration);
                this.Destroy();
            }
            this.actionCoroutine = this.StartCoroutine(DoPlay());
        }

        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        public void Destroy()
        {
            this.StopAllCoroutines();
            this.ResID = -1;
            this.Position = default;
            this.Target = null;
            this.Delay = 0;
            this.isReady = false;
            this.Destroyed?.Invoke();
            this.Destroyed = null;
            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
    }
}
