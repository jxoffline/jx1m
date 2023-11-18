using System.Collections;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using System;
using FS.VLTK.Entities.Config;
using System.Linq;
using FS.VLTK.UI.UICore;
using FS.VLTK.Utilities.UnityComponent;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;

namespace FS.VLTK.UI.CoreUI
{
    /// <summary>
    /// Lớp quản lý text trên thông tin nhân vật
    /// </summary>
    public class UIRoleHeader : TTMonoBehaviour
    {
        /// <summary>
        /// Icon trạng thái PK
        /// </summary>
        [Serializable]
        private class PKValueIconSprite
        {
            /// <summary>
            /// Trị PK
            /// </summary>
            public int Value;

            /// <summary>
            /// Sprite Icon
            /// </summary>
            public string SpriteName;
        }

        #region Define
        /// <summary>
        /// Thanh máu
        /// </summary>
        [SerializeField]
        private UISpriteHPBar UISlider_HPBar;

        /// <summary>
        /// Thanh mana
        /// </summary>
        [SerializeField]
        private UISpriteHPBar UISlider_MPBar;

        /// <summary>
        /// Text tên nhân vật
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_Name;

        /// <summary>
        /// Text danh hiệu bang hội
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_GuildTitle;

        /// <summary>
        /// Danh hiệu nhân vật
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_RoleTitle;

        /// <summary>
        /// Text danh hiệu tạm
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_Title;

        /// <summary>
        /// Ảnh danh hiệu đặc biệt
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIImage_SpecialTitle;

        /// <summary>
        /// Ảnh danh hiệu quan hàm
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIImage_OfficeTitle;

        /// <summary>
        /// Ảnh danh hiệu phi phong
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIImage_MantleTitle;

        /// <summary>
        /// Tọa độ đặt
        /// </summary>
        [SerializeField]
        private Vector2 _Offset;

        /// <summary>
        /// Tọa độ đặt khi cưỡi ngựa
        /// </summary>
        [SerializeField]
        private Vector2 _RiderOffset;

        /// <summary>
        /// Icon trưởng nhóm
        /// </summary>
        [SerializeField]
        private Transform UIImage_TeamLeaderIcon;

        /// <summary>
        /// Icon đội viên
        /// </summary>
        [SerializeField]
        private Transform UIImage_TeamMemberIcon;

        /// <summary>
        /// Icon lượng sát khí hiện có
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_PKValueIcon;

        /// <summary>
        /// Danh sách Sprite tương đương sát khí
        /// </summary>
        [SerializeField]
        private PKValueIconSprite[] PKValueSprites;

        /// <summary>
        /// Text nội dung chat
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_ChatContent;

        /// <summary>
        /// Text tên cửa hàng
        /// </summary>
        [SerializeField]
        private TextMeshPro UIText_StallName;
        #endregion

        #region Properties
        /// <summary>
        /// Màu của thanh máu
        /// </summary>
        public Color HPBarColor
        {
            get
            {
                return this.UISlider_HPBar.Color;
            }
            set
            {
                this.UISlider_HPBar.Color = value;
            }
        }

        /// <summary>
        /// Màu của tên
        /// </summary>
        public Color NameColor
        {
            get
            {
                return this.UIText_Name.color;
            }
            set
            {
                this.UIText_Name.color = value;
            }
        }

        /// <summary>
        /// Màu của danh hiệu bang
        /// </summary>
        public Color GuildTitleColor
        {
            get
            {
                return this.UIText_GuildTitle.color;
            }
            set
            {
                this.UIText_GuildTitle.color = value;
            }
        }

        /// <summary>
        /// Hiển thị thanh khí
        /// </summary>
        public bool IsShowMPBar
        {
            get
            {
                return this.UISlider_MPBar.gameObject.activeSelf;
            }
            set
            {
                this.UISlider_MPBar.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// % máu
        /// </summary>
        public int HPPercent
        {
            get
            {
                return (int) (this.UISlider_HPBar.Value * 100);
            }
            set
            {
                this.UISlider_HPBar.Value = value / 100f;
            }
        }

        /// <summary>
        /// % khí
        /// </summary>
        public int MPPercent
        {
            get
            {
                return (int) (this.UISlider_MPBar.Value * 100);
            }
            set
            {
                this.UISlider_MPBar.Value = value / 100f;
            }
        }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        public string Name
        {
            get
            {
                return this.UIText_Name.text;
            }
            set
            {
                this.UIText_Name.text = value;
                this.RebuildLayout();
            }
        }

        private string _GuildTitle = "";
        /// <summary>
        /// Danh hiệu bang hội
        /// </summary>
        public string GuildTitle
        {
            get
            {
                return this._GuildTitle;
            }
            set
            {
                this._GuildTitle = value;

                if (string.IsNullOrEmpty(value))
                {
                    this.UIText_GuildTitle.gameObject.SetActive(false);
                }
                else
                {
                    this.UIText_GuildTitle.gameObject.SetActive(true);
                    this.UIText_GuildTitle.text = value;
                }
                this.RebuildLayout();
            }
        }

        private string _Title = "";
        /// <summary>
        /// Danh hiệu
        /// </summary>
        public string Title
        {
            get
            {
                return this._Title;
            }
            set
            {
                this._Title = value;

                if (string.IsNullOrEmpty(value))
                {
                    this.UIText_Title.gameObject.SetActive(false);
                }
                else
                {
                    this.UIText_Title.gameObject.SetActive(true);
                    this.UIText_Title.text = value;
                }
                this.RebuildLayout();
            }
        }

        private int _PKValue = 0;
        /// <summary>
        /// Trị PK
        /// </summary>
        public int PKValue
        {
            get
            {
                return this._PKValue;
            }
            set
            {
                this._PKValue = value;
                /// Nếu không có sát khí
                if (value <= 0)
                {
                    this.UIImage_PKValueIcon.gameObject.SetActive(false);
                }
                else
                {
                    this.UIImage_PKValueIcon.gameObject.SetActive(true);

                    int nValue = value;
                    if (nValue > this.PKValueSprites.Length)
                    {
                        nValue = this.PKValueSprites.Length;
                    }
                    this.UIImage_PKValueIcon.SpriteName = this.PKValueSprites[nValue - 1].SpriteName;
                    this.UIImage_PKValueIcon.Load();
                }
            }
        }

        /// <summary>
        /// Hiển thị thanh máu không
        /// </summary>
        public bool SystemSettingShowHPBar
        {
            get
            {
                return this.UISlider_HPBar.gameObject.activeSelf;
            }
            set
            {
                this.UISlider_HPBar.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Có hiển thị tên, danh hiệu không
        /// </summary>
        public bool SystemSettingShowName
        {
            get
            {
                return this.UIText_Name.gameObject.activeSelf;
            }
            set
            {
                this.UIText_Name.gameObject.SetActive(value);
                if (!string.IsNullOrEmpty(this.Title))
                {
                    this.UIText_Title.gameObject.SetActive(value);
                }
                if (!string.IsNullOrEmpty(this.GuildTitle))
                {
                    this.UIText_GuildTitle.gameObject.SetActive(value);
                }
                //this.UIImage_CoatTitle.gameObject.SetActive(value);
                //this.UIImage_RoyalSealTitle.gameObject.SetActive(value);
            }
        }

        private int _TeamType = 0;
        /// <summary>
        /// Loại tổ đội
        /// <para>1: Đội trưởng, 2: Đội viên, Giá trị khác: Ẩn icon</para>
        /// </summary>
        public int TeamType
        {
            get
            {
                return this._TeamType;
            }
            set
            {
                this._TeamType = value;

                this.UIImage_TeamLeaderIcon.gameObject.SetActive(false);
                this.UIImage_TeamMemberIcon.gameObject.SetActive(false);
                if (value == 1)
                {
                    this.UIImage_TeamLeaderIcon.gameObject.SetActive(true);
                }
                else if (value == 2)
                {
                    this.UIImage_TeamMemberIcon.gameObject.SetActive(true);
                }
            }
        }

        private long _RoleValue = 0;
        /// <summary>
        /// Vinh dự tài phú của nhân vật
        /// </summary>
        public long RoleValue
        {
            get
            {
                return this._RoleValue;
            }
            set
            {
                this._RoleValue = value;

                /// Danh hiệu Phi phong tương ứng
                MantleTitleXML mantleTitle = Loader.Loader.MantleTitles.OrderByDescending(x => x.RoleValue).Where(x => x.RoleValue <= value / 10000f).FirstOrDefault();
                /// Nếu không tìm thấy
                if (mantleTitle == null)
                {
                    this.UIImage_MantleTitle.gameObject.SetActive(false);
                }
                /// Nếu tìm thấy
                else
                {
                    this.UIImage_MantleTitle.BundleDir = string.Format("UI/{0}", Loader.Loader.MantleTitlesBundleDir);
                    this.UIImage_MantleTitle.AtlasName = Loader.Loader.MantleTitlesAtlasName;
                    this.UIImage_MantleTitle.SpriteNames = mantleTitle.SpriteNames.ToArray();
                    this.UIImage_MantleTitle.Duration = mantleTitle.AnimationSpeed;
                    this.UIImage_MantleTitle.PixelPerfect = true;
                    this.UIImage_MantleTitle.gameObject.SetActive(true);
                    this.UIImage_MantleTitle.Play();
                }
            }
        }

        private int _SpecialTitleID = -1;
        /// <summary>
        /// ID danh hiệu đặc biệt
        /// </summary>
        public int SpecialTitleID
        {
            get
            {
                return this._SpecialTitleID;
            }
            set
            {
                this._SpecialTitleID = value;

                /// Tìm thông tin danh hiệu tương ứng
                if (!Loader.Loader.SpecialTitles.TryGetValue(value, out KSpecialTitleXML titleData))
                {
                    this.UIImage_SpecialTitle.gameObject.SetActive(false);
                    /// Xây lại giao diện
                    this.RebuildLayout();
                    return;
                }

                this.UIImage_SpecialTitle.BundleDir = string.Format("UI/{0}", titleData.BundleDir);
                this.UIImage_SpecialTitle.AtlasName = titleData.AtlasName;
                this.UIImage_SpecialTitle.SpriteNames = titleData.SpriteNames.ToArray();
                this.UIImage_SpecialTitle.Duration = titleData.AnimationSpeed;
                this.UIImage_SpecialTitle.PixelPerfect = true;
                this.UIImage_SpecialTitle.gameObject.SetActive(true);
                this.UIImage_SpecialTitle.Play();
                /// Xây lại giao diện
                this.RebuildLayout();
            }
        }

        private int _RoleOfficeRank = -1;
        /// <summary>
        /// ID quan hàm hiện tại
        /// </summary>
        public int RoleOfficeRank
		{
			get
			{
                return this._RoleOfficeRank;
			}
			set
			{
                this._RoleOfficeRank = value;

                /// Tìm thông tin danh hiệu tương ứng
                OfficeTitleXML officeTitle = Loader.Loader.OfficeTitles.Where(x => x.ID == value).FirstOrDefault();

                /// Nếu không tìm thấy
                if (officeTitle == null)
				{
                    this.UIImage_OfficeTitle.gameObject.SetActive(false);
                    /// Xây lại giao diện
                    this.RebuildLayout();
                }
				/// Nếu tìm thấy
				else
				{
                    this.UIImage_OfficeTitle.BundleDir = string.Format("UI/{0}", Loader.Loader.OfficeTitlesBundleDir);
                    this.UIImage_OfficeTitle.AtlasName = Loader.Loader.OfficeTitlesAtlasName;
                    this.UIImage_OfficeTitle.SpriteNames = officeTitle.SpriteNames.ToArray();
                    this.UIImage_OfficeTitle.Duration = officeTitle.AnimationSpeed;
                    this.UIImage_OfficeTitle.PixelPerfect = true;
                    this.UIImage_OfficeTitle.gameObject.SetActive(true);
                    this.UIImage_OfficeTitle.Play();
                    /// Xây lại giao diện
                    this.RebuildLayout();
                }
            }
		}

        private int _CurrentRoleTitle = -1;
        /// <summary>
        /// Danh hiệu nhân vật hiện tại
        /// </summary>
        public int CurrentRoleTitle
		{
			get
			{
                return this._CurrentRoleTitle;
			}
			set
			{
                this._CurrentRoleTitle = value;

                /// Thông tin danh hiệu tương ứng
                if (Loader.Loader.RoleTitles != null && Loader.Loader.RoleTitles.TryGetValue(value, out KTitleXML titleXML))
				{
                    this.UIText_RoleTitle.text = titleXML.Text;
                    /// Xây lại giao diện
                    this.RebuildLayout();
                }
                /// Nếu không tồn tại
				else
				{
                    this._CurrentRoleTitle = -1;
                    this.UIText_RoleTitle.text = "";
                    /// Xây lại giao diện
                    this.RebuildLayout();
                }
			}
		}

        /// <summary>
        /// Nội dung Chat
        /// </summary>
        public string ChatContent
        {
            get
            {
                return this.UIText_ChatContent.text;
            }
            set
            {
                this.UIText_ChatContent.text = value;

                /// Nếu đang ẩn
                if (!this.chatBox.gameObject.activeSelf)
                {
                    /// Hiện ChatBox
                    this.chatBox.gameObject.SetActive(true);
                    /// Xây lại giao diện
                    this.RebuildLayout();
                }

                /// Ngừng luồng tự ẩn
                if (this.autoHideChatBoxCoroutine != null)
                {
                    this.StopCoroutine(this.autoHideChatBoxCoroutine);
                }
                /// Bắt đầu tự ẩn
                this.autoHideChatBoxCoroutine = this.StartCoroutine(this.AutoHideChatBox());
            }
        }

        /// <summary>
        /// Tên sạp hàng
        /// </summary>
        public string StallName
        {
            get
            {
                return this.UIText_StallName.text;
            }
            set
            {
                this.UIText_StallName.text = value;

                /// Nếu có cửa hàng
                if (!string.IsNullOrEmpty(value))
                {
                    /// Hiện
                    this.stallBox.gameObject.SetActive(true);
                    /// Xây lại giao diện
                    this.RebuildLayout();
                }
                /// Nếu không có cửa hàng
                else
                {
                    /// Ẩn
                    this.stallBox.gameObject.SetActive(false);
                    /// Xây lại giao diện
                    this.RebuildLayout();
                }
            }
        }

        /// <summary>
        /// ID nhân vật
        /// </summary>
        public int RoleID { get; set; } = -1;
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng ChatBox
        /// </summary>
        private GameObject chatBox;

        /// <summary>
        /// Đối tượng StallBox
        /// </summary>
        private FitCollider2D stallBox;

        /// <summary>
        /// Màu ban đầu của thanh HP Bar
        /// </summary>
        private Color initHPBarColor;

        /// <summary>
        /// Màu ban đầu của thanh HP Bar
        /// </summary>
        private Color initNameColor;

        /// <summary>
        /// Thành phần Vertical Box
        /// </summary>
        private VerticalBox vBox;

        /// <summary>
        /// Luồng tự ẩn ChatBox
        /// </summary>
        private Coroutine autoHideChatBoxCoroutine;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.stallBox = this.UIText_StallName.transform.parent.GetComponent<FitCollider2D>();
            this.chatBox = this.UIText_ChatContent.transform.parent.gameObject;
            this.vBox = this.UIText_Name.transform.parent.GetComponent<VerticalBox>();
            this.initHPBarColor = this.UISlider_HPBar.Color;
            this.initNameColor = this.UIText_Name.color;
            this.InitPrefabs();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
        {
            /// Nếu đối tượng chưa được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Xây lại giao diện
            this.vBox.Rebuild();
        }

        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIText_Name.text = "";
            this.UIText_Title.text = "";
            this.UIText_GuildTitle.text = "";
            this.UIText_RoleTitle.text = "";
            this.UISlider_HPBar.Value = 0;
            this.UISlider_MPBar.Value = 0;
            this.UIImage_MantleTitle.gameObject.SetActive(false);
            this.UIImage_OfficeTitle.gameObject.SetActive(false);
            this.UIImage_SpecialTitle.gameObject.SetActive(false);
            this.chatBox.gameObject.SetActive(false);
            this.stallBox.gameObject.SetActive(false);

            /// Sự kiện Click vào sạp hàng
            this.stallBox.OnClick = () =>
            {
                this.ButtonOpenShop_Clicked();
            };
        }

        /// <summary>
        /// Luồng tự hủy ChatBox
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoHideChatBox()
        {
            /// Đợi 10s
            yield return new WaitForSeconds(10f);
            /// Ẩn ChatBox
            this.chatBox.gameObject.SetActive(false);
            /// Xây lại giao diện
            this.RebuildLayout();
            /// Hủy luồng
            this.autoHideChatBoxCoroutine = null;
        }
        #endregion

        #region Events
        /// <summary>
        /// Sự kiện khi Button mở cửa hàng được ấn
        /// </summary>
        private void ButtonOpenShop_Clicked()
        {
            /// Nếu là bản thân
            if (this.RoleID == Global.Data.RoleData.RoleID)
            {
                /// Bỏ qua
                return;
            }

            /// Đối tượng tương ứng
            GSprite sprite = KTGlobal.FindSpriteByID(this.RoleID);

            /// Không tìm thấy
            if (sprite == null)
            {
                /// Bỏ qua
                return;
            }

            /// Thực hiện Click vào Shop của người chơi
            Global.Data.GameScene.PlayerShopClick(sprite);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Destroy()
        {
            this.StopAllCoroutines();
            this.HPBarColor = this.initHPBarColor;
            this.NameColor = this.initNameColor;
            this.HPPercent = 0;
            this.Name = "";
            this.Title = "";
            this.IsShowMPBar = false;
            this.MPPercent = 0;
            this.RoleValue = 0;
            this.GuildTitle = "";
            this.SystemSettingShowName = true;
            this.SystemSettingShowHPBar = true;
            this.PKValue = 0;
            this.chatBox.gameObject.SetActive(false);
            this.autoHideChatBoxCoroutine = null;
            this.stallBox.gameObject.SetActive(false);
            this.stallBox.OnClick = null;
            this.RoleID = -1;
        }

        /// <summary>
        /// Chuyển màu tên, thanh máu về như ban đầu
        /// </summary>
        public void RestoreColor()
        {
            this.HPBarColor = this.initHPBarColor;
            this.NameColor = this.initNameColor;
        }

        /// <summary>
        /// Cập nhật trạng thái cưỡi
        /// </summary>
        /// <param name="isRiding"></param>
        public void UpdateRideState(bool isRiding)
        {
            if (isRiding)
            {
                this.transform.localPosition = this._RiderOffset;
            }
            else
            {
                this.transform.localPosition = this._Offset;
            }
        }
        #endregion
    }
}

