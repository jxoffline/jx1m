using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Welfare.Recharge
{
    /// <summary>
    /// Nội dung nạp trong khung phúc lợi nạp lần đầu
    /// </summary>
    public class UIWelfare_Recharge_FirstRecharge : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UI_ItemPrefab;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Button nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Get;

        /// <summary>
        /// Đánh dấu đã hiện
        /// </summary>
        [SerializeField]
        private RectTransform UIImage_MarkAlreadyGotten;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemsList = null;

        /// <summary>
        /// Trạng thái Button
        /// </summary>
        private int buttonState = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện lấy quà
        /// </summary>
        public Action<FistRechage> GetAwards { get; set; }

        /// <summary>
        /// Khung có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
        }

        private FistRechage _Data;
        /// <summary>
        /// Dữ liệu nạp lần đầu
        /// </summary>
        public FistRechage Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemsList = this.UI_ItemPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Get.onClick.AddListener(this.ButtonGetAwards_Clicked);
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện lấy phần quà
        /// </summary>
        private void ButtonGetAwards_Clicked()
        {
            /// Nếu đã nhận
            if (this.buttonState == 2)
            {
                KTGlobal.AddNotification("Bạn đã nhận quà thưởng nạp đầu rồi!");
                return;
            }
            /// Nếu không có dữ liệu
            else if (this._Data == null)
            {
                KTGlobal.AddNotification("Không có gì để nhận!");
                return;
            }

            /// Thực thi sự kiện nhận quà
            this.GetAwards?.Invoke(this._Data);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="workd"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách vật phẩm
        /// </summary>
        private void ClearItemsList()
        {
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm vật phẩm vào danh sách thưởng
        /// </summary>
        /// <param name="itemGD"></param>
        private void AddAwardItem(GoodsData itemGD)
        {
            UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_ItemPrefab);
            uiItemBox.transform.SetParent(this.transformItemsList, false);
            uiItemBox.gameObject.SetActive(true);
            uiItemBox.Data = itemGD;
        }

        /// <summary>
        /// Làm mới dữ liệu khung
        /// </summary>
        private void Refresh()
        {
            /// Xóa danh sách cũ
            this.ClearItemsList();

            /// Khóa Button nhận
            this.UIButton_Get.interactable = false;

            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                KTGlobal.AddNotification("Dữ liệu truyền về bị lỗi, hãy liên hệ với hỗ trợ để được trợ giúp.");
                return;
            }

            /// Thông tin truyền về
            string[] infoParams = this._Data.BtnState.Split(':');
            /// Nếu không thỏa mãn kích thước
            if (infoParams.Length != 2)
            {
                KTGlobal.AddNotification("Dữ liệu truyền về có lỗi, hãy liên hệ với hỗ trợ để được trợ giúp!");
                return;
            }

            try
            {
                /// Đã nhận chưa
                this.buttonState = int.Parse(infoParams[0]);
                if (this.buttonState == 2)
                {
                    this.UIImage_MarkAlreadyGotten.gameObject.SetActive(true);
                }
                else
                {
                    this.UIImage_MarkAlreadyGotten.gameObject.SetActive(false);
                }
            }
            catch (Exception)
            {
                KTGlobal.AddNotification("Dữ liệu truyền về có lỗi, hãy liên hệ với hỗ trợ để được trợ giúp!");
                return;
            }

            /// Cập nhật trạng thái Button nhận
            this.UIButton_Get.interactable = this.buttonState == 1;

            /// Duyệt danh sách vật phẩm
            foreach (TotalItem itemInfo in this._Data.TotalItem)
            {
                /// Nếu vật phẩm tồn tại
                if (Loader.Loader.Items.TryGetValue(itemInfo.ItemID, out ItemData itemData))
                {
                    /// Tạo vật phẩm tương ứng chiếu
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    /// Thiết lập khóa
                    itemGD.Binding = 1;
                    /// Thiết lập số lượng
                    itemGD.GCount = itemInfo.Number;
                    /// Tạo ô vật phẩm tương ứng
                    this.AddAwardItem(itemGD);
                }
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật trạng thái
        /// </summary>
        /// <param name="state"></param>
        public void UpdateState(int state)
        {
            this.buttonState = state;
            this.UIButton_Get.interactable = this.buttonState == 1;
            this.UIImage_MarkAlreadyGotten.gameObject.SetActive(this.buttonState == 2);
        }

        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
