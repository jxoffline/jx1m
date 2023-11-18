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
        private List<GrowPointObject> waitToBeAddedGrowPoint = new List<GrowPointObject>();

        /// <summary>
        /// Tải điểm thu thập vật
        /// </summary>
        /// <param name="growPointData"></param>
        public void ToLoadGrowPoint(GrowPointObject growPointData)
        {
            this.waitToBeAddedGrowPoint.Add(growPointData);
        }

        /// <summary>
        /// Thêm điểm thu thập vào bản đồ
        /// </summary>
        private void AddListGrowPoint()
        {
            if (this.waitToBeAddedGrowPoint.Count <= 0)
            {
                return;
            }

            GrowPointObject growPointData = this.waitToBeAddedGrowPoint[0];
            this.waitToBeAddedGrowPoint.RemoveAt(0);
            this.AddListGrowPoint(growPointData);
        }

        /// <summary>
        /// Tải danh sách điểm thu thập
        /// </summary>
        /// <param name="growPointData"></param>
        private void AddListGrowPoint(GrowPointObject growPointData)
        {
            /// Tên đối tượng
            string name = string.Format("GrowPoint_{0}", growPointData.ID);

            /// Đối tượng cũ
            GSprite sprite = this.FindSprite(name);
            /// Nếu đối tượng có tồn tại
            if (sprite != null)
            {
                /// Xóa đối tượng
                KTGlobal.RemoveObject(sprite, true);
            }

            /// Tải xuống đối tượng
            sprite = this.LoadGrowPoint(growPointData);
        }

        /// <summary>
        /// Tải xuống điểm thu thập
        /// </summary>
        public GSprite LoadGrowPoint(GrowPointObject growPointData)
        {
            string name = string.Format("GrowPoint_{0}", growPointData.ID);

            GSprite growPoint = new GSprite();
            growPoint.BaseID = growPointData.ID;
            growPoint.SpriteType = GSpriteTypes.GrowPoint;

            this.LoadSprite(
                growPoint,
                growPointData.ID,
                name,
                null,
                null,
                null,
                growPointData,
                null,
                null,
                null,
                null,
                Direction.DOWN,
                growPointData.PosX,
                growPointData.PosY
            );

            /// Bắt đầu
            growPoint.Start();


            /// Tìm đối tượng cũ
            GameObject oldObject = KTObjectPoolManager.Instance.FindSpawn(x => x.name == name);
            /// Nếu tồn tại
            if (oldObject != null)
            {
                /// Trả lại Pool
                KTObjectPoolManager.Instance.ReturnToPool(oldObject);
            }

            //Monster growP = Object2DFactory.MakeMonster();
            Monster growP = KTObjectPoolManager.Instance.Instantiate<Monster>("GrowPoint");
            growP.name = name;

            /// Gắn đối tượng tham chiếu
            growP.RefObject = growPoint;

            //growP.gameObject.name = name;
            ColorUtility.TryParseHtmlString("#8fff2e", out Color minimapNameColor);
            growP.MinimapNameColor = minimapNameColor;
            growP.ShowMinimapIcon = true;
            growP.ShowMinimapName = false;
            growP.MinimapIconSize = new Vector2(6, 6);
            ColorUtility.TryParseHtmlString("#8fff2e", out Color nameColor);
            growP.NameColor = nameColor;
            //growP.UIHeaderOffset = new Vector2(10, 100);

            growP.ShowHPBar = false;
            growP.ShowElemental = false;

            /// Res
            growP.StaticID = growPointData.ResID;
            growP.ResID = FS.VLTK.Loader.Loader.ListMonsters[growPointData.ResID].ResID;
            growP.Direction = Direction.DOWN;
            growP.UpdateData();


            GameObject role2D = growP.gameObject;
            growPoint.Role2D = role2D;
            role2D.transform.localPosition = new Vector2((float) growPointData.PosX, (float) growPointData.PosY);

            /// Thực hiện động tác đứng
            growPoint.DoStand();
            growP.ResumeCurrentAction();

            return growPoint;
        }

        /// <summary>
        /// Xóa điểm thu thập tương ứng khỏi hệ thống
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="gpID"></param>
        /// <returns></returns>
        public bool DelGrowPoints(int gpID)
        {
            int roleID = gpID;
            GSprite growPoint = this.FindSprite(roleID);
            this.waitToBeAddedGrowPoint.RemoveAll(x => x.ID == gpID);

            if (null != growPoint)
            {
                KTGlobal.RemoveObject(growPoint, true);
            }

            return true;
        }
        #endregion
    }
}
