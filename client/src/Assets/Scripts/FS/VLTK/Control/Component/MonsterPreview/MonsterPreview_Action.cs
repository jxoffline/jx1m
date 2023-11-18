using FS.GameEngine.Logic;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý động tác
    /// </summary>
    public partial class MonsterPreview
    {
        #region Define
        /// <summary>
        /// Thân
        /// </summary>
        [SerializeField]
        private GameObject Body;

        /// <summary>
        /// Luồng thực thi hiệu ứng Async
        /// </summary>
        private Coroutine actionCoroutine;

        /// <summary>
        /// Đối tượng thực hiện động tác nhân vật
        /// </summary>
        private new MonsterAnimation2D animation = null;
        #endregion

        /// <summary>
        /// Cập nhật ResID
        /// </summary>
        public void UpdateResID()
        {
            this.animation.ResID = this.ResID;

            /// Thông tin Res
            if (Loader.Loader.MonsterActionSetXML.Monsters.TryGetValue(this.ResID, out Entities.Config.MonsterActionSetXML.Component resData))
            {
                /// Chỉnh vị trí nếu bị lệch
                this.Body.transform.localPosition = new Vector2(resData.PosX, resData.PosY);
            }
        }

        /// <summary>
        /// Khởi tạo động tác
        /// </summary>
        private void InitAction()
        {
            this.animation = this.gameObject.GetComponent<MonsterAnimation2D>();
            this.animation.Body = this.Body.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Thực thi bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrame(int skip, Action callback)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callback?.Invoke();
        }

        /// <summary>
        /// Luồng thực hiện tải lại động tác
        /// </summary>
        private Coroutine reloadAnimationCoroutine = null;

        /// <summary>
        /// Tải lại động tác
        /// </summary>
        private void ReloadAnimation()
        {
            if (this.reloadAnimationCoroutine != null)
            {
                this.StopCoroutine(this.reloadAnimationCoroutine);
            }
            this.reloadAnimationCoroutine = this.StartCoroutine(this.ExecuteSkipFrame(1, () => {
                this.animation.Reload();
                this.reloadAnimationCoroutine = null;
            }));
        }

        /// <summary>
        /// Khởi tạo Camera
        /// </summary>
        private void InitCamera()
        {
            this.ReferenceCamera = GameObject.Instantiate<Camera>(Global.ObjectPreviewCameraPrefab);
            this.ReferenceCamera.transform.SetParent(Global.ObjectPreviewCameraPrefab.transform.parent, false);
            this.ReferenceCamera.targetTexture = new RenderTexture(512, 512, 16);
            this.ReferenceCamera.gameObject.SetActive(true);
            this.ReferenceCamera.transform.localPosition = new Vector3(this.gameObject.transform.localPosition.x, this.gameObject.transform.localPosition.y, this.ReferenceCamera.transform.localPosition.z);
        }

        /// <summary>
        /// Xóa đối tượng Camera
        /// </summary>
        private void DestroyCamera()
        {
            if (this.ReferenceCamera != null)
            {
                GameObject.Destroy(this.ReferenceCamera.gameObject);
            }
        }

        /// <summary>
        /// Tiếp tục thực hiện động tác hiện tại
        /// </summary>
        public void ResumeCurrentAction()
        {
            this.animation.ResID = this.ResID;
            this.InstantStand();
        }

        /// <summary>
        /// Đứng ngay lập tức
        /// </summary>
        private void InstantStand()
        {
            if (this.gameObject == null || !this.gameObject.activeSelf)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(MonsterActionType.NormalStand, this.Direction, 0.5f, int.MaxValue, 0f));
        }
    }
}
