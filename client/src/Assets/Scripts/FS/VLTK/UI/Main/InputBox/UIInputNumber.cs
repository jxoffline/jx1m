using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

namespace FS.VLTK.UI.Main.InputBox
{
    /// <summary>
    /// Khung nhập số
    /// </summary>
    public class UIInputNumber : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Nội dung nhập
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Text;

        /// <summary>
        /// Button trừ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Sub;

        /// <summary>
        /// Button cộng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Add;

        /// <summary>
        /// Input nhập số
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_Number;

        /// <summary>
        /// Button hủy bỏ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Cancel;

        /// <summary>
        /// Button đồng ý
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OK;
        #endregion

        #region Properties
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
        /// Giá trị khởi tạo
        /// </summary>
        public int InitValue { get; set; }

        /// <summary>
        /// Giá trị nhỏ nhất
        /// </summary>
        public int MinValue { get; set; } = int.MinValue;

        /// <summary>
        /// Giá trị lớn nhất
        /// </summary>
        public int MaxValue { get; set; } = int.MaxValue;

        /// <summary>
        /// Sự kiện khi Button Xác nhận được ấn
        /// </summary>
        public Action<int> OK { get; set; }

        /// <summary>
        /// Sự kiện khi Button Hủy bỏ được ấn
        /// </summary>
        public Action Cancel { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.Refresh();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIInput_Number.text = this.InitValue.ToString();

            this.UIButton_Add.onClick.AddListener(this.ButtonAdd_Clicked);
            this.UIButton_Sub.onClick.AddListener(this.ButtonSub_Clicked);
            this.UIButton_Cancel.onClick.AddListener(this.ButtonCancel_Clicked);
            this.UIButton_OK.onClick.AddListener(this.ButtonOK_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button cộng được ấn
        /// </summary>
        private void ButtonAdd_Clicked()
        {
            if (int.TryParse(this.UIInput_Number.text, out int number))
            {
                number = Math.Min(number, this.MaxValue);
                number = Math.Max(number, this.MinValue);
                if (number < this.MaxValue)
                {
                    number++;
                    this.UIInput_Number.text = number.ToString();
                }
                else
                {
                    this.UIButton_Add.interactable = false;
                }
            }
            else
            {
                this.UIButton_Add.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button trừ được ấn
        /// </summary>
        private void ButtonSub_Clicked()
        {
            if (int.TryParse(this.UIInput_Number.text, out int number))
            {
                number = Math.Min(number, this.MaxValue);
                number = Math.Max(number, this.MinValue);
                if (number > this.MinValue)
                {
                    number--;
                    this.UIInput_Number.text = number.ToString();
                }
                else
                {
                    this.UIButton_Sub.interactable = false;
                }
            }
            else
            {
                this.UIButton_Sub.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button đồng ý được ấn
        /// </summary>
        private void ButtonOK_Clicked()
        {
            if (int.TryParse(this.UIInput_Number.text, out int number))
            {
                number = Math.Min(number, this.MaxValue);
                number = Math.Max(number, this.MinValue);
                this.OK?.Invoke(number);
                this.Destroy();
            }
        }

        /// <summary>
        /// Sự kiện khi Button hủy bỏ được ấn
        /// </summary>
        private void ButtonCancel_Clicked()
        {
            this.Cancel?.Invoke();
            this.Destroy();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới
        /// </summary>
        private void Refresh()
        {
            if (int.TryParse(this.UIInput_Number.text, out int number))
            {
                this.UIButton_Sub.interactable = number > this.MinValue;
                this.UIButton_Add.interactable = number < this.MaxValue;
                this.UIButton_OK.interactable = true;

                if (number > this.MaxValue)
                {
                    this.UIInput_Number.text = this.MaxValue.ToString();
                }
                else if (number < this.MinValue)
                {
                    this.UIInput_Number.text = this.MinValue.ToString();
                }
            }
            else
            {
                this.UIButton_Add.interactable = false;
                this.UIButton_Sub.interactable = false;
                this.UIButton_OK.interactable = false;
            }
        }

        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        private void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }
        #endregion
    }
}
