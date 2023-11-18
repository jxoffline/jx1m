using System;
using System.Collections.Generic;
using Server.Data;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.LuckyCircle;
using FS.VLTK.Entities.Config;
using System.Collections;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung Vòng quay may mắn
    /// </summary>
    public class UILuckyCircle : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        [SerializeField]
        private UILuckyCircle_Cell[] UIItems;

        /// <summary>
        /// Text thông tin phần thưởng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AwardInfo;

        /// <summary>
        /// Button quay
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_StartTurn;

        /// <summary>
        /// Button nhận quà
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GetAward;

        /// <summary>
        /// Khung lịch sử
        /// </summary>
        [SerializeField]
        private UILuckyCircle_HistoryBox UIHistoryBox;

        /// <summary>
        /// Toggle sử dụng KNB
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_UseToken;

        /// <summary>
        /// Toggle sử dụng KNB khóa
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_UseBoundToken;

        /// <summary>
        /// Toggle sử dụng vật phẩm
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_UseItem;
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực hiện quay
        /// </summary>
        private Coroutine startTurnCoroutine = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện quay
        /// </summary>
        public Action<int> StartTurn { get; set; }

        /// <summary>
        /// Sự kiện nhận thưởng
        /// </summary>
        public Action GetAward { get; set; }

        /// <summary>
        /// Dữ liệu vòng quay
        /// </summary>
        public G2C_LuckyCircle Data { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.RefreshData();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_GetAward.onClick.AddListener(this.ButtonGetAward_Clicked);
            this.UIButton_StartTurn.onClick.AddListener(this.ButtonStartTurn_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button nhận thưởng được ấn
        /// </summary>
        private void ButtonGetAward_Clicked()
        {
            /// Toác
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Thông tin vòng quay bị lỗi!");
                return;
            }
            /// Nếu vẫn còn vật phẩm chưa nhận
            else if (this.Data.LastStopPos < 0 || this.Data.LastStopPos >= this.Data.Items.Count)
            {
                KTGlobal.AddNotification("Không có vật phẩm nào chưa nhận!");
                return;
            }

            /// Hủy Button nhận
            this.UIButton_GetAward.interactable = false;
            /// Thực thi sự kiện nhận
            this.GetAward?.Invoke();
            /// Hủy Toggle
            this.UIToggle_UseToken.Enable = false;
            this.UIToggle_UseBoundToken.Enable = false;
            this.UIToggle_UseItem.Enable = false;
        }

        /// <summary>
        /// Sự kiện khi Button quay được ấn
        /// </summary>
        private void ButtonStartTurn_Clicked()
        {
            /// Toác
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Thông tin vòng quay bị lỗi!");
                return;
            }
            /// Nếu vẫn còn vật phẩm chưa nhận
            else if (this.Data.LastStopPos >= 0 && this.Data.LastStopPos < this.Data.Items.Count)
            {
                KTGlobal.AddNotification("Hãy nhận vật phẩm trước rồi mới tiến hành quay tiếp!");
                return;
            }

            /// Hủy Button quay
            this.UIButton_StartTurn.interactable = false;
            /// Thực thi sự kiện quay
            this.StartTurn?.Invoke(this.UIToggle_UseToken.Active ? 1 : this.UIToggle_UseBoundToken.Active ? 2 : 3);
            /// Hủy Toggle
            this.UIToggle_UseToken.Enable = false;
            this.UIToggle_UseBoundToken.Enable = false;
            this.UIToggle_UseItem.Enable = false;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Hủy kích hoạt toàn bộ vật phẩm
        /// </summary>
        private void UnSelectAllItems()
        {
            foreach (UILuckyCircle_Cell uiCell in this.UIItems)
            {
                uiCell.Selected = false;
            }
        }

        /// <summary>
        /// Thực hiện quay
        /// </summary>
        /// <param name="stopPos"></param>
        /// <param name="durationSec"></param>
        /// <returns></returns>
        private IEnumerator DoStartTurn(int stopPos, int durationSec)
        {
            /// Ẩn Button quay
            this.UIButton_StartTurn.interactable = false;
            this.UIButton_GetAward.interactable = false;

            /// Thiết lập Text
            this.UIText_AwardInfo.text = "Mỗi lượt quay tiêu hao <color=yellow>1000 KNB (khóa)</color> hoặc <color=yellow>[Thẻ vòng quay]</color><br>Ấn <color=green>Rút thăm</color> để bắt đầu quay.";

            /// Số vòng ngẫu nhiên
            int round = UnityEngine.Random.Range(3, 6);
            /// Tổng số ô
            int totalCells = this.UIItems.Length * round + stopPos;
            /// Thời gian mỗi ô
            float durationEachCell = (float) durationSec / totalCells;
            /// Thời gian đã quay
            float lifeTime = 0f;
            /// Vị trí ô hiện tại
            int currentCellPos = 0;
            /// Tổng số ô đã quay
            int totalPassedCells = 0;
            /// Đối tượng đợi
            WaitForSeconds wait = new WaitForSeconds(durationEachCell);
            /// Lặp liên tục
            while (true)
            {
                /// Tăng tổng số ô đã quay
                totalPassedCells++;
                /// Thực thi hiệu ứng tại ô tương ứng
                this.UIItems[currentCellPos].PlayAnimation();
                /// Tăng thứ tự ô lên
                currentCellPos++;
                /// Nếu vượt quá
                if (currentCellPos >= this.UIItems.Length)
                {
                    currentCellPos -= this.UIItems.Length;
                }
                /// Đợi
                yield return wait;
                /// Tăng thời gian đã quay lên
                lifeTime += durationEachCell;
                /// Nếu đã quá số ô đã qua
                if (totalPassedCells >= totalCells)
                {
                    break;
                }
            }

            /// Hiện Button nhận thưởng
            this.UIButton_GetAward.interactable = true;
            this.UIButton_StartTurn.interactable = false;

            /// Chọn ô đích
            this.UIItems[this.Data.LastStopPos].Selected = true;

            /// Nếu có phần thưởng chưa nhận
            if (this.Data.LastStopPos >= 0 && this.Data.LastStopPos < this.Data.Items.Count)
            {
                /// Thông tin vật phẩm tương ứng
                G2C_LuckyCircle_ItemInfo itemInfo = this.Data.Items[this.Data.LastStopPos];
                /// Nếu tồn tại
                if (Loader.Loader.Items.TryGetValue(itemInfo.ItemID, out ItemData itemData))
                {
                    /// Vật phẩm tương ứng
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    /// Thiết lập Text
                    this.UIText_AwardInfo.text = string.Format("Nhận được <color=#1afff4>{0} cái</color> <color=#{1}>[{2}]</color>", itemInfo.Quantity, ColorUtility.ToHtmlStringRGB(KTGlobal.GetItemColor(itemGD)), KTGlobal.GetItemName(itemGD));

                    /// Hiện Button nhận
                    this.UIButton_StartTurn.interactable = false;
                    this.UIButton_GetAward.interactable = true;
                }
            }
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Toác
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Thông tin vòng quay bị lỗi!");
                return;
            }
            /// Lỗi số lượng vật phẩm không khớp
            else if (this.Data.Items == null || this.Data.Items.Count != this.UIItems.Length)
            {
                KTGlobal.AddNotification("Thông tin vòng quay bị lỗi!");
                return;
            }

            /// Hủy Button nhận
            this.UIButton_StartTurn.interactable = true;
            this.UIButton_GetAward.interactable = false;

            /// Duyệt danh sách ô quà
            for (int i = 0; i < this.Data.Items.Count; i++)
            {
                /// Gắn dữ liệu vào ô tương ứng
                UILuckyCircle_Cell uiCell = this.UIItems[i];
                uiCell.ItemID = this.Data.Items[i].ItemID;
                uiCell.Quantity = this.Data.Items[i].Quantity;
                uiCell.EffectType = this.Data.Items[i].EffectType;
            }

            /// Thiết lập Text
            this.UIText_AwardInfo.text = "Mỗi lượt quay tiêu hao <color=yellow>1000 KNB (khóa)</color> hoặc <color=yellow>[Thẻ vòng quay]</color><br>Ấn <color=green>Rút thăm</color> để bắt đầu quay.";
            /// Nếu có phần thưởng chưa nhận
            if (this.Data.LastStopPos >= 0 && this.Data.LastStopPos < this.Data.Items.Count)
            {
                /// Chọn ô này
                this.UIItems[this.Data.LastStopPos].Selected = true;

                /// Thông tin vật phẩm tương ứng
                G2C_LuckyCircle_ItemInfo itemInfo = this.Data.Items[this.Data.LastStopPos];
                /// Nếu tồn tại
                if (Loader.Loader.Items.TryGetValue(itemInfo.ItemID, out ItemData itemData))
                {
                    /// Vật phẩm tương ứng
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    /// Thiết lập Text
                    this.UIText_AwardInfo.text = string.Format("Nhận được <color=#1afff4>{0} cái</color> <color=#{1}>[{2}]</color>", itemInfo.Quantity, ColorUtility.ToHtmlStringRGB(KTGlobal.GetItemColor(itemGD)), KTGlobal.GetItemName(itemGD));

                    /// Hiện Button nhận
                    this.UIButton_StartTurn.interactable = false;
                    this.UIButton_GetAward.interactable = true;
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Server phản hồi trạng thái Button chức năng
        /// </summary>
        /// <param name="enableStartTurn"></param>
        /// <param name="enableGetAward"></param>
        public void ServerResponseButtonState(bool enableStartTurn, bool enableGetAward)
        {
            this.UIButton_StartTurn.interactable = enableStartTurn;
            this.UIButton_GetAward.interactable = enableGetAward;

            /// Hủy Toggle
            this.UIToggle_UseToken.Enable = enableStartTurn;
            this.UIToggle_UseBoundToken.Enable = enableStartTurn;
            this.UIToggle_UseItem.Enable = enableStartTurn;
        }

        /// <summary>
        /// Nhận yêu cầu từ Server thực hiện quay
        /// </summary>
        /// <param name="stopPos"></param>
        /// <param name="durationSec"></param>
        public void ServerStartTurn(int stopPos, int durationSec)
        {
            /// Toác
            if (this.Data == null)
            {
                KTGlobal.AddNotification("Thông tin vòng quay bị lỗi!");
                return;
            }
            /// Lỗi số lượng vật phẩm không khớp
            else if (this.Data.Items == null || this.Data.Items.Count != this.UIItems.Length)
            {
                KTGlobal.AddNotification("Thông tin vòng quay bị lỗi!");
                return;
            }

            /// Lưu lại vị trí dừng
            this.Data.LastStopPos = stopPos;
            /// Hủy kích hoạt toàn bộ các ô
            this.UnSelectAllItems();

            /// Thực hiện quay
            if (this.startTurnCoroutine != null)
            {
                this.StopCoroutine(this.startTurnCoroutine);
            }
            this.startTurnCoroutine = this.StartCoroutine(this.DoStartTurn(stopPos, durationSec));
        }

        /// <summary>
        /// Sự kiện Server phản hồi nhận vật phẩm thành công
        /// </summary>
        /// <param name="stopPos"></param>
        public void ServerGetAward(int stopPos)
        {
            /// Hủy vị trí dừng
            this.Data.LastStopPos = -1;
            /// Nếu có phần thưởng chưa nhận
            if (stopPos >= 0 && stopPos < this.Data.Items.Count)
            {
                /// Thông tin vật phẩm tương ứng
                G2C_LuckyCircle_ItemInfo itemInfo = this.Data.Items[stopPos];
                /// Thêm lịch sử
                this.UIHistoryBox.AppendHistory(itemInfo.ItemID, itemInfo.Quantity);
            }

            /// Hiện Button quya
            this.UIButton_StartTurn.interactable = true;
            this.UIButton_GetAward.interactable = false;
            /// Hủy Toggle
            this.UIToggle_UseToken.Enable = true;
            this.UIToggle_UseBoundToken.Enable = true;
            this.UIToggle_UseItem.Enable = true;
        }
        #endregion
    }
}
