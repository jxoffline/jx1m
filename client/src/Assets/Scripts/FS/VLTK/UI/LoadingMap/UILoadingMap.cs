using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using UnityEngine.Networking;
using System.Xml.Linq;
using FS.Drawing;
using FS.VLTK.Loader;
using FS.VLTK.Control.Component;

namespace FS.VLTK.UI.LoadingMap
{
    /// <summary>
    /// Khung tải bản đồ
    /// </summary>
    public class UILoadingMap : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text thông báo của Progress Bar
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ProgressBarText;

        /// <summary>
        /// Text phần trăm tiến trình
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ProgresBarPercentText;

        /// <summary>
        /// Progress Bar tiến trình
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_ProgressBar;

        /// <summary>
        /// Kích hoạt chế độ Debug
        /// </summary>
        [SerializeField]
        private bool EnableDebug = true;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện bắt đầu ngay khi tải xuống bản đồ
        /// </summary>
        public Action StartLoading { get; set; } = null;

        /// <summary>
        /// Sự kiện khi hoàn tất công việc
        /// </summary>
        public Action WorkFinished { get; set; } = null;

        /// <summary>
        /// Sự kiện khi đối tượng bị xóa
        /// </summary>
        public Action DoDestroy { get; set; }

        /// <summary>
        /// ID bản đồ cần tải xuống
        /// </summary>
        public int MapCode { get; set; } = 0;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            //this.WorkFinished?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị hủy
        /// </summary>
        private void OnDestroy()
        {
            this.DoDestroy?.Invoke();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Thiết lập ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIText_ProgresBarPercentText.text = "0%";
            this.UISlider_ProgressBar.value = 0;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực hiện tải tài nguyên 2D
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoLoad2DMapRes()
        {
            /// Xóa bản đồ cũ
            Super.DestroyCurrentMap();
            /// Bỏ qua 1 Frame
            yield return null;
            /// Tạo mới
            GameObject root2DScene = new GameObject("Scene 2D Root");
            root2DScene.transform.localPosition = Vector3.zero;

            GameObject go = new GameObject("Empty Scene");
            go.transform.SetParent(root2DScene.transform, false);
            go.transform.localPosition = Vector3.zero;
            Map map2D = go.AddComponent<Map>();
            Global.CurrentMap = map2D;
            map2D.MapCode = this.MapCode;
            map2D.LeaderPosition = new Vector2(Global.Data.RoleData.PosX, Global.Data.RoleData.PosY);
            map2D.ReportProgress = this.ReportProgressBar;
            map2D.UpdateProgressText = (text) =>
            {
                this.UIText_ProgressBarText.text = text;
            };
            map2D.Finish = () => {
                /// Thực thi sự kiện kết thúc
                this.WorkFinished?.Invoke();
            };
            map2D.Load();

            /// Bỏ qua 1 Frame
            yield return null;
            /// Thực thi sự kiện bắt đầu ngay khi tải xuống bản đồ
            this.StartLoading?.Invoke();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gọi đến báo cáo tiến trình tải xuống
        /// </summary>
        /// <param name="percent"></param>
        public void ReportProgressBar(int percent)
        {
            this.UIText_ProgresBarPercentText.text = string.Format("{0}%", percent);

            this.UISlider_ProgressBar.value = percent / (float)100f;
        }

        /// <summary>
        /// Bắt đầu tải xuống tài nguyên bản đồ
        /// </summary>
        public void BeginLoad2DMapRes()
        {
            this.StartCoroutine(this.DoLoad2DMapRes());
        }
        #endregion
    }
}