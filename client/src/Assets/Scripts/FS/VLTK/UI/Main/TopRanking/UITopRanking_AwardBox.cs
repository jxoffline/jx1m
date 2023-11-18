using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.TopRanking
{
    /// <summary>
    /// Thông tin quà của thứ hạng tương ứng trong khung đua top
    /// </summary>
    public class UITopRanking_AwardBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên phần quà
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_AwardName;

        /// <summary>
        /// Prefab ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UI_ItemPrefab;

        /// <summary>
        /// Button nhận quà
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Get;
        #endregion

        #region Properties
        private AwardConfig _Data;
        /// <summary>
        /// Thông tin phần quà
        /// </summary>
        public AwardConfig Data
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
        /// Sự kiện nhận quà
        /// </summary>
        public Action Get { get; set; }

        /// <summary>
        /// Kích hoạt Button nhận thưởng
        /// </summary>
        public bool EnableGet
        {
            get
            {
                return this.UIButton_Get.interactable;
            }
            set
            {
                this.UIButton_Get.interactable = value;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransformdanh sách vật phẩm
        /// </summary>
        private RectTransform transformItemList;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemList = this.UI_ItemPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_Get.onClick.AddListener(this.ButtonGet_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button nhận thưởng được ấn
        /// </summary>
        private void ButtonGet_Clicked()
        {
            this.Get?.Invoke();
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
            this.StartCoroutine(this.DoExecuteSkipFrames(1, work));
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Xóa danh sách vật phẩm
            foreach (Transform child in this.transformItemList.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Toác
            if (this._Data == null)
            {
                return;
            }

          
            /// Tên
            this.UIText_AwardName.text = this._Data.RankStart == this._Data.RankEnd ? string.Format("Top {0}", this._Data.RankStart) : string.Format("Top {0} - {1}", this._Data.RankStart, this._Data.RankEnd);
            /// Duyệt danh sách phần quà
            foreach (string awardInfo in this._Data.AwardList.Split('|'))
            {
                string[] fields = awardInfo.Split(':');
                /// Thông tin
                int itemID = int.Parse(fields[0]);
                int count = int.Parse(fields[1]);
                /// Thông tin vật phẩm
                if (Loader.Loader.Items.TryGetValue(itemID, out Entities.Config.ItemData itemData))
                {
                    /// Tạo vật phẩm ảo
                    GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                    /// Số lượng
                    itemGD.GCount = count;
                    /// Tạo mới ô vật phẩm
                    UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_ItemPrefab);
                    uiItemBox.transform.SetParent(this.transformItemList, false);
                    uiItemBox.gameObject.SetActive(true);
                    uiItemBox.Data = itemGD;
                }
            }

            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemList);
            });
        }
        #endregion
    }
}
