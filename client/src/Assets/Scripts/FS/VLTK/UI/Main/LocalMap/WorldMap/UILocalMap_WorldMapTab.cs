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
    /// Tab bản đồ thế giới
    /// </summary>
    public class UILocalMap_WorldMapTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab thông tin địa danh
        /// </summary>
        [SerializeField]
        private UILocalMap_WorldMapTab_PlaceInfo UIPlaceInfo_Prefab;

        /// <summary>
        /// Prefab thông tin đường đi
        /// </summary>
        [SerializeField]
        private UILocalMap_WorldMapTab_WayPortInfo UIWayPortInfo_Prefab;

        /// <summary>
        /// Prefab thông tin khu vực trong bản đồ thế giới
        /// </summary>
        [SerializeField]
        private UILocalMap_WorldMapTab_World_AreaInfo UIWorldAreaInfo_Prefab;

        /// <summary>
        /// Image bản đồ
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_MapBackground;

        /// <summary>
        /// Mark vị trí hiện tại
        /// </summary>
        [SerializeField]
        private RectTransform UI_CurrentPosMark;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách chi tiết
        /// </summary>
        private RectTransform transformInfoList = null;
		#endregion

		#region Properties
        /// <summary>
        /// Sự kiện di chuyển đến bản đồ tương ứng
        /// </summary>
        public Action<int> GoToMap { get; set; }
		#endregion

		#region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
		private void Awake()
		{
            this.transformInfoList = this.UIWorldAreaInfo_Prefab.transform.parent.GetComponent<RectTransform>();
		}

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
		private void OnEnable()
		{
            this.LoadWorldMap();
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
                if (child.gameObject != this.UIWorldAreaInfo_Prefab.gameObject && child.gameObject != this.UIWayPortInfo_Prefab.gameObject &&  child.gameObject != this.UIPlaceInfo_Prefab.gameObject)
				{
                    GameObject.Destroy(child.gameObject);
				}
			}

            /// Ẩn vị trí hiện tại
            this.UI_CurrentPosMark.gameObject.SetActive(false);
		}

        /// <summary>
        /// Tải xuống bản đồ thế giới
        /// </summary>
        private void LoadWorldMap()
		{
            this.Clear();

            /// Đối tượng WorldMap
            WorldMapXML.World worldMapInfo = Loader.Loader.WorldMap.WorldMap;

            /// Gắn ảnh
            this.UIImage_MapBackground.SpriteName = worldMapInfo.ImageFileName;
            //this.UIImage_MapBackground.AtlasName = worldMapInfo.ImageFileName;
            this.UIImage_MapBackground.Load();

            /// Duyệt danh sách khu vực
            foreach (WorldMapXML.World.Area areaInfo in worldMapInfo.Areas)
			{
                /// Tạo đối tượng tương ứng
                UILocalMap_WorldMapTab_World_AreaInfo uiAreaInfo = GameObject.Instantiate<UILocalMap_WorldMapTab_World_AreaInfo>(this.UIWorldAreaInfo_Prefab);
                uiAreaInfo.transform.SetParent(this.transformInfoList, false);
                uiAreaInfo.gameObject.SetActive(true);
                uiAreaInfo.Data = areaInfo;
                uiAreaInfo.Click = () => {
                    this.LoadArea(areaInfo.ID);
                };
            }

            /// Duyệt danh sách địa danh
            foreach (WorldMapXML.Place placeInfo in worldMapInfo.Places)
            {
                /// Thêm địa danh tương ứng
                this.AddPlace(placeInfo);
            }
        }

        /// <summary>
        /// Tải xuống bản đồ khu vực
        /// </summary>
        /// <param name="areaID"></param>
        private void LoadArea(int areaID)
		{
            this.Clear();

            /// Đối tượng khu vực
            WorldMapXML.Area areaInfo = Loader.Loader.WorldMap.Areas.Where(x => x.ID == areaID).FirstOrDefault();

            /// Gắn ảnh
            this.UIImage_MapBackground.SpriteName = areaInfo.ImageFileName;
            //this.UIImage_MapBackground.AtlasName = areaInfo.ImageFileName;
            this.UIImage_MapBackground.Load();

            /// Duyệt danh sách địa danh
            foreach (WorldMapXML.Place placeInfo in areaInfo.Places)
			{
                /// Thêm địa danh tương ứng
                this.AddPlace(placeInfo);
			}

            /// Duyệt danh sách chỉ đường
            foreach (WorldMapXML.WayPort wayPortInfo in areaInfo.WayPorts)
			{
                UILocalMap_WorldMapTab_WayPortInfo uiWayPortInfo = GameObject.Instantiate<UILocalMap_WorldMapTab_WayPortInfo>(this.UIWayPortInfo_Prefab);
                uiWayPortInfo.transform.SetParent(this.transformInfoList, false);
                uiWayPortInfo.gameObject.SetActive(true);
                uiWayPortInfo.Data = wayPortInfo;
            }
        }

        /// <summary>
        /// Thêm địa danh tương ứng
        /// </summary>
        /// <param name="data"></param>
        private void AddPlace(WorldMapXML.Place data)
		{
            UILocalMap_WorldMapTab_PlaceInfo uiPlaceInfo = GameObject.Instantiate<UILocalMap_WorldMapTab_PlaceInfo>(this.UIPlaceInfo_Prefab);
            uiPlaceInfo.transform.SetParent(this.transformInfoList, false);
            uiPlaceInfo.gameObject.SetActive(true);
            uiPlaceInfo.Data = data;
            uiPlaceInfo.Click = () => {
                /// Thông tin bản đồ tương ứng
                if (Loader.Loader.Maps.TryGetValue(data.MapCode, out Map mapData))
				{
                    KTGlobal.AddNotification(string.Format("Dịch chuyển đến {0}!", mapData.Name));
				}
                this.GoToMap?.Invoke(data.MapCode);
            };

            /// Nếu trùng với vị trí đang đứng
            if (Global.Data.RoleData.MapCode == data.MapCode)
			{
                /// Đánh dấu đây là vị trí hiện tại
                this.UI_CurrentPosMark.gameObject.SetActive(true);
                this.UI_CurrentPosMark.transform.localPosition = new Vector2(data.IconPosX, data.IconPosY);
            }
        }
		#endregion
	}
}
