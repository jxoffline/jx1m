using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using Server.Data;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.MiniTaskAndTeamFrame
{
    /// <summary>
    /// Khung MiniTaskBox trong nhóm MiniTask và MiniTeamFrame
    /// </summary>
    public class UIMiniTaskAndTeamFrame_MiniTaskBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab thông tin nhiệm vụ
        /// </summary>
        [SerializeField]
        private UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo UI_TaskInfoPrefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform đối tượng
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Properties

        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.UI_TaskInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.ClearList();
            this.InitPrefabs();
            /// Làm mới
            this.RefreshTasks();
            /// Tự thêm nhiệm vụ chính tuyến tiếp theo nếu không có nhiệm vụ chính tuyến nào đang làm
            this.AutoInsertNextMainQuestInfo();
            /// Xây lại giao diện
            this.RebuildLayout();
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm nàyg gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }
            /// Tự thêm nhiệm vụ chính tuyến tiếp theo nếu không có nhiệm vụ chính tuyến nào đang làm
            this.AutoInsertNextMainQuestInfo();
            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
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
        /// Thực thi sự kiện bỏ qua số lượng Frame tương ứng
        /// </summary>
        /// <param name="frames"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int frames, Action action)
        {
            for (int i = 1; i <= frames; i++)
            {
                yield return null;
            }
            action?.Invoke();
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
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
            }));
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void ClearList()
        {
            foreach (Transform child in this.rectTransform.transform)
            {
                if (child.gameObject != this.UI_TaskInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            //this.RebuildLayout();
        }

        /// <summary>
        /// Tìm UI chứa thông tin nhiệm vụ tương ứng
        /// </summary>
        /// <param name="taskData"></param>
        /// <returns></returns>
        private UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo FindTaskNode(TaskData taskData)
        {
            foreach (Transform child in this.rectTransform.transform)
            {
                if (child.gameObject != this.UI_TaskInfoPrefab.gameObject)
                {
                    UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo uiTaskInfo = child.gameObject.GetComponent<UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo>();
                    if (uiTaskInfo != null)
                    {
                        if (uiTaskInfo.Data.DbID == taskData.DbID)
                        {
                            return uiTaskInfo;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Thêm UI chứa thông tin nhiệm vụ tương ứng
        /// </summary>
        /// <param name="taskData"></param>
        /// <returns></returns>
        private UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo AddTaskNode(TaskData taskData)
        {
            UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo uiTaskData = GameObject.Instantiate<UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo>(this.UI_TaskInfoPrefab);
            uiTaskData.transform.SetParent(this.rectTransform, false);
            uiTaskData.gameObject.SetActive(true);
            uiTaskData.Data = taskData;
            uiTaskData.transform.SetAsFirstSibling();
            return uiTaskData;
        }

        /// <summary>
        /// Xóa UI thông tin nhiệm vụ tương ứng
        /// </summary>
        /// <param name="uiTaskData"></param>
        private bool RemoveTaskNode(TaskData taskData)
        {
            /// UI nhiệm vụ cũ
            UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo uiTaskData = this.FindTaskNode(taskData);
            /// Nếu tìm thấy
            if (uiTaskData != null)
            {
                GameObject.Destroy(uiTaskData.gameObject);
                this.RebuildLayout();
                return true;
            }
            /// Nếu không tìm thấy
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Tự động thêm nhiệm vụ chính tuyến tiếp theo lên đầu nếu danh sách rỗng
        /// </summary>
        private void AutoInsertNextMainQuestInfo()
        {
            /// Duyệt danh sách xóa nhiệm vụ tiếp theo
            foreach (Transform child in this.rectTransform.transform)
            {
                if (child.gameObject != this.UI_TaskInfoPrefab.gameObject)
                {
                    UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo uiTaskInfo = child.gameObject.GetComponent<UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo>();
                    if (uiTaskInfo != null && uiTaskInfo.IsNextTask)
                    {
                        GameObject.Destroy(uiTaskInfo.gameObject);
                    }
                }
            }

            /// Nếu không tồn tại nhiệm vụ chính tuyến nào đang làm
            if (Global.Data.RoleData.TaskDataList == null || !Global.Data.RoleData.TaskDataList.Any((taskData) => {
                if (Loader.Loader.Tasks.TryGetValue(taskData.DoingTaskID, out TaskDataXML taskDataXML))
                {
                    return taskDataXML.TaskClass == 0;
                }
                return false;
            }))
            {
                /// Nhiệm vụ tiếp theo
                TaskDataXML nextTaskXML = KTGlobal.GetNextTask(0);
                /// Nếu tồn tại nhiệm vụ tiếp theo
                if (nextTaskXML != null)
                {
                    /// Tạo mới thông tin nhiệm vụ
                    TaskData taskData = new TaskData()
                    {
                        DbID = -1,
                        DoingTaskID = nextTaskXML.ID,
                    };
                    UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo uiTaskInfo = this.AddTaskNode(taskData);
                    uiTaskInfo.transform.SetAsFirstSibling();
                    this.RebuildLayout();
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm nhiệm vụ mới vào danh sách
        /// </summary>
        /// <param name="taskData"></param>
        public void AddNewTask(TaskData taskData)
        {
            /// UI nhiệm vụ cũ
            UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo uiTaskData = this.FindTaskNode(taskData);
            /// Nếu không tồn tại nhiệm UI nhiệm vụ cũ
            if (uiTaskData == null)
            {
                this.AddTaskNode(taskData);
            }

            /// Tự thêm nhiệm vụ chính tuyến tiếp theo nếu không có nhiệm vụ chính tuyến nào đang làm
            this.AutoInsertNextMainQuestInfo();
        }

        /// <summary>
        /// Cập nhật thông tin nhiệm vụ tương ứng
        /// </summary>
        /// <param name="taskData"></param>
        public void UpdateTask(TaskData taskData)
        {
            /// UI nhiệm vụ cũ
            UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo uiTaskData = this.FindTaskNode(taskData);
            /// Nếu tồn tại nhiệm UI nhiệm vụ cũ
            if (uiTaskData != null)
            {
                uiTaskData.Data = taskData;
                uiTaskData.Refresh();
            }

            /// Tự thêm nhiệm vụ chính tuyến tiếp theo nếu không có nhiệm vụ chính tuyến nào đang làm
            this.AutoInsertNextMainQuestInfo();
        }

        /// <summary>
        /// Xóa nhiệm vụ tương ứng khỏi danh sách
        /// </summary>
        /// <param name="taskData"></param>
        public void RemoveTask(TaskData taskData)
        {
            this.RemoveTaskNode(taskData);

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                /// Tự thêm nhiệm vụ chính tuyến tiếp theo nếu không có nhiệm vụ chính tuyến nào đang làm
                this.AutoInsertNextMainQuestInfo();
            }));
            
        }

        /// <summary>
        /// Làm mới danh sách nhiệm vụ
        /// </summary>
        public void RefreshTasks()
        {
            this.ClearList();
            if (Global.Data.RoleData.TaskDataList != null)
            {
                foreach (TaskData task in Global.Data.RoleData.TaskDataList)
                {
                    this.AddTaskNode(task);
                }
            }
            /// Xây lại giao diện
            this.RebuildLayout();

            /// Tự thêm nhiệm vụ chính tuyến tiếp theo nếu không có nhiệm vụ chính tuyến nào đang làm
            this.AutoInsertNextMainQuestInfo();
        }
        #endregion
    }
}