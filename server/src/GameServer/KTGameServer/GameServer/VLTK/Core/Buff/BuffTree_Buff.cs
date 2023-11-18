using GameServer.KiemThe.GameDbController;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý Logic Buff
    /// </summary>
    public partial class BuffTree
    {
        /// <summary>
        /// Danh sách Buff
        /// </summary>
        private readonly ConcurrentDictionary<int, BuffDataEx> listBuffs = new ConcurrentDictionary<int, BuffDataEx>();

        /// <summary>
        /// Danh sách Buff hiện có
        /// </summary>
        public List<BuffDataEx> ListBuffs
        {
            get
            {
                return this.listBuffs.Values.ToList();
            }
        }

        /// <summary>
        /// Thêm Buff cho đối tượng
        /// </summary>
        /// <param name="buffID"></param>
        /// <param name="level"></param>
        /// <param name="durationTicks"></param>
        /// <param name="stack"></param>
        /// <param name="saveToDB"></param>
        public void AddBuff(int buffID, int level, long durationTicks, int stack = 1, bool saveToDB = false)
		{
            /// Kỹ năng tương ứng
            SkillDataEx skillData = KSkill.GetSkillData(buffID);
            /// Nếu tồn tại kỹ năng
            if (skillData != null)
            {
                /// Tạo mới SkillLevelRef
                SkillLevelRef skill = new SkillLevelRef()
                {
                    Data = skillData,
                    AddedLevel = level,
                };
                /// Tạo Buff
                BuffDataEx buff = new BuffDataEx()
                {
                    Duration = durationTicks,
                    StartTick = KTGlobal.GetCurrentTimeMilis(),
                    Skill = skill,
                    StackCount = stack,
                    SaveToDB = saveToDB,
                    CustomProperties = saveToDB ? skill.Properties.Clone() : null,
                };
                this.AddBuff(buff);
            }
        }

        /// <summary>
        /// Thêm Buff cho đối tượng
        /// </summary>
        /// <param name="buffID"></param>
        /// <param name="level"></param>
        /// <param name="stack"></param>
        /// <param name="isFromItem"></param>
        public void AddBuff(int buffID, int level, int stack = 1, bool isFromItem = false)
        {
            /// Kỹ năng tương ứng
            SkillDataEx skillData = KSkill.GetSkillData(buffID);
            if (skillData == null)
            {
                return;
            }
            SkillLevelRef skill = new SkillLevelRef()
            {
                Data = skillData,
                AddedLevel = level,
                BonusLevel = 0,
                CanStudy = false,
            };

            PropertyDictionary skillPd = skill.Properties;
            int duration = -1;
            if (skillPd != null && skillPd.ContainsKey((int) MAGIC_ATTRIB.magic_skill_statetime))
            {
                if (skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_statetime).nValue[0] == -1)
                {
                    duration = -1;
                }
                else
                {
                    duration = skillPd.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skill_statetime).nValue[0] * 1000 / 18;
                }
            }

            this.AddBuff(new BuffDataEx()
            {
                Skill = skill,
                Duration = duration,
                LoseWhenUsingSkill = false,
                SaveToDB = skillData.IsStateNoClearOnDead,
                StartTick = KTGlobal.GetCurrentTimeMilis(),
                StackCount = stack,
                CustomProperties = isFromItem ? skill.Properties : null,
            });
        }

        /// <summary>
        /// Thêm Buff vào danh sách
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="saveToDB"></param>
        public void AddBuff(BuffDataEx buff)
        {
            try
			{
                /// Toác
                if (buff == null || buff.Skill == null || buff.Skill.Data == null)
				{
                    return;
				}

                /// Nếu Buff này bị loại bỏ
                if (this.IsBuffAvoided(buff.Skill.SkillID))
                {
                    return;
                }

                /// Nếu Buff này là vòng sáng con, và trên người đã có vòng sáng cha thì thôi
                if (buff.Skill.Data.IsArua && buff.Skill.Data.ParentAura != -1 && this.HasArua(buff.Skill.Data.ParentAura))
				{
                    return;
				}

                /// Nếu đã tồn tại Buff
                if (this.listBuffs.TryGetValue(buff.Skill.SkillID, out BuffDataEx oldBuff))
                {
                    /// Nếu Buff cũ có cấp độ cao hơn thì không đè vào được
                    if (oldBuff.Skill.Level > buff.Skill.Level)
                    {
                        return;
                    }

                    /// Nếu Buff cũ có cấp độ ngang hàng nhưng thời gian duy trì lâu hơn thì không đè vào
                    if (oldBuff.Skill.Level == buff.Skill.Level)
                    {
                        /// Nếu không có Symbol cộng dồn Buff, thời gian duy trì nhỏ hơn thời gian duy trì Buff cũ
                        if (oldBuff.Properties != null && !oldBuff.Properties.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_superpose_magic) && (oldBuff.TimeLeft == -1 || oldBuff.TimeLeft > buff.TimeLeft))
                        {
                            return;
                        }

                        /// Nếu là Buff có Tag 'Aura' thì không cho đè
                        if (oldBuff.Tag != null && (oldBuff.Tag as string) == "Aura")
                        {
                            //Console.WriteLine("[" + this.refObject.RoleName + "][" + buff.Skill.Data.Name + "] => Same kind of child aura can not be added.");

                            /// Xóa chỉ số Buff cũ
                            KTAttributesModifier.DetachProperties(oldBuff.CustomProperties ?? oldBuff.Properties, this.refObject, oldBuff.StackCount, oldBuff.Skill.SkillID, oldBuff.Level);
                            /// Cập nhật lại chỉ số Buff mới
                            KTAttributesModifier.AttachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);

                            /// Thêm Buff vào danh sách
                            this.listBuffs[buff.Skill.SkillID] = buff;

                            /// Gửi gói tin về Client
                            if (buff.Skill.Data.StateEffectID != 0 && buff.Skill.Data.Type != 3 && (!buff.Skill.Data.IsArua || buff.Skill.Data.ParentAura != -1))
                            {
                                KT_TCPHandler.SendSpriteAddBuff(this.refObject, buff);
                            }

                            /// Bỏ qua
                            return;
                        }
                    }

                    /// Nếu là vòng sáng con
                    if (oldBuff.Skill.Data.IsArua)
                    {
                        /// Nếu có vòng sáng cha đang kích hoạt tương ứng
                        if (this.listAruas.TryGetValue(oldBuff.Skill.Data.ParentAura, out _))
                        {
                            return;
                        }
                    }

                    /// Nếu có Symbol tàng hình và cấp độ trùng cấp độ Buff thì thiết lập lại thời gian
                    if (oldBuff.Properties != null && oldBuff.Properties.ContainsKey((int) MAGIC_ATTRIB.magic_hide) && oldBuff.Level == buff.Level)
                    {
                        /// Cập nhật thời gian
                        oldBuff.StartTick = KTGlobal.GetCurrentTimeMilis();
                    }
                    else
                    {
                        /// Xóa Buff cũ
                        this.RemoveBuff(oldBuff, true, false);
                    }
                }

                /// Thêm Buff vào danh sách
                this.listBuffs[buff.Skill.SkillID] = buff;

                /// Gọi hàm thực hiện nhận Buff mới
                this.refObject.OnReceiveBuff(buff);

                //Console.WriteLine("[" + this.refObject.RoleName + "][" + buff.Skill.Data.Name + "] => IsAttachable = " + this.IsAttachableBuff(buff) + " - TargetType = " + buff.Skill.Data.TargetType + " - CurseOwner = NULL: " + ((buff.CurseOwner == null)));
                /// Nếu là loại Buff Attach vào bản thân hoặc kỹ năng bị động thì mới Attach Property
                if (this.IsAttachableBuff(buff) || buff.Skill.Data.Type == 3)
                {
                    /// Nếu là Buff có Tag 'Aura' thì kiểm tra cấp độ nếu khác cấp độ hiện tại của Buff thì mới Attach chỉ số
                    if (oldBuff == null || oldBuff.Tag == null || ((oldBuff.Tag as string) != null && (oldBuff.Tag as string) == "Aura" && oldBuff.Level != buff.Level))
                    {
                        /// Nếu có Symbol cộng dồn
                        if (oldBuff != null && buff.Properties != null && buff.Properties.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_superpose_magic))
                        {
                            KMagicAttrib magicAttrib = buff.Properties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_superpose_magic);
                            buff.StackCount = Math.Min(oldBuff.StackCount + 1, magicAttrib.nValue[0]);
                        }

                        /// Kích hoạt Buff
                        KTAttributesModifier.AttachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);
                    }
                }

                /// Nếu phải lưu vào DB thì Export nguyên dãy Buff ra lưu
                if (buff.SaveToDB)
                {
                    this.ExportBuffTree();
                }

                /// Gửi gói tin về Client
                if (buff.Skill.Data.StateEffectID != 0 && buff.Skill.Data.Type != 3 && (!buff.Skill.Data.IsArua || buff.Skill.Data.ParentAura != -1))
                {
                    KT_TCPHandler.SendSpriteAddBuff(this.refObject, buff);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Thêm Buff vào danh sách
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="tickTimeSec"></param>
        /// <param name="doTick"></param>
        public void AddBuff(BuffDataEx buff, float tickTimeSec, Action doTick)
        {
            try
			{
                /// Nếu không có Buff
                if (buff == null)
				{
                    return;
				}

                /// Nếu Buff này bị loại bỏ
                if (this.IsBuffAvoided(buff.Skill.SkillID))
                {
                    return;
                }

                /// Nếu đã tồn tại Buff
                if (this.listBuffs.TryGetValue(buff.Skill.SkillID, out BuffDataEx oldBuff))
                {
                    /// Nếu Buff cũ có cấp độ cao hơn thì không đè vào được
                    if (oldBuff.Skill.Level > buff.Skill.Level)
                    {
                        return;
                    }

                    /// Nếu Buff cũ có cấp độ ngang hàng
                    if (oldBuff.Skill.Level == buff.Skill.Level)
                    {
                        /// Nếu không có Symbol cộng dồn Buff, thời gian duy trì nhỏ hơn thời gian duy trì Buff cũ
                        if (!oldBuff.Properties.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_superpose_magic) && (oldBuff.TimeLeft == -1 || oldBuff.TimeLeft > buff.TimeLeft))
                        {
                            return;
                        }
                    }

                    /// Nếu có Symbol tàng hình và cấp độ trùng cấp độ Buff thì thiết lập lại thời gian
                    if (oldBuff.Properties.ContainsKey((int) MAGIC_ATTRIB.magic_hide) && oldBuff.Level == buff.Level)
                    {
                        /// Cập nhật thời gian
                        oldBuff.StartTick = KTGlobal.GetCurrentTimeMilis();
                    }
                    else
                    {
                        /// Xóa Buff cũ
                        this.RemoveBuff(oldBuff, true, false);
                    }
                }

                /// Thêm Buff vào danh sách
                this.listBuffs[buff.Skill.SkillID] = buff;

                /// Đánh dấu loại là Buff liên tục
                buff.Tag = "ContinuouslyBuff";
                /// Thêm Buff vào luồng thực thi đếm
                KTBuffManager.Instance.AddBuff(this.refObject, buff, (int) (tickTimeSec * 1000), doTick);

                /// Gọi hàm thực hiện nhận Buff mới
                this.refObject.OnReceiveBuff(buff);

                /// Nếu là loại Buff Attach vào bản thân hoặc kỹ năng bị động thì mới Attach Property
                if (this.IsAttachableBuff(buff) || buff.Skill.Data.Type == 3)
                {

                    //LogManager.WriteLog(LogTypes.Skill, "[" + this.refObject.RoleName + "][" + buff.Skill.SkillID + "]: " + buff.Properties.ToString());
                    /// Kích hoạt Buff
                    /// Nếu là Buff có Tag 'Aura' thì kiểm tra cấp độ nếu khác cấp độ hiện tại của Buff thì mới Attach chỉ số
                    if (oldBuff == null || oldBuff.Tag == null || ((oldBuff.Tag as string) != null && (oldBuff.Tag as string) == "Aura" && oldBuff.Level != buff.Level))
                    {
                        /// Nếu có Symbol cộng dồn
                        if (oldBuff != null && buff.Properties.ContainsKey((int) MAGIC_ATTRIB.magic_skilladdition_superpose_magic))
                        {
                            KMagicAttrib magicAttrib = buff.Properties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_skilladdition_superpose_magic);
                            buff.StackCount = Math.Min(oldBuff.StackCount + 1, magicAttrib.nValue[0]);
                        }

                        /// Kích hoạt Buff
                        KTAttributesModifier.AttachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);
                    }
                }

                /// Nếu phải lưu vào DB thì Export nguyên dãy Buff ra lưu
                if (buff.SaveToDB)
                {
                    this.ExportBuffTree();
                }

                /// Gửi gói tin về Client
                if (buff.Skill.Data.StateEffectID != 0 && buff.Skill.Data.Type != 3 && (!buff.Skill.Data.IsArua || buff.Skill.Data.ParentAura != -1))
                {
                    KT_TCPHandler.SendSpriteAddBuff(this.refObject, buff);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Xóa Buff khỏi danh sách
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="doLogic">Thông báo cho Client và lưu vào DB</param>
        /// <param name="notify">Thông báo cho Client và lưu vào DB</param>
        public void RemoveBuff(BuffDataEx buff, bool doLogic = true, bool notify = true)
        {
            try
			{
                /// Nếu không có Buff
                if (buff == null)
                {
                    return;
                }

                if (this.listBuffs.TryGetValue(buff.Skill.SkillID, out BuffDataEx oldBuff))
                {
                    /// Xóa Buff khỏi danh sách
                    this.listBuffs.TryRemove(oldBuff.Skill.SkillID, out _);

                    /// Đánh dấu đã chạy qua pha Detach rồi
                    buff.IsDetached = true;

                    /// Nếu là loại Buff liên tục
                    if (buff.Tag != null && (string) buff.Tag == "ContinuouslyBuff")
					{
                        /// Xóa Buff khỏi luồng thực thi đếm
                        KTBuffManager.Instance.RemoveBuff(this.refObject, oldBuff);
                    }

                    //Console.WriteLine(new System.Diagnostics.StackTrace().ToString());
                    //Console.WriteLine("Remove buff => " + this.refObject.RoleName + ", " + buff.Skill.Data.Name);

                    if (doLogic)
                    {
                        /// Nếu là loại Buff Attach vào bản thân, hoặc kỹ năng bị động thì mới Detach Property
                        if (this.IsAttachableBuff(oldBuff) || oldBuff.Skill.Data.Type == 3)
                        {
                            /// Hủy hiệu quả Buff đã kích hoạt
                            KTAttributesModifier.DetachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);
                        }

                        /// Gọi hàm thực thi khi mất Buff
                        this.refObject.OnLostBuff(buff);
                    }

                    if (notify)
                    {
                        /// Nếu phải lưu vào DB
                        if (oldBuff.SaveToDB)
                        {
                            /// Nếu là người chơi
                            if (this.refObject is KPlayer player)
                            {
                                GameDb.DeleteDBBuffer(player, oldBuff.Skill.SkillID);
                            }
                            /// Cập nhật lại
                            this.ExportBuffTree();
                        }

                        /// Gửi gói tin về Client
                        if (oldBuff.Skill.Data.StateEffectID != 0)
                        {
                            KT_TCPHandler.SendSpriteRemoveBuff(this.refObject, oldBuff);
                        }
                    }
                }
            }
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Xóa Buff khỏi danh sách
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="doLogic">Thông báo cho Client và lưu vào DB</param>
        /// <param name="notify">Thông báo cho Client và lưu vào DB</param>
        public void RemoveBuff(int skillID, bool doLogic = true, bool notify = true)
        {
            if (this.listBuffs.TryGetValue(skillID, out BuffDataEx buff))
            {
                this.RemoveBuff(buff, doLogic, notify);
            }
        }

        /// <summary>
        /// Xóa toàn bộ Buff
        /// <para>Trường hợp không cần thực hiện thao tác thông thường như Detach, Notify thì thiết lập doLogic = false</para>
        /// <param name="doLogic">Thực hiện Detach ProDict</param>
        /// <param name="notify">Thông báo cho Client và lưu vào DB</param>
        /// </summary>
        public void RemoveAllBuffs(bool doLogic = true, bool notify = true)
        {
            try
			{
                /// Danh sách khóa trong dãy
                List<int> ids = this.listBuffs.Keys.ToList();

                /// Duyệt toàn bộ các khóa
                foreach (int id in ids)
                {
                    /// Lấy ra giá trị Buff tương ứng
                    if (!this.listBuffs.TryGetValue(id, out BuffDataEx buff))
                    {
                        continue;
                    }

                    /// Xóa Buff khỏi danh sách
                    this.listBuffs.TryRemove(buff.Skill.SkillID, out _);

                    /// Đánh dấu đã chạy qua pha Detach rồi
                    buff.IsDetached = true;

                    /// Nếu là loại Buff liên tục
                    if (buff.Tag != null && (string) buff.Tag == "ContinuouslyBuff")
                    {
                        /// Xóa Buff khỏi luồng thực thi đếm
                        KTBuffManager.Instance.RemoveBuff(this.refObject, buff);
                    }

                    //Console.WriteLine("Remove buff => " + buff.Skill.Data.Name);

                    if (doLogic)
                    {
                        /// Hủy hiệu quả Buff đã kích hoạt
                        KTAttributesModifier.DetachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);

                        /// Gọi hàm thực thi khi mất Buff
                        this.refObject.OnLostBuff(buff);
                    }

                    if (notify)
                    {
                        /// Nếu phải lưu vào DB
                        if (buff.SaveToDB)
                        {
                            /// Nếu là người chơi
                            if (this.refObject is KPlayer player)
                            {
                                GameDb.DeleteDBBuffer(player, buff.Skill.SkillID);
                            }
                            /// Cập nhật lại
                            this.ExportBuffTree();
                        }

                        /// Gửi gói tin về Client
                        if (buff.Skill.Data.StateEffectID != 0)
                        {
                            KT_TCPHandler.SendSpriteRemoveBuff(this.refObject, buff);
                        }
                    }
                }
            }
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Xóa toàn bộ Buff
        /// <para>Trường hợp không cần thực hiện thao tác thông thường như Detach, Notify thì thiết lập doLogic = false</para>
        /// <param name="doLogic">Thực hiện Detach ProDict</param>
        /// <param name="notify">Thông báo cho Client và lưu vào DB</param>
        /// </summary>
        public void ClearBuffsOnDead()
        {
            try
			{
                /// Danh sách khóa trong dãy
                List<int> ids = this.listBuffs.Keys.ToList();

                /// Duyệt toàn bộ các khóa
                foreach (int id in ids)
                {
                    /// Lấy ra giá trị Buff tương ứng
                    if (!this.listBuffs.TryGetValue(id, out BuffDataEx buff))
                    {
                        continue;
                    }

                    /// Nếu không phải Buff tự xóa khi đối tượng chết
                    if (buff.KeepOnDeath)
                    {
                        continue;
                    }

                    /// Xóa Buff khỏi danh sách
                    this.listBuffs.TryRemove(id, out _);

                    /// Đánh dấu đã chạy qua pha Detach rồi
                    buff.IsDetached = true;

                    /// Nếu là loại Buff liên tục
                    if (buff.Tag != null && (string) buff.Tag == "ContinuouslyBuff")
                    {
                        /// Xóa Buff khỏi luồng thực thi đếm
                        KTBuffManager.Instance.RemoveBuff(this.refObject, buff);
                    }

                    //Console.WriteLine("Remove buff => " + this.refObject.RoleName + ", " + buff.Skill.Data.Name);

                    /// Hủy hiệu quả Buff đã kích hoạt
                    KTAttributesModifier.DetachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);

                    /// Gửi gói tin về Client
                    if (buff.Skill.Data.StateEffectID != 0)
                    {
                        KT_TCPHandler.SendSpriteRemoveBuff(this.refObject, buff);
                    }
                }

                this.ExportBuffTree();
            }
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
			}
        }

        /// <summary>
        /// Kiểm tra có Buff của kỹ năng tương ứng không
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public bool HasBuff(int skillID)
        {
            if (this.listBuffs.TryGetValue(skillID, out BuffDataEx buff))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Trả về Buff tương ứng trong danh sách
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public BuffDataEx GetBuff(int skillID)
        {
            if (this.listBuffs.TryGetValue(skillID, out BuffDataEx buff))
            {
                return buff;
            }
            return null;
        }

        /// <summary>
        /// Trả về một Buff ngẫu nhiên hỗ trợ chủ động của bản thân
        /// </summary>
        /// <returns></returns>
        public BuffDataEx GetRandomPositiveBuff()
        {
            try
			{
                List<BuffDataEx> positiveBuffs = new List<BuffDataEx>();

                /// Danh sách khóa trong dãy
                List<int> ids = this.listBuffs.Keys.ToList();
                foreach (int id in ids)
                {
                    if (!this.listBuffs.TryGetValue(id, out BuffDataEx buff))
                    {
                        continue;
                    }

					 if (id == 1997 || id == 1998)
                    {
                       // Console.WriteLine("SKIPPP TIEN THAO LO! :" + id);
                        continue;
                    }														
                    /// Nếu là kỹ năng hỗ trợ chủ động
                    if (buff.Skill.Data.Type == 2 && buff.Skill.Data.TargetType == "self" && buff.Skill.Data.SkillStyle != "supportbuff")
                    {
                        positiveBuffs.Add(buff);
                    }
                }

                /// Nếu không tìm thấy trạng thái nào
                if (positiveBuffs.Count <= 0)
                {
                    return null;
                }
                /// Lấy ngẫu nhiên trạng thái trả ra kết quả
                else
                {
                    return positiveBuffs[KTGlobal.GetRandomNumber(0, positiveBuffs.Count - 1)];
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Xóa các Buff quá thời hạn
        /// </summary>
        public void ClearTimeoutBuffs()
		{
			try
			{
                /// Danh sách khóa trong dãy
                List<int> ids = this.listBuffs.Keys.ToList();
                foreach (int id in ids)
                {
                    /// Nếu Buff không tồn tại
                    if (!this.listBuffs.TryGetValue(id, out BuffDataEx buff))
                    {
                        continue;
                    }

                    /// Thời gian còn lại
                    long timeLeft = buff.TimeLeft;
                    /// Nếu Buff đã quá hạn
                    if (timeLeft != -1 && timeLeft <= 0)
					{
                        /// Xóa khỏi danh sách
                        this.RemoveBuff(id);
					}
                }
            }
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }
    }
}
