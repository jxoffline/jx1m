using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.MainUI.RadarMap;
using Server.Data;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Đối tượng bản đồ nhỏ ở góc
    /// </summary>
    public class UIRadarMap : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Texture bản đồ nhỏ chiếu bởi RadarMapCamera
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UIImage_MapTexture;

        /// <summary>
        /// Mask ẩn bản đồ nhỏ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_MaskInvisibleMinimap;

        /// <summary>
        /// Text tên bản đồ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MapName;

        /// <summary>
        /// Text tọa độ hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Position;

        /// <summary>
        /// Nút mở bản đồ khu vực
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ToLocalMap;

        /// <summary>
        /// Tọa độ khi hiện
        /// </summary>
        [SerializeField]
        private Vector2 VisiblePosition;

        /// <summary>
        /// Tọa độ khi ẩn
        /// </summary>
        [SerializeField]
        private Vector2 InvisiblePosition;

        /// <summary>
        /// Tốc độ thực hiện hiệu ứng
        /// </summary>
        [SerializeField]
        private float AnimationDuration = 2f;

        /// <summary>
        /// Text Debug tọa độ thực
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_DebugWorldPos;

        /// <summary>
        /// Khay vật phẩm dùng nhanh
        /// </summary>
        [SerializeField]
        private UIRadarMap_QuickItemsBox UIQuickItemsBox;
        #endregion

        #region Properties
        /// <summary>
        /// Tên bản đồ
        /// </summary>
        public string MapName
        {
            get
            {
                return this.UIText_MapName.text;
            }
            set
            {
                this.UIText_MapName.text = value;
            }
        }

        /// <summary>
        /// Sự kiện chuyển tới khung bản đồ khu vực
        /// </summary>
        public Action GoToLocalMap { get; set; }

        /// <summary>
        /// Sự kiện ấn dùng thuốc ở khay
        /// </summary>
        public Action<GoodsData> UseItem { get; set; }

        /// <summary>
        /// Sự kiện vật phẩm được chọn
        /// </summary>
        public Action<List<GoodsData>> ItemSelected { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Transform
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Chạy luồng cập nhật vị trí
            this.StartCoroutine(this.UpdateLeaderPositionContinuously());
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }

            /// Chạy luồng cập nhật vị trí
            this.StartCoroutine(this.UpdateLeaderPositionContinuously());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_ToLocalMap.onClick.AddListener(this.ButtonGoToLocalMap_Clicked);

            /// Nếu không phải GM
            if (Global.Data.RoleData.GMAuth != 1)
            {
                this.UIText_DebugWorldPos.gameObject.SetActive(false);
            }
            
            this.UIQuickItemsBox.UseItem = this.UseItem;
            this.UIQuickItemsBox.ItemSelected = this.ItemSelected;
        }

        /// <summary>
        /// Chuyển qua khung bản đồ khu vực
        /// </summary>
        private void ButtonGoToLocalMap_Clicked()
        {
            /// Nếu bản đồ hiện tại không cho phép mở bản đồ nhỏ
            if (!Loader.Loader.Maps[Global.Data.RoleData.MapCode].ShowMiniMap)
			{
                KTGlobal.AddNotification("Ở đây không được sử dụng bản đồ khu vực!");
                return;
			}
            this.GoToLocalMap?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng cập nhật vị trí của Leader liên tục
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateLeaderPositionContinuously()
        {
            /// Nghỉ 0.1s
            WaitForSeconds wait = new WaitForSeconds(0.1f);
            /// Lặp liên tục
            while (true)
            {
                /// Toác
                if (Global.Data.Leader == null || Global.CurrentMapData == null)
                {
                    /// Bỏ qua
                    yield return wait;
                    /// Tiếp tục
                    continue;
                }

                /// Vị trí hiện tại của Leader
                Vector2 leaderGridPos = KTGlobal.WorldPositionToGridPosition(Global.Data.Leader.PositionInVector2);
                /// Cập nhật Text
                this.UIText_Position.text = string.Format("{0},{1}", (int) leaderGridPos.x, (int) leaderGridPos.y);
                /// Nếu là GM
                if (Global.Data.RoleData.GMAuth == 1)
                {
                    this.UIText_DebugWorldPos.text = string.Format("{0},{1}", Global.Data.Leader.PosX, Global.Data.Leader.PosY);
                }

                /// Nghỉ
                yield return wait;
            }
        }
		#endregion

		#region Public methods
        /// <summary>
        /// Làm mới đối tượng
        /// </summary>
        public void Refresh()
		{
            this.UIImage_MaskInvisibleMinimap.gameObject.SetActive(!Loader.Loader.Maps[Global.Data.RoleData.MapCode].ShowMiniMap);
		}
		#endregion
	}
}