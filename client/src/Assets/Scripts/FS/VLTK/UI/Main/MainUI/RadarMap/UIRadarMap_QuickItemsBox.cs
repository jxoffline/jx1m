using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Logic.Settings;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.RadarMap
{
    /// <summary>
    /// Khung sử dụng thuốc nhanh dưới RadarMap
    /// </summary>
    public class UIRadarMap_QuickItemsBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Các ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox[] UIItemBox_Items;

        /// <summary>
        /// Khung chọn vật phẩm
        /// </summary>
        [SerializeField]
        private UISelectItem UISelectItem;

        /// <summary>
        /// Khung vật phẩm mở rộng
        /// </summary>
        [SerializeField]
        private RectTransform UIExpandableItems;

        /// <summary>
        /// Toggle vật phẩm mở rộng
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_ExpandItems;
        #endregion

        #region Private fields
        /// <summary>
        /// UI Hover các ô vật phẩm
        /// </summary>
        private UIHoverableObject[] uiHover_Items;

        /// <summary>
        /// Luồng thực thi yêu cầu lưu thiết lập vật phẩm dùng nhanh vào hệ thống
        /// </summary>
        private Coroutine saveQuickItemCoroutine = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện dùng vật phẩm tương ứng
        /// </summary>
        public Action<GoodsData> UseItem { get; set; }

        /// <summary>
        /// Sự kiện vật phẩm được chọn
        /// </summary>
        public Action<List<GoodsData>> ItemSelected { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.uiHover_Items = new UIHoverableObject[this.UIItemBox_Items.Length];
            for (int i = 0; i < this.UIItemBox_Items.Length; i++)
            {
                this.uiHover_Items[i] = this.UIItemBox_Items[i].GetComponent<UIHoverableObject>();
            }
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.StartCoroutine(this.UpdateItemCountInBag());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            /// Thiết lập sự kiện cho Button
            void SetHoverButtonEvent(UIHoverableObject uiHoverableObject, UIItemBox uiItemBox, int slotIndex)
            {
                uiHoverableObject.Hover = () =>
                {
                    this.ButtonItem_Hovered(uiItemBox, slotIndex);
                };
                uiHoverableObject.Click = () =>
                {
                    this.ButtonItem_Clicked(uiItemBox, slotIndex);
                };
                uiItemBox.Click = () =>
                {
                    this.ButtonItem_Clicked(uiItemBox, slotIndex);
                };
            }

            for (int i = 0; i < this.UIItemBox_Items.Length; i++)
            {
                SetHoverButtonEvent(this.uiHover_Items[i], this.UIItemBox_Items[i], i);
            }
            this.UIToggle_ExpandItems.OnSelected = (isSelected) =>
            {
                this.UIExpandableItems.gameObject.SetActive(isSelected);
            };
        }

        /// <summary>
        /// Sự kiện khi bắt đầu Hover Button hồi sinh lực
        /// </summary>
        /// <param name="uiItemBox"></param>
        /// <param name="slotIndex"></param>
        private void ButtonItem_Hovered(UIItemBox uiItemBox, int slotIndex)
        {
            this.ShowQuickUseItemSelectFrame(uiItemBox, slotIndex);
        }


        /// <summary>
        /// Sự kiện khi Button hồi sinh lực được ấn
        /// </summary>
        /// <param name="uiItemBox"></param>
        /// <param name="slotIndex"></param>
        private void ButtonItem_Clicked(UIItemBox uiItemBox, int slotIndex)
        {
            /// Nếu chưa có thuốc nào được đặt vào
            if (uiItemBox.Data == null)
            {
                this.ShowQuickUseItemSelectFrame(uiItemBox, slotIndex);
            }
            else
            {
                /// Nếu danh sách vật phẩm không tồn tại
                if (Global.Data.RoleData.GoodsDataList == null)
                {
                    return;
                }

                /// Tìm vị trí vật phẩm tương ứng trong túi
                GoodsData itemGD = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == uiItemBox.Data.GoodsID).FirstOrDefault();
                /// Nếu không có vật phẩm
                if (itemGD == null)
                {
                    return;
                }
                this.UseItem?.Invoke(itemGD);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện sau khoảng thời gian tương ứng
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        private IEnumerator ExecuteLater(float sec, Action callBack)
        {
            yield return new WaitForSeconds(sec);
            callBack?.Invoke();
        }

        /// <summary>
        /// Hiện khung chọn vật phẩm có thể dùng
        /// <param name="uiItemBox"></param>
        /// <param name="slotIndex"></param>
        /// </summary>
        private void ShowQuickUseItemSelectFrame(UIItemBox uiItemBox, int slotIndex)
        {
            if (Global.Data.RoleData.GoodsDataList == null)
            {
                return;
            }

            List<GoodsData> hpMedicines = Global.Data.RoleData.GoodsDataList?.Where(x => KTGlobal.IsItemCanUse(x.GoodsID)).GroupBy(x => x.GoodsID).Select(x => x.First()).ToList();
            this.UISelectItem.Title = "Chọn vật phẩm sử dụng nhanh";
            this.UISelectItem.Items = hpMedicines;
            this.UISelectItem.ItemSelected = (itemGD) => {
                /// Nếu không có vật phẩm được chọn
                if (itemGD == null)
                {
                    return;
                }
                /// Nếu vật phẩm không tồn tại trong hệ thống
                if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
                {
                    return;
                }

                /// Tạo mới đối tượng vật phẩm tương ứng
                GoodsData hpMedicine = new GoodsData()
                {
                    GoodsID = itemGD.GoodsID,
                    GCount = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == itemGD.GoodsID).Sum(x => x.GCount),
                    Binding = itemGD.Binding,
                };
                uiItemBox.Data = hpMedicine;

                /// Danh sách vật phẩm dùng nhanh
                string[] itemIDsString = Global.Data.RoleData.QuickItems.Split('|');
                /// Toác
                if (itemIDsString.Length != 5)
                {
                    itemIDsString = new string[]
                    {
                        "-1", "-1", "-1", "-1", "-1",
                    };
                    Global.Data.RoleData.QuickItems = string.Join("|", itemIDsString);
                }
                itemIDsString[slotIndex] = itemGD.GoodsID.ToString();
                /// Lưu lại thông tin
                Global.Data.RoleData.QuickItems = string.Join("|", itemIDsString);

                /// Thực thi sự kiện chọn thuốc
                this.SaveAndExecuteItemSelected();
            };
            this.UISelectItem.Show();
        }
        
        /// <summary>
        /// Cập nhật số lượng thuốc hiện có trong túi đồ
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateItemCountInBag()
        {
            /// Danh sách vật phẩm nhanh trước đó
            Tuple<int, int>[] lastItems = new Tuple<int, int>[]
            {
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(-1, -1),
                new Tuple<int, int>(-1, -1),
            };

            while (true)
            {
                /// Danh sách vật phẩm dùng nhanh
                string[] itemIDsString = Global.Data.RoleData.QuickItems.Split('|');
                /// Toác
                if (itemIDsString.Length != 5)
                {
                    itemIDsString = new string[]
                    {
                        "-1", "-1", "-1", "-1", "-1",
                    };
                    Global.Data.RoleData.QuickItems = string.Join("|", itemIDsString);
                }

                /// Duyệt danh sách
                for (int i = 0; i < itemIDsString.Length; i++)
                {
                    /// ID không hợp lệ
                    if (!int.TryParse(itemIDsString[i], out int itemID))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Tạo mới vật phẩm
                    GoodsData itemGD = new GoodsData()
                    {
                        GoodsID = itemID,
                    };
                    /// Thiết lập vật phẩm
                    this.UIItemBox_Items[i].Data = itemGD;
                    this.UIItemBox_Items[i].Refresh();

                    /// Nếu vật phẩm tồn tại
                    if (Global.Data.RoleData.GoodsDataList != null && Loader.Loader.Items.ContainsKey(itemID))
                    {
                        int count = Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == itemID).Sum(x => x.GCount);
                        /// Nếu số lượng <= 0
                        if (count <= 0)
                        {
                            /// Xóa khỏi ô
                            this.UIItemBox_Items[i].Data = null;

                            /// Lưu lại thông tin
                            itemIDsString[i] = "-1";
                            Global.Data.RoleData.QuickItems = string.Join("|", itemIDsString);

                            /// Lưu thiết lập Auto
                            this.SaveAndExecuteItemSelected();
                        }
                        else
                        {
                            this.UIItemBox_Items[i].Data.GCount = count;
                            this.UIItemBox_Items[i].RefreshQuantity();
                        }

                        /// Nếu ID vật phẩm giống nhau và số lượng bị giảm xuống tức là có sử dụng
                        if (this.UIItemBox_Items[i].Data != null && lastItems[i].Item1 == this.UIItemBox_Items[i].Data.GoodsID && lastItems[i].Item2 > count)
                        {
                            this.UIItemBox_Items[i].PlayUseItemSuccessfullyEffect();
                        }

                        /// Cập nhật ID vật phẩm và số lượng
                        lastItems[i] = new Tuple<int, int>(this.UIItemBox_Items[i].Data == null ? -1 : this.UIItemBox_Items[i].Data.GoodsID, count);
                    }
                }

                /// Nghỉ 1 giây
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Lưu thiết lập hệ thống và thực thi sự kiện chọn thuốc
        /// </summary>
        private void SaveAndExecuteItemSelected()
        {
            if (this.saveQuickItemCoroutine != null)
            {
                this.StopCoroutine(this.saveQuickItemCoroutine);
            }
            this.saveQuickItemCoroutine = this.StartCoroutine(this.ExecuteLater(5f, () => {
                this.ItemSelected?.Invoke(this.UIItemBox_Items.Select(x => x.Data).ToList());
            }));
        }
        #endregion

        #region Public methods

        #endregion
    }
}
