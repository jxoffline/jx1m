using System;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityThreading
{
    /// <summary>
    /// Đối tượng quản lý Background Worker
    /// </summary>
    public class UnityBackgroundWorkerManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Background Worker
        /// </summary>
        public static UnityBackgroundWorkerManager Instance { get; private set; }
        #endregion

        #region Define
#if UNITY_EDITOR
        /// <summary>
        /// Thống kê tổng số Worker đang thực thi
        /// <para>Chỉ hoạt động với Editor</para>
        /// </summary>
        [SerializeField]
        private int _WorkersCount;
#endif

        /// <summary>
        /// Pool chứa danh sách công việc với ID Worker tương ứng
        /// </summary>
        private static readonly Dictionary<int, Action> worksPool = new Dictionary<int, Action>();
        /// <summary>
        /// Worker thực thi công việc tương ứng
        /// </summary>
        public class UnityBackgroundWorker
        {
            /// <summary>
            /// Công việc cần thực thi
            /// </summary>
            private struct BackgroundWork : IJob
            {
                /// <summary>
                /// ID công việc
                /// </summary>
                private readonly int id;

                /// <summary>
                /// Công việc cần thực thi
                /// </summary>
                /// <param name="id"></param>
                public BackgroundWork(int id)
                {
                    this.id = id;
                }

                /// <summary>
                /// Thực thi công việc
                /// </summary>
                public void Execute()
                {
                    if (UnityBackgroundWorkerManager.worksPool.TryGetValue(id, out Action job))
                    {
                        try
                        {
                            job?.Invoke();
                        }
                        catch (Exception) { }
                    }
                    else
                    {
                        //Debug.LogError("Job for Worker ID = " + id + " not FOUND!");
                    }
                }
            }

            #region Private fields
            /// <summary>
            /// ID tự tăng
            /// </summary>
            private static int AutoID = 0;

            /// <summary>
            /// Đối tượng Handle công việc
            /// </summary>
            protected JobHandle? jobHandle = null;

            /// <summary>
            /// Hoàn tất công việc chưa
            /// </summary>
            protected bool completed = false;

            /// <summary>
            /// Đối tượng công việc đang thực thi
            /// </summary>
            private BackgroundWork work;
            #endregion

            #region Properties
            /// <summary>
            /// ID Worker
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// Công việc cần thực thi (chạy ở Thread phụ)
            /// </summary>
            public Action DoWork { get; set; }

            /// <summary>
            /// Sự kiện khi hoàn tất công việc (chạy ở Thread chính)
            /// </summary>
            public Action RunWorkerCompleted { get; set; }

            /// <summary>
            /// Sự kiện khi Worker báo cáo tiến độ (chạy ở Thread chính)
            /// <para>percentage: int => % tiến trình</para>
            /// </summary>
            public Action<int> ProgressChanged { get; set; }

            /// <summary>
            /// Đang bận thực thi công việc
            /// </summary>
            public bool IsBusy
            {
                get
                {
                    return !this.completed;
                }
            }

            /// <summary>
            /// Mới có báo cáo % tiến độ
            /// </summary>
            protected bool IsProgressReported { get; set; }

            /// <summary>
            /// % tiến độ báo cáo lần trước
            /// </summary>
            protected int ProgressPercentage { get; set; }

            /// <summary>
            /// Hàm CallBack trước khi bắt đầu công việc
            /// </summary>
            protected Action BeforeDoWorkCallBack { get; set; }
            #endregion

            #region Constructor
            /// <summary>
            /// Hàm khởi tạo đối tượng
            /// </summary>
            protected UnityBackgroundWorker()
            {
                UnityBackgroundWorker.AutoID++;
                this.ID = UnityBackgroundWorker.AutoID;
            }
            #endregion

            #region Public methods
            /// <summary>
            /// Thực hiện công việc theo phương thức Async
            /// </summary>
            public void RunWorkerAsync()
            {
                /// Thực hiện CallBack DoWork
                this.BeforeDoWorkCallBack?.Invoke();

                this.work = new BackgroundWork(this.ID);
                this.jobHandle = this.work.Schedule();
            }

            /// <summary>
            /// Thực hiện công việc, và tạm dừng Main Thread đến khi công việc được thực hiện xong
            /// </summary>
            public void RunWorker()
            {
                this.RunWorkerAsync();
                this.jobHandle.Value.Complete();
            }

            /// <summary>
            /// Báo cáo tiến độ
            /// </summary>
            /// <param name="percentage">% tiến độ</param>
            public void ReportProgress(int percentage)
            {
                this.ProgressPercentage = percentage;
                this.IsProgressReported = true;
            }
            #endregion
        }

        /// <summary>
        /// Worker thực thi công việc tương ứng
        /// </summary>
        private class UnityBackgroundWorkerImplements : UnityBackgroundWorker
        {
            #region Properties
            /// <summary>
            /// Đã hoàn tất công việc chưa
            /// </summary>
            public bool Completed
            {
                get
                {
                    return this.completed;
                }
                set
                {
                    this.completed = value;
                }
            }

            /// <summary>
            /// Đối tượng Handle công việc
            /// </summary>
            public JobHandle? JobHandle
            {
                get
                {
                    return this.jobHandle;
                }
            }

            /// <summary>
            /// Hàm CallBack gọi trước khi thực hiện công việc
            /// </summary>
            public Action InnerBeforeDoWorkCallBack
            {
                get
                {
                    return this.BeforeDoWorkCallBack;
                }
                set
                {
                    this.BeforeDoWorkCallBack = value;
                }
            }

            /// <summary>
            /// Mới có báo cáo % tiến độ
            /// </summary>
            public bool InnerIsProgressReported
            {
                get
                {
                    return this.IsProgressReported;
                }
                set
                {
                    this.IsProgressReported = value;
                }
            }

            /// <summary>
            /// % tiến độ báo cáo lần trước
            /// </summary>
            public int InnerProgressPercentage
            {
                get
                {
                    return this.ProgressPercentage;
                }
                set
                {
                    this.ProgressPercentage = value;
                }
            }
            #endregion

            #region Constructor
            /// <summary>
            /// Hàm khởi tạo nội bộ đối tượng
            /// </summary>
            public UnityBackgroundWorkerImplements() : base()
            {

            }
            #endregion
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách Worker đang thực thi
        /// </summary>
        private readonly List<UnityBackgroundWorkerImplements> works = new List<UnityBackgroundWorkerImplements>();
        #endregion

        #region Properties
        /// <summary>
        /// Tổng số công việc đang thực thi
        /// </summary>
        public int WorkersCount
        {
            get
            {
                return this.works.Count;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            UnityBackgroundWorkerManager.Instance = this;
            /// Làm rỗng danh sách công việc
            UnityBackgroundWorkerManager.worksPool.Clear();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
#if UNITY_EDITOR
            this._WorkersCount = this.works.Count;
#endif

            /// Vị trí trong danh sách
            int i = 0;
            /// Duyệt toàn bộ các Worker trong danh sách
            while (i < this.works.Count)
            {
                /// Worker tại vị trí tương ứng
                UnityBackgroundWorkerImplements worker = this.works[i];
                /// Nếu chưa bắt đầu công việc
                if (worker.JobHandle != null)
                {
                    /// Nếu có báo cáo tiến độ
                    if (worker.InnerIsProgressReported)
                    {
                        worker.InnerIsProgressReported = false;
                        /// Thực thi sự kiện báo cáo tiến độ
                        worker.ProgressChanged?.Invoke(worker.InnerProgressPercentage);
                    }
                    /// Nếu công việc đã hoàn tất
                    if (worker.JobHandle.Value.IsCompleted)
                    {
                        worker.Completed = true;
                        worker.RunWorkerCompleted?.Invoke();
                        this.works.RemoveAt(i);

                        /// Xóa công việc khỏi Pool
                        UnityBackgroundWorkerManager.worksPool.Remove(worker.ID);
                    }
                }
                i++;
            }
        }

        /// <summary>
        /// Sự kiện khi thoát ứng dụng
        /// </summary>
        private void OnApplicationQuit()
        {
            /// Làm rỗng danh sách công việc
            UnityBackgroundWorkerManager.worksPool.Clear();
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        /// <summary>
        /// Tạo mới đối tượng Background Worker
        /// </summary>
        /// <returns></returns>
        public UnityBackgroundWorker NewBackgroundWorker()
        {
            /// Tạo mới đối tượng Inner Background Worker
            UnityBackgroundWorkerImplements worker = new UnityBackgroundWorkerImplements();
            /// Thiết lập hàm CallBack gọi trước khi thực hiện công việc
            worker.InnerBeforeDoWorkCallBack = () => {
                /// Thêm công việc vào Pool
                UnityBackgroundWorkerManager.worksPool[worker.ID] = worker.DoWork;
            };
            /// Thêm Worker vào danh sách quản lý
            this.works.Add(worker);
            /// Đánh dấu chưa hoàn tất công việc
            worker.Completed = false;
            /// Trả ra đối tượng Worker tương ứng
            return worker;
        }
        #endregion
    }
}
