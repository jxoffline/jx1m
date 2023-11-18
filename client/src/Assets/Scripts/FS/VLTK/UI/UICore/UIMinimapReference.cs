using System.Collections;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using FS.GameEngine.Logic;
using FS.VLTK.Factory;
using System;
using System.Linq;

namespace FS.VLTK.UI.CoreUI
{
    /// <summary>
    /// Khung chiếu đối tượng trên bản đồ nhỏ
    /// </summary>
    public class UIMinimapReference : MonoBehaviour
    {
        /// <summary>
        /// Hiệu ứng trạng thái nhiệm vụ tương ứng
        /// </summary>
        [Serializable]
        private class QuestStateIconPrefab
        {
            /// <summary>
            /// Trạng thái nhiệm vụ
            /// </summary>
            public NPCTaskStates State;

            /// <summary>
            /// Đối tượng thực thi hiệu ứng
            /// </summary>
            public UIAnimatedSprite UIAnimatedSprite;
        }

        #region Define
        /// <summary>
        /// Tên đối tượng
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_Name;

        /// <summary>
        /// Ảnh đại diện ở Minimap
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UISprite_MinimapIcon;

        /// <summary>
        /// Tọa độ đặt Text (theo màn hình)
        /// </summary>
        [SerializeField]
        private Vector2 _TextOffset;

        /// <summary>
        /// Hiệu ứng trạng thái nhiệm vụ tương ứng
        /// </summary>
        [SerializeField]
        private QuestStateIconPrefab[] QuestStateIconPrefabs;
        #endregion

        #region Properties
        /// <summary>
        /// Tên đối tượng
        /// </summary>
        public string Name
        {
            get
            {
                return this.UIText_Name.text;
            }
            set
            {
                this.UIText_Name.text = value;
            }
        }

        /// <summary>
        /// Màu chữ tên đối tượng
        /// </summary>
        public Color NameColor
        {
            get
            {
                return this.UIText_Name.color;
            }
            set
            {
                this.UIText_Name.color = value;
            }
        }

        /// <summary>
        /// Hiển thị tên đối tượng
        /// </summary>
        public bool ShowName
        {
            get
            {
                return this.UIText_Name.gameObject.activeSelf;
            }
            set
            {
                this.UIText_Name.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Hiển thị Icon đối tượng
        /// </summary>
        public bool ShowIcon
        {
            get
            {
                return this.UISprite_MinimapIcon.gameObject.activeSelf;
            }
            set
            {
                this.UISprite_MinimapIcon.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Đường dẫn Bundle chứa ảnh
        /// </summary>
        public string BundleDir
        {
            get
            {
                return this.UISprite_MinimapIcon.BundleDir;
            }
            set
            {
                this.UISprite_MinimapIcon.BundleDir = value;
            }
        }

        /// <summary>
        /// Tên Atlas chứa ảnh
        /// </summary>
        public string AtlasName
        {
            get
            {
                return this.UISprite_MinimapIcon.AtlasName;
            }
            set
            {
                this.UISprite_MinimapIcon.AtlasName = value;
            }
        }

        /// <summary>
        /// Tên ảnh Icon đối tượng
        /// </summary>
        public string SpriteName
        {
            get
            {
                return this.UISprite_MinimapIcon.SpriteName;
            }
            set
            {
                this.UISprite_MinimapIcon.SpriteName = value;
                this.UpdateIcon();
            }
        }

        /// <summary>
        /// Tọa độ điểm đặt (tính theo màn hình)
        /// </summary>
        public Vector2 TextOffset
        {
            get
            {
                return this._TextOffset;
            }
            set
            {
                this._TextOffset = value;
            }
        }

        /// <summary>
        /// Kích thước Icon
        /// </summary>
        public Vector2 IconSize
        {
            get
            {
                return this.UISprite_MinimapIcon.GetComponent<SpriteRenderer>().size;
            }
            set
            {
                SpriteRenderer renderer = this.UISprite_MinimapIcon.GetComponent<SpriteRenderer>();
                renderer.size = value;
            }
        }

        private NPCTaskStates _NPCTaskState = NPCTaskStates.None;
        /// <summary>
        /// Trạng thái nhiệm vụ của NPC
        /// </summary>
        public NPCTaskStates NPCTaskState
        {
            get
            {
                return this._NPCTaskState;
            }
            set
            {
                this._NPCTaskState = value;

                /// Ẩn toàn bộ trạng thái TaskState của NPC
                this.DisableAllNPCStatesIcon();

                QuestStateIconPrefab uiStatePrefab = this.QuestStateIconPrefabs.Where(x => x.State == value).FirstOrDefault();
                /// Nếu tìm thấy
                if (uiStatePrefab != null)
                {
                    uiStatePrefab.UIAnimatedSprite.gameObject.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Đối tượng tham chiếu
        /// </summary>
        public GameObject ReferenceObject { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Dịch vào góc
            this.transform.localPosition = new Vector2(-100000, -100000);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật ảnh Icon đối tượng
        /// </summary>
        private void UpdateIcon()
        {
            this.UISprite_MinimapIcon.Load();
        }

        /// <summary>
        /// Ẩn toàn bộ trạng thái Task của NPC
        /// </summary>
        private void DisableAllNPCStatesIcon()
        {
            /// Làm rỗng hiển thị
            foreach (QuestStateIconPrefab uiStatePrefab in this.QuestStateIconPrefabs)
            {
                uiStatePrefab.UIAnimatedSprite.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật vị trí
        /// </summary>
        public void UpdatePosition()
        {
            /// Toác gì đó
            if (this.ReferenceObject == null)
            {
                /// Bỏ qua
                return;
            }
            /// Không có minimap
            else if (Global.CurrentMap.LocalMapSprite == null)
            {
                /// Bỏ qua
                return;
            }

            /// Vị trí hiện tại chuyển qua tọa độ minimap
            Vector2 localMapPos = KTGlobal.WorldPositionToWorldNavigationMapPosition(this.ReferenceObject.transform.localPosition);
            /// Cập nhật vị trí của đối tượng
            this.transform.localPosition = localMapPos;
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Destroy()
        {
            this.StopAllCoroutines();
            this.NameColor = default;
            this.Name = "";
            this.ShowName = false;
            this.ShowIcon = false;
            this.BundleDir = "";
            this.AtlasName = "";
            this.SpriteName = "";
            this.TextOffset = default;
            this.IconSize = default;
            this.ReferenceObject = null;
            this.DisableAllNPCStatesIcon();
            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
        #endregion
    }
}
