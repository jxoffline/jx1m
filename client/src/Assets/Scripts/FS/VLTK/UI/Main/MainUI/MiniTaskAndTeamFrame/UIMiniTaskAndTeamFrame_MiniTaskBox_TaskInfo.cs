using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using System.Collections;
using Server.Data;
using FS.GameEngine.Logic;
using FS.VLTK.Control.Component;
using FS.VLTK.Logic;

namespace FS.VLTK.UI.Main.MainUI.MiniTaskAndTeamFrame
{
    /// <summary>
    /// Thông tin nhiệm vụ trong khung MiniTaskBox
    /// </summary>
    public class UIMiniTaskAndTeamFrame_MiniTaskBox_TaskInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Text thông tin nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text chi tiết nhiệm vụ
        /// </summary>
        [SerializeField]
        private UIRichText UIText_Detail;

        /// <summary>
        /// Text tiến độ nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Progress;

        /// <summary>
        /// Image đánh dấu đã hoàn thành nhiệm vụ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_CompletedMark;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform đối tượng
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// RectTransform tên nhiệm vụ
        /// </summary>
        private RectTransform rectTransformTaskName;

        /// <summary>
        /// RectTransform tên nhiệm vụ
        /// </summary>
        private RectTransform rectTransformTaskNameBox;

        /// <summary>
        /// RectTransform tiêu đề nhiệm vụ
        /// </summary>
        private RectTransform rectTransformTaskTitle;
        #endregion

        #region Properties
        private TaskData _Data;
        /// <summary>
        /// Thông tin nhiệm vụ
        /// </summary>
        public TaskData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Đối tượng cấu hình nhiệm vụ tương ứng
                if (Loader.Loader.Tasks.TryGetValue(value.DoingTaskID, out Entities.Config.TaskDataXML taskData))
                {
                    /// Nếu đây là nhiệm vụ sắp nhận được
                    if (this.IsNextTask)
                    {
                        this.UIText_Name.text = string.Format("<color=#ed61ff>Tiếp theo:</color> {0} {1}", KTGlobal.GetTaskPrefix(value.DoingTaskID), taskData.Title);
                        this.UIText_Detail.Text = KTGlobal.GetNextTasDetailString(taskData.TaskClass, out Action clickEvent);
                        this.UIText_Progress.text = "";
                        this.UIImage_CompletedMark.gameObject.SetActive(false);
                        this.UIButton.onClick.RemoveAllListeners();
                        if (clickEvent != null)
                        {
                            this.UIButton.onClick.AddListener(() => {
                                /// Ngừng thực thi tự đánh
                                KTAutoFightManager.Instance.StopAutoFight();
                                clickEvent?.Invoke();
                            });
                        }
                    }
                    /// Nếu đây là nhiệm vụ đang theo dõi
                    else
                    {
                        this.UIText_Name.text = string.Format("{0} {1}", KTGlobal.GetTaskPrefix(value.DoingTaskID), taskData.Title);
                        this.UIText_Detail.Text = KTGlobal.GetTaskDetailString(value, out Action clickEvent);
                        if (taskData.TargetType == (int) TaskTypes.Talk)
                        {
                            this.UIText_Progress.text = string.Format("<color=#fdc091>Tiến độ:</color> {0}/{1}", 1, 1);
                            this.UIImage_CompletedMark.gameObject.SetActive(true);
                        }
                        else
                        {
                            this.UIText_Progress.text = string.Format("<color=#fdc091>Tiến độ:</color> {0}/{1}", value.DoingTaskVal1, taskData.TargetNum);
                            this.UIImage_CompletedMark.gameObject.SetActive(KTGlobal.IsQuestCompleted(value));
                        }
                        this.UIButton.onClick.RemoveAllListeners();
                        if (clickEvent != null)
                        {
                            this.UIButton.onClick.AddListener(() => {
                                /// Ngừng thực thi tự đánh
                                KTAutoFightManager.Instance.StopAutoFight();
                                clickEvent?.Invoke();
                            });
                        }
                    }
                }
                else
                {
                    this.UIText_Name.text = "Unknow Task";
                    this.UIText_Detail.Text = "Task data not found ID = " + value.DoingTaskID;
                    this.UIText_Progress.text = "";
                    this.UIText_Detail.ClickEvents = null;
                    this.UIImage_CompletedMark.gameObject.SetActive(false);
                    this.UIButton.onClick.RemoveAllListeners();
                }
            }
        }

        /// <summary>
        /// Có phải nhiệm vụ kế tiếp không
        /// </summary>
        public bool IsNextTask
        {
            get
            {
                if (this._Data == null)
                {
                    return false;
                }
                return this._Data.DbID == -1;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
            this.rectTransformTaskName = this.UIText_Name.GetComponent<RectTransform>();
            this.rectTransformTaskNameBox = this.UIText_Name.transform.parent.GetComponent<RectTransform>();
            this.rectTransformTaskTitle = this.UIText_Name.transform.parent.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
		private void OnEnable()
		{
            this.Refresh();
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
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
			{
                return;
			}

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransformTaskName);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransformTaskNameBox);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransformTaskTitle);
            }));
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        public void Refresh()
        {
            this.RebuildLayout();
        }
        #endregion
    }
}
