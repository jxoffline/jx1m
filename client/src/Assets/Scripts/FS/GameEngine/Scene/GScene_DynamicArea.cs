using FS.Drawing;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.GameFramework.Logic;
using FS.VLTK;
using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý điểm thu thập
    /// </summary>
    public partial class GScene
    {
        #region Hệ thống điểm thu thập
        /// <summary>
        /// Danh sách điểm thu thập
        /// </summary>
        private List<DynamicArea> waitToBeAddedDynamicArea = new List<DynamicArea>();

        /// <summary>
        /// Tải điểm thu thập vật
        /// </summary>
        /// <param name="DynamicAreaData"></param>
        public void ToLoadDynamicArea(DynamicArea dynamicAreaData)
        {
            this.waitToBeAddedDynamicArea.Add(dynamicAreaData);
        }

        /// <summary>
        /// Thêm điểm thu thập vào bản đồ
        /// </summary>
        private void AddListDynamicArea()
        {
            if (this.waitToBeAddedDynamicArea.Count <= 0)
            {
                return;
            }

            DynamicArea DynamicAreaData = this.waitToBeAddedDynamicArea[0];
            this.waitToBeAddedDynamicArea.RemoveAt(0);
            this.AddListDynamicArea(DynamicAreaData);
        }

        /// <summary>
        /// Tải danh sách điểm thu thập
        /// </summary>
        /// <param name="DynamicAreaData"></param>
        private void AddListDynamicArea(DynamicArea dynamicAreaData)
        {
            /// Tên đối tượng
            string name = string.Format("DynamicArea_{0}", dynamicAreaData.ID);

            /// Đối tượng cũ
            GSprite sprite = this.FindSprite(name);
            /// Nếu đối tượng có tồn tại
            if (sprite != null)
            {
                /// Xóa đối tượng
                KTGlobal.RemoveObject(sprite, true);
            }

            /// Tải xuống đối tượng
            sprite = this.LoadDynamicArea(dynamicAreaData);
        }

        /// <summary>
        /// Tải xuống điểm thu thập
        /// </summary>
        public GSprite LoadDynamicArea(DynamicArea dynamicAreaData)
        {
            string name = string.Format("DynamicArea_{0}", dynamicAreaData.ID);

            GSprite dynamicArea = new GSprite();
            dynamicArea.BaseID = dynamicAreaData.ID;
            dynamicArea.SpriteType = GSpriteTypes.DynamicArea;

            this.LoadSprite(
                dynamicArea,
                dynamicAreaData.ID,
                name,
                null,
                null,
                null,
                null,
                dynamicAreaData,
                null,
                null,
                null,
                Direction.DOWN,
                dynamicAreaData.PosX,
                dynamicAreaData.PosY
            );

            /// Bắt đầu
            dynamicArea.Start();


            /// Tìm đối tượng cũ
            GameObject oldObject = KTObjectPoolManager.Instance.FindSpawn(x => x.name == name);
            /// Nếu tồn tại
            if (oldObject != null)
            {
                /// Trả lại Pool
                KTObjectPoolManager.Instance.ReturnToPool(oldObject);
            }

            //Monster dynArea = Object2DFactory.MakeMonster();
            Monster dynArea = KTObjectPoolManager.Instance.Instantiate<Monster>("DynamicArea");
            dynArea.name = name;

            /// Gắn đối tượng tham chiếu
            dynArea.RefObject = dynamicArea;

            //dynArea.gameObject.name = name;
            ColorUtility.TryParseHtmlString("#1aff05", out Color minimapNameColor);
            dynArea.MinimapNameColor = minimapNameColor;
            dynArea.ShowMinimapIcon = false;
            dynArea.ShowMinimapName = false;
            dynArea.MinimapIconSize = new Vector2(6, 6);
            ColorUtility.TryParseHtmlString("#1aff05", out Color nameColor);
            dynArea.NameColor = nameColor;
            //dynArea.UIHeaderOffset = new Vector2(10, 50);

            dynArea.ShowHPBar = false;
            dynArea.ShowElemental = false;

            /// Res
            dynArea.StaticID = dynamicAreaData.ResID;
            dynArea.ResID = FS.VLTK.Loader.Loader.ListMonsters[dynamicAreaData.ResID].ResID;
            dynArea.Direction = Direction.DOWN;
            dynArea.UpdateData();

            GameObject role2D = dynArea.gameObject;
            dynamicArea.Role2D = role2D;
            role2D.transform.localPosition = new Vector2((float) dynamicAreaData.PosX, (float) dynamicAreaData.PosY);

            /// Thực hiện động tác đứng
            dynamicArea.DoStand();
            dynArea.ResumeCurrentAction();

            return dynamicArea;
        }

        /// <summary>
        /// Xóa điểm thu thập tương ứng khỏi hệ thống
        /// </summary>
        /// <param name="gpID"></param>
        /// <returns></returns>
        public bool DelDynamicAreas(int gpID)
        {
            int roleID = gpID;
            GSprite DynamicArea = this.FindSprite(roleID);
            this.waitToBeAddedDynamicArea.RemoveAll(x => x.ID == gpID);

            if (null != DynamicArea)
            {
                KTGlobal.RemoveObject(DynamicArea, true);
            }

            return true;
        }
        #endregion
    }
}
