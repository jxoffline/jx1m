using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK;
using FS.VLTK.Control.Component;
using FS.VLTK.Factory;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý xe tiêu
    /// </summary>
    public partial class GScene
    {
        #region Hệ thống xe tiêu
        /// <summary>
        /// Danh sách xe tiêu
        /// </summary>
        private List<TraderCarriageData> waitToBeAddedCarriages = new List<TraderCarriageData>();

        /// <summary>
        /// Tải xe tiêu
        /// </summary>
        /// <param name="data"></param>
        public void ToLoadTraderCarriage(TraderCarriageData data)
        {
            this.waitToBeAddedCarriages.Add(data);
        }

        /// <summary>
        /// Thêm xe tiêu vào bản đồ
        /// </summary>
        private void AddListTraderCarriage()
        {
            if (this.waitToBeAddedCarriages.Count <= 0)
            {
                return;
            }

            TraderCarriageData data = this.waitToBeAddedCarriages[0];
            this.waitToBeAddedCarriages.RemoveAt(0);
            this.AddListTraderCarriage(data);
        }

        /// <summary>
        /// Tải danh sách xe tiêu
        /// </summary>
        /// <param name="data"></param>
        private void AddListTraderCarriage(TraderCarriageData data)
        {
            /// Toác
            if (data == null)
            {
                return;
            }

            /// Nếu đối tượng không tồn tại thì bỏ qua
            if (!Global.Data.TraderCarriages.TryGetValue(data.RoleID, out _))
            {
                return;
            }

            /// Tên đối tượng
            string name = string.Format("TraderCarriage_{0}", data.RoleID);

            /// Đối tượng cũ
            GSprite sprite = this.FindSprite(name);
            /// Nếu đối tượng có tồn tại
            if (sprite != null)
            {
                /// Xóa đối tượng
                KTGlobal.RemoveObject(sprite, true);
            }

            /// Tải xuống đối tượng
            sprite = this.LoadTraderCarriage(data);
            /// Thực hiện gửi gói tin tải xuống hoàn tất
            GameInstance.Game.SpriteLoadAlready(sprite.RoleID);
        }

        /// <summary>
        /// Tải xuống xe tiêu
        /// </summary>
        public GSprite LoadTraderCarriage(TraderCarriageData data)
        {
            string name = string.Format("TraderCarriage_{0}", data.RoleID);

            GSprite carriageObj = new GSprite();
            carriageObj.BaseID = data.RoleID;
            carriageObj.SpriteType = GSpriteTypes.TraderCarriage;

            this.LoadSprite(
                carriageObj,
                data.RoleID,
                name,
                null,
                null,
                null,
                null,
                null,
                null,
                data,
                null,
                (Direction) data.Direction,
                data.PosX,
                data.PosY
            );

            /// Bắt đầu
            carriageObj.Start();

            /// Tìm đối tượng cũ
            GameObject oldObject = KTObjectPoolManager.Instance.FindSpawn(x => x.name == name);
            /// Nếu tồn tại
            if (oldObject != null)
            {
                /// Trả lại Pool
                KTObjectPoolManager.Instance.ReturnToPool(oldObject);
            }

            Monster carriage = KTObjectPoolManager.Instance.Instantiate<Monster>("TraderCarriage");
            carriage.name = name;

            /// Gắn đối tượng tham chiếu
            carriage.RefObject = carriageObj;

            carriage.ShowMinimapIcon = false;
            carriage.ShowMinimapName = false;

            ColorUtility.TryParseHtmlString("#8BD4FF", out Color nameColor);
            carriage.NameColor = nameColor;
            carriage.ShowHPBar = true;
            carriage.ShowElemental = false;

            /// Res
            carriage.StaticID = data.ResID;
            carriage.ResID = FS.VLTK.Loader.Loader.ListMonsters[data.ResID].ResID;
            carriage.Direction = (Direction) data.Direction;
            carriage.UpdateData();


            GameObject role2D = carriage.gameObject;
            carriageObj.Role2D = role2D;
            role2D.transform.localPosition = new Vector2(data.PosX, data.PosY);

            /// Cập nhật hiển thị thanh máu
            this.RefreshSpriteLife(carriageObj);

            /// Thực hiện động tác đứng
            carriageObj.DoStand();
            carriage.ResumeCurrentAction();

            return carriageObj;
        }

        /// <summary>
        /// Xóa xe tiêu tương ứng khỏi hệ thống
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public bool DelTraderCarriage(int roleID)
        {
            GSprite carriageObj = this.FindSprite(roleID);
            /// Xóa toàn bộ xe tiêu khác đang đợi load tương ứng
            this.waitToBeAddedCarriages.RemoveAll(x => x.RoleID == roleID);
            
            /// Nếu tìm thấy
            if (carriageObj != null)
            {
                /// Nếu đang thực hiện động tác chết
                if (carriageObj.IsDeath || carriageObj.HP <= 0)
                {
                    /// Bỏ qua
                    return false;
                }

                /// Xóa xe tiêu
                KTGlobal.RemoveObject(carriageObj, true);
            }

            return true;
        }
        #endregion
    }
}
