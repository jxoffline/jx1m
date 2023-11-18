using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.Welfare.SevenDayLogin;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Welfare
{
    /// <summary>
    /// Khung quà đăng nhập 7 ngày
    /// </summary>
    public class UIWelfare_SevenDayLogin : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ô vật phẩm thưởng
        /// </summary>
        [SerializeField]
        private UIWelfare_SevenDayLogin_SlotItemBox UIItem_Prefab;

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

        /// <summary>
        /// Prefab ô vật phẩm thưởng liên tiếp
        /// </summary>
        [SerializeField]
        private UIWelfare_SevenDayLogin_SlotItemBox UIItem_PrefabContinuous;

        /// <summary>
        /// Button nhận thưởng liên tiếp
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GetContinuous;

        /// <summary>
        /// Text thông tin đã Online liên tiếp
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_OnlineDetailsContinuous;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemsList = null;

        /// <summary>
        /// RectTransform danh sách vật phẩm liên tiếp
        /// </summary>
        private RectTransform transformItemsListContinuous = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện nhận thưởng
        /// </summary>
        public Action<SevenDaysLoginItem> Get { get; set; }

        /// <summary>
        /// Sự kiện nhận thưởng
        /// </summary>
        public Action<SevenDaysLoginItem> GetContinuous { get; set; }

        /// <summary>
        /// Sự kiện gửi yêu cầu lấy thông tin đăng nhập 7 ngày
        /// </summary>
        public Action QueryGetSevenDayLoginInfo { get; set; }

        /// <summary>
        /// Dữ liệu đăng nhập 7 ngày
        /// </summary>
        public SevenDayEvent Data { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemsList = this.UIItem_Prefab.transform.parent.GetComponent<RectTransform>();
            this.transformItemsListContinuous = this.UIItem_PrefabContinuous.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Gửi yêu cầu truy vấn phúc lợi đăng nhập 7 ngày
            this.QueryGetSevenDayLoginInfo?.Invoke();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Get.onClick.AddListener(this.ButtonGet_Clicked);
            this.UIButton_GetContinuous.onClick.AddListener(this.ButtonGetContinuous_Clicked);
            /// Ẩn button nhận
            this.UIButton_Get.interactable = false;
            this.UIButton_GetContinuous.interactable = false;
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
            else if (this.Data.SevenDaysLogin.CurrentAwardInfo == null)
            {
                KTGlobal.AddNotification("Không có gì để nhận!");
                return;
            }

            /// Thực hiện sự kiện quay nhận
            this.Get?.Invoke(this.Data.SevenDaysLogin.CurrentAwardInfo);
            /// Ẩn button nhận
            this.UIButton_Get.interactable = false;
            this.UIButton_GetContinuous.interactable = false;
        }

        /// Sự kiện khi Button nhận được ấn
        /// </summary>
        private void ButtonGetContinuous_Clicked()
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Không có gì để nhận!");
                return;
            }
            /// Nếu không có gì để nhận
            else if (this.Data.SevenDaysLoginContinus.CurrentAwardInfo == null)
            {
                KTGlobal.AddNotification("Không có gì để nhận!");
                return;
            }

            /// Thực hiện sự kiện quay nhận
            this.GetContinuous?.Invoke(this.Data.SevenDaysLoginContinus.CurrentAwardInfo);
            /// Ẩn button nhận
            this.UIButton_Get.interactable = false;
            this.UIButton_GetContinuous.interactable = false;
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
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsListContinuous);
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
            foreach (Transform child in this.transformItemsListContinuous.transform)
            {
                if (child.gameObject != this.UIItem_PrefabContinuous.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm phần thưởng mốc tương ứng
        /// </summary>
        /// <param name="awardInfo"></param>
        private void AddAward(SevenDaysLoginItem awardInfo)
        {
            /// Danh sách vật phẩm Random
            List<GoodsData> randomItems = new List<GoodsData>();

            /// Đã nhận chưa
            bool alreadyGotten = this.Data.SevenDaysLogin.HasAlreadyGotten(awardInfo.Days);
            /// Nếu đã nhận rồi thì lấy ra thông tin vật phẩm đã nhận tương ứng
            if (alreadyGotten && this.Data.SevenDaysLogin.GetReceivedAwardItemInfo(awardInfo.Days, out int itemID, out int itemQuantity))
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
                foreach (RollAwardItem awardItem in awardInfo.RollAwardItem)
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

            UIWelfare_SevenDayLogin_SlotItemBox uiSlotItem = GameObject.Instantiate<UIWelfare_SevenDayLogin_SlotItemBox>(this.UIItem_Prefab);
            uiSlotItem.transform.SetParent(this.transformItemsList, false);
            uiSlotItem.gameObject.SetActive(true);
            uiSlotItem.CanGet = this.Data.SevenDaysLogin.CanGet(awardInfo.Days);
            uiSlotItem.AlreadyGotten = alreadyGotten;
            uiSlotItem.OutOfDate = this.Data.SevenDaysLogin.IsOutOfDate(awardInfo.Days);
            uiSlotItem.Items = randomItems;
            uiSlotItem.Day = awardInfo.Days;
            uiSlotItem.Refresh();
        }

        /// <summary>
        /// Tìm thông tin phần quà có day tương ứng
        /// </summary>
        /// <param name="day"></param>
        private UIWelfare_SevenDayLogin_SlotItemBox FindAward(int day)
        {
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIWelfare_SevenDayLogin_SlotItemBox uiSlotItem = child.GetComponent<UIWelfare_SevenDayLogin_SlotItemBox>();
                    if (uiSlotItem.Day == day)
                    {
                        return uiSlotItem;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Thêm phần thưởng mốc tương ứng
        /// </summary>
        /// <param name="awardInfo"></param>
        private void AddAwardContinuous(SevenDaysLoginItem awardInfo)
        {
            /// Danh sách vật phẩm Random
            List<GoodsData> randomItems = new List<GoodsData>();

            /// Đã nhận chưa
            bool alreadyGotten = this.Data.SevenDaysLoginContinus.HasAlreadyGotten(awardInfo.Days);
            /// Nếu đã nhận rồi thì lấy ra thông tin vật phẩm đã nhận tương ứng
            if (alreadyGotten && this.Data.SevenDaysLoginContinus.GetReceivedAwardItemInfo(awardInfo.Days, out int itemID, out int itemQuantity))
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
                foreach (RollAwardItem awardItem in awardInfo.RollAwardItem)
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

            UIWelfare_SevenDayLogin_SlotItemBox uiSlotItem = GameObject.Instantiate<UIWelfare_SevenDayLogin_SlotItemBox>(this.UIItem_Prefab);
            uiSlotItem.transform.SetParent(this.transformItemsListContinuous, false);
            uiSlotItem.gameObject.SetActive(true);
            uiSlotItem.CanGet = this.Data.SevenDaysLoginContinus.CanGet(awardInfo.Days);
            uiSlotItem.AlreadyGotten = alreadyGotten;
            uiSlotItem.OutOfDate = false;
            uiSlotItem.Items = randomItems;
            uiSlotItem.Day = awardInfo.Days;
            uiSlotItem.Refresh();
        }

        /// <summary>
        /// Tìm thông tin phần quà có day tương ứng
        /// </summary>
        /// <param name="day"></param>
        private UIWelfare_SevenDayLogin_SlotItemBox FindAwardContinuous(int day)
        {
            foreach (Transform child in this.transformItemsListContinuous.transform)
            {
                if (child.gameObject != this.UIItem_PrefabContinuous.gameObject)
                {
                    UIWelfare_SevenDayLogin_SlotItemBox uiSlotItem = child.GetComponent<UIWelfare_SevenDayLogin_SlotItemBox>();
                    if (uiSlotItem.Day == day)
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
            this.UIButton_GetContinuous.interactable = false;
            /// Làm rỗng Text trạng thái Online
            this.UIText_OnlineDetails.text = "";
            this.UIText_OnlineDetailsContinuous.text = "";

            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Có lỗi khi tải dữ liệu phúc lợi đăng nhập 7 ngày, hãy báo hỗ trợ để được trợ giúp!");
                PlayZone.Instance.HideUIWelfare();
                return;
            }

            ///// Nếu sự kiện chưa mở
            //if (!this.Data.IsOpen)
            //{
            //    KTGlobal.AddNotification("Phúc lợi này chưa được mở!");
            //    this.UIText_OnlineDetails.text = "Phúc lợi này chưa được mở!";
            //    return;
            //}

            /// Cập nhật thông tin tổng thời gian đã đăng nhập
            this.UIText_OnlineDetails.text = string.Format("Bạn đã đăng nhập <color=green>{0} ngày</color>. Nếu bỏ lỡ ngày nào sẽ không nhận được quà ngày đó.", this.Data.SevenDaysLogin.DayID);
            this.UIText_OnlineDetailsContinuous.text = string.Format("Bạn đã đăng nhập liên tiếp <color=green>{0} ngày</color>.", this.Data.SevenDaysLoginContinus.TotalDayLoginContinus);

            /// Xây danh sách quà
            foreach (SevenDaysLoginItem itemInfo in this.Data.SevenDaysLogin.SevenDaysLoginItem)
            {
                /// Tạo ô quà mốc tương ứng
                this.AddAward(itemInfo);
            }
            foreach (SevenDaysLoginItem itemInfo in this.Data.SevenDaysLoginContinus.SevenDaysLoginItem)
            {
                /// Tạo ô quà mốc tương ứng
                this.AddAwardContinuous(itemInfo);
            }

            /// Nếu đang mở khung phúc lợi
            if (PlayZone.Instance.UIWelfare != null)
            {
                /// Hint có quà ở khung phúc lợi Online
                PlayZone.Instance.UIWelfare.HintLogin(this.Data.SevenDaysLogin.HasSomethingToGet || this.Data.SevenDaysLoginContinus.HasSomethingToGet);
            }

            /// Nếu tồn tại quà có thể nhận thì làm sáng Button nhận
            this.UIButton_Get.interactable = this.Data.SevenDaysLogin.HasSomethingToGet;
            this.UIButton_GetContinuous.interactable = this.Data.SevenDaysLoginContinus.HasSomethingToGet;

            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Cập nhật trạng thái
        /// </summary>
        /// <param name="day"></param>
        /// <param name="itemID"></param>
        /// <param name="itemCount"></param>
        public void UpdateState(int day, int itemID, int itemCount)
        {
            this.Data.SevenDaysLogin.DayID = day;
            /// Nếu chưa tồn tại
            if (this.Data.SevenDaysLogin.RevicedHistory == null)
            {
                this.Data.SevenDaysLogin.RevicedHistory = new List<SevenDayLoginHistoryItem>();
            }
            /// Cập nhật lịch sử
            this.Data.SevenDaysLogin.RevicedHistory.Add(new SevenDayLoginHistoryItem()
            {
                DayID = day,
                GoodIDs = itemID,
                GoodNum = itemCount,
            });

            /// Cập nhật trạng thái cho toàn bộ vật phẩm bên trong
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIWelfare_SevenDayLogin_SlotItemBox uiSlotItem = child.GetComponent<UIWelfare_SevenDayLogin_SlotItemBox>();
                    /// Đã nhận chưa
                    bool alreadyGotten = this.Data.SevenDaysLogin.HasAlreadyGotten(uiSlotItem.Day);
                    uiSlotItem.CanGet = this.Data.SevenDaysLogin.CanGet(uiSlotItem.Day);
                    uiSlotItem.AlreadyGotten = alreadyGotten;
                    uiSlotItem.OutOfDate = this.Data.SevenDaysLogin.IsOutOfDate(uiSlotItem.Day);
                    uiSlotItem.Refresh();
                }
            }

            /// Cập nhật thông tin tổng thời gian đã đăng nhập
            this.UIText_OnlineDetails.text = string.Format("Bạn đã đăng nhập liên tiếp <color=green>{0} ngày</color>. Nếu bỏ lỡ ngày nào sẽ không nhận được quà ngày đó.", day);

            /// Nếu đang mở khung phúc lợi
            if (PlayZone.Instance.UIWelfare != null)
            {
                /// Hint có quà ở khung phúc lợi Online
                PlayZone.Instance.UIWelfare.HintLogin(this.Data.SevenDaysLoginContinus.HasSomethingToGet || this.Data.SevenDaysLogin.HasSomethingToGet);
            }

            /// Nếu tồn tại quà có thể nhận thì làm sáng Button nhận
            this.UIButton_Get.interactable = this.Data.SevenDaysLogin.HasSomethingToGet;
            this.UIButton_GetContinuous.interactable = this.Data.SevenDaysLoginContinus.HasSomethingToGet;
        }

        /// <summary>
        /// Cập nhật trạng thái
        /// </summary>
        /// <param name="dayID"></param>
        /// <param name="itemID"></param>
        /// <param name="itemCount"></param>
        public void UpdateStateContinuous(int dayID, int itemID, int itemCount)
        {
            this.Data.SevenDaysLoginContinus.Step = dayID;
            /// Nếu chưa tồn tại
            if (this.Data.SevenDaysLoginContinus.SevenDayLoginAward == null)
            {
                this.Data.SevenDaysLoginContinus.SevenDayLoginAward = "";
            }
            /// Cập nhật lịch sử
            this.Data.SevenDaysLoginContinus.SevenDayLoginAward += string.Format("|{0}_{1}", itemID, itemCount);

            /// Cập nhật trạng thái cho toàn bộ vật phẩm bên trong
            foreach (Transform child in this.transformItemsListContinuous.transform)
            {
                if (child.gameObject != this.UIItem_PrefabContinuous.gameObject)
                {
                    UIWelfare_SevenDayLogin_SlotItemBox uiSlotItem = child.GetComponent<UIWelfare_SevenDayLogin_SlotItemBox>();
                    /// Đã nhận chưa
                    bool alreadyGotten = this.Data.SevenDaysLoginContinus.HasAlreadyGotten(uiSlotItem.Day);
                    uiSlotItem.CanGet = this.Data.SevenDaysLoginContinus.CanGet(uiSlotItem.Day);
                    uiSlotItem.AlreadyGotten = alreadyGotten;
                    uiSlotItem.OutOfDate = false;
                    uiSlotItem.Refresh();
                }
            }

            /// Cập nhật thông tin tổng thời gian đã đăng nhập
            this.UIText_OnlineDetailsContinuous.text = string.Format("Bạn đã đăng nhập liên tiếp <color=green>{0} ngày</color>.", dayID);

            /// Nếu đang mở khung phúc lợi
            if (PlayZone.Instance.UIWelfare != null)
            {
                /// Hint có quà ở khung phúc lợi Online
                PlayZone.Instance.UIWelfare.HintLogin(this.Data.SevenDaysLoginContinus.HasSomethingToGet || this.Data.SevenDaysLogin.HasSomethingToGet);
            }

            /// Nếu tồn tại quà có thể nhận thì làm sáng Button nhận
            this.UIButton_Get.interactable = this.Data.SevenDaysLogin.HasSomethingToGet;
            this.UIButton_GetContinuous.interactable = this.Data.SevenDaysLoginContinus.HasSomethingToGet;
        }

        /// <summary>
        /// Thực hiện quay quà ở mốc tương ứng
        /// </summary>
        /// <param name="dayID"></param>
        /// <param name="itemID"></param>
        /// <param name="itemNumber"></param>
        public void StartRoll(int dayID, int itemID, int itemNumber)
        {
            /// Ô quà tương ứng
            UIWelfare_SevenDayLogin_SlotItemBox uiItemBox = this.FindAward(dayID);
            /// Nếu không tồn tại
            if (uiItemBox == null)
            {
                return;
            }
            /// Thực hiện quay
            uiItemBox.Play(itemID, itemNumber);
        }

        /// <summary>
        /// Thực hiện quay quà ở mốc tương ứng
        /// </summary>
        /// <param name="dayID"></param>
        /// <param name="itemID"></param>
        /// <param name="itemNumber"></param>
        public void StartRollContinuous(int dayID, int itemID, int itemNumber)
        {
            /// Ô quà tương ứng
            UIWelfare_SevenDayLogin_SlotItemBox uiItemBox = this.FindAwardContinuous(dayID);
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
