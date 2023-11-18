using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using System;

namespace FS.VLTK.UI.Main.MainUI.RolePart
{
    /// <summary>
    /// Khung thông tin Buff
    /// </summary>
    public class UIRolePart_BuffInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Buff Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Buff;

        /// <summary>
        /// Icon buff
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_BuffIcon;

        /// <summary>
        /// Text thời gian
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TimerText;
        #endregion

        #region Properties
        /// <summary>
        /// Tên Bundle chứa Icon
        /// </summary>
        public string IconBundleDir
        {
            get
            {
                return this.UIImage_BuffIcon.BundleDir;
            }
            set
            {
                this.UIImage_BuffIcon.BundleDir = value;
            }
        }

        /// <summary>
        /// Tên Atlas chứa Icon
        /// </summary>
        public string IconAtlasName
        {
            get
            {
                return this.UIImage_BuffIcon.AtlasName;
            }
            set
            {
                this.UIImage_BuffIcon.AtlasName = value;
            }
        }

        /// <summary>
        /// Tên Sprite Icon
        /// </summary>
        public string IconSpriteName
        {
            get
            {
                return this.UIImage_BuffIcon.SpriteName;
            }
            set
            {
                this.UIImage_BuffIcon.SpriteName = value;
            }
        }

        /// <summary>
        /// Thời gian tồn tại tính theo giây, -1 nếu tồn tại vĩnh viễn
        /// </summary>
        public long DurationSecond { get; set; }

        /// <summary>
        /// Có hiển thị Timer không
        /// </summary>
        public bool ShowTimer
        {
            get
            {
                return this.UIText_TimerText.gameObject.activeSelf;
            }
            set
            {
                this.UIText_TimerText.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi ấn vào Icon
        /// </summary>
        public Action IconClick { get; set; }

        /// <summary>
        /// Sự kiện khi Buff hết thời gian
        /// </summary>
        public Action TimeOut { get; set; }

        /// <summary>
        /// Thời gian tồn tại (nếu Buff có thời hạn)
        /// </summary>
        public long LifeTime
        {
            get
            {
                return this.lifeTime;
            }
        }
        #endregion

        /// <summary>
        /// Luồng thực thi đếm ngược thời gian
        /// </summary>
        private Coroutine timerRoutine;

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        private long lifeTime;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
            this.RefreshTimer();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Buff.onClick.AddListener(this.ButtonIcon_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Icon được ấn
        /// </summary>
        private void ButtonIcon_Clicked()
        {
            this.IconClick?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng đếm ngược thời gian
        /// </summary>
        /// <returns></returns>
        private IEnumerator CountDown()
        {
            this.lifeTime = 0;
            while (this.lifeTime < this.DurationSecond)
            {
                this.lifeTime++;
                this.UpdateTimerDisplayText();
                yield return new WaitForSeconds(1);
            }
            this.TimeOut?.Invoke();
        }

        /// <summary>
        /// Hiển thị thời gian
        /// </summary>
        private void UpdateTimerDisplayText()
        {
            /// Nếu đánh dấu không hiện CountDown
            if (!this.ShowTimer)
            {
                this.UIText_TimerText.gameObject.SetActive(false);
            }
            else if (this.DurationSecond == -1 && this.UIText_TimerText.gameObject.activeSelf)
            {
                this.UIText_TimerText.gameObject.SetActive(false);
            }
            else if (!this.UIText_TimerText.gameObject.activeSelf)
            {
                this.UIText_TimerText.gameObject.SetActive(true);
            }

            long timeLeft = this.DurationSecond - this.lifeTime;

            if (timeLeft >= 0)
            {
                this.UIText_TimerText.text = KTGlobal.DisplayTime(timeLeft);
            }
            else
            {
                this.UIText_TimerText.text = "";
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới thời gian
        /// </summary>
        public void RefreshTimer()
        {
            if (this.timerRoutine != null)
            {
                this.StopCoroutine(this.timerRoutine);
                this.timerRoutine = null;
            }
            if (this.DurationSecond != -1)
            {
                this.timerRoutine = this.StartCoroutine(this.CountDown());
            }
        }

        /// <summary>
        /// Làm mới đối tượng
        /// </summary>
        public void Refresh()
        {
            this.UIImage_BuffIcon.Load();
        }
        #endregion
    }
}
