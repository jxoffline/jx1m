using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.MainUI.RolePart
{
    /// <summary>
    /// Bảng biểu diễn chi tiết Buff
    /// </summary>
    public class UIRolePart_BuffDetailBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên Buff
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_BuffName;

        /// <summary>
        /// Text chi tiết Buff
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_BuffDetail;

        /// <summary>
        /// Text cấp độ Buff
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_BuffLevel;

        /// <summary>
        /// Icon Buff
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_BuffIcon;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Vị trí xuất hiện so với vị trí gốc
        /// </summary>
        [SerializeField]
        private Vector2 Offset;
        #endregion

        #region Properties
        /// <summary>
        /// Tên buff
        /// </summary>
        public string BuffName
        {
            get
            {
                return this.UIText_BuffName.text;
            }
            set
            {
                this.UIText_BuffName.text = value;
            }
        }

        /// <summary>
        /// Thông tin buff
        /// </summary>
        public string BuffDetail
        {
            get
            {
                return this.UIText_BuffDetail.text;
            }
            set
            {
                this.UIText_BuffDetail.text = value;
            }
        }

        private int _BuffLevel;
        /// <summary>
        /// Cấp độ Buff
        /// </summary>
        public int BuffLevel
        {
            get
            {
                return this._BuffLevel;
            }
            set
            {
                this._BuffLevel = value;
                this.UIText_BuffLevel.text = value.ToString();
            }
        }

        /// <summary>
        /// Đường dẫn file Bundle chứa ảnh
        /// </summary>
        public string IconBundleDir
        {
            get
            {
                return this.UIImage_BuffIcon.BundleDir;
            }
            set
            {
                this.UIImage_BuffIcon.BundleDir = value;
            }
        }

        /// <summary>
        /// Tên Atlas chứa ảnh
        /// </summary>
        public string IconAtlasName
        {
            get
            {
                return this.UIImage_BuffIcon.AtlasName;
            }
            set
            {
                this.UIImage_BuffIcon.AtlasName = value;
            }
        }

        /// <summary>
        /// Tên ảnh
        /// </summary>
        public string IconSpriteName
        {
            get
            {
                return this.UIImage_BuffIcon.SpriteName;
            }
            set
            {
                this.UIImage_BuffIcon.SpriteName = value;
            }
        }

        /// <summary>
        /// Vị trí hiển thị
        /// </summary>
        public Vector2 Position { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện ở Frame tiếp theo
        /// </summary>
        /// <param name="work"></param>
        private void ExecuteNextFrame(Action work)
        {
            IEnumerator DoExecute()
            {
                yield return null;
                work?.Invoke();
            }
            this.StartCoroutine(DoExecute());
        }

        /// <summary>
        /// Cập nhật hiển thị vị trí
        /// </summary>
        private void UpdateVisible()
        {
            this.gameObject.transform.position = this.Position;
            this.gameObject.GetComponent<RectTransform>().anchoredPosition += this.Offset + new Vector2(this.gameObject.GetComponent<RectTransform>().sizeDelta.x / 2, 0);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiển thị khung
        /// </summary>
        public void Show()
        {
            this.UpdateVisible();
            this.gameObject.SetActive(true);
            this.ExecuteNextFrame(() => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.UIText_BuffDetail.transform.parent.GetComponent<RectTransform>());
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.UIText_BuffName.transform.parent.GetComponent<RectTransform>());
            });
            this.UIImage_BuffIcon.Load();
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
