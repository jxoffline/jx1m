using FS.VLTK;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.GameEngine.Sprite
{
    /// <summary>
    /// Quản lý Buff
    /// </summary>
    public partial class GSprite
    {
        /// <summary>
        /// Thực thể Buff
        /// </summary>
        private class SpriteBuff
        {
            /// <summary>
            /// ID Buff
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Thời gian thêm vào
            /// </summary>
            public long AddTick { get; set; }

            /// <summary>
            /// Thời gian tồn tại Buff (Milis)
            /// </summary>
            public long Duration { get; set; }

            /// <summary>
            /// Đã hết thời gian chưa
            /// </summary>
            public bool IsOver
            {
                get
                {
                    /// Nếu tồn tại vĩnh viễn
                    if (this.Duration == -1)
                    {
                        return false;
                    }
                    return KTGlobal.GetCurrentTimeMilis() - this.AddTick >= this.Duration;
                }
            }
        }

        /// <summary>
        /// Danh sách Buff của đối tượng
        /// </summary>
        private readonly Dictionary<int, SpriteBuff> Buffs = new Dictionary<int, SpriteBuff>();

        /// <summary>
        /// Thực thi Tick Buff
        /// </summary>
        private void TickBuffs()
        {
            /// Danh sách Buff cần xóa
            List<SpriteBuff> toDeleteBuffs = null;
            /// Duyệt danh sách Buff
            foreach (SpriteBuff buff in this.Buffs.Values)
            {
                /// Nếu đã hết thời gian
                if (buff.IsOver)
                {
                    /// Nếu chưa tồn tại danh sách cần xóa thì tạo mới
                    if (toDeleteBuffs == null)
                    {
                        toDeleteBuffs = new List<SpriteBuff>();
                    }
                    /// Thêm vào danh sách cần xóa
                    toDeleteBuffs.Add(buff);
                }
            }
            /// Nếu có danh sách Buff cần xóa
            if (toDeleteBuffs != null)
            {
                /// Duyệt danh sách cần xóa và xóa Buff tương ứng
                foreach (SpriteBuff buff in toDeleteBuffs)
                {
                    /// Xóa Buff tương ứng
                    this.RemoveBuff(buff.ID);
                }
            }
        }

        /// <summary>
        /// Thêm Buff vào danh sách
        /// </summary>
        /// <param name="buffID"></param>
        /// <param name="durationTicks"></param>
        public void AddBuff(int buffID, long durationTicks)
        {
            /// Nếu đã tồn tại Buff cũ
            if (this.Buffs.TryGetValue(buffID, out SpriteBuff buff))
            {
                /// Xóa Buff cũ
                this.RemoveBuff(buffID);
            }
            /// Thêm Buff vào danh sách
            this.Buffs[buffID] = new SpriteBuff()
            {
                ID = buffID,
                AddTick = KTGlobal.GetCurrentTimeMilis(),
                Duration = durationTicks,
            };
            /// Thêm hiệu ứng tương ứng
            if (this.ComponentCharacter != null)
            {
                this.ComponentCharacter.AddEffect(buffID, VLTK.Entities.Enum.EffectType.Buff);
            }
            else if (this.ComponentMonster != null)
            {
                this.ComponentMonster.AddEffect(buffID, VLTK.Entities.Enum.EffectType.Buff);
            }
        }

        /// <summary>
        /// Xóa Buff tương ứng
        /// </summary>
        /// <param name="buffID"></param>
        public void RemoveBuff(int buffID)
        {
            /// Nếu Buff tồn tại
            if (this.Buffs.TryGetValue(buffID, out SpriteBuff buff))
            {
                /// Xóa Buff khỏi danh sách
                this.Buffs.Remove(buffID);
                /// Xóa hiệu ứng tương ứng khỏi đối tượng
                if (this.ComponentCharacter != null)
                {
                    this.ComponentCharacter.RemoveEffect(buffID);
                }
                else if (this.ComponentMonster != null)
                {
                    this.ComponentMonster.RemoveEffect(buffID);
                }
            }
        }
    }
}
