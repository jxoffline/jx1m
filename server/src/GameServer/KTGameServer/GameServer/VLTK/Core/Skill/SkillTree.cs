using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Data;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Danh sách kỹ năng của nhân vật
    /// </summary>
    public partial class SkillTree
    {
        /// <summary>
        /// Đối tượng chủ nhân
        /// </summary>
        public KPlayer Player { get; private set; }

        /// <summary>
        /// Danh sách kỹ năng của nhân vật
        /// </summary>
        /// <param name="player"></param>
        public SkillTree(KPlayer player)
        {
            this.Player = player;
            this.BuildSkillTree();
        }

        /// <summary>
        /// Xây SkillTree dựa vào dữ liệu nhân vật
        /// </summary>
        private void BuildSkillTree()
        {
            /// Làm rỗng danh sách kỹ năng đã học
            this.listStudiedSkills.Clear();
            /// Làm rỗng danh sách kỹ năng tự thăng kinh nghiệm
            this.listExpSkills.Clear();

            if (this.Player.SkillDataList != null)
            {
                foreach (SkillData dbSkillData in this.Player.SkillDataList)
                {
                    SkillDataEx skillData = KSkill.GetSkillData(dbSkillData.SkillID);
                    if (skillData != null)
                    {
                        SkillLevelRef levelRef = new SkillLevelRef()
                        {
                            AddedLevel = dbSkillData.SkillLevel,
                            Data = skillData,
                            CanStudy = true,
                            Exp = dbSkillData.Exp,
                        };
                        this.listStudiedSkills[skillData.ID] = levelRef;

                        /// Nếu là kỹ năng tự thăng kinh nghiệm
                        if (levelRef.Data.IsExpSkill)
                        {
                            /// Thêm vào danh sách
                            this.listExpSkills[levelRef.SkillID] = levelRef;
                        }
                    }

                    /// Phục hồi dữ liệu Cooldown lấy từ DB
                    this.AddSkillCooldown(dbSkillData.SkillID, dbSkillData.LastUsedTick, dbSkillData.CooldownTick);
                }
            }
        }

        /// <summary>
        /// Đồng bộ dữ liệu SkillTree vào DB
        /// </summary>
        public void ExportSkillTree()
        {
            /// Làm rỗng danh sách kỹ năng tự thăng kinh nghiệm
            this.listExpSkills.Clear();

            /// Tạo mới danh sách
            List<SkillData> dbSkillDataList = new List<SkillData>();
            /// Danh sách kỹ năng theo ID
            List<int> skillIDs = this.listStudiedSkills.Keys.ToList();
            /// Duyệt danh sách
            foreach (int skillID in skillIDs)
            {
                /// Thông tin kỹ năng tương ứng
                SkillLevelRef levelRef = this.GetSkillLevelRef(skillID);
                /// Toác gì đó
                if (levelRef == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu là kỹ năng tự thăng kinh nghiệm
                if (levelRef.Data.IsExpSkill)
                {
                    /// Thêm vào danh sách
                    this.listExpSkills[levelRef.SkillID] = levelRef;
                }

                SkillData dbSkillData = new SkillData()
                {
                    SkillID = levelRef.SkillID,
                    SkillLevel = levelRef.AddedLevel,
                    CanStudy = levelRef.CanStudy,
                    LastUsedTick = this.GetSkillLastUsedTick(levelRef.SkillID),
                    CooldownTick = this.GetSkillCooldown(levelRef.SkillID),
                    Exp = levelRef.Exp,
                };
                dbSkillDataList.Add(dbSkillData);
            }

            this.Player.SkillDataList = dbSkillDataList;

            /// Gửi tín hiệu thay đổi danh sách kỹ năng về Client
            KT_TCPHandler.SendRenewSkillList(this.Player);
        }
    }
}