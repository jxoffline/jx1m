using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using HSGameEngine.GameEngine.Network.Tools;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using FS.VLTK.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Network.Skill
{
	/// <summary>
	/// Quản lý Socket kỹ năng
	/// </summary>
	public static class KTTCPSkillManager
    {
        #region Làm mới danh sách kỹ năng
        /// <summary>
        /// Nhận gói tin làm mới dữ liệu kỹ năng từ Server
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveRenewSkillList(int cmdID, byte[] data, int length)
        {
            try
            {
                KeyValuePair<int, List<SkillData>> pair = DataHelper.BytesToObject<KeyValuePair<int, List<SkillData>>>(data, 0, length);
                Global.Data.RoleData.SkillDataList = pair.Value;
                Global.Data.RoleData.SkillPoint = pair.Key;

                if (PlayZone.Instance.UISkillTree != null)
                {
                    PlayZone.Instance.UISkillTree.RefreshSkillData();
                }

                if (PlayZone.Instance.UIBottomBar != null)
                {
                    PlayZone.Instance.UIBottomBar.UISkillBar.RefreshCooldowns();
                    PlayZone.Instance.UIBottomBar.UISkillBar.RefreshSkillIcon();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Gửi yêu cầu cộng điểm kỹ năng lên Server 
        /// <param name="list"></param>
        /// <param name="selectedSubID"></param>
        /// </summary>
        public static void SendDistributeSkillPoints(Dictionary<int, int> list, int selectedSubID)
        {
            try
            {
                if (!Global.Data.PlayGame)
                {
                    return;
                }

                C2G_DistributeSkillPoint distributeSkillPoint = new C2G_DistributeSkillPoint()
                {
                    SelectedRouteID = selectedSubID,
                    ListDistributedSkills = list,
                };

                byte[] bytes = DataHelper.ObjectToBytes<C2G_DistributeSkillPoint>(distributeSkillPoint);
                GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_SKILL_ADDPOINT)));
            }
            catch (Exception) { }
        }
        #endregion

        #region Thiết lập QuickKey
        /// <summary>
        /// Gửi yêu cầu thiết lập QuickKey vào khung kỹ năng
        /// </summary>
        /// <param name="skills"></param>
        public static void SendSaveQuickKey(List<int> skills)
        {
            try
            {
                string strcmd = string.Join("|", skills);
                GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, strcmd, (int)(TCPGameServerCmds.CMD_KT_C2G_SET_SKILL_TO_QUICKKEY)));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Gửi yêu cầu thiết lập và kích hoạt kỹ năng vòng sáng
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="status"></param>
        public static void SendSaveAndActivateAruaKey(int skillID, int status)
        {
            try
            {
                string strcmd = string.Format("{0}_{1}", skillID, status);
                GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, strcmd, (int) (TCPGameServerCmds.CMD_KT_C2G_SET_AND_ACTIVATE_AURA)));
            }
            catch (Exception) { }
        }
        #endregion

        #region Sử dụng kỹ năng
        /// <summary>
        /// Gửi yêu cầu sử dụng kỹ năng lên Server
        /// </summary>
        /// <param name="skillID"></param>
        public static void SendUseSkill(int skillID, bool ignoreTarget = false)
        {
            try
            {
                if (!Global.Data.PlayGame)
                {
                    return;
                }

                /// Nếu đang khinh công
                if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_jump)
                {
                    return;
                }
                /// Nếu không thể dùng kỹ năng
                else if (!Global.Data.Leader.CanDoLogic)
                {
                    return;
                }

                /// Mục tiêu
                GSprite target = SkillManager.SelectedTarget;

                C2G_UseSkill useSkill = new C2G_UseSkill()
                {
                    Direction = (int) Global.Data.Leader.Direction,
                    SkillID = skillID,
                    TargetID = ignoreTarget ? - 1 : target == null ? -1 : target.RoleID,
                    PosX = Global.Data.Leader.PosX,
                    PosY = Global.Data.Leader.PosY,
                    TargetPosX = (int) Global.Data.GameScene.LastSkillMarkTargetPosition.x,
                    TargetPosY = (int) Global.Data.GameScene.LastSkillMarkTargetPosition.y,
                };

                byte[] bytes = DataHelper.ObjectToBytes<C2G_UseSkill>(useSkill);
                GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_USESKILL)));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận phản hồi từ Server về kết quả sử dụng kỹ năng
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveUseSkillResult(string[] fields)
        {
            try
            {
                int res = int.Parse(fields[0]);
                switch (res)
                {
                    case (int)UseSkillResult.Caster_Is_Null:
                        KTGlobal.AddNotification("Không có đỗi tượng xuất chiêu!");
                        break;
                    case (int)UseSkillResult.Not_Enough_Mana:
                        KTGlobal.AddNotification("Nội lực không đủ!");
                        break;
                    case (int)UseSkillResult.No_Corresponding_Skill_Found:
                        KTGlobal.AddNotification("Không có kỹ năng tương ứng!");
                        break;
                    case (int)UseSkillResult.Passive_Skill_Can_Not_Be_Used:
                        KTGlobal.AddNotification("Kỹ năng bị động không thể sử dụng!");
                        break;
                    case (int)UseSkillResult.Skill_Is_Cooldown:
                        KTGlobal.AddNotification("Kỹ năng chưa sẵn sàng!");
                        break;
                    case (int)UseSkillResult.Target_Not_In_Range:
                        KTGlobal.AddNotification("Mục tiêu không nằm trong phạm vi!");
                        break;
                    case (int)UseSkillResult.No_Target_Found:
                        KTGlobal.AddNotification("Không có mục tiêu!");
                        break;
                    case (int)UseSkillResult.Bullet_Data_Not_Found:
                        KTGlobal.AddNotification("Không tìm thấy thông tin đạn!");
                        break;
                    case (int)UseSkillResult.Can_Not_Attack_Peace_Target:
                        KTGlobal.AddNotification("Không thể tấn công mục tiêu trong trạng thái hòa bình!");
                        break;
                    case (int)UseSkillResult.Caster_Is_Dead:
                        //KTGlobal.AddNotification("Không thể sử dụng kỹ năng khi nhân vật đã tử vong!");
                        break;
                    case (int)UseSkillResult.Can_Not_Fly_This_Time:
                        //KTGlobal.AddNotification("Không thể sử dụng kỹ năng khinh công lúc này!");
                        break;
                    case (int)UseSkillResult.Can_Not_Use_Skill_While_Being_Locked:
                        KTGlobal.AddNotification("Không thể sử dụng kỹ năng khi đang trong trạng thái bị khống chế!");
                        break;
                    case (int)UseSkillResult.Can_Not_Use_Skill_While_Riding:
                        KTGlobal.AddNotification("Không thể sử dụng kỹ năng này trong trạng thái cưỡi!");
                        break;
                    case (int)UseSkillResult.Unsuitable_Weapon:
                        KTGlobal.AddNotification("Vũ khí không thích hợp!");
                        break;
                    case (int)UseSkillResult.Unsuitable_Faction:
                        KTGlobal.AddNotification("Môn phái không thích hợp!");
                        break;
                    case (int)UseSkillResult.Unsuitable_Route:
                        KTGlobal.AddNotification("Hệ phái không thích hợp!");
                        break;
                }

                /// Đánh dấu không đợi dùng kỹ năng nữa
                SkillManager.IsWaitingToUseSkill = false;

                /// Nếu GS báo về là mục tiêu không nằm trong phạm vi
                if (res == (int) UseSkillResult.Target_Not_In_Range || res == (int) UseSkillResult.No_Target_Found || res == (int) UseSkillResult.Can_Not_Attack_Peace_Target)
                {
                    /// Đổi mục tiêu
                    KTAutoFightManager.Instance.ChangeAutoFightTarget();
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo từ Server có đối tượng đang dùng kỹ năng ở xung quanh
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveObjectUseSkill(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_UseSkill useSkill = DataHelper.BytesToObject<G2C_UseSkill>(data, 0, length);

                /// Đối tượng xuất chiêu
                GSprite sprite;
                if (useSkill.RoleID == Global.Data.RoleData.RoleID)
                {
                    sprite = Global.Data.Leader;

                    /// Ngừng gọi pet
                    KTAutoFightManager.Instance.IsCallingPet = false;
                }
                else
                {
                    sprite = KTGlobal.FindSpriteByID(useSkill.RoleID);
                }

                /// Nếu đối tượng không tồn tại
                if (sprite == null)
                {
                    return;
                }

                /// Dừng di chuyển ngay lập tức
                sprite.StopMove();

                /// Vị trí hiện tại của đối tượng
                Vector2 currentPos = sprite.PositionInVector2;

                /// Nếu khoảng cách quá lớn
                if (Vector2.Distance(currentPos, new Vector2(useSkill.PosX, useSkill.PosY)) >= 100)
                {
                    /// Cập nhật vị trí xuất chiêu cho đối tượng
                    sprite.Coordinate = new Drawing.Point(useSkill.PosX, useSkill.PosY);
                }
                    
                /// Thực hiện dùng kỹ năng
                SkillManager.SpriteUseSkill(sprite, (Direction) useSkill.Direction, useSkill.SkillID, useSkill.IsSpecialAttack);
            }
            catch (Exception) { }
        }
        #endregion

        #region Kỹ năng Cooldown
        /// <summary>
        /// Nhận thông báo kỹ năng trong trạng thái Cooldown từ Server
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveSkillCooldown(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_SkillCooldown useSkill = DataHelper.BytesToObject<G2C_SkillCooldown>(data, 0, length);

                /// Tìm kỹ năng tương ứng trong data của Leader
                SkillData skill = Global.Data.RoleData.SkillDataList.Where(x => x.SkillID == useSkill.SkillID).FirstOrDefault();
                /// Nếu tìm thấy thì cập nhật dữ liệu
                if (skill != null)
                {
                    skill.LastUsedTick = useSkill.StartTick;
                    skill.CooldownTick = useSkill.CooldownTick;
                }

                /// đồng bộ dữ liệu lên UI
                if (PlayZone.Instance.UIBottomBar != null)
                {
                    PlayZone.Instance.UIBottomBar.UISkillBar.UpdateSkillCooldown(useSkill.SkillID);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo làm mới toàn bộ thời gian phục hồi kỹ năng từ Server
        /// </summary>
        public static void ReceiveResetSkillCooldown(string[] excludedSkills)
        {
            try
            {
                /// Danh sách loại trừ
                List<int> listExcludedSkills = new List<int>();
                foreach (string skillID in excludedSkills)
                {
                    try
                    {
                        int id = int.Parse(skillID);
                        listExcludedSkills.Add(id);
                    }
                    catch (Exception) { }
                }

                foreach (SkillData skillData in Global.Data.RoleData.SkillDataList)
                {
                    /// Nếu không chứa trong danh sách loại trừ
                    if (!listExcludedSkills.Contains(skillData.SkillID))
                    {
                        skillData.LastUsedTick = 0;
                    }
                }

                /// đồng bộ dữ liệu lên UI
                if (PlayZone.Instance.UIBottomBar != null)
                {
                    PlayZone.Instance.UIBottomBar.UISkillBar.RefreshCooldowns();
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Đạn bay, nổ
        /// <summary>
        /// Tạo đạn với gói tin tương ứng
        /// </summary>
        /// <param name="bullet"></param>
        private static void CreateBullet(G2C_CreateBullet bullet)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            try
            {
                /// Chủ nhân đạn
                GSprite caster = null;
                if (bullet.CasterID != -1)
                {
                    /// Nếu là Leader
                    if (bullet.CasterID == Global.Data.RoleData.RoleID)
                    {
                        caster = Global.Data.Leader;
                    }
                    /// Là đối tượng khác
                    else
                    {
                        caster = KTGlobal.FindSpriteByID(bullet.CasterID);
                    }
                }

                /// Nếu là đạn bay theo đường tròn
                if (bullet.CircleRadius > 0)
                {
                    SkillManager.CreateBulletFlyByCircle(caster, bullet.Delay, bullet.BulletID, bullet.ResID, bullet.LifeTime, bullet.CircleRadius, bullet.DirVector, bullet.FromPos, bullet.ToPos, bullet.FollowCaster, bullet.Velocity, bullet.LoopAnimation);
                }
                /// Nếu không phải
                else
                {
                    /// Nếu không có mục tiêu đuổi
                    if (bullet.TargetID == -1)
                    {
                        /// Nếu là đạn nổ tại vị trí chỉ định
                        if (bullet.FromPos == bullet.ToPos)
                        {
                            /// Nếu là ném
                            if (bullet.Velocity > 0)
                            {
                                SkillManager.CreateBulletThrowing(caster, bullet.Delay, bullet.BulletID, bullet.ResID, bullet.FromPos, bullet.LifeTime, bullet.Velocity, bullet.LoopAnimation);
                            }
                            /// Nếu là đạn tĩnh
                            else
                            {
                                SkillManager.CreateBulletStatic(caster, bullet.Delay, bullet.BulletID, bullet.ResID, bullet.FromPos, bullet.LifeTime, bullet.LoopAnimation, bullet.LifeTime >= 5);
                            }
                        }
                        else
                        {
                            /// Nếu là loại đạn bay từ vị trí ra chiêu đến vị trí FromPos sau đó bay đến ToPos
                            if (bullet.Delay < 0)
                            {
                                SkillManager.CreateBulletLinearFlyThenGoToPos(caster, bullet.BulletID, bullet.ResID, bullet.FromPos, bullet.ToPos, bullet.Velocity, bullet.LoopAnimation);
                            }
                            /// Nếu là loại đạn bay từ vị trí FromPos đến vị trí ToPos
                            else
                            {
                                SkillManager.CreateBullet(caster, bullet.Delay, bullet.BulletID, bullet.ResID, bullet.FromPos, bullet.ToPos, bullet.Velocity, bullet.Comeback, bullet.LoopAnimation);
                            }
                        }
                    }
                    /// Nếu có mục tiêu đuổi
                    else
                    {
                        GSprite target = KTGlobal.FindSpriteByID(bullet.TargetID);
                        if (target == null || target.Role2D == null)
                        {
                            KTDebug.LogError("Create Chase Target bullet -> Target not FOUND");
                            return;
                        }
                        SkillManager.CreateBulletChaseTarget(caster, bullet.Delay, bullet.BulletID, bullet.ResID, bullet.FromPos, target.Role2D, bullet.Velocity, bullet.LoopAnimation);
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo tạo đạn bay từ Server
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveCreateBullet(int cmdID, byte[] data, int length)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            try
            {
                G2C_CreateBullet bullet = DataHelper.BytesToObject<G2C_CreateBullet>(data, 0, length);
                KTTCPSkillManager.CreateBullet(bullet);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo tạo nhiều tia đạn bay từ Server
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveCreateBullets(int cmdID, byte[] data, int length)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            try
            {
                List<G2C_CreateBullet> bullets = DataHelper.BytesToObject<List<G2C_CreateBullet>>(data, 0, length);
                if (bullets != null)
                {
                    foreach (G2C_CreateBullet bullet in bullets)
                    {
                        KTTCPSkillManager.CreateBullet(bullet);
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Tạo hiệu ứng đạn nổ theo gói tin tương ứng
        /// </summary>
        /// <param name="explode"></param>
        private static void CreateBulletExplode(G2C_BulletExplode explode)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            try
            {
                /// Mục tiêu nổ
                GSprite target = null;
                if (explode.TargetID != -1)
                {
                    target = KTGlobal.FindSpriteByID(explode.TargetID);
                }
                
                /// Tạo hiệu ứng nổ tại vị trí tương ứng
                SkillManager.CreateExplosion(explode.Delay, explode.ResID, explode.Pos, target != null ? target.Role2D : null);
            }
            catch (Exception) { }
        } 

        /// <summary>
        /// Nhận thông báo từ Server đạn nổ
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveBulletExplode(int cmdID, byte[] data, int length)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            try
            {
                G2C_BulletExplode explode = DataHelper.BytesToObject<G2C_BulletExplode>(data, 0, length);

                KTTCPSkillManager.CreateBulletExplode(explode);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo từ Server đạn nổ
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveBulletExplodes(int cmdID, byte[] data, int length)
        {
            /// Nếu FPS thấp thì không thực hiện
            if (MainGame.Instance.GetRenderQuality() == MainGame.RenderQuality.Low)
            {
                return;
            }

            try
            {
                List<G2C_BulletExplode> explodes = DataHelper.BytesToObject<List<G2C_BulletExplode>>(data, 0, length);
                if (explodes != null)
                {
                    foreach (G2C_BulletExplode explode in explodes)
                    {
                        KTTCPSkillManager.CreateBulletExplode(explode);
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo từ Server đối tượng có sự thay đổi Buff
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveSpriteBuff(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_SpriteBuff spriteBuff = DataHelper.BytesToObject<G2C_SpriteBuff>(data, 0, length);
                if (spriteBuff == null)
                {
                    return;
                }

                //KTDebug.LogError("Receive Buff -> " + (spriteBuff.PacketType == 0 ? "REMOVE" : "ADD") + " - Skill ID = " + spriteBuff.SkillID + " - Res ID = " + spriteBuff.ResID);

                /// Đối tượng thao tác
                GSprite sprite;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == spriteBuff.RoleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(spriteBuff.RoleID);
                }

                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Loại gói tin là gì
                switch (spriteBuff.PacketType)
                {
                    /// Xóa Buff
                    case 0:
                        /// Nếu là Leader
                        if (Global.Data.RoleData.RoleID == spriteBuff.RoleID)
                        {
                            if (Global.Data.RoleData.BufferDataList == null)
							{
                                Global.Data.RoleData.BufferDataList = new List<BufferData>();
                            }

                            /// Tiến hành xóa Buff
                            Global.Data.RoleData.BufferDataList.RemoveAll(x => x.BufferID == spriteBuff.SkillID);

                            /// Xóa Buff khỏi khay Buff
                            if (PlayZone.Instance.UIRolePart != null)
                            {
                                PlayZone.Instance.UIRolePart.UIBuffList.RemoveBuff(spriteBuff.SkillID);
                            }
                        }

                        /// Nếu là người chơi
                        if (sprite.ComponentCharacter != null)
                        {
                            sprite.ComponentCharacter.RemoveEffect(spriteBuff.ResID);

                            if (Global.Data.OtherRoles.TryGetValue(spriteBuff.RoleID, out RoleData rd))
                            {
                                if (rd.BufferDataList == null)
                                {
                                    rd.BufferDataList = new List<BufferData>();
                                }

                                /// Tìm Buff tương ứng trong danh sách
                                rd.BufferDataList.RemoveAll(x => x.BufferID == spriteBuff.SkillID);
                            }
                        }
                        /// Nếu là quái hoặc NPC
                        else if (sprite.ComponentMonster != null)
                        {
                            sprite.ComponentMonster.RemoveEffect(spriteBuff.ResID);

                            if (Global.Data.SystemMonsters.TryGetValue(spriteBuff.RoleID, out MonsterData md))
                            {
                                if (md.ListBuffs == null)
                                {
                                    md.ListBuffs = new List<BufferData>();
                                }

                                /// Xóa Buff tương ứng
                                md.ListBuffs.RemoveAll(x => x.BufferID == spriteBuff.SkillID);
                            }
                        }
                        break;
                    /// Thêm Buff
                    case 1:
                        /// Nếu là Leader
                        if (Global.Data.RoleData.RoleID == spriteBuff.RoleID)
                        {
                            /// Tạo mới Buff
                            BufferData buff = new BufferData()
                            {
                                BufferID = spriteBuff.SkillID,
                                BufferSecs = spriteBuff.Duration,
                                StartTime = spriteBuff.StartTick,
                                BufferVal = spriteBuff.Level,
                                CustomProperty = spriteBuff.Properties,
                            };

                            /// Nếu có rồi thì xóa trước
                            Global.Data.RoleData.BufferDataList.RemoveAll(x => x.BufferID == spriteBuff.SkillID);

                            /// Thêm Buff vào danh sách
                            Global.Data.RoleData.BufferDataList.Add(buff);

                            /// Thêm Buff vào khay Buff
                            if (PlayZone.Instance.UIRolePart != null)
                            {
                                PlayZone.Instance.UIRolePart.UIBuffList.AddBuff(buff);
                            }
                        }

                        /// Nếu là người chơi
                        if (sprite.ComponentCharacter != null)
                        {
                            sprite.AddBuff(spriteBuff.ResID, spriteBuff.Duration == -1 ? -1 : spriteBuff.Duration - (KTGlobal.GetServerTime() - spriteBuff.StartTick));

                            /// Nếu là người chơi khác
                            if (Global.Data.OtherRoles.TryGetValue(spriteBuff.RoleID, out RoleData rd))
                            {
                                /// Nếu có rồi thì xóa trước
                                rd.BufferDataList.RemoveAll(x => x.BufferID == spriteBuff.SkillID);

                                /// Thêm Buff vào danh sách
                                rd.BufferDataList.Add(new BufferData()
                                {
                                    BufferID = spriteBuff.SkillID,
                                    BufferSecs = spriteBuff.Duration,
                                    StartTime = spriteBuff.StartTick,
                                    BufferVal = spriteBuff.Level,
                                });
                            }
                        }
                        /// Nếu là quái hoặc NPC
                        else if (sprite.ComponentMonster != null)
                        {
                            sprite.AddBuff(spriteBuff.ResID, spriteBuff.Duration == -1 ? -1 : spriteBuff.Duration - (KTGlobal.GetServerTime() - spriteBuff.StartTick));

                            if (Global.Data.SystemMonsters.TryGetValue(spriteBuff.RoleID, out MonsterData md))
                            {
                                /// Nếu có rồi thì xóa trước
                                md.ListBuffs.RemoveAll(x => x.BufferID == spriteBuff.SkillID);

                                /// Thêm Buff vào danh sách
                                md.ListBuffs.Add(new BufferData()
                                {
                                    BufferID = spriteBuff.SkillID,
                                    BufferSecs = spriteBuff.Duration,
                                    StartTime = spriteBuff.StartTick,
                                    BufferVal = spriteBuff.Level,
                                });
                            }
                        }
                        break;
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo từ Server đối tượng thay đổi trạng thái ẩn thân
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveObjectInvisibleStateChanged(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_SpriteInvisibleStateChanged spriteInvisibleStateChanged = DataHelper.BytesToObject<G2C_SpriteInvisibleStateChanged>(data, 0, length);

                /// Đối tượng thao tác
                GSprite sprite = null;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == spriteInvisibleStateChanged.RoleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(spriteInvisibleStateChanged.RoleID);
                }

                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Cập nhật hiển thị cho đối tượng tương ứng
                if (sprite.ComponentMonster != null)
                {
                    sprite.ComponentMonster.MaxAlpha = spriteInvisibleStateChanged.Type == 1 ? 0.5f : 1f;
                }
                else if (sprite.ComponentCharacter != null)
                {
                    sprite.ComponentCharacter.MaxAlpha = spriteInvisibleStateChanged.Type == 1 ? 0.5f : 1f;
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Tốc biến, khinh công
        /// <summary>
        /// Nhận thông báo từ Server đối tượng tốc biến đến vị trí chỉ định
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveTeleportToPosition(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_BlinkToPosition blink = DataHelper.BytesToObject<G2C_BlinkToPosition>(data, 0, length);

                /// Đối tượng thao tác
                GSprite sprite = null;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == blink.RoleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(blink.RoleID);
                }

                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Nếu đối tượng đang trong trạng thái khinh công
                if (sprite.CurrentAction == KE_NPC_DOING.do_runattack || sprite.CurrentAction == KE_NPC_DOING.do_jump)
                {
                    if (sprite.FlyingCoroutine != null)
                    {
                        PlayZone.Instance.StopCoroutine(sprite.FlyingCoroutine);
                        sprite.FlyingCoroutine = null;
                        sprite.DoStand();
                    }
                }

                /// Vị trí đích đến
                Vector2 destPos = new Vector2(blink.PosX, blink.PosY); 
                
                /// Nếu vị trí hiện tại của đối tượng trùng với vị trí đích đến thì bỏ qua
                if (sprite.PositionInVector2 == destPos)
                {
                    return;
                }

                ///// Nếu là Leader
                //if (Global.Data.RoleData.RoleID == blink.RoleID)
                //{
                //    Global.Data.RoleData.PosX = blink.PosX;
                //    Global.Data.RoleData.PosY = blink.PosY;
                //}
                ///// Nếu là người chơi khác
                //else if (Global.Data.OtherRoles.TryGetValue(blink.RoleID, out RoleData rd))
                //{
                //    rd.PosX = blink.PosX;
                //    rd.PosY = blink.PosY;
                //}
                ///// Nếu là quái
                //else if (Global.Data.SystemMonsters.TryGetValue(blink.RoleID, out MonsterData md))
                //{
                //    md.PosX = blink.PosX;
                //    md.PosY = blink.PosY;
                //}

                /// Thực hiện quay đối tượng theo hướng chỉ định
                sprite.Direction = (Direction) blink.Direction;

                /// Ngừng StoryBoard đang thực thi
                sprite.StopMove();

                /// Thực hiện hiệu ứng
                SkillManager.SpriteBlinkToPosition(sprite, destPos, blink.Duration);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo từ Server đối tượng khinh công đến vị trí chỉ định
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveFlyToPosition(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_FlyToPosition fly = DataHelper.BytesToObject<G2C_FlyToPosition>(data, 0, length);

                /// Đối tượng thao tác
                GSprite sprite = null;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == fly.RoleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(fly.RoleID);
                }

                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Nếu đối tượng đang trong trạng thái khinh công
                if (sprite.CurrentAction == KE_NPC_DOING.do_runattack || sprite.CurrentAction == KE_NPC_DOING.do_jump)
                {
                    if (sprite.FlyingCoroutine != null)
                    {
                        PlayZone.Instance.StopCoroutine(sprite.FlyingCoroutine);
                        sprite.FlyingCoroutine = null;
                        sprite.DoStand();
                    }
                }

                /// Vị trí đích đến
                Vector2 destPos = new Vector2(fly.PosX, fly.PosY); 
                
                /// Nếu vị trí hiện tại của đối tượng trùng với vị trí đích đến thì bỏ qua
                if (sprite.PositionInVector2 == destPos)
                {
                    return;
                }

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == fly.RoleID)
                {
                    Global.Data.RoleData.PosX = fly.PosX;
                    Global.Data.RoleData.PosY = fly.PosY;
                }
                /// Nếu là người chơi khác
                else if (Global.Data.OtherRoles.TryGetValue(fly.RoleID, out RoleData rd))
                {
                    rd.PosX = fly.PosX;
                    rd.PosY = fly.PosY;
                }
                /// Nếu là quái
                else if (Global.Data.SystemMonsters.TryGetValue(fly.RoleID, out MonsterData md))
                {
                    return;
                }

                /// Thực hiện quay đối tượng theo hướng chỉ định
                sprite.Direction = (Direction) fly.Direction;

				/// Ngừng StoryBoard đang thực thi
				sprite.StopMove();

				/// Thực hiện hiệu ứng
				SkillManager.SpriteFlyToPosition(sprite, destPos, fly.Duration);
            }
            catch (Exception) { }
        }
        #endregion

        #region Thông báo tốc độ di chuyển, xuất chiêu của đối tượng thay đổi
        /// <summary>
        /// Nhận thông báo từ Server tốc độ di chuyển của đối tượng thay đổi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveTargetMoveSpeedChanged(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_MoveSpeedChanged moveSpeedChanged = DataHelper.BytesToObject<G2C_MoveSpeedChanged>(data, 0, length);

                /// Đối tượng thao tác
                GSprite sprite = null;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == moveSpeedChanged.RoleID)
                {
                    sprite = Global.Data.Leader;
                    /// Nếu có kích hoạt Bug tốc độ di chuyển
                    if (KTGlobal.EnableSpeedCheat)
                    {
                        moveSpeedChanged.MoveSpeed = 200;
                    }
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(moveSpeedChanged.RoleID);
                }

                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == moveSpeedChanged.RoleID)
                {
                    /// Đổi tốc chạy
                    Global.Data.RoleData.MoveSpeed = moveSpeedChanged.MoveSpeed;

                    /// ID pet
                    int petID = Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet;
                    /// Nếu có
                    if (petID != -1)
                    {
                        /// Thông tin pet tương ứng
                        if (Global.Data.SystemPets.TryGetValue(petID, out PetDataMini petData))
                        {
                            petData.MoveSpeed = moveSpeedChanged.MoveSpeed;
                        }
                    }
                }
                /// Nếu là người chơi khác
                else if (Global.Data.OtherRoles.TryGetValue(moveSpeedChanged.RoleID, out RoleData rd))
                {
                    /// Đổi tốc chạy
                    rd.MoveSpeed = moveSpeedChanged.MoveSpeed;

                    /// ID pet
                    int petID = rd.CurrentPetID;
                    /// Nếu có
                    if (petID != -1)
                    {
                        /// Thông tin pet tương ứng
                        if (Global.Data.SystemPets.TryGetValue(petID, out PetDataMini petData))
                        {
                            petData.MoveSpeed = moveSpeedChanged.MoveSpeed;
                        }
                    }
                }
                /// Nếu là quái
                else if (Global.Data.SystemMonsters.TryGetValue(moveSpeedChanged.RoleID, out MonsterData md))
                {
                    md.MoveSpeed = moveSpeedChanged.MoveSpeed;
                }
                /// Nếu là bot
                else if (Global.Data.Bots.TryGetValue(moveSpeedChanged.RoleID, out RoleData bRD))
                {
                    bRD.MoveSpeed = moveSpeedChanged.MoveSpeed;
                }

                /// Nếu có StoryBoard tức đang thực hiện động tác chạy
                if (sprite.IsMoving)
                {
                    KE_NPC_DOING action = sprite.CurrentAction;
                    sprite.RefreshMove(action);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo từ Server tốc độ xuất chiêu của đối tượng thay đổi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveTargetAttackSpeedChanged(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_AttackSpeedChanged attackSpeedChanged = DataHelper.BytesToObject<G2C_AttackSpeedChanged>(data, 0, length);

                /// Đối tượng thao tác
                GSprite sprite = null;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == attackSpeedChanged.RoleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(attackSpeedChanged.RoleID);
                }

                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == attackSpeedChanged.RoleID)
                {
                    Global.Data.RoleData.AttackSpeed = attackSpeedChanged.AttackSpeed;
                    Global.Data.RoleData.CastSpeed = attackSpeedChanged.CastSpeed;
                }
                /// Nếu là người chơi khác
                else if (Global.Data.OtherRoles.TryGetValue(attackSpeedChanged.RoleID, out RoleData rd))
                {
                    rd.AttackSpeed = attackSpeedChanged.AttackSpeed;
                    rd.CastSpeed = attackSpeedChanged.CastSpeed;
                }
                /// Nếu là quái
                else if (Global.Data.SystemMonsters.TryGetValue(attackSpeedChanged.RoleID, out MonsterData md))
                {
                    md.AttackSpeed = attackSpeedChanged.AttackSpeed;
                }
                /// Nếu là bot
                else if (Global.Data.Bots.TryGetValue(attackSpeedChanged.RoleID, out RoleData bRD))
                {
                    bRD.AttackSpeed = attackSpeedChanged.AttackSpeed;
                    bRD.CastSpeed = attackSpeedChanged.CastSpeed;
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Trạng thái
        /// <summary>
        /// Nhận thông báo từ Server động tác của đối tượng thay đổi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveSpriteChangeAction(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_SpriteChangeAction changeAction = DataHelper.BytesToObject<G2C_SpriteChangeAction>(data, 0, length);

                /// Đối tượng thao tác
                GSprite sprite = null;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == changeAction.RoleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(changeAction.RoleID);
                }

                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Nếu là động tác ngồi
                if (changeAction.ActionID == (int) KE_NPC_DOING.do_sit)
                {
                    /// Ngừng Storyboard ngay lập tức
                    sprite.StopMove();

                    IEnumerator ExecuteLater()
                    {
                        yield return new WaitForSeconds(0.2f);

                        /// Thực hiện đổi hướng
                        sprite.Direction = (Direction) changeAction.Direction;
                        /// Thực hiện động tác tương ứng
                        if (sprite.CurrentAction != KE_NPC_DOING.do_sit)
                        {
                            sprite.DoSit();
                        }

                        /// Cập nhật tọa độ đối tượng
                        sprite.Coordinate = new Drawing.Point(changeAction.PosX, changeAction.PosY);
                    }
                    PlayZone.Instance.StartCoroutine(ExecuteLater());
                }
                else if (changeAction.ActionID == (int) KE_NPC_DOING.do_run || changeAction.ActionID == (int) KE_NPC_DOING.do_walk)
                {
                    /// Nếu có Storyboard
                    if (sprite.IsMoving)
                    {
                        /// Nếu động tác đang thực hiện khác động tác thiết lập
                        if (changeAction.ActionID != (int) sprite.CurrentAction)
                        {
                            /// Thực hiện cập nhật động tác
                            sprite.RefreshMove((KE_NPC_DOING) changeAction.ActionID);
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Gửi yêu cầu thay đổi động tác lên Server
        /// </summary>
        /// <param name="action"></param>
        public static void SendLeaderChangeAction(KE_NPC_DOING action)
        {
            try
            {
                if (!Global.Data.PlayGame)
                {
                    return;
                }

                /// Nếu đang khinh công
                if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_jump)
                {
                    return;
                }
                /// Nếu đang ngồi
                else if (Global.Data.Leader.CurrentAction == KE_NPC_DOING.do_sit)
                {
                    return;
                }

                C2G_SpriteChangeAction changeAction = new C2G_SpriteChangeAction()
                {
                    Direction = (int)Global.Data.Leader.Direction,
                    PosX = Global.Data.Leader.PosX,
                    PosY = Global.Data.Leader.PosY,
                    ActionID = (int)action,
                    StartTick = TimeManager.GetCorrectLocalTime(),
                };

                byte[] bytes = DataHelper.ObjectToBytes<C2G_SpriteChangeAction>(changeAction);
                GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int)(TCPGameServerCmds.CMD_KT_C2G_CHANGEACTION)));
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo từ Server trạng thái ngũ hành của đối tượng thay đổi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveSpriteSeriesState(int cmdID, byte[] data, int length)
        {
            try
            {
                G2C_SpriteSeriesState seriesState = DataHelper.BytesToObject<G2C_SpriteSeriesState>(data, 0, length);

                /// Đối tượng thao tác
                GSprite sprite = null;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == seriesState.RoleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(seriesState.RoleID);
                }

                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Nếu đối tượng đã chết
                if (sprite.IsDeath)
                {
                    return;
                }
                /// Nếu trạng thái ngũ hành không phù hợp
                else if (seriesState.SeriesID < (int)KE_STATE.emSTATE_BEGIN || seriesState.SeriesID >= (int)KE_STATE.emSTATE_ALLNUM)
                {
                    return;
                }

                /// Thực hiện trạng thái ngũ hành cho đối tượng
                if (seriesState.Type == 0)
                {
                    sprite.RemoveSeriesState((KE_STATE)seriesState.SeriesID);
                }
                else if (seriesState.Type == 1)
                {
                    sprite.AddSeriesState((KE_STATE)seriesState.SeriesID, seriesState.Time);

                    /// Nếu là Leader
                    if (Global.Data.Leader == sprite)
                    {
                        /// Nếu không thể thực hiện Logic
                        if (!sprite.CanDoLogic)
                        {
                            /// Ngừng gọi pet
                            KTAutoFightManager.Instance.IsCallingPet = false;
                        }
                    }

                    /// Nếu là các trạng thái ngũ hành đặc biệt cần biểu diễn
                    switch (seriesState.SeriesID)
                    {
                        case (int) KE_STATE.emSTATE_DRAG:
                        case (int) KE_STATE.emSTATE_KNOCK:
                        {
                            if (sprite.ComponentCharacter != null)
                            {
                                sprite.DoHurt(seriesState.Time);
                            }
                            else if (sprite.ComponentMonster != null)
                            {
                                sprite.DoHurt(seriesState.Time);
                            }

                            /// Thực hiện hiệu ứng
                            SkillManager.SpriteBlinkToPosition(sprite, new Vector2(seriesState.DragPosX, seriesState.DragPosY), seriesState.Time, true);
                            
                            break;
                        }
                    }
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Trap
        /// <summary>
        /// Nhận thông báo hiển thị bẫy từ Server
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveNewTrap(int cmdID, byte[] data, int length)
        {
            try
            {
                if (Global.Data.Leader == null)
                {
                    return;
                }

                TrapRole trap = DataHelper.BytesToObject<TrapRole>(data, 0, length);

                /// Đối tượng thao tác
                GSprite sprite = null;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == trap.CasterID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(trap.CasterID);
                }

                /// Thực hiện tạo bẫy tại vị trí tương ứng
                SkillManager.CreateBulletStatic(sprite, 0f, trap.ID, trap.ResID, trap.Pos, trap.LifeTime, true, true);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận thông báo xóa bẫy từ Server
        /// </summary>
        /// <param name="trapID"></param>
        public static void ReceiveDelTrap(int trapID)
        {
            try
            {
                if (Global.Data.Leader == null)
                {
                    return;
                }

                SkillManager.DestroyBulletImmediately(trapID);
            }
            catch (Exception) { }
        }
        #endregion

        #region Danh sách kỹ năng tu luyện
        /// <summary>
        /// Nhận thông báo danh sách kỹ năng tu luyện
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveExpSkillList(int cmdID, byte[] data, int length)
        {
            /// Dữ liệu
            List<ExpSkillData> _data = DataHelper.BytesToObject<List<ExpSkillData>>(data, 0, length);
            /// Nếu không có
            if (_data == null)
            {
                /// Tạo mới
                _data = new List<ExpSkillData>();
            }

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UISetExpSkill == null)
            {
                /// Mở khung
                PlayZone.Instance.OpenUISetExpSkill(_data);
            }
            /// Nếu đã mở khung
            else
            {
                /// Thiết lập dữ liệu
                PlayZone.Instance.UISetExpSkill.Data = _data;
            }
        }

        /// <summary>
        /// Gửi yêu cầu truy vấn danh sách kỹ năng tu luyện
        /// </summary>
        public static void SendGetExpSkillList()
        {
            string strcmd = "";
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, strcmd, (int) (TCPGameServerCmds.CMD_KT_GET_EXPSKILLS)));
        }
        
        /// <summary>
        /// Gửi yêu cầu thiết lập kỹ năng tu luyện
        /// </summary>
        /// <param name="skillID"></param>
        public static void SendSetExpSkill(int skillID)
        {
            string strcmd = string.Format("{0}", skillID);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, strcmd, (int) (TCPGameServerCmds.CMD_KT_SET_EXPSKILL)));
        }

        /// <summary>
        /// Nhận phản hồi thiết lập kỹ năng tu luyện
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveSetExpSkillList(string[] fields)
        {
            /// ID kỹ năng
            int skillID = int.Parse(fields[0]);
            /// Nếu đang mở khung
            if (PlayZone.Instance.UISetExpSkill != null)
            {
                /// Cập nhật
                PlayZone.Instance.UISetExpSkill.UpdateCurrentExpSkill(skillID);
            }
        }
        #endregion
    }
}
