using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Client => Server
    /// <para>Người chơi Click vào sự lựa chọn hoặc chọn vật phẩm</para>
    /// </summary>
    [ProtoContract]
    public class C2G_LuaNPCDialog
    {
        /// <summary>
        /// ID NPCDialog
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// ID NPC
        /// </summary>
        [ProtoMember(2)]
        public int NPCID { get; set; }

        /// <summary>
        /// ID sự lựa chọn
        /// </summary>
        [ProtoMember(3)]
        public int SelectionID { get; set; }

        /// <summary>
        /// Vật phẩm được chọn
        /// </summary>
        [ProtoMember(4)]
        public DialogItemSelectionInfo SelectedItem { get; set; }

        /// <summary>
        /// Danh sách các tham biến đi kèm
        /// </summary>
        [ProtoMember(5)]
        public Dictionary<int, string> OtherParams { get; set; }
    }
}
