using FS.VLTK.Factory.ObjectsManager;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng người chơi ở khung soi nhân vật
    /// </summary>
    public partial class CharacterPreview : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public RoleDataMini Data { get; set; }

        private Direction _Direction = Direction.NONE;
        /// <summary>
        /// Hướng quay
        /// </summary>
        public Direction Direction
        {
            get
            {
                return this._Direction;
            }
            set
            {
                if (value == this._Direction)
                {
                    return;
                }

                this._Direction = value;
                this.ResumeCurrentAction();
            }
        }

        /// <summary>
        /// Camera tham chiếu
        /// </summary>
        public Camera ReferenceCamera { get; private set; }

        /// <summary>
        /// Sự kiện OnStart
        /// </summary>
        public Action OnStart { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            KTRolePreviewManager.Instance.AddElement(this);
            this.InitAction();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitCamera();
            this.OnStart?.Invoke();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy
        /// </summary>
        private void OnDestroy()
        {
            KTRolePreviewManager.Instance.RemoveElement(this);
            this.DestroyCamera();
        }
        #endregion
    }
}
