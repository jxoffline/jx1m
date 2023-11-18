using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;
using FS.VLTK.Entities.Config;
using Server.Data;

namespace FS.VLTK.UI.Main.Welfare.Recharge
{
    /// <summary>
    /// Khung nạp mỗi ngày
    /// </summary>
    public class UIWelfare_Recharge_EverydayRecharge : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab danh sách vật phẩm ở mốc tương ứng
        /// </summary>
        [SerializeField]
        private UIWelfare_Recharge_ItemList UI_ItemListPrefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemsList = null;

        /// <summary>
        /// Trạng thái Button các ô vật phẩm
        /// </summary>
        private readonly List<int> buttonStates = new List<int>();
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện nhận quà
        /// </summary>
        public Action<DayRechageAward> Get { get; set; }

        private DayRechage _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public DayRechage Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Awake()
        {
            this.transformItemsList = this.UI_ItemListPrefab.transform.parent.GetComponent<RectTransform>();
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
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="workd"></param>
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
        private void ClearItemsList()
        {
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UI_ItemListPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm danh sách vật phẩm
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="awardsInfo"></param>
        private void AddItemList(int pos, DayRechageAward awardsInfo)
        {
            UIWelfare_Recharge_ItemList uiItemList = GameObject.Instantiate<UIWelfare_Recharge_ItemList>(this.UI_ItemListPrefab);
            uiItemList.transform.SetParent(this.transformItemsList, false);
            uiItemList.gameObject.SetActive(true);
            /// Danh sách vật phẩm thưởng
            List<GoodsData> items = new List<GoodsData>();
            /// Danh sách thiết lập vật phẩm
            string[] strParams = awardsInfo.GoodsIDs.Split('|');
            foreach (string strParam in strParams)
            {
                try
                {
                    string[] para = strParam.Split(',');
                    int itemID = int.Parse(para[0]);
                    int itemQuantity = int.Parse(para[1]);

                    /// Thông tin vật phẩm tương ứng
                    if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
                    {
                        /// Tạo mới vật phẩm
                        GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                        itemGD.Binding = 1;
                        itemGD.GCount = itemQuantity;
                        /// Thêm vật phẩm vào danh sách
                        items.Add(itemGD);
                    }
                }
                catch (Exception) { }
            }
            /// Thiết lập vật phẩm
            uiItemList.Items = items;
            /// Nếu chưa nhận thì mở khóa Button nhận
            uiItemList.EnableGet = this.buttonStates[pos] == 1;
            /// Sự kiện
            if (this.buttonStates[pos] == 1)
            {
                uiItemList.Get = () => {
                    this.Get?.Invoke(awardsInfo);
                };
            }
            uiItemList.AlreadyGotten = this.buttonStates[pos] == 2;
            /// Mốc nạp tương ứng
            uiItemList.RechargeAmount = awardsInfo.MinYuanBao;
        }

        /// <summary>
        /// Làm mới dữ liệu khung
        /// </summary>
        private void Refresh()
        {
            /// Xóa danh sách cũ
            this.ClearItemsList();

            /// Làm rỗng danh sách
            this.buttonStates.Clear();

            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                KTGlobal.AddNotification("Dữ liệu truyền về bị lỗi, hãy liên hệ với hỗ trợ để được trợ giúp.");
                return;
            }

            /// Thông tin truyền về
            string[] infoParamsS1 = this._Data.BtnState.Split(':');
            /// Nếu không thỏa mãn kích thước
            if (infoParamsS1.Length != 2)
            {
                KTGlobal.AddNotification("Dữ liệu truyền về có lỗi, hãy liên hệ với hỗ trợ để được trợ giúp!");
                return;
            }

            /// Thông tin trạng thái Button
            string[] infoParams = infoParamsS1[0].Split(',');
            /// Nếu không thỏa mãn kích thước
            if (infoParams.Length != this._Data.DayRechageAward.Count)
            {
                KTGlobal.AddNotification("Dữ liệu truyền về có lỗi, hãy liên hệ với hỗ trợ để được trợ giúp!");
                return;
            }

            try
            {
                /// Duyệt danh sách và lấy ra trạng thái Button
                for (int i = 0; i < infoParams.Length; i++)
                {
                    this.buttonStates.Add(int.Parse(infoParams[i]));
                }
            }
            catch (Exception)
            {
                KTGlobal.AddNotification("Dữ liệu truyền về có lỗi, hãy liên hệ với hỗ trợ để được trợ giúp!");
                return;
            }

            /// Duyệt danh sách vật phẩm
            for (int i = 0; i < this._Data.DayRechageAward.Count; i++)
            {
                /// Thêm danh sách vật phẩm thưởng
                this.AddItemList(i, this._Data.DayRechageAward[i]);
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật trạng thái
        /// </summary>
        /// <param name="states"></param>
        public void UpdateState(List<int> states)
        {
            /// Duyệt danh sách và lấy ra trạng thái Button
            for (int i = 0; i < states.Count; i++)
            {
                this.buttonStates[i] = states[i];
            }

            /// Duyệt danh sách
            int idx = 0;
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UI_ItemListPrefab.gameObject)
                {
                    UIWelfare_Recharge_ItemList uiItemBox = child.GetComponent<UIWelfare_Recharge_ItemList>();
                    /// Cập nhật trạng thái
                    uiItemBox.EnableGet = this.buttonStates[idx] == 1;
                    uiItemBox.AlreadyGotten = this.buttonStates[idx] == 2;
                    idx++;
                }
            }
        }

        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
