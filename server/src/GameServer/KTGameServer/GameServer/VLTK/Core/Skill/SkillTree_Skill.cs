using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý Logic kỹ năng
    /// </summary>
    public partial class SkillTree
    {
        /// <summary>
        /// Danh sách kỹ năng mà nhân vật học được
        /// <para>Key: ID kỹ năng</para>
        /// <para>Value: Cấp độ kỹ năng</para>
        /// </summary>
        private readonly ConcurrentDictionary<int, SkillLevelRef> listStudiedSkills = new ConcurrentDictionary<int, SkillLevelRef>();

        /// <summary>
        /// Điểm kỹ năng đã cộng vào
        /// </summary>
        public int AddedSkillPoints
        {
            get
            {
                return this.listStudiedSkills.Where(x => x.Value.CanAddPoint && !x.Value.Data.IsExpSkill).Sum(x => x.Value.AddedLevel);
            }
        }

        /// <summary>
        /// Danh sách kỹ năng tự thăng kinh nghiệm
        /// </summary>
        private readonly ConcurrentDictionary<int, SkillLevelRef> listExpSkills = new ConcurrentDictionary<int, SkillLevelRef>();

        /// <summary>
        /// Trả về danh sách kỹ năng tự tăng kinh nghiệm
        /// </summary>
        /// <returns></returns>
        public List<SkillLevelRef> GetExpSkills()
        {
            /// Tạo mới
            List<SkillLevelRef> skills = new List<SkillLevelRef>();
            /// Danh sách theo ID
            List<int> skillIDs = this.listExpSkills.Keys.ToList();
            /// Duyệt danh sách
            foreach (int skillID in skillIDs)
            {
                /// Thông tin kỹ năng tương ứng
                SkillLevelRef skill = this.GetSkillLevelRef(skillID);
                /// Toác
                if (skill == null)
                {
                    /// Bỏ qua
                    continue;
                }
                /// Thêm vào danh sách
                skills.Add(skill);
            }
            /// Trả về kết quả
            return skills;
        }

        /// <summary>
        /// Danh sách kỹ năng của người chơi
        /// </summary>
        public List<SkillLevelRef> ListSkills
        {
            get
            {
                /// Tạo mới
                List<SkillLevelRef> skills = new List<SkillLevelRef>();
                /// Danh sách theo ID
                List<int> skillIDs = this.listStudiedSkills.Keys.ToList();
                /// Duyệt danh sách
                foreach (int skillID in skillIDs)
                {
                    /// Thông tin kỹ năng tương ứng
                    SkillLevelRef skill = this.GetSkillLevelRef(skillID);
                    /// Toác
                    if (skill == null)
                    {
                        /// Bỏ qua
                        continue;
                    }
                    /// Thêm vào danh sách
                    skills.Add(skill);
                }
                /// Trả về kết quả
                return skills;
            }
        }

        /// <summary>
        /// Hàm này gọi đến khi có kỹ năng thăng cấp tự động
        /// </summary>
        /// <param name="skill"></param>
        public void OnSkillLevelUp(SkillLevelRef skill)
        {
            /// Nếu là kỹ năng bị động hoặc vòng sáng đang được kích hoạt
            if (skill.Data.Type == 3 || (skill.Data.IsArua && this.Player.Buffs.CurrentArua != null && this.Player.Buffs.CurrentArua.Skill.SkillID == skill.SkillID))
            {
                /// Thực hiện lại kỹ năng bị động
                KTSkillManager.ProcessPassiveSkill(this.Player, skill);
            }

            /// Thực hiện cập nhật danh sách hỗ trợ sát thương cho kỹ năng khác
            this.DoProcessSkillAppendDamages(skill.Data, skill.Level, skill.Level);

            /// Đổ dữ liệu về RoleDataEx
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Thêm kỹ năng
        /// <para>Kỹ năng được thêm vào sẽ mang cấp 0</para>
        /// </summary>
        /// <param name="id"></param>
        public void AddSkill(int id)
        {
            /// Kỹ năng tương ứng
            SkillLevelRef skillRef = this.GetSkillLevelRef(id);
            /// Nếu không tồn tại
            if (skillRef == null)
            {
                SkillDataEx skillData = KSkill.GetSkillData(id);
                if (skillData != null && this.GetSkillLevelRef(id) == null)
                {
                    SkillLevelRef levelRef = new SkillLevelRef()
                    {
                        AddedLevel = 0,
                        Data = skillData,
                        CanStudy = true,
                        Exp = 0,
                    };
                    this.listStudiedSkills[skillData.ID] = levelRef;

                    if (!levelRef.CanStudy)
                    {
                        /// Cái này đảm bảo cái hàm Verify đằng sau chắc chắn nó sẽ trả ra lỗi
                        levelRef.AddedLevel = 999999;
                    }
                    else
                    {
                        KT_TCPHandler.AddSkillToDB(this.Player, id);
                    }
                }
            }
            this.Player.RefreshSkillPoints();

            /// Đổ dữ liệu về RoleDataEx và lưu vào DB
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Thêm danh sách kỹ năng
        /// <para>Kỹ năng được thêm vào sẽ mang cấp 0</para>
        /// </summary>
        /// <param name="skillIds"></param>
        /// <param name="notifyToClient"></param>
        public void AddSkills(ICollection<int> skillIds)
        {
            foreach (int id in skillIds)
            {
                /// Kỹ năng tương ứng
                SkillLevelRef skillRef = this.GetSkillLevelRef(id);
                /// Nếu không tồn tại
                if (skillRef == null)
                {
                    SkillDataEx skillData = KSkill.GetSkillData(id);
                    if (skillData != null && this.GetSkillLevelRef(id) == null)
                    {
                        SkillLevelRef levelRef = new SkillLevelRef()
                        {
                            AddedLevel = 0,
                            Data = skillData,
                            CanStudy = true,
                            Exp = 0,
                        };
                        this.listStudiedSkills[skillData.ID] = levelRef;

                        if (!levelRef.CanStudy)
                        {
                            /// Cái này đảm bảo cái hàm Verify đằng sau chắc chắn nó sẽ trả ra lỗi
                            levelRef.AddedLevel = 9999999;

                            break;
                        }

                        KT_TCPHandler.AddSkillToDB(this.Player, id);
                    }
                }
            }
            this.Player.RefreshSkillPoints();

            /// Đổ dữ liệu về RoleDataEx và lưu vào DB
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Cập nhật dữ liệu cấp độ kỹ năng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="level"></param>
        public void AddSkillLevel(int id, int level)
        {
            /// Kỹ năng tương ứng
            SkillLevelRef skillRef = this.GetSkillLevelRef(id);
            /// Nếu tồn tại
            if (skillRef != null)
            {
                /// Nếu là kỹ năng bị động hoặc vòng sáng đang được kích hoạt
                if (skillRef.Data.Type == 3 || (skillRef.Data.IsArua && this.Player.Buffs.CurrentArua != null && this.Player.Buffs.CurrentArua.Skill.SkillID == skillRef.SkillID))
                {
                    /// Lấy giá trị Buff cũ
                    BuffDataEx buff = this.Player.Buffs.GetBuff(skillRef.SkillID);
                    if (buff != null)
                    {
                        /// Xóa Buff cũ đi
                        this.Player.Buffs.RemoveBuff(buff, true, false);
                    }
                }
                int oldLevel = skillRef.Level;
                skillRef.AddedLevel += level;
                skillRef.AddedLevel = System.Math.Min(skillRef.AddedLevel, skillRef.Data.MaxSkillLevel);

                /// Nếu là kỹ năng bị động hoặc vòng sáng đang được kích hoạt
                if (skillRef.Data.Type == 3 || (skillRef.Data.IsArua && this.Player.Buffs.CurrentArua != null && this.Player.Buffs.CurrentArua.Skill.SkillID == skillRef.SkillID))
                {
                    /// Thực hiện lại kỹ năng bị động
                    KTSkillManager.ProcessPassiveSkill(this.Player, skillRef);
                }

                /// Thực hiện cập nhật danh sách hỗ trợ sát thương cho kỹ năng khác
                this.DoProcessSkillAppendDamages(skillRef.Data, oldLevel, skillRef.Level);

                /// Cập nhật thông tin kỹ năng vào DB
                KT_TCPHandler.UpdateSkillInfoFromDB(this.Player, id, skillRef.AddedLevel, this.GetSkillLastUsedTick(id), this.GetSkillCooldown(id), skillRef.Exp);
            }
            this.Player.RefreshSkillPoints();

            /// Đổ dữ liệu về RoleDataEx và lưu vào DB
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Cập nhật dữ liệu danh sách cấp độ kỹ năng
        /// </summary>
        /// <param name="data"></param>
        public void AddSkillsLevel(ICollection<KeyValuePair<int, int>> data)
        {
            /// Duyệt danh sách
            foreach (KeyValuePair<int, int> pair in data)
            {
                /// Thông tin kỹ năng tương ứng
                SkillLevelRef skillRef = this.GetSkillLevelRef(pair.Key);
                /// Nếu tồn tại
                if (skillRef != null)
                {
                    /// Nếu là kỹ năng bị động hoặc vòng sáng đang được kích hoạt
                    if (skillRef.Data.Type == 3 || (skillRef.Data.IsArua && this.Player.Buffs.CurrentArua != null && this.Player.Buffs.CurrentArua.Skill.SkillID == skillRef.SkillID))
                    {
                        /// Lấy giá trị Buff cũ
                        BuffDataEx buff = this.Player.Buffs.GetBuff(skillRef.SkillID);
                        if (buff != null)
                        {
                            /// Xóa Buff cũ đi
                            this.Player.Buffs.RemoveBuff(buff, true, false);
                        }
                    }

                    int oldLevel = skillRef.Level;
                    skillRef.AddedLevel += pair.Value;
                    skillRef.AddedLevel = System.Math.Min(skillRef.AddedLevel, skillRef.Data.MaxSkillLevel);

                    /// Nếu là kỹ năng bị động hoặc vòng sáng đang được kích hoạt
                    if (skillRef.Data.Type == 3 || (skillRef.Data.IsArua && this.Player.Buffs.CurrentArua != null && this.Player.Buffs.CurrentArua.Skill.SkillID == skillRef.SkillID))
                    {
                        /// Thực hiện lại kỹ năng bị động
                        KTSkillManager.ProcessPassiveSkill(this.Player, skillRef);
                    }

                    /// Thực hiện cập nhật danh sách hỗ trợ sát thương cho kỹ năng khác
                    this.DoProcessSkillAppendDamages(skillRef.Data, oldLevel, skillRef.Level);

                    /// Cập nhật thông tin kỹ năng vào DB
                    KT_TCPHandler.UpdateSkillInfoFromDB(this.Player, pair.Key, skillRef.AddedLevel, this.GetSkillLastUsedTick(skillRef.SkillID), this.GetSkillCooldown(skillRef.SkillID), skillRef.Exp);
                }
            }
            this.Player.RefreshSkillPoints();

            /// Đổ dữ liệu về RoleDataEx và lưu vào DB
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Tẩy điểm kỹ năng trong danh sách tương ứng
        /// </summary>
        /// <param name="data"></param>
        public void ResetSkillsLevel(ICollection<int> data)
        {
            /// Duyệt danh sách
            foreach (int id in data)
            {
                /// Thông tin kỹ năng tương ứng
                SkillLevelRef skill = this.GetSkillLevelRef(id);
                /// Nếu tồn tại
                if (skill != null)
                {
                    /// Nếu là kỹ năng tự lên cấp
                    if (skill.Data.IsExpSkill)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Xóa Buff tương ứng nếu là kỹ năng bị động
                    if (skill.Data.Type == 3)
                    {
                        this.Player.Buffs.RemoveBuff(skill.SkillID);
                    }

                    /// Thiết lập cấp độ về 0
                    this.listStudiedSkills[id].AddedLevel = 0;

                    /// Xóa sát thương hỗ trợ
                    this.appendSkillDamages.TryRemove(id, out _);
                    this.appendSkillDamagesPercent.TryRemove(id, out _);

                    /// Lưu kỹ năng vào DB
                    KT_TCPHandler.UpdateSkillInfoFromDB(this.Player, skill.SkillID, skill.AddedLevel, this.GetSkillLastUsedTick(skill.SkillID), this.GetSkillCooldown(skill.SkillID), skill.Exp);
                }
            }
            this.Player.RefreshSkillPoints();

            /// Đổ dữ liệu về RoleDataEx và lưu vào DB
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Tẩy lại toàn bộ điểm kỹ năng
        /// </summary>
        public void ResetAllSkillsLevel()
        {
            /// Danh sách kỹ năng theo ID
            List<int> skillIDs = this.listStudiedSkills.Keys.ToList();
            /// Duyệt toàn bộ danh sách
            foreach (int skillID in skillIDs)
            {
                /// Thông tin kỹ năng tương ứng
                SkillLevelRef skillRef = this.GetSkillLevelRef(skillID);
                /// Toác gì đó
                if (skillRef == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Nếu là kỹ năng tự lên cấp
                if (skillRef.Data.IsExpSkill)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Xóa Buff tương ứng nếu là kỹ năng bị động
                if (skillRef.Data.Type == 3)
                {
                    this.Player.Buffs.RemoveBuff(skillRef.SkillID);
                }

                /// Thiết lập cấp độ về 0
                skillRef.AddedLevel = 0;

                /// Xóa sát thương hỗ trợ
                this.appendSkillDamages.Clear();
                this.appendSkillDamagesPercent.Clear();

                /// Lưu kỹ năng vào DB
                KT_TCPHandler.UpdateSkillInfoFromDB(this.Player, skillRef.SkillID, skillRef.AddedLevel, this.GetSkillLastUsedTick(skillRef.SkillID), this.GetSkillCooldown(skillRef.SkillID), skillRef.Exp);
            }
            this.Player.RefreshSkillPoints();

            /// Đổ dữ liệu về RoleDataEx và lưu vào DB
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Xóa kỹ năng
        /// </summary>
        public void RemoveSkill(int id)
        {
            /// Kỹ năng tương ứng
            SkillLevelRef skill = this.GetSkillLevelRef(id);
            /// Nếu tồn tại
            if (skill != null)
            {
                /// Xóa Buff tương ứng nếu là kỹ năng bị động
                if (skill.Data.Type == 3)
                {
                    this.Player.Buffs.RemoveBuff(skill.SkillID);
                }

                /// Xóa kỹ năng khỏi danh sách
                this.listStudiedSkills.TryRemove(id, out _);

                /// Thực hiện giảm sát thương được hỗ trợ
                this.DoProcessSkillAppendDamages(skill.Data, skill.Level, 0);

                /// Xóa kỹ năng khỏi DB
                KT_TCPHandler.DeleteSkillFromDB(this.Player, id);
            }
            this.Player.RefreshSkillPoints();

            /// Đổ dữ liệu về RoleDataEx và lưu vào DB
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Xóa kỹ năng
        /// </summary>
        /// <param name="skillIds"></param>
        public void RemoveSkills(ICollection<int> skillIds)
        {
            /// Duyệt danh sách
            foreach (int id in skillIds)
            {
                /// Kỹ năng tương ứng
                SkillLevelRef skill = this.GetSkillLevelRef(id);
                /// Nếu tồn tại
                if (skill != null)
                {
                    /// Xóa Buff tương ứng nếu là kỹ năng bị động
                    if (skill.Data.Type == 3)
                    {
                        this.Player.Buffs.RemoveBuff(skill.SkillID);
                    }

                    /// Xóa kỹ năng khỏi danh sách
                    this.listStudiedSkills.TryRemove(id, out _);

                    /// Xóa kỹ năng khỏi DB
                    KT_TCPHandler.DeleteSkillFromDB(this.Player, id);
                }
            }
            this.Player.RefreshSkillPoints();

            /// Đổ dữ liệu về RoleDataEx và lưu vào DB
            this.ExportSkillTree();

            /// Thực hiện lại các kỹ năng được hỗ trợ
            this.ProcessEnchantSkills();
        }

        /// <summary>
        /// Trả về thông tin kỹ năng
        /// </summary>
        /// <param name="id"></param>
        public SkillLevelRef GetSkillLevelRef(int id)
        {
            if (this.listStudiedSkills.TryGetValue(id, out SkillLevelRef levelRef))
            {
                /// Cập nhật lại điểm cộng thêm
                levelRef.BonusLevel = this._AllSkillBonusLevel + this.GetAdditionSkillLevel(levelRef.SkillID);
                /// Trả về kết quả
                return levelRef;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Kiểm tra trong SkillTree có kỹ năng tương ứng không
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool HasSkill(int id)
        {
            return this.GetSkillLevelRef(id) != null;
        }
    }
}
