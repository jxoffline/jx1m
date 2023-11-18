using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.LocalMap
{
    /// <summary>
    /// Tab bản đồ liên máy chủ
    /// </summary>
    public class UILocalMap_CrossServerMapTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab thông tin bản đồ
        /// </summary>
        [SerializeField]
        private UILocalMap_CrossServerMapTab_MapInfo UIMapInfo_Prefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách chi tiết
        /// </summary>
        private RectTransform transformInfoList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện di chuyển đến vị trí của NPC tương ứng
        /// </summary>
        public Action<int, int> GoToNPC { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformInfoList = this.UIMapInfo_Prefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
		private void OnEnable()
        {
            this.LoadCrossServerMap();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
		private void OnDisable()
        {
            this.Clear();
        }
        #endregion

        #region Code UI

        #endregion

        #region Private methods
        /// <summary>
        /// Làm rỗng
        /// </summary>
        private void Clear()
        {
            /// Duyệt danh sách và xóa ký hiệu
            foreach (Transform child in this.transformInfoList.transform)
            {
                if (child.gameObject != this.UIMapInfo_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Tải xuống bản đồ liên máy chủ
        /// </summary>
        private void LoadCrossServerMap()
        {
            this.Clear();

            /// Duyệt danh sách địa danh
            foreach (CrossServerMapXML.MapInfo mapInfo in Loader.Loader.CrossServerMap.Maps)
            {
                /// Thêm bản đồ tương ứng
                this.AddMap(mapInfo);
            }
        }

        /// <summary>
        /// Thêm bản đồ tương ứng
        /// </summary>
        /// <param name="data"></param>
        private void AddMap(CrossServerMapXML.MapInfo data)
        {
            UILocalMap_CrossServerMapTab_MapInfo uiMapInfo = GameObject.Instantiate<UILocalMap_CrossServerMapTab_MapInfo>(this.UIMapInfo_Prefab);
            uiMapInfo.transform.SetParent(this.transformInfoList, false);
            uiMapInfo.gameObject.SetActive(true);
            uiMapInfo.Data = data;
            uiMapInfo.Click = () => {
                this.GoToNPC?.Invoke(data.NPCMapCode, data.NPCID);
            };
        }
        #endregion
    }
}
