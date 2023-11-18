using FS.VLTK.UI.Main.MainUI.RolePart;
using FS.VLTK.Utilities.UnityUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.VLTK.Factory;
using FS.VLTK.Entities.Object;
using System;
using FS.VLTK.Entities.Config;
using System.Linq;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung thuộc tính nhân vật ở Main UI
    /// </summary>
    public class UIRolePart : MonoBehaviour
    {
        /// <summary>
        /// Sprite tương ứng trạng thái PK
        /// </summary>
        [Serializable]
        private class PKModeSprite
        {
            /// <summary>
            /// Trạng thái PK
            /// </summary>
            public Entities.Enum.PKMode PKMode;

            /// <summary>
            /// Tên trạng thái
            /// </summary>
            public string Name;

            /// <summary>
            /// Màu
            /// </summary>
            public Color Color = Color.white;
        }

        #region Define
        /// <summary>
        /// Khung thông tin Ping
        /// </summary>
        [SerializeField]
        private UIRolePart_PingInfo UI_PingInfo;

        /// <summary>
        /// Toggle mặt nhân vật
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle_RolePart;

        /// <summary>
        /// Danh sách Buff
        /// </summary>
        [SerializeField]
        private UIRolePart_BuffList UI_ListBuff;

        /// <summary>
        /// Avarta nhân vật
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UIImage_RoleAvarta;

        /// <summary>
        /// Text tên nhân vật
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleName;

        /// <summary>
        /// Text cấp độ nhân vật
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleLevel;

        /// <summary>
        /// Ảnh ngũ hành
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_ElementalIcon;

        /// <summary>
        /// Button mở khung quản lý mật khẩu cấp 2
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenSecondPasswordManager;

        /// <summary>
        /// Button trạng thái PK
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_PKStatus;

        /// <summary>
        /// Bảng thông tin PK
        /// </summary>
        [SerializeField]
        private UIRolePart_PKSelectionBox UIRect_PKDetailBox;

        /// <summary>
        /// Slider thanh máu
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_HPBar;

        /// <summary>
        /// Text máu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_HPValue;

        /// <summary>
        /// Slider thanh khí
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_MPBar;

        /// <summary>
        /// Text khí
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MPValue;

        /// <summary>
        /// Slider thanh thể lực
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_VitalityBar;

        /// <summary>
        /// Text thể lực
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_VitalityValue;

        /// <summary>
        /// Slider thanh kinh nghiệm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_ExpBar;

        /// <summary>
        /// Text % kinh nghiệm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ExpValue;

        /// <summary>
        /// Danh sách Sprite tương ứng trạng thái PK
        /// </summary>
        [SerializeField]
        private PKModeSprite[] UI_PKModeSprites;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện trạng thái PK được chọn
        /// </summary>
        public UIRolePart_PKSelectionBox UIPKSelection
        {
            get
            {
                return this.UIRect_PKDetailBox;
            }
        }

        /// <summary>
        /// Khung danh sách Buff
        /// </summary>
        public UIRolePart_BuffList UIBuffList
        {
            get
            {
                return this.UI_ListBuff;
            }
        }

        /// <summary>
        /// Khung thông tin Ping
        /// </summary>
        public UIRolePart_PingInfo UIPingInfo
        {
            get
            {
                return this.UI_PingInfo;
            }
        }

        /// <summary>
        /// Tên nhân vật
        /// </summary>
        public string RoleName
        {
            get
            {
                return this.UIText_RoleName.text;
            }
            set
            {
                this.UIText_RoleName.text = value;
            }
        }

        /// <summary>
        /// Cấp độ nhân vật
        /// </summary>
        public int RoleLevel
        {
            get
            {
                return int.Parse(this.UIText_RoleLevel.text);
            }
            set
            {
                this.UIText_RoleLevel.text = value.ToString();
            }
        }

        private int _RoleAvartaID;
        /// <summary>
        /// ID Avarta nhân vật
        /// </summary>
        public int RoleAvartaID
        {
            get
            {
                return this._RoleAvartaID;
            }
            set
            {
                this._RoleAvartaID = value;
                this.UpdateAvarta();
            }
        }

        /// <summary>
        /// Sự kiện mở khung quản lý mật khẩu cấp 2
        /// </summary>
        public Action OpenSecondPasswordManager { get; set; }

        private int _HP;
        /// <summary>
        /// Máu nhân vật
        /// </summary>
        public int HP
        {
            get
            {
                return this._HP;
            }
            set
            {
                this._HP = value;
                this.UpdateHP();
            }
        }

        private int _HPMax;
        /// <summary>
        /// Máu nhân vật
        /// </summary>
        public int HPMax
        {
            get
            {
                return this._HPMax;
            }
            set
            {
                this._HPMax = value;
                this.UpdateHP();
            }
        }

        private int _MP;
        /// <summary>
        /// Khí nhân vật
        /// </summary>
        public int MP
        {
            get
            {
                return this._MP;
            }
            set
            {
                this._MP = value;
                this.UpdateMP();
            }
        }

        private int _MPMax;
        /// <summary>
        /// Khí nhân vật
        /// </summary>
        public int MPMax
        {
            get
            {
                return this._MPMax;
            }
            set
            {
                this._MPMax = value;
                this.UpdateMP();
            }
        }

        private int _Vitality;
        /// <summary>
        /// Thể lực nhân vật
        /// </summary>
        public int Vitality
        {
            get
            {
                return this._Vitality;
            }
            set
            {
                this._Vitality = value;
                this.UpdateVitality();
            }
        }

        private int _VitalityMax;
        /// <summary>
        /// Khí nhân vật
        /// </summary>
        public int VitalityMax
        {
            get
            {
                return this._VitalityMax;
            }
            set
            {
                this._VitalityMax = value;
                this.UpdateVitality();
            }
        }

        private long _Exp;
        /// <summary>
        /// Khí nhân vật
        /// </summary>
        public long Exp
        {
            get
            {
                return this._Exp;
            }
            set
            {
                this._Exp = value;
                this.UpdateExp();
            }
        }

        private long _ExpMax;
        /// <summary>
        /// Khí nhân vật
        /// </summary>
        public long ExpMax
        {
            get
            {
                return this._ExpMax;
            }
            set
            {
                this._ExpMax = value;
                this.UpdateExp();
            }
        }

        private int _FactionID;
        /// <summary>
        /// ID môn phái nhân vật
        /// </summary>
        public int FactionID
        {
            get
            {
                return this._FactionID;
            }
            set
            {
                this._FactionID = value;
                this.UpdateFaction();
            }
        }

        private int _PKMode;
        /// <summary>
        /// Trạng thái PK
        /// </summary>
        public int PKMode
        {
            get
            {
                return this._PKMode;
            }
            set
            {
                this._PKMode = value;
                this.UpdatePKMode();
            }
        }

        /// <summary>
        /// Sự kiện khi mặt nhân vật được ấn
        /// </summary>
        public Action<bool> RoleFaceSelected { get; set; }
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
            this.UIButton_PKStatus.Click = this.ButtonPKStatus_Clicked;
            this.UIButton_OpenSecondPasswordManager.onClick.AddListener(() =>
            {
                this.OpenSecondPasswordManager?.Invoke();
            });
            this.UIToggle_RolePart.onValueChanged.AddListener(this.ToggleRoleFace_Selected);
            this.UIImage_RoleAvarta.RoleID = Global.Data.RoleData.RoleID;
        }

        /// <summary>
        /// Sự kiện khi Button trạng thái PK được ấn
        /// </summary>
        private void ButtonPKStatus_Clicked()
        {
            this.UIRect_PKDetailBox.Visible = !this.UIRect_PKDetailBox.Visible;
        }

        /// <summary>
        /// Sự kiện khi mặt nhân vật được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleRoleFace_Selected(bool isSelected)
        {
            this.RoleFaceSelected?.Invoke(isSelected);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật máu nhân vật
        /// </summary>
        private void UpdateHP()
        {
            if (this._HPMax == 0)
            {
                return;
            }
            float percent = this._HP / (float) this._HPMax;
            this.UISlider_HPBar.value = percent;
            this.UIText_HPValue.text = string.Format("{0}/{1}", this._HP, this._HPMax);
        }

        /// <summary>
        /// Cập nhật khí nhân vật
        /// </summary>
        private void UpdateMP()
        {
            if (this._MPMax == 0)
            {
                return;
            }
            float percent = this._MP / (float) this._MPMax;
            this.UISlider_MPBar.value = percent;
            this.UIText_MPValue.text = string.Format("{0}/{1}", this._MP, this._MPMax);
        }

        /// <summary>
        /// Cập nhật thể lực nhân vật
        /// </summary>
        private void UpdateVitality()
        {
            if (this._VitalityMax == 0)
            {
                return;
            }
            float percent = this._Vitality / (float) this._VitalityMax;
            this.UISlider_VitalityBar.value = percent;
            this.UIText_VitalityValue.text = string.Format("{0}/{1}", this._Vitality, this._VitalityMax);
        }

        /// <summary>
        /// Cập nhật kinh nghiệm nhân vật
        /// </summary>
        private void UpdateExp()
        {
            if (this._ExpMax == 0)
            {
                return;
            }
            float percent = this._Exp / (float) this._ExpMax;
            this.UISlider_ExpBar.value = percent;
            this.UIText_ExpValue.text = string.Format("{0}%", Utils.Truncate(percent * 100f, 1));
        }

        /// <summary>
        /// Cập nhật môn phái
        /// </summary>
        private void UpdateFaction()
        {
            FactionXML faction = Loader.Loader.Factions[this._FactionID];
            if (faction.ID == 0)
            {
                this.UIImage_ElementalIcon.gameObject.SetActive(false);
            }
            else
            {
                this.UIImage_ElementalIcon.gameObject.SetActive(true);
                UnityEngine.Sprite elementSprite = Loader.Loader.Elements[faction.Elemental].NormalSprite;
                this.UIImage_ElementalIcon.sprite = elementSprite;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái PK
        /// </summary>
        private void UpdatePKMode()
        {
            PKModeSprite sprite = this.UI_PKModeSprites.Where(x => x.PKMode == (Entities.Enum.PKMode) this.PKMode).FirstOrDefault();
            if (sprite == null)
            {
                sprite = this.UI_PKModeSprites.Where(x => x.PKMode == Entities.Enum.PKMode.Peace).FirstOrDefault();
            }

            this.UIButton_PKStatus.Name = sprite.Name;
            this.UIButton_PKStatus.TextColor = sprite.Color;
            this.UIButton_PKStatus.Refresh(true);
        }

        /// <summary>
        /// Cập nhật mặt
        /// </summary>
        private void UpdateAvarta()
        {
            this.UIImage_RoleAvarta.AvartaID = Global.Data.RoleData.RolePic;
        }
        #endregion
    }
}

