using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý vòng sáng
    /// </summary>
    public partial class BuffTree
    {
        /// <summary>
        /// Danh sách vòng sáng
        /// </summary>
        private readonly ConcurrentDictionary<int, BuffDataEx> listAruas = new ConcurrentDictionary<int, BuffDataEx>();

        /// <summary>
        /// Vòng sáng đang kích hoạt hiện tại
        /// </summary>
        public BuffDataEx CurrentArua { get; private set; }

        /// <summary>
        /// Danh sách vòng sáng hiện có
        /// </summary>
        public List<BuffDataEx> ListAruas
        {
            get
            {
                return this.listAruas.Values.ToList();
            }
        }


        /// <summary>
        /// Thiết lập vòng sáng cho đối tượng
        /// </summary>
        /// <param name="skill">Kỹ năng</param>
        /// <param name="tickTimeSec">Tối thiểu 0.5s</param>
        /// <param name="doTick">Hàm thực thi mỗi lần</param>
        public void SetArua(SkillLevelRef skill, float tickTimeSec, Action doTick)
        {
            try
			{
                /// Nếu kỹ năng không tồn tại
                if (skill == null)
                {
                    return;
                }

                /// Xóa toàn bộ vòng sáng cũ hiện có
                this.RemoveAllAruas();

                /// Tạo mới vòng sáng tương ứng
                BuffDataEx arua = new BuffDataEx()
                {
                    Duration = -1,
                    LoseWhenUsingSkill = false,
                    Skill = skill,
                    SaveToDB = false,
                    StartTick = KTGlobal.GetCurrentTimeMilis(),
                    CustomProperties = skill.Properties.Clone(),
                };

                /// Cộng toàn bộ thuộc tính hỗ trợ
                if (this.refObject is KPlayer)
                {
                    KPlayer player = this.refObject as KPlayer;
                    PropertyDictionary enchantPd = player.Skills.GetEnchantProperties(skill.SkillID);
                    if (enchantPd != null)
                    {
                        arua.CustomProperties.AddProperties(enchantPd);
                    }
                }

                /// Nếu có Symbol hồi máu mỗi nửa giây, và có Symbol hiệu suất phục hồi sinh lực thì nhân vào
                if (arua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_fastlifereplenish_v) && arua.CustomProperties.ContainsKey((int) MAGIC_ATTRIB.magic_lifereplenish_p))
                {
                    KMagicAttrib pMagicAttrib = arua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                    KMagicAttrib magicAttrib = arua.CustomProperties.Get<KMagicAttrib>((int) MAGIC_ATTRIB.magic_fastlifereplenish_v);
                    magicAttrib.nValue[0] += magicAttrib.nValue[0] * pMagicAttrib.nValue[0] / 100;
                    /// Xóa Symbol hiệu suất phục hồi sinh lực khỏi ProDict
                    arua.CustomProperties.Remove((int) MAGIC_ATTRIB.magic_lifereplenish_p);
                }

                /// Thiết lập vòng sáng hiện tại
                this.CurrentArua = arua;

                /// Thêm vòng sáng vào danh sách
                this.AddArua(arua, tickTimeSec, doTick);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Thêm Buff loại vòng sáng hỗ trợ vào danh sách
        /// </summary>
        /// <param name="buff">Buff tương ứng vòng sáng</param>
        /// <param name="tickTimeSec">Tối thiểu 0.5</param>
        /// <param name="doTick">Hàm thực thi mỗi lần</param>
        public void AddArua(BuffDataEx buff, float tickTimeSec, Action doTick)
        {
            try
			{
                /// Vòng sáng con
                List<BuffDataEx> childAuras = this.listBuffs.Values.Where(x => x.Skill.Data.IsArua && x.Skill.Data.ParentAura == buff.Skill.SkillID).ToList();
                /// Duyệt danh sách vòng sáng con và xóa
                foreach (BuffDataEx childAura in childAuras)
				{
                    this.RemoveBuff(childAura);
				}

                /// Nếu đã tồn tại vòng sáng
                if (this.listAruas.TryGetValue(buff.Skill.SkillID, out BuffDataEx oldBuff))
                {
                    /// Xóa vòng sáng cũ
                    this.RemoveArua(oldBuff, true, false);
                }

                /// Thêm Buff vào danh sách
                this.listAruas[buff.Skill.SkillID] = buff;

                /// Đánh dấu đây là Buff liên tục
                buff.Tag = "ContinuouslyBuff";
                /// Thêm Buff vào luồng thực thi đếm
                KTBuffManager.Instance.AddBuff(this.refObject, buff, (int) (tickTimeSec * 1000), doTick);

                /// Nếu là loại có thuộc tính Attach vào bản thân
                if (this.IsAttachableBuff(buff))
                {
                    /// Kích hoạt Buff
                    KTAttributesModifier.AttachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);
                }

                /// Gửi gói tin về Client
                if (buff.Skill.Data.StateEffectID != 0)
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
        /// Xóa vòng sáng khỏi danh sách
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="doLogic">Thông báo cho Client và lưu vào DB</param>
        /// <param name="notify">Thông báo cho Client và lưu vào DB</param>
        public void RemoveArua(BuffDataEx buff, bool doLogic = true, bool notify = true)
        {
            try
			{
                if (this.listAruas.TryGetValue(buff.Skill.SkillID, out BuffDataEx oldBuff))
                {
                    /// Xóa Buff khỏi danh sách
                    this.listAruas.TryRemove(oldBuff.Skill.SkillID, out _);

                    /// Nếu là loại Buff liên tục
                    if (buff.Tag != null && (string) buff.Tag == "ContinuouslyBuff")
                    {
                        /// Xóa Buff khỏi luồng thực thi đếm
                        KTBuffManager.Instance.RemoveBuff(this.refObject, oldBuff);
                    }

                    if (doLogic)
                    {
                        /// Nếu là loại vòng sáng hỗ trợ
                        if (buff.Skill.Data.SkillStyle != "aurarangemagicattack")
                        {
                            /// Hủy hiệu quả Buff đã kích hoạt
                            KTAttributesModifier.DetachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);
                        }
                    }

                    if (notify)
                    {
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
        /// Xóa vòng sáng khỏi danh sách
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="doLogic">Thông báo cho Client và lưu vào DB</param>
        /// <param name="notify">Thông báo cho Client và lưu vào DB</param>
        public void RemoveArua(int skillID, bool doLogic = true, bool notify = true)
        {
            if (this.listAruas.TryGetValue(skillID, out BuffDataEx buff))
            {
                this.RemoveArua(buff, doLogic, notify);
            }
        }

        /// <summary>
        /// Xóa toàn bộ vòng sáng
        /// <para>Trường hợp không cần thực hiện thao tác thông thường như Detach, Notify thì thiết lập doLogic = false</para>
        /// <param name="doLogic">Thực hiện Detach ProDict</param>
        /// <param name="notify">Thông báo cho Client và lưu vào DB</param>
        /// </summary>
        public void RemoveAllAruas(bool doLogic = true, bool notify = true)
        {
            try
			{
                this.CurrentArua = null;

                /// Danh sách khóa trong dãy
                List<int> ids = this.listAruas.Keys.ToList();

                /// Duyệt toàn bộ các khóa
                foreach (int id in ids)
                {
                    /// Lấy ra giá trị Buff tương ứng
                    if (!this.listAruas.TryGetValue(id, out BuffDataEx buff))
                    {
                        continue;
                    }

                    /// Xóa Buff khỏi danh sách
                    this.listAruas.TryRemove(buff.Skill.SkillID, out _);

                    /// Nếu là loại Buff liên tục
                    if (buff.Tag != null && (string) buff.Tag == "ContinuouslyBuff")
                    {
                        /// Xóa Buff khỏi luồng thực thi đếm
                        KTBuffManager.Instance.RemoveBuff(this.refObject, buff);
                    }

                    if (doLogic)
                    {
                        /// Nếu là loại vòng sáng hỗ trợ
                        if (buff.Skill.Data.SkillStyle != "aurarangemagicattack")
                        {
                            /// Hủy hiệu quả Buff đã kích hoạt
                            KTAttributesModifier.DetachProperties(buff.CustomProperties ?? buff.Properties, this.refObject, buff.StackCount, buff.Skill.SkillID, buff.Level);
                        }
                    }

                    if (notify)
                    {
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
        /// Kiểm tra có vòng sáng của kỹ năng tương ứng không
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public bool HasArua(int skillID)
        {
            if (this.listAruas.TryGetValue(skillID, out BuffDataEx buff))
            {
                return true;
            }
            return false;
        }
    }
}
