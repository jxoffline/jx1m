using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using FS.VLTK.Utilities.UnityComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng điểm truyền tống
    /// </summary>
    public partial class Teleport
    {
        #region Define
        /// <summary>
        /// Thành phần cha chứa các bộ phận
        /// </summary>
        public GameObject ComponentRoot;

        /// <summary>
        /// Thân đối tượng
        /// </summary>
        public GameObject Body;

        /// <summary>
        /// Đối tượng thực hiện động tác nhân vật
        /// </summary>
        private new MonsterAnimation2D animation = null;
        #endregion

        /// <summary>
        /// Hàm này gọi đến ngay khi đối tượng được tạo ra
        /// </summary>
        private void InitAction()
        {
            this.ResumeCurrentAction();
        }


        /// <summary>
        /// Luồng thực thi hiệu ứng Async
        /// </summary>
        private Coroutine actionCoroutine;


        /// <summary>
        /// Thực hiện hiệu ứng
        /// </summary>
        public void ResumeCurrentAction()
        {
            if (this.animation.ResID == null)
            {
                this.animation.ResID = this.Data.ResID;
            }
            this.Stand();
        }

        /// <summary>
        /// Đứng ngay lập tức
        /// </summary>
        public void Stand()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            if (this.actionCoroutine != null)
            {
                this.StopCoroutine(this.actionCoroutine);
            }
            this.actionCoroutine = this.StartCoroutine(this.animation.DoActionAsync(Entities.Enum.MonsterActionType.NormalStand, Entities.Enum.Direction.DOWN, 1f, int.MaxValue, 0f));
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
        /// Thay đổi màu đối tượng
        /// </summary>
        /// <param name="color"></param>
        public void MixColor(Color color)
        {
            this.groupColor.Color = color;
        }

        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        public void Destroy()
        {
            this.DestroyUI();

            this.StopAllCoroutines();
            this.Data = new Entities.Object.TeleportData();
            this.MinimapIconSize = default;
            this.actionCoroutine = null;
            this.Destroyed?.Invoke();
            this.Destroyed = null;
            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
    }
}
