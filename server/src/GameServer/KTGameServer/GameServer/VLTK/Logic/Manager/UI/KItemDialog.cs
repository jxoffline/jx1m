using GameServer.KiemThe.Core.Task;
using GameServer.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Đối tượng ItemDialog
    /// </summary>
    public class KItemDialog
    {
        /// <summary>
        /// ID tự tăng
        /// </summary>
        private static int AutoID = -1;

        /// <summary>
        /// ID khung
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Chủ nhân
        /// </summary>
        public KPlayer Owner { get; set; }

        /// <summary>
        /// Text nội dung
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Danh sách các sự lựa chọn
        /// </summary>
        public Dictionary<int, string> Selections { get; set; } = new Dictionary<int, string>();

        /// <summary>
        /// Danh sách các thuộc tính đi kèm
        /// </summary>
        public Dictionary<int, string> OtherParams { get; set; } = new Dictionary<int, string>();

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public List<DialogItemSelectionInfo> Items { get; set; } = new List<DialogItemSelectionInfo>();

        /// <summary>
        /// Đánh dấu vật phẩm có được lựa chọn không
        /// </summary>
        public bool ItemSelectable { get; set; } = true;

        /// <summary>
        /// Text danh sách vật phẩm
        /// </summary>
        public string ItemHeaderString { get; set; } = "Danh sách vật phẩm";

        /// <summary>
        /// Sự kiện khi người chơi Click vào Selection
        /// </summary>
        public Action<TaskCallBack> OnSelect { get; set; }

        /// <summary>
        /// Sự kiện khi người chơi Click chọn vật phẩm
        /// </summary>
        public Action<DialogItemSelectionInfo> OnItemSelect { get; set; }

        /// <summary>
        /// Tạo mới đối tượng KNPCDialog
        /// </summary>
        public KItemDialog()
        {
            KItemDialog.AutoID = (KItemDialog.AutoID + 1) % 10000007;
            this.ID = KItemDialog.AutoID;
        }

        /// <summary>
        /// Gửi yêu cầu về Client để hiện khung
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="player"></param>
        public void Show(GoodsData item, KPlayer player)
        {
            /// Gửi gói tin
            KT_TCPHandler.OpenItemDialog(player, item.GoodsID, item.Id, this.ID, this.Text, this.Selections, this.Items, this.ItemSelectable, this.ItemHeaderString, this.OtherParams);
        }

        public void ShowDialog(GoodsData item, KPlayer player)
        {

            KTItemDialogManager.AddItemDialog(this);
            /// Gửi gói tin
            KT_TCPHandler.OpenItemDialog(player, item.GoodsID, item.Id, this.ID, this.Text, this.Selections, this.Items, this.ItemSelectable, this.ItemHeaderString, this.OtherParams);
        }
    }
}
