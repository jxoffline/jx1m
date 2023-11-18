using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.SkillTree
{
    /// <summary>
    /// Ô kỹ năng trong khung Kỹ năng môn phái
    /// </summary>
    public class SkillTree_SkillItemBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button Icon kỹ năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Icon;

        /// <summary>
        /// Image Icon kỹ năng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Icon;

        /// <summary>
        /// Text tên kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text mô tả ngắn của kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ShortDesc;

        /// <summary>
        /// Text cấp độ kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Button cộng điểm kỹ năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddPoint;

        /// <summary>
        /// Button trừ điểm kỹ năng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SubPoint;

        /// <summary>
        /// Text số điểm đã cộng vào kỹ năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AdditionLevel;
        #endregion

        #region Properties
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public int SkillID { get; set; }

        /// <summary>
        /// Đường dẫn file Bundle chứa Icon
        /// </summary>
        public string IconBundleDir
        {
            get
            {
                return this.UIImage_Icon.BundleDir;
            }
            set
            {
                this.UIImage_Icon.BundleDir = value;
            }
        }

        /// <summary>
        /// Tên Atlas chứa Icon
        /// </summary>
        public string IconAtlasName
        {
            get
            {
                return this.UIImage_Icon.AtlasName;
            }
            set
            {
                this.UIImage_Icon.AtlasName = value;
            }
        }

        /// <summary>
        /// Tên Sprite của Icon
        /// </summary>
        public string IconSpriteName
        {
            get
            {
                return this.UIImage_Icon.SpriteName;
            }
            set
            {
                this.UIImage_Icon.SpriteName = value;
            }
        }

        private bool _CanStudy;
        /// <summary>
        /// Có thể học không
        /// </summary>
        public bool CanStudy
        {
            get
            {
                return this._CanStudy;
            }
            set
            {
                this._CanStudy = value;
                this.UIButton_AddPoint.gameObject.SetActive(value);
                this.UIButton_SubPoint.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Tên kỹ năng
        /// </summary>
        public string SkillName
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
        /// Mô tả ngắn của kỹ năng
        /// </summary>
        public string SkillShortDesc
        {
            get
            {
                return this.UIText_ShortDesc.text;
            }
            set
            {
                this.UIText_ShortDesc.text = value;
            }
        }

        private int _SkillLevel = 0;
        /// <summary>
        /// Cấp độ kỹ năng
        /// </summary>
        public int SkillLevel
        {
            get
            {
                return this._SkillLevel;
            }
            set
            {
                this._SkillLevel = value;
                this.TempSkillLevel = value;
                this.UpdateVisible();
            }
        }

        /// <summary>
        /// Giá trị kỹ năng tạm thời mà người chơi đã thao tác ở vị trí này
        /// </summary>
        public int TempSkillLevel { get; private set; }

        private int _SkillMaxLevel = 0;
        /// <summary>
        /// Cấp độ tối đa cho phép cộng điểm của kỹ năng
        /// </summary>
        public int SkillMaxLevel
        {
            get
            {
                return this._SkillMaxLevel;
            }
            set
            {
                this._SkillMaxLevel = value;
                this.UpdateButtonsVisible();
            }
        }

        private int _AdditionLevel = 0;
        /// <summary>
        /// Điểm kỹ năng được cộng thêm từ trang bị hoặc kỹ năng khác
        /// </summary>
        public int AdditionLevel
        {
            get
            {
                return this._AdditionLevel;
            }
            set
            {
                this._AdditionLevel = value;
                this.UpdateVisible();
                this.UpdateButtonsVisible();
            }
        }

        /// <summary>
        /// Điểm cộng đã được người chơi thay đổi vào đây
        /// </summary>
        public bool ContentChanged
        {
            get
            {
                return this.TempSkillLevel != this._SkillLevel;
            }
        }

        /// <summary>
        /// Số điểm kỹ năng trở về 0
        /// </summary>
        public bool SkillPointBackToZero
        {
            set
            {
                if (value)
                {
                    this.UIButton_AddPoint.interactable = false;
                }
                else
                {
                    this.UpdateButtonsVisible();
                }
            }
        }

        /// <summary>
        /// Sự kiện hiển thị thông tin kỹ năng
        /// </summary>
        public Action ShowSkillInfo { get; set; }

        /// <summary>
        /// Sự kiện khi điểm được phân phối ở ô kỹ năng này
        /// </summary>
        public Action PointAdd { get; set; }

        /// <summary>
        /// Sự kiện khi trừ đi ở ô kỹ năng này
        /// </summary>
        public Action PointSub { get; set; }
        #endregion

        /// <summary>
        /// Đối tượng Image của Icon
        /// </summary>
        private UnityEngine.UI.Image iconImage;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.iconImage = this.UIImage_Icon.gameObject.GetComponent<UnityEngine.UI.Image>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.UpdateVisible();
            this.UpdateButtonsVisible();
            this.UpdateSkillMaxLevel();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_AddPoint.onClick.AddListener(this.ButtonAddPoint_Clicked);
            this.UIButton_SubPoint.onClick.AddListener(this.ButtonSubPoint_Clicked);
            this.UIButton_Icon.onClick.AddListener(this.ButtonIcon_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button cộng điểm kỹ năng được ấn
        /// </summary>
        private void ButtonAddPoint_Clicked()
        {
            if (!this._CanStudy)
            {
                return;
            }

            this.TempSkillLevel++;
            this.UIText_Level.text = this.TempSkillLevel.ToString();
            this.UpdateButtonsVisible();
            this.PointAdd?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button trừ điểm kỹ năng được ấn
        /// </summary>
        private void ButtonSubPoint_Clicked()
        {
            if (!this._CanStudy)
            {
                return;
            }

            this.TempSkillLevel--;
            this.UIText_Level.text = this.TempSkillLevel.ToString();
            this.UpdateButtonsVisible();
            this.PointSub?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button Icon được ấn
        /// </summary>
        private void ButtonIcon_Clicked()
        {
            this.ShowSkillInfo?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật hiển thị dựa theo trạng thái của đối tượng
        /// </summary>
        private void UpdateVisible()
        {
            if (this._SkillLevel > 0)
            {
                this.UIText_Level.text = this._SkillLevel.ToString();
                ColorUtility.TryParseHtmlString("#FFFFFF", out Color color);
                this.iconImage.color = color;
                this.UIText_AdditionLevel.text = "+" + this._AdditionLevel.ToString();
            }
            else
            {
                this.UIText_Level.text = "Chưa học";
                ColorUtility.TryParseHtmlString("#797979", out Color color);
                this.iconImage.color = color;
                this.UIText_AdditionLevel.text = "";
            }
        }

        /// <summary>
        /// Cập nhật hiển thị Button theo trạng thái của đối tượng
        /// </summary>
        private void UpdateButtonsVisible()
        {
            if (!this._CanStudy || this._SkillMaxLevel <= 0)
            {
                this.UIButton_AddPoint.gameObject.SetActive(false);
                this.UIButton_SubPoint.gameObject.SetActive(false);
            }
            else
            {
                this.UIButton_AddPoint.gameObject.SetActive(true);
                this.UIButton_SubPoint.gameObject.SetActive(true);

                if (this.TempSkillLevel >= this._SkillMaxLevel)
                {
                    this.UIButton_AddPoint.interactable = false;
                }
                else
                {
                    this.UIButton_AddPoint.interactable = true;
                }

                if (this.TempSkillLevel > 0 && this.TempSkillLevel > this._SkillLevel)
                {
                    this.UIButton_SubPoint.interactable = true;
                }
                else
                {
                    this.UIButton_SubPoint.interactable = false;
                }
            }
        }

        /// <summary>
        /// Cập nhật hiển thị cấp độ tối đa kỹ năng
        /// </summary>
        private void UpdateSkillMaxLevel()
        {
            this.UIButton_AddPoint.interactable = this._SkillMaxLevel > this._SkillLevel;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sự kiện khi điểm kỹ năng của nhân vật thay đổi
        /// </summary>
        /// <param name="currentPoint"></param>
        public void LeaderSkillPointChanged(int currentPoint)
        {
            if (currentPoint <= 0)
            {
                this.UIButton_AddPoint.interactable = false;
            }
        }

        /// <summary>
        /// Làm mới đối tượng
        /// </summary>
        public void Refresh()
        {
            this.UIImage_Icon.Load();
        }
        #endregion
    }
}