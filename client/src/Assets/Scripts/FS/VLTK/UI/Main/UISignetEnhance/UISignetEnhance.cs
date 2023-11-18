using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using FS.VLTK.UI.Main.Bag;
using Server.Data;
using System.Collections;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung cường hóa ngũ hành ấn
    /// </summary>
    public class UISignetEnhance : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Ô đặt Ngũ hành ấn
        /// </summary>
        [SerializeField]
        private UIItemBox UI_SignetBox;

        /// <summary>
        /// Text tên thuộc tính
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AttributeName;

        /// <summary>
        /// Text giá trị thuộc tính
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AttributeValue;

        /// <summary>
        /// Toggle cường hóa ngũ hành tương khắc
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_SeriesEnhance;

        /// <summary>
        /// Toggle nhược hóa ngũ hành tương khắc
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_SeiresConque;

        /// <summary>
        /// Prefab ô đặt Ngũ Hành Hồn Thạch
        /// </summary>
        [SerializeField]
        private UIItemBox UI_FiveElementsStonePrefab;

        /// <summary>
        /// Lưới danh sách vật phẩm
        /// </summary>
        [SerializeField]
        private UIBag_Grid UIBag_Grid;

        /// <summary>
        /// Button cường hóa
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Enhance;

        /// <summary>
        /// Hiệu ứng cường hóa trang bị thành công
        /// </summary>
        [SerializeField]
        private UIAnimatedImage UIImage_EnhanceSuccessEffect;
        #endregion

        #region Constants
        /// <summary>
        /// Số vị trí tối đa có thể đặt vào Ngũ hành hồn thạch
        /// </summary>
        private const int MaxFSSlot = 15;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách Ngũ hành hồn thạch đặt vào
        /// </summary>
        private RectTransform transformFSList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện cường hóa
        /// </summary>
        public Action<GoodsData, List<GoodsData>, int> Enhance { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformFSList = this.UI_FiveElementsStonePrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.MakeDefaultSlot();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Enhance.onClick.AddListener(this.ButtonEnhance_Clicked);
            this.UIToggle_SeriesEnhance.OnSelected = (isSelected) => {
                if (isSelected)
                {
                    this.UIText_AttributeName.text = "Cường hóa ngũ hành tương khắc";
                    this.CalculateSignetEnhance();
                }
            };
            this.UIToggle_SeiresConque.OnSelected = (isSelected) => {
                if (isSelected)
                {
                    this.UIText_AttributeName.text = "Nhược hóa ngũ hành tương khắc";
                    this.CalculateSignetEnhance();
                }
            };
            this.UIToggle_SeriesEnhance.Active = true;
            this.UIText_AttributeName.text = "Cường hóa ngũ hành tương khắc";
            this.UIText_AttributeValue.text = "";
            this.UIBag_Grid.BagItemClicked = this.ButtonBagItem_Clicked;
            this.UI_SignetBox.Click = this.ButtonSignet_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button cường hóa được ấn
        /// </summary>
        private void ButtonEnhance_Clicked()
        {
            /// Nếu chưa đặt vào ngũ hành ấn
            if (this.UI_SignetBox.Data == null)
            {
                KTGlobal.AddNotification("Hãy đặt vào Ngũ Hành Ấn!");
                return;
            }
            /// Nếu chưa đặt vào Ngũ hành hồn thạch
            if (this.GetTotalFiveElementsStone() <= 0)
            {
                KTGlobal.AddNotification("Hãy đặt vào Ngũ Hành Hồn Thạch!");
                return;
            }

            /// Danh sách Ngũ hành hồn thạch đặt vào
            List<GoodsData> fsList = new List<GoodsData>();
            /// Duyệt danh sách
            foreach (Transform child in this.transformFSList.transform)
            {
                if (child.gameObject != this.UI_FiveElementsStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    /// Nếu vị trí trống
                    if (uiItemBox.Data != null)
                    {
                        fsList.Add(uiItemBox.Data);
                    }
                }
            }

            /// Thực thi sự kiện cường hóa
            this.Enhance?.Invoke(this.UI_SignetBox.Data, fsList, this.UIToggle_SeriesEnhance.Active ? 0 : 1);
        }

        /// <summary>
        /// Sự kiện khi Button trang bị đang cường hóa được ấn
        /// </summary>
        private void ButtonSignet_Clicked()
        {
            /// Nếu chưa đặt vào ngũ hành ấn
            if (this.UI_SignetBox.Data == null)
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () => {
                this.UIBag_Grid.AddItem(this.UI_SignetBox.Data);
                this.UI_SignetBox.Data = null;
                KTGlobal.CloseItemInfo();

                /// Tính toán hệ số cường hóa ngũ hành ấn
                this.CalculateSignetEnhance();
            }));

            KTGlobal.ShowItemInfo(this.UI_SignetBox.Data, buttons);
        }

        /// <summary>
        /// Sự kiện khi Button ô Ngũ hành hồn thạch được ấn
        /// </summary>
        /// <param name="fsSlot"></param>
        private void ButtonFS_Clicked(UIItemBox fsSlot)
        {
            if (fsSlot == null)
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () => {
                this.UIBag_Grid.AddItem(fsSlot.Data);
                fsSlot.Data = null;
                KTGlobal.CloseItemInfo();

                /// Tính toán hệ số cường hóa ngũ hành ấn
                this.CalculateSignetEnhance();
            }));

            KTGlobal.ShowItemInfo(fsSlot.Data, buttons);
        }

        /// <summary>
        /// Sự kiện khi Button trong lưới vật phẩm túi đồ được ấn
        /// </summary>
        /// <param name="itemGD"></param>
        private void ButtonBagItem_Clicked(GoodsData itemGD)
        {
            if (itemGD == null)
            {
                return;
            }
            /// Nếu là Ngũ Hành Hồn Thạch
            if (KTGlobal.FiveElementStoneID == itemGD.GoodsID)
            {
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
                    this.UIBag_Grid.RemoveItem(itemGD);
                    UIItemBox fsSlot = this.FindEmptySlot();
                    if (fsSlot == null)
                    {
                        KTGlobal.AddNotification("Danh sách Ngũ Hành Hồn Thạch đặt vào đã đầy, không thể đặt thêm vào!");
                        return;
                    }
                    fsSlot.Data = itemGD;
                    KTGlobal.CloseItemInfo();

                    /// Tính toán hệ số cường hóa ngũ hành ấn
                    this.CalculateSignetEnhance();
                }));

                KTGlobal.ShowItemInfo(itemGD, buttons);
            }
            /// Nếu là trang bị
            else if (KTGlobal.IsEquip(itemGD.GoodsID))
            {
                /// TODO kiểm tra trang bị có cường hóa được không
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
                    /// Nếu vị trí đã có trang bị thì yêu cầu gỡ xuống
                    if (this.UI_SignetBox.Data != null)
                    {
                        KTGlobal.AddNotification("Hãy gỡ Ngũ Hành Ấn hiện tại xuống trước!");
                        return;
                    }

                    /// ItemID trang bị
                    if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData equipItemData))
                    {
                        KTGlobal.AddNotification("Vật phẩm không tồn tại!");
                        return;
                    }

                    /// Nếu trang bị đặt vào không phải ngũ hành ấn
                    if (!KTGlobal.IsSignet(itemGD.GoodsID))
                    {
                        KTGlobal.AddNotification("Chỉ có thể cường hóa Ngũ Hành Ấn!");
                        return;
                    }

                    this.UIBag_Grid.RemoveItem(itemGD);
                    this.UI_SignetBox.Data = itemGD;
                    KTGlobal.CloseItemInfo();

                    /// Tính toán hệ số cường hóa ngũ hành ấn
                    this.CalculateSignetEnhance();
                }));

                KTGlobal.ShowItemInfo(itemGD, buttons);
            }
            else
            {
                KTGlobal.ShowItemInfo(itemGD);
            }
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
        private void RebuildLayout()
        {
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            /// Thực hiện xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformFSList);
            }));
        }

        /// <summary>
        /// Tạo mặc định các vị trí đặt Ngũ hành hồn thạch
        /// </summary>
        private void MakeDefaultSlot()
        {
            for (int i = 1; i <= UISignetEnhance.MaxFSSlot; i++)
            {
                UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_FiveElementsStonePrefab);
                uiItemBox.transform.SetParent(this.transformFSList, false);
                uiItemBox.gameObject.SetActive(true);
                uiItemBox.Data = null;
                uiItemBox.Click = () => {
                    this.ButtonFS_Clicked(uiItemBox);
                };
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Làm rỗng danh sách Ngũ hành hồn thạch
        /// </summary>
        private void ClearSlot()
        {
            /// Duyệt danh sách
            foreach (Transform child in this.transformFSList.transform)
            {
                if (child.gameObject != this.UI_FiveElementsStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    /// Làm rỗng dữ liệu
                    uiItemBox.Data = null;
                }
            }
        }

        /// <summary>
        /// Tìm một vị trí trống trong lưới đặt Ngũ hành hồn thạch
        /// </summary>
        /// <returns></returns>
        private UIItemBox FindEmptySlot()
        {
            /// Duyệt danh sách
            foreach (Transform child in this.transformFSList.transform)
            {
                if (child.gameObject != this.UI_FiveElementsStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    /// Nếu vị trí trống
                    if (uiItemBox.Data == null)
                    {
                        return uiItemBox;
                    }
                }
            }
            /// Không tìm thấy kết quả
            return null;
        }

        /// <summary>
        /// Trả về tổng số Ngũ hành hồn thạch đã đặt vào
        /// </summary>
        /// <returns></returns>
        private int GetTotalFiveElementsStone()
        {
            /// Tổng số
            int count = 0;
            /// Duyệt danh sách
            foreach (Transform child in this.transformFSList.transform)
            {
                if (child.gameObject != this.UI_FiveElementsStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    /// Nếu vị trí trống
                    if (uiItemBox.Data != null)
                    {
                        count += uiItemBox.Data.GCount;
                    }
                }
            }
            /// Trả về kết quả
            return count;
        }

        /// <summary>
        /// Tính toán giá trị cường hóa Ngũ Hành Ấn
        /// </summary>
        private void CalculateSignetEnhance()
        {
            /// Thiết lập mặc định giá trị
            this.UIText_AttributeValue.text = "";

            /// Nếu chưa đặt vào Ngũ Hành Ấn
            if (this.UI_SignetBox.Data == null)
            {
                return;
            }
            /// Ngũ hành ấn tương ứng
            GoodsData itemGD = this.UI_SignetBox.Data;
            /// ItemID trang bị
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData equipItemData))
            {
                return;
            }
            /// Nếu trang bị đặt vào không phải ngũ hành ấn
            else if (!KTGlobal.IsSignet(itemGD.GoodsID))
            {
                return;
            }

            /// Thông tin Ngũ hành Ấn đã đặt vào
            //if (this.GetSignetInfo(this.UIToggle_SeriesEnhance.Active ? 0 : 1, out int level, out int exp))
            //{
            //    /// Exp tối đa cấp hiện tại
            //    int currentMaxExp = Loader.Loader.SignetExps[level].UpgardeExp;
            //    /// Thông tin cường hóa
            //    KTGlobal.CalculateSignetEnhance(this.GetTotalFiveElementsStone(), level, exp, out int nextLevel, out int nextExp);
            //    /// Exp tối đa cấp kế
            //    int nextMaxExp = Loader.Loader.SignetExps[nextLevel].UpgardeExp;
            //    this.UIText_AttributeValue.text = string.Format("+{0} ({1}/{2}) => <color=#ffe224>+{3} ({4}/{5})</color>", level, exp, currentMaxExp, nextLevel, nextExp, nextMaxExp);
            //}
        }

        /// <summary>
        /// Trả về thông tin Ngũ Hành Ấn đã đặt vào
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        private bool GetSignetInfo(int type, out int level, out int exp)
        {
            /// Thiết lập giá trị mặc định
            level = 0;
            exp = 0;
            /// Nếu chưa đặt vào Ngũ Hành Ấn
            if (this.UI_SignetBox.Data == null)
            {
                return false;
            }
            /// Ngũ hành ấn tương ứng
            GoodsData itemGD = this.UI_SignetBox.Data;
            /// ItemID trang bị
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData equipItemData))
            {
                return false;
            }
            /// Nếu trang bị đặt vào không phải ngũ hành ấn
            else if (!KTGlobal.IsSignet(itemGD.GoodsID))
            {
                return false;
            }

            try
            {
                /// Nếu là cường hóa ngũ hành tương khắc
                if (type == 0)
                {
                    string[] param = itemGD.OtherParams[Entities.Enum.ItemPramenter.Pram_1].Split('|');
                    level = int.Parse(param[0]);
                    exp = int.Parse(param[1]);
                }
                /// Nếu là nhược hóa ngũ hành tương khắc
                else
                {
                    string[] param = itemGD.OtherParams[Entities.Enum.ItemPramenter.Pram_2].Split('|');
                    level = int.Parse(param[0]);
                    exp = int.Parse(param[1]);
                }

                return true;
            }
            catch (Exception ex)
            {
                KTDebug.LogError(ex.ToString());
            }
            return false;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm rỗng danh sách Ngũ hành hồn thạch
        /// </summary>
        public void EmptyFiveElementsStoneList()
        {
            this.ClearSlot();
        }

        /// <summary>
        /// Cập nhật hiển thị Ngũ hành ấn
        /// </summary>
        public void RefreshEnhanceEquip()
        {
            this.UI_SignetBox.Refresh();
            this.CalculateSignetEnhance();
        }

        /// <summary>
        /// Thực thi hiệu ứng cường hóa trang bị thành công
        /// </summary>
        public void PlaySignetEnhanceSuccessEffect()
        {
            this.UIImage_EnhanceSuccessEffect.gameObject.SetActive(true);
            this.UIImage_EnhanceSuccessEffect.Play();
        }
        #endregion
    }
}
