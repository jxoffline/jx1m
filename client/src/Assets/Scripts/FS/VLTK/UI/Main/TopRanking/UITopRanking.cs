using FS.VLTK.UI.Main.TopRanking;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung đua top
    /// </summary>
    public class UITopRanking : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab tab xếp hạng
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_RankingTabPrefab;

        /// <summary>
        /// Prefab thông tin người chơi được xếp hạng
        /// </summary>
        [SerializeField]
        private UITopRanking_PlayerInfo UI_PlayerInfoPrefab;

        /// <summary>
        /// Text giá trị
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ValueHeader;

        /// <summary>
        /// Text xếp hạng bản thân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SelfRank;

        /// <summary>
        /// Text đếm lùi thời gian nhận thưởng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CountDown;

        /// <summary>
        /// Prefab ô phần thưởng
        /// </summary>
        [SerializeField]
        private UITopRanking_AwardBox UI_AwardBoxPrefab;
        #endregion

        #region Properties
        private List<TopRankingConfig> _Data;
        /// <summary>
        /// Thông tin
        /// </summary>
        public List<TopRankingConfig> Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Đánh dấu thời điểm có dữ liệu
                this.initTicks = KTGlobal.GetCurrentTimeMilis();
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện nhận quà
        /// </summary>
        public Action<int, int> GetAward { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách tab xếp hạng
        /// </summary>
        private RectTransform transformRankingTabList;

        /// <summary>
        /// RectTransform danh sách người chơi
        /// </summary>
        private RectTransform transformPlayerList;

        /// <summary>
        /// RectTransform danh sách phần quà
        /// </summary>
        private RectTransform transformAwardBoxList;

        /// <summary>
        /// Thời điểm có dữ liệu
        /// </summary>
        private long initTicks = 0;

        /// <summary>
        /// Luồng thực hiện đếm lùi
        /// </summary>
        private Coroutine countDownCoroutine;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformRankingTabList = this.UIToggle_RankingTabPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformPlayerList = this.UI_PlayerInfoPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformAwardBoxList = this.UI_AwardBoxPrefab.transform.parent.GetComponent<RectTransform>();
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

        /// <summary>
        /// Sự kiện khi Button nhận thưởng được ấn
        /// </summary>
        /// <param name="rankType"></param>
        /// <param name="awardIndex"></param>
        private void ButtonGet_Clicked(int rankType, int awardIndex)
        {
            this.GetAward?.Invoke(rankType, awardIndex);
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
        /// Thực hiện đếm lùi
        /// </summary>
        /// <param name="data"></param>
        /// <param name="totalSec"></param>
        private void StartCountDown(TopRankingConfig data, int totalSec)
        {
            IEnumerator DoWork()
            {
                WaitForSeconds wait = new WaitForSeconds(1f);
                while (true)
                {
                    totalSec--;
                    /// Quá thời gian
                    if (totalSec < 0)
                    {
                        /// Thiết lập về 0
                        totalSec = 0;
                    }
                    /// Thiết lập Text
                    this.UIText_CountDown.text = totalSec <= 0 ? "Đã kết thúc" : KTGlobal.DisplayFullDateAndTime(totalSec);
                    /// Nếu đã hết thời gian
                    if (totalSec <= 0 && data.AwardConfig != null)
                    {
                        // if ((awardInfo.RankStart == data.SelfIndex || awardInfo.RankEnd == data.SelfIndex || (awardInfo.RankStart < data.SelfIndex && awardInfo.RankEnd > data.SelfIndex)&& data.State==3))
                        /// Thông tin thứ hạng tương ứng
                        AwardConfig awardInfo = data.AwardConfig.Where(x => x.RankStart != null && x.RankStart <= data.SelfIndex && x.RankEnd != null && x.RankEnd >= data.SelfIndex).FirstOrDefault();
                        /// Nếu tìm thấy
                        if (awardInfo != null)
                        {
                            /// Nếu chưa nhận
                            if (data.State == 3)
                            {
                                /// Duyệt danh sách tìm UI tương ứng
                                foreach (Transform child in this.transformAwardBoxList.transform)
                                {
                                    /// UI tương ứng
                                    UITopRanking_AwardBox uiAwardBox = child.GetComponent<UITopRanking_AwardBox>();
                                    /// Nếu là UI này
                                    if (uiAwardBox != null)
                                    {
                                        uiAwardBox.EnableGet = true;
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    /// Đợi 1s
                    yield return wait;
                }
                /// Xóa luồng
                this.countDownCoroutine = null;
            }
            /// Nếu đã tồn tại luồng cũ
            if (this.countDownCoroutine != null)
            {
                this.StopCoroutine(this.countDownCoroutine);
            }
            /// Chạy luồng mới
            this.StartCoroutine(DoWork());
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

            /// Xóa rỗng danh sách
            foreach (Transform child in this.transformRankingTabList.transform)
            {
                if (child.gameObject != this.UIToggle_RankingTabPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            foreach (Transform child in this.transformPlayerList.transform)
            {
                if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            foreach (Transform child in this.transformAwardBoxList.transform)
            {
                if (child.gameObject != this.UI_AwardBoxPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Xóa hết Text
            this.UIText_SelfRank.text = "";
            this.UIText_CountDown.text = "";
            this.UIText_ValueHeader.text = "";

            /// Tab đầu tiên
            UIToggleSprite uiFirstTab = null;

            /// Duyệt danh sách
            foreach (TopRankingConfig data in this._Data)
            {
                /// Tạo tab
                UIToggleSprite uiTabInfo = GameObject.Instantiate<UIToggleSprite>(this.UIToggle_RankingTabPrefab);
                uiTabInfo.transform.SetParent(this.transformRankingTabList, false);
                uiTabInfo.gameObject.SetActive(true);
                uiTabInfo.Name = data.RankName;
                uiTabInfo.OnSelected = (isSelected) =>
                {
                    /// Nếu không được chọn
                    if (!isSelected)
                    {
                        /// Bỏ qua
                        return;
                    }

                    /// Xóa rỗng danh sách
                    foreach (Transform child in this.transformPlayerList.transform)
                    {
                        if (child.gameObject != this.UI_PlayerInfoPrefab.gameObject)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                    }
                    foreach (Transform child in this.transformAwardBoxList.transform)
                    {
                        if (child.gameObject != this.UI_AwardBoxPrefab.gameObject)
                        {
                            GameObject.Destroy(child.gameObject);
                        }
                    }

                    /// Header
                    this.UIText_ValueHeader.text = data.RankName;
                    /// Tổng số giây từ thời điểm tạo đến hiện tại
                    int totalSec = (int) ((KTGlobal.GetCurrentTimeMilis() - this.initTicks) / 1000);
                    /// Đếm lùi
                    this.StartCountDown(data, data.TimeLess - totalSec);
                    /// Thứ hạng cá nhân
                    this.UIText_SelfRank.text = string.Format("{0} {1}", data.SelfIndex < 0 ? "Không xếp hạng" : data.SelfIndex.ToString(), data.State == 2 ? "(Đã nhận)" : "");

                    /// Nếu tồn tại danh sách người chơi
                    if (data.ListPlayer != null)
                    {
                        /// Duyệt danh sách người chơi
                        foreach (PlayerRanking playerInfo in data.ListPlayer)
                        {
                            /// Tạo mới
                            UITopRanking_PlayerInfo uiPlayerInfo = GameObject.Instantiate<UITopRanking_PlayerInfo>(this.UI_PlayerInfoPrefab);
                            uiPlayerInfo.transform.SetParent(this.transformPlayerList, false);
                            uiPlayerInfo.gameObject.SetActive(true);
                            uiPlayerInfo.Rank = playerInfo.ID;
                            uiPlayerInfo.Name = playerInfo.RoleName;
                            uiPlayerInfo.FactionID = playerInfo.FactionID;
                            uiPlayerInfo.Value = playerInfo.Value;
                        }
                    }
                    
                    /// Nếu tồn tại danh sách phần quà
                    if (data.AwardConfig != null)
                    {
                        /// Duyệt danh sách phần thưởng
                        foreach (AwardConfig awardInfo in data.AwardConfig)
                        {
                            /// Tạo mới
                            UITopRanking_AwardBox uiAwardInfo = GameObject.Instantiate<UITopRanking_AwardBox>(this.UI_AwardBoxPrefab);
                            uiAwardInfo.transform.SetParent(this.transformAwardBoxList, false);
                            uiAwardInfo.gameObject.SetActive(true);
                            uiAwardInfo.Data = awardInfo;


                            if ((awardInfo.RankStart == data.SelfIndex || awardInfo.RankEnd == data.SelfIndex || (awardInfo.RankStart < data.SelfIndex && awardInfo.RankEnd > data.SelfIndex)) && data.State==3)
                            {
                                uiAwardInfo.EnableGet = true;
                            }
                            else
                            {
                                uiAwardInfo.EnableGet = false;
                            }
                           
                            uiAwardInfo.Get = () =>
                            {
                                this.ButtonGet_Clicked(data.RankType, awardInfo.Index);
                            };
                        }
                    }

                    /// Xây lại giao diện
                    this.ExecuteSkipFrames(1, () =>
                    {
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformPlayerList);
                        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformAwardBoxList);
                    });
                };

                /// Nếu chưa có Tab đầu tiên
                if (uiFirstTab == null)
                {
                    /// Đánh dấu
                    uiFirstTab = uiTabInfo;
                    /// Kích hoạt
                    uiTabInfo.Active = true;
                }
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformRankingTabList);
            });
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật trạng thái
        /// </summary>
        /// <param name="rankType"></param>
        /// <param name="awardIndex"></param>
        public void UpdateState(int rankType, int awardIndex)
        {
            /// Dữ liệu
            TopRankingConfig data = this.Data.Where(x => x.RankType == rankType).FirstOrDefault();
            /// Không tồn tại thì thôi
            if (data == null)
            {
                return;
            }
            /// Thông tin phần quà
            AwardConfig awardInfo = data.AwardConfig.Where(x => x.Index == awardIndex).FirstOrDefault();
            /// Không tòn tại thì thôi
            if (awardInfo == null)
            {
                return;
            }
            /// Cập nhật trạng thái thành đã nhận rồi
            data.State = 2;

            /// Duyệt danh sách tìm UI tương ứng
            foreach (Transform child in this.transformAwardBoxList.transform)
            {
                /// UI tương ứng
                UITopRanking_AwardBox uiAwardBox = child.GetComponent<UITopRanking_AwardBox>();
                /// Nếu là UI này
                if (uiAwardBox != null && uiAwardBox.Data == awardInfo)
                {
                    uiAwardBox.EnableGet = true;
                    break;
                }
            }
        }
        #endregion
    }
}
