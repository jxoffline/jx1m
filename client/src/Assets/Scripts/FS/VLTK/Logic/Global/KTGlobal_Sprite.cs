using FS.GameEngine.Interface;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Factory;
using FS.VLTK.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Có phải Click vào mục tiêu không
        /// <para>Biến này dùng để đánh dấu, nếu Click vào mục tiêu thì sẽ thực hiện chọn mục tiêu thay vì dịch chuyển đến vị trí Click</para>
        /// </summary>
        public static bool IsClickOnTarget { get; set; } = false;

        #region Tìm kiếm Sprite
        /// <summary>
        /// Trả về danh sách đối tượng có ResID tương ứng
        /// </summary>
        /// <param name="resID"></param>
        /// <returns></returns>
        public static List<GSprite> FindSpritesByResID(int resID)
        {
            return KTObjectsManager.Instance.FindObjects<GSprite>(x => x != null && x.ComponentMonster != null && x.ComponentMonster.StaticID == resID).ToList();
        }

        /// <summary>
        /// Tìm đối tượng có ID tương ứng
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GSprite FindSpriteByID(int roleID)
        {
            GSprite sprite = Global.Data.GameScene.FindSprite(roleID);
            return sprite;
        }

        /// <summary>
        /// Tìm người chơi có tên tương ứng
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public static GSprite FindPlayerByName(string roleName)
        {
            /// Nếu là bản thân
            if (Global.Data.RoleData.RoleName == roleName)
            {
                /// Trả về kết quả
                return Global.Data.GameScene.GetLeader();
            }
            /// Nếu là người chơi khác
            else
            {
                /// Thông tin người chơi khác
                RoleData rd = Global.Data.OtherRoles.Values.Where(x => x.RoleName == roleName).FirstOrDefault();
                /// Nếu không tìm thấy
                if (rd == null)
                {
                    /// Toác
                    return null;
                }
                /// Tìm đối tượng tương ứng
                GSprite sprite = KTGlobal.FindSpriteByID(rd.RoleID);
                /// Trả về kết quả
                return sprite;
            }
        }

        /// <summary>
        /// Tìm NPC gần nhất theo ID Res
        /// </summary>
        /// <param name="npcResID"></param>
        /// <returns></returns>
        public static GSprite FindNearestNPCByResID(int npcResID)
        {
            return KTObjectsManager.Instance.FindObjects<GSprite>((sprite) => {
                return sprite != null && sprite.ComponentMonster != null && sprite.SpriteType == GSpriteTypes.NPC && sprite.ComponentMonster.StaticID == npcResID;
            }).MinBy(x => Vector2.Distance(Global.Data.Leader.PositionInVector2, x.PositionInVector2));
        }

        /// <summary>
        /// Tìm NPC gần nhất theo tên
        /// </summary>
        /// <param name="npcName"></param>
        /// <returns></returns>
        public static GSprite FindNearestNPCByName(string npcName)
        {
            return KTObjectsManager.Instance.FindObjects<GSprite>((sprite) => {
                return sprite != null && sprite.ComponentMonster != null && sprite.SpriteType == GSpriteTypes.NPC && sprite.RoleName == npcName;
            }).MinBy(x => Vector2.Distance(Global.Data.Leader.PositionInVector2, x.PositionInVector2));
        }
        #endregion

        #region Quản lý Sprite
        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        /// <param name="clearFromParent"></param>
        /// <param name="obj"></param>
        public static void RemoveObject(IObject obj, bool clearFromParent = true)
        {
            if (obj == null)
			{
                return;
			}

            if (obj is GSprite sprite)
            {
                sprite.StopMove();
            }

            if (clearFromParent)
            {
                /// Xóa khỏi danh sách quản lý
                KTObjectsManager.Instance.RemoveObject(obj);
            }

            /// Xóa đối tượng
            obj.Destroy();
        }

        /// <summary>
        /// Xóa toàn bộ các đối tượng trong bản đồ
        /// </summary>
        public static void RemoveAllObjects()
        {
            List<IObject> objList = KTObjectsManager.Instance.GetObjects();
            foreach (IObject obj in objList)
            {
                if (obj == null)
				{
                    continue;
				}
                /// Thực hiện xóa đối tượng
                obj.Destroy();
            }

            /// Làm rỗng danh sách quản lý
            KTObjectsManager.Instance.Clear();
        }
        #endregion
    }
}
