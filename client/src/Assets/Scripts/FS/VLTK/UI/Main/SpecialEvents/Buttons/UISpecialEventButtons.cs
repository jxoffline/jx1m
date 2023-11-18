using FS.GameEngine.Logic;
using System;
using UnityEngine;

namespace FS.VLTK.UI.Main.SpecialEvents
{
    /// <summary>
    /// Quản lý Button các sự kiện đặc biệt
    /// </summary>
    public class UISpecialEventButtons : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button mở bảng xếp hạng Tống Kim
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SongJin_OpenRankingBoard;

        /// <summary>
        /// Button bảng xếp hạng môn phái
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_FactionBattle_OpenBoard;

        /// <summary>
        /// Button bảng xếp hạng Phong Hỏa Liên Thành
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_FHLC_OpenBoard;
        #endregion

        #region Properties
        /// <summary>
        /// Mở bảng xếp hạng Tống Kim
        /// </summary>
        public Action OpenSongJinRankingBoard { get; set; }

        /// <summary>
        /// Mở Bảng Xếp Hạng Thi đấu môn phái
        /// </summary>
        public Action OpenFactionBattleBoard { get; set; }

        /// <summary>
        /// Mở bảng xếp hạng Phong Hỏa Liên Thành
        /// </summary>
        public Action OpenFengHuoLianChengScoreBoard { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.HideAllButtons();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_SongJin_OpenRankingBoard.onClick.AddListener(() => {
                this.OpenSongJinRankingBoard?.Invoke();
            });
            this.UIButton_FactionBattle_OpenBoard.onClick.AddListener(() => {
                this.OpenFactionBattleBoard?.Invoke();
            });
            this.UIButton_FHLC_OpenBoard.onClick.AddListener(() => {
                this.OpenFengHuoLianChengScoreBoard?.Invoke();
            });
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiển thị Button ID tương ứng hoạt động
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        public void SetButtonState(int id, bool state)
        {
            switch (id)
            {
                /// Tống Kim
                case (int) GameEvent.SongJin:
                {
                    this.UIButton_SongJin_OpenRankingBoard.gameObject.SetActive(state);
                    break;
                }
                /// Thi đấu môn phái
                case (int) GameEvent.FactionBattle:
                {
                    this.UIButton_FactionBattle_OpenBoard.gameObject.SetActive(state);
                    break;
                }
                /// Phong Hỏa Liên Thành
                case (int) GameEvent.FengHuoLianCheng:
                {
                    this.UIButton_FHLC_OpenBoard.gameObject.SetActive(state);
                    break;
                }
            }
        }

        /// <summary>
        /// Ẩn toàn bộ Button
        /// </summary>
        public void HideAllButtons()
        {
            this.UIButton_SongJin_OpenRankingBoard.gameObject.SetActive(false);
            this.UIButton_FactionBattle_OpenBoard.gameObject.SetActive(false);
            this.UIButton_FHLC_OpenBoard.gameObject.SetActive(false);
        }
        #endregion
    }
}
