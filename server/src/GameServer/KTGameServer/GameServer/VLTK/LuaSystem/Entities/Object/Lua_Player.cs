using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core.Activity.PlayerPray;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem.Entities.Math;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using MoonSharp.Interpreter;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.LuaSystem.Entities
{
    /// <summary>
    /// Đối tượng người chơi
    /// </summary>
    [MoonSharpUserData]
    public class Lua_Player
    {
        #region Base for all objects
        /// <summary>
        /// Đối tượng tham chiếu trong hệ thống
        /// </summary>
        [MoonSharpHidden]
        public KPlayer RefObject { get; set; }

        /// <summary>
        /// Trả về ID đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetID()
        {
            return this.RefObject.RoleID;
        }

        /// <summary>
        /// Trả về tên đối tượng
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            return this.RefObject.RoleName;
        }
        #endregion

        #region Base for all scene-objects
        /// <summary>
        /// Bản đồ hiện tại
        /// </summary>
        [MoonSharpHidden]
        public Lua_Scene CurrentScene { get; set; }

        /// <summary>
        /// Trả về vị trí của đối tượng
        /// </summary>
        /// <returns></returns>
        public Lua_Vector2 GetPos()
        {
            return new Lua_Vector2(this.RefObject.PosX, this.RefObject.PosY);
        }

        /// <summary>
        /// Trả về bản đồ hiện tại
        /// </summary>
        /// <returns></returns>
        public Lua_Scene GetScene()
        {
            return this.CurrentScene;
        }

        /// <summary>
        /// Trả về danh hiệu hiện tại
        /// </summary>
        /// <returns></returns>
        public string GetTitle()
        {
            return this.RefObject.Title;
        }

        /// <summary>
        /// Trả về loại đối tượng
        /// </summary>
        /// <returns></returns>
        public int GetObjectType()
        {
            return (int) ObjectTypes.OT_CLIENT;
        }
        #endregion


        /// <summary>
        /// Trả về ID môn phái của người chơi
        /// </summary>
        /// <returns></returns>
        public int GetFactionID()
        {
            return this.RefObject.m_cPlayerFaction.GetFactionId();
        }

        /// <summary>
        /// Trả về tên môn phái của người chơi
        /// </summary>
        /// <returns></returns>
        public string GetFactionName()
        {
            return this.RefObject.m_cPlayerFaction.GetFactionName();
        }

        /// <summary>
        /// Trả về giá trị điểm tiềm năng được cộng thêm của nhân vật
        /// </summary>
        /// <returns></returns>
        public int GetBonusRemainPotentialPoint()
        {
            return this.RefObject.GetBonusRemainPotentialPoints();
        }

        /// <summary>
        /// Cấp độ
        /// </summary>
        /// <returns></returns>
        public int GetLevel()
        {
            return this.RefObject.m_Level;
        }
        
        /// <summary>
        /// Có kỹ năng ID tương ứng không
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public bool HasSkill(int skillID)
        {
            return this.RefObject.Skills.HasSkill(skillID);
        }

        /// <summary>
        /// Trả về cấp độ hiện tại của kỹ năng (bao gồm cả các cấp độ cộng thêm từ kỹ năng, trang bị khác)
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public int GetSkillLevel(int skillID)
        {
            SkillLevelRef skillRef = this.RefObject.Skills.GetSkillLevelRef(skillID);
            if (skillRef == null)
            {
                return -1;
            }
            return skillRef.Level;
        }

        /// <summary>
        /// Trả về giới tính của người chơi
        /// </summary>
        /// <returns></returns>
        public int GetSex()
        {
            return this.RefObject.RoleSex;
        }

        /// <summary>
        /// Gia nhập môn phái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="factionID"></param>
        /// <returns>-1: Player NULL, -2: Môn phái không tồn tại, 0: Giới tính không phù hợp, 1: Thành công</returns>
        public int JoinFaction(int factionID)
        {
            return KTPlayerManager.JoinFaction(this.RefObject, factionID);
        }

        /// <summary>
        /// Tẩy điểm kỹ năng
        /// </summary>
        /// <param name="player"></param>
        /// <returns>-9999: Lỗi không rõ, -1: Player NULL, -2: Môn phái không tồn tại, 0: Giới tính không phù hợp, 1: Thành công</returns>
        public void ResetAllSkillsLevel()
        {
            KTPlayerManager.ResetAllSkillsLevel(this.RefObject);
        }

        /// <summary>
        /// Tẩy điểm tiềm năng
        /// </summary>
        public void UnAssignRemainPotentialPoints()
        {
            this.RefObject.UnAssignPotential();
        }

        /// <summary>
        /// Thiết lập cấp độ cho người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="level"></param>
        public void SetLevel(int level)
        {
            KTPlayerManager.SetRoleLevel(this.RefObject, level);
        }

        /// <summary>
        /// Tăng kinh nghiệm cho người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="exp"></param>
        public void AddExp(long exp)
        {
            KTPlayerManager.AddExp(this.RefObject, exp);
        }

        /// <summary>
        /// Thêm điểm tiềm năng do ăn bánh cho người chơi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="point"></param>
        public void AddBonusRemainPotentialPoint(int point)
        {
            int currentPoint = this.RefObject.GetBonusRemainPotentialPoints();
            this.RefObject.SetBonusRemainPotentialPoints(currentPoint + point);
        }

        /// <summary>
        /// Thêm kỹ năng cho nhân vật
        /// </summary>
        /// <param name="skillID"></param>
        public void AddSkill(int skillID)
        {
            this.RefObject.Skills.AddSkill(skillID);
        }

        /// <summary>
        /// Xóa kỹ năng của nhân vật
        /// </summary>
        /// <param name="skillID"></param>
        public void RemoveSkill(int skillID)
        {
            this.RefObject.Skills.RemoveSkill(skillID);
        }

        /// <summary>
        /// Thêm cấp độ cho kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="addLevel"></param>
        public void AddSkillLevel(int skillID, int addLevel)
        {
            this.RefObject.Skills.AddSkillLevel(skillID, addLevel);
        }

        /// <summary>
        /// Thêm Buff tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="level"></param>
        /// <param name="stack"></param>
        /// <param name="isFromItem"></param>
        public void AddBuff(int skillID, int level, int stack = 1, bool isFromItem = true)
        {
            this.RefObject.Buffs.AddBuff(skillID, level, stack, isFromItem);
        }

        /// <summary>
        /// Thêm Buff tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="level"></param>
        /// <param name="durationTicks"></param>
        /// <param name="stack"></param>
        /// <param name="saveToDB"></param>
        public void AddBuffWithDuration(int skillID, int level, long durationTicks, int stack = 1, bool saveToDB = false)
		{
            this.RefObject.Buffs.AddBuff(skillID, level, durationTicks, stack, saveToDB);
		}

        /// <summary>
        /// Xóa Buff có ID tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        public void RemoveBuff(int skillID)
        {
            this.RefObject.Buffs.RemoveBuff(skillID);
        }

        /// <summary>
        /// Kiểm tra Buff ID tương ứng có tồn tại trên người không
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public bool HasBuff(int skillID)
		{
            return this.RefObject.Buffs.HasBuff(skillID);
		}

        /// <summary>
        /// Đối tượng còn sống không
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return !this.RefObject.IsDead();
        }

        /// <summary>
        /// Dịch chuyển người chơi sang bản đồ khác
        /// </summary>
        /// <param name="mapID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public void ChangeScene(int mapID, int posX, int posY)
        {
            /// Nếu đối tượng đã chết thì không chuyển Map được
            if (this.RefObject.IsDead())
            {
                return;
            }

            GameMap gameMap = KTMapManager.Find(mapID);
            /// Lấy dữ liệu bản đồ đích đến
            if (gameMap != null)
            {
                /// Nếu bản đồ đích khác bản đồ hiện tại
                if (this.RefObject.CurrentMapCode != mapID)
                {
                    KTPlayerManager.ChangeMap(this.RefObject, mapID, posX, posY);
                }
                else
                {
                    KTPlayerManager.ChangePos(this.RefObject, posX, posY);
                }
            }
            /// Thông báo bản đồ không tồn tại
            else
            {
                KTPlayerManager.ShowNotification(this.RefObject, "Bản đồ này chưa được mở!");
            }
        }

        /// <summary>
        /// Thiết lập tọa độ cho đối tượng
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public void SetPos(int posX, int posY)
        {
            /// Nếu đối tượng đã chết thì không chuyển Map được
            if (this.RefObject.IsDead())
            {
                return;
            }

            /// Gửi gói tin thông báo đối tượng thay đổi vị trí
            KTPlayerManager.ChangePos(this.RefObject, posX, posY);
        }

        /// <summary>
        /// Gửi thông báo ToolTip đến người chơi
        /// </summary>
        /// <param name="message"></param>
        public void AddNotification(string message)
        {
            KTPlayerManager.ShowNotification(this.RefObject, message);
        }

        /// <summary>
        /// Thiết lập thông tin điểm về thành mặc định khi chết
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public void SetDefaultReliveInfo(int mapCode, int posX, int posY)
        {
            KT_TCPHandler.SetDefaultRelivePos(this.RefObject, mapCode, posX, posY);
        }

        /// <summary>
        /// Trả ID bản đồ điểm về thành khi trọng thương
        /// </summary>
        /// <returns></returns>
        public int GetDefaultReliveSceneID()
        {
            KT_TCPHandler.GetPlayerDefaultRelivePos(this.RefObject, out int mapCode, out int posX, out int posY);
            return mapCode;
        }

        /// <summary>
        /// Trả về tọa độ điểm về thành khi trọng thương
        /// </summary>
        /// <returns></returns>
        public Lua_Vector2 GetDefaultRelivePos()
        {
            KT_TCPHandler.GetPlayerDefaultRelivePos(this.RefObject, out int mapCode, out int posX, out int posY);
            return new Lua_Vector2(posX, posY);
        }

        /// <summary>
        /// Dùng kỹ năng tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="skillLevel"></param>
        /// <param name="isChildSkill"></param>
        public bool UseSkill(int skillID, int skillLevel, bool isChildSkill = false)
        {
            SkillDataEx skillData = KSkill.GetSkillData(skillID);
            if (skillData == null)
            {
                return false;
            }
            SkillLevelRef skillRef = new SkillLevelRef()
            {
                Data = skillData,
                AddedLevel = skillLevel,
                BonusLevel = 0,
                CanStudy = false,
            };
            KTSkillManager.UseSkillResult result = KTSkillManager.UseSkill(this.RefObject, null, null, skillRef, isChildSkill);
            return result == KTSkillManager.UseSkillResult.Success;
        }

        /// <summary>
        /// Thiết lập trạng thái PK
        /// </summary>
        /// <param name="pkMode"></param>
        public void SetPKMode(int pkMode)
        {
            this.RefObject.PKMode = pkMode;
        }

        /// <summary>
        /// Trả về trạng thái PK
        /// </summary>
        /// <returns></returns>
        public int GetPKMode()
        {
            return this.RefObject.PKMode;
        }

        /// <summary>
        /// Thiết lập Camp
        /// </summary>
        /// <param name="camp"></param>
        public void SetCamp(int camp)
        {
            this.RefObject.Camp = camp;
        }

        /// <summary>
        /// Trả về Camp
        /// </summary>
        /// <returns></returns>
        public int GetCamp()
        {
            return this.RefObject.Camp;
        }

        /// <summary>
        /// Trả về trị PK
        /// </summary>
        /// <returns></returns>
        public int GetPKValue()
        {
            return this.RefObject.PKValue;
        }

        /// <summary>
        /// Thiết lập trị PK
        /// </summary>
        /// <param name="pkValue"></param>
        public void SetPKValue(int pkValue)
        {
            this.RefObject.PKValue = pkValue;
        }

        /// <summary>
        /// Trả về ID bang hội
        /// </summary>
        /// <returns></returns>
        public int GetGuildID()
		{
            return this.RefObject.GuildID;
		}

        /// <summary>
        /// Trả về chức vị trong bang
        /// </summary>
        /// <returns></returns>
        public int GetGuildRank()
		{
            return this.RefObject.GuildRank;
		}

        /// <summary>
        /// Trả về ID tộc
        /// </summary>
        /// <returns></returns>
        public int GetFamilyID()
		{
            return this.RefObject.FamilyID;
		}

        /// <summary>
        /// Trả về chức vị trong tộc
        /// </summary>
        /// <returns></returns>
        public int GetFamilyRank()
		{
            return this.RefObject.FamilyRank;
		}

        /// <summary>
        /// Trả về số uy danh hiện có
        /// </summary>
        /// <returns></returns>
        public int GetPrestige()
		{
            return this.RefObject.Prestige;
		}

        /// <summary>
        /// Thiết lập số uy danh hiện có
        /// </summary>
        /// <param name="value"></param>
        public void SetPrestige(int value)
		{
            this.RefObject.Prestige = value;
		}

        /// <summary>
        /// Reset số lượt đi phụ bản trong ngày
        /// </summary>
        /// <param name="eventID"></param>
        public void ResetCopySceneEnterTimes(int eventID)
		{
            CopySceneEventManager.SetCopySceneTotalEnterTimesToday(this.RefObject, (DailyRecord) eventID, 0);
        }

        /// <summary>
        /// Trả về số lượt quay chúc phúc trong ngày
        /// </summary>
        /// <returns></returns>
        public int GetPrayTimes()
		{
            return KTPlayerPrayManager.GetTotalTurnLeft(this.RefObject);
		}

        /// <summary>
        /// Thiết lập số lượt quay chúc phúc trong ngày
        /// </summary>
        /// <param name="value"></param>
        public void SetPrayTimes(int value)
		{
            KTPlayerPrayManager.SetTotalTurnLeft(this.RefObject, value);
		}

        /// <summary>
        /// Thiết lập kinh nghiệm Tu Luyện Châu
        /// </summary>
        /// <param name="exp"></param>
        public void SetXiuLianZhu_Exp(int exp)
		{
            this.RefObject.XiuLianZhu_Exp = exp;
        }

        /// <summary>
        /// Trả về giá trị Kinh nghiệm Tu luyện châu còn lại
        /// </summary>
        /// <returns></returns>
        public int GetXiuLianZhu_Exp()
		{
            return this.RefObject.XiuLianZhu_Exp;
		}

        /// <summary>
        /// Trả về thời gian Tu luyện còn lại (giờ * 10)
        /// </summary>
        /// <returns></returns>
        public int GetXiuLianZhu_TimeLeft()
		{
            return this.RefObject.XiuLianZhu_TotalTime;
		}

        /// <summary>
        /// Thiết lập thời gian Tu luyện còn lại (giờ * 10)
        /// </summary>
        /// <param name="hour10"></param>
        public void SetXiuLianZhu_TimeLeft(int hour10)
		{
            this.RefObject.XiuLianZhu_TotalTime = hour10;
		}

        /// <summary>
        /// Trả về giá trị kinh nghiệm tối đa có thể Tu Luyện
        /// </summary>
        /// <returns></returns>
        public int GetLimitXiuLianZhu_Exp()
		{
            return ItemXiuLianZhuManager.GetLimitExp(this.RefObject);
		}

        /// <summary>
        /// Trả về ID máy chủ
        /// </summary>
        /// <returns></returns>
        public int GetZoneID()
        {
            return this.RefObject.ZoneID;
        }

        /// <summary>
        /// Trả về số giờ (*10) Tu Luyện Châu có được thêm mỗi ngày
        /// </summary>
        /// <returns></returns>
        public int GetXiuLianZhu_TimeAddedPerDay()
		{
            return ItemXiuLianZhuManager.GetHourAddPerDay() * 10;
		}

        /// <summary>
        /// Trả về giá trị kinh nghiệm Tu Luyện có được mỗi giờ
        /// </summary>
        /// <param name="hour10"></param>
        /// <returns></returns>
        public int GetXiuLianZhu_ExpAddedByHour(int hour10)
		{
            return ItemXiuLianZhuManager.GetAddedExpByHour(this.RefObject, hour10);
		}

        /// <summary>
        /// Kiểm tra bản thân có nhóm không
        /// </summary>
        /// <returns></returns>
        public bool HasTeam()
        {
            return this.RefObject.TeamID != -1 && KTTeamManager.IsTeamExist(this.RefObject.TeamID);
        }

        /// <summary>
        /// Trả về số thành viên trong nhóm
        /// </summary>
        /// <returns></returns>
        public int GetTeamSize()
        {
            return KTTeamManager.GetTeamSize(this.RefObject.TeamID);
        }

        /// <summary>
        /// Kiểm tra bản thân có phải trưởng nhóm không
        /// </summary>
        /// <returns></returns>
        public bool IsTeamLeader()
        {
            return this.RefObject.TeamID != -1 && KTTeamManager.IsTeamExist(this.RefObject.TeamID) && this.RefObject.TeamLeader == this.RefObject;
        }

        /// <summary>
        /// Trả về tổng số Pet hiện có
        /// </summary>
        /// <returns></returns>
        public int GetTotalPets()
        {
            /// Toác
            if (this.RefObject.PetList == null)
            {
                return 0;
            }
            return this.RefObject.PetList.Count;
        }
    }
}
