using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;
using FS.VLTK.UI.Main.MainUI.EventBroadboardMini;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung bảng công cáo sự kiện, phụ bản
    /// </summary>
    public class UIEventBroadboardMini : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên sự kiện
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_EventName;

        /// <summary>
        /// Text mô tả sự kiện
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_EventShortDetail;

        /// <summary>
        /// Prefab dòng nội dung
        /// </summary>
        [SerializeField]
        private UIEventBroadboardMini_Content UIText_CustomTextPrefab;

        /// <summary>
        /// Button mở khung tổ đội
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenTeamFrame;

        /// <summary>
        /// Button hiện khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Show;

        /// <summary>
        /// Button ẩn khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Hide;

        /// <summary>
        /// Vị trí khung xuất hiện trên màn hình
        /// </summary>
        [SerializeField]
        private Vector2 VisiblePosition;

        /// <summary>
        /// Vị trí khung ẩn khỏi màn hình
        /// </summary>
        [SerializeField]
        private Vector2 InvisiblePosition;

        /// <summary>
        /// Thời gian thực hiện hiệu ứng
        /// </summary>
        [SerializeField]
        private float AnimationDuration;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform nội dung
        /// </summary>
        private RectTransform transformCustomText = null;

        /// <summary>
        /// Luồng thực thi đếm lùi
        /// </summary>
        private Coroutine countdownTimer = null;

        /// <summary>
        /// RectTransform của đối tượng
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// Luồng thực hiện hiệu ứng
        /// </summary>
        private Coroutine animationCoroutine;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện mở khung tổ đội
        /// </summary>
        public Action OpenTeamFrame { get; set; }

        /// <summary>
        /// Tên sự kiện
        /// </summary>
        public string EventName
        {
            get
            {
                return this.UIText_EventName.text;
            }
            set
            {
                this.UIText_EventName.text = value;
            }
        }

        private string _ShortDesc;
        /// <summary>
        /// Mô tả ngắn
        /// </summary>
        public string ShortDesc
        {
            get
            {
                return this._ShortDesc;
            }
            set
            {
                this._ShortDesc = value;
                this.UpdateShortDesc();
            }
        }

        private List<string> _CustomContents = null;
        /// <summary>
        /// Các nội dung tùy chọn
        /// </summary>
        public List<string> CustomContents
        {
            get
            {
                return this._CustomContents;
            }
            set
            {
                this._CustomContents = value;
                this.UpdateCustomContents();
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
            this.transformCustomText = this.UIText_CustomTextPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.UpdateShortDesc();
            this.RebuildLayout();
        }

        /// <summary>
        /// Hàm nàyg gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.UpdateShortDesc();
            this.RebuildLayout();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
            this.countdownTimer = null;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_OpenTeamFrame.onClick.AddListener(this.ButtonOpenUITeamFrame_Clicked);

            this.UIButton_Hide.onClick.AddListener(this.Hide);
            this.UIButton_Show.onClick.AddListener(this.Show);
        }

        /// <summary>
        /// Sự kiện khi Button mở khung tổ đội được ấn
        /// </summary>
        private void ButtonOpenUITeamFrame_Clicked()
        {
            this.OpenTeamFrame?.Invoke();
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Thực hiện đếm lùi
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartCountDown(int timeSec)
        {
            /// Lặp liên tục
            while (true)
            {
                /// Nếu dưới 0 thì thoát
                if (timeSec < 0)
                {
                    break;
                }
                /// Cập nhật Text
                this.UIText_EventShortDetail.text = string.Format("Thời gian: <color=green>{0}</color>", KTGlobal.DisplayTime(timeSec--));
                /// Đợi 1s
                yield return new WaitForSeconds(1f);
            }
            /// Hủy luồng
            this.countdownTimer = null;
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrame(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
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

            this.StartCoroutine(this.ExecuteSkipFrame(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformCustomText);
            }));
        }

        /// <summary>
        /// Tìm Text ở dòng tương ứng (đánh số từ 1 lên)
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private UIEventBroadboardMini_Content FindContent(int line)
        {
            /// Tổng số con
            int childCount = this.transformCustomText.childCount;
            /// Nếu số dòng vượt quá thì bỏ qua
            if (line >= childCount)
            {
                return null;
            }
            /// Trả về kết quả
            return this.transformCustomText.GetChild(line).GetComponent<UIEventBroadboardMini_Content>();
        }

        /// <summary>
        /// Thêm dòng mới
        /// </summary>
        /// <returns></returns>
        private UIEventBroadboardMini_Content AddNewLine()
        {
            UIEventBroadboardMini_Content uiText = GameObject.Instantiate<UIEventBroadboardMini_Content>(this.UIText_CustomTextPrefab);
            uiText.gameObject.SetActive(true);
            uiText.transform.SetParent(this.transformCustomText, false);

            return uiText;
        }

        /// <summary>
        /// Ẩn toàn bộ các dòng
        /// </summary>
        private void DisableAllLines()
        {
            foreach (Transform child in this.transformCustomText.transform)
            {
                UIEventBroadboardMini_Content uiText = child.GetComponent<UIEventBroadboardMini_Content>();
                uiText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Cập nhật hiển thị mô tả
        /// </summary>
        /// <param name="sec"></param>
        public void UpdateShortDesc()
        {
            /// Nếu đối tượng không hiển thị
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Nếu luồng đếm lùi đang tồn tại
            if (this.countdownTimer != null)
            {
                this.StopCoroutine(this.countdownTimer);
            }

            /// Nếu chứa Timer
            if (!string.IsNullOrEmpty(this._ShortDesc) && this._ShortDesc.Contains("TIME"))
            {
                try
                {
                    string[] fields = this._ShortDesc.Split('|');
                    int timeSec = int.Parse(fields[1]);
                    /// Thực hiện đếm lùi
                    this.countdownTimer = this.StartCoroutine(this.StartCountDown(timeSec));
                }
                catch (Exception) { }
            }
            /// Nếu không chứa Timer
            else
            {
                this.UIText_EventShortDetail.text = this._ShortDesc;
            }
        }

        /// <summary>
        /// Thiết lập Text theo thứ tự dòng tương ứng
        /// </summary>
        /// <param name="contentLine"></param>
        private void UpdateCustomContents()
        {
            /// Nếu số con lớn hơn tổng số dòng thì phải sinh thêm
            while (this.transformCustomText.childCount <= this._CustomContents.Count)
            {
                /// Sinh thêm
                this.AddNewLine();
            }

            /// Ẩn toàn bộ các dòng
            this.DisableAllLines();

            /// Thứ tự dòng
            int idx = 0;
            /// Duyệt danh sách nội dung
            foreach (string text in this._CustomContents)
            {
                idx++;
                /// Đối tượng tương ứng
                UIEventBroadboardMini_Content uiText = this.FindContent(idx);
                /// Nếu không tìm thấy thì Break luôn
                if (uiText == null)
                {
                    break;
                }
                /// Thiết lập giá trị
                uiText.Text = text;
                /// Kích hoạt dòng
                uiText.gameObject.SetActive(true);
            }
            /// Xây lại giao diện
            this.RebuildLayout();
        }

        #region Animation
        /// <summary>
        /// Luồng thực hiện chạy hiệu ứng
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="toPosition"></param>
        /// <returns></returns>
        private IEnumerator PlayAnimation(Vector2 fromPosition, Vector2 toPosition)
        {
            if (this.rectTransform == null)
            {
                yield break;
            }

            this.UIButton_Hide.gameObject.SetActive(false);
            this.UIButton_Show.gameObject.SetActive(false);

            this.rectTransform.anchoredPosition = fromPosition;
            yield return null;
            float lifeTime = 0f;
            while (lifeTime < this.AnimationDuration)
            {
                lifeTime += Time.deltaTime;
                float percent = lifeTime / this.AnimationDuration;
                Vector2 newPos = fromPosition + (toPosition - fromPosition) * percent;
                this.rectTransform.anchoredPosition = newPos;
                yield return null;
            }
            this.rectTransform.anchoredPosition = toPosition;

            if (toPosition == this.VisiblePosition)
            {
                this.UIButton_Hide.gameObject.SetActive(true);
            }
            else
            {
                this.UIButton_Show.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hiện khung
        /// </summary>
        private void Show()
        {
            if (this.animationCoroutine != null)
            {
                this.StopCoroutine(this.animationCoroutine);
            }

            this.animationCoroutine = this.StartCoroutine(this.PlayAnimation(this.InvisiblePosition, this.VisiblePosition));
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        private void Hide()
        {
            if (this.animationCoroutine != null)
            {
                this.StopCoroutine(this.animationCoroutine);
            }

            this.animationCoroutine = this.StartCoroutine(this.PlayAnimation(this.VisiblePosition, this.InvisiblePosition));
        }
        #endregion
        #endregion

        #region Public methods
        #endregion
    }
}
