using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using FS.GameFramework.Logic;
using FS.GameEngine.Logic;
using FS.VLTK.Loader;

namespace FS.VLTK.UI.LoadingGame
{
    /// <summary>
    /// Màn hình tải game hoặc dữ liệu
    /// </summary>
    public class UILoadingGame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text phần trăm tiến trình tải
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LoadingPercent;

        /// <summary>
        /// ProgressBar tiến trình tải
        /// </summary>
        [SerializeField]
        private Slider UISlider_LoadingProgressBar;

        /// <summary>
        /// Text phiên bản
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_VersionText;
        #endregion

        /// <summary>
        /// Bước tiếp theo
        /// </summary>
        public Action NextStep { get; set; }

        #region Core Monobehaviour
        /// <summary>
        /// Hàm này gọi đến tại frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefab();
            this.InitMainScene();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Thiết lập ban đầu
        /// </summary>
        private void InitPrefab()
        {
            this.UIText_LoadingPercent.text = "0%";
            this.UISlider_LoadingProgressBar.value = 0f;

            this.UIText_VersionText.text = string.Format("{0}", MainGame.Instance.Version);
        }

        /// <summary>
        /// Khởi tạo màn hình MainGame
        /// </summary>
        /// <returns></returns>
        private void InitMainScene()
        {
            GameObject go = new GameObject("LoadConfig");
            LoadConfig loadConfig = go.AddComponent<LoadConfig>();
            loadConfig.OnProgressBarReport = this.OnProgressBarReport;
            loadConfig.OnLoadFinish = this.OnLoadFinish;
        }

        /// <summary>
        /// Gọi từ ngoài, báo cáo tiến trình tải xuống
        /// </summary>
        /// <param name="percent"></param>
        public void OnProgressBarReport(int percent)
        {
            this.UIText_LoadingPercent.text = percent + "%";
            this.UISlider_LoadingProgressBar.value = percent / 100f;
        }

        /// <summary>
        /// Gọi đến khi tiến trình tải xuống hoàn tất
        /// </summary>
        private void OnLoadFinish()
        {
            IEnumerator RunNextSecond()
            {
                yield return new WaitForSeconds(1f);
                this.NextStep?.Invoke();
            }
            this.StartCoroutine(RunNextSecond());
        }
        #endregion
    }
}