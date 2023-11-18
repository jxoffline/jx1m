using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Mô tả hệ thống danh vọng
    /// </summary>
    [ProtoContract]
    public class ReputeInfo
    {
        /// <summary>
        /// DbID
        /// <para>Camp: DbID / 100</para>
        /// <para>Class: DbID % 100</para>
        /// </summary>
        [ProtoMember(1)]
        public int DBID { get; set; }

        /// <summary>
        /// Cấp độ hiện tại
        /// </summary>
        [ProtoMember(2)]
        public int Level { get; set; }

        /// <summary>
        /// Kinh nghiệm hiện tại
        /// </summary>
        [ProtoMember(3)]
        public int Exp { get; set; }

        /// <summary>
        /// Loại tương ứng
        /// </summary>
        public int Camp
        {
            get
            {
                return this.DBID / 100;
            }
        }

        /// <summary>
        /// Lớp tương ứng
        /// </summary>
        public int Class
        {
            get
            {
                return this.DBID % 100;
            }
        }
    }
}
