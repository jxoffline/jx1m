using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.Mail;
using FS.VLTK.UI.Main.ItemBox;
using System.Collections;
using FS.GameEngine.Logic;
using Server.Data;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung hộp thư người chơi
    /// </summary>
    public class UIMailBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab thư
        /// </summary>
        [SerializeField]
        private UIMailBox_Mail UI_MailPrefab;

        /// <summary>
        /// Text tiêu đề thư
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MailTitle;

        /// <summary>
        /// Text tên người gửi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SenderName;

        /// <summary>
        /// Text nội dung thư
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MailContent;

        /// <summary>
        /// Text KNB khóa đính kèm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Token;

        /// <summary>
        /// Text bạc khóa đính kèm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Money;

        /// <summary>
        /// Prefab vật phẩm đính kèm
        /// </summary>
        [SerializeField]
        private UIItemBox UI_StickItemPrefab;

        /// <summary>
        /// Button xóa thư
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_DeleteMail;

        /// <summary>
        /// Button lấy tất cả vật phẩm và tiền đính kèm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GetAll;

        /// <summary>
        /// Đánh dấu đây là bạc thường
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_Money;

        /// <summary>
        /// Đánh dấu đây là bạc khóa
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_BoundMoney;

        /// <summary>
        /// Đánh dấu đây là KNB thường
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_Token;

        /// <summary>
        /// Đánh dấu đây là KNB khóa
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_BoundToken;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách thư
        /// </summary>
        private RectTransform transformMailList = null;

        /// <summary>
        /// RectTransform nội dung thư
        /// </summary>
        private RectTransform transformMailContent = null;

        /// <summary>
        /// RectTransform vật phẩm đính kèm
        /// </summary>
        private RectTransform transformStickItemList = null;

        /// <summary>
        /// Thư đang đọc
        /// </summary>
        private MailData currentMailData = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện xóa thư
        /// </summary>
        public Action<MailData> DeleteMail { get; set; }

        /// <summary>
        /// Sự kiện lấy toàn bộ vật phẩm đính kèm và tiền
        /// </summary>
        public Action<MailData> GetAllStickItemsAndMoney { get; set; }

        /// <summary>
        /// Sự kiện đọc thư
        /// </summary>
        public Action<MailData> ReadMail { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformMailList = this.UI_MailPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformMailContent = this.UIText_MailContent.transform.parent.GetComponent<RectTransform>();
            this.transformStickItemList = this.UI_StickItemPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_DeleteMail.onClick.AddListener(this.ButtonDeleteMail_Clicked);
            this.UIButton_GetAll.onClick.AddListener(this.ButtonGetAll_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            /// Thực thi sự kiện đóng khung
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button xóa thư được ấn
        /// </summary>
        private void ButtonDeleteMail_Clicked()
        {
            /// Nếu không có thư
            if (this.currentMailData == null)
            {
                return;
            }

            /// Thực thi sự kiện xóa thư
            this.DeleteMail?.Invoke(this.currentMailData);
        }

        /// <summary>
        /// Sự kiện khi Button lấy tất cả vật phẩm và tiền đính kèm được ấn
        /// </summary>
        private void ButtonGetAll_Clicked()
        {
            /// Nếu không có thư
            if (this.currentMailData == null)
            {
                return;
            }
            /// Nếu thư không có gì để nhận
            else if (this.currentMailData.HasFetchAttachment != 1)
            {
                return;
            }

            /// Thực thi sự kiện lấy tất cả vật phẩm và tiền đính kèm được ấn
            this.GetAllStickItemsAndMoney?.Invoke(this.currentMailData);
        }

        /// <summary>
        /// Sự kiện khi Button thư được ấn
        /// </summary>
        /// <param name="mailData"></param>
        private void ButtonMail_Clicked(MailData mailData)
        {
            /// Nếu không có thư
            if (mailData == null)
            {
                return;
            }

            /// Thực thi sự kiện đọc thư
            this.ReadMail?.Invoke(mailData);
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
        /// <param name="transform"></param>
        private void RebuildLayout(RectTransform transform)
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách thư
        /// </summary>
        private void ClearAllMails()
        {
            foreach (Transform child in this.transformMailList.transform)
            {
                if (child.gameObject != this.UI_MailPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildLayout(this.transformMailList);
        }

        /// <summary>
        /// Hủy kích hoạt toàn bộ sách thư
        /// </summary>
        private void RemoveActiveForAllMails()
        {
            foreach (Transform child in this.transformMailList.transform)
            {
                if (child.gameObject != this.UI_MailPrefab.gameObject)
                {
                    UIMailBox_Mail uiMailBox = child.GetComponent<UIMailBox_Mail>();
                    uiMailBox.Active = false;
                }
            }
        }

        /// <summary>
        /// Làm rỗng danh sách vật phẩm đính kèm
        /// </summary>
        private void ClearStickItems()
        {
            foreach (Transform child in this.transformStickItemList.transform)
            {
                if (child.gameObject != this.UI_StickItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildLayout(this.transformStickItemList);
        }

        /// <summary>
        /// Thêm thư vào danh sách
        /// </summary>
        /// <param name="data"></param>
        private void AddMailItem(MailData data)
        {
            UIMailBox_Mail uiMail = GameObject.Instantiate<UIMailBox_Mail>(this.UI_MailPrefab);
            uiMail.transform.SetParent(this.transformMailList, false);
            uiMail.gameObject.SetActive(true);

            uiMail.Data = data;
            uiMail.Selected = () => {
                this.ButtonMail_Clicked(data);
            };
        }

        /// <summary>
        /// Thêm vật phẩm đính kèm trong thư
        /// </summary>
        /// <param name="itemGD"></param>
        public void AddStickItem(GoodsData itemGD)
        {
            UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_StickItemPrefab);
            uiItemBox.transform.SetParent(this.transformStickItemList, false);
            uiItemBox.gameObject.SetActive(true);

            uiItemBox.Data = itemGD;
        }

        /// <summary>
        /// Tìm UI thư có ID tương ứng
        /// </summary>
        /// <param name="mailID"></param>
        /// <returns></returns>
        private UIMailBox_Mail FindMailItem(int mailID)
        {
            foreach (Transform child in this.transformMailList.transform)
            {
                if (child.gameObject != this.UI_MailPrefab.gameObject)
                {
                    UIMailBox_Mail uiMailBox = child.GetComponent<UIMailBox_Mail>();
                    if (uiMailBox.Data.MailID == mailID)
                    {
                        return uiMailBox;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Xóa dữ liệu thư đang đọc
        /// </summary>
        private void ClearData()
        {
            this.UIText_MailTitle.text = "";
            this.UIText_SenderName.text = "";
            this.UIText_MailContent.text = "";
            this.RebuildLayout(this.transformMailContent);
            this.UIText_Token.text = "";
            this.UIText_Money.text = "";
            this.UIButton_DeleteMail.interactable = false;
            this.UIButton_GetAll.interactable = false;
            this.ClearStickItems();
        }

        /// <summary>
        /// Làm mới nội dung thư đang đọc
        /// </summary>
        private void RefreshData()
        {
            /// Nếu không có thư
            if (this.currentMailData == null)
            {
                return;
            }
            this.UIText_MailTitle.text = this.currentMailData.Subject;
            this.UIText_SenderName.text = this.currentMailData.SenderRName;
            this.UIText_MailContent.text = this.currentMailData.Content;
            this.RebuildLayout(this.transformMailContent);
            this.ClearStickItems();

            /// Có gì đính kèm không
            bool hasSomethingToTake = false;
            /// Nếu có vật phẩm đính kèm hoặc tiền
            if (this.currentMailData.HasFetchAttachment == 1)
            {
                /// Nếu có vật phẩm đính kèm
                if (this.currentMailData.GoodsList != null && this.currentMailData.GoodsList.Count > 0)
                {
                    foreach (GoodsData itemGD in this.currentMailData.GoodsList)
                    {
                        /// Thêm vào danh sách
                        this.AddStickItem(itemGD);
                    }
                    hasSomethingToTake = true;
                }
                this.RebuildLayout(this.transformStickItemList);

                /// Cập nhật số bạc
                this.UIText_Money.text = KTGlobal.GetDisplayMoney(this.currentMailData.Money);
                /// Nếu có bạc khóa
                if (this.currentMailData.Money > 0)
                {
                    hasSomethingToTake = true;
                }
                /// Cập nhật số KNB
                this.UIText_Token.text = KTGlobal.GetDisplayMoney(this.currentMailData.Token);
                /// Nếu có KNB khóa
                if (this.currentMailData.Token > 0)
                {
                    hasSomethingToTake = true;
                }

                /// Hiện Icon tương ứng
                this.UIMark_BoundMoney.gameObject.SetActive(this.currentMailData.MailType == 0);
                this.UIMark_BoundToken.gameObject.SetActive(this.currentMailData.MailType == 0);
                this.UIMark_Money.gameObject.SetActive(this.currentMailData.MailType == 1);
                this.UIMark_Token.gameObject.SetActive(this.currentMailData.MailType == 1);
            }
            /// Nếu không có vật phẩm đính kèm hoặc tiền
            else
            {
                /// Cập nhật số bạc khóa
                this.UIText_Money.text = "";
                /// Cập nhật số KNB khóa
                this.UIText_Token.text = "";
                /// Hủy toàn bộ Icon
                this.UIMark_BoundMoney.gameObject.SetActive(false);
                this.UIMark_BoundToken.gameObject.SetActive(false);
                this.UIMark_Money.gameObject.SetActive(false);
                this.UIMark_Token.gameObject.SetActive(false);
            }

            this.UIButton_GetAll.interactable = hasSomethingToTake;
            this.UIButton_DeleteMail.interactable = true;
        }
        #endregion

        #region Public fields
        /// <summary>
        /// Làm mới danh sách thư
        /// </summary>
        public void Refresh()
        {
            /// Làm rỗng danh sách thư
            this.ClearAllMails();

            /// Xóa dữ liệu thư đang đọc
            this.ClearData();

            /// Nếu danh sách thư trống
            if (Global.Data.MailDataList == null)
            {
                /// Đánh dấu không thư nào được chọn
                this.currentMailData = null;
                return;
            }

            /// Nếu thư đang xem tồn tại
            if (this.currentMailData != null)
            {
                /// Cập nhật thông tin thư hiện tại
                this.currentMailData = Global.Data.MailDataList.Where(x => x.MailID == this.currentMailData.MailID).FirstOrDefault();
            }

            /// Duyệt và xây danh sách thư
            foreach (MailData mailData in Global.Data.MailDataList)
            {
                /// Thêm thư vào danh sách
                this.AddMailItem(mailData);
            }

            /// Nếu thư đang xem tồn tại
            if (this.currentMailData != null)
            {
                /// UI thư tương ứng
                UIMailBox_Mail uiMailItem = this.FindMailItem(this.currentMailData.MailID);
                /// Nếu tồn tại
                if (uiMailItem != null)
                {
                    /// Nếu chưa được chọn
                    if (!uiMailItem.Active)
                    {
                        uiMailItem.Active = true;
                    }
                    else
                    {
                        this.ButtonMail_Clicked(this.currentMailData);
                    }
                }
            }

            /// Xây lại giao diện
            this.RebuildLayout(this.transformMailContent);
        }

        /// <summary>
        /// Cập nhật nội dung thư đang đọc
        /// </summary>
        /// <param name="mailData"></param>
        public void UpdateCurrentMailData(MailData mailData)
        {
            /// Hủy kích hoạt các đối tượng khác
            this.RemoveActiveForAllMails();

            /// Cập nhật thư đang đọc hiện tại
            this.currentMailData = mailData;

            /// UI chứa Mail tương ứng
            UIMailBox_Mail uiMailItem = this.FindMailItem(mailData.MailID);
            /// Nếu tồn tại thì cập nhật thay đổi
            if (uiMailItem != null)
            {
                uiMailItem.Data = mailData;
                uiMailItem.Active = true;
            }

            /// Làm mới dữ liệu thư đang đọc
            this.RefreshData();
        }
        #endregion
    }
}
