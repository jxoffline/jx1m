using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main.TaskBox
{
    /// <summary>
    /// Danh mục nhiệm vụ
    /// </summary>
    public class UITaskBox_TaskCategory : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên danh mục
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;

        /// <summary>
        /// Toggle danh mục
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

        /// <summary>
        /// Image nhiệm vụ chính tuyến
        /// </summary>
        [SerializeField]
        private RectTransform UIImage_MainTask;

        /// <summary>
        /// Image nhiệm vụ phụ tuyến
        /// </summary>
        [SerializeField]
        private RectTransform UIImage_SubTask;
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách nhiệm vụ
        /// </summary>
        public Dictionary<int, UITaskBox_Task> uiTasks = new Dictionary<int, UITaskBox_Task>();
        #endregion

        #region Properties
        /// <summary>
        /// ID danh mục
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string Name
        {
            get
            {
                return this.UIText_Title.text;
            }
            set
            {
                this.UIText_Title.text = value;
            }
        }

        /// <summary>
        /// Prefab Toggle nhiệm vụ
        /// </summary>
        public UITaskBox_Task UITask_Prefab { get; set; }

        /// <summary>
        /// Sự kiện khi nhiệm vụ thuộc danh mục được chọn
        /// </summary>
        public Action<UITaskBox_Task> TaskSelected { get; set; }

        /// <summary>
        /// Kích hoạt đối tượng
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIToggle.Active;
            }
            set
            {
                this.UIToggle.Active = value;
            }
        }

        /// <summary>
        /// Có phải nhánh nhiệm vụ chính tuyến không
        /// </summary>
        public bool IsMainTask
        {
            get
            {
                return this.UIImage_MainTask.gameObject.activeSelf;
            }
            set
            {
                this.UIImage_MainTask.gameObject.SetActive(value);
                this.UIImage_SubTask.gameObject.SetActive(!value);
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            
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
            this.UIToggle.OnSelected = this.Toggle_Selected;
        }

        /// <summary>
        /// Sự kiện khi Toggle được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void Toggle_Selected(bool isSelected)
        {
            foreach (UITaskBox_Task uiTask in this.uiTasks.Values)
            {
                uiTask.gameObject.SetActive(isSelected);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thêm nhiệm vụ mới vào danh mục
        /// </summary>
        /// <param name="taskData"></param>
        private UITaskBox_Task DoAddTask(TaskDataXML taskData)
        {
            UITaskBox_Task uiTask = GameObject.Instantiate<UITaskBox_Task>(this.UITask_Prefab);
            this.uiTasks[taskData.ID] = uiTask;
            uiTask.transform.SetParent(this.transform.parent, false);
            uiTask.gameObject.SetActive(true);
            uiTask.Data = taskData;
            uiTask.Active = false;
            uiTask.Completed = KTGlobal.HadTaskBeenCompleted(taskData.ID);
            uiTask.Selected = () => {
                this.TaskSelected?.Invoke(uiTask);
            };
            return uiTask;
        }

        /// <summary>
        /// Trả về UI nhiệm vụ theo ID
        /// </summary>
        /// <param name="taskID"></param>
        private UITaskBox_Task GetUITaskByTaskDbID(int taskID)
        {
            if (this.uiTasks.TryGetValue(taskID, out UITaskBox_Task uiTask))
            {
                return uiTask;
            }
            return null;
        }

        /// <summary>
        /// Xóa nút nhiệm vụ tương ứng
        /// </summary>
        /// <param name="uiTask"></param>
        private void RemoveUITask(UITaskBox_Task uiTask)
        {
            GameObject.Destroy(uiTask.gameObject);
            this.uiTasks.Remove(uiTask.Data.ID);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm nhiệm vụ mới vào danh mục
        /// </summary>
        /// <param name="taskData"></param>
        public UITaskBox_Task AddTask(TaskDataXML taskData)
        {
            UITaskBox_Task uiTask = this.GetUITaskByTaskDbID(taskData.ID);
            if (uiTask != null)
            {
                return uiTask;
            }
            uiTask = this.DoAddTask(taskData);
            return uiTask;
        }

        /// <summary>
        /// Cập nhật thông tin nhiệm vụ
        /// </summary>
        /// <param name="taskData"></param>
        /// <param name="isEnable"></param>
        public void UpdateTaskState(TaskDataXML taskData)
        {
            UITaskBox_Task uiTask = this.GetUITaskByTaskDbID(taskData.ID);
            if (uiTask != null)
            {
                uiTask.Completed = KTGlobal.HadTaskBeenCompleted(taskData.ID);
            }
        }
        #endregion
    }
}
