using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using ProtoBuf;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung nhiệm vụ bang hội
    /// </summary>
    public class UIGuildEx_Quest : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text số nhiệm vụ đã làm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalQuestLeft;

        /// <summary>
        /// Text tên nhiệm vụ hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CurrentQuestName;

        /// <summary>
        /// Text tiến độ nhiệm vụ hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CurrentQuestValue;

        /// <summary>
        /// Button đổi nhiệm vụ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_ChangeQuest;

        /// <summary>
        /// Button bỏ nhiệm vụ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AbandonQuest;

        /// <summary>
        /// Text số bang cống tiêu hao khi đổi nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ChangeQuestCostMoney;

        /// <summary>
        /// Text số nhiệm vụ tối đa trong ngày
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MaxQuestPerDay;

        /// <summary>
        /// Text mô tả nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TaskDescription;

        /// <summary>
        /// Button thực hiện nhiệm vụ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_DoTask;

        /// <summary>
        /// Text chi tiết nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TaskDetail;

        /// <summary>
        /// Text thưởng nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TaskAward;

        /// <summary>
        /// Prefab vật phẩm thưởng nhiệm vụ
        /// </summary>
        [SerializeField]
        private UIItemBox UI_TaskAwardItemPrefab;
        #endregion

        #region Properties
        private GuildTask _Data;
        /// <summary>
        /// Thông tin nhiệm vụ bang hội
        /// </summary>
        public GuildTask Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện đổi nhiệm vụ
        /// </summary>
        public Action ChangeTask { get; set; }

        /// <summary>
        /// Sự kiện bỏ nhiệm vụ
        /// </summary>
        public Action AbandonTask { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm thưởng
        /// </summary>
        private RectTransform transformAwardItemList;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformAwardItemList = this.UI_TaskAwardItemPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_ChangeQuest.onClick.AddListener(this.ButtonChangeTask_Clicked);
            this.UIButton_AbandonQuest.onClick.AddListener(this.ButtonAbandonTask_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }
        
        /// <summary>
        /// Sự kiện khi Button đổi nhiệm vụ được ấn
        /// </summary>
        private void ButtonChangeTask_Clicked()
        {
            /// Nếu không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện thao tác này!");
                return;
            }

            /// Thực thi sự kiện
            this.ChangeTask?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button bỏ nhiệm vụ được ấn
        /// </summary>
        private void ButtonAbandonTask_Clicked()
        {
            /// Nếu không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện thao tác này!");
                return;
            }

            /// Thực thi sự kiện
            this.AbandonTask?.Invoke();
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
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Toác
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }
            else if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Làm rỗng danh sách vật phẩm yêu cầu
            foreach (Transform child in this.transformAwardItemList.transform)
            {
                if (child.gameObject != this.UI_TaskAwardItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }


            this.UIText_TotalQuestLeft.text = this._Data.TaskCountInDay+"";
            /// Thông tin nhiệm vụ
            if (Loader.Loader.Tasks.TryGetValue(this._Data.TaskID, out Entities.Config.TaskDataXML taskDataXML))
            {
                /// Đổ dữ liệu
                this.UIText_CurrentQuestName.text = taskDataXML.Title;
                this.UIText_CurrentQuestValue.text = string.Format("{0}/{1}", this._Data.TaskValue, taskDataXML.TargetNum);

                /// Tạo mới TaskData
                TaskData taskData = new TaskData()
                {
                    DbID = -1,
                    DoingTaskID = taskDataXML.ID,
                    DoingTaskVal1 = this._Data.TaskValue,
                    DoingTaskVal2 = -1,
                    DoingTaskFocus = -1,
                    DoneCount = 0,
                    StarLevel = 0,
                    AddDateTime = this._Data.DayCreate,
                };

                
                /// Mô tả nhiệm vụ
                this.UIText_TaskDescription.text = taskDataXML.DoingTalk;
                /// Chi tiết nhiệm vụ
                this.UIText_TaskDetail.text = KTGlobal.GetTaskGuildDetailString(taskData, taskDataXML, false, out Action clickEvent);
                /// Xóa và thêm sự kiện
                this.UIButton_DoTask.onClick.RemoveAllListeners();
                this.UIButton_DoTask.onClick.AddListener(() => {
                    clickEvent?.Invoke();
                });

                List<string> awardMoneyAndExpString = new List<string>();
                if (taskDataXML.Experienceaward > 0)
                {
                    awardMoneyAndExpString.Add(string.Format("Kinh nghiệm: <color=#faed00>{0}</color>", KTGlobal.GetDisplayNumber(taskDataXML.Experienceaward)));
                }
                if (taskDataXML.Bac > 0)
                {
                    awardMoneyAndExpString.Add(string.Format("Bạc: <color=#faed00>{0}</color>", KTGlobal.GetDisplayMoney(taskDataXML.Bac)));
                }
                if (taskDataXML.BacKhoa > 0)
                {
                    awardMoneyAndExpString.Add(string.Format("Bạc khóa: <color=#faed00>{0}</color>", KTGlobal.GetDisplayMoney(taskDataXML.BacKhoa)));
                }
                if (taskDataXML.DongKhoa > 0)
                {
                    awardMoneyAndExpString.Add(string.Format("KNB khóa: <color=#faed00>{0}</color>", KTGlobal.GetDisplayMoney(taskDataXML.DongKhoa)));
                }
                this.UIText_TaskAward.text = string.Join(", ", awardMoneyAndExpString);

                /// Danh sách vật phẩm thưởng
                string itemsString = taskDataXML.Taskaward;
                if (!string.IsNullOrEmpty(itemsString))
                {
                    /// Duyệt danh sách vật phẩm tương ứng
                    foreach (string itemString in itemsString.Split('#'))
                    {
                        string[] itemStrs = itemString.Split(',');
                        try
                        {
                            int itemID = int.Parse(itemStrs[0]);
                            int quantity = int.Parse(itemStrs[1]);
                            /// Tìm vật phẩm tương ứng trong hệ thống
                            if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
                            {
                                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                                itemGD.Binding = 1;
                                itemGD.Strong = 100;
                                itemGD.GCount = quantity;
                                UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_TaskAwardItemPrefab);
                                uiItemBox.transform.SetParent(this.transformAwardItemList);
                                uiItemBox.gameObject.SetActive(true);
                                uiItemBox.Data = itemGD;
                                uiItemBox.Refresh();
                            }
                        }
                        catch (Exception) { }
                    }
                    /// Xây lại giao diện
                    this.ExecuteSkipFrames(1, () =>
                    {
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformAwardItemList);
                    });
                }
            }

            /// Số bang cống tiêu hao khi đổi nhiệm vụ
            this.UIText_ChangeQuestCostMoney.text = KTGlobal.GetDisplayMoney(Loader.Loader.GuildConfig.ChangeQuestCost);
            /// Số nhiệm vụ tối đa trong ngày'
            /// 
            if(this._Data!=null)
            {
                this.UIText_MaxQuestPerDay.text = this._Data.TaskCountInDay + "/" +  Loader.Loader.GuildConfig.MaxQuestPerDay.ToString();
            }
            else
            {
                this.UIText_MaxQuestPerDay.text =  "0/" + Loader.Loader.GuildConfig.MaxQuestPerDay.ToString();
            }
            
        }
        #endregion
    }
}
