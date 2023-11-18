using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory.UIManager;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.UI.Main.Bag
{
    /// <summary>
    /// Lưới túi đồ
    /// </summary>
    public class UIBag_Grid : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIBag_Item UIItem_Prefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform chứa danh sách ô vật phẩm
        /// </summary>
        private RectTransform UITransform_SlotList;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi vật phẩm trong lưới được Click
        /// </summary>
        public Action<GoodsData> BagItemClicked { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            /// Thêm đối tượng vào danh sách quản lý ở UIBagManager
            UIBagManager.Instance.AddElement(this);
            this.UITransform_SlotList = this.UIItem_Prefab.transform.parent.gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.ClearSlotList();
            this.MakeDefaultSlot();
            this.RefreshBagData();
            this.RebuildLayout();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy
        /// </summary>
        private void OnDestroy()
        {
            /// Xóa đối tượng khỏi danh sách quản lý ở UIBagManager
            UIBagManager.Instance.RemoveElement(this);
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }

        /// <summary>
        /// Sự kiện khi vật phẩm trong túi được ấn
        /// </summary>
        /// <param name="itemGD"></param>
        private void BagItem_Clicked(GoodsData itemGD)
        {
            /// Dữ liệu vật phẩm
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out Entities.Config.ItemData itemData))
            {
                return;
            }

            this.BagItemClicked?.Invoke(itemGD);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số lượng Frame nhất định
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrame(int skip, Action callback)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callback?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrame(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.UITransform_SlotList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách vật phẩm
        /// </summary>
        private void ClearSlotList()
        {
            foreach (Transform child in this.UITransform_SlotList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    GameObject.Destroy(this.gameObject);
                }
            }
        }

        /// <summary>
        /// Trả về một vị trí trống trong túi
        /// </summary>
        /// <returns></returns>
        private UIBag_Item GetEmptySlot()
        {
            foreach (Transform child in this.UITransform_SlotList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIBag_Item uiItemBox = child.gameObject.GetComponent<UIBag_Item>();
                    if (uiItemBox != null && uiItemBox.Data == null)
                    {
                        return uiItemBox;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Trả về ô vật phẩm tại vị trí tương ứng
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private UIBag_Item GetItemSlot(int slot)
        {
            /// Nếu quá phạm vi
            if (slot + 1 >= this.UITransform_SlotList.transform.childCount)
            {
                /// Bỏ qua
                return null;
            }

            Transform child = this.UITransform_SlotList.transform.GetChild(slot + 1);
            if (child != null)
            {
                UIBag_Item uiItemBox = child.gameObject.GetComponent<UIBag_Item>();
                if (uiItemBox != null)
                {
                    return uiItemBox;
                }
            }
            return null;
        }

        /// <summary>
        /// Tìm vật phẩm có DbID tương ứng
        /// </summary>
        /// <param name="dbID"></param>
        /// <returns></returns>
        private UIBag_Item FindItemByDbID(int dbID)
        {
            foreach (Transform child in this.UITransform_SlotList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIBag_Item uiItemBox = child.gameObject.GetComponent<UIBag_Item>();
                    if (uiItemBox != null && uiItemBox.Data != null && uiItemBox.Data.Id == dbID)
                    {
                        return uiItemBox;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Trả về vị trí trong túi
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private int GetSlotIndex(UIBag_Item slot)
        {
            int idx = -1;
            foreach (Transform child in this.UITransform_SlotList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    idx++;
                    if (child.gameObject == slot.gameObject)
                    {
                        return idx;
                    }
                }
            }
            return idx;
        }

        /// <summary>
        /// Tạo các vị trí mặc định
        /// </summary>
        private void MakeDefaultSlot()
        {
            for (int i = 1; i <= Global.Data.RoleData.BagNum; i++)
            {
                UIBag_Item uiBagItem = GameObject.Instantiate<UIBag_Item>(this.UIItem_Prefab);
                uiBagItem.gameObject.SetActive(true);
                uiBagItem.transform.SetParent(this.UITransform_SlotList, false);
                uiBagItem.Data = null;
                uiBagItem.Click = null;
                uiBagItem.Locked = true;
            }
        }

        /// <summary>
        /// Làm mới dữ liệu tại ô tương ứng
        /// </summary>
        /// <param name="uiItemBox"></param>
        private void RefreshSlot(UIBag_Item uiItemBox)
        {
            uiItemBox.Data = null;
            uiItemBox.Click = null;
        }

        /// <summary>
        /// Làm rỗng túi
        /// </summary>
        private void EmptyBag()
        {
            foreach (Transform child in this.UITransform_SlotList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIBag_Item uiItemBox = child.gameObject.GetComponent<UIBag_Item>();
                    if (uiItemBox != null)
                    {
                        uiItemBox.Data = null;
                        uiItemBox.Click = null;
                    }
                }
            }
        }

        /// <summary>
        /// Làm mới dữ liệu túi đồ
        /// </summary>
        private void RefreshBagData()
        {
            this.EmptyBag();

            /// Duyệt toàn bộ danh sách trong túi, mở các ô tương ứng
            for (int i = 0; i < Global.Data.RoleData.BagNum; i++)
            {
                this.OpenSlot(i);
            }

            /// Duyệt toàn bộ danh sách vật phẩm trong túi
            if (Global.Data.RoleData.GoodsDataList != null)
            {
                /// Duyệt danh sách vật phẩm
                foreach (GoodsData itemGD in Global.Data.RoleData.GoodsDataList)
                {
                    /// Nếu dữ liệu vật phẩm không tồn tại
                    if (itemGD == null)
                    {
                        continue;
                    }
                    /// Nếu vị trí này không có vật phẩm
                    else if (itemGD.GCount <= 0 || itemGD.Using >= 0 || itemGD.Site != 0)
                    {
                        continue;
                    }

                    /// Thêm vật phẩm vào danh sách
                    this.AddItem(itemGD);
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới lại danh sách các vật phẩm trong túi đồ
        /// </summary>
        public void Reload()
        {
            this.RefreshBagData();
        }

        /// <summary>
        /// Thêm vật phẩm vào vị trí tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        public void AddItem(GoodsData itemGD)
        {
            if (itemGD == null)
            {
                return;
            }

            UIBag_Item uiItemBox = this.GetItemSlot(itemGD.BagIndex);
            if (uiItemBox == null)
            {
                return;
            }

            uiItemBox.Data = itemGD;
            uiItemBox.Click = () => {
                this.BagItem_Clicked(itemGD);
            };
        }

        /// <summary>
        /// Xóa vật phẩm khỏi túi
        /// </summary>
        /// <param name="itemGD"></param>
        public void RemoveItem(GoodsData itemGD)
        {
            if (itemGD == null)
            {
                return;
            }

            /// Tìm ô vật phẩm tương ứng
            UIBag_Item oldSlot = this.FindItemByDbID(itemGD.Id);
            /// Làm rỗng vị trí
            this.RefreshSlot(oldSlot);
        }

        /// <summary>
        /// Làm mới vật phẩm
        /// </summary>
        /// <param name="itemGD"></param>
        public void RefreshItem(GoodsData itemGD)
        {
            if (itemGD == null)
            {
                return;
            }

            /// Tìm ô vật phẩm tương ứng
            UIBag_Item oldSlot = this.FindItemByDbID(itemGD.Id);
            /// Nếu vị trí này mới là không có vật phẩm, hoặc vật phẩm đang được trang bị trên người, hoặc không nằm trong túi đồ
            if (itemGD.GCount <= 0 || itemGD.Using >= 0 || itemGD.Site != 0)
            {
                /// Nếu vị trí tồn tại
                if (oldSlot != null)
                {
                    /// Vị trí cũ
                    int oldIndex = this.GetSlotIndex(oldSlot);

                    /// Làm rỗng vị trí
                    this.RefreshSlot(oldSlot);

                    /// Nếu là thao tác đổi chỗ thì cái này sẽ tồn tại
                    GoodsData currentSlotItem = Global.Data.RoleData.GoodsDataList.Where(x => x.BagIndex == oldIndex).FirstOrDefault();
                    if (currentSlotItem != null)
                    {
                        oldSlot.Data = currentSlotItem;
                        oldSlot.Click = () => {
                            this.BagItem_Clicked(currentSlotItem);
                        };
                    }
                }
                return;
            }
            /// Nếu vị trí này có vật phẩm
            else if (itemGD.Site == 0 && itemGD.GCount > 0 && itemGD.Using == -1)
            {
                /// Nếu vị trí có tồn tại
                if (oldSlot != null)
                {
                    /// Cập nhật dữ liệu
                    oldSlot.Data = itemGD;
                }
            }
        }

        /// <summary>
        /// Mở khóa vị trí tương ứng
        /// </summary>
        /// <param name="slot"></param>
        public void OpenSlot(int slot)
        {
            UIBag_Item uiBagItem = this.GetItemSlot(slot);
            if (uiBagItem == null)
            {
                return;
            }
            uiBagItem.Locked = false;
        }
        #endregion
    }
}
