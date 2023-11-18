using System.Collections.Generic;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Client về Server thông báo cộng điểm kỹ năng
    /// </summary>
    [ProtoContract]
    public class C2G_DistributeSkillPoint
    {
        /// <summary>
        /// ID nhánh tu luyện được chọn
        /// </summary>
        [ProtoMember(1)]
        public int SelectedRouteID { get; set; }

        /// <summary>
        /// Danh sách kỹ năng được phân phối theo ID kỹ năng
        /// </summary>
        [ProtoMember(2)]
        public Dictionary<int, int> ListDistributedSkills { get; set; }
    }
}
