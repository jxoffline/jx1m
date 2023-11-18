using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.UI.Main.Welfare.SeriesLogin;
using Server.Data;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Welfare
{
    /// <summary>
    /// Khung phúc lợi điểm danh
    /// </summary>
    public class UIWelfare_SeriesLogin : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIWelfare_SeriesLogin_ListAward_Item UI_ItemPrefab;

        /// <summary>
        /// Text ngày
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Day;

        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UI_Item;

        /// <summary>
        /// Đánh dấu đã nhận chưa
        /// </summary>
        [SerializeField]
        private RectTransform Mark_AlreadyGotten;

        /// <summary>
        /// Đánh dấu đã quá hạn
        /// </summary>
        [SerializeField]
        private RectTransform Mark_OutOfDate;

        /// <summary>
        /// Button nhận thưởng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GetAward;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật
        /// </summary>
        private RectTransform transformItemsList = null;

        /// <summary>
        /// Ngày được chọn
        /// </summary>
        private int selectedDay = -1;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện nhận thưởng
        /// </summary>
        public Action<int> GetAward { get; set; }

        /// <summary>
        /// Truy vấn thông tin điểm danh
        /// </summary>
        public Action QueryGetSeriesLoginAwards { get; set; }

        /// <summary>
        /// Sự kiện mua điểm danh
        /// </summary>
        public Action BuyNow { get; set; }

        /// <summary>
        /// Dữ liệu báo dánh
        /// </summary>
        public CheckPontConfig Data { get; set; }
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
            this.QueryGetSeriesLoginAwards?.Invoke();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_GetAward.onClick.AddListener(this.ButtonGetAward_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button nhận thưởng được ấn
        /// </summary>
        private void ButtonGetAward_Clicked()
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Không có dữ liệu điểm danh!");
                return;
            }
            /// Nếu không có ngày
            else if (this.selectedDay < 1 || this.selectedDay > 30)
            {
                KTGlobal.AddNotification("Hãy chọn một ngày!");
                return;
            }
            /// Nếu không thể nhận
            else if (this.Data.GetStateAtDay(this.selectedDay) != 0)
            {
                KTGlobal.AddNotification("Không có vật phẩm có thể nhận!");
                return;
            }

            /// Thực thi sự kiện
            this.GetAward?.Invoke(this.selectedDay);

            /// Khóa Button
            this.UIButton_GetAward.interactable = false;
        }

        /// <summary>
        /// Sự kiện khi Button vật phẩm được ấn
        /// </summary>
        /// <param name="data"></param>
        private void ButtonItem_Clicked(CheckPointItem data)
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Không có dữ liệu điểm danh!");
                return;
            }

            /// Chọn ngày tương ứng
            this.selectedDay = data.Day;
            this.UIText_Day.text = string.Format("Ngày {0}", data.Day);

            /// Thêm vật phẩm tương ứng vào
            this.UI_Item.Data = data.Item;
            /// Nếu có thể nhận
            if (this.Data.GetStateAtDay(data.Day) == 0)
            {
                this.UIButton_GetAward.gameObject.SetActive(true);
            }
            else
            {
                this.UIButton_GetAward.gameObject.SetActive(false);
            }

            /// Nếu đã nhận rồi
            if (this.Data.GetStateAtDay(data.Day) == 1)
            {
                this.Mark_AlreadyGotten.gameObject.SetActive(true);
            }
            /// Nếu chưa nhận
            else
            {
                this.Mark_AlreadyGotten.gameObject.SetActive(false);
            }

            /// Nếu quá hạn
            if (this.Data.GetStateAtDay(data.Day) == 2)
            {
                this.Mark_OutOfDate.gameObject.SetActive(true);
            }
            /// Nếu chưa nhận
            else
            {
                this.Mark_OutOfDate.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
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
        /// Xây lại giao diện tương ứng
        /// </summary>
        /// <param name="transform"></param>
        private void RebuildLayout(RectTransform transform)
        {
            /// Nếu đối tượng chưa được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            /// Xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }));
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildItemLayout()
        {
            this.RebuildLayout(this.transformItemsList);
        }

        /// <summary>
        /// Xóa danh sách vật phẩm
        /// </summary>
        private void ClearItemList()
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
        /// Thêm vật phẩm thưởng tương ứng
        /// </summary>
        /// <param name="data"></param>
        private void AddItem(CheckPointItem data)
        {
            UIWelfare_SeriesLogin_ListAward_Item uiItemBox = GameObject.Instantiate<UIWelfare_SeriesLogin_ListAward_Item>(this.UI_ItemPrefab);
            uiItemBox.transform.SetParent(this.transformItemsList, false);
            uiItemBox.gameObject.SetActive(true);
            uiItemBox.Data = data.Item;
            /// Trạng thái
            int state = this.Data.GetStateAtDay(data.Day);
            uiItemBox.AlreadyGotten = state == 1;
            uiItemBox.CanGet = state == 0;
            uiItemBox.OutOfDate = state == 2;
            uiItemBox.Click = () => {
                this.ButtonItem_Clicked(data);
            };
            uiItemBox.DateTimeText = string.Format("Ngày {0}", data.Day);
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void DoRefresh()
        {
            /// Xóa danh sách vật phẩm
            this.ClearItemList();

            /// Xóa vật phẩm đang chọn
            this.selectedDay = -1;
            this.UI_Item.Data = null;
            this.UIText_Day.text = "";

            /// Đóng Button chức năng
            this.UIButton_GetAward.gameObject.SetActive(false);

            /// Ẩn Hint điểm danh
            PlayZone.Instance.UIWelfare.HintSeriesLogin(false);

            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                return;
            }

            /// Duyệt danh sách
            foreach (CheckPointItem card in this.Data.CheckPointItem)
            {
                /// Thêm thẻ tương ứng
                this.AddItem(card);
            }

            /// Hint điểm danh
            PlayZone.Instance.UIWelfare.HintSeriesLogin(true);

            /// Xây lại giao diện
            this.RebuildItemLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        public void Refresh()
        {
            this.DoRefresh();
        }

        /// <summary>
        /// Làm mới dữ liệu ở ngày tương ứng
        /// </summary>
        /// <param name="state"></param>
        public void RefreshDataCurrentDay(int state)
        {
            int day = this.selectedDay;

            /// Nếu có trạng thái mới
            if (state != -1)
            {
                /// Thay đổi dữ liệu tương ứng
                this.Data.SetStateAtDay(day, state);
            }
            /// Lỗi gì đó
            else
            {
                /// Mở Button nhận
                this.UIButton_GetAward.interactable = this.Data.GetStateAtDay(day) == 0;
            }

            this.DoRefresh();
            this.selectedDay = day;
            /// Thực thi sự kiện chọn ngày tương ứng
            CheckPointItem item = this.Data.CheckPointItem.Where(x => x.Day == day).FirstOrDefault();
            if (item != null)
            {
                this.ButtonItem_Clicked(item);
            }
        }
        #endregion
    }
}
