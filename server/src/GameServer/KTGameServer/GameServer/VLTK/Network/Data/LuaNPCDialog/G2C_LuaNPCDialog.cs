using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Server => Client
    /// <para>Sự kiện mở khung thoại NPC</para>
    /// </summary>
    [ProtoContract]
    public class G2C_LuaNPCDialog
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
        /// Text nội dung
        /// </summary>
        [ProtoMember(3)]
        public string Text { get; set; }

        /// <summary>
        /// Sự lựa chọn
        /// </summary>
        [ProtoMember(4)]
        public Dictionary<int, string> Selections { get; set; }

        /// <summary>
        /// Danh sách lựa chọn vật phẩm
        /// </summary>
        [ProtoMember(5)]
        public List<DialogItemSelectionInfo> Items { get; set; }

        /// <summary>
        /// Vật phẩm có thể lựa chọn không
        /// </summary>
        [ProtoMember(6)]
        public bool ItemSelectable { get; set; }

        /// <summary>
        /// Danh sách các tham biến đi kèm
        /// </summary>
        [ProtoMember(7)]
        public Dictionary<int, string> OtherParams { get; set; }

        /// <summary>
        /// Text danh sách vật phẩm
        /// </summary>
        [ProtoMember(8)]
        public string ItemHeaderString { get; set; }
    }
}
