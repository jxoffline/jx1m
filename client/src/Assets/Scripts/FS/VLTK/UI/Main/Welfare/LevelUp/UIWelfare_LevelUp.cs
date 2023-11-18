using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.Welfare.LevelUp;
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
    /// Khung phúc lợi thăng cấp
    /// </summary>
    public class UIWelfare_LevelUp : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ô danh sách vật phẩm
        /// </summary>
        [SerializeField]
        private UIWelfare_LevelUp_ListAward UI_ItemListPrefab;

        /// <summary>
        /// Thông tin đã thăng cấp
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LevelUpInfo;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách các mốc quà
        /// </summary>
        private RectTransform transformItemList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Thông tin thăng cấp
        /// </summary>
        public LevelUpGiftConfig Data { get; set; }

        /// <summary>
        /// Truy vấn thông tin thăng cấp
        /// </summary>
        public Action QueryGetLevelUpInfo { get; set; }

        /// <summary>
        /// Sự kiện nhận vật phẩm ở mốc tương ứng
        /// </summary>
        public Action<LevelUpItem> Get { get; set; }
        #endregion

        #region Core Monobehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemList = this.UI_ItemListPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Gửi yêu cầu lấy thông tin đăng nhập
            this.QueryGetLevelUpInfo?.Invoke();
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
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách quà
        /// </summary>
        private void ClearItems()
        {
            foreach (Transform child in this.transformItemList.transform)
            {
                if (child.gameObject != this.UI_ItemListPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm phần quà tương ứng
        /// </summary>
        /// <param name="awardInfo"></param>
        private void AddAward(LevelUpItem awardInfo)
        {
            /// Tạo danh sách vật phẩm
            List<GoodsData> items = new List<GoodsData>();
            /// Duyệt danh sách vật phẩm thiết lập
            foreach (KeyValuePair<int, int> pair in awardInfo.Items)
            {
                /// Nếu vật phẩm tồn tại
                if (Loader.Loader.Items.TryGetValue(pair.Key, out ItemData itemData))
                {
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    itemGD.Binding = 1;
                    itemGD.GCount = pair.Value;
                    items.Add(itemGD);
                }
            }

            UIWelfare_LevelUp_ListAward uiAwardList = GameObject.Instantiate<UIWelfare_LevelUp_ListAward>(this.UI_ItemListPrefab);
            uiAwardList.transform.SetParent(this.transformItemList, false);
            uiAwardList.gameObject.SetActive(true);
            uiAwardList.ID = awardInfo.ID;
            uiAwardList.State = this.Data.GetState(awardInfo.ID);
            uiAwardList.Level = awardInfo.ToLevel;
            uiAwardList.Items = items;
            uiAwardList.Get = () => {
                /// Nếu không thể nhận
                if (uiAwardList.State != 1)
                {
                    return;
                }
                /// Thực thi sự kiện nhận
                this.Get?.Invoke(awardInfo);
            };
        }

        /// <summary>
        /// Tìm thông tin phần quà có id tương ứng
        /// </summary>
        /// <param name="id"></param>
        private UIWelfare_LevelUp_ListAward FindAward(int id)
        {
            foreach (Transform child in this.transformItemList.transform)
            {
                if (child.gameObject != this.UI_ItemListPrefab.gameObject)
                {
                    UIWelfare_LevelUp_ListAward uiSlotItem = child.GetComponent<UIWelfare_LevelUp_ListAward>();
                    if (uiSlotItem.ID == id)
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
            /// Làm rỗng Text trạng thái Online
            this.UIText_LevelUpInfo.text = "";

            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Có lỗi khi tải dữ liệu phúc lợi đăng nhập tích lũy, hãy báo hỗ trợ để được trợ giúp!");
                this.UIText_LevelUpInfo.text = "Có lỗi khi tải dữ liệu phúc lợi đăng nhập tích lũy, hãy báo hỗ trợ để được trợ giúp!";
                PlayZone.Instance.HideUIWelfare();
                return;
            }

            /// Cập nhật thông tin tổng thời gian đã đăng nhập
            this.UIText_LevelUpInfo.text = string.Format("Bạn đang đạt cấp <color=green>{0}</color>.{1}", Global.Data.RoleData.Level, this.Data.HasSomethingToGet ? "Nhấn <color=yellow>Nhận</color> tại mốc tương ứng để nhận quà." : "");

            /// Xây danh sách quà
            foreach (LevelUpItem itemInfo in this.Data.LevelUpItem)
            {
                /// Tạo ô quà mốc tương ứng
                this.AddAward(itemInfo);
            }

            /// Nếu đang mở khung phúc lợi
            if (PlayZone.Instance.UIWelfare != null)
            {
                /// Hint có quà ở khung phúc lợi Online
                PlayZone.Instance.UIWelfare.HintLevelUp(this.Data.HasSomethingToGet);
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Cập nhật trạng thái cho quà tại mốc tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        public void RefreshState(int id, int state)
        {
            UIWelfare_LevelUp_ListAward uiAwardInfo = this.FindAward(id);
            if (uiAwardInfo != null)
            {
                uiAwardInfo.State = state;
            }
        }
        #endregion
    }
}
