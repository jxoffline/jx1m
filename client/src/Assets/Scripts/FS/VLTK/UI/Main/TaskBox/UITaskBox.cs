using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.UI.Main.TaskBox;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung nhiệm vụ
    /// </summary>
    public class UITaskBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Button bỏ nhiệm vụ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Abandon;

        /// <summary>
        /// Prefab loại nhóm nhiệm vụ
        /// </summary>
        [SerializeField]
        private UITaskBox_TaskCategory UIToggle_CategoryPrefab;

        /// <summary>
        /// Prefab nhiệm vụ
        /// </summary>
        [SerializeField]
        private UITaskBox_Task UIToggle_TaskPrefab;

        /// <summary>
        /// Text tên nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TaskName;

        /// <summary>
        /// Text yêu cầu nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Requiration;

        /// <summary>
        /// Ký hiệu hoàn tất nhiệm vụ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_QuestCompletedMark;

        /// <summary>
        /// Text mô tả nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TaskDescription;

        /// <summary>
        /// Text chi tiết nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TaskDetail;

        /// <summary>
        /// Button thực thi nhiệm vụ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_DoTask;

        /// <summary>
        /// Text phần thưởng tiền và kinh nghiệm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AwardMoneyAndExp;

        /// <summary>
        /// Prefab ô phần thưởng
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_AwardPrefab;

        /// <summary>
        /// Text tiến độ nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TaskProgress;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách Tab bên trái
        /// </summary>
        private RectTransform transformLeftLayout;

        /// <summary>
        /// RectTransform danh sách phần thưởng
        /// </summary>
        private RectTransform transformAwardItemLayout;

        /// <summary>
        /// Danh sách danh mục nhiệm vụ
        /// </summary>
        private readonly Dictionary<int, UITaskBox_TaskCategory> categories = new Dictionary<int, UITaskBox_TaskCategory>();

        /// <summary>
        /// Nhiệm vụ đang được chọn
        /// </summary>
        private TaskData selectedTask;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung nhiệm vụ
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện bỏ nhiệm vụ
        /// </summary>
        public Action<TaskData> AbandonTask { get; set; }

        /// <summary>
        /// Sự kiện theo dõi nhiệm vụ
        /// </summary>
        public Action<TaskData> TrackTask { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformLeftLayout = this.UIToggle_CategoryPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
            this.transformAwardItemLayout = this.UIItem_AwardPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.BuildCategories();
            this.InitBaseData();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Abandon.onClick.AddListener(this.ButtonAbandonTask_Clicked);
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Toggle nhiệm vụ được ấn
        /// </summary>
        /// <param name="uiTask"></param>
        private void ToggleTask_Clicked(UITaskBox_Task uiTask)
        {
            /// Làm rỗng danh sách quà thưởng
            this.ClearItemAwardList();

            this.UIText_TaskName.text = uiTask.Data.Title;
            List<int> requirationLevel = new List<int>();
            requirationLevel.Add(uiTask.Data.MinLevel);
            if (uiTask.Data.MaxLevel != -1)
            {
                requirationLevel.Add(uiTask.Data.MaxLevel);
            }
            bool isAbleToTakeQuest = uiTask.Data.MinLevel <= Global.Data.RoleData.Level && (uiTask.Data.MaxLevel == -1 || uiTask.Data.MaxLevel >= Global.Data.RoleData.Level);
            this.UIText_Requiration.text = string.Format("<color={0}>{1}</color>", !isAbleToTakeQuest ? "#ff0f0f" : "#93ff0f", string.Join(" - ", requirationLevel));


            /// Mô tả nhiệm vụ
            this.UIText_TaskDescription.text = uiTask.Data.DoingTalk;

            /// Thông tin nhiệm vụ
            TaskData taskData = KTGlobal.GetTaskByStaticID(uiTask.Data.ID);
            /// Biểu tượng hoàn thành
            this.UIImage_QuestCompletedMark.gameObject.SetActive(uiTask.Completed);
            /// Chi tiết nhiệm vụ
            this.UIText_TaskDetail.text = KTGlobal.GetTaskDetailString(taskData, uiTask.Data, uiTask.Completed, out Action clickEvent);
            /// Xóa và thêm sự kiện
            this.UIButton_DoTask.onClick.RemoveAllListeners();
            this.UIButton_DoTask.onClick.AddListener(() => {
                clickEvent?.Invoke();
            });
            List<string> awardMoneyAndExpString = new List<string>();
            if (uiTask.Data.Experienceaward > 0)
            {
                awardMoneyAndExpString.Add(string.Format("Kinh nghiệm: <color=#faed00>{0}</color>", KTGlobal.GetDisplayNumber(uiTask.Data.Experienceaward)));
            }
            if (uiTask.Data.Bac > 0)
            {
                awardMoneyAndExpString.Add(string.Format("Bạc: <color=#faed00>{0}</color>", KTGlobal.GetDisplayMoney(uiTask.Data.Bac)));
            }
            if (uiTask.Data.BacKhoa > 0)
            {
                awardMoneyAndExpString.Add(string.Format("Bạc khóa: <color=#faed00>{0}</color>", KTGlobal.GetDisplayMoney(uiTask.Data.BacKhoa)));
            }
            if (uiTask.Data.DongKhoa > 0)
            {
                awardMoneyAndExpString.Add(string.Format("KNB khóa: <color=#faed00>{0}</color>", KTGlobal.GetDisplayMoney(uiTask.Data.DongKhoa)));
            }
            this.UIText_AwardMoneyAndExp.text = string.Join(", ", awardMoneyAndExpString);

            /// Danh sách vật phẩm thưởng
            string itemsString = uiTask.Data.Taskaward;
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
                            this.AddAwardItem(itemGD);
                        }
                    }
                    catch (Exception) { }
                }
                this.RebuildAwardItemLayout();
            }

            if (!uiTask.Completed && taskData != null)
            {
                this.UIButton_Abandon.interactable = true;
                this.selectedTask = taskData;

                if (uiTask.Data.TargetType == (int) TaskTypes.Talk)
                {
                    this.UIText_TaskProgress.text = string.Format("<color=#fdc091>Tiến độ:</color> {0}/{1}", 1, 1);
                }
                else
                {
                    this.UIText_TaskProgress.text = string.Format("<color=#fdc091>Tiến độ:</color> {0}/{1}", taskData.DoingTaskVal1, uiTask.Data.TargetNum);
                }
            }
            else
            {
                this.UIButton_Abandon.interactable = false;
                this.selectedTask = null;
                this.UIText_TaskProgress.text = "";
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
        /// Sự kiện khi Button bỏ nhiệm vụ được ấn
        /// </summary>
        private void ButtonAbandonTask_Clicked()
        {
            if (this.selectedTask == null)
            {
                return;
            }
            this.AbandonTask?.Invoke(this.selectedTask);
        }

        /// <summary>
        /// Sự kiện khi Button theo dõi nhiệm vụ được ấn
        /// </summary>
        private void ButtonTrackTask_Clicked()
        {
            if (this.selectedTask == null)
            {
                return;
            }
            this.TrackTask?.Invoke(this.selectedTask);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrame(int skip, Action callback)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callback?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện bên trái
        /// </summary>
        private void RebuildLeftLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrame(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformLeftLayout);
            }));
        }

        /// <summary>
        /// Xây lại giao diện các ô vật phẩm thưởng
        /// </summary>
        private void RebuildAwardItemLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrame(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformAwardItemLayout);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách nhóm và toggle nhiệm vụ
        /// </summary>
        private void ClearCategoryAndTaskToggleList()
        {
            foreach (RectTransform child in this.transformLeftLayout.transform)
            {
                if (child.gameObject != this.UIToggle_CategoryPrefab.gameObject && child.gameObject != this.UIToggle_TaskPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildLeftLayout();
        }

        /// <summary>
        /// Làm rỗng danh sách vật phẩm thưởng
        /// </summary>
        private void ClearItemAwardList()
        {
            foreach (RectTransform child in this.transformAwardItemLayout.transform)
            {
                if (child.gameObject != this.UIItem_AwardPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildAwardItemLayout();
        }

        /// <summary>
        /// Thêm vật phẩm vào danh sách thưởng
        /// </summary>
        /// <param name="itemGD"></param>
        private void AddAwardItem(GoodsData itemGD)
        {
            UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UIItem_AwardPrefab);
            uiItemBox.transform.SetParent(this.transformAwardItemLayout);
            uiItemBox.gameObject.SetActive(true);
            uiItemBox.Data = itemGD;
        }

        /// <summary>
        /// Thêm nhóm nhiệm vụ tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="isSelected"></param>
        /// <param name="isMainTask"></param>
        private UITaskBox_TaskCategory AddCategory(int id, string name, bool isSelected, bool isMainTask)
        {
            UITaskBox_TaskCategory uiCategory = GameObject.Instantiate<UITaskBox_TaskCategory>(this.UIToggle_CategoryPrefab);
            this.categories[id] = uiCategory;
            uiCategory.transform.SetParent(this.transformLeftLayout, false);
            uiCategory.gameObject.SetActive(true);
            uiCategory.IsMainTask = isMainTask;
            uiCategory.ID = id;
            uiCategory.Name = name;
            uiCategory.UITask_Prefab = this.UIToggle_TaskPrefab;
            uiCategory.TaskSelected = this.ToggleTask_Clicked;
            uiCategory.Active = isSelected;
            return uiCategory;
        }

        /// <summary>
        /// Xây dựng cây nhiệm vụ theo danh mục
        /// </summary>
        /// <param name="categoryID"></param>
        /// <param name="isSelectFirst"></param>
        private void BuildTaskTree(int categoryID, bool isSelectFirst = false)
        {
            /// Thông tin nhiệm vụ gốc
            TaskDataXML rootNode = Loader.Loader.Tasks.Values.Where(x => x.TaskClass == categoryID && x.PrevTask == -1).FirstOrDefault();
            if (rootNode == null)
            {
                return;
            }

            HashSet<int> markup = new HashSet<int>();
            TaskDataXML node = rootNode;
            while (node != null)
            {
                if (markup.Contains(node.ID))
                {
                    KTDebug.LogError("Infinity LOOP => TaskID = " + node.ID);
                    break;
                }

                markup.Add(node.ID);
                UITaskBox_Task uiTask = this.categories[categoryID].AddTask(node);
                if (isSelectFirst)
                {
                    this.StartCoroutine(this.ExecuteSkipFrame(5, () => {
                        uiTask.Active = true;
                        this.ToggleTask_Clicked(uiTask);
                    }));
                    isSelectFirst = false;
                }
                int nextTaskID = node.NextTask;
                if (!Loader.Loader.Tasks.TryGetValue(nextTaskID, out node))
                {
                    node = null;
                }
            }

            this.RebuildLeftLayout();
        }

        /// <summary>
        /// Xây nhóm nhiệm vụ
        /// </summary>
        private void BuildCategories()
        {
            this.ClearCategoryAndTaskToggleList();

            this.AddCategory(0, "Chính tuyến", true, true);
            this.BuildTaskTree(0, true);
            this.AddCategory(5, "Phụ tuyến", false, false);
            this.BuildTaskTree(5, false);
        }

        /// <summary>
        /// Xây dữ liệu dựa vào Data
        /// </summary>
        private void InitBaseData()
        {
            if (Global.Data.RoleData.TaskDataList != null)
            {
                foreach (TaskData taskData in Global.Data.RoleData.TaskDataList)
                {
                    this.AddTask(taskData);
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm nhiệm vụ tương ứng
        /// </summary>
        /// <param name="task"></param>
        public void AddTask(TaskData task)
        {
            if (Loader.Loader.Tasks.TryGetValue(task.DoingTaskID, out TaskDataXML taskDataXML))
            {
                this.categories[taskDataXML.TaskClass].UpdateTaskState(taskDataXML);
            }
            else
            {
                return;
            }

            HashSet<int> markup = new HashSet<int>();
            /// Nút nhiệm vụ hiện tại
            TaskDataXML node = taskDataXML;
            /// Chừng nào còn nút liền trước
            while (node != null)
            {
                if (markup.Contains(node.ID))
                {
                    KTDebug.LogError("Infinity LOOP => TaskID = " + node.ID);
                    break;
                }

                markup.Add(node.ID);

                /// Nhiệm vụ trước đó
                int prevTaskID = node.PrevTask;
                if (!Loader.Loader.Tasks.TryGetValue(prevTaskID, out node))
                {
                    node = null;
                }
                else
                {
                    this.categories[taskDataXML.TaskClass].UpdateTaskState(node);
                }
            }
        }

        /// <summary>
        /// Hoàn thành nhiệm vụ tương ứng
        /// </summary>
        /// <param name="task"></param>
        public void CompleteTask(TaskData task)
        {
            if (Loader.Loader.Tasks.TryGetValue(task.DoingTaskID, out TaskDataXML taskDataXML))
            {
                this.categories[taskDataXML.TaskClass].UpdateTaskState(taskDataXML);

                if (task == this.selectedTask)
                {
                    this.UIImage_QuestCompletedMark.gameObject.SetActive(true);
                    this.UIButton_Abandon.interactable = false;
                }
            }
        }

        /// <summary>
        /// Cập nhật thông tin nhiệm vụ
        /// </summary>
        /// <param name="taskData"></param>
        public void UpdateTask(TaskData taskData)
        {
            if (this.selectedTask != taskData)
            {
                return;
            }

            if (!Loader.Loader.Tasks.TryGetValue(taskData.DoingTaskID, out TaskDataXML taskDataXML))
            {
                return;
            }

            if (taskDataXML.TargetType == (int) TaskTypes.Talk)
            {
                this.UIText_TaskProgress.text = string.Format("<color=#fdc091>Tiến độ:</color> {0}/{1}", 1, 1);
            }
            else
            {
                this.UIText_TaskProgress.text = string.Format("<color=#fdc091>Tiến độ:</color> {0}/{1}", taskData.DoingTaskVal1, taskDataXML.TargetNum);
            }
        }

        /// <summary>
        /// Hủy nhiệm vụ tương ứng
        /// </summary>
        /// <param name="taskData"></param>
        public void DoAbandonTask(TaskData taskData)
        {
            if (this.selectedTask != taskData)
            {
                return;
            }

            this.UIButton_Abandon.interactable = false;
            this.UIText_TaskProgress.text = "";
        }
        #endregion
    }
}
