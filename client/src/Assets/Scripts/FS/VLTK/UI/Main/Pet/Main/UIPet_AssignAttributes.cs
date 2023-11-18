using FS.VLTK.Utilities.UnityUI;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung phân phối tiềm năng pet
    /// </summary>
    public class UIPet_AssignAttributes : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Button cộng điểm Sức
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddStr;

        /// <summary>
        /// Button trừ điểm Sức
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SubStr;

        /// <summary>
        /// Button cộng điểm Thân
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddDex;

        /// <summary>
        /// Button trừ điểm Thân
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SubDex;

        /// <summary>
        /// Button cộng điểm Ngoại
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddSta;

        /// <summary>
        /// Button trừ điểm Ngoại
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SubSta;

        /// <summary>
        /// Button cộng điểm Nội
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddInt;

        /// <summary>
        /// Button trừ điểm Nội
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SubInt;

        /// <summary>
        /// Text Giá trị Sức
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Str;

        /// <summary>
        /// Text Giá trị Thân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Dex;

        /// <summary>
        /// Text Giá trị Ngoại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Sta;

        /// <summary>
        /// Text Giá trị Nội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Int;

        /// <summary>
        /// Text Điểm tiềm năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RemainPoint;

        /// <summary>
        /// Button hủy bỏ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Cancel;

        /// <summary>
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Accept;
        #endregion

        #region Properties
        private int _RemainPoint;
        /// <summary>
        /// Điểm tiềm năng
        /// </summary>
        public int RemainPoint
        {
            get
            {
                return this._RemainPoint;
            }
            set
            {
                this._RemainPoint = value;
                this.UIText_RemainPoint.text = value.ToString();
                this.tmpRemainPoint = value;

                if (value > 0)
                {
                    this.SetStateToAllAddButtons(true);
                }
            }
        }

        private int _Str;
        /// <summary>
        /// Sức
        /// </summary>
        public int Str
        {
            get
            {
                return this._Str;
            }
            set
            {
                this._Str = value;
                this.UIText_Str.text = value.ToString();
            }
        }

        private int _Dex;
        /// <summary>
        /// Thân
        /// </summary>
        public int Dex
        {
            get
            {
                return this._Dex;
            }
            set
            {
                this._Dex = value;
                this.UIText_Dex.text = value.ToString();
            }
        }

        private int _Sta;
        /// <summary>
        /// Ngoại
        /// </summary>
        public int Sta
        {
            get
            {
                return this._Sta;
            }
            set
            {
                this._Sta = value;
                this.UIText_Sta.text = value.ToString();
            }
        }

        private int _Int;
        /// <summary>
        /// Nội
        /// </summary>
        public int Int
        {
            get
            {
                return this._Int;
            }
            set
            {
                this._Int = value;
                this.UIText_Int.text = value.ToString();
            }
        }

        /// <summary>
        /// Sự kiện khi Button chấp nhận được ấn
        /// </summary>
        public Action<int, int, int, int> Accept { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Điểm Sức có thay đổi
        /// </summary>
        private bool IsStrChanged
        {
            get
            {
                try
                {
                    int nStr = int.Parse(this.UIText_Str.text);

                    return this._Str != nStr;
                }
                catch (Exception) { }
                return false;
            }
        }

        /// <summary>
        /// Điểm Thân có thay đổi
        /// </summary>
        private bool IsDexChanged
        {
            get
            {
                try
                {
                    int nDex = int.Parse(this.UIText_Dex.text);

                    return this._Dex != nDex;
                }
                catch (Exception) { }
                return false;
            }
        }

        /// <summary>
        /// Điểm Ngoại có thay đổi
        /// </summary>
        private bool IsStaChanged
        {
            get
            {
                try
                {
                    int nSta = int.Parse(this.UIText_Sta.text);

                    return this._Sta != nSta;
                }
                catch (Exception) { }
                return false;
            }
        }

        /// <summary>
        /// Điểm Nội có thay đổi
        /// </summary>
        private bool IsIntChanged
        {
            get
            {
                try
                {
                    int nInt = int.Parse(this.UIText_Int.text);

                    return this._Int != nInt;
                }
                catch (Exception) { }
                return false;
            }
        }

        /// <summary>
        /// Điểm cộng có thay đổi
        /// </summary>
        private bool IsPointChanged
        {
            get
            {
                return this.IsStrChanged || this.IsDexChanged || this.IsStaChanged || this.IsIntChanged;
            }
        }

        /// <summary>
        /// Biến lưu điểm tiềm năng tạm thời
        /// </summary>
        private int tmpRemainPoint;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.UIButton_Accept.interactable = false;
            this.UIButton_Cancel.interactable = false;

            this.SetStateToAllSubButtons(false);
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.UIButton_Accept.interactable = false;
            this.UIButton_Cancel.interactable = false;

            this.SetStateToAllSubButtons(false);
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);

            this.UIButton_Cancel.onClick.AddListener(this.ButtonCancel_Clicked);
            this.UIButton_Accept.onClick.AddListener(this.ButtonAccept_Clicked);

            this.UIButton_AddStr.onClick.AddListener(this.ButtonAddStr_Clicked);
            this.InitHoverOnButton(this.UIButton_AddStr, this.ButtonAddStr_Clicked);
            this.UIButton_SubStr.onClick.AddListener(this.ButtonSubStr_Clicked);
            this.InitHoverOnButton(this.UIButton_SubStr, this.ButtonSubStr_Clicked);

            this.UIButton_AddDex.onClick.AddListener(this.ButtonAddDex_Clicked);
            this.InitHoverOnButton(this.UIButton_AddDex, this.ButtonAddDex_Clicked);
            this.UIButton_SubDex.onClick.AddListener(this.ButtonSubDex_Clicked);
            this.InitHoverOnButton(this.UIButton_SubDex, this.ButtonSubDex_Clicked);

            this.UIButton_AddSta.onClick.AddListener(this.ButtonAddSta_Clicked);
            this.InitHoverOnButton(this.UIButton_AddSta, this.ButtonAddSta_Clicked);
            this.UIButton_SubSta.onClick.AddListener(this.ButtonSubSta_Clicked);
            this.InitHoverOnButton(this.UIButton_SubSta, this.ButtonSubSta_Clicked);

            this.UIButton_AddInt.onClick.AddListener(this.ButtonAddInt_Clicked);
            this.InitHoverOnButton(this.UIButton_AddInt, this.ButtonAddInt_Clicked);
            this.UIButton_SubInt.onClick.AddListener(this.ButtonSubInt_Clicked);
            this.InitHoverOnButton(this.UIButton_SubInt, this.ButtonSubInt_Clicked);

            this.SetStateToAllAddButtons(false);
            this.SetStateToAllSubButtons(false);
        }

        /// <summary>
        /// Sự kiện khi nút đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            if (IsPointChanged)
            {
                KTGlobal.ShowMessageBox("Điểm tiềm năng cộng vào vẫn chưa được lưu lại, nếu đóng khung lúc này, các giá trị đã được phân phối sẽ mất, và bạn phải bắt đầu lại từ lần kế tiếp mở khung. Xác nhận đóng khung?", () => {
                    this.Hide();
                }, true);
            }
            else
            {
                this.Hide();
            }
        }

        /// <summary>
        /// Sự kiện khi Button hủy bỏ được ấn
        /// </summary>
        private void ButtonCancel_Clicked()
        {
            KTGlobal.ShowMessageBox("Sau khi làm mới dữ liệu, các giá trị đã được phân phối sẽ mất, bạn phải tiến hành nhập lại. Xác nhận làm mới dữ liệu lại từ đầu?", () => {
                this.UIText_Str.text = this._Str.ToString();
                this.UIText_Dex.text = this._Dex.ToString();
                this.UIText_Sta.text = this._Sta.ToString();
                this.UIText_Int.text = this._Int.ToString();
                this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

                if (this.tmpRemainPoint > 0)
                {
                    this.SetStateToAllAddButtons(true);
                }
                else
                {
                    this.SetStateToAllAddButtons(false);
                }
                this.SetStateToAllSubButtons(false);

                if (this.IsPointChanged)
                {
                    this.UIButton_Accept.interactable = true;
                    this.UIButton_Cancel.interactable = true;
                }
                else
                {
                    this.UIButton_Accept.interactable = false;
                    this.UIButton_Cancel.interactable = false;
                }
            }, true);
        }

        /// <summary>
        /// Sự kiện khi Button đồng ý được ấn
        /// </summary>
        private void ButtonAccept_Clicked()
        {
            KTGlobal.ShowMessageBox("Xác nhận cộng điểm vào các thuộc tính này?", () => {
                try
                {
                    int nStr = int.Parse(this.UIText_Str.text) - this._Str;
                    int nDex = int.Parse(this.UIText_Dex.text) - this._Dex;
                    int nSta = int.Parse(this.UIText_Sta.text) - this._Sta;
                    int nInt = int.Parse(this.UIText_Int.text) - this._Int;

                    if (nStr < 0 || nDex < 0 || nSta < 0 || nInt < 0)
                    {
                        throw new Exception();
                    }

                    /// Thực thi sự kiện
                    this.Accept?.Invoke(nStr, nDex, nSta, nInt);

                    /// Đóng khung
                    this.Hide();
                }
                catch (Exception)
                {
                    KTGlobal.AddNotification("Có sai sót trong thao tác dữ liệu, hãy thử mở lại khung!");
                }
            }, true);
        }

        /// <summary>
        /// Sự kiện khi Button cộng điểm Sức được ấn
        /// </summary>
        private void ButtonAddStr_Clicked()
        {
            if (!this.UIButton_AddStr.interactable)
            {
                return;
            }

            this.tmpRemainPoint--;
            this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

            int tmpStr = int.Parse(this.UIText_Str.text);
            tmpStr++;
            this.UIText_Str.text = tmpStr.ToString();

            if (this.tmpRemainPoint <= 0)
            {
                this.SetStateToAllAddButtons(false);
            }

            if (this.IsStrChanged)
            {
                this.UIButton_SubStr.interactable = true;
            }
            else
            {
                this.UIButton_SubStr.interactable = false;
            }

            if (this.IsPointChanged)
            {
                this.UIButton_Accept.interactable = true;
                this.UIButton_Cancel.interactable = true;
            }
            else
            {
                this.UIButton_Accept.interactable = false;
                this.UIButton_Cancel.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button trừ điểm Sức
        /// </summary>
        private void ButtonSubStr_Clicked()
        {
            if (!this.UIButton_SubStr.interactable)
            {
                return;
            }

            this.tmpRemainPoint++;
            this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

            int tmpStr = int.Parse(this.UIText_Str.text);
            tmpStr--;
            this.UIText_Str.text = tmpStr.ToString();

            if (this.tmpRemainPoint > 0)
            {
                this.SetStateToAllAddButtons(true);
            }

            if (this.IsStrChanged)
            {
                this.UIButton_SubStr.interactable = true;
            }
            else
            {
                this.UIButton_SubStr.interactable = false;
            }

            if (this.IsPointChanged)
            {
                this.UIButton_Accept.interactable = true;
                this.UIButton_Cancel.interactable = true;
            }
            else
            {
                this.UIButton_Accept.interactable = false;
                this.UIButton_Cancel.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button cộng điểm Thân được ấn
        /// </summary>
        private void ButtonAddDex_Clicked()
        {
            if (!this.UIButton_AddDex.interactable)
            {
                return;
            }

            this.tmpRemainPoint--;
            this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

            int tmpDex = int.Parse(this.UIText_Dex.text);
            tmpDex++;
            this.UIText_Dex.text = tmpDex.ToString();

            if (this.tmpRemainPoint <= 0)
            {
                this.SetStateToAllAddButtons(false);
            }

            if (this.IsDexChanged)
            {
                this.UIButton_SubDex.interactable = true;
            }
            else
            {
                this.UIButton_SubDex.interactable = false;
            }

            if (this.IsPointChanged)
            {
                this.UIButton_Accept.interactable = true;
                this.UIButton_Cancel.interactable = true;
            }
            else
            {
                this.UIButton_Accept.interactable = false;
                this.UIButton_Cancel.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button trừ điểm Thân
        /// </summary>
        private void ButtonSubDex_Clicked()
        {
            if (!this.UIButton_SubDex.interactable)
            {
                return;
            }

            this.tmpRemainPoint++;
            this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

            int tmpDex = int.Parse(this.UIText_Dex.text);
            tmpDex--;
            this.UIText_Dex.text = tmpDex.ToString();

            if (this.tmpRemainPoint > 0)
            {
                this.SetStateToAllAddButtons(true);
            }

            if (this.IsDexChanged)
            {
                this.UIButton_SubDex.interactable = true;
            }
            else
            {
                this.UIButton_SubDex.interactable = false;
            }

            if (this.IsPointChanged)
            {
                this.UIButton_Accept.interactable = true;
                this.UIButton_Cancel.interactable = true;
            }
            else
            {
                this.UIButton_Accept.interactable = false;
                this.UIButton_Cancel.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button cộng điểm Ngoại được ấn
        /// </summary>
        private void ButtonAddSta_Clicked()
        {
            if (!this.UIButton_AddSta.interactable)
            {
                return;
            }

            this.tmpRemainPoint--;
            this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

            int tmpSta = int.Parse(this.UIText_Sta.text);
            tmpSta++;
            this.UIText_Sta.text = tmpSta.ToString();

            if (this.tmpRemainPoint <= 0)
            {
                this.SetStateToAllAddButtons(false);
            }

            if (this.IsStaChanged)
            {
                this.UIButton_SubSta.interactable = true;
            }
            else
            {
                this.UIButton_SubSta.interactable = false;
            }

            if (this.IsPointChanged)
            {
                this.UIButton_Accept.interactable = true;
                this.UIButton_Cancel.interactable = true;
            }
            else
            {
                this.UIButton_Accept.interactable = false;
                this.UIButton_Cancel.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button trừ điểm Ngoại
        /// </summary>
        private void ButtonSubSta_Clicked()
        {
            if (!this.UIButton_SubSta.interactable)
            {
                return;
            }

            this.tmpRemainPoint++;
            this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

            int tmpSta = int.Parse(this.UIText_Sta.text);
            tmpSta--;
            this.UIText_Sta.text = tmpSta.ToString();

            if (this.tmpRemainPoint > 0)
            {
                this.SetStateToAllAddButtons(true);
            }

            if (this.IsStaChanged)
            {
                this.UIButton_SubSta.interactable = true;
            }
            else
            {
                this.UIButton_SubSta.interactable = false;
            }

            if (this.IsPointChanged)
            {
                this.UIButton_Accept.interactable = true;
                this.UIButton_Cancel.interactable = true;
            }
            else
            {
                this.UIButton_Accept.interactable = false;
                this.UIButton_Cancel.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button cộng điểm Nội được ấn
        /// </summary>
        private void ButtonAddInt_Clicked()
        {
            if (!this.UIButton_AddInt.interactable)
            {
                return;
            }

            this.tmpRemainPoint--;
            this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

            int tmpInt = int.Parse(this.UIText_Int.text);
            tmpInt++;
            this.UIText_Int.text = tmpInt.ToString();

            if (this.tmpRemainPoint <= 0)
            {
                this.SetStateToAllAddButtons(false);
            }

            if (this.IsIntChanged)
            {
                this.UIButton_SubInt.interactable = true;
            }
            else
            {
                this.UIButton_SubInt.interactable = false;
            }

            if (this.IsPointChanged)
            {
                this.UIButton_Accept.interactable = true;
                this.UIButton_Cancel.interactable = true;
            }
            else
            {
                this.UIButton_Accept.interactable = false;
                this.UIButton_Cancel.interactable = false;
            }
        }

        /// <summary>
        /// Sự kiện khi Button trừ điểm Nội
        /// </summary>
        private void ButtonSubInt_Clicked()
        {
            if (!this.UIButton_SubInt.interactable)
            {
                return;
            }

            this.tmpRemainPoint++;
            this.UIText_RemainPoint.text = this.tmpRemainPoint.ToString();

            int tmpInt = int.Parse(this.UIText_Int.text);
            tmpInt--;
            this.UIText_Int.text = tmpInt.ToString();

            if (this.tmpRemainPoint > 0)
            {
                this.SetStateToAllAddButtons(true);
            }

            if (this.IsIntChanged)
            {
                this.UIButton_SubInt.interactable = true;
            }
            else
            {
                this.UIButton_SubInt.interactable = false;
            }

            if (this.IsPointChanged)
            {
                this.UIButton_Accept.interactable = true;
                this.UIButton_Cancel.interactable = true;
            }
            else
            {
                this.UIButton_Accept.interactable = false;
                this.UIButton_Cancel.interactable = false;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Khởi tạo sự kiện Hover cho các Button cộng điểm
        /// </summary>
        /// <param name="button"></param>
        /// <param name="tickFunc"></param>
        private void InitHoverOnButton(UnityEngine.UI.Button button, Action tickFunc)
        {
            UIHoverableObject hoverable = button.gameObject.GetComponent<UIHoverableObject>();
            hoverable.Continuously = true;
            hoverable.HoverDuration = 0.2f;
            hoverable.HoverTick = 0.05f;
            float nSkip = 1;
            float nMultiply = 1.2f;
            const float MaxSkip = 512;
            hoverable.Tick = () => {
                if (nSkip < MaxSkip)
                {
                    nSkip *= nMultiply;
                }
                for (int i = 1; i <= nSkip; i++)
                {
                    tickFunc?.Invoke();
                }
            };
            hoverable.HoverEnd = () => {
                nSkip = 1;
            };
        }

        /// <summary>
        /// Cập nhật trạng thái cho tất cả các Button cộng trừ điểm
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetStateToAllAddButtons(bool isEnable)
        {
            this.UIButton_AddStr.interactable = isEnable;
            this.UIButton_AddDex.interactable = isEnable;
            this.UIButton_AddSta.interactable = isEnable;
            this.UIButton_AddInt.interactable = isEnable;
        }

        /// <summary>
        /// Cập nhật trạng thái cho tất cả các Button cộng trừ điểm
        /// </summary>
        /// <param name="isEnable"></param>
        private void SetStateToAllSubButtons(bool isEnable)
        {
            this.UIButton_SubStr.interactable = isEnable;
            this.UIButton_SubDex.interactable = isEnable;
            this.UIButton_SubSta.interactable = isEnable;
            this.UIButton_SubInt.interactable = isEnable;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Đóng khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
