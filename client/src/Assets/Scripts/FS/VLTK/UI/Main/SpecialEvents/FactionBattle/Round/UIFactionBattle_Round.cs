using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Server.Data;
using System.Collections;

namespace FS.VLTK.UI.Main.SpecialEvents.FactionBattle
{
    /// <summary>
    /// Bảng thi đấu môn phái
    /// </summary>
    public class UIFactionBattle_Round : MonoBehaviour
    {
		#region Define
		/// <summary>
		/// Button đóng khung 
		/// </summary>
		[SerializeField]
        private Button UIButton_Close;

        /// <summary>
        ///  khung khu vực thi đấu 1
        /// </summary>
        [SerializeField]
        private UIFactionBattle_Round_RoundInfo[] UI_Round1;

        /// <summary>
        ///  khung khu vực thi đấu 2
        /// </summary>
        [SerializeField]
        private UIFactionBattle_Round_RoundInfo[] UI_Round2;

        /// <summary>
        ///  khung khu vực thi đấu 3
        /// </summary>
        [SerializeField]
        private UIFactionBattle_Round_RoundInfo[] UI_Round3;

        /// <summary>
        ///  khung khu vực thi đấu 4
        /// </summary>
        [SerializeField]
        private UIFactionBattle_Round_RoundInfo[] UI_Round4;
        #endregion

        #region Properties
        /// <summary>
        /// sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// danh sách thi đấu
        /// </summary>
        private FACTION_PVP_RANKING_INFO _Data;
        /// <summary>
        /// Thông tin bảng xếp hạng môn phái
        /// </summary>
        public FACTION_PVP_RANKING_INFO Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.Refresh();
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này được gọi ở frame đầu tiền
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
            this.StartCoroutine(this.AutoClose());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Hàm khởi tạo
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
            KTGlobal.AddNotification("Bảng xếp thi đấu sẽ tự đóng sau 10 giây!");
            yield return new WaitForSeconds(10f);
            this.ButtonClose_Clicked();
		}

        /// <summary>
        /// Xóa dữ liệu toàn bộ của các vòng đấu
        /// </summary>
        private void ClearRoundsData()
		{
            foreach (UIFactionBattle_Round_RoundInfo uiRoundInfo in this.UI_Round1)
			{
                uiRoundInfo.Data = null;
			}
            foreach (UIFactionBattle_Round_RoundInfo uiRoundInfo in this.UI_Round2)
			{
                uiRoundInfo.Data = null;
			}
            foreach (UIFactionBattle_Round_RoundInfo uiRoundInfo in this.UI_Round3)
			{
                uiRoundInfo.Data = null;
			}
            foreach (UIFactionBattle_Round_RoundInfo uiRoundInfo in this.UI_Round4)
			{
                uiRoundInfo.Data = null;
			}
		}

        /// <summary>
        /// Thiết lập dữ liệu trận tương ứng
        /// </summary>
        /// <param name="uiRoundInfo"></param>
        /// <param name="roundData"></param>
        private void SetRoundData(UIFactionBattle_Round_RoundInfo uiRoundInfo, ELIMINATION_SCOREBOARD roundData)
		{
            uiRoundInfo.Data = roundData;
		}

        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        private void Refresh()
        {
            /// Xóa dữ liệu toàn bộ các vòng đấu
            this.ClearRoundsData();

            /// Nếu không có dữ liệu
            if (this.Data == null && (this.Data.ELIMINATION_SCORE == null || this.Data.ELIMINATION_SCORE.Count <= 0))
			{
                return;
			}

            /// Duyệt danh sách
            foreach (ELIMINATION_SCOREBOARD roundData in this.Data.ELIMINATION_SCORE)
			{
                /// Thứ tự vòng
                int roundIndex = roundData.ROUNDID;
                /// Thứ tự trận
                int arenaID = roundData.ARENAID;
                /// UI tương ứng
                UIFactionBattle_Round_RoundInfo uiRoundData = null;
                /// Nếu là vòng 1
                if (roundIndex == 1)
				{
                    uiRoundData = this.UI_Round1[arenaID - 1];
				}
                /// Nếu là vòng 2
                else if (roundIndex == 2)
				{
                    uiRoundData = this.UI_Round2[arenaID - 1];
				}
                /// Nếu là vòng 3
                else if (roundIndex == 3)
				{
                    uiRoundData = this.UI_Round3[arenaID - 1];
				}
                /// Nếu là vòng 4
                else if (roundIndex == 4)
				{
                    uiRoundData = this.UI_Round4[arenaID - 1];
				}

                /// Nếu không có dữ liệu
                if (uiRoundData == null)
				{
                    continue;
				}

                /// Thiết lập dữ liệu
                this.SetRoundData(uiRoundData, roundData);
            }
        }
        #endregion
    }
}
