using System.Collections;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Thông tin mục tiêu - Pet
    /// </summary>
    public class UIPetFace : MonoBehaviour
    {
        #region Define
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
        /// Dữ liệu quái
        /// </summary>
        public PetDataMini Data { get; set; }
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
                yield return new WaitForSeconds(0.5f);
                if (this.Data == null || KTGlobal.FindSpriteByID(this.Data.ID) == null)
                {
                    this.Visible = false;
                }
            }
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
                percent = this._HP / (float) this._HPMax;
            }

            this.UISlider_HPBar.value = percent;
            this.UIText_HP.text = string.Format("{0}/{1}", this._HP, this._HPMax);
        }
        #endregion

        #region Public methods
        #endregion
    }
}