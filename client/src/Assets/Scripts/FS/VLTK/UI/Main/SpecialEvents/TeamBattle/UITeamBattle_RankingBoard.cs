using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.SpecialEvents.TeamBattle;
using Server.Data;
using System.Collections;

namespace FS.VLTK.UI.Main.SpecialEvents
{
    /// <summary>
    /// Khung bảng xếp hạng chiến đội Võ lâm liên đấu
    /// </summary>
    public class UITeamBattle_RankingBoard : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text thời gian cập nhật cuối
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LastUpdateTime;

        /// <summary>
        /// Prefab thông tin nhóm
        /// </summary>
        [SerializeField]
        private UITeamBattle_RankingBoard_TeamInfo UI_TeamInfoPrefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách nhóm
        /// </summary>
        private RectTransform transformTeamList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Dữ liệu
        /// </summary>
        public List<TeamBattleInfo> Data { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformTeamList = this.UI_TeamInfoPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
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
        /// Sự kiện khi Button đóng khung được ấn
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
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            /// Xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformTeamList);
            }));
        }

        /// <summary>
        /// Làm rỗng dữ liệu
        /// </summary>
        private void ClearData()
        {
            foreach (Transform child in this.transformTeamList.transform)
            {
                if (child.gameObject != this.UI_TeamInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm chiến đội tương ứng
        /// </summary>
        /// <param name="teamInfo"></param>
        private void AddTeam(TeamBattleInfo teamInfo)
        {
            UITeamBattle_RankingBoard_TeamInfo uiTeamInfo = GameObject.Instantiate<UITeamBattle_RankingBoard_TeamInfo>(this.UI_TeamInfoPrefab);
            uiTeamInfo.transform.SetParent(this.transformTeamList, false);
            uiTeamInfo.gameObject.SetActive(true);
            uiTeamInfo.Data = teamInfo;
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void Refresh()
        {
            /// Làm rỗng dữ liệu
            this.ClearData();

            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                return;
            }

            /// Cập nhật thời gian
            this.UIText_LastUpdateTime.text = this.Data.First().LastUpdateRankTime.ToString("HH:mm - dd/MM/yyyy");

            /// Duyệt danh sách chiến đội
            foreach (TeamBattleInfo teamInfo in this.Data)
            {
                this.AddTeam(teamInfo);
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
