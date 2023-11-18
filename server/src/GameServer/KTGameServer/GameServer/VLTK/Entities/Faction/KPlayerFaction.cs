using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Môn phái của người chơi
    /// </summary>
    public class KPlayerFaction
    {
        /// <summary>
        /// Đối tượng người chơi
        /// </summary>
        private readonly KPlayer m_rPlayer;

        /// <summary>
        /// ID môn phái
        /// </summary>
        private int m_byFactionId;

        /// <summary>
        /// Quản lý môn phái của người chơi
        /// </summary>
        /// <param name="rPlayer"></param>
        public KPlayerFaction(KPlayer rPlayer, int factionID, int routeID)
        {
            this.m_rPlayer = rPlayer;
            this.m_byFactionId = factionID;
        }

        /// <summary>
        /// Trả về ID môn phái của người chơi
        /// </summary>
        /// <returns></returns>
        public int GetFactionId()
        {
            return this.m_byFactionId;
        }

        /// <summary>
        /// Trả về tên môn phái của người chơi
        /// </summary>
        /// <returns></returns>
        public string GetFactionName()
        {
            return KFaction.GetName(this.m_byFactionId);
        }

        /// <summary>
        /// Xóa kỹ năng môn phái cũ
        /// </summary>
        private void RemoveFactionSkills()
        {
            List<int> skills = KFaction.GetFactionSkills(this.m_byFactionId);
            this.m_rPlayer.Skills.RemoveSkills(skills);
        }

        /// <summary>
        /// Tẩy lại cấp độ kỹ năng môn phái
        /// </summary>
        public void ResetFactionSkillsLevel()
        {
            List<int> skills = KFaction.GetFactionSkills(this.m_byFactionId);
            this.m_rPlayer.Skills.ResetSkillsLevel(skills);
        }

        /// <summary>
        /// Thêm kỹ năng môn phái mới
        /// </summary>
        private void AddFactionSkills()
        {
            List<int> skills = KFaction.GetFactionSkills(this.m_byFactionId);
            this.m_rPlayer.Skills.AddSkills(skills);
        }

        /// <summary>
        /// Đổi môn phái
        /// </summary>
        /// <param name="byFactionId"></param>
        /// <returns></returns>
        public bool ChangeFaction(int byFactionId)
        {
            /// Nếu phái không tồn tại
            if (!KFaction.IsFactionExist(byFactionId))
            {
                return false;
            }

            /// Thực hiện chuyển phái
            this.ProcessFactionChange(this.m_byFactionId, byFactionId);

            /// Trả về kết quả
            return true;
        }

        /// <summary>
        /// Tẩy toàn bộ điểm kỹ năng
        /// </summary>
        /// <returns></returns>
        public bool ResetAllSkillsPoint()
        {
            /// Thực hiện tự chuyển phái
            this.ProcessFactionChange(this.m_byFactionId, this.m_byFactionId);

            /// Trả về kết quả
            return true;
        }

        /// <summary>
        /// Xóa toàn bộ chỉ số cộng của môn phái và nhánh cũ
        /// </summary>
        /// <param name="oldFactionID"></param>
        /// <param name="nDamagePhysicsChange"></param>
        /// <param name="nDamageMagicChange"></param>
        /// <param name="nAttackRateChange"></param>
        /// <param name="nDefenceChange"></param>
        /// <param name="nLifeChange"></param>
        /// <param name="nManaChange"></param>
        private void RemoveFactionProperties(int oldFactionID, out int nDamagePhysicsChange, out int nDamageMagicChange, out int nAttackRateChange, out int nDefenceChange, out int nLifeChange, out int nManaChange)
        {
            /// Tẩy toàn bộ điểm kỹ năng nhánh tương ứng
            this.ResetFactionSkillsLevel();
            /// Gọi đến sự kiện trước khi thay đổi nhánh môn phái
            this.m_rPlayer.OnPreFactionChanged();
            /// Xóa toàn bộ hiệu ứng trang bị
            this.m_rPlayer.GetPlayEquipBody().ClearAllEffecEquipBody();

            /// Vật công ngoại
            nDamagePhysicsChange = KPlayerSetting.GetStrength2DamagePhysics(oldFactionID, this.m_rPlayer.GetCurStrength()) + KPlayerSetting.GetDexterity2DamagePhysics(oldFactionID, this.m_rPlayer.GetCurDexterity());
            /// Vật công nội
            nDamageMagicChange = KPlayerSetting.GetEnergy2DamageMagic(oldFactionID, this.m_rPlayer.GetCurEnergy());
            /// Chính xác
            nAttackRateChange = KPlayerSetting.GetDexterity2AttackRate(oldFactionID, this.m_rPlayer.GetCurDexterity());
            /// Né tránh
            nDefenceChange = KPlayerSetting.GetDexterity2Defence(oldFactionID, this.m_rPlayer.GetCurDexterity());
            /// Sinh lực
            nLifeChange = KPlayerSetting.GetVitality2Life(oldFactionID, this.m_rPlayer.GetCurVitality());
            /// Nội lực
            nManaChange = KPlayerSetting.GetEnergy2Mana(oldFactionID, this.m_rPlayer.GetCurEnergy());
        }

        /// <summary>
        /// Tính toán lại chỉ số môn phái và nhánh tương ứng
        /// </summary>
        /// <param name="byFactionID"></param>
        /// <param name="nDamagePhysicsChange"></param>
        /// <param name="nDamageMagicChange"></param>
        /// <param name="nAttackRateChange"></param>
        /// <param name="nDefenceChange"></param>
        /// <param name="nLifeChange"></param>
        /// <param name="nManaChange"></param>
        private void CalculateFactionProperties(int byFactionID, out int nDamagePhysicsChange, out int nDamageMagicChange, out int nAttackRateChange, out int nDefenceChange, out int nLifeChange, out int nManaChange)
        {
            /// Cập nhật lại môn phái và nhánh
            this.m_byFactionId = byFactionID;

            /// Vật công ngoại
            nDamagePhysicsChange = KPlayerSetting.GetStrength2DamagePhysics(byFactionID, this.m_rPlayer.GetCurStrength()) + KPlayerSetting.GetDexterity2DamagePhysics(byFactionID, this.m_rPlayer.GetCurDexterity());
            /// Vật công nội
            nDamageMagicChange = KPlayerSetting.GetEnergy2DamageMagic(byFactionID, this.m_rPlayer.GetCurEnergy());
            /// Chính xác
            nAttackRateChange = KPlayerSetting.GetDexterity2AttackRate(byFactionID, this.m_rPlayer.GetCurDexterity());
            /// Né tránh
            nDefenceChange = KPlayerSetting.GetDexterity2Defence(byFactionID, this.m_rPlayer.GetCurDexterity());
            /// Sinh lực
            nLifeChange = KPlayerSetting.GetVitality2Life(byFactionID, this.m_rPlayer.GetCurVitality());
            /// Nội lực
            nManaChange = KPlayerSetting.GetEnergy2Mana(byFactionID, this.m_rPlayer.GetCurEnergy());
        }

        /// <summary>
        /// Thực hiện thay đổi môn phái và nhánh
        /// </summary>
        /// <param name="oldFactionID"></param>
        /// <param name="newFactionID"></param>
        private void ProcessFactionChange(int oldFactionID, int newFactionID)
        {
            /// Thực hiện xóa toàn bộ chỉ số cộng
            this.RemoveFactionProperties(oldFactionID, out int oDamagePhysicsChange, out int oDamageMagicChange, out int oAttackRateChange, out int oDefenceChange, out int oLifeChange, out int oManaChange);

            /// Nếu môn phái mới khác môn phái cũ
            if (oldFactionID != newFactionID)
            {
                /// Xóa toàn bộ kỹ năng phái cũ
                this.RemoveFactionSkills();
            }

            /// Chỉ số mới
            this.CalculateFactionProperties(newFactionID, out int nDamagePhysicsChange, out int nDamageMagicChange, out int nAttackRateChange, out int nDefenceChange, out int nLifeChange, out int nManaChange);

            /// Nếu môn phái mới khác môn phái cũ
            if (oldFactionID != newFactionID)
            {
                /// Thêm toàn bộ kỹ năng phái mới
                this.AddFactionSkills();
            }

            /// Cập nhật lại chỉ số cơ bản cho nhân vật
            /// Vật công ngoại
            this.m_rPlayer.ChangePhysicsDamage(-oDamagePhysicsChange + nDamagePhysicsChange);
            /// Vật công nội
            this.m_rPlayer.ChangeMagicDamage(-oDamageMagicChange + nDamageMagicChange);
            /// Chính xác
            this.m_rPlayer.ChangeAttackRating(-oAttackRateChange + nAttackRateChange, 0, 0);
            /// Né tránh
            this.m_rPlayer.ChangeDefend(-oDefenceChange + nDefenceChange, 0, 0);
            /// Sinh lực
            this.m_rPlayer.ChangeLifeMax(-oLifeChange + nLifeChange, 0, 0);
            /// Nội lực
            this.m_rPlayer.ChangeManaMax(-oManaChange + nManaChange, 0, 0);

            /// Nếu môn phái mới khác môn phái cũ
            if (oldFactionID != newFactionID)
            {
                /// Thực hiện đổi ngũ hành trang bị tương ứng môn phái mới
                this.m_rPlayer.GetPlayEquipBody().ChangeEquipsCorrespondingToSeries();
            }

            /// Gọi đến sự kiện thay đổi nhánh môn phái
            this.m_rPlayer.OnFactionChanged();
            /// Thực hiện làm mới thuộc tính trang bị
            this.m_rPlayer.GetPlayEquipBody().AttackAllEquipBody();
        }
    }
}
