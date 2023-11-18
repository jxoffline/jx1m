using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.Chat
{
    /// <summary>
    /// Khung chọn biểu tượng cảm xúc
    /// </summary>
    public class UIChat_SelectStickerFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Danh sách biểu tượng
        /// </summary>
        [SerializeField]
        private List<Sprite> UI_Stickers;

        /// <summary>
        /// Prefab Button biểu tượng
        /// </summary>
        [SerializeField]
        private UIChat_SelectStickerFrame_StickerButton UIButton_StickerPrefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách biểu tượng
        /// </summary>
        private RectTransform transformStickersList = null;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

        /// <summary>
        /// Đang trong trạng thái hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
        }

        /// <summary>
        /// Sự kiện chọn Sticker
        /// </summary>
        public Action<int> SelectSticker { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformStickersList = this.UIButton_StickerPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
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
            this.Close?.Invoke();
            this.Hide();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
		{
            for (int i = 1; i <= skip; i++)
			{
                yield return null;
			}
            work?.Invoke();
		}

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
		{
            if (!this.gameObject.activeSelf)
			{
                return;
			}
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformStickersList);
            }));
		}

        /// <summary>
        /// Làm rỗng danh sách biểu tượng
        /// </summary>
        private void ClearStickers()
		{
            foreach (Transform child in this.transformStickersList.transform)
			{
                if (child.gameObject != this.UIButton_StickerPrefab.gameObject)
				{
                    GameObject.Destroy(child.gameObject);
				}
			}
		}

        /// <summary>
        /// Làm mới danh sách
        /// </summary>
        private void Refresh()
		{
            this.ClearStickers();
            int idx = 0;
            foreach (Sprite sprite in this.UI_Stickers)
			{
                UIChat_SelectStickerFrame_StickerButton uiStickerButton = GameObject.Instantiate<UIChat_SelectStickerFrame_StickerButton>(this.UIButton_StickerPrefab);
                uiStickerButton.transform.SetParent(this.transformStickersList, false);
                uiStickerButton.gameObject.SetActive(true);
                uiStickerButton.Sprite = sprite;
                uiStickerButton.Index = idx;
                uiStickerButton.Click = () => {
                    this.SelectSticker?.Invoke(uiStickerButton.Index);
                };
                idx++;
            }
            this.RebuildLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiển thị
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
