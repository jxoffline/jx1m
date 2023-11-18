using FS.VLTK.UI.Main.TurnPlate;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung vòng quay may mắn đặc biệt
    /// </summary>
    public class UITurnPlate : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Danh sách ô vật phẩm
        /// </summary>
        [SerializeField]
        private UITurnPlate_ItemBox[] UIItems;

        /// <summary>
        /// Button bắt đầu quay
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Start;

        /// <summary>
        /// Button nhận thưởng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GetAwards;
        #endregion

        #region Properties
        private List<KeyValuePair<int, int>> _Items;
        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public List<KeyValuePair<int, int>> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện bắt đầu quay
        /// </summary>
        public Action StartTurn { get; set; }

        /// <summary>
        /// Sự kiện nhận thưởng
        /// </summary>
        public Action GetAwards { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực thi hiệu ứng quay
        /// </summary>
        private Coroutine playingCoroutine;
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
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Start.onClick.AddListener(this.ButtonStartTurn_Clicked);
            this.UIButton_GetAwards.onClick.AddListener(this.ButtonGetAward_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button bắt đầu quay được ấn
        /// </summary>
        private void ButtonStartTurn_Clicked()
        {
            /// Hủy kích hoạt Buttons
            this.UIButton_Start.interactable = false;
            this.UIButton_GetAwards.interactable = false;
            /// Thực thi sự kiện
            this.StartTurn?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button nhận thưởng được ấn
        /// </summary>
        private void ButtonGetAward_Clicked()
        {
            /// Hủy kích hoạt Buttons
            this.UIButton_Start.interactable = false;
            this.UIButton_GetAwards.interactable = false;
            /// Thực thi sự kiện
            this.GetAwards?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực thi sự kiện bỏ qua một số frame
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
        /// Thực thi sự kiện bỏ qua một số frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private void ExecuteSkipFrames(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }

        /// <summary>
        /// Thực hiện chạy hiệu ứng quay
        /// </summary>
        /// <param name="totalRounds"></param>
        /// <param name="stopPos"></param>
        /// <param name="duration"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        private IEnumerator DoPlay(int totalRounds, int stopPos, float duration)
        {
            /// Duyệt danh sách ô
            foreach (UITurnPlate_ItemBox uiItemBox in this.UIItems)
            {
                /// Hủy kích hoạt
                uiItemBox.Active = false;
            }

            /// Tổng số ô cần đi qua
            int totalCells = totalRounds * this.UIItems.Length + stopPos;

            /// Thời gian nghỉ
            float waitTime = duration / totalCells;

            /// Ô hiện tại
            int idx = 0;
            /// Lặp đến hết tổng số ô
            for (int i = 1; i <= totalCells; i++)
            {
                /// Tăng ô hiện tại lên
                idx++;
                /// Nếu vượt quá
                if (idx >= this.UIItems.Length)
                {
                    /// Chia ra
                    idx %= this.UIItems.Length;
                }
                /// Sáng ô tương ứng
                this.UIItems[idx].Highlight();
                /// Nghỉ
                yield return new WaitForSeconds(waitTime);
            }

            /// Kích hoạt ô tương ứng
            this.UIItems[stopPos].Active = true;

            /// Kích hoạt Button nhận thưởng
            this.UIButton_GetAwards.interactable = true;
        }

        /// <summary>
        /// Thực hiện quay
        /// </summary>
        /// <param name="totalRounds"></param>
        /// <param name="stopPos"></param>
        /// <param name="duration"></param>
        private void LocalPlay(int totalRounds, int stopPos, float duration)
        {
            /// Nếu tồn tại
            if (this.playingCoroutine != null)
            {
                /// Ngừng
                this.StopCoroutine(this.playingCoroutine);
            }
            /// Thực thi luồng
            this.playingCoroutine = this.StartCoroutine(this.DoPlay(totalRounds, stopPos, duration));
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Toác
            if (this._Items == null || this._Items.Count != 12)
            {
                /// Bỏ qua
                return;
            }

            /// Duyệt danh sách ô vật phẩm
            foreach (UITurnPlate_ItemBox uiItemBox in this.UIItems)
            {
                /// Hủy kích hoạt
                uiItemBox.Active = false;
            }

            /// Thứ tự
            int idx = -1;
            /// Duyệt danh sách vật phẩm
            foreach (KeyValuePair<int, int> pair in this._Items)
            {
                /// Tăng thứ tự
                idx++;

                /// Thêm dữ liệu ô vật phẩm tương ứng
                this.UIItems[idx].ItemID = pair.Key;
                this.UIItems[idx].Quantity = pair.Value;
            }

            /// Kích hoạt Button quay
            this.UIButton_Start.interactable = true;
            /// Hủy kích hoạt Button nhận thưởng
            this.UIButton_GetAwards.interactable = false;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật trạng thái của Button chức năng
        /// </summary>
        /// <param name="enableStart"></param>
        /// <param name="enableGetAwards"></param>
        public void UpdateButtonStatus(bool enableStart, bool enableGetAwards)
        {
            this.UIButton_Start.interactable = enableStart;
            this.UIButton_GetAwards.interactable = enableGetAwards;
        }

        /// <summary>
        /// Thực hiện quay
        /// </summary>
        /// <param name="totalRounds"></param>
        /// <param name="stopPos"></param>
        /// <param name="duration"></param>
        /// <param name="done"></param>
        public void Play(int totalRounds, int stopPos, float duration)
        {
            this.LocalPlay(totalRounds, stopPos, duration);
        }

        /// <summary>
        /// Sự kiện Server phản hồi nhận vật phẩm thành công
        /// </summary>
        /// <param name="stopPos"></param>
        public void ServerGetAward(int stopPos)
        {
            /// Hiện Button quay
            this.UIButton_Start.interactable = true;
            this.UIButton_GetAwards.interactable = false;
        }

        /// <summary>
        /// Cập nhật vị trí phần thưởng chưa nhận lần trước
        /// </summary>
        /// <param name="stopPos"></param>
        public void UpdateLastStopPos(int stopPos)
        {
            /// Toác
            if (stopPos < 0 || stopPos >= this.UIItems.Length)
            {
                /// Bỏ qua
                return;
            }

            /// Sáng ô tương ứng
            this.UIItems[stopPos].Active = true;

            /// Hiện Button nhận thưởng
            this.UIButton_GetAwards.interactable = true;
            /// Ẩn Button quay
            this.UIButton_Start.interactable = false;
        }
        #endregion
    }
}
