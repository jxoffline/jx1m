using System;
using System.Collections.Generic;
using UnityEngine;
using FS.VLTK.Utilities.UnityUI;
using System.Collections;
using UnityEngine.UI;

namespace FS.VLTK.UI.Main.Ranking
{
    /// <summary>
    /// Tab loại thứ hạng
    /// </summary>
    public class UIRanking_RankTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle danh mục con
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_SubRankPrefab;

        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform nhóm thứ hạng
        /// </summary>
        private RectTransform transformGroupRankList = null;
        #endregion

        #region Properties
        /// <summary>
        /// ID thứ hạng
        /// </summary>
        public int RankType { get; set; }

        /// <summary>
        /// Tên xếp hạng
        /// </summary>
        public string RankName
        {
            get
            {
                return this.UIToggle.Name;
            }
            set
            {
                this.UIToggle.Name = value;
            }
        }

        /// <summary>
        /// Danh sách thứ hạng con
        /// </summary>
        public Dictionary<int, string> SubRankTypes { get; set; }

        /// <summary>
        /// Sự kiện chọn loại xếp hạng
        /// </summary>
        public Action<int> Click { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformGroupRankList = this.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
            this.SetSubTabsVisible(false);
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.RebuildLayout();
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
            /// Nếu được chọn
            if (isSelected)
            {
                /// Nếu có thông tin
                if (this.RankType != -1)
                {
                    /// Thực thi sự kiện Click
                    this.Click?.Invoke(this.RankType);
                }
                /// Nếu không có thông tin
                else
                {
                    /// Hiện toàn bộ Tab con
                    this.SetSubTabsVisible(true);
                    /// Xây lại giao diện
                    this.RebuildParentLayout();
                }
            }
            /// Nếu không được chọn
            else
            {
                /// Ẩn toàn bộ Tab con
                this.SetSubTabsVisible(false);
                /// Xây lại giao diện
                this.RebuildParentLayout();
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle con được cọn
        /// </summary>
        /// <param name="subToggle"></param>
        private void SubToggle_Selected(int rankType)
        {
            /// Thực thi sự kiện Click
            this.Click?.Invoke(rankType);
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
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
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformGroupRankList);
            }));
        }

        /// <summary>
        /// Xây lại giao diện cha
        /// </summary>
        private void RebuildParentLayout()
        {
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transform.parent.GetComponent<RectTransform>());
            }));
        }

        /// <summary>
        /// Làm rỗng toàn bộ các Tab
        /// </summary>
        private void ClearAllSubTabs()
        {
            foreach (Transform child in this.transformGroupRankList.transform)
            {
                if (child.gameObject != this.UIToggle_SubRankPrefab.gameObject && child.gameObject != this.UIToggle.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void Refresh()
        {
            /// Nếu có danh sách Tab con
            if (this.SubRankTypes != null && this.SubRankTypes.Count > 0)
            {
                /// Duyệt danh sách loại thứ hạng
                foreach (KeyValuePair<int, string> rankInfo in this.SubRankTypes)
                {
                    UIToggleSprite uiToggle = GameObject.Instantiate<UIToggleSprite>(this.UIToggle_SubRankPrefab);
                    uiToggle.transform.SetParent(this.transformGroupRankList, false);
                    uiToggle.gameObject.SetActive(true);
                    uiToggle.Group = this.GetComponent<ToggleGroup>();
                    /// Gắn Tag tương ứng
                    uiToggle.Name = rankInfo.Value;

                    /// Gắn sự kiện Select
                    uiToggle.OnSelected = (isSelected) => {
                        if (isSelected)
                        {
                            this.SubToggle_Selected(rankInfo.Key);
                        }
                    };
                }
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Thiết lập hiển thị các Tab con
        /// </summary>
        /// <param name="isVisible"></param>
        private void SetSubTabsVisible(bool isVisible)
        {
            /// Duyệt danh sách Tab con
            foreach (Transform child in this.transformGroupRankList.transform)
            {
                if (child.gameObject != this.UIToggle_SubRankPrefab.gameObject && child.gameObject != this.UIToggle.gameObject)
                {
                    /// Thiết lập hiển thị tương ứng
                    child.gameObject.SetActive(isVisible);
                }
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
