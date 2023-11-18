using FS.GameEngine.Logic;
using Server.Data;
using System.Collections;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Thông tin mục tiêu - Quái
    /// </summary>
    public class UIMonsterFace : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Image Avarta
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_Avarta;

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

        private Elemental _Elemental;
        /// <summary>
        /// Ngũ hành
        /// </summary>
        public Elemental Elemental
        {
            get
            {
                return this._Elemental;
            }
            set
            {
                this._Elemental = value;
                this.UpdateElementalAvarta();
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
            // this.Data = null;
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
            /// Lặp liên tục
            while (true)
            {
                /// Nghỉ 0.2s
                yield return new WaitForSeconds(0.2f);

                /// Nếu là quái
                if (Global.Data.SystemMonsters.TryGetValue(this._RoleID, out MonsterData md))
                {
                    goto CONTINUE;
                }
                /// Nếu là xe tiêu
                else if (Global.Data.TraderCarriages.TryGetValue(this._RoleID, out TraderCarriageData td))
                {
                    goto CONTINUE;
                }

                /// Không tìm thấy thì ẩn đi
                this.Visible = false;

            CONTINUE:
                yield return null;
            }
        }

        /// <summary>
        /// Cập nhật Avarta ngũ hành
        /// </summary>
        private void UpdateElementalAvarta()
        {
            if (Loader.Loader.Elements.TryGetValue(this._Elemental, out Entities.Object.ElementData elementData))
            {
                this.UIImage_Avarta.gameObject.SetActive(true);
                UnityEngine.Sprite elementSprite = elementData.BigSprite;
                this.UIImage_Avarta.sprite = elementSprite;
            }
            else
            {
                this.UIImage_Avarta.gameObject.SetActive(false);
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