using System;
using System.Collections.Generic;
using UnityEngine;
using Server.Data;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using FS.GameFramework.Logic;
using FS.GameEngine.Sprite;
using FS.GameEngine.GoodsPack;
using FS.GameEngine.Logic;
using System.Linq;
using FS.VLTK;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý vật phẩm
    /// </summary>
    public partial class GScene
    {
        #region Hệ thống vật phẩm rơi ở Map
        /// <summary>
        /// Danh sách vật phẩm rơi ở Map
        /// </summary>
        private List<NewGoodsPackData> waitToBeAddedGoodsPack = new List<NewGoodsPackData>();

        /// <summary>
        /// Tìm vật phẩm theo điều kiện chỉ định
        /// </summary>
        /// <param name="predicate"></param>
        public List<GGoodsPack> FindGoodsPacks(Predicate<GGoodsPack> predicate)
        {
            if (predicate == null)
            {
                return KTObjectsManager.Instance.FindObjects<GGoodsPack>(x => true).Cast<GGoodsPack>().ToList();
            }
            return KTObjectsManager.Instance.FindObjects<GGoodsPack>(x => (x is GGoodsPack goodsPack && predicate.Invoke(goodsPack))).Cast<GGoodsPack>().ToList();
        }

        /// <summary>
        /// Tìm vật phẩm ở vị trí gần nhất theo điều kiện chỉ định
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public GGoodsPack FindNearestGoodsPack(Predicate<GGoodsPack> predicate)
        {
            List<GGoodsPack> goodsPacks = this.FindGoodsPacks(predicate);
            Vector2 leaderPos = this.Leader.PositionInVector2;
            return goodsPacks.MinBy((gp) => 
            {
                Vector2 gpPos = gp.PositionInVector2;
                float distance = Vector2.Distance(leaderPos, gpPos);
                return distance;
            });
        }

        /// <summary>
        /// Tải vật phẩm rơi ở Map vật
        /// </summary>
        /// <param name="gpData"></param>
        public void ToLoadGoodsPack(NewGoodsPackData gpData)
        {
            this.waitToBeAddedGoodsPack.Add(gpData);
        }

        /// <summary>
        /// Thêm vật phẩm rơi ở Map vào bản đồ
        /// </summary>
        private void AddListGoodsPack()
        {
            if (this.waitToBeAddedGoodsPack.Count <= 0)
            {
                return;
            }

            NewGoodsPackData gpData = this.waitToBeAddedGoodsPack[0];
            this.waitToBeAddedGoodsPack.RemoveAt(0);
            this.AddListGoodsPack(gpData);
        }

        /// <summary>
        /// Tải danh sách vật phẩm rơi ở Map
        /// </summary>
        /// <param name="gpData"></param>
        private void AddListGoodsPack(NewGoodsPackData gpData)
        {
            /// Tên đối tượng
            string name = string.Format("GoodsPack_{0}", gpData.AutoID);

            /// Đối tượng cũ
            GGoodsPack goodsPack = this.FindName(name) as GGoodsPack;
            /// Nếu đối tượng có tồn tại
            if (goodsPack != null)
            {
                return;
            }

            /// Tải xuống đối tượng
            this.LoadGoodsPack(gpData);
        }

        /// <summary>
        /// Tải xuống vật phẩm rơi ở Map
        /// </summary>
        public GGoodsPack LoadGoodsPack(NewGoodsPackData gpData)
        {
            string name = string.Format("GoodsPack_{0}", gpData.AutoID);

            GGoodsPack goodsPack = new GGoodsPack();
            goodsPack.SpriteType = GSpriteTypes.GoodsPack;
            goodsPack.BaseID = gpData.AutoID;
            goodsPack.Name = name;
            goodsPack.GoodsID = gpData.GoodsID;
            goodsPack.Stars = gpData.Star;
            goodsPack.EnhanceLevel = gpData.EnhanceLevel;
            goodsPack.PosX = gpData.PosX;
            goodsPack.PosY = gpData.PosY;
            goodsPack.LifeTimeTicks = gpData.LifeTime;
            goodsPack.LinesCount = gpData.LinesCount;

            /// Tìm đối tượng cũ
            GameObject oldObject = KTObjectPoolManager.Instance.FindSpawn(x => x.name == name);
            /// Nếu tồn tại
            if (oldObject != null)
            {
                /// Trả lại Pool
                KTObjectPoolManager.Instance.ReturnToPool(oldObject);
            }

            VLTK.Control.Component.Item itemObj = KTObjectPoolManager.Instance.Instantiate<VLTK.Control.Component.Item>("Item");
            itemObj.name = name;

            goodsPack.Role2D = itemObj.gameObject;
            itemObj.RefObject = goodsPack;
            itemObj.gameObject.SetActive(true);
            itemObj.gameObject.transform.localPosition = new Vector2(gpData.PosX, gpData.PosY);
            ColorUtility.TryParseHtmlString(gpData.HTMLColor, out Color nameColor);
            itemObj.NameColor = nameColor;
            itemObj.Data = new GoodsData()
            {
                GoodsID = gpData.GoodsID,
                GCount = gpData.GoodCount,
            };
            itemObj.UpdateData();
            itemObj.Play();

            this.Add(goodsPack);
            goodsPack.Start();

            return goodsPack;
        }

        /// <summary>
        /// Xóa vật phẩm rơi ở Map tương ứng khỏi hệ thống
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="gpAutoID"></param>
        /// <returns></returns>
        public bool DelGoodsPack(int gpAutoID)
        {
            string gpName = string.Format("GoodsPack_{0}", gpAutoID);
            GGoodsPack goodsPack = this.FindName(gpName) as GGoodsPack;
            this.waitToBeAddedGoodsPack.RemoveAll(x => x.AutoID == gpAutoID);

            if (null != goodsPack)
            {
                KTGlobal.RemoveObject(goodsPack, true);
            }

            return true;
        }
        #endregion
    }
}
