using System;
using System.Collections.Generic;
using FS.Drawing;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using Server.Data;
using Server.Tools;
using UnityEngine;
using FS.VLTK.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using FS.VLTK;
using FS.VLTK.Control.Component;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý tầng NETWORK
    /// </summary>
    public partial class GScene
    {
        #region Sự kiện
        /// <summary>
        /// Sự kiện PlayGame
        /// </summary>
        /// <param name="roleID"></param>
        public void ServerPlayGame(int roleID)
        {
            GameInstance.Game.CurrentSession.PlayGame = true;
        }


        /// <summary>
        /// Sự kiện ngừng chơi Game
        /// </summary>
        /// <param name="roleID"></param>
        public static void ServerStopGame()
        {
            if (null != GameInstance.Game && null != GameInstance.Game.CurrentSession)
            {
                GameInstance.Game.CurrentSession.PlayGame = false;
            }
        }

        /// <summary>
        /// Cập nhật dữ liệu máu của đối tượng
        /// </summary>
        /// <param name="lifeChangeData"></param>
        public void ServerUpdateRoleData(SpriteLifeChangeData lifeChangeData)
        {
            /// Toác
            if (lifeChangeData == null)
            {
                return;
            }
            else if (Global.Data == null || Global.Data.RoleData == null)
            {
                return;
            }

            /// đồng bộ dữ liệu đối tượng
            this.ServerUpdateRoleData(lifeChangeData.RoleID, lifeChangeData.MaxHP, lifeChangeData.MaxMP, lifeChangeData.MaxStamina, lifeChangeData.HP, lifeChangeData.MP, lifeChangeData.Stamina);
            if (lifeChangeData.RoleID == Global.Data.RoleData.RoleID)
            {
                PlayZone.Instance.NotifyLeaderRoleFace(-1);
                if (null != PlayZone.Instance.UIRoleInfo)
                {
                    PlayZone.Instance.UIRoleInfo.UpdateRoleData();
                }
            }
            else
            {
                PlayZone.Instance.NotifyLeaderRoleFace(lifeChangeData.RoleID);
            }
        }

        /// <summary>
        /// Nhận gói tin từ Server thông báo vị trí Pet thay đổi
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public void ServerPetChangePos(int petID, int posX, int posY)
        {
            try
            {
                /// Đối tượng di chuyển
                GSprite sprite = KTGlobal.FindSpriteByID(petID);
                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Kiểm tra 2 vị trí hiện tại và ở Server truyền về, nếu xa quá thì giật ra luôn
                if (Vector2.Distance(new Vector2(posX, posY), sprite.PositionInVector2) >= 700)
                {
                    /// Set lại vị trí xuất phát
                    sprite.PosX = posX;
                    sprite.PosY = posY;
                }

                /// Dừng và xóa StoryBoard cũ của đối tượng
                sprite.StopMove();

                /// Vị trí đầu cuối
                Vector2 fromVector = sprite.PositionInVector2;
                Vector2 toVector = new Vector2(posX, posY);

                /// Thực hiện tìm đường
                List<Vector2> paths = Global.Data.GameScene.FindPath(sprite, ref fromVector, ref toVector);

                /// Nếu không có đường đi thì bỏ qua
                if (paths == null || paths.Count < 1)
                {
                    return;
                }

                /// Hàng đợi danh sách các điểm trên đường đi
                Queue<Vector2> queue = new Queue<Vector2>();
                /// Nạp tất cả các điểm vừa tìm được vào hàng đợi
                foreach (Vector2 pos in paths)
                {
                    queue.Enqueue(pos);
                }

                /// Chuyển về dạng string
                string pathString = "";
                while (queue.Count > 0)
                {
                    Vector2 node = queue.Dequeue();
                    pathString += "|" + string.Format("{0}_{1}", (int) node.x, (int) node.y);
                }
                pathString = pathString.Substring(1);

                /// Thêm StoryBoard mới vào danh sách
                KTStoryBoard.Instance.AddOrUpdate(sprite, pathString);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận gói tin từ Server thông báo đối tượng di chuyển
        /// <para>Nếu pathString rỗng thì tự tìm đường ở Client</para>
        /// <para>Nếu pathString khác rỗng thì chạy theo đường đi tương ứng</para>
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="fromX"></param>
        /// <param name="fromY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <param name="pathString"></param>
        /// <param name="action"></param>
        public void ServerLinearMove(int roleID, int fromX, int fromY, int toX, int toY, string pathString, int action)
        {
            try
            {
                /// Đối tượng di chuyển
                GSprite sprite;

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == roleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    /// Tìm đối tượng
                    sprite = KTGlobal.FindSpriteByID(roleID);
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

                /// Kiểm tra 2 vị trí hiện tại và ở Server truyền về, nếu xa quá thì giật ra luôn
                if (Vector2.Distance(new Vector2(fromX, fromY), sprite.PositionInVector2) >= 500)
                {
                    /// Set lại vị trí xuất phát
                    sprite.PosX = fromX;
                    sprite.PosY = fromY;
                }

                /// Dừng và xóa StoryBoard cũ của đối tượng
                sprite.StopMove();

                /// Nếu đường đi rỗng
                if (string.IsNullOrEmpty(pathString))
                {
                    /// Thêm StoryBoard mới vào danh sách
                    KTStoryBoard.Instance.Add(sprite, new Vector2(fromX, fromY), new Vector2(toX, toY), null, (FS.VLTK.Entities.Enum.KE_NPC_DOING) action);
                }
                /// Nếu có đường đi sẵn
                else
                {
                    /// Thêm StoryBoard mới vào danh sách
                    KTStoryBoard.Instance.Add(sprite, pathString, null, (FS.VLTK.Entities.Enum.KE_NPC_DOING) action);
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Đối tượng ngừng di chuyển
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="direction"></param>
        public void ServerStopMove(int roleID, int posX, int posY, int moveSpeed, int direction)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                /// Tìm đối tượng tương ứng
                GSprite sprite = KTGlobal.FindSpriteByID(roleID);
                /// Nếu không tìm thấy đối tượng
                if (sprite == null)
                {
                    return;
                }

                /// Nếu là bản thân
                if (Global.Data.RoleData.RoleID == roleID)
                {
                    /// Nếu có kích hoạt Bug tốc độ di chuyển
                    if (KTGlobal.EnableSpeedCheat)
                    {
                        moveSpeed = 200;
                    }

                    Global.Data.RoleData.MoveSpeed = moveSpeed;
                }
                /// Nếu là người chơi khác
                else if (Global.Data.OtherRoles.TryGetValue(roleID, out RoleData rd))
                {
                    rd.MoveSpeed = moveSpeed;
                }
                /// Nếu là quái
                else if (Global.Data.SystemMonsters.TryGetValue(roleID, out MonsterData md))
                {
                    md.MoveSpeed = moveSpeed;
                }
                /// Nếu là xe tiêu
                else if (Global.Data.TraderCarriages.TryGetValue(roleID, out TraderCarriageData td))
                {
                    td.MoveSpeed = moveSpeed;
                }

                /// Xóa StoryBoard tương ứng của đối tượng
                sprite.StopMove();

                /// Vị trí hiện tại
                Vector2 currentPos = sprite.PositionInVector2;
                /// Vị trí từ GS
                Vector2 gsPos = new Vector2(posX, posY);

                /// Nếu vị trí hiện tại khác so với GS
                if (Vector2.Distance(currentPos, gsPos) > 20 && (sprite.RoleData != null || sprite.TraderCarriageData != null))
                {
                    sprite.DoRun();
                    KTStoryBoard.Instance.Add(sprite, sprite.PositionInVector2, gsPos, () => {
                        /// Cập nhật hướng cho đối tượng
                        sprite.Direction = (VLTK.Entities.Enum.Direction) direction;
                    });
                }
                else
                {
                    /// Nếu là người chơi
                    if (sprite.RoleData != null || sprite.TraderCarriageData != null)
                    {
                        /// Cập nhật vị trí cho đối tượng
                        sprite.Coordinate = new Point(posX, posY);
                        /// Cập nhật hướng cho đối tượng
                        sprite.Direction = (VLTK.Entities.Enum.Direction) direction;
                    }
                    /// Thực hiện động tác đứng
                    sprite.DoStand();
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);

            }
        }


        /// <summary>
        /// Đối tượng tái sinh
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="series"></param>
        /// <param name="hp"></param>
        /// <param name="mp"></param>
        /// <param name="stamina"></param>
        public void ServerRealive(int roleID, int x, int y, int direction, int series, int hp, int mp, int stamina)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                /// Đối tượng trên bản đồ
                GSprite sprite = null;
                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == roleID)
                {
                    sprite = Global.Data.Leader;
                }
                else
                {
                    sprite = KTGlobal.FindSpriteByID(roleID);
                }

                /// Nếu tìm thấy đối tượng
                if (null != sprite)
                {
                    /// Ngừng di chuyển
                    sprite.StopMove();
                }

                /// Nếu là Leader
                if (roleID == Global.Data.RoleData.RoleID)
                {
                    /// Thiết lập giá trị máu, mana và thể lực
                    Global.Data.RoleData.CurrentHP = hp;
                    Global.Data.RoleData.CurrentMP = mp;
                    Global.Data.RoleData.CurrentStamina = stamina;

                    /// Thực hiện tải xuống Leader
                    this.LoadLeader(x, y, direction);

                    /// Thực hiện sống lại
                    sprite.DoRevive();
                    return;
                }

                /// Người chơi khác
                RoleData roleData = null;
                /// Quái
                MonsterData monsterData = null;

                /// Nếu là người chơi khác
                if (Global.Data.OtherRoles.ContainsKey(roleID))
                {
                    roleData = Global.Data.OtherRoles[roleID];

                    /// Cập nhật máu
                    roleData.CurrentHP = hp;

                    //KTDebug.LogError("Revive => " + Global.Data.OtherRoles[roleID].RoleName);

                    /// Tải dữ liệu người chơi khác
                    this.ToLoadOtherRole(roleData, x, y, direction);

					/// Thực hiện sống lại
					sprite.DoRevive();
				}
                else if (Global.Data.SystemMonsters.ContainsKey(roleID))
                {
                    monsterData = Global.Data.SystemMonsters[roleID];

                    /// Cập nhật máu
                    monsterData.HP = hp;

                    /// Cập nhật ngũ hành
                    monsterData.Elemental = series;

                    /// Tải dữ liệu quái
                    this.ToLoadMonster(monsterData, x, y, direction);
                }

            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }


        /// <summary>
        /// Thực hiện biểu diễn kết quả kỹ năng
        /// </summary>
        /// <param name="skillResult"></param>
        private void ProcessSkillResult(SkillResult skillResult)
        {
            if (skillResult == null)
            {
                return;
            }

            /// Toác
            skillResult.TargetCurrentHP = Math.Max(0, skillResult.TargetCurrentHP);

            /// Nếu là kỹ năng gây sát thương
            if (skillResult.Type != (int) VLTK.Entities.Enum.SkillResult.None)
            {
                /// Nếu đối tượng xuất chiêu là Leader
                if (skillResult.CasterID == Global.Data.RoleData.RoleID)
                {
                    /// Nếu đối tượng xuất chiêu là Pet của bản thân
                    if (skillResult.PetID != -1 && skillResult.PetID == Global.Data.RoleData.CurrentPetID + (int) VLTK.Entities.Enum.ObjectBaseID.Pet)
                    {
                        /// Đối tượng xuất chiêu
                        GSprite caster = KTGlobal.FindSpriteByID(skillResult.PetID);
                        /// Mục tiêu
                        GSprite target = KTGlobal.FindSpriteByID(skillResult.TargetID);

                        /// Nếu tìm thấy mục tiêu
                        if (target != null && caster != null)
                        {
                            /// Nếu mục tiêu là quái
                            if (target.SpriteType == GSpriteTypes.Monster)
                            {
                                if (Global.Data.SystemMonsters.TryGetValue(skillResult.TargetID, out MonsterData md))
                                {
                                    md.HP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }
                            /// Nếu mục tiêu là người chơi khác
                            else if (target.SpriteType == GSpriteTypes.Other)
                            {
                                if (Global.Data.OtherRoles.TryGetValue(skillResult.TargetID, out RoleData rd))
                                {
                                    rd.CurrentHP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }
                            /// Néu mục tiêu là pet
                            else if (target.SpriteType == GSpriteTypes.Pet)
                            {
                                if (Global.Data.SystemPets.TryGetValue(skillResult.TargetID, out PetDataMini petData))
                                {
                                    petData.HP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }

                            /// Cập nhật lên khung máu nếu có
                            this.RefreshSpriteLife(target);

                            /// Nếu mục tiêu tử vong
                            if (target.HP <= 0)
                            {
                                /// Đánh dấu đối tượng đã chết
                                target.DoDeath();
                            }

                            /// Hiển thị Text kết quả kỹ năng
                            KTGlobal.ShowSkillResultText(caster, target, skillResult.Type, skillResult.Damage);
                        }
                    }
                    /// Nếu đối tượng ra chiêu là bản thân
                    else if (skillResult.PetID == -1)
                    {
                        /// Đối tượng xuất chiêu
                        GSprite caster = Global.Data.Leader;
                        /// Mục tiêu
                        GSprite target = KTGlobal.FindSpriteByID(skillResult.TargetID);

                        /// Nếu tìm thấy mục tiêu
                        if (target != null)
                        {
                            /// Nếu mục tiêu là quái
                            if (target.SpriteType == GSpriteTypes.Monster)
                            {
                                if (Global.Data.SystemMonsters.TryGetValue(skillResult.TargetID, out MonsterData md))
                                {
                                    md.HP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }
                            /// Nếu mục tiêu là người chơi khác
                            else if (target.SpriteType == GSpriteTypes.Other)
                            {
                                if (Global.Data.OtherRoles.TryGetValue(skillResult.TargetID, out RoleData rd))
                                {
                                    rd.CurrentHP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }
                            /// Néu mục tiêu là pet
                            else if (target.SpriteType == GSpriteTypes.Pet)
                            {
                                if (Global.Data.SystemPets.TryGetValue(skillResult.TargetID, out PetDataMini petData))
                                {
                                    petData.HP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }

                            /// Cập nhật lên khung máu nếu có
                            this.RefreshSpriteLife(target);

                            /// Nếu mục tiêu tử vong
                            if (target.HP <= 0)
                            {
                                /// Đánh dấu đối tượng đã chết
                                target.DoDeath();
                            }

                            /// Hiển thị Text kết quả kỹ năng
                            KTGlobal.ShowSkillResultText(caster, target, skillResult.Type, skillResult.Damage);
                        }
                    }
                }
                /// Nếu đối tượng xuất chiêu không phải bản thân
                else
                {
                    /// Đối tượng xuất chiêu
                    GSprite caster = KTGlobal.FindSpriteByID(skillResult.CasterID);
                    /// Mục tiêu
                    GSprite target;

                    /// Nếu mục tiêu là bản thân
                    if (skillResult.TargetID == Global.Data.RoleData.RoleID)
                    {
                        /// Nếu mục tiêu là Pet của bản thân
                        if (skillResult.PetID != -1 && skillResult.PetID == Global.Data.RoleData.CurrentPetID + (int) VLTK.Entities.Enum.ObjectBaseID.Pet)
                        {
                            /// Tìm mục tiêu tương ứng
                            target = KTGlobal.FindSpriteByID(skillResult.PetID);
                            /// Nếu tìm thấy
                            if (target != null)
                            {
                                if (Global.Data.SystemPets.TryGetValue(skillResult.PetID, out PetDataMini petData))
                                {
                                    petData.HP = skillResult.TargetCurrentHP;
                                }

                                /// Cập nhật hiển thị lên khung máu nếu có
                                this.RefreshSpriteLife(target);

                                /// Nếu mục tiêu tử vong
                                if (target.HP <= 0)
                                {
                                    /// Đánh dấu đối tượng đã chết
                                    target.DoDeath();
                                }

                                /// Hiển thị Text kết quả kỹ năng
                                KTGlobal.ShowSkillResultText(caster, target, skillResult.Type, skillResult.Damage);
                            }
                        }
                        /// Nếu mục tiêu là xe tiêu
                        else if (skillResult.TraderCarriageID != -1)
                        {
                            /// Tìm mục tiêu tương ứng
                            target = KTGlobal.FindSpriteByID(skillResult.TraderCarriageID);
                            /// Nếu tìm thấy
                            if (target != null)
                            {
                                /// Tìm thông tin xe tiêu tương ứng
                                if (Global.Data.TraderCarriages.TryGetValue(skillResult.TraderCarriageID, out TraderCarriageData tcData))
                                {
                                    tcData.HP = skillResult.TargetCurrentHP;
                                }

                                /// Cập nhật hiển thị lên khung máu nếu có
                                this.RefreshSpriteLife(target);

                                /// Nếu mục tiêu tử vong
                                if (target.HP <= 0)
                                {
                                    /// Đánh dấu đối tượng đã chết
                                    target.DoDeath();
                                }

                                /// Hiển thị Text kết quả kỹ năng
                                KTGlobal.ShowSkillResultText(caster, target, skillResult.Type, skillResult.Damage);
                            }
                        }
                        /// Nếu mục tiêu là bản thân
                        else if (skillResult.PetID == -1 && skillResult.TraderCarriageID == -1)
                        {
                            /// Tìm mục tiêu tương ứng
                            target = Global.Data.Leader;

                            /// Cập nhật máu cho bản thân
                            Global.Data.RoleData.CurrentHP = skillResult.TargetCurrentHP;

                            /// Cập nhật hiển thị lên khung máu nếu có
                            this.RefreshSpriteLife(target);

                            /// Cập nhật hiển thị lên khung khí nếu có
                            this.RefreshLeaderMagic();

                            /// Nếu mục tiêu tử vong
                            if (target.HP <= 0)
                            {
                                /// Đánh dấu đối tượng đã chết
                                target.DoDeath();
                            }

                            /// Hiển thị Text kết quả kỹ năng
                            KTGlobal.ShowSkillResultText(caster, target, skillResult.Type, skillResult.Damage);

                            ///// Nếu có damage
                            //if (skillResult.Damage > 0)
                            //{
                            //    /// Ngừng gọi pet
                            //    KTAutoFightManager.Instance.IsCallingPet = false;
                            //}
                        }
                    }
                    /// Nếu mục tiêu không phải bản thân
                    else
                    {
                        /// Tìm mục tiêu tương ứng
                        target = KTGlobal.FindSpriteByID(skillResult.TargetID);

                        /// Nếu tìm thấy mục tiêu
                        if (target != null)
                        {
                            /// Nếu mục tiêu là quái
                            if (target.SpriteType == GSpriteTypes.Monster)
                            {
                                if (Global.Data.SystemMonsters.TryGetValue(skillResult.TargetID, out MonsterData md))
                                {
                                    md.HP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }
                            /// Nếu mục tiêu là người chơi khác
                            else if (target.SpriteType == GSpriteTypes.Other)
                            {
                                if (Global.Data.OtherRoles.TryGetValue(skillResult.TargetID, out RoleData rd))
                                {
                                    rd.CurrentHP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }
                            /// Néu mục tiêu là pet
                            else if (target.SpriteType == GSpriteTypes.Pet)
                            {
                                if (Global.Data.SystemPets.TryGetValue(skillResult.TargetID, out PetDataMini petData))
                                {
                                    petData.HP = skillResult.TargetCurrentHP;
                                    /// Thông báo hiển thị khung mặt đối tượng
                                    PlayZone.Instance.NotifyRoleFace(target.RoleID, Global.Data.RoleData.RoleID);
                                }
                            }

                            /// Cập nhật hiển thị lên khung máu nếu có
                            this.RefreshSpriteLife(target);

                            /// Nếu mục tiêu tử vong
                            if (target.HP <= 0)
                            {
                                /// Đánh dấu đối tượng đã chết
                                target.DoDeath();
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Nhận thông báo danh sách kết quả kỹ năng gửi về từ Server
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public void ReceiveSkillResults(int cmdID, byte[] data, int length)
        {
            try
            {
                /// Dữ liêu gửi về
                List<SkillResult> skillResults = DataHelper.BytesToObject<List<SkillResult>>(data, 0, length);
                if (skillResults == null)
                {
                    return;
                }

                foreach (SkillResult skillResult in skillResults)
                {
                    this.ProcessSkillResult(skillResult);
                }
            }
            catch (Exception) { }
        }


        /// <summary>
        /// Nhận thông báo kết quả kỹ năng gửi về từ Server
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public void ReceiveSkillResult(int cmdID, byte[] data, int length)
        {
            try
            {
                /// Dữ liêu gửi về
                SkillResult skillResult = DataHelper.BytesToObject<SkillResult>(data, 0, length);
                if (skillResult == null)
                {
                    return;
                }
                this.ProcessSkillResult(skillResult);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Sự kiện máy chủ phản hồi chỉ số máu, khí và thể lực của nhân vật
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="lifeV"></param>
        /// <param name="magicV"></param>
        /// <param name="staminaV"></param>
        public void ServerRelife(int roleID, int x, int y, int direction, int lifeV, int magicV, int staminaV)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                /// Toác
                lifeV = Math.Max(0, lifeV);
                magicV = Math.Max(0, magicV);
                staminaV = Math.Max(0, staminaV);

                /// Nếu là Leader
                if (roleID == Global.Data.RoleData.RoleID)
                {
                    Global.Data.RoleData.CurrentHP = lifeV;
                    Global.Data.RoleData.CurrentMP = magicV;
                    Global.Data.RoleData.CurrentStamina = staminaV;
                    if (this.Leader != null)
                    {
                        /// Nếu mục tiêu tử vong
                        if (this.Leader.HP <= 0)
                        {
                            /// Đánh dấu đối tượng đã chết
                            this.Leader.DoDeath();
                        }

                        this.RefreshSpriteLife(this.Leader);
                        this.RefreshLeaderMagic();
                    }
                    return;
                }

                /// Nếu là người chơi khác
                if (Global.Data.OtherRoles.ContainsKey(roleID))
                {
                    RoleData roleData = Global.Data.OtherRoles[roleID];
                    roleData.CurrentHP = lifeV;
                }
                /// Nếu là quái
                else if (Global.Data.SystemMonsters.ContainsKey(roleID))
                {
                    MonsterData monsterData = Global.Data.SystemMonsters[roleID];
                    monsterData.HP = lifeV;
                }
                /// Nếu là pet
                else if (Global.Data.SystemPets.ContainsKey(roleID))
                {
                    PetDataMini petData = Global.Data.SystemPets[roleID];
                    petData.HP = lifeV;
                }
                /// Nếu là xe tiêu
                else if (Global.Data.TraderCarriages.ContainsKey(roleID))
                {
                    TraderCarriageData tdData = Global.Data.TraderCarriages[roleID];
                    tdData.HP = lifeV;
                }

                /// Đối tượng tương ứng
                GSprite sprite = this.FindSprite(roleID);
                /// Nếu tìm thấy
                if (sprite != null)
                {
                    /// Nếu mục tiêu tử vong
                    if (sprite.HP <= 0)
                    {
                        /// Đánh dấu đối tượng đã chết
                        sprite.DoDeath();
                    }

                    if (sprite.HP != lifeV)
                    {
                        this.RefreshSpriteLife(sprite);
                    }
                }

                /// Nếu có dữ liệu
                if (Global.Data.RoleData != null)
                {
                    if (Global.Data.RoleData.RoleID == roleID)
                    {
                        if (PlayZone.Instance.UIRoleInfo != null)
                        {
                            PlayZone.Instance.UIRoleInfo.UpdateRoleData();
                        }
                    }
                    else if (Global.Data.RoleData.CurrentPetID != -1 && roleID - (int) VLTK.Entities.Enum.ObjectBaseID.Pet == Global.Data.RoleData.CurrentPetID)
                    {
                        if (PlayZone.Instance.UIPet != null)
                        {
                            PlayZone.Instance.UIPet.UpdateHP(Global.Data.RoleData.CurrentPetID, sprite.HP, sprite.HPMax);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Xóa đối tượng tương ứng khỏi bản đồ
        /// </summary>
        /// <param name="roleID"></param>
        public void ServerRoleLeave(int roleID)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                GSprite sprite = this.FindSprite(roleID);
                if (null != sprite)
                {
                    /// Đánh dấu có phải người không
                    bool isRole = false;
                    /// Nếu là người
                    if (Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out _))
                    {
                        Global.Data.OtherRoles.Remove(sprite.RoleID);
                        /// Đánh dấu là người
                        isRole = true;
                    }
                    else if (Global.Data.SystemMonsters.TryGetValue(sprite.RoleID, out _))
                    {
                        Global.Data.SystemMonsters.Remove(sprite.RoleID);
                    }

                    /// Nếu không phải người
                    if (!isRole)
                    {
                        /// Nếu đang thực hiện động tác chết thì thôi
                        if (sprite.IsDeath || sprite.HP <= 0)
                        {
                            return;
                        }
                    }

                    /// Thực hiện xóa đối tượng
                    KTGlobal.RemoveObject(sprite, true);
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);

            }
        }

        /// <summary>
        /// Thay đổi trang bị
        /// </summary>
        /// <param name="roleID"></param>
        public void ServerChangeCode(int roleID)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                GSprite sprite = KTGlobal.FindSpriteByID(roleID);
                if (sprite != null)
                {
                    this.ChangeBodyCode(sprite);
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Thêm vật phẩm rơi ở Map
        /// </summary>
        /// <param name="gpData"></param>
        public void ServerNewGoodsPack(NewGoodsPackData gpData)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Vật phẩm tương ứng
                ItemData itemData = null;
                /// Nếu vật phẩm không tồn tại
                if (!Loader.Items.TryGetValue(gpData.GoodsID, out itemData))
                {
                    return;
                }

                /// Tải xuống vật phẩm rơi ở Map tương ứng
                this.ToLoadGoodsPack(gpData);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);

            }
        }

        /// <summary>
        /// Xóa vật phẩm rơi ở Map tương ứng
        /// </summary>
        /// <param name="autoID"></param>
        public void ServerDelGoodsPack(int autoID)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Thực hiện xóa vật phẩm tương ứng
                this.DelGoodsPack(autoID);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);

            }
        }

        /// <summary>
        /// Nhận thông tin có điểm thu thập mới
        /// </summary>
        /// <param name="growPointData"></param>
        public void ServerNewGrowPoint(GrowPointObject growPointData)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Tải dữ liệu điểm thu thập
                this.ToLoadGrowPoint(growPointData);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận thông tin xóa điểm thu thập
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="id"></param>
        public void ServerDelGrowPoint(int id)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Xóa điểm thu thập
                this.DelGrowPoints(id);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận thông tin có khu vực động mới
        /// </summary>
        /// <param name="dynamicAreaData"></param>
        public void ServerNewDynamicArea(DynamicArea dynamicAreaData)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Tải dữ liệu điểm thu thập
                this.ToLoadDynamicArea(dynamicAreaData);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận thông tin xóa khu vực động
        /// </summary>
        /// <param name="id"></param>
        public void ServerDelDynamicArea(int id)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Xóa khu vực động
                this.DelDynamicAreas(id);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận thông tin có NPC mới
        /// </summary>
        /// <param name="npc"></param>
        public void ServerNewNPC(NPCRole npc)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                this.AddNewNPC(npc);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Xóa NPC
        /// </summary>
        /// <param name="npcID"></param>
        public void ServerDelNPC(int npcID)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                this.DelNpc(npcID);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận thông tin có pet mới
        /// </summary>
        /// <param name="petData"></param>
        public void ServerNewPet(PetDataMini petData)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Tải dữ liệu pet
                this.ToLoadPet(petData);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận thông tin xóa pet
        /// </summary>
        /// <param name="id"></param>
        public void ServerDelPet(int id)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Xóa pet
                this.DelPet(id);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận thông tin có xe tiêu mới
        /// </summary>
        /// <param name="data"></param>
        public void ServerNewTraderCarriage(TraderCarriageData data)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Tải dữ liệu xe tiêu
                this.ToLoadTraderCarriage(data);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Nhận thông tin xóa xe tiêu
        /// </summary>
        /// <param name="id"></param>
        public void ServerDelTraderCarriage(int id)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Xóa xe tiêu
                this.DelTraderCarriage(id);
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Thay đổi trạng thái nhiệm vụ của NPC
        /// </summary>
        /// <param name="npcResID"></param>
        /// <param name="state"></param>
        public void ServerUpdateNPCTaskState(int npcResID, int state)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                /// Trạng thái tương ứng
                NPCTaskState npcTaskState = KTGlobal.FindTaskByNPCResID(npcResID);
                if (npcTaskState != null)
                {
                    npcTaskState.TaskState = state;
                }

                /// Danh sách đối tượng NPC tương ứng
                List<GSprite> sprites = KTGlobal.FindSpritesByResID(npcResID);
                if (null == sprites)
                {
                    return;
                }

                /// Duyệt danh sách NPC tương ứng tìm được
                foreach (GSprite sprite in sprites)
                {
                    this.UpdateNPCTaskState(sprite, (NPCTaskStates) npcTaskState.TaskState);
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);

            }
        }

        /// <summary>
        /// Thông báo đối tượng thay đổi vị trí
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="animation"></param>
        public void ServerChangePos(int roleID, int x, int y, int direction)
        {
            try
            {
                GSprite sprite;
                /// Nếu là Leader
                if (roleID == Global.Data.RoleData.RoleID)
                {
                    sprite = Global.Data.Leader;
                }
                /// Nếu không phải Leader
                else
                {
                    sprite = KTGlobal.FindSpriteByID(roleID);
                }

                /// Nếu không tồn tại thì thôi
                if (sprite == null)
				{
                    return;
				}

                /// Nếu đang có luồng bay thì ngừng lại
                if (sprite.FlyingCoroutine != null)
                {
                    PlayZone.Instance.StopCoroutine(sprite.FlyingCoroutine);
                    sprite.FlyingCoroutine = null;
                }

                /// Ngừng StoryBoard
                sprite.StopMove();
                /// Nếu là Leader
                if (roleID == Global.Data.RoleData.RoleID)
                {
                    /// Cập nhật thời điểm buộc đổi vị trí
                    KTLeaderMovingManager.LastForceChangePosTick = KTGlobal.GetCurrentTimeMilis();
                    /// Ngừng đuổi mục tiêu
                    KTLeaderMovingManager.StopChasingTarget();
                    /// Đánh dấu mục tiêu trước đó đang đợi để thực thi kỹ năng
                    SkillManager.LastWaitingTarget = null;
                }

                /// Thiết lập vị trí cho đối tượng
                sprite.Coordinate = new Point(x, y);
                /// Thiết lập hướng cho đối tượng
                sprite.Direction = (VLTK.Entities.Enum.Direction) direction;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Server gửi sự kiện thay đổi Sinh lực, nội lực, thể lực của đối tượng
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="maxHP"></param>
        /// <param name="maxMP"></param>
        /// <param name="maxStamina"></param>
        /// <param name="hp"></param>
        /// <param name="mp"></param>
        /// <param name="stamina"></param>
        public void ServerUpdateRoleData(int roleID, int maxHP, int maxMP, int maxStamina, int hp, int mp, int stamina)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                RoleData rd = null;
                MonsterData md = null;

                /// Nếu là Leader
                if (roleID == Global.Data.RoleData.RoleID)
                {
                    Global.Data.RoleData.MaxHP = maxHP;
                    Global.Data.RoleData.MaxMP = maxMP;
                    Global.Data.RoleData.MaxStamina = maxStamina;
                    Global.Data.RoleData.CurrentHP = hp;
                    Global.Data.RoleData.CurrentMP = mp;
                    Global.Data.RoleData.CurrentStamina = stamina;
                    if (null != this.Leader)
                    {
                        this.RefreshSpriteLife(this.Leader);
                        this.RefreshLeaderMagic();
                    }
                    return;
                }
                /// Nếu là người chơi khác
                else if (Global.Data.OtherRoles.ContainsKey(roleID))
                {
                    Global.Data.OtherRoles[roleID].MaxHP = maxHP;
                    Global.Data.OtherRoles[roleID].CurrentHP = hp;
                }
                /// Nếu là quái
                else if (Global.Data.SystemMonsters.ContainsKey(roleID))
                {
                    Global.Data.SystemMonsters[roleID].MaxHP = maxHP;
                    Global.Data.SystemMonsters[roleID].HP = hp;
                }
                /// Nếu là BOT
                else if (Global.Data.Bots.ContainsKey(roleID))
                {
                    Global.Data.Bots[roleID].MaxHP = maxHP;
                    Global.Data.Bots[roleID].CurrentHP = hp;
                }
                /// Nếu là Pet
                else if (Global.Data.SystemPets.ContainsKey(roleID))
                {
                    Global.Data.SystemPets[roleID].MaxHP = maxHP;
                    Global.Data.SystemPets[roleID].HP = hp;
                }

                /// Tìm đối tượng tương ứng
                GSprite sprite = KTGlobal.FindSpriteByID(roleID);
                if (null != sprite)
                {
                    /// Nếu đối tượng đã chết thì bỏ qua
                    if (sprite.IsDeath)
                    {
                        return;
                    }

                    /// Cập nhật hiển thị máu lên khung
                    this.RefreshSpriteLife(sprite);
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Thay đổi trạng thái PK
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="pkMode"></param>
        /// <param name="camp"></param>
        public void ServerChangePKMode(int roleID, int pkMode, int camp)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                /// Nếu là Leader
                if (roleID == Global.Data.RoleData.RoleID)
                {
                    Global.Data.RoleData.PKMode = pkMode;
                    Global.Data.RoleData.Camp = camp;
                    return;
                }
                /// Nếu là người chơi khác
                else if (Global.Data.OtherRoles.ContainsKey(roleID))
                {
                    Global.Data.OtherRoles[roleID].PKMode = pkMode;
                    Global.Data.OtherRoles[roleID].Camp = camp;
                }
                /// Nếu là quái
                else if (Global.Data.SystemMonsters.ContainsKey(roleID))
                {
                    Global.Data.SystemMonsters[roleID].Camp = camp;
                }
                /// Nếu là BOT
                else if (Global.Data.Bots.ContainsKey(roleID))
                {
                    Global.Data.Bots[roleID].Camp = camp;
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);
            }
        }

        /// <summary>
        /// Thay đổi giá trị sát khí
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="pkValue"></param>
        public void ServerChangePKValue(int roleID, int pkValue)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                /// Nếu là Leader
                if (roleID == Global.Data.RoleData.RoleID)
                {
                    Global.Data.RoleData.PKValue = pkValue;
                    return;
                }

                if (Global.Data.OtherRoles.TryGetValue(roleID, out RoleData rd))
                {
                    rd.PKValue = pkValue;
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);

            }
        }

        /// <summary>
        /// Xử lý gói tin từ Server thông báo có đối tượng
        /// </summary>
        /// <param name="otherRoleID"></param>
        /// <param name="currentX"></param>
        /// <param name="currentY"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <param name="currentDirection"></param>
        /// <param name="action"></param>
        /// <param name="pathString"></param>
        /// <param name="camp"></param>
        public void ServerLoadAlready(int otherRoleID, int currentX, int currentY, int toX, int toY, int currentDirection, int action, string pathString, int camp)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }
                /// Nếu chưa có Leader
                else if (null == this.Leader)
                {
                    return;
                }

                GSprite sprite = KTGlobal.FindSpriteByID(otherRoleID);
                if (null != sprite)
                {
                    /// Chuyển hướng đối tượng
                    sprite.Direction = (VLTK.Entities.Enum.Direction) currentDirection;

                    /// Nếu là người chơi
                    if (sprite.ComponentCharacter != null)
                    {
                        /// Lấy thông tin người chơi
                        if (Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out RoleData rd))
                        {
                            /// Nếu có cửa hàng
                            if (!string.IsNullOrEmpty(rd.StallName))
                            {
                                /// Cập nhật vị trí
                                sprite.PosX = currentX;
                                sprite.PosY = currentY;

                                /// Hiển thị tên cửa hàng
                                sprite.ComponentCharacter.ShowMyselfShopName(rd.StallName);
                                /// Thực hiện động tác ngồi
                                sprite.DoSit();

                                /// Bỏ qua
                                return;
                            }

                            /// Thiết lập Camp
                            rd.Camp = camp;
                        }
                        /// Nếu là Bot bán hàng
                        else if (Global.Data.StallBots.TryGetValue(sprite.RoleID, out StallBotData botData))
                        {
                            /// Cập nhật vị trí
                            sprite.PosX = currentX;
                            sprite.PosY = currentY;

                            /// Thực hiện động tác ngồi
                            sprite.DoSit();
                            /// Cập nhật tên sạp hàng
                            sprite.ComponentCharacter.ShowMyselfShopName(botData.StallName);

                            /// Bỏ qua
                            return;
                        }
                    }


                    /// Nếu đang nằm chết
                    if (action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_death)
                    {
                        sprite.DoDeath();
                    }
                    else if (action != (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_run && action != (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_walk)
                    {
                        /// Động tác hiện tại của đối tượng
                        if (action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_sit)
                        {
                            sprite.DoSit();
                        }
                        else if (action == (int) VLTK.Entities.Enum.KE_NPC_DOING.do_stand)
                        {
                            sprite.DoStand();
                        }
                    }
                    /// Nếu có đường di chuyển
                    if (!string.IsNullOrEmpty(pathString) && (action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_run || action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_walk))
                    {
                        if (action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_run)
                        {
                            sprite.DoRun();
                        }
                        else if (action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_walk)
                        {
                            sprite.DoWalk();
                        }

                        this.ServerLinearMove(otherRoleID, currentX, currentY, -1, -1, pathString, action);
                    }
                    /// Nếu không có đường di chuyển nhưng vị trí hiện tại khác vị trí đích
                    else if ((toX != -1 && toY != -1) && (currentX != toX || currentY != toY) && (action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_run || action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_walk))
                    {
                        if (action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_run)
                        {
                            sprite.DoRun();
                        }
                        else if (action == (int) FS.VLTK.Entities.Enum.KE_NPC_DOING.do_walk)
                        {
                            sprite.DoWalk();
                        }

                        this.ServerLinearMove(otherRoleID, currentX, currentY, toX, toY, "", action);
                    }
                    /// Nếu không có di chuyển thì cập nhật vị trí
                    else
                    {
                        sprite.PosX = currentX;
                        sprite.PosY = currentY;
                    }
                }
                else
                {
                }
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);

            }
        }

        /// <summary>
        /// Nhận gói tin thông báo từ Server
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="serverTicks"></param>
        public void ServerClientHeart(int roleID, long serverTicks)
        {
            try
            {
                /// Nếu chưa tải xuống bản đồ
                if (!this.MapLoadingCompleted)
                {
                    return;
                }

                /// Cập nhật độ lệch múi giờ so với giờ Server
                KTGlobal.LastDiffTimeToServer = serverTicks - KTGlobal.GetCurrentTimeMilis();
            }
            catch (Exception e)
            {
                KTDebug.LogException(e);

            }
        }
        #endregion
    }
}
