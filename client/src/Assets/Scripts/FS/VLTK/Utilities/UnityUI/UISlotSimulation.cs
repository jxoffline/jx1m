using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Khung trượt dạng Game Slot
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.RectMask2D))]
    public class UISlotSimulation : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// RectTransform danh sách nút
        /// </summary>
        [SerializeField]
        private RectTransform _SlotTransform;

        /// <summary>
        /// Danh sách nút bên trong
        /// </summary>
        [SerializeField]
        private RectTransform[] _Items;

        /// <summary>
        /// Button tương ứng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Số vòng
        /// </summary>
        [SerializeField]
        private int _Round = 2;

        /// <summary>
        /// Vận tốc ban đầu
        /// </summary>
        [SerializeField]
        private float _Velocity = 100;

        /// <summary>
        /// Gia tốc 
        /// </summary>
        [SerializeField]
        private float _Acceleration;

        /// <summary>
        /// Tự động thực thi
        /// </summary>
        [SerializeField]
        private bool _AutoPlay = false;

        /// <summary>
        /// Vị trí dừng
        /// </summary>
        [SerializeField]
        private int _StopIndex;

#if UNITY_EDITOR
        /// <summary>
        /// Mô phỏng ngay
        /// </summary>
        [SerializeField]
        private bool _SimulateNow;

        /// <summary>
        /// Thiết lập lại thứ tự các nút ngay
        /// </summary>
        [SerializeField]
        private bool _RearrangeNow;
#endif
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng mô phỏng chuyển động
        /// </summary>
        private Coroutine simulationCoroutine = null;

        /// <summary>
        /// Đánh dấu vị trí của nút con ban đầu
        /// </summary>
        private readonly Dictionary<int, RectTransform> ItemIndexes = new Dictionary<int, RectTransform>();
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện Click
        /// </summary>
        public Action Click { get; set; }

        /// <summary>
        /// Danh sách nội dung bên trong
        /// </summary>
        public RectTransform[] Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
                this.ClearNodes();
                this.RefreshNodes();
            }
        }

        /// <summary>
        /// Số vòng
        /// </summary>
        public int Round
        {
            get
            {
                return this._Round;
            }
            set
            {
                this._Round = value;
                this.Stop();
            }
        }

        /// <summary>
        /// Vận tốc
        /// </summary>
        public float Velocity
        {
            get
            {
                return this._Velocity;
            }
            set
            {
                this._Velocity = value;
                this.Stop();
            }
        }

        /// <summary>
        /// Gia tốc
        /// </summary>
        public float Acceleration
        {
            get
            {
                return this._Acceleration;
            }
            set
            {
                this._Acceleration = value;
                this.Stop();
            }
        }

        /// <summary>
        /// Vị trí dừng lại
        /// </summary>
        public int StopIndex
        {
            get
            {
                return this._StopIndex;
            }
            set
            {
                this._StopIndex = value;
                this.Stop();
            }
        }

        /// <summary>
        /// Tự thực thi
        /// </summary>
        public bool AutoPlay
        {
            get
            {
                return this._AutoPlay;
            }
            set
            {
                this._AutoPlay = value;
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

#if UNITY_EDITOR
        /// <summary>
        /// Hàm này chạy liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            /// Nếu đang ở chế độ Play
            if (Application.isPlaying)
            {
                return;
            }

            /// Nếu yêu cầu mô phỏng ngay
            if (this._SimulateNow)
            {
                this.Play();
                this._SimulateNow = false;
            }

            /// Nếu yêu cầu sắp xếp lại thứ tự như ban đầu
            if (this._RearrangeNow)
            {
                this.RearrangeItems();
                this._RearrangeNow = false;
            }
        }
#endif

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Làm mới
            this.RefreshNodes();
            /// Nếu thiết lập tự thực thi
            if (this._AutoPlay)
            {
                this.Play();
            }
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            if (this.UIButton != null)
            {
                this.UIButton.onClick.AddListener(this.Button_Clicked);
            }
        }

        /// <summary>
        /// Sự kiện khi Button được ấn
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i < skip; i++)
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
            /// Nếu không kích hoạt đối tượng
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            /// Thực thi xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this._SlotTransform);
            }));
        }

        /// <summary>
        /// Thực thi mô phỏng chuyển động
        /// </summary>
        /// <returns></returns>
        private IEnumerator Simulate()
        {
            /// Nếu danh sách nút rỗng
            if (this._SlotTransform.transform.childCount <= 0)
            {
                yield break;
            }

            /// Tổng số nút đến nút dừng
            int nodeToDest = 0;
            /// Tổng số nút cho 1 vòng
            int roundTotalNodes = 0;
            /// Đánh dấu đã tim thấy nút dừng chưa
            bool isFoundStopIndex = false;
            /// Duyệt danh sách
            foreach (Transform child in this._SlotTransform.transform)
            {
                /// RectTransform nút con
                RectTransform childRectTransform = child.GetComponent<RectTransform>();
                /// Nếu đây là nút dừng
                if (this.ItemIndexes[this._StopIndex] == childRectTransform)
                {
                    isFoundStopIndex = true;
                }
                /// Nếu chưa đến nút dừng
                else if (!isFoundStopIndex)
                {
                    /// Cập nhật khoảng cách
                    nodeToDest++;
                }

                /// Tăng tổng khoảng cách 1 vòng lên
                roundTotalNodes++;
            }
            int totalNodes = this._Round * roundTotalNodes + nodeToDest;

            /// Nút con vị trí đầu tiên
            RectTransform firstItem = this._SlotTransform.GetChild(0).GetComponent<RectTransform>();

            /// Vận tốc tức thời
            float velocity = this._Velocity;
            /// Tổng số nút đã đi qua
            int totalPassed = 0;
            /// Chừng nào chưa hết thời gian
            while (totalPassed < totalNodes)
            {
                /// Nếu là pha 1
                if (totalPassed <= totalNodes / 2)
                {
                    /// dv = a * dt
                    velocity += this._Acceleration * Time.deltaTime;
                }
                else
                {
                    /// dv = a * dt
                    velocity -= this._Acceleration * Time.deltaTime;
                }

                /// Khoảng cách dịch được
                float moveDistance = velocity * Time.deltaTime;

                /// Dịch đối tượng về vị trí tương ứng
                this._SlotTransform.anchoredPosition -= new Vector2(0, moveDistance);
                /// Nếu khoảng cách lớn hơn chiều cao của nút con ở vị trí đầu tiên
                if (Mathf.Abs(this._SlotTransform.anchoredPosition.y) > firstItem.sizeDelta.y)
                {
                    /// Dịch đối tượng về vị trí mới
                    this._SlotTransform.anchoredPosition = new Vector2(this._SlotTransform.anchoredPosition.x, firstItem.sizeDelta.y - Mathf.Abs(this._SlotTransform.anchoredPosition.y));
                    /// Đẩy nút con ở vị trí đầu tiên xuống cuối
                    firstItem.transform.SetAsLastSibling();
                    /// Cập nhật nút con mới ở vị trí đầu tiên
                    firstItem = this._SlotTransform.GetChild(0).GetComponent<RectTransform>();
                    /// Tăng tổng số nút đã đi qua
                    totalPassed++;
                }

                /// Bỏ qua Frame
                yield return null;
            }

            /// Thiết lập tọa độ về vị trí y = 0
            this._SlotTransform.anchoredPosition = new Vector2(this._SlotTransform.anchoredPosition.x, 0);
        }

        /// <summary>
        /// Làm rỗng danh sách các nút
        /// </summary>
        private void ClearNodes()
        {
            foreach (Transform child in this._SlotTransform.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Làm mới danh sách nút
        /// </summary>
        private void RefreshNodes()
        {
            /// Ngừng luồng đang thực thi
            this.Stop();

            /// Thêm toàn bộ các nút vào danh sách
            foreach (RectTransform node in this._Items)
            {
                node.transform.SetParent(this._SlotTransform, false);
            }

            /// Xậy lại giao diện
            this.RebuildLayout();

            /// Làm rỗng mảng đánh dấu vị trí
            this.ItemIndexes.Clear();

            /// Duyệt danh sách nút con và thêm vào mảng đánh dấu
            for (int idx = 0; idx < this._Items.Length; idx++)
            {
                this.ItemIndexes[idx] = this._Items[idx];
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Trả về thứ tự nút tương ứng trong danh sách
        /// </summary>
        /// <param name="rectTransform"></param>
        public int FindNodeIndex(RectTransform rectTransform)
        {
            /// Duyệt danh sách
            foreach (KeyValuePair<int, RectTransform> pair in this.ItemIndexes)
            {
                /// Nếu tìm thấy
                if (pair.Value == rectTransform)
                {
                    return pair.Key;
                }
            }
            /// Nếu không tìm thấy
            return -1;
        }

        /// <summary>
        /// Trả về thứ tự nút tương ứng trong danh sách
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int FindNodeIndex(Predicate<RectTransform> predicate)
        {
            /// Duyệt danh sách
            foreach (KeyValuePair<int, RectTransform> pair in this.ItemIndexes)
            {
                /// Nếu thỏa mãn điều kiện
                if (predicate(pair.Value))
                {
                    return pair.Key;
                }
            }
            /// Nếu không tìm thấy
            return -1;
        }

        /// <summary>
        /// Thực thi
        /// </summary>
        /// <param name="stopIndex"></param>
        public void Play()
        {
            /// Ngừng luồng cũ lại
            this.Stop();

            /// Xây lại giao diện nếu cần
            this.RebuildLayout();

            /// Thực thi mô phỏng
            this.simulationCoroutine = this.StartCoroutine(this.Simulate());
        }

        /// <summary>
        /// Ngừng lại
        /// </summary>
        public void Stop()
        {
            if (this.simulationCoroutine != null)
            {
                this.StopCoroutine(this.simulationCoroutine);
                this.simulationCoroutine = null;
            }
        }

        /// <summary>
        /// Tự xếp lại thứ tự danh sách
        /// </summary>
        public void RearrangeItems()
        {
            /// Ngừng luồng mô phỏng
            this.Stop();
            /// Duyệt danh sách và sắp xếp lại về thứ tự gốc
            foreach (KeyValuePair<int, RectTransform> pair in this.ItemIndexes)
            {
                pair.Value.transform.SetSiblingIndex(pair.Key);
            }
        }
        #endregion
    }
}
