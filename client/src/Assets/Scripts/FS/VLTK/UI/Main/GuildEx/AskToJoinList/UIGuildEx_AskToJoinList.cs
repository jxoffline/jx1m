using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.GuildEx.AskToJoinList;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung danh sách người xin vào bang hội
    /// </summary>
    public class UIGuildEx_AskToJoinList : MonoBehaviour
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
        private UIGuildEx_AskToJoinList_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Button phân trang - trang trước
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_Pagination_PreviousPage;

        /// <summary>
        /// Button phân trang - trang tiếp theo
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_Pagination_NextPage;

        /// <summary>
        /// Text phân trang - trang hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Pagination_CurrentPage;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleName;

        /// <summary>
        /// Text môn phái người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleFaction;

        /// <summary>
        /// Text cấp độ người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleLevel;

        /// <summary>
        /// Text tài phú người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleTotalValues;

        /// <summary>
        /// Button đồng ý
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Accept;

        /// <summary>
        /// Button từ chối
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Reject;

        /// <summary>
        /// Text trạng thái tự động duyệt vào bang hội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AutoAcceptState;

        /// <summary>
        /// Button mở khung thiết lập tự duyệt thành viên xin vào bang
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OpenAutoAcceptSetting;

        /// <summary>
        /// Khung thiết lập tự động duyệt người chơi vào bang
        /// </summary>
        [SerializeField]
        private UIGuildEx_AskToJoinList_AutoAcceptRule UI_AutoAcceptRule;
        #endregion

        #region Properties
        private RequestJoinInfo _Data;
        /// <summary>
        /// Danh sách người chơi xin vào bang
        /// </summary>
        public RequestJoinInfo Data
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

        /// <summary>
        /// Sự kiện đồng ý cho người chơi vào bang
        /// </summary>
        public Action<int> Accept { get; set; }

        /// <summary>
        /// Sự kiện từ chối cho người chơi vào bang
        /// </summary>
        public Action<int> Reject { get; set; }

        /// <summary>
        /// Sự kiện lưu thiết lập tự đồng ý cho người chơi vào bang
        /// </summary>
        public Action<string> SaveAutoAcceptRule { get; set; }

        /// <summary>
        /// Truy vấn danh sách người chơi xin vào bang theo trang
        /// </summary>
        public Action<int> QueryAskToJoinPlayers { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách người chơi
        /// </summary>
        private RectTransform playerListTransform;

        /// <summary>
        /// Người chơi được chọn
        /// </summary>
        private RequestJoin selectedPlayer;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.playerListTransform = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_Accept.onClick.AddListener(this.ButtonAccept_Clicked);
            this.UIButton_Reject.onClick.AddListener(this.ButtonReject_Clicked);
            this.UIButton_OpenAutoAcceptSetting.onClick.AddListener(this.ButtonOpenAutoAcceptRule_Clicked);
            this.UIButton_Pagination_NextPage.Click = this.ButtonPaginationNextPage_Clicked;
            this.UIButton_Pagination_PreviousPage.Click = this.ButtonPaginationPreviousPage_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button phân trang - trang kế tiếp được ấn
        /// </summary>
        private void ButtonPaginationNextPage_Clicked()
        {
            /// Nếu không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu là trang đầu tiên
            else if (this._Data.PageIndex <= 1)
            {
                /// Bỏ qua
                return;
            }
            /// Ẩn Button chức năng
            this.UIButton_Pagination_NextPage.Enable = false;
            this.UIButton_Pagination_PreviousPage.Enable = false;

            /// Thực hiện truy vấn
            this.QueryAskToJoinPlayers?.Invoke(this._Data.PageIndex - 1);
        }

        /// <summary>
        /// Sự kiện khi Button phân trang - trang trước được ấn
        /// </summary>
        private void ButtonPaginationPreviousPage_Clicked()
        {
            /// Nếu không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu là trang cuối cùng
            else if (this._Data.PageIndex >= this._Data.TotalPage)
            {
                /// Bỏ qua
                return;
            }
            /// Ẩn Button chức năng
            this.UIButton_Pagination_NextPage.Enable = false;
            this.UIButton_Pagination_PreviousPage.Enable = false;

            /// Thực hiện truy vấn
            this.QueryAskToJoinPlayers?.Invoke(this._Data.PageIndex + 1);
        }

        /// <summary>
        /// Sự kiện khi Button đồng ý được ấn
        /// </summary>
        private void ButtonAccept_Clicked()
        {
            /// Nếu không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không có người chơi nào được chọn
            else if (this.selectedPlayer == null)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Hãy chọn một người chơi, sau đó tiến hành thao tác!");
                return;
            }

            /// Thực thi sự kiện
            this.Accept?.Invoke(this.selectedPlayer.RoleID);
            /// Tải lại trang
            this.QueryAskToJoinPlayers?.Invoke(this._Data.PageIndex);
        }

        /// <summary>
        /// Sự kiện khi Button từ chối được ấn
        /// </summary>
        private void ButtonReject_Clicked()
        {
            /// Nếu không có bang hội
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không có người chơi nào được chọn
            else if (this.selectedPlayer == null)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Hãy chọn một người chơi, sau đó tiến hành thao tác!");
                return;
            }

            /// Thực thi sự kiện
            this.Reject?.Invoke(this.selectedPlayer.RoleID);
            /// Tải lại trang
            this.QueryAskToJoinPlayers?.Invoke(this._Data.PageIndex);
        }

        /// <summary>
        /// Sự kiện khi Button mở khung thiết lập tự duyệt người chơi vào bang được ấn
        /// </summary>
        private void ButtonOpenAutoAcceptRule_Clicked()
        {
            /// Nếu không có dữ liệu truyền về
            if (this._Data == null)
            {
                /// Bỏ qua
                return;
            }
            /// Nếu không có bang hội
            else if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Bạn không có bang hội, không thể thực hiện thao tác này!");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (Global.Data.RoleData.GuildRank != (int) GuildRank.Master && Global.Data.RoleData.GuildRank != (int) GuildRank.ViceMaster)
            {
                /// Bỏ qua
                KTGlobal.AddNotification("Chỉ có bang chủ hoặc phó bang chủ mới có thể thực hiện thao tác này!");
                return;
            }

            /// Chuỗi mã hóa thông tin
            string[] autoAcceptRules = this._Data.AutoAcceptRule.Split('|');
            /// Giải mã dữ liệu
            int.TryParse(autoAcceptRules[0], out int levelRule);
            int.TryParse(autoAcceptRules[1], out int factionIDRule);
            long.TryParse(autoAcceptRules[2], out long totalValueRule);
            /// Đổ dữ liệu
            this.UI_AutoAcceptRule.Level = levelRule;
            this.UI_AutoAcceptRule.FactionID = factionIDRule;
            this.UI_AutoAcceptRule.TotalValue = totalValueRule;
            this.UI_AutoAcceptRule.AutoAccept = this._Data.AutoAccept == 1;
            /// Sự kiện lưu thiết lập
            this.UI_AutoAcceptRule.Save = () =>
            {
                /// Thực thi sự kiện
                this.SaveAutoAcceptRule?.Invoke(string.Format("{0}:{1}:{2}:{3}:{4}", Global.Data.RoleData.GuildID, this.UI_AutoAcceptRule.AutoAccept ? 1 : 0, this.UI_AutoAcceptRule.Level, this.UI_AutoAcceptRule.FactionID, this.UI_AutoAcceptRule.TotalValue));
            };
            /// Hiện khung
            this.UI_AutoAcceptRule.Show();
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
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Nếu không có bang
            if (Global.Data.RoleData.GuildID <= 0)
            {
                /// Đóng khung
                this.Close?.Invoke();
                return;
            }
            /// Nếu không có dữ liệu
            else if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                return;
            }

            /// UI người chơi đầu tiên
            UIGuildEx_AskToJoinList_PlayerInfo uiFirstPlayer = null;

            /// Xóa toàn bộ danh sách người chơi xin vào bang
            foreach (Transform child in this.playerListTransform.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Toác
            if (this._Data.TotalRequestJoin != null)
            {
                /// Duyệt danh sách người chơi xin vào
                foreach (RequestJoin playerInfo in this._Data.TotalRequestJoin)
                {
                    /// Tạo mới
                    UIGuildEx_AskToJoinList_PlayerInfo uiPlayer = GameObject.Instantiate<UIGuildEx_AskToJoinList_PlayerInfo>(this.UI_PlayerInfoPrefab);
                    uiPlayer.transform.SetParent(this.playerListTransform, false);
                    uiPlayer.gameObject.SetActive(true);
                    uiPlayer.Data = playerInfo;
                    uiPlayer.Select = () =>
                    {
                        /// Đánh dấu người chơi được chọn
                        this.selectedPlayer = uiPlayer.Data;
                        /// Đổ dữ liệu hiển thị
                        this.UIText_RoleName.text = this.selectedPlayer.RoleName;
                        this.UIText_RoleLevel.text = this.selectedPlayer.Level.ToString();
                        this.UIText_RoleFaction.text = KTGlobal.GetFactionName(this.selectedPlayer.RoleFactionID, out Color color);
                        this.UIText_RoleFaction.color = color;
                        this.UIText_RoleTotalValues.text = this.selectedPlayer.RoleValue.ToString();
                    };
                    /// Nếu chưa chọn người chơi đầu tiên
                    if (uiFirstPlayer == null)
                    {
                        /// Đánh dấu
                        uiFirstPlayer = uiPlayer;
                        /// Thực hiện chọn thằng đầu tiên
                        uiFirstPlayer.Active = true;
                    }
                }
            }
            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.playerListTransform);
            });

            /// Hiện số trang
            this.UIText_Pagination_CurrentPage.text = string.Format("{0}/{1}", this._Data.PageIndex, this._Data.TotalPage);

            /// Button phân trang
            this.UIButton_Pagination_PreviousPage.Enable = this._Data.PageIndex > 1;
            this.UIButton_Pagination_NextPage.Enable = this._Data.PageIndex < this._Data.TotalPage;

            /// Button phê duyệt / từ chối
            this.UIButton_Accept.gameObject.SetActive((Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster) && uiFirstPlayer != null);
            this.UIButton_Reject.gameObject.SetActive((Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster) && uiFirstPlayer != null);

            /// Button mở khung thiết lập tự phê duyệt người chơi vào bang
            this.UIButton_OpenAutoAcceptSetting.gameObject.SetActive(Global.Data.RoleData.GuildRank == (int) GuildRank.Master || Global.Data.RoleData.GuildRank == (int) GuildRank.ViceMaster);
        
            /// Nếu tự động duyệt vào bang hội
            if (this._Data.AutoAccept == 1)
            {
                /// Chuỗi mã hóa thông tin
                string[] autoAcceptRules = this._Data.AutoAcceptRule.Split('|');
                /// Giải mã dữ liệu
                int.TryParse(autoAcceptRules[0], out int levelRule);
                int.TryParse(autoAcceptRules[1], out int factionIDRule);
                long.TryParse(autoAcceptRules[2], out long totalValueRule);
                string factionName = KTGlobal.GetFactionName(factionIDRule, out Color color);
                string htmlColor = ColorUtility.ToHtmlStringRGB(color);
                /// Tự động duyệt vào bang hội
                this.UIText_AutoAcceptState.text = string.Format("<color=orange>Tự động phê duyệt</color> người chơi đạt điều kiện:\n   - Cấp độ: <color=#64f766>{0} trở lên</color>\n   - Môn phái: <color=#{1}>{2}</color>\n   - Tài phú: <color=#f7f264>{3} trở lên</color>", levelRule, htmlColor, factionName, totalValueRule);
            }
            /// Nếu không tự động duyệt vào bang hội
            else
            {
                /// Tự động duyệt vào bang hội
                this.UIText_AutoAcceptState.text = "<color=orange>Tự động phê duyệt</color> người chơi: <color=#f53838>Không kích hoạt</color>";
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới hiển thị tự động duyệt người chơi vào bang
        /// </summary>
        public void RefreshAutoAcceptRules()
        {
            /// Nếu không có bang
            if (Global.Data.RoleData.GuildID <= 0)
            {
                return;
            }
            /// Nếu không có dữ liệu
            else if (this._Data == null)
            {
                return;
            }

            /// Nếu tự động duyệt vào bang hội
            if (this._Data.AutoAccept == 1)
            {
                /// Chuỗi mã hóa thông tin
                string[] autoAcceptRules = this._Data.AutoAcceptRule.Split('|');
                /// Giải mã dữ liệu
                int.TryParse(autoAcceptRules[0], out int levelRule);
                int.TryParse(autoAcceptRules[1], out int factionIDRule);
                long.TryParse(autoAcceptRules[2], out long totalValueRule);
                string factionName = KTGlobal.GetFactionName(factionIDRule, out Color color);
                string htmlColor = ColorUtility.ToHtmlStringRGB(color);
                /// Tự động duyệt vào bang hội
                this.UIText_AutoAcceptState.text = string.Format("<color=orange>Tự động phê duyệt</color> người chơi đạt điều kiện:\n   - Cấp độ: <color=#64f766>{0} trở lên</color>\n   - Môn phái: <color=#{1}>{2}</color>\n   - Tài phú: <color=#f7f264>{3} trở lên</color>", levelRule, htmlColor, factionName, totalValueRule);
            }
            /// Nếu không tự động duyệt vào bang hội
            else
            {
                /// Tự động duyệt vào bang hội
                this.UIText_AutoAcceptState.text = "<color=orange>Tự động phê duyệt</color> người chơi: <color=#f53838>Không kích hoạt</color>";
            }
        }
        #endregion
    }
}
