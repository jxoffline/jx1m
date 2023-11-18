using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.SpecialEvents.SongJinRankingBoard;
using System.Collections;
using Server.Data;

namespace FS.VLTK.UI.Main.SpecialEvents
{
    /// <summary>
    /// Bảng xếp hạng Tống Kim
    /// </summary>
    public class UISongJinRankingBoard : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thông tin người chơi
        /// </summary>
        [SerializeField]
        private UISongJinRankingBoard_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Text thứ hạng bản thân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MyRank;

        /// <summary>
        /// Text tổng điểm phe Tống
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SongScore;

        /// <summary>
        /// Text tổng điểm phe Kim
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_JinScore;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách người chơi
        /// </summary>
        private RectTransform transformPlayersList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        private SongJinBattleRankingInfo _Data;
        /// <summary>
        /// Thông tin bảng xếp hạng Tống KIm
        /// </summary>
        public SongJinBattleRankingInfo Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.RefreshData();
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformPlayersList = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
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

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPlayersList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        private void ClearList()
        {
            foreach (Transform child in this.transformPlayersList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Thêm người chơi vào danh sách
        /// </summary>
        /// <param name="playerInfo"></param>
        private void AddPlayer(SongJinRanking playerInfo)
        {
            UISongJinRankingBoard_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UISongJinRankingBoard_PlayerInfo>(this.UI_PlayerInfoPrefab);
            uiPlayerInfo.gameObject.SetActive(true);
            uiPlayerInfo.transform.SetParent(this.transformPlayersList, false);

            uiPlayerInfo.Data = playerInfo;
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Làm rỗng danh sách
            this.ClearList();

            /// Dữ liệu bản thân
            SongJinRanking myselfRanking = this._Data.PlayerRanks[this._Data.PlayerRanks.Count - 1];
            /// Cập nhật thứ hạng
            this.UIText_MyRank.text = myselfRanking.Rank.ToString();

            /// Điểm số phe Tống
            this.UIText_SongScore.text = this._Data.SongTotalScore.ToString();
            /// Điểm số phe Kim
            this.UIText_JinScore.text = this._Data.JinTotalScore.ToString();

            /// Xây danh sách
            for (int i = 0; i < this._Data.PlayerRanks.Count - 1; i++)
            {
                this.AddPlayer(this._Data.PlayerRanks[i]);
            }
            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion

        #region Public methods

        #endregion
    }
}
