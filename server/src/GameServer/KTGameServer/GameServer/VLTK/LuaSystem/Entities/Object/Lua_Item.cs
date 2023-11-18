using GameServer.KiemThe.Core.Item;
using GameServer.Logic;
using MoonSharp.Interpreter;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng vật phẩm
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Item
    {
        #region Base for all objects
        /// <summary>
        /// Đối tượng tham chiếu trong hệ thống
        /// </summary>
        [MoonSharpHidden]
        public GoodsData RefObject { get; set; }

        /// <summary>
        /// Trả về DbID đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.RefObject.Id;
        }

        /// <summary>
        /// Trả về tên đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            if (!ItemManager._TotalGameItem.TryGetValue(this.RefObject.GoodsID, out ItemData itemData))
            {
                return "";
            }
            return itemData.Name;
        }
        #endregion

        #region Base for all scene-objects
        /// <summary>
        /// Bản đồ hiện tại
        /// </summary>
        [MoonSharpHidden]
        public Lua_Scene CurrentScene { get; set; }
        #endregion

        /// <summary>
        /// Trả về ID vật phẩm trong file cấu hình
        /// </summary>
        /// <returns></returns>
        public int GetItemID()
        {
            if (!ItemManager._TotalGameItem.TryGetValue(this.RefObject.GoodsID, out ItemData itemData))
            {
                return -1;
            }
            return itemData.ItemID;
        }

        /// <summary>
        /// Thực thi sự kiện dùng vật phẩm thành công
        /// </summary>
        /// <param name="player"></param>
        public void FinishUsing(Lua_Player player)
        {
            /// Xóa vật phẩm sau khi sử dụng nếu có
            ItemManager.DeductItemOnUse(player.RefObject, this.RefObject, "LUACALL");
        }

        /// <summary>
        /// Trả về ItemDataTemmplate
        /// </summary>
        /// <returns></returns>
        [MoonSharpHidden]
        public ItemData GetItemData()
        {
            ItemData itemData = ItemManager.GetItemTemplate(this.RefObject.GoodsID);
            if (itemData == null)
            {
                return null;
            }
            else
            {
                return itemData;
            }
        }

        /// <summary>
        /// Kiểm tra xem vật phẩm có tồn tại không
        /// </summary>
        /// <returns></returns>
        public bool HasExist()
        {
            return this.GetItemData() != null;
        }

        /// <summary>
        /// Trả ra Genre của item
        /// </summary>
        /// <returns></returns>
        public int GetGenre()
        {
            if (this.GetItemData() != null)
            {
                return GetItemData().Genre;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Trả về value của vật phẩm
        /// </summary>
        /// <returns></returns>
        public int GetItemValue()
        {
            if (this.GetItemData() != null)
            {
                return GetItemData().ItemValue;
            }
            else
            {
                return -1;
            }
        }
        public int GetItemReqLevel()
        {
            //if (this.GetItemData() != null)
            //{
            //    return GetItemData().ReqLevel;
            //}
            //else
            //{
            //    return -1;
            //}

            return -1;
        }

        /// <summary>
        /// Trả về level của vật phẩm
        /// </summary>
        /// <returns></returns>
        public int GetItemLevel()
        {
            if (this.GetItemData() != null)
            {
                return GetItemData().Level;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Trả ra kiểu 
        /// </summary>
        /// <returns></returns>
        public int GetParticularType()
        {
            if (this.GetItemData() != null)
            {
                return GetItemData().ParticularType;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Lấy ra loại đồ
        /// </summary>
        /// <returns></returns>
        public int GetCategory()
        {
            if (this.GetItemData() != null)
            {
                return -1;
            }
            else
            {
                return -1;
            }
        }

        

        /// <summary>
        /// Trả về danh sách ExtParam của vật phẩm
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, int> GetExtParams()
        {
            /// Kết quả
            Dictionary<int, int> dict = new Dictionary<int, int>();
            /// Thông tin vật phẩm
            ItemData itemData = this.GetItemData();
            /// Nếu không tồn tại
            if (itemData == null)
            {
                return dict;
            }

            /// Nếu không tồn tại ExtParams
            if (itemData.ListExtPram == null)
            {
                return dict;
            }

            /// Duyệt danh sách thêm vào kết quả
            foreach (ExtPram extParam in itemData.ListExtPram)
            {
                dict[extParam.Index] = extParam.Pram;
            }
            /// Trả ra kết quả
            return dict;
        }

        /// <summary>
        /// Trả về ExtParam của vật phẩm tại vị trí tương ứng
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public int GetExtParam(int idx)
        {
            /// Kết quả
            int result = -1;
            /// Thông tin vật phẩm
            ItemData itemData = this.GetItemData();
            /// Nếu không tồn tại
            if (itemData == null)
            {
                return result;
            }

            /// Nếu không tồn tại ExtParams
            if (itemData.ListExtPram == null)
            {
                return result;
            }

            /// Thông tin Param
            ExtPram extParam = itemData.ListExtPram.Where(x => x.Index == idx).FirstOrDefault();
            /// Nếu không tìm thấy
            if (extParam == null)
            {
                return result;
            }
            return extParam.Pram;
        }

        /// <summary>
        /// Trả về trạng thái khóa hay không khóa
        /// </summary>
        /// <returns></returns>
        public int GetBindingState()
		{
            return this.RefObject.Binding;
		}
    }
}
