using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;
using Server.Data;

namespace FS.VLTK.UI.Main.Welfare.LevelUp
{
    /// <summary>
    /// Ô phần quà móc tương ứng
    /// </summary>
    public class UIWelfare_LevelUp_ListAward : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Levels;

        /// <summary>
        /// Prefab vật phẩm trong danh sách quà
        /// </summary>
        [SerializeField]
        private UIWelfare_LevelUp_ListAward_Item UI_ItemPrefab;

        /// <summary>
        /// Đánh dấu không thể nhận
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_CanNotGet;

        /// <summary>
        /// Đánh dấu đã nhận rồi
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_AlreadyGotten;

        /// <summary>
        /// Button nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Get;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm quà
        /// </summary>
        private RectTransform transformItemsList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện nhận quà
        /// </summary>
        public Action Get { get; set; }

        /// <summary>
        /// ID phần quà
        /// </summary>
        public int ID { get; set; }

        private int _State;
        /// <summary>
        /// Trạng thái nhận
        /// <para>0: Chưa đủ điều kiện, 1: Có thể nhận, 2: Đã nhận</para>
        /// </summary>
        public int State
        {
            get
            {
                return this._State;
            }
            set
            {
                this._State = value;
                this.UpdateStateToAllItems();

                switch (value)
                {
                    /// Có thể nhận
                    case 1:
                    {
                        this.UIButton_Get.interactable = true;
                        this.UIButton_Get.gameObject.SetActive(true);
                        this.UIMark_AlreadyGotten.gameObject.SetActive(false);
                        this.UIMark_CanNotGet.gameObject.SetActive(false);
                        break;
                    }
                    /// Đã nhận rồi
                    case 2:
                    {
                        this.UIButton_Get.interactable = false;
                        this.UIButton_Get.gameObject.SetActive(false);
                        this.UIMark_AlreadyGotten.gameObject.SetActive(true);
                        this.UIMark_CanNotGet.gameObject.SetActive(false);
                        break;
                    }
                    /// Không đủ điều kiện
                    default:
                    {
                        this.UIButton_Get.interactable = false;
                        this.UIButton_Get.gameObject.SetActive(false);
                        this.UIMark_AlreadyGotten.gameObject.SetActive(false);
                        this.UIMark_CanNotGet.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Danh sách vật phẩm thưởng
        /// </summary>
        public List<GoodsData> Items { get; set; }

        private int _Level;
        /// <summary>
        /// Cấp độ yêu cầu
        /// </summary>
        public int Level
        {
            get
            {
                return this._Level;
            }
            set
            {
                this._Level = value;
                this.UIText_Levels.text = value.ToString();
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemsList = this.UI_ItemPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_Get.onClick.AddListener(this.ButtonGet_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button nhận được ấn
        /// </summary>
        private void ButtonGet_Clicked()
        {
            /// Nếu không có gì để nhận
            if (this._State != 1)
            {
                return;
            }

            this.Get?.Invoke();
            this.UIButton_Get.interactable = false;
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
            /// Nếu chưa kích hoạt thì thôi
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách vật phẩm
        /// </summary>
        private void ClearItems()
        {
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Cập nhật trạng thái cho toàn bộ vật phẩm
        /// </summary>
        private void UpdateStateToAllItems()
        {
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    UIWelfare_LevelUp_ListAward_Item uiItemBox = child.GetComponent<UIWelfare_LevelUp_ListAward_Item>();
                    uiItemBox.State = this._State;
                }
            }
        }

        /// <summary>
        /// Thêm vật phẩm vào danh sách
        /// </summary>
        /// <param name="itemGD"></param>
        private void AddItem(GoodsData itemGD)
        {
            UIWelfare_LevelUp_ListAward_Item uiItemBox = GameObject.Instantiate<UIWelfare_LevelUp_ListAward_Item>(this.UI_ItemPrefab);
            uiItemBox.transform.SetParent(this.transformItemsList, false);
            uiItemBox.gameObject.SetActive(true);
            uiItemBox.Data = itemGD;
            uiItemBox.State = this._State;
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void Refresh()
        {
            /// Làm rỗng danh sách
            this.ClearItems();

            /// Duyệt danh sách vật phẩm
            foreach (GoodsData itemGD in this.Items)
            {
                /// Thêm vật phẩm tương ứng
                this.AddItem(itemGD);
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
