using System.Collections.Concurrent;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Danh sách kỹ năng cộng thêm
    /// </summary>
    public partial class SkillTree
    {
        /// <summary>
        /// Danh sách kỹ năng đơn lẻ cộng thêm
        /// </summary>
        private readonly ConcurrentDictionary<int, int> listAdditionSkillLevel = new ConcurrentDictionary<int, int>();

        private int _AllSkillBonusLevel = 0;
        /// <summary>
        /// Điểm cộng thêm cho tất cả kỹ năng
        /// </summary>
        public int AllSkillBonusLevel
        {
            get
            {
                return this._AllSkillBonusLevel;
            }
            set
            {
                this._AllSkillBonusLevel = value;
            }
        }

        /// <summary>
        /// Trả về cấp độ cộng thêm của kỹ năng tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public int GetAdditionSkillLevel(int skillID)
        {
            /// Nếu chưa tồn tại
            if (!this.listAdditionSkillLevel.ContainsKey(skillID))
            {
                /// Trả về 0
                return 0;
            }
            /// Trả về kết quả
            return this.listAdditionSkillLevel[skillID];
        }

        /// <summary>
        /// Thêm kỹ năng đơn lẻ cộng thêm
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="level"></param>
        public void AddAdditionSkill(int skillID, int level)
        {
            /// Nếu chưa tồn tại
            if (!this.listAdditionSkillLevel.ContainsKey(skillID))
            {
                /// Tạo mới
                this.listAdditionSkillLevel[skillID] = 0;
            }
            /// Tăng cấp
            this.listAdditionSkillLevel[skillID] += level;

            /// Xây cây kỹ năng
            this.ExportSkillTree();
        }

        /// <summary>
        /// Xóa kỹ năng đơn lẻ cộng thêm
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="level"></param>
        public void RemoveAdditionSkill(int skillID, int level)
        {
            /// Nếu không tồn tại
            if (!this.listAdditionSkillLevel.ContainsKey(skillID))
            {
                /// Bỏ qua
                return;
            }
            /// Giảm cấp
            this.listAdditionSkillLevel[skillID] -= level;
            /// Nếu âm
            if (this.listAdditionSkillLevel[skillID] < 0)
            {
                /// Set về 0
                this.listAdditionSkillLevel[skillID] = 0;
            }

            /// Xây cây kỹ năng
            this.ExportSkillTree();
        }
    }
}
