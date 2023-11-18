using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace GameDBServer.Logic.Pet
{
    /// <summary>
    /// Thông tin Pet
    /// </summary>
    [ProtoContract]
    public class PetData
    {
        /// <summary>
        /// ID pet
        /// </summary>
        [ProtoMember(1)]
        public int ID { get; set; }

        /// <summary>
        /// ID chủ nhân
        /// </summary>
        [ProtoMember(2)]
        public int RoleID { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        [ProtoMember(3)]
        public int ResID { get; set; }

        /// <summary>
        /// Tên pet
        /// </summary>
        [ProtoMember(4)]
        public string Name { get; set; }

        /// <summary>
        /// Cấp độ Pet
        /// </summary>
        [ProtoMember(5)]
        public int Level { get; set; }

        /// <summary>
        /// Danh sách kỹ năng pet
        /// </summary>
        [ProtoMember(6)]
        public Dictionary<int, int> Skills { get; set; }

        /// <summary>
        /// Giá trị lĩnh ngộ
        /// </summary>
        [ProtoMember(7)]
        public int Enlightenment { get; set; }

        /// <summary>
        /// Trang bị pet
        /// </summary>
        [ProtoMember(8)]
        public Dictionary<int, int> Equips { get; set; }

        /// <summary>
        /// Kinh nghiệm hiện tại
        /// </summary>
        [ProtoMember(9)]
        public int Exp { get; set; }

        /// <summary>
        /// Sức mạnh
        /// </summary>
        [ProtoMember(10)]
        public int Str { get; set; }

        /// <summary>
        /// Thân pháp
        /// </summary>
        [ProtoMember(11)]
        public int Dex { get; set; }

        /// <summary>
        /// Ngoại công
        /// </summary>
        [ProtoMember(12)]
        public int Sta { get; set; }

        /// <summary>
        /// Nội công
        /// </summary>
        [ProtoMember(13)]
        public int Int { get; set; }

        /// <summary>
        /// Tiềm năng
        /// </summary>
        [ProtoMember(14)]
        public int RemainPoints { get; set; }

        /// <summary>
        /// Độ vui vẻ
        /// </summary>
        [ProtoMember(15)]
        public int Joyful { get; set; }

        /// <summary>
        /// Tuổi thọ
        /// </summary>
        [ProtoMember(16)]
        public int Life { get; set; }

        /// <summary>
        /// Sinh lực
        /// </summary>
        [ProtoMember(17)]
        public int HP { get; set; }

        /// <summary>
        /// Vật công ngoại
        /// </summary>
        [ProtoMember(18)]
        public int PAtk { get; set; }

        /// <summary>
        /// Vật công nội
        /// </summary>
        [ProtoMember(19)]
        public int MAtk { get; set; }

        /// <summary>
        /// Chính xác
        /// </summary>
        [ProtoMember(20)]
        public int Hit { get; set; }

        /// <summary>
        /// Né tránh
        /// </summary>
        [ProtoMember(21)]
        public int Dodge { get; set; }

        /// <summary>
        /// Chí mạng
        /// </summary>
        [ProtoMember(22)]
        public int Crit { get; set; }

        /// <summary>
        /// Tốc chạy
        /// </summary>
        [ProtoMember(23)]
        public int MoveSpeed { get; set; }

        /// <summary>
        /// Tốc đánh ngoại công
        /// </summary>
        [ProtoMember(24)]
        public int AtkSpeed { get; set; }

        /// <summary>
        /// Tốc đánh nội công
        /// </summary>
        [ProtoMember(25)]
        public int CastSpeed { get; set; }

        /// <summary>
        /// Kháng băng
        /// </summary>
        [ProtoMember(26)]
        public int IceRes { get; set; }

        /// <summary>
        /// Kháng hỏa
        /// </summary>
        [ProtoMember(27)]
        public int FireRes { get; set; }

        /// <summary>
        /// Kháng vật
        /// </summary>
        [ProtoMember(28)]
        public int PDef { get; set; }

        /// <summary>
        /// Kháng lôi
        /// </summary>
        [ProtoMember(29)]
        public int LightningRes { get; set; }

        /// <summary>
        /// Kháng độc
        /// </summary>
        [ProtoMember(30)]
        public int PoisonRes { get; set; }

        /// <summary>
        /// Sinh lực tối đa
        /// </summary>
        [ProtoMember(31)]
        public int MaxHP { get; set; }
    }
}
