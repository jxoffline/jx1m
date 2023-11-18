using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace FS.VLTK.UI.Main.RoleInfo
{
    /// <summary>
    /// Tab chỉ số nhân vật
    /// </summary>
    public class UIRoleInfo_PropertiesTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button mở bảng cộng điểm tiềm năng nhân vật
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenRoleRemainPointFrame;

        /// <summary>
        /// Text Điểm tiềm năng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RemainPoint;

        /// <summary>
        /// Text Sinh lực
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_HPText;

        /// <summary>
        /// Slider thanh Sinh lực
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_HPBar;

        /// <summary>
        /// Text Nội lực
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MPText;

        /// <summary>
        /// Slider thanh Nội lực
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_MPBar;

        /// <summary>
        /// Text Thể lực
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_VitalityText;

        /// <summary>
        /// Slider thanh Thể lực
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_VitalityBar;

        /// <summary>
        /// Text Tinh lực
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Energy;

        /// <summary>
        /// Text Hoạt lực
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Potential;

        /// <summary>
        /// Text Lực tay
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Damage;

        /// <summary>
        /// Text Chí mạng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CritAtk;

        /// <summary>
        /// Text Tốc chạy
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MoveSpeed;

        /// <summary>
        /// Text Chính xác
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Hit;

        /// <summary>
        /// Text Né tránh
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Dodge;

        /// <summary>
        /// Text tốc đánh ngoại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AtkSpeed;

        /// <summary>
        /// Text tốc đánh nội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CastSpeed;

        /// <summary>
        /// Text kháng vật
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Def;

        /// <summary>
        /// Text kháng băng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_IceRes;

        /// <summary>
        /// Text kháng hỏa
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FireRes;

        /// <summary>
        /// Text kháng lôi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LightningRes;

        /// <summary>
        /// Text kháng độc
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PoisonRes;
        #endregion

        #region Properties
        private int _RemainPoint;
        /// <summary>
        /// Tiềm năng
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
            }
        }
        
        private int _HP;
        /// <summary>
        /// Sinh lực
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
                this.RefreshHP();
            }
        }
        
        private int _HPMax;
        /// <summary>
        /// Sinh lực tối đa
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
                this.RefreshHP();
            }
        }
        
        private int _MP;
        /// <summary>
        /// Nội lực
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
                this.RefreshMP();
            }
        }
        
        private int _MPMax;
        /// <summary>
        /// Nội lực tối đa
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
                this.RefreshMP();
            }
        }
        
        private int _Vitality;
        /// <summary>
        /// Thể lực
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
                this.RefreshVitality();
            }
        }
        
        private int _VitalityMax;
        /// <summary>
        /// Thể lực tối đa
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
                this.RefreshVitality();
            }
        }

        private int _Energy;
        /// <summary>
        /// Tinh lực
        /// </summary>
        public int Energy
        {
            get
            {
                return this._Energy;
            }
            set
            {
                this._Energy = value;
                this.UIText_Energy.text = value.ToString();
            }
        }

        private int _Potential;
        /// <summary>
        /// Hoạt lực
        /// </summary>
        public int Potential
        {
            get
            {
                return this._Potential;
            }
            set
            {
                this._Potential = value;
                this.UIText_Potential.text = value.ToString();
            }
        }

        private int _Damage;
        /// <summary>
        /// Lực Tay
        /// </summary>
        public int Damage
        {
            get
            {
                return this._Damage;
            }
            set
            {
                this._Damage = value;
                this.UIText_Damage.text = value.ToString();
            }
        }

        private int _MoveSpeed;
        /// <summary>
        ///  Tốc Chạy
        /// </summary>
        public int MoveSpeed
        {
            get
            {
                return this._MoveSpeed;
            }
            set
            {
                this._MoveSpeed = value;
                this.UIText_MoveSpeed.text = value.ToString();
            }
        }

        private int _CritAtk;
        /// <summary>
        ///  Chí Mạng
        /// </summary>
        public int CritAtk
        {
            get
            {
                return this._CritAtk;
            }
            set
            {
                this._CritAtk = value;
                this.UIText_CritAtk.text = string.Format("{0}%", this._CritAtk);
            }
        }

        private int _Hit;
        /// <summary>
        ///  Chính xác 
        /// </summary>
        public int Hit
        {
            get
            {
                return this._Hit;
            }
            set
            {
                this._Hit = value;
                this.UIText_Hit.text = value.ToString();
            }
        }

        private int _Dodge;
        /// <summary>
        ///  Né Tránh
        /// </summary>
        public int Dodge
        {
            get
            {
                return this._Dodge;
            }
            set
            {
                this._Dodge = value;
                this.UIText_Dodge.text = value.ToString();
            }
        }

        private int _AtkSpeed;
        /// <summary>
        /// Tốc đánh
        /// </summary>
        public int AtkSpeed
        {
            get
            {
                return this._AtkSpeed;
            }
            set
            {
                this._AtkSpeed = value;
                this.UIText_AtkSpeed.text = value.ToString();
            }
        }

        private int _CastSpeed;
        /// <summary>
        /// Tốc đánh
        /// </summary>
        public int CastSpeed
        {
            get
            {
                return this._CastSpeed;
            }
            set
            {
                this._CastSpeed = value;
                this.UIText_CastSpeed.text = value.ToString();
            }
        }

        private int _Def;
        /// <summary>
        /// Kháng băng
        /// </summary>
        public int Def
        {
            get
            {
                return this._Def;
            }
            set
            {
                this._Def = value;
                this.UIText_Def.text = value.ToString();
            }
        }

        private int _IceRes;
        /// <summary>
        /// Kháng băng
        /// </summary>
        public int IceRes
        {
            get
            {
                return this._IceRes;
            }
            set
            {
                this._IceRes = value;
                this.UIText_IceRes.text = value.ToString();
            }
        }

        private int _FireRes;
        /// <summary>
        /// Kháng hỏa
        /// </summary>
        public int FireRes
        {
            get
            {
                return this._FireRes;
            }
            set
            {
                this._FireRes = value;
                this.UIText_FireRes.text = value.ToString();
            }
        }

        private int _LightningRes;
        /// <summary>
        /// Kháng lôi
        /// </summary>
        public int LightningRes
        {
            get
            {
                return this._LightningRes;
            }
            set
            {
                this._LightningRes = value;
                this.UIText_LightningRes.text = value.ToString();
            }
        }

        private int _PoisonRes;
        /// <summary>
        /// Kháng độc
        /// </summary>
        public int PoisonRes
        {
            get
            {
                return this._PoisonRes;
            }
            set
            {
                this._PoisonRes = value;
                this.UIText_PoisonRes.text = value.ToString();
            }
        }

        /// <summary>
        /// Sự kiện khi Button Mở khung tiềm năng nhân vật được ấn
        /// </summary>
        public Action OpenRoleRemainPointFrame { get; set; }
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
            this.UIButton_OpenRoleRemainPointFrame.onClick.AddListener(this.ButtonOpenRoleRemainPointFrame_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button Mở khung tiềm năng nhân vật được ấn
        /// </summary>
        private void ButtonOpenRoleRemainPointFrame_Clicked()
        {
            this.OpenRoleRemainPointFrame?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới dữ liệu sinh lực
        /// </summary>
        private void RefreshHP()
        {
            if (this._HPMax > 0)
            {
                this.UIText_HPText.text = string.Format("{0}/{1}", this._HP, this._HPMax);
                this.UISlider_HPBar.value = this._HP / (float) this._HPMax;
            }
        }

        /// <summary>
        /// Làm mới dữ liệu nội lực
        /// </summary>
        private void RefreshMP()
        {
            if (this._MPMax > 0)
            {
                this.UIText_MPText.text = string.Format("{0}/{1}", this._MP, this._MPMax);
                this.UISlider_MPBar.value = this._MP / (float) this._MPMax;
            }
        }

        /// <summary>
        /// Làm mới dữ liệu Thể lực
        /// </summary>
        private void RefreshVitality()
        {
            if (this._VitalityMax > 0)
            {
                this.UIText_VitalityText.text = string.Format("{0}/{1}", this._Vitality, this._VitalityMax);
                this.UISlider_VitalityBar.value = this._Vitality / (float) this._VitalityMax;
            }
        }
        #endregion
    }
}