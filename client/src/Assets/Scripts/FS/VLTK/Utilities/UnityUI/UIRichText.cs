using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using FS.GameEngine.Logic;
using System;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Rich Text với các sự kiện click được gắn trong thẻ <link="FuncName"></link>
    /// </summary>
    public class UIRichText : MonoBehaviour, IPointerClickHandler
    {
        #region Define
        /// <summary>
        /// Đối tượng Text
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Text;
        #endregion

        #region Properties
        /// <summary>
        /// Nội dung Rich Text
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText_Text.text;
            }
            set
            {
                this.UIText_Text.text = value;
            }
        }

        /// <summary>
        /// Sự kiện khi người dùng Click vào thẻ link trong Rich Text
        /// </summary>
        public Dictionary<string, Action<string>> ClickEvents { get; set; } = new Dictionary<string, Action<string>>();
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

        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sự kẹn khi người dùng click vào đối tượng
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (this.UIText_Text == null)
            {
                return;
            }

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(this.UIText_Text, eventData.position, null);
            if (linkIndex != -1)
            {
                TMP_LinkInfo linkInfo = this.UIText_Text.textInfo.linkInfo[linkIndex];
                string funcName = linkInfo.GetLinkID();
                string text = linkInfo.GetLinkText();

                if (this.ClickEvents != null && this.ClickEvents.TryGetValue(funcName, out Action<string> action))
                {
                    action?.Invoke(text);
                }
            }
        }
        #endregion
    }
}
