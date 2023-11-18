using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Server.Data;
using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.LuckyCircle
{
    /// <summary>
    /// Khung lịch sử quay trong Vòng quay may mắn
    /// </summary>
    public class UILuckyCircle_HistoryBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab nội dung
        /// </summary>
        [SerializeField]
        private UIRichText UIText_Prefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform nội dung
        /// </summary>
        private RectTransform rectTransform = null;

        /// <summary>
        /// ScrollView nội dung
        /// </summary>
        private UnityEngine.UI.ScrollRect scrollView;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.UIText_Prefab.transform.parent.GetComponent<RectTransform>();
            this.scrollView = this.UIText_Prefab.transform.parent.parent.parent.GetComponent<UnityEngine.UI.ScrollRect>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.ClearHistory();
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
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
                this.scrollView.normalizedPosition = Vector2.zero;
            }));
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm lịch sử tương ứng
        /// </summary>
        /// <param name="text"></param>
        /// <param name="htmlColor"></param>
        public void AppendHistory(string text, string htmlColor = "#ffffff")
        {
            /// Thêm vào nội dung Chat
            UIRichText uiText = GameObject.Instantiate<UIRichText>(this.UIText_Prefab);
            uiText.transform.SetParent(this.rectTransform, false);
            uiText.gameObject.SetActive(true);
            uiText.Text = string.Format("<color={0}>{1}</color>", htmlColor, text);
            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Thêm lịch sử tương ứng
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="quantity"></param>
        public void AppendHistory(int itemID, int quantity)
        {
            /// Nếu số lượng không thỏa mãn
            if (quantity <= 0)
            {
                return;
            }
            /// Thông tin vật phẩm
            if (!Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
            {
                return;
            }

            GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
            /// Thêm vào nội dung Chat
            UIRichText uiText = GameObject.Instantiate<UIRichText>(this.UIText_Prefab);
            uiText.transform.SetParent(this.rectTransform, false);
            uiText.gameObject.SetActive(true);
            uiText.Text = string.Format("Nhận được <color=#1afff4>{0} cái</color> {1}", quantity, KTGlobal.GetItemDescInfoStringForChat(itemGD));
            uiText.ClickEvents = new Dictionary<string, Action<string>>()
            {
                { string.Format("ITEM_{0}", itemGD.Id), (_) => {
                    KTGlobal.ShowItemInfo(itemGD);
                } },
            };
            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Xóa lịch sử
        /// </summary>
        public void ClearHistory()
        {
            foreach (Transform child in this.rectTransform.transform)
            {
                if (child.gameObject != this.UIText_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
