using FS.VLTK.Utilities.UnityUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI
{
    /// <summary>
    /// Khung tooltip thông báo góc trên chính giữa màn hình
    /// </summary>
    public class UINotificationTip : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab nội dung
        /// </summary>
        [SerializeField]
        private UIFadeOutText UIText_ContentPrefab;

        /// <summary>
        /// Số Text tối đa chứa được, quá số này sẽ xóa các Text cũ đi
        /// </summary>
        [SerializeField]
        private int _MaxCapacity = 4;
        #endregion

        /// <summary>
        /// Danh sách Text
        /// </summary>
        private readonly List<UIFadeOutText> listContents = new List<UIFadeOutText>();

        /// <summary>
        /// Rect Transform
        /// </summary>
        private RectTransform rectTransform;

        #region Properties
        /// <summary>
        /// Số Text tối đa chứa được, quá số này sẽ xóa các Text cũ đi
        /// </summary>
        public int MaxCapacity
        {
            get
            {
                return this._MaxCapacity;
            }
            set
            {
                this._MaxCapacity = value;

                while (this.listContents.Count >= this._MaxCapacity)
                {
                    this.listContents[0].DestroyImmediate();
                    this.listContents.RemoveAt(0);
                }
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.gameObject.GetComponent<RectTransform>();
        }

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

        }
        #endregion

        #region Private methods
        #endregion

        #region Public methods
        /// <summary>
        /// Hiển thị Text lên màn hình
        /// </summary>
        /// <param name="text"></param>
        public void AddText(string text)
        {
            UIFadeOutText uiText = GameObject.Instantiate<UIFadeOutText>(this.UIText_ContentPrefab);
            uiText.transform.SetParent(this.gameObject.transform, false);
            uiText.gameObject.SetActive(true);
            uiText.Text = text;
            uiText.Finish = () => {
                this.listContents.Remove(uiText);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
            };

            while (this.listContents.Count >= this._MaxCapacity)
            {
                this.listContents[0].DestroyImmediate();
                this.listContents.RemoveAt(0);
            }
            this.listContents.Add(uiText);
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
        }
        #endregion
    }
}