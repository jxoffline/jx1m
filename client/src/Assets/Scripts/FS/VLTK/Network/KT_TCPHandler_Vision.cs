using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using FS.VLTK.Network.Skill;
using Server.Data;
using Server.Tools;
using System.Collections;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
	{
        #region Thêm
        /// <summary>
        /// Nhận gói tin thông báo danh sách các đối tượng xung quanh
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static IEnumerator ReceiveNewObjects(AddNewObjects data)
        {
            /// Toác
            if (data == null)
            {
                yield break;
            }

            /// Nếu tồn tại danh sách người chơi
            if (data.Players != null)
            {
                /// Duyệt danh sách
                foreach (RoleDataMini rdMini in data.Players)
                {
                    /// Toác
                    if (rdMini == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Dữ liệu
                    RoleData roleData = KTGlobal.RoleDataMiniToRoleData(rdMini);

                    /// Toác
                    if (roleData == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Lưu vào danh sách
                    Global.Data.OtherRoles[roleData.RoleID] = roleData;
                    Global.Data.OtherRolesByName[roleData.RoleName] = roleData;

                    /// Tải xuống
                    Global.Data.GameScene.ToLoadOtherRole(roleData, roleData.PosX, roleData.PosY, roleData.RoleDirection);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách pet
            if (data.Pets != null)
            {
                /// Duyệt danh sách
                foreach (PetDataMini petData in data.Pets)
                {
                    /// Toác
                    if (petData == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách theo dõi của Leader
                    Global.Data.SystemPets[petData.ID] = petData;

                    /// Tải xuống
                    Global.Data.GameScene.ServerNewPet(petData);

                    /// Nếu chủ nhân là bản thân
                    if (petData.RoleID == Global.Data.RoleData.RoleID)
                    {
                        /// Thiết lập lại
                        Global.Data.RoleData.CurrentPetID = petData.ID - (int) ObjectBaseID.Pet;
                        /// Lưu lại ID pet lần trước
                        Global.Data.LastPetID = petData.ID - (int) ObjectBaseID.Pet;
                    }

                    /// Nếu là pet hiện tại của bản thân
                    if (Global.Data.RoleData.CurrentPetID != -1 && petData.ID == Global.Data.RoleData.CurrentPetID)
                    {
                        /// Nếu hiển thị khung
                        if (PlayZone.Instance.UIPet != null)
                        {
                            /// Cập nhật máu
                            PlayZone.Instance.UIPet.UpdateHP(Global.Data.RoleData.CurrentPetID, petData.HP, petData.MaxHP);
                        }
                    }

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách quái
            if (data.Monsters != null)
            {
                //KTDebug.Log("Load monsters count = " + data.Monsters.Count);
                /// Duyệt danh sách
                foreach (MonsterData monsterData in data.Monsters)
                {
                    /// Toác
                    if (monsterData == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Lưu lại
                    Global.Data.SystemMonsters[monsterData.RoleID] = monsterData;
                    /// Nếu còn sống
                    if (monsterData.HP > 0)
                    {
                        /// Tải xuống
                        Global.Data.GameScene.ToLoadMonster(monsterData, monsterData.PosX, monsterData.PosY, monsterData.Direction);
                    }

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách vật phẩm rơi
            if (data.GoodsPacks != null)
            {
                /// Duyệt danh sách
                foreach (NewGoodsPackData gpData in data.GoodsPacks)
                {
                    /// Toác
                    if (data == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Tải xuống
                    Global.Data.GameScene.ServerNewGoodsPack(gpData);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách điểm thu thập
            if (data.GrowPoints != null)
            {
                /// Duyệt danh sách
                foreach (GrowPointObject growPointObject in data.GrowPoints)
                {
                    /// Toác
                    if (growPointObject == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Tải xuống
                    Global.Data.GameScene.ServerNewGrowPoint(growPointObject);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách khu vực động
            if (data.DynamicAreas != null)
            {
                /// Duyệt danh sách
                foreach (DynamicArea dynArea in data.DynamicAreas)
                {
                    /// Toác
                    if (dynArea == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Tải xuống
                    Global.Data.GameScene.ServerNewDynamicArea(dynArea);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách bot bán hàng
            if (data.StallBots != null)
            {
                /// Duyệt danh sách
                foreach (StallBotData botData in data.StallBots)
                {
                    /// Toác
                    if (botData == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào Cache
                    Global.Data.StallBots[botData.RoleID] = botData;
                    /// Tải xuống
                    Global.Data.GameScene.ToLoadStallBot(botData);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách bot biểu diễn
            if (data.DecoBots != null)
            {
                /// Duyệt danh sách
                foreach (DecoBotData botData in data.DecoBots)
                {
                    /// Toác
                    if (botData == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Dữ liệu
                    RoleData roleData = KTGlobal.DecoBotDataToRoleData(botData);
                    /// Toác
                    if (roleData == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Lưu dữ liệu
                    Global.Data.Bots[roleData.RoleID] = roleData;
                    /// Tải xuống
                    Global.Data.GameScene.ToLoadBot(roleData);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách npc
            if (data.NPCs != null)
            {
                /// Duyệt danh sách
                foreach (NPCRole npc in data.NPCs)
                {
                    /// Toác
                    if (npc == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Tải xuống
                    Global.Data.GameScene.ServerNewNPC(npc);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách bẫy
            if (data.Traps != null)
            {
                /// Duyệt danh sách
                foreach (TrapRole trap in data.Traps)
                {
                    /// Toác
                    if (trap == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Đối tượng thao tác
                    GSprite sprite;

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

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách xe tiêu
            if (data.Carriages != null)
            {
                /// Duyệt danh sách
                foreach (TraderCarriageData carriageData in data.Carriages)
                {
                    /// Toác
                    if (carriageData == null)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách theo dõi của Leader
                    Global.Data.TraderCarriages[carriageData.RoleID] = carriageData;

                    /// Tải xuống
                    Global.Data.GameScene.ServerNewTraderCarriage(carriageData);

                    /// Đợi
                    yield return null;
                }
            }

        }
        #endregion

        #region Xóa
        /// <summary>
        /// Nhận gói tin thông báo xóa các đối tượng xung quanh
        /// </summary>
        /// <param name="data"></param>
        public static IEnumerator ReceiveRemoveObjects(RemoveObjects data)
        {
            /// Toác
            if (data == null)
            {
                yield break;
            }

            /// Nếu tồn tại danh sách người chơi
            if (data.Players != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.Players)
                {
                    Global.Data.GameScene.ServerRoleLeave(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách pet
            if (data.Pets != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.Pets)
                {
                    Global.Data.GameScene.ServerRoleLeave(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách quái
            if (data.Monsters != null)
            {
                //KTDebug.Log("Remove monsters count = " + data.Monsters.Count);
                /// Duyệt danh sách
                foreach (int roleID in data.Monsters)
                {
                    Global.Data.GameScene.ServerRoleLeave(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách vật phẩm rơi
            if (data.GoodsPacks != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.GoodsPacks)
                {
                    Global.Data.GameScene.ServerDelGoodsPack(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách điểm thu thập
            if (data.GrowPoints != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.GrowPoints)
                {
                    Global.Data.GameScene.ServerDelGrowPoint(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách khu vực động
            if (data.DynamicAreas != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.DynamicAreas)
                {
                    Global.Data.GameScene.ServerDelDynamicArea(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách bot bán hàng
            if (data.StallBots != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.StallBots)
                {
                    Global.Data.GameScene.DelStallBot(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách bot biểu diễn
            if (data.DecoBots != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.DecoBots)
                {
                    Global.Data.GameScene.DelBot(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách npc
            if (data.NPCs != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.NPCs)
                {
                    Global.Data.GameScene.ServerDelNPC(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách bẫy
            if (data.Traps != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.Traps)
                {
                    KTTCPSkillManager.ReceiveDelTrap(roleID);

                    /// Đợi
                    yield return null;
                }
            }
            /// Nếu tồn tại danh sách xe tiêu
            if (data.Carriages != null)
            {
                /// Duyệt danh sách
                foreach (int roleID in data.Carriages)
                {
                    /// Xóa khỏi danh sách theo dõi của Leader
                    Global.Data.TraderCarriages.Remove(roleID);

                    Global.Data.GameScene.ServerDelTraderCarriage(roleID);

                    /// Đợi
                    yield return null;
                }
            }
        }
        #endregion
    }
}
