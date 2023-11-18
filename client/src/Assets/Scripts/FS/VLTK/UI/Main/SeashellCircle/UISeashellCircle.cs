using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using FS.VLTK.UI.Main.SeashellCircle;
using System.Collections;
using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using UnityEngine.UI;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung Bách Bảo Rương
    /// </summary>
    public class UISeashellCircle : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab Text lịch sử quay
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_HistoryPrefab;

        /// <summary>
        /// Thanh cuộn danh sách lịch sử quay
        /// </summary>
        [SerializeField]
        private ScrollRect UIScroll_History;

        /// <summary>
        /// Toggle cược 2 sò
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_Bet2;

        /// <summary>
        /// Toggle cược 10 sò
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_Bet10;

        /// <summary>
        /// Toggle cược 20 sò
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_Bet50;

        /// <summary>
        /// Text tổng số vỏ sò tích lũy ở Server
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ServerTotalSeashells;

        /// <summary>
        /// Text số vỏ sò hiện có
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CurrentSeashellOwning;

        /// <summary>
        /// Text mô tả phần thưởng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AwardDesc;

        /// <summary>
        /// Button bắt đầu
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Start;

        /// <summary>
        /// Button nhận thưởng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GetAward;

        /// <summary>
        /// Button nhận vỏ sò
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GetSeashell;

        /// <summary>
        /// RectTransform danh sách tầng
        /// </summary>
        [SerializeField]
        private RectTransform UITransform_StageList;

        /// <summary>
        /// Thông tin quà thưởng theo tầng
        /// </summary>
        [SerializeField]
        private UISeashellCircle_AwardStageInfo[] UI_AwardStages;

        /// <summary>
        /// Danh sách ô item
        /// </summary>
        [SerializeField]
        private UISeashellCircle_Item[] UIItems;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách lịch sử quay
        /// </summary>
        private RectTransform transformHistoryTextList = null;

        /// <summary>
        /// RectTransform danh sách tầng
        /// </summary>
        private RectTransform transformStageList = null;

        /// <summary>
        /// Luồng thực thi quay
        /// </summary>
        private Coroutine turningCoroutine;

        /// <summary>
        /// Đã khởi tạo chưa
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Tổng số nội dung lịch sử tối đa được lưu
        /// </summary>
        private const int MaxHistoryCount = 20;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện bắt đầu
        /// </summary>
        public Action<int> StartTurn { get; set; }

        /// <summary>
        /// Sự kiện nhận quà
        /// </summary>
        public Action GetAward { get; set; }

        /// <summary>
        /// Sự kiện nhận sò
        /// </summary>
        public Action GetSeashell { get; set; }

        private long _ServerTotalSeashells = 0;
        /// <summary>
        /// Tổng số sò hệ thống tích lũy
        /// </summary>
        public long ServerTotalSeashells
        {
            get
            {
                return this._ServerTotalSeashells;
            }
            set
            {
                this._ServerTotalSeashells = value;
                /// Thiết lập Text
                this.UIText_ServerTotalSeashells.text = string.Format("Hệ thống hiện tích trữ <color=green>{0}</color> vỏ sò.", value);
            }
        }

        private int _CurrentStage = -1;
        /// <summary>
        /// Tầng hiện tại của quà
        /// </summary>
        public int CurrentStage
        {
            get
            {
                return this._CurrentStage;
            }
            set
            {
                this._CurrentStage = value;
                /// Cập nhật thông tin
                this.UpdateStage();
            }
        }

        private int _CurrentBet = 0;
        /// <summary>
        /// Số lượng cược
        /// </summary>
        public int CurrentBet
        {
            get
            {
                return this._CurrentBet;
            }
            set
            {
                /// Nếu không thỏa mãn thì mặc định chọn cược 2
                if (value != 2 && value != 10 && value != 50)
                {
                    value = 2;
                }

                /// Giá trị cũ
                int oldValue = this._CurrentBet;
                this._CurrentBet = value;
                /// Cập nhật Bet
                this.UIText_AwardDesc.text = string.Format("Cược <color=green>{0}</color> vỏ sò.", value);
                /// Nếu giá trị có thay đổi
                if (oldValue != value)
                {
                    /// Sáng Toggle tương ứng
                    switch (value)
                    {
                        case 2:
                        {
                            this.UIToggle_Bet2.Active = true;
                            break;
                        }
                        case 10:
                        {
                            this.UIToggle_Bet10.Active = true;
                            break;
                        }
                        case 50:
                        {
                            this.UIToggle_Bet50.Active = true;
                            break;
                        }
                    }
                }

                /// Cập nhật hiển thị
                this.UpdateBet();
            }
        }

        private int _CurrentPos;
        /// <summary>
        /// Vị trí hiện tại
        /// </summary>
        public int CurrentPos
        {
            get
            {
                return this._CurrentPos;
            }
            set
            {
                this._CurrentPos = value;
                /// Cập nhật vị trí
                if (value >= 0 && value < this.UIItems.Length)
                {
                    /// Sáng ô ở vị trí đích
                    this.UIItems[value].Selected = true;
                }
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformHistoryTextList = this.UIText_HistoryPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.UpdateTotalSeashellsOwning();
            this.ClearHistory();
            this.RebuildHistoryLayout();
            this.initialized = true;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Start.onClick.AddListener(this.ButtonStart_Clicked);
            this.UIButton_GetAward.onClick.AddListener(this.ButtonGetAward_Clicked);
            this.UIButton_GetSeashell.onClick.AddListener(this.ButtonGetSeashell_Clicked);
            this.UIToggle_Bet2.OnSelected = this.ToggleBet2_Selected;
            this.UIToggle_Bet10.OnSelected = this.ToggleBet10_Selected;
            this.UIToggle_Bet50.OnSelected = this.ToggleBet50_Selected;
            /// Nếu không có số cược
            if (this._CurrentBet != 2 && this._CurrentBet != 10 && this._CurrentBet != 50)
            {
                /// Cược 2 sò mặc định
                this.CurrentBet = 2;
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle đặt cược 2 vỏ sò được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleBet2_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.CurrentBet = 2;
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle đặt cược 10 vỏ sò được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleBet10_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.CurrentBet = 10;
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle đặt cược 50 vỏ sò được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleBet50_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.CurrentBet = 50;
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
        /// Sự kiện khi Button bắt đầu quay được ấn
        /// </summary>
        private void ButtonStart_Clicked()
        {
            /// Số sò đặt cược
            int betNumber = 0;
            if (this.UIToggle_Bet2.Active)
            {
                betNumber = 2;
            }
            else if (this.UIToggle_Bet10.Active)
            {
                betNumber = 10;
            }
            else if (this.UIToggle_Bet50.Active)
            {
                betNumber = 50;
            }

            /// Nếu không thấy thông tin cược
            if (betNumber == 0)
            {
                KTGlobal.AddNotification("Hãy đặt cược số vỏ sò của bạn!");
                return;
            }

            /// Thực thi sự kiện quay
            this.StartTurn?.Invoke(betNumber);
            /// Khóa toàn bộ Button chức năng
            this.UIButton_Start.interactable = false;
            this.UIButton_GetAward.interactable = false;
            this.UIButton_GetSeashell.interactable = false;
            /// Khóa toàn bộ Toggle
            this.UIToggle_Bet2.Enable = false;
            this.UIToggle_Bet10.Enable = false;
            this.UIToggle_Bet50.Enable = false;
        }

        /// <summary>
        /// Sự kiện khi Button nhận quà được ấn
        /// </summary>
        private void ButtonGetAward_Clicked()
        {
            /// Thực thi sự kiện nhận quà
            this.GetAward?.Invoke();
            /// Khóa toàn bộ Button chức năng
            this.UIButton_Start.interactable = false;
            this.UIButton_GetAward.interactable = false;
            this.UIButton_GetSeashell.interactable = false;
            /// Khóa toàn bộ Toggle
            this.UIToggle_Bet2.Enable = false;
            this.UIToggle_Bet10.Enable = false;
            this.UIToggle_Bet50.Enable = false;
        }

        /// <summary>
        /// Sự kiện khi Button nhận sò được ấn
        /// </summary>
        private void ButtonGetSeashell_Clicked()
        {
            /// Thực thi sự kiện nhận sò
            this.GetSeashell?.Invoke();
            /// Khóa toàn bộ Button chức năng
            this.UIButton_Start.interactable = false;
            this.UIButton_GetAward.interactable = false;
            this.UIButton_GetSeashell.interactable = false;
            /// Khóa toàn bộ Toggle
            this.UIToggle_Bet2.Enable = false;
            this.UIToggle_Bet10.Enable = false;
            this.UIToggle_Bet50.Enable = false;
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
        /// <param name="rectTransform"></param>
        private void RebuildLayout(RectTransform rectTransform)
        {
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            /// Thực hiện xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

                /// Chuyển Scroll xuống dưới cùng
                this.UIScroll_History.normalizedPosition = new Vector2(0, 0);
            }));
        }

        #region History
        /// <summary>
        /// Xây lại giao diện lịch sử
        /// </summary>
        private void RebuildHistoryLayout()
        {
            this.RebuildLayout(this.transformHistoryTextList);
        }

        /// <summary>
        /// Xóa lịch sử
        /// </summary>
        private void ClearHistory()
        {
            foreach (Transform child in this.transformHistoryTextList.transform)
            {
                if (child.gameObject != this.UIText_HistoryPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm lịch sử
        /// </summary>
        /// <param name="message"></param>
        private void AppendHistory(string message)
        {
            int idx = 1;
            int totalChild = this.transformHistoryTextList.childCount;
            /// Xóa dần nếu số phần tử vượt quá thiết lập
            while (totalChild > UISeashellCircle.MaxHistoryCount)
            {
                GameObject.Destroy(this.transformHistoryTextList.transform.GetChild(idx).gameObject);
                idx++;
                totalChild--;
            }

            TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_HistoryPrefab);
            uiText.transform.SetParent(this.transformHistoryTextList, false);
            uiText.gameObject.SetActive(true);
            uiText.text = message;
            /// Xây lại giao diện
            this.RebuildHistoryLayout();
        }
        #endregion

        #region Stage & Bet
        /// <summary>
        /// Cập nhật trạng thái tầng
        /// </summary>
        private void UpdateStage()
        {
            /// Nếu tầng không tồn tại
            if (this._CurrentStage < 0 || this._CurrentStage >= this.UI_AwardStages.Length)
            {
                /// Nếu chưa khởi tạo
                if (!this.initialized)
                {
                    this.UIText_AwardDesc.text = "";
                }
                /// Nếu quay vào rương
                else if (this.IsTreasure(this._CurrentPos))
                {
                    this.UIText_AwardDesc.text = "Chúc mừng bạn nhận được Bảo rương!";
                }
                else
                {
                    this.UIText_AwardDesc.text = "Thật đáng tiếc, cần chút may mắn nữa thôi!";
                }
                this.UITransform_StageList.gameObject.SetActive(false);
                return;
            }
            /// Nếu vị trí không tồn tại
            else if (this._CurrentPos < 0 || this._CurrentPos >= this.UIItems.Length)
            {
                /// Nếu chưa khởi tạo
                if (!this.initialized)
                {
                    this.UIText_AwardDesc.text = "";
                }
                /// Nếu quay vào rương
                else if (this.IsTreasure(this._CurrentPos))
                {
                    this.UIText_AwardDesc.text = "Chúc mừng bạn nhận được Bảo rương!";
                }
                else
                {
                    this.UIText_AwardDesc.text = "Thật đáng tiếc, cần chút may mắn nữa thôi!";
                }
                this.UITransform_StageList.gameObject.SetActive(false);
                return;
            }

            /// Hiện thông tin tầng tương ứng
            this.UITransform_StageList.gameObject.SetActive(true);
            /// Sáng Toggle tại vị trí tương ứng
            this.UI_AwardStages[this._CurrentStage].Selected = true;

            /// Lượng cược
            int nBet = this._CurrentBet;
            /// Lượng nhân thêm
            int multiply = 1;
            /// Loại cược là gì
            switch (nBet)
            {
                case 2:
                {
                    multiply = Loader.Loader.SeashellCircleInfo.BetRates.Bet_2;
                    break;
                }
                case 10:
                {
                    multiply = Loader.Loader.SeashellCircleInfo.BetRates.Bet_10;
                    break;
                }
                case 50:
                {
                    multiply = Loader.Loader.SeashellCircleInfo.BetRates.Bet_50;
                    break;
                }
            }

            /// Nếu nhận được rương
            if (Loader.Loader.SeashellCircleInfo.TreasurePosition.Contains(this._CurrentPos))
            {
                this.UIText_AwardDesc.text = string.Format("Nhận được <color=green>{0} cái</color> <color=yellow>[{1}]</color>", multiply, KTGlobal.GetItemName(KTGlobal.SeashellTreasureID));
            }
            /// Nếu nhận được quà thường
            else
            {
                /// Cập nhật thông tin Text của quà trong tầng
                KeyValuePair<KTSeashellCircle.Cell, int> pair = this.GetCellAtPos(this._CurrentPos);
                /// Nếu không tồn tại
                if (pair.Key == null)
                {
                    this.UITransform_StageList.gameObject.SetActive(false);
                    this.UIText_AwardDesc.text = "";
                    return;
                }


                /// Nếu là tầng 0
                //if (this._CurrentStage == 0)
                {
                    /// Thứ tự tầng đang xét
                    int stageIdx = 0;
                    /// Duyệt danh sách và thiết lập Text tương ứng
                    foreach (UISeashellCircle_AwardStageInfo awardStageInfo in this.UI_AwardStages)
                    {
                        /// Lượng nhận được ở tầng tương ứng
                        int nValue = pair.Key.ValuesByStage[stageIdx];
                        /// Nếu là Huyền Tinh
                        if (pair.Key.Type == KTSeashellCircle.SeashellCircleCellType.CrystalStone)
                        {
                            awardStageInfo.Text = KTGlobal.GetItemName(nValue);
                        }
                        /// Nếu là Tinh hoạt lực
                        else if (pair.Key.Type == KTSeashellCircle.SeashellCircleCellType.GatherMakePoint)
                        {
                            awardStageInfo.Text = string.Format("{0} {1}", nValue, pair.Key.Name);
                        }
                        /// Nếu là bạc hoặc KNB khóa
                        else
                        {
                            awardStageInfo.Text = string.Format("{0} {1}", KTGlobal.GetDisplayMoney(nValue), pair.Key.Name);
                        }

                        /// Tăng thứ tự tầng đang xét lên
                        stageIdx++;
                    }
                }

                /// Lượng nhận được
                int value = pair.Key.ValuesByStage[this._CurrentStage];
                /// Lượng đổi được ra sò
                int totalSeashells = Loader.Loader.SeashellCircleInfo.ExchangeSeashells[this._CurrentStage].Value * this._CurrentBet / 100;
                /// Kiểm tra theo loại
                switch (pair.Key.Type)
                {
                    /// Huyền Tinh
                    case KTSeashellCircle.SeashellCircleCellType.CrystalStone:
                    {
                        this.UIText_AwardDesc.text = string.Format("Nhận được <color=green>{0} cái</color> <color=yellow>[{1}]</color> hoặc đổi được <color=green>{2}</color> <color=yellow>[{3}]</color>", multiply, KTGlobal.GetItemName(value), totalSeashells, KTGlobal.GetItemName(KTGlobal.SeashellItemID));
                        break;
                    }
                    /// Tinh hoạt lực
                    case KTSeashellCircle.SeashellCircleCellType.GatherMakePoint:
                    {
                        this.UIText_AwardDesc.text = string.Format("Nhận được <color=green>{0}</color> <color=yellow>Tinh hoạt lực</color> hoặc đổi được <color=green>{1}</color> <color=yellow>[{2}]</color>", value * multiply, totalSeashells, KTGlobal.GetItemName(KTGlobal.SeashellItemID));
                        break;
                    }
                    /// Bạc khóa
                    case KTSeashellCircle.SeashellCircleCellType.Money:
                    {
                        this.UIText_AwardDesc.text = string.Format("Nhận được <color=green>{0}</color> <color=yellow>Bạc khóa</color> hoặc đổi được <color=green>{1}</color> <color=yellow>[{2}]</color>", KTGlobal.GetDisplayMoney(value * multiply), totalSeashells, KTGlobal.GetItemName(KTGlobal.SeashellItemID));
                        break;
                    }
                    /// KNB khóa
                    case KTSeashellCircle.SeashellCircleCellType.BoundToken:
                    {
                        this.UIText_AwardDesc.text = string.Format("Nhận được <color=green>{0}</color> <color=yellow>KNB khóa</color> hoặc đổi được <color=green>{1}</color> <color=yellow>[{2}]</color>", KTGlobal.GetDisplayMoney(value * multiply), totalSeashells, KTGlobal.GetItemName(KTGlobal.SeashellItemID));
                        break;
                    }
                    default:
                    {
                        this.UIText_AwardDesc.text = "";
                        break;
                    }
                }
            }
            
        }

        /// <summary>
        /// Cập nhật hiển thị cược
        /// </summary>
        private void UpdateBet()
        {
            int number = 1;
            switch (this._CurrentBet)
            {
                case 2:
                {
                    number = Loader.Loader.SeashellCircleInfo.BetRates.Bet_2;
                    break;
                }
                case 10:
                {
                    number = Loader.Loader.SeashellCircleInfo.BetRates.Bet_10;
                    break;
                }
                case 50:
                {
                    number = Loader.Loader.SeashellCircleInfo.BetRates.Bet_50;
                    break;
                }
            }

            /// Duyệt danh sách và thiết lập Text tương ứng
            foreach (UISeashellCircle_AwardStageInfo awardStageInfo in this.UI_AwardStages)
            {
                awardStageInfo.Number = number;
            }
        }
        #endregion

        #region Turn
        /// <summary>
        /// Bắt đầu quay
        /// </summary>
        /// <param name="fromCell"></param>
        /// <param name="totalCells"></param>
        /// <param name="duration"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        private IEnumerator DoPlay(int fromCell, int totalCells, float duration, Action done)
        {
            /// Xóa toàn bộ các ô sáng
            foreach (UISeashellCircle_Item item in this.UIItems)
            {
                item.Selected = false;
            }

            /// Thời gian mỗi lần sang ô kế tiếp
            float durationEach = duration / totalCells;
            /// Tổng số ô đã đi qua
            int cellsPassed = 0;
            /// Vị thứ tự ô hiện tại
            int currentCellIndex = fromCell;
            /// Chừng nào chưa hết số ô
            while (cellsPassed < totalCells)
            {
                /// Thực hiện hiệu ứng ở ô hiện tại
                this.UIItems[currentCellIndex].PlayAnimation();
                /// Tăng số ô đã đi qua
                cellsPassed++;
                /// Bỏ qua thời gian đợi
                yield return new WaitForSeconds(durationEach);
                /// Cập nhật vị trí tiếp theo
                currentCellIndex++;
                /// Nếu vượt quá kích thước số ô
                if (currentCellIndex >= this.UIItems.Length)
                {
                    /// Về vị trí 0
                    currentCellIndex = 0;
                }
            }
            /// Chọn ô ở vị trí đích
            //this.UIItems[currentCellIndex].Selected = true;
            this.CurrentPos = currentCellIndex;
            /// Ngừng luồng thực thi quay
            this.turningCoroutine = null;
            /// Thực thi sự kiện hoàn tất
            done?.Invoke();
        }
        #endregion

        #region Ultilities
        /// <summary>
        /// Kiểm tra vị trí hiện tại có phải của rương không
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private bool IsTreasure(int pos)
        {
            return Loader.Loader.SeashellCircleInfo.TreasurePosition.Contains(pos);
        }

        /// <summary>
        /// Trả về thông tin ô tại vị trí tương ứng
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private KeyValuePair<KTSeashellCircle.Cell, int> GetCellAtPos(int pos)
        {
            /// Duyệt danh sách ô
            foreach (KTSeashellCircle.Cell cell in Loader.Loader.SeashellCircleInfo.Cells)
            {
                /// Nếu nằm trong vị trí 1 sao
                if (cell.Position_1.Any(x => x == pos))
                {
                    return new KeyValuePair<KTSeashellCircle.Cell, int>(cell, 1);
                }
                /// Nếu nằm trong vị trí 2 sao
                if (cell.Position_2.Any(x => x == pos))
                {
                    return new KeyValuePair<KTSeashellCircle.Cell, int>(cell, 2);
                }
                /// Nếu nằm trong vị trí 3 sao
                if (cell.Position_3.Any(x => x == pos))
                {
                    return new KeyValuePair<KTSeashellCircle.Cell, int>(cell, 3);
                }
            }
            /// Không tìm thấy
            return new KeyValuePair<KTSeashellCircle.Cell, int>(null, -1);
        }
        #endregion
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện quay
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="totalCells"></param>
        /// <param name="duration"></param>
        /// <param name="done"></param>
        public void StartPlayTurn(int fromPos, int totalCells, float duration, Action done)
        {
            /// Nếu luồng cũ tồn tại thì xóa
            if (this.turningCoroutine != null)
            {
                this.StopCoroutine(this.turningCoroutine);
            }
            /// Nếu vị trí ban đầu nằm ngoài phạm vi
            if (fromPos < 0 || fromPos >= this.UIItems.Length)
            {
                /// Mặc định từ vị trí đầu tiên
                fromPos = 0;
            }
            /// Tạo luồng thực hiện quay
            this.turningCoroutine = this.StartCoroutine(this.DoPlay(fromPos, totalCells, duration, done));
        }

        /// <summary>
        /// Cập nhật tổng số sò hiện có
        /// </summary>
        public void UpdateTotalSeashellsOwning()
        {
            /// Tổng số sò hiện có
            int totalSeashells = 0;
            /// Nếu túi đồ không rỗng
            if (Global.Data.RoleData.GoodsDataList != null)
            {
                /// Tính tổng số vỏ sò hiện có
                totalSeashells = Global.Data.RoleData.GoodsDataList.Where(x => KTGlobal.SeashellItemID == x.GoodsID).Sum(x => x.GCount);
            }
            /// Cập nhật Text
            this.UIText_CurrentSeashellOwning.text = totalSeashells.ToString();
        }

        /// <summary>
        /// Cập nhật trạng thái của Buttons và Toggles tương ứng
        /// </summary>
        /// <param name="enableBet"></param>
        /// <param name="enableStartTurn"></param>
        /// <param name="enableGet"></param>
        /// <param name="enableExchange"></param>
        public void UpdateButtonsState(bool enableBet, bool enableStartTurn, bool enableGet, bool enableExchange)
        {
            this.UIToggle_Bet2.Enable = enableBet;
            this.UIToggle_Bet10.Enable = enableBet;
            this.UIToggle_Bet50.Enable = enableBet;

            this.UIButton_Start.interactable = enableStartTurn;
            this.UIButton_GetAward.interactable = enableGet;
            this.UIButton_GetSeashell.interactable = enableExchange;
        }

        /// <summary>
        /// Làm rỗng mô tả tầng
        /// </summary>
        public void ClearStageDesc()
        {
            this.UITransform_StageList.gameObject.SetActive(false);
            this.UIText_AwardDesc.text = "";
        }

        /// <summary>
        /// Thêm nội dung vào lịch sử
        /// </summary>
        /// <param name="text"></param>
        public void AddHistory(string text)
        {
            this.AppendHistory(text);
        }
        #endregion
    }
}
