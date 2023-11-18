using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.SelectServer
{
    /// <summary>
    /// Button thông tin máy chủ trong khung chọn máy chủ
    /// </summary>
    public class UISelectServer_ButtonServerDetails : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button 
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ServerDetails;

        /// <summary>
        /// Text tên máy chủ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ServerName;

        /// <summary>
        /// Trạng thái máy chủ
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_ServerStatus;

        /// <summary>
        /// Text trạng thái máy chủ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ServerDescription;

        /// <summary>
        /// Tên Sprite ở trạng thái bảo trì
        /// </summary>
        [SerializeField]
        private string MaintenanceSprite;

        /// <summary>
        /// Tên Sprite ở trạng thái tốt
        /// </summary>
        [SerializeField]
        private string GoodSprite;

        /// <summary>
        /// Tên Sprite ở trạng thái bận
        /// </summary>
        [SerializeField]
        private string BusySprite;

        /// <summary>
        /// Tên Sprite ở trạng thái đày
        /// </summary>
        [SerializeField]
        private string FullSprite;
        #endregion

        #region Properties
        /// <summary>
        /// Tên máy chủ
        /// </summary>
        public string ServerName
        {
            get
            {
                return this.UIText_ServerName.text;
            }
            set
            {
                this.UIText_ServerName.text = value;
            }
        }

        private int _Status;
        /// <summary>
        /// Trạng thái máy chủ
        /// </summary>
        public int Status
        {
            get
            {
                return this._Status;
            }
            set
            {
                switch (value)
                {
                    /// Tốt
                    case 4:
                    {
                        this.UIImage_ServerStatus.SpriteName = this.GoodSprite;
                        break;
                    }
                    /// Bận
                    case 3:
                    {
                        this.UIImage_ServerStatus.SpriteName = this.BusySprite;
                        break;
                    }
                    /// Đầy
                    case 2:
                    {
                        this.UIImage_ServerStatus.SpriteName = this.FullSprite;
                        break;
                    }
                    /// Bảo trì
                    default:
                    {
                        this.UIImage_ServerStatus.SpriteName = this.MaintenanceSprite;
                        break;
                    }
                }
                this.UIImage_ServerStatus.Load();
            }
        }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description
        {
            get
            {
                return this.UIText_ServerDescription.text;
            }
            set
            {
                this.UIText_ServerDescription.text = value;
            }
        }

        /// <summary>
        /// Sự kiện chọn máy chủ
        /// </summary>
        public Action Select { get; set; }
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
            this.UIButton_ServerDetails.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được chọn
        /// </summary>
        private void Button_Clicked()
        {
            this.Select?.Invoke();
        }
        #endregion
    }
}
