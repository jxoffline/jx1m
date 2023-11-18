using GameServer.KiemThe.Utilities;
using System;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Ref data của kỹ năng theo cấp
    /// </summary>
    public class SkillLevelRef
    {
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public int SkillID
        {
            get
            {
                if (this.Data == null)
                {
                    return -1;
                }
                return this.Data.ID;
            }
        }

        /// <summary>
        /// Dữ liệu kỹ năng tổng thể
        /// </summary>
        public SkillDataEx Data { get; set; }

        /// <summary>
        /// Có thể học được không (áp dụng với các kỹ năng phải làm nhiệm vụ mới học được, ví dụ kỹ năng 110)
        /// </summary>
        public bool CanStudy { get; set; }

        /// <summary>
        /// Cấp độ kỹ năng (gồm số điểm đã cộng và cấp độ được cộng thêm từ đồ, buff hoặc kỹ năng khác)
        /// </summary>
        public int Level
        {
            get
            {
                if (this.AddedLevel == 0)
                {
                    return 0;
                }
                return this.AddedLevel + this.BonusLevel;
            }
        }

        /// <summary>
        /// Cấp độ đã cộng điểm
        /// </summary>
        public int AddedLevel { get; set; }

        /// <summary>
        /// Cấp độ được cộng thêm từ đồ, buff hoặc kỹ năng khác
        /// </summary>
        public int BonusLevel { get; set; }

        /// <summary>
        /// Có thể cộng điểm không
        /// </summary>
        public bool CanAddPoint
        {
            get
            {
                return this.Data.CanAddPoint && this.Data.MaxSkillLevel > 0;
            }
        }

        /// <summary>
        /// PropertyDictionary của kỹ năng theo cấp độ
        /// </summary>
        public PropertyDictionary Properties
        {
            get
            {
                if (this.Data.Properties.TryGetValue(this.Level, out PropertyDictionary pd))
                {
                    return pd;
                }
                return null;
            }
        }

        /// <summary>
        /// Kinh nghiệm
        /// <para>Hoặc cũng có thể là mã hóa cho thời gian phục hồi chiêu dùng cho AI quái</para>
        /// </summary>
        public int Exp { get; set; }

        /// <summary>
        /// Tạo mới một bản sao của đối tượng
        /// </summary>
        /// <returns></returns>
        public SkillLevelRef Clone()
        {
            SkillLevelRef skill = new SkillLevelRef()
            {
                AddedLevel = this.AddedLevel,
                BonusLevel = this.BonusLevel,
                CanStudy = this.CanStudy,
                Data = this.Data,
            };
            return skill;
        }

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID: {0}, Name: {1}, Level: {2}", this.Data.ID, this.Data.Name, this.Level);
        }
    }
}