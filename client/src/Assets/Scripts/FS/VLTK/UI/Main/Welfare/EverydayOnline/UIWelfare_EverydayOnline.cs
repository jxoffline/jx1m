using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.Welfare.EverydayOnline;
using Server.Data;
using System.Collections;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main.Welfare
{
    /// <summary>
    /// Khung phúc lợi Online trong ngày
    /// </summary>
    public class UIWelfare_EverydayOnline : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ô vật phẩm thưởng
        /// </summary>
        [SerializeField]
        private UIWelfare_EverydayOnline_SlotItemBox UIItem_Prefab;

        /// <summary>
        /// Button nhận thưởng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Get;

        /// <summary>
        /// Text thông tin đã Online
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_OnlineDetails;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemsList = null;

        /// <summary>
        /// Nội bộ tự khóa Button nhận
        /// </summary>
        private bool innerLockButtonGet = false;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện nhận thưởng
        /// </summary>
        public Action<EveryDayOnLine> Get { get; set; }

        /// <summary>
        /// Sự kiện gửi yêu cầu lấy thông tin Online
        /// </summary>
        public Action QueryGetEverydayOnlineInfo { get; set; }

        /// <summary>
        /// Dữ liệu đăng nhập Online
        /// </summary>
        public EveryDayOnLineEvent Data { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemsList = this.UIItem_Prefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Gửi yêu cầu truy vấn phúc lợi Online nhận thưởng
            this.QueryGetEverydayOnlineInfo?.Invoke();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.StartCoroutine(this.ExecuteEverySec());
            if (this.Data == null)
            {

            }
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Get.onClick.AddListener(this.ButtonGet_Clicked);
            /// Ẩn button nhận
            this.UIButton_Get.interactable = false;
        }

        /// <summary>
        /// Sự kiện khi Button nhận được ấn
        /// </summary>
        private void ButtonGet_Clicked()
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Không có gì để nhận!");
                return;
            }
            /// Nếu không có gì để nhận
            else if (this.Data.CurrentAwardInfo == null)
            {
                KTGlobal.AddNotification("Không có gì để nhận!");
                return;
            }

            /// Thực hiện sự kiện quay nhận
            this.Get?.Invoke(this.Data.CurrentAwardInfo);
            /// Ẩn button nhận
            this.UIButton_Get.interactable = false;
            /// Thiết lập tự khóa Button nhận
            this.innerLockButtonGet = true;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực hiện mỗi giây
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteEverySec()
        {
            while (true)
            {
                /// Nếu có dữ liệu
                if (this.Data != null)
                {
                    /// Duyệt danh sách mốc và cập nhật thời gian tương ứng
                    foreach (Transform child in this.transformItemsList.transform)
                    {
                        if (child.gameObject != this.UIItem_Prefab.gameObject)
                        {
                            UIWelfare_EverydayOnline_SlotItemBox uiItemBox = child.GetComponent<UIWelfare_EverydayOnline_SlotItemBox>();
                            uiItemBox.CurrentOnlineSec = this.Data.LocalOnlineSec;
                            uiItemBox.Refresh();
                        }
                    }

                    /// Nếu đang mở khung phúc lợi
                    if (PlayZone.Instance.UIWelfare != null)
                    {
                        /// Hint có quà ở khung phúc lợi Online
                        PlayZone.Instance.UIWelfare.HintOnline(this.Data.HasThingToGet);
                    }

                    /// Cập nhật thông tin tổng thời gian đã đăng nhập
                    this.UIText_OnlineDetails.text = string.Format("Bạn đã Online tổng cộng <color=green>{0}</color>.{1}", KTGlobal.DisplayTimeHourMinuteSecondOnly(this.Data.LocalOnlineSec), this.Data.HasThingToGet ? " Nhấn <color=yellow>Nhận</color> để nhận phần thưởng mốc hiện tại." : "");

                    /// Nếu tồn tại quà có thể nhận thì làm sáng Button nhận
                    if (!this.innerLockButtonGet)
                    {
                        this.UIButton_Get.interactable = this.Data.HasThingToGet;
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

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
        private void ClearItems()
        {
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm phần thưởng mốc tương ứng
        /// </summary>
        /// <param name="onlineInfo"></param>
        private void AddAward(EveryDayOnLine onlineInfo)
        {
            /// Danh sách vật phẩm Random
            List<GoodsData> randomItems = new List<GoodsData>();

            /// Đã nhận chưa
            bool alreadyGotten = onlineInfo.StepID <= this.Data.EveryDayOnLineAwardStep;
            /// Nếu đã nhận rồi thì lấy ra thông tin vật phẩm đã nhận tương ứng
            if (alreadyGotten && this.Data.GetReceivedAwardItemInfo(onlineInfo.StepID, out int itemID, out int itemQuantity))
            {
                /// Nếu vật phẩm tồn tại
                if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
                {
                    randomItems.Clear();
                    /// Tạo mới vật phẩm
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    itemGD.Binding = 1;
                    itemGD.GCount = itemQuantity;
                    randomItems.Add(itemGD);
                }
            }
            else
            {
                /// Duyệt danh sách Random và thêm vào
                foreach (AwardItem awardItem in onlineInfo.RollAwardItem)
                {
                    /// Nếu vật phẩm tồn tại
                    if (Loader.Loader.Items.TryGetValue(awardItem.ItemID, out ItemData itemData))
                    {
                        /// Tạo mới vật phẩm
                        GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                        itemGD.Binding = 1;
                        itemGD.GCount = awardItem.Number;
                        randomItems.Add(itemGD);
                    }
                }
            }

            UIWelfare_EverydayOnline_SlotItemBox uiSlotItem = GameObject.Instantiate<UIWelfare_EverydayOnline_SlotItemBox>(this.UIItem_Prefab);
            uiSlotItem.transform.SetParent(this.transformItemsList, false);
            uiSlotItem.gameObject.SetActive(true);
            uiSlotItem.StepID = onlineInfo.StepID;
            uiSlotItem.TimeSec = onlineInfo.TimeSecs;
            uiSlotItem.CurrentOnlineSec = this.Data.LocalOnlineSec;
            uiSlotItem.AlreadyGotten = alreadyGotten;
            uiSlotItem.Items = randomItems;
            uiSlotItem.Refresh();
        }

        /// <summary>
        /// Tìm thông tin phần quà có StepID tương ứng
        /// </summary>
        /// <param name="stepID"></param>
        private UIWelfare_EverydayOnline_SlotItemBox FindAward(int stepID)
        {
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIWelfare_EverydayOnline_SlotItemBox uiSlotItem = child.GetComponent<UIWelfare_EverydayOnline_SlotItemBox>();
                    if (uiSlotItem.StepID == stepID)
                    {
                        return uiSlotItem;
                    }
                }
            }
            return null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        public void Refresh()
        {
            /// Làm rỗng danh sách vật phẩm
            this.ClearItems();
            /// Ẩn button nhận
            this.UIButton_Get.interactable = false;
            /// Làm rỗng Text trạng thái Online
            this.UIText_OnlineDetails.text = "";

            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Có lỗi khi tải dữ liệu phúc lợi Online, hãy báo hỗ trợ để được trợ giúp!");
                this.UIText_OnlineDetails.text = "Có lỗi khi tải dữ liệu phúc lợi Online, hãy báo hỗ trợ để được trợ giúp!";
                PlayZone.Instance.HideUIWelfare();
                return;
            }

            /// Nếu sự kiện chưa mở
            if (!this.Data.IsOpen)
            {
                KTGlobal.AddNotification("Phúc lợi này chưa được mở!");
                this.UIText_OnlineDetails.text = "Phúc lợi này chưa được mở!";
                return;
            }

            /// Cập nhật thông tin tổng thời gian đã đăng nhập
            this.UIText_OnlineDetails.text = string.Format("Bạn đã Online tổng cộng <color=green>{0}</color>.{1}", KTGlobal.DisplayTimeHourMinuteSecondOnly(this.Data.LocalOnlineSec), this.Data.HasThingToGet ? " Nhấn <color=yellow>Nhận</color> để nhận phần thưởng mốc hiện tại." : "");

            /// Xây danh sách quà
            foreach (EveryDayOnLine onlineInfo in this.Data.Item)
            {
                /// Tạo ô quà mốc tương ứng
                this.AddAward(onlineInfo);
            }

            /// Nếu đang mở khung phúc lợi
            if (PlayZone.Instance.UIWelfare != null)
            {
                /// Hint có quà ở khung phúc lợi Online
                PlayZone.Instance.UIWelfare.HintOnline(this.Data.HasThingToGet);
            }

            /// Thứ tự quà có thể nhận hiện tại
            int currentAwardID = this.Data.CurrentAward;
            /// Nếu tồn tại quà có thể nhận thì làm sáng Button nhận
            this.UIButton_Get.interactable = currentAwardID != -1;
            /// Bỏ đánh dấu khóa Button nhận
            this.innerLockButtonGet = false;

            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Cập nhật trạng thái
        /// </summary>
        /// <param name="onlineRecord"></param>
        /// <param name="stepID"></param>
        public void UpdateState(int onlineRecord, int stepID)
        {
            this.Data.DayOnlineSecond = onlineRecord;
            this.Data.EveryDayOnLineAwardStep = stepID;
            this.Data.ReceiveTick = KTGlobal.GetCurrentTimeMilis();

            /// Cập nhật trạng thái cho toàn bộ vật phẩm bên trong
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIWelfare_EverydayOnline_SlotItemBox uiSlotItem = child.GetComponent<UIWelfare_EverydayOnline_SlotItemBox>();
                    /// Đã nhận chưa
                    bool alreadyGotten = uiSlotItem.StepID <= this.Data.EveryDayOnLineAwardStep;
                    uiSlotItem.AlreadyGotten = alreadyGotten;
                    uiSlotItem.Refresh();
                }
            }

            /// Nếu đang mở khung phúc lợi
            if (PlayZone.Instance.UIWelfare != null)
            {
                /// Hint có quà ở khung phúc lợi Online
                PlayZone.Instance.UIWelfare.HintOnline(this.Data.HasThingToGet);
            }

            /// Cập nhật thông tin tổng thời gian đã đăng nhập
            this.UIText_OnlineDetails.text = string.Format("Bạn đã Online tổng cộng <color=green>{0}</color>.{1}", KTGlobal.DisplayTimeHourMinuteSecondOnly(this.Data.LocalOnlineSec), this.Data.HasThingToGet ? " Nhấn <color=yellow>Nhận</color> để nhận phần thưởng mốc hiện tại." : "");

            /// Nếu tồn tại quà có thể nhận thì làm sáng Button nhận
            this.UIButton_Get.interactable = this.Data.HasThingToGet;
            /// Bỏ đánh dấu khóa Button nhận
            this.innerLockButtonGet = false;
        }

        /// <summary>
        /// Thực hiện quay quà ở mốc tương ứng
        /// </summary>
        /// <param name="stepIndex"></param>
        /// <param name="itemID"></param>
        /// <param name="itemNumber"></param>
        public void StartRoll(int stepIndex, int itemID, int itemNumber)
        {
            /// Ô quà tương ứng
            UIWelfare_EverydayOnline_SlotItemBox uiItemBox = this.FindAward(stepIndex);
            /// Nếu không tồn tại
            if (uiItemBox == null)
            {
                return;
            }
            /// Thực hiện quay
            uiItemBox.Play(itemID, itemNumber);
        }
        #endregion
    }
}
