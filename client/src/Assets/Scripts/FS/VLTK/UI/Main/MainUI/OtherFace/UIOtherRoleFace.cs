using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using FS.VLTK.Entities.Object;
using FS.VLTK.Factory;
using FS.VLTK.Entities.Config;
using System;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Thông tin mục tiêu - Người chơi khác
    /// </summary>
    public class UIOtherRoleFace : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Image Avarta
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UIImage_Avarta;

        /// <summary>
        /// Text tên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Slider sinh lực
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_HPBar;

        /// <summary>
        /// Text thông tin máu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_HP;

        /// <summary>
        /// Image ngũ hành
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_Elemental;

        /// <summary>
        /// Text tên môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionName;

        /// <summary>
        /// Button chọn đối tượng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;
        #endregion

        #region Properties
        private int _RoleID;
        /// <summary>
        /// ID đối tượng
        /// </summary>
        public int RoleID
        {
            get
            {
                return this._RoleID;
            }
            set
            {
                this._RoleID = value;
                this.UIImage_Avarta.RoleID = value;
            }
        }

        private int _FactionID = -1;
        /// <summary>
        /// ID môn phái
        /// </summary>
        public int FactionID
        {
            get
            {
                return this._FactionID;
            }
            set
            {
                if (this._FactionID == value)
                {
                    return;
                }
                this._FactionID = value;
                this.UpdateFaction();
            }
        }

        private int _RoleAvartaID = -1;
        /// <summary>
        /// ID mặt
        /// </summary>
        public int RoleAvartaID
        {
            get
            {
                return this._RoleAvartaID;
            }
            set
            {
                if (this._RoleAvartaID == value)
                {
                    return;
                }
                this._RoleAvartaID = value;
                this.UpdateAvarta();
            }
        }

        private string _Name;
        /// <summary>
        /// Tên
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                this.UIText_Name.text = value;
            }
        }

        private int _Level;
        /// <summary>
        /// Cấp độ
        /// </summary>
        public int Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                this._Level = value;
                this.UIText_Level.text = value.ToString();
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
                this.UpdateHP();
            }
        }

        private int _HPMax;
        /// <summary>
        /// Sinh lực thượng hạn
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

        /// <summary>
        /// Hiển thị
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Sự kiện khi Avarta của đối tượng được ấn
        /// </summary>
        public Action AvartaClicked { get; set; }

        /// <summary>
        /// Thiết lập Avarta có Click được không
        /// </summary>
        public bool AvartaClickable
        {
            get
            {
                return this.UIButton.interactable;
            }
            set
            {
                this.UIButton.interactable = value;
            }
        }

        /// <summary>
        /// Dữ liệu quái
        /// </summary>
        public RoleData Data { get; set; }
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
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.StartCoroutine(this.AutoCloseIfTargetNotFound());
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
            this.Data = null;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton.onClick.AddListener(this.ButtonAvarta_Clicked);
        }

        private void ButtonAvarta_Clicked()
        {
            this.AvartaClicked?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Tự động đóng khung nếu mục tiêu không tồn tại
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoCloseIfTargetNotFound()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                if (this.Data == null || (!Global.Data.OtherRoles.TryGetValue(this.Data.RoleID, out _) && !Global.Data.Bots.TryGetValue(this.Data.RoleID, out _)))
                {
                    this.Visible = false;
                }
            }
        }

        /// <summary>
        /// Cập nhật môn phái
        /// </summary>
        private void UpdateFaction()
        {
            FactionXML faction = Loader.Loader.Factions[this._FactionID];
            this.UIText_FactionName.text = faction.Name;
            if (faction.ID == 0)
            {
                this.UIImage_Elemental.gameObject.SetActive(false);
            }
            else
            {
                this.UIImage_Elemental.gameObject.SetActive(true);
                UnityEngine.Sprite elementSprite = Loader.Loader.Elements[faction.Elemental].NormalSprite;
                this.UIImage_Elemental.sprite = elementSprite;
            }
        }

        /// <summary>
        /// Cập nhật mặt
        /// </summary>
        private void UpdateAvarta()
        {
            this.UIImage_Avarta.AvartaID = this.RoleAvartaID;
        }

        /// <summary>
        /// Cập nhật thanh máu
        /// </summary>
        private void UpdateHP()
        {
            float percent;
            if (this._HPMax <= 0)
            {
                percent = 0;
            }
            else
            {
                percent = this._HP / (float)this._HPMax;
            }

            this.UISlider_HPBar.value = percent;
            this.UIText_HP.text = string.Format("{0}/{1}", this._HP, this._HPMax);
        }
        #endregion

        #region Public methods
        #endregion
    }
}