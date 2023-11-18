using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace FS.VLTK.UI.Main.NPCDialog
{
    /// <summary>
    /// Prefab lựa chọn trong NPC Dialog
    /// </summary>
    public class UINPCDialog_Selection : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button chọn
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Selection;

        /// <summary>
        /// Text
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Text;
        #endregion

        #region Properties
        /// <summary>
        /// ID sự kiện
        /// </summary>
        public int SelectionID { get; set; }

        /// <summary>
        /// Nội dung
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
        /// Sự kiện khi đối tượng được Click
        /// </summary>
        public Action Click { get; set; }
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
            this.UIButton_Selection.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được Click
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion
    }
}