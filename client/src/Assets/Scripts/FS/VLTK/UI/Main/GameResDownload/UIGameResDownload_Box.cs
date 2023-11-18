using FS.VLTK.Entities;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.Threading;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GameResDownload
{
    /// <summary>
    /// Khung tải ngầm bản đồ
    /// </summary>
    public class UIGameResDownload_Box : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text thông tin tải
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_DownloadInfo;

        /// <summary>
        /// Button tải
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Download;

        /// <summary>
        /// Button nhận quà
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GetAward;

        /// <summary>
        /// Button tải sau
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_DownloadLater;

        /// <summary>
        /// Slider tiến độ tải
        /// </summary>
        [SerializeField]
        private UISliderText UISlider_DownloadProgress;

        /// <summary>
        /// Text số File đã tải được
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_DownloadedFiles;

        /// <summary>
        /// Prefab vật phẩm thưởng
        /// </summary>
        [SerializeField]
        private UIItemBox UI_ItemPrefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách phần quà
        /// </summary>
        private RectTransform transformAwardsList = null;

        /// <summary>
        /// Tổng số Byte cần tải
        /// </summary>
        private long TotalNeedToDownloadBytes;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện tải
        /// </summary>
        public Action Download { get; set; }

        /// <summary>
        /// Sự kiện nhận quà
        /// </summary>
        public Action GetAwards { get; set; }

        /// <summary>
        /// Dữ liệu
        /// </summary>
        public BonusDownload Data { get; set; }

        /// <summary>
        /// Danh sách File cần tải xuống
        /// </summary>
        public List<UpdateZipFile> NeedDownloadFiles { get; set; }

        /// <summary>
        /// Đã hoàn thành chưa
        /// </summary>
        public bool Completed
        {
            get
            {
                return this.UIButton_GetAward.interactable;
            }
            set
            {
                this.UIButton_GetAward.interactable = value;
            }
        }

        /// <summary>
        /// Hiển thị khung
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformAwardsList = this.UI_ItemPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_Download.onClick.AddListener(this.ButtonDownload_Clicked);
            this.UIButton_DownloadLater.onClick.AddListener(this.ButtonDownloadLater_Clicked);
            this.UIButton_GetAward.onClick.AddListener(this.ButtonGetAwards_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button tải được ấn
        /// </summary>
        private void ButtonDownload_Clicked()
        {
            /// Nếu không có gì để tải
            if (this.NeedDownloadFiles == null || this.NeedDownloadFiles.Count <= 0)
            {
                KTGlobal.AddNotification("Không có gì để tải!");
                return;
            }

            /// Đóng tương tác Button
            this.UIButton_Download.gameObject.SetActive(false);
            this.UIButton_DownloadLater.gameObject.SetActive(false);
            this.UIButton_GetAward.gameObject.SetActive(true);
            this.UIButton_GetAward.interactable = false;

            /// Thực thi sự kiện Download
            this.Download?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button tải sau được ấn
        /// </summary>
        private void ButtonDownloadLater_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button nhận quà được ấn
        /// </summary>
        private void ButtonGetAwards_Clicked()
        {
            /// Nếu không có gì để nhận
            if (this.Data == null || !this.Data.CanRevice)
            {
                KTGlobal.AddNotification("Không có gì để nhận!");
                return;
            }
            /// Nếu tải chưa hoàn tất
            else if (!this.Completed)
            {
                KTGlobal.AddNotification("Quá trình tải xuống chưa hoàn tất, không thể nhận quà!");
                return;
            }
            this.GetAwards?.Invoke();
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

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformAwardsList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách vật phẩm thưởng
        /// </summary>
        private void ClearAwardsList()
        {
            foreach (Transform child in this.transformAwardsList.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm vật phẩm vào danh sách thưởng
        /// </summary>
        /// <param name="itemGD"></param>
        private void AddAwardItem(GoodsData itemGD)
        {
            UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_ItemPrefab);
            uiItemBox.transform.SetParent(this.transformAwardsList, false);
            uiItemBox.gameObject.SetActive(true);
            uiItemBox.Data = itemGD;
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Làm mới giao diện
        /// </summary>
        public void Refresh()
        {
            this.ClearAwardsList();

            /// Đóng các Button chức năng
            this.UIButton_Download.gameObject.SetActive(false);
            this.UIButton_DownloadLater.gameObject.SetActive(false);
            this.UIButton_GetAward.gameObject.SetActive(false);

            /// Nếu dữ liệu rỗng
            if (this.Data == null)
            {
                this.UIText_DownloadInfo.text = "Thông tin dữ liệu bị lỗi. Hãy liên hệ với hỗ trợ để được trợ giúp!";
                return;
            }

            /// Duyệt danh sách và thêm vật phẩm vào
            foreach (BonusItem awardInfo in this.Data.BonusItems)
            {
                /// Nếu vật phẩm tồn tại
                if (Loader.Loader.Items.TryGetValue(awardInfo.ItemID, out ItemData itemData))
                {
                    /// Tạo mới vật phẩm
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    itemGD.Binding = 1;
                    itemGD.GCount = awardInfo.Number;
                    /// Thêm vật phẩm vào danh sách
                    this.AddAwardItem(itemGD);
                }
            }

            /// Reset tổng số Bytes cần tải
            this.TotalNeedToDownloadBytes = 0;
            /// Nếu có dữ liệu cần tải xuống
            if (this.NeedDownloadFiles != null && this.NeedDownloadFiles.Count > 0)
            {
                /// Cập nhật tổng số Bytes cần tải
                foreach (UpdateZipFile file in this.NeedDownloadFiles)
                {
                    this.TotalNeedToDownloadBytes += (long) file.FileSize;
                }

                this.UIButton_Download.gameObject.SetActive(true);
                this.UIButton_DownloadLater.gameObject.SetActive(true);
                /// Cập nhật số File đã tải
                this.UIText_DownloadedFiles.text = string.Format("Tổng số đã tải: {0}/{1} ({2}/{3})", 0, this.NeedDownloadFiles.Count, 0, KTGlobal.BytesToString(this.TotalNeedToDownloadBytes));
                /// Đánh dấu tiến độ Slider về 0%
                this.UISlider_DownloadProgress.Value = 0;
                /// Nếu có phần quà nhận
                if (this.Data.CanRevice)
                {
                    this.UIText_DownloadInfo.text = "Để trải nghiệm tốt hơn, bạn cần tải đủ <color=yellow>tài nguyên</color>. Sau khi <color=orange>tải xong</color> sẽ nhận được <color=green>phần quà nhỏ</color>, với <color=yellow>mỗi nhân vật</color> chỉ được nhận <color=green>một lần</color> duy nhất.";
                }
                /// Nếu không có phân quà nhận
                else
                {
                    this.UIText_DownloadInfo.text = "Bạn <color=yellow>đã nhận quà</color> trước đó, <color=green>không thể</color> nhận thêm phần quà nữa!";
                }
            }
            /// Nếu không có dữ liệu cần tải xuống
            else
            {
                /// Đánh dấu Slider tải về 100%
                this.UISlider_DownloadProgress.Value = 100;
                /// Cập nhật số File đã tải
                this.UIText_DownloadedFiles.text = "Đã tải hoàn tất";
                /// Nếu có phần quà nhận
                if (this.Data.CanRevice)
                {
                    this.UIText_DownloadInfo.text = "Tải xuống <color=green>hoàn tất</color>, bạn có thể <color=orange>nhận được</color> các <color=yellow>phần quà</color> bên dưới. Ấn <color=orange>Nhận</color> để nhận quà.";
                    /// Hiện Button nhận quà
                    this.UIButton_GetAward.gameObject.SetActive(true);
                }
                /// Nếu không có phân quà nhận
                else
                {
                    this.UIText_DownloadInfo.text = "Bạn <color=yellow>đã nhận quà</color> trước đó, <color=green>không thể</color> nhận thêm phần quà nữa!";
                    /// Hiện Button nhận quà
                    this.UIButton_GetAward.gameObject.SetActive(true);
                }
                /// Đánh dấu Button tương tác được hay không
                this.UIButton_GetAward.interactable = this.Data.CanRevice;

                /// Nếu đã nhận rồi
                if (!this.Data.CanRevice)
                {
                    /// Thông báo tự đóng khung sau 5s
                    KTGlobal.AddNotification("Tải hoàn tất, tự động đóng khung sau 5 giây!");
                    /// Thiết lập đóng khung
                    KTTimerManager.Instance.SetTimeout(5f, () => {
                        PlayZone.Instance.CloseGameResDownload();
                    });
                }
            }


            this.RebuildLayout();
        }

        /// <summary>
        /// Cập nhật tổng số File đã tải
        /// </summary>
        /// <param name="totalDownloadedFiles"></param>
        /// <param name="totalDownloadedBytes"></param>
        public void UpdateTotalDownloadedFiles(int totalDownloadedFiles, long totalDownloadedBytes)
        {
            /// Cập nhật số File đã tải
            this.UIText_DownloadedFiles.text = string.Format("Tổng số đã tải: {0}/{1} ({2}/{3})", totalDownloadedFiles, this.NeedDownloadFiles.Count, KTGlobal.BytesToString(totalDownloadedBytes), KTGlobal.BytesToString(this.TotalNeedToDownloadBytes));
        }

        /// <summary>
        /// Cập nhật tiến độ tải
        /// </summary>
        /// <param name="percent"></param>
        public void UpdateDownloadProgress(int percent)
        {
            this.UISlider_DownloadProgress.Value = percent;
        }
        #endregion
    }
}
