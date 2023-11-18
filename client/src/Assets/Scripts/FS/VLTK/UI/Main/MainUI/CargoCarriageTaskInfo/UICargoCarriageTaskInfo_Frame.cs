using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Logic;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.CargoCarriageTaskInfo
{
    /// <summary>
    /// Khung thông tin nhiệm vụ vận tiêu
    /// </summary>
    public class UICargoCarriageTaskInfo_Frame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text mô tả sự kiện
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Description;

        /// <summary>
        /// Text thông tin NPC giao tiêu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SourceNPC;

        /// <summary>
        /// Button NPC giao tiêu
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SourceNPC;

        /// <summary>
        /// Text thông tin NPC trả tiêu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_DestinationNPC;

        /// <summary>
        /// Button NPC trả tiêu
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_DestinationNPC;

        /// <summary>
        /// Text loại xe tiêu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CarriageType;

        /// <summary>
        /// Prefab Text yêu cầu cọc tiền
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MoneyRequirationPrefab;

        /// <summary>
        /// Prefab Text tiền thưởng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AwardMoneyPrefab;

        /// <summary>
        /// Prefab phần thưởng nhiệm vụ
        /// </summary>
        [SerializeField]
        private UIItemBox UI_AwardItemPrefab;

        /// <summary>
        /// Mark đã hoàn thành chưa
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_Completed;
        #endregion

        #region Properties
        private G2C_CargoCarriageTaskData _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public G2C_CargoCarriageTaskData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Nếu chưa chạy qua hàm Start thì thôi
                if (!this.isStarted)
                {
                    /// Bỏ qua
                    return;
                }
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Đã hoàn thành chưa
        /// </summary>
        public bool Completed
        {
            get
            {
                return this.UIMark_Completed.gameObject.activeSelf;
            }
            set
            {
                this.UIMark_Completed.gameObject.SetActive(value);
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform mô tả sự kiện
        /// </summary>
        private RectTransform transformEventDescriptionList;

        /// <summary>
        /// RectTransform tiền cọc
        /// </summary>
        private RectTransform transformMoneyRequirationList;

        /// <summary>
        /// RectTransform tiền thưởng
        /// </summary>
        private RectTransform transformAwardMoneyList;

        /// <summary>
        /// RectTransform vật phẩm thưởng
        /// </summary>
        private RectTransform transformAwardItemList;

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
            this.transformEventDescriptionList = this.UIText_Description.transform.parent.GetComponent<RectTransform>();
            this.transformMoneyRequirationList = this.UIText_MoneyRequirationPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformAwardMoneyList = this.UIText_AwardMoneyPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformAwardItemList = this.UI_AwardItemPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Khởi tạo ban đầu
            this.InitPrefabs();
            /// Xây lại giao diện
            this.RebuildLayouts();
            /// Làm mới dữ liệu
            this.RefreshData();
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
            /// Xây lại giao diện
            this.RebuildLayouts();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_SourceNPC.onClick.AddListener(this.ButtonGoToSourceNPC_Clicked);
            this.UIButton_DestinationNPC.onClick.AddListener(this.ButtonGoToDestinationNPC_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            /// Ẩn khung
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button dịch chuyển đến chỗ NPC giao tiêu được ấn
        /// </summary>
        private void ButtonGoToSourceNPC_Clicked()
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Thực hiện tìm đường đến chỗ NPC tương ứng
            KTGlobal.QuestAutoFindPath(this._Data.SourceMapCode, this._Data.SourceNPCPosX, this._Data.SourceNPCPosY, () =>
            {
                AutoQuest.Instance.StopAutoQuest();
                AutoPathManager.Instance.StopAutoPath();
                GSprite sprite = KTGlobal.FindNearestNPCByName(this._Data.SourceNPCName);
                if (sprite == null)
                {
                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                    return;
                }
                Global.Data.TargetNpcID = sprite.RoleID;
                Global.Data.GameScene.NPCClick(sprite);
            });
        }

        /// <summary>
        /// Sự kiện khi Button dịch chuyển đến chỗ NPC trả tiêu được ấn
        /// </summary>
        private void ButtonGoToDestinationNPC_Clicked()
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Thực hiện tìm đường đến chỗ NPC tương ứng
            KTGlobal.QuestAutoFindPath(this._Data.DestinationMapCode, this._Data.DestinationNPCPosX, this._Data.DestinationNPCPosY, () =>
            {
                AutoQuest.Instance.StopAutoQuest();
                AutoPathManager.Instance.StopAutoPath();
                GSprite sprite = KTGlobal.FindNearestNPCByName(this._Data.DestinationNPCName);
                if (sprite == null)
                {
                    KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                    return;
                }
                Global.Data.TargetNpcID = sprite.RoleID;
                Global.Data.GameScene.NPCClick(sprite);
            });
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực thi sự kiện bỏ qua một số Frame
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

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayouts()
        {
            /// Nếu đối tượng không được kích hoạt thì thôi
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Thực thi sự kiện ở Frame tiếp theo
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformEventDescriptionList);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformMoneyRequirationList);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformAwardMoneyList);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformAwardItemList);
            });
        }

        /// <summary>
        /// Cập nhật dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                /// Ẩn khung
                this.Hide();
                /// Bỏ qua
                return;
            }

            /// Xóa toàn bộ yêu cầu cọc tiền
            foreach (Transform child in this.transformMoneyRequirationList.transform)
            {
                if (child.gameObject != this.UIText_MoneyRequirationPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Xóa toàn bộ phần thưởng tiền
            foreach (Transform child in this.transformAwardMoneyList.transform)
            {
                if (child.gameObject != this.UIText_AwardMoneyPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Xóa toàn bộ phần thưởng vật phẩm
            foreach (Transform child in this.transformAwardItemList.transform)
            {
                if (child.gameObject != this.UI_AwardItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Tên bản đồ giao tiêu
            string sourceMapName = "Chưa rõ";
            /// Thông tin bản đồ giao tiêu
            if (Loader.Loader.Maps.TryGetValue(this._Data.SourceMapCode, out Entities.Config.Map sourceMapData))
            {
                sourceMapName = sourceMapData.Name;
            }
            /// Thông tin NPC giao tiêu
            this.UIText_SourceNPC.text = string.Format("<color=yellow>{0}</color>\nVị trí: <color=green>{1}</color>", this._Data.SourceNPCName, sourceMapName);

            /// Tên bản đồ nhận tiêu
            string destinationMapName = "Chưa rõ";
            /// Thông tin bản đồ nhận tiêu
            if (Loader.Loader.Maps.TryGetValue(this._Data.DestinationMapCode, out Entities.Config.Map destinationMapData))
            {
                destinationMapName = destinationMapData.Name;
            }
            /// Thông tin NPC nhận tiêu
            this.UIText_DestinationNPC.text = string.Format("<color=yellow>{0}</color>\nVị trí: <color=green>{1}</color>", this._Data.DestinationNPCName, destinationMapName);

            /// Tên loại tiêu
            string carriageType = "Chưa rõ";
            /// Loại tiêu
            switch (this._Data.Type)
            {
                case 1:
                {
                    carriageType = "Thường";
                    break;
                }
                case 2:
                {
                    carriageType = "Bạch Kim";
                    break;
                }
                case 3:
                {
                    carriageType = "Hoàng Kim";
                    break;
                }
            }
            /// Thiết lập loại tiêu
            this.UIText_CarriageType.text = carriageType;

            /// Nếu có yêu cầu cọc bạc
            if (this._Data.RequireMoney > 0)
            {
                /// Tạo mới
                TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_MoneyRequirationPrefab);
                uiText.transform.SetParent(this.transformMoneyRequirationList, false);
                uiText.gameObject.SetActive(true);
                uiText.text = string.Format("Bạc: <color=yellow>{0}</color>", KTGlobal.GetDisplayMoney(this._Data.RequireMoney));
            }
            /// Nếu có yêu cầu cọc bạc khóa
            if (this._Data.RequireBoundMoney > 0)
            {
                /// Tạo mới
                TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_MoneyRequirationPrefab);
                uiText.transform.SetParent(this.transformMoneyRequirationList, false);
                uiText.gameObject.SetActive(true);
                uiText.text = string.Format("Bạc khóa: <color=yellow>{0}</color>", KTGlobal.GetDisplayMoney(this._Data.RequireBoundMoney));
            }
            /// Nếu có yêu cầu cọc KNB
            if (this._Data.RequireToken > 0)
            {
                /// Tạo mới
                TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_MoneyRequirationPrefab);
                uiText.transform.SetParent(this.transformMoneyRequirationList, false);
                uiText.gameObject.SetActive(true);
                uiText.text = string.Format("KNB: <color=yellow>{0}</color>", KTGlobal.GetDisplayMoney(this._Data.RequireToken));
            }
            /// Nếu có yêu cầu cọc KNB khóa
            if (this._Data.RequireBoundToken > 0)
            {
                /// Tạo mới
                TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_MoneyRequirationPrefab);
                uiText.transform.SetParent(this.transformMoneyRequirationList, false);
                uiText.gameObject.SetActive(true);
                uiText.text = string.Format("KNB khóa: <color=yellow>{0}</color>", KTGlobal.GetDisplayMoney(this._Data.RequireBoundToken));
            }

            /// Nếu có phần thưởng bạc
            if (this._Data.AwardMoney > 0)
            {
                /// Tạo mới
                TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_AwardMoneyPrefab);
                uiText.transform.SetParent(this.transformAwardMoneyList, false);
                uiText.gameObject.SetActive(true);
                uiText.text = string.Format("Bạc: <color=yellow>{0}</color>", KTGlobal.GetDisplayMoney(this._Data.AwardMoney));
            }
            /// Nếu có phần thưởng bạc khóa
            if (this._Data.AwardBoundMoney > 0)
            {
                /// Tạo mới
                TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_AwardMoneyPrefab);
                uiText.transform.SetParent(this.transformAwardMoneyList, false);
                uiText.gameObject.SetActive(true);
                uiText.text = string.Format("Bạc khóa: <color=yellow>{0}</color>", KTGlobal.GetDisplayMoney(this._Data.AwardBoundMoney));
            }
            /// Nếu có phần thưởng KNB
            if (this._Data.AwardToken > 0)
            {
                /// Tạo mới
                TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_AwardMoneyPrefab);
                uiText.transform.SetParent(this.transformAwardMoneyList, false);
                uiText.gameObject.SetActive(true);
                uiText.text = string.Format("KNB: <color=yellow>{0}</color>", KTGlobal.GetDisplayMoney(this._Data.AwardToken));
            }
            /// Nếu có phần thưởng KNB khóa
            if (this._Data.AwardBoundToken > 0)
            {
                /// Tạo mới
                TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_AwardMoneyPrefab);
                uiText.transform.SetParent(this.transformAwardMoneyList, false);
                uiText.gameObject.SetActive(true);
                uiText.text = string.Format("KNB khóa: <color=yellow>{0}</color>", KTGlobal.GetDisplayMoney(this._Data.AwardBoundToken));
            }

            /// Nếu có danh sách phần thưởng
            if (this._Data.AwardItems != null)
            {
                /// Duyệt danh sách phần thưởng
                foreach (CargoCarriageAwardItemData itemInfo in this._Data.AwardItems)
                {
                    /// Thông tin vật phẩm tương ứng
                    if (!Loader.Loader.Items.TryGetValue(itemInfo.ItemID, out Entities.Config.ItemData itemData))
                    {
                        continue;
                    }

                    /// Tạo mới
                    UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_AwardItemPrefab);
                    uiItemBox.transform.SetParent(this.transformAwardItemList, false);
                    uiItemBox.gameObject.SetActive(true);

                    /// Tạo mới vật phẩm xem trước
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    itemGD.GCount = itemInfo.Quantity;
                    itemGD.Binding = itemInfo.Bound ? 1 : 0;

                    /// Thiết lập vào ô
                    uiItemBox.Data = itemGD;
                }
            }

            /// Xây lại giao diện
            this.RebuildLayouts();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiển thị khung
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
