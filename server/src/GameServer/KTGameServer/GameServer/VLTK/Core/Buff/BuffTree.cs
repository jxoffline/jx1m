using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Data;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Utilities;
using Server.Tools;
using GameServer.KiemThe.GameDbController;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Cây Buff của đối tượng
    /// <para>Dữ liệu Buff được lưu chung với kỹ năng ở file SkillData</para>
    /// </summary>
    public partial class BuffTree
    {
        /// <summary>
        /// Đối tượng tham chiếu
        /// </summary>
        private readonly GameObject refObject;


        /// <summary>
        /// Cây Buff của đối tượng
        /// </summary>
        public BuffTree(GameObject refObject)
        {
            this.refObject = refObject;
            this.BuildBuffTree();
        }

        #region Core
        /// <summary>
        /// Xây dữ liệu cây Buff từ dữ liệu của người chơi
        /// <para>Nếu không phải người chơi thì hàm này không có tác dụng</para>
        /// </summary>
        private void BuildBuffTree()
        {
            if (this.refObject is KPlayer)
            {
                KPlayer player = this.refObject as KPlayer;

                /// Danh sách Buff cần xóa khi hết thời gian
                List<BufferData> toRemoveBuff = new List<BufferData>();

                this.listBuffs.Clear();

                if (player.BufferDataList != null)
                {
                    /// Duyệt danh sách Buff
                    foreach (BufferData dbBuffData in player.BufferDataList)
                    {
                        /// Nếu Buff đã hết hạn
                        if (dbBuffData.BufferSecs != -1 && KTGlobal.GetCurrentTimeMilis() - dbBuffData.StartTime >= dbBuffData.BufferSecs)
                        {
                            toRemoveBuff.Add(dbBuffData);
                            continue;
                        }

                        /// Nếu không phải loại Buff từ vật phẩm
                        if (string.IsNullOrEmpty(dbBuffData.CustomProperty))
                        {
                            SkillLevelRef skillRef = player.Skills.GetSkillLevelRef(dbBuffData.BufferID);
                            /// Nếu có dữ liệu kỹ năng và đồng thời đây không phải kỹ năng bị động hay vòng sáng
                            if (skillRef != null && skillRef.Data.Type != 3 && !skillRef.Data.IsArua)
                            {
                                BuffDataEx buff = new BuffDataEx()
                                {
                                    Skill = skillRef,
                                    StartTick = dbBuffData.StartTime,
                                    Duration = dbBuffData.BufferSecs,
                                    SaveToDB = true,
                                    CustomProperties = null,
                                };
                                this.listBuffs[buff.Skill.SkillID] = buff;
                                /// Thêm vào Timer
                                KTBuffManager.Instance.AddBuff(this.refObject, buff);
                            }
                        }
                        else
                        {
                            /// Kỹ năng tương ứng
                            SkillDataEx skill = KSkill.GetSkillData(dbBuffData.BufferID);

                            /// Nếu kỹ năng không tồn tại
                            if (skill == null)
                            {
                                continue;
                            }

                            /// Dữ liệu kỹ năng tương ứng
                            SkillLevelRef skillRef = new SkillLevelRef()
                            {
                                Data = skill,
                                AddedLevel = 1,
                                CanStudy = false,
                            };
                            BuffDataEx buff = new BuffDataEx()
                            {
                                Skill = skillRef,
                                Duration = dbBuffData.BufferSecs,
                                LoseWhenUsingSkill = false,
                                SaveToDB = true,
                                StackCount = 1,
                                StartTick = dbBuffData.StartTime,
                                CustomProperties = PropertyDictionary.FromPortableDBString(dbBuffData.CustomProperty),
                            };
                            this.listBuffs[buff.Skill.SkillID] = buff;
                            /// Thêm vào Timer
                            KTBuffManager.Instance.AddBuff(this.refObject, buff);
                        }
                    }
                }

                /// Duyệt danh sách Buff cần xóa do hết hạn
                foreach (BufferData dbBuff in toRemoveBuff)
                {
                    /// Xóa Buff khỏi danh sách Buff của đối tượng
                    player.BufferDataList.Remove(dbBuff);
                    /// Xóa Buff khỏi DB
                    GameDb.DeleteDBBuffer(player, dbBuff.BufferID);
                }

                /// Kích hoạt toàn bộ Buff lấy từ DB ra
                this.AttachAllBuffs();
            }
        }

        /// <summary>
        /// Đồng bộ dữ liệu cây Buff cho người chơi
        /// <para>Nếu không phải người chơi thì hàm này không có tác dụng</para>
        /// </summary>
        /// <param name="includeProperties">Bao gồm cả thuộc tính</param>
        public void ExportBuffTree(bool includeProperties = false)
        {
            if (this.refObject is KPlayer)
            {
                KPlayer player = this.refObject as KPlayer;

                List<BufferData> buffs = new List<BufferData>();

                /// Duyệt toàn bộ danh sách Buff hiện tại trên cây
                foreach (BuffDataEx buff in this.listBuffs.Values)
                {
                    /// Nếu Buff cần lưu vào DB thì mới thêm
                    if (buff.SaveToDB)
                    {
                        buffs.Add(new BufferData()
                        {
                            BufferID = buff.Skill.SkillID,
                            StartTime = buff.StartTick,
                            BufferSecs = buff.Duration,
                            BufferVal = buff.Level,
                            CustomProperty = buff.CustomProperties == null ? "" : buff.CustomProperties.ToPortableDBString(),
                        });
                    }
                }

                player.BufferDataList = buffs;
            }
        }

        /// <summary>
        /// Chuyển danh sách Buff sang dạng BufferDataMini
        /// </summary>
        /// <returns></returns>
        public List<BufferData> ToBufferData()
        {
            List<BufferData> buffs = new List<BufferData>();
            /// Duyệt toàn bộ danh sách Buff hiện tại trên cây
            foreach (BuffDataEx buff in this.listBuffs.Values)
            {
                buffs.Add(new BufferData()
                {
                    BufferID = buff.Skill.SkillID,
                    StartTime = buff.StartTick,
                    BufferSecs = buff.Duration,
                    BufferVal = buff.Level,
                    CustomProperty = buff.CustomProperties?.ToPortableDBString(),
                });
            }
            /// Duyệt toàn bộ danh sách vòng sáng hiện tại trên cây
            foreach (BuffDataEx buff in this.listAruas.Values)
            {
                buffs.Add(new BufferData()
                {
                    BufferID = buff.Skill.SkillID,
                    StartTime = buff.StartTick,
                    BufferSecs = buff.Duration,
                    BufferVal = buff.Level,
                    CustomProperty = buff.CustomProperties?.ToPortableDBString(),
                });
            }
            return buffs;
        }

        /// <summary>
        /// Kiểm tra có phải loại Buff có chỉ số Attach vào người không
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        private bool IsAttachableBuff(BuffDataEx buff)
        {
            /// Nếu là bùa chú
            if (buff.CurseOwner != null)
            {
                /// Nếu bản thân không phải kẻ ra bùa chú
                if (buff.CurseOwner != this.refObject)
                {
                    /// Nếu mục tiêu là kẻ địch
                    if (buff.Skill.Data.TargetType == "enemy" || buff.Skill.Data.TargetType == "enemies")
                    {
                        return true;
                    }
                }
            }
            /// Nếu là Buff hỗ trợ
            else
            {
                /// Nếu là Buff lên bản thân
                if (buff.Skill.Data.TargetType == "self" || buff.Skill.Data.TargetType == "selfnothide")
                {
                    return true;
                }
                /// Nếu là đồng đội
                else if (buff.Skill.Data.TargetType == "ally" || buff.Skill.Data.TargetType == "team" || buff.Skill.Data.TargetType == "teamnoself" || buff.Skill.Data.TargetType == "allyandnpc" || buff.Skill.Data.TargetType == "allynoself" || buff.Skill.Data.TargetType == "npcteamnoself")
                {
                    return true;
                }
                /// Nếu là đối tượng chung CampID
                else if (buff.Skill.Data.TargetType == "camp")
                {
                    return true;
                }
                /// Nếu là đồng đội đã tử vong
                else if (buff.Skill.Data.TargetType == "revivable")
                {
                    return true;
                }
                /// Nếu là chủ nhân của pet
                else if (buff.Skill.Data.TargetType == "owner")
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kích hoạt toàn bộ Buff
        /// </summary>
        private void AttachAllBuffs()
        {
            /// Duyệt toàn bộ danh sách Buff
            foreach (BuffDataEx buff in this.listBuffs.Values)
            {
                /// Kích hoạt Buff
                KTAttributesModifier.AttachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);
            }
        }
        #endregion
    }
}
