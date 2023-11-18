using FS.VLTK.Factory;
using FS.VLTK.Loader;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý động tác
    /// </summary>
    public partial class Item
    {
        #region Define
        /// <summary>
        /// Thân đối tượng
        /// </summary>
        [SerializeField]
        private GameObject Body;

        /// <summary>
        /// Đối tượng thực hiện động tác
        /// </summary>
        private new DropItemAnimation2D animation = null;
        #endregion

        /// <summary>
        /// Model của đối tượng
        /// </summary>
        public GameObject Model
        {
            get
            {
                return this.Body.transform.parent.gameObject;
            }
        }

        /// <summary>
        /// Luồng thực thi quay đối tượng
        /// </summary>
        private Coroutine rotateCoroutine;

        /// <summary>
        /// Luồng thực thi hiệu ứng ném lên trời và rơi xuống đất
        /// </summary>
        private Coroutine throwCoroutine;

        /// <summary>
        /// Khởi tạo động tác
        /// </summary>
        private void InitAction()
        {

        }

        /// <summary>
        /// Thiết lập Sorting Order
        /// </summary>
        public void SortingOrderHandler()
        {
            Vector2 currentPos = this.gameObject.transform.localPosition;
            this.gameObject.transform.localPosition = new Vector3(currentPos.x, currentPos.y, currentPos.y / 10000);
        }

        /// <summary>
        /// Cập nhật dữ liệu
        /// </summary>
        public void UpdateData()
        {
            this.animation.Data = this.ItemData;
        }

        /// <summary>
        /// Thực hiện động tác
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

            /// Tải Sprite
            this.animation.DoAction();

            if (this.rotateCoroutine != null)
            {
                this.StopCoroutine(this.rotateCoroutine);
            }
            this.rotateCoroutine = this.StartCoroutine(this.animation.DoRotateAsync(135, 0, 0.5f, 5));

            if (this.throwCoroutine != null)
            {
                this.StopCoroutine(this.throwCoroutine);
            }
            this.throwCoroutine = this.StartCoroutine(this.DoThrowAsync(70, 100, 0, 0.5f));
        }
        
        /// <summary>
        /// Thực thi hiệu ứng ném lên trời và rơi xuống đất
        /// </summary>
        /// <param name="startHeight"></param>
        /// <param name="skyHeight"></param>
        /// <param name="groundHeight"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator DoThrowAsync(int startHeight, int skyHeight, int groundHeight, float duration)
        {
            float throwUpDuration = duration / 3;
            float falldownDuration = duration - throwUpDuration;

            float firstPosX = this.gameObject.transform.localPosition.x;
            float firstPosY = this.gameObject.transform.localPosition.y;

            /// Pha ném lên
            float lifeTime = 0f;
            this.gameObject.transform.localPosition = new Vector2(firstPosX, firstPosY + startHeight);
            yield return null;
            while (true)
            {
                lifeTime += Time.deltaTime;
                if (lifeTime >= throwUpDuration)
                {
                    break;
                }

                float percent = lifeTime / throwUpDuration;

                float newHeight = startHeight + (skyHeight - startHeight) * percent;
                this.gameObject.transform.localPosition = new Vector2(firstPosX, firstPosY + newHeight);

                yield return null;
            }

            /// Pha rơi xuống
            lifeTime = 0f;
            this.gameObject.transform.localPosition = new Vector2(firstPosX, firstPosY + skyHeight);
            yield return null;
            while (true)
            {
                lifeTime += Time.deltaTime;
                if (lifeTime >= falldownDuration)
                {
                    break;
                }

                float percent = lifeTime / falldownDuration;

                float newHeight = skyHeight + (groundHeight - skyHeight) * percent;
                this.gameObject.transform.localPosition = new Vector2(firstPosX, firstPosY + newHeight);

                yield return null;
            }
        }

        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        public void Destroy()
        {
            this.DestroyUI();

            this.StopAllCoroutines();
            this.rotateCoroutine = null;
            this.throwCoroutine = null;
            this.Destroyed?.Invoke();

            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
    }
}
