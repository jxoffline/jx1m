using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Dữ liệu Buff
    /// </summary>
    public class BuffDataEx
    {
        /// <summary>
        /// Kỹ năng chứa
        /// </summary>
        public SkillLevelRef Skill { get; set; }

        /// <summary>
        /// Cấp độ của Buff
        /// <para>Nếu buff khác đè vào thì sẽ căn cứ cấp độ xem có cho phép đè không</para>
        /// </summary>
        public int Level
        {
            get
            {
                return this.Skill.Level;
            }
        }

        /// <summary>
        /// Thời gian bắt đầu
        /// </summary>
        public long StartTick { get; set; }

        /// <summary>
        /// Thời gian duy trì (mili giây)
        /// <para>-1 nghĩa là mãi mãi</para>
        /// </summary>
        public long Duration { get; set; }

        /// <summary>
        /// Thời gian còn lại
        /// <para>Nếu Buff vĩnh viễn thì sẽ luôn trả ra -1</para>
        /// </summary>
        public long TimeLeft
        {
            get
            {
                if (this.Duration == -1)
                {
                    return -1;
                }
                else
                {
                    return (int)(this.Duration - KTGlobal.GetCurrentTimeMilis() + this.StartTick);
                }
            }
        }

        /// <summary>
        /// Thuộc tính của Buff, mặc định sẽ là thuộc tính của kỹ năng.
        /// <para>Nếu có thiết lập CustomProperties thì sẽ lấy giá trị này</para>
        /// </summary>
        public PropertyDictionary Properties
        {
            get
            {
                if (this.CustomProperties != null)
                {
                    return this.CustomProperties;
                }
                else
                {
                    return this.Skill.Properties;
                }
            }
        }

        /// <summary>
        /// Thuộc tính tùy chọn của Buff.
        /// <para>Khi giá trị này NULL thì sẽ lấy thuộc tính của kỹ năng</para>
        /// </summary>
        public PropertyDictionary CustomProperties { get; set; }

        /// <summary>
        /// Buff có được giữ lại khi đối tượng tử vong
        /// </summary>
        public bool KeepOnDeath
        {
            get
            {
                return this.Skill.Data.IsStateNoClearOnDead;
            }
        }

        /// <summary>
        /// Buff có biến mất khi đối tượng dùng kỹ năng không
        /// </summary>
        public bool LoseWhenUsingSkill { get; set; }

        /// <summary>
        /// Buff có được lưu vào DB không
        /// </summary>
        public bool SaveToDB { get; set; } = false;

        /// <summary>
        /// Số cộng dồn hiện tại
        /// <para>Mặc định nếu không thiết lập thì sẽ mang giá trị 1</para>
        /// </summary>
        public int StackCount { get; set; } = 1;

        /// <summary>
        /// Dữ liệu đi kèm
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Chủ nhân nếu là bùa chú
        /// </summary>
        public GameObject CurseOwner { get; set; }

        /// <summary>
        /// Đã Detach chưa
        /// </summary>
        public bool IsDetached { get; set; } = false;

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("ID: {0}, Name: {1}, Level: {2}", this.Skill.SkillID, this.Skill.Data.Name, this.Level);
        }

        /// <summary>
        /// Tạo bản sao của đối tượng
        /// </summary>
        /// <returns></returns>
        public BuffDataEx Clone()
		{
            BuffDataEx buffData = new BuffDataEx()
            {
                Skill = this.Skill.Clone(),
                StartTick = this.StartTick,
                Duration = this.Duration,
                CustomProperties = this.CustomProperties,
                LoseWhenUsingSkill = this.LoseWhenUsingSkill,
                SaveToDB = this.SaveToDB,
                StackCount = this.StackCount,
                Tag = this.Tag,
                CurseOwner = this.CurseOwner,
                IsDetached = this.IsDetached,
            };
            return buffData;
		}
    }
}
