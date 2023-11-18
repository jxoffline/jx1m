using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.SpecialEvents.FactionBattle
{
    /// <summary>
    /// Bảng xếp hạng thi đấu môn phái
    /// </summary>
    public class UIFactionBattle_Ranking : MonoBehaviour
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
        private UIFactionBattle_Ranking_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Text thứ hạng bản thân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MyRank;
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

        /// <summary>
        /// Thông tin bảng xếp hạng Môn phái
        /// </summary>
        public List<FACTION_PVP_RANKING> Data { get; set; }
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
            this.RefreshData();
            this.StartCoroutine(this.AutoClose());
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
        /// Luồng thực hiện tự đóng khung sau 10s
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoClose()
        {
            KTGlobal.AddNotification("Bảng xếp hạng sẽ tự đóng sau 10 giây!");
            yield return new WaitForSeconds(10f);
            this.ButtonClose_Clicked();
        }

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
        private void AddPlayer(FACTION_PVP_RANKING playerInfo)
        {
            UIFactionBattle_Ranking_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIFactionBattle_Ranking_PlayerInfo>(this.UI_PlayerInfoPrefab);
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

            if (this.Data == null || this.Data.Count <= 0)
            {
                return ;
            }
            /// Dữ liệu bản thân
            FACTION_PVP_RANKING myselfRanking = this.Data[this.Data.Count - 1];
            /// Cập nhật thứ hạng
            this.UIText_MyRank.text = string.Format("{0}/{1}", myselfRanking.Rank, this.Data.Count);

            /// Xây danh sách
            for (int i = 0; i < this.Data.Count - 1; i++)
            {
                FACTION_PVP_RANKING playerInfo = this.Data[i];
                this.AddPlayer(playerInfo);
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
