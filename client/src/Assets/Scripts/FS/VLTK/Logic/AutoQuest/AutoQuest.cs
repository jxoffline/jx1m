using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Auto tự làm nhiệm vụ
    /// </summary>
    public partial class AutoQuest : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Auto tự làm nhiệm vụ
        /// </summary>
        public static AutoQuest Instance { get; private set; }

        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            AutoQuest.Instance = this;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Nhiệm vụ đang thực thi Auto
        /// </summary>
        public TaskData Task { get; set; }
        /// <summary>
        /// Dữ liệu nhiệm vụ tương ứng
        /// </summary>
        public TaskDataXML TaskData
        {
            get
            {
                if (this.Task == null)
                {
                    return null;
                }

                if (Loader.Loader.Tasks.TryGetValue(this.Task.DoingTaskID, out TaskDataXML taskData))
                {
                    return taskData;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Sự kiện khi hoàn tất tự thực hiện nhiệm vụ
        /// </summary>
        public Action Done { get; set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực thi nhiệm vụ tự động
        /// </summary>
        public void StartAutoQuest()
        {
            this.StopAllCoroutines();
            if (this.Task == null || this.TaskData == null)
            {
                return;
            }

            switch (this.TaskData.TargetType)
            {
                case (int) TaskTypes.KillMonster:
                case (int) TaskTypes.MonsterSomething:
                {
                    this.StartCoroutine(this.StartAutoQuest_AttackMonster());
                    break;
                }
                case (int) TaskTypes.Collect:
                {
                    this.StartCoroutine(this.StartAutoQuest_CollectGrowPoint());
                    break;
                }
            }
        }

        /// <summary>
        /// Ngừng thực thi nhiệm vụ tự động
        /// </summary>
        public void StopAutoQuest()
        {
            this.StopAllCoroutines();
            this.Task = null;
            this.Done = null;
        }
        #endregion
    }
}
