using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ActivityList;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung danh sách hoạt động
    /// </summary>
    public class UIActivityList : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Thông tin ngày
        /// </summary>
        [Serializable]
        private class DayInfo
        {
            /// <summary>
            /// Ngày
            /// </summary>
            public int Day;

            /// <summary>
            /// Toggle
            /// </summary>
            public UIToggleSprite UIToggle;
        }

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thông tin hoạt động
        /// </summary>
        [SerializeField]
        private UIActivityList_ActivityInfo UI_ActivityInfoPrefab;

        /// <summary>
        /// Danh sách theo ngày
        /// </summary>
        [SerializeField]
        private DayInfo[] UI_DayOfWeek;

        /// <summary>
        /// Text tên hoạt động
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ActivityName;

        /// <summary>
        /// Text loại hoạt động
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ActivityType;

        /// <summary>
        /// Text yêu cầu cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RequireLevel;

        /// <summary>
        /// Text yêu cầu thêm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_OtherRequirations;

        /// <summary>
        /// Text thời gian diễn ra hoạt động
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ActivityTime;

        /// <summary>
        /// Text mô tả hoạt động
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ActivityDescription;

        /// <summary>
        /// Text phần thưởng thêm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_OtherAwards;

        /// <summary>
        /// Prefab ô vật phẩm thưởng
        /// </summary>
        [SerializeField]
        private UIItemBox UI_AwardItemPrefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách hoạt động
        /// </summary>
        private RectTransform transformActivityList;

        /// <summary>
        /// RectTransform danh sách vật phẩm thưởng
        /// </summary>
        private RectTransform transformAwardItemList;

        /// <summary>
        /// RectTransform mô tả sự kiện
        /// </summary>
        private RectTransform transformDescriptionText;

        /// <summary>
        /// RectTransform thời gian sự kiện
        /// </summary>
        private RectTransform transformTimeText;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Core MonoBahaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Awake()
        {
            this.transformActivityList = this.UI_ActivityInfoPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformAwardItemList = this.UI_AwardItemPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformTimeText = this.UIText_ActivityTime.transform.parent.GetComponent<RectTransform>();
            this.transformDescriptionText = this.UIText_ActivityDescription.transform.parent.GetComponent<RectTransform>();
        }

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
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            /// Duyệt danh sách Toggle ngày trong tuần
            foreach (DayInfo _dayInfo in this.UI_DayOfWeek)
            {
                /// Thiết lập sự kiện
                _dayInfo.UIToggle.OnSelected = (isSelected) => {
                    if (isSelected)
                    {
                        this.ToggleDay_Selected(_dayInfo.Day);
                    }
                };
            }

            /// Hủy toàn bộ thông tin
            this.UIText_ActivityName.text = "";
            this.UIText_ActivityType.text = "";
            this.UIText_RequireLevel.text = "";
            this.UIText_OtherRequirations.text = "";
            this.UIText_ActivityTime.text = "";
            this.UIText_ActivityDescription.text = "";
            this.UIText_OtherAwards.text = "";

            /// Làm rỗng danh sách vật phẩm thưởng
            foreach (Transform child in this.transformAwardItemList.transform)
            {
                if (child.gameObject != this.UI_AwardItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Xem ngày hôm nay là thứ mấy
            DayOfWeek todayDay = new DateTime(KTGlobal.GetServerTime() * TimeSpan.TicksPerMillisecond).DayOfWeek;
            int todayDayInt = -1;
            switch (todayDay)
            {
                case DayOfWeek.Monday:
                {
                    todayDayInt = 2;
                    break;
                }
                case DayOfWeek.Tuesday:
                {
                    todayDayInt = 3;
                    break;
                }
                case DayOfWeek.Wednesday:
                {
                    todayDayInt = 4;
                    break;
                }
                case DayOfWeek.Thursday:
                {
                    todayDayInt = 5;
                    break;
                }
                case DayOfWeek.Friday:
                {
                    todayDayInt = 6;
                    break;
                }
                case DayOfWeek.Saturday:
                {
                    todayDayInt = 7;
                    break;
                }
                case DayOfWeek.Sunday:
                {
                    todayDayInt = 8;
                    break;
                }
            }

            /// Thông tin ngày tương ứng
            DayInfo dayInfo = this.UI_DayOfWeek.Where(x => x.Day == todayDayInt).FirstOrDefault();
            /// Nếu tìm thấy
            if (dayInfo != null)
            {
                /// Đánh dấu Toggle ngày hôm nay
                dayInfo.UIToggle.Active = true;
            }
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Toggle ngày tương ứng được chọn
        /// </summary>
        /// <param name="day"></param>
        private void ToggleDay_Selected(int day)
        {
            /// Hủy toàn bộ thông tin
            this.UIText_ActivityName.text = "";
            this.UIText_ActivityType.text = "";
            this.UIText_RequireLevel.text = "";
            this.UIText_OtherRequirations.text = "";
            this.UIText_ActivityTime.text = "";
            this.UIText_ActivityDescription.text = "";
            this.UIText_OtherAwards.text = "";

            /// Làm rỗng danh sách vật phẩm thưởng
            foreach (Transform child in this.transformAwardItemList.transform)
            {
                if (child.gameObject != this.UI_AwardItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Làm rỗng danh sách sự kiện trong ngày
            foreach (Transform child in this.transformActivityList.transform)
            {
                if (child.gameObject != this.UI_ActivityInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// UI sự kiện đầu tiên
            UIActivityList_ActivityInfo uiFirstActivity = null;

            /// Duyệt danh sách sự kiện trong ngày
            foreach (ActivityXML activity in Loader.Loader.Activities.Values)
            {
                /// Nếu không phải ngày thì thôi
                if (activity.DayOfWeek.Contains(day))
                {
                    /// Thêm vào danh sách
                    UIActivityList_ActivityInfo uiActivityInfo = GameObject.Instantiate<UIActivityList_ActivityInfo>(this.UI_ActivityInfoPrefab);
                    uiActivityInfo.transform.SetParent(this.transformActivityList, false);
                    uiActivityInfo.gameObject.SetActive(true);
                    uiActivityInfo.Data = activity;
                    uiActivityInfo.Select = () =>
                    {
                        /// Làm rỗng danh sách vật phẩm thưởng
                        foreach (Transform child in this.transformAwardItemList.transform)
                        {
                            if (child.gameObject != this.UI_AwardItemPrefab.gameObject)
                            {
                                GameObject.Destroy(child.gameObject);
                            }
                        }

                        /// Đổ dữ liệu
                        this.UIText_ActivityName.text = activity.ActivityName;
                        this.UIText_ActivityType.text = activity.TypeDesc;
                        this.UIText_RequireLevel.text = activity.LevelJoin.ToString();
                        this.UIText_OtherRequirations.text = activity.OtherRequiration;
                        this.UIText_ActivityTime.text = activity.GetActivityTimes();
                        this.UIText_ActivityDescription.text = activity.Description;
                        this.UIText_OtherAwards.text = activity.OtherRewards;

                        /// Duyệt danh sách vật phẩm thưởng
                        foreach (int itemID in activity.ItemReward)
                        {
                            /// Thông tin vật phẩm tương ứng
                            if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
                            {
                                /// Tạo vật phẩm ảo
                                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                                /// Tạo mới ô vật phẩm
                                UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_AwardItemPrefab);
                                uiItemBox.transform.SetParent(this.transformAwardItemList, false);
                                uiItemBox.gameObject.SetActive(true);
                                uiItemBox.Data = itemGD;
                            }
                        }

                        /// Xây lại giao diện
                        this.ExecuteSkipFrames(1, () =>
                        {
                            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformAwardItemList);
                            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformTimeText);
                            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformDescriptionText);
                        });
                    };
                    /// Nếu chưa có UI sự kiện đầu tiên
                    if (uiFirstActivity == null)
                    {
                        /// Đánh dấu
                        uiFirstActivity = uiActivityInfo;
                        /// Chọn sự kiện đầu tiên
                        uiFirstActivity.Active = true;
                    }
                }
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformActivityList);
            });
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DoExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private void ExecuteSkipFrames(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }
        #endregion
    }
}
