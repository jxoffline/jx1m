using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data
{
    [ProtoContract]
    public class RecoreRanking
    {
        /// <summary>
        /// Thứ tự xếp hạng
        /// </summary>
        [ProtoMember(1)]
        public int Index { get; set; }

        /// <summary>
        /// ID người chơi
        /// </summary>
        [ProtoMember(2)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên người chơi
        /// </summary>
        [ProtoMember(3)]
        public string RoleName { get; set; }

        /// <summary>
        /// Tổng giá trị
        /// </summary>
        [ProtoMember(4)]
        public int TotalValue { get; set; }

        /// <summary>
        /// Kiểu Rank
        /// </summary>
        [ProtoMember(5)]
        public int RankType { get; set; }


    }
}
