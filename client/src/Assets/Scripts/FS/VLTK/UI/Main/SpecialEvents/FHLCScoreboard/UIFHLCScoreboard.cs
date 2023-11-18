using FS.VLTK.UI.Main.SpecialEvents.FHLCScoreboard;
using Server.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SpecialEvents
{
    /// <summary>
    /// Khung bảng xếp hạng sự kiện Phong Hỏa Liên Thành
    /// </summary>
    public class UIFHLCScoreboard : MonoBehaviour
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
        private UIFHLCScoreboard_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Text thứ hạng bản thân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SelfRank;
        #endregion

        #region Properties
        private FHLCScoreboardData _Data;
        /// <summary>
        /// Dữ liệu xếp hạng
        /// </summary>
        public FHLCScoreboardData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách người chơi
        /// </summary>
        private RectTransform transformPlayerList;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformPlayerList = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DoExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private void ExecuteSkipFrames(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Toác
            if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Làm rỗng danh sách người chơi
            foreach (Transform child in this.transformPlayerList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Duyệt danh sách
            foreach (FHLC_Record record in this._Data.Records)
            {
                /// Tạo mới
                UIFHLCScoreboard_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UIFHLCScoreboard_PlayerInfo>(this.UI_PlayerInfoPrefab);
                uiPlayerInfo.transform.SetParent(this.transformPlayerList, false);
                uiPlayerInfo.gameObject.SetActive(true);
                uiPlayerInfo.Data = record;
            }

            /// Thứ hạng bản thân
            this.UIText_SelfRank.text = this._Data.SelfRank.ToString();

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPlayerList);
            });
        }
        #endregion
    }
}
