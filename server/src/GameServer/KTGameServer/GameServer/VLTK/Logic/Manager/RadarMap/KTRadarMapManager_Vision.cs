using GameServer.Interface;
using GameServer.KiemThe;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Core.Item;
using GameServer.Server;
using Server.Data;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý tầm nhìn của người chơi
    /// </summary>
    public static partial class KTRadarMapManager
    {
        #region Cập nhật tầm nhìn
        /// <summary>
        /// Hàm này cập nhật các đối tượng mới xung quanh hoặc xóa các đối tượng cũ trước đó nếu ngoài phạm vi
        /// </summary>
        public static void UpdateVision(KPlayer client)
        {
            /// Nếu đang đợi chuyển Map thì thôi
            if (client.WaitingForChangeMap)
            {
                return;
            }
            /// Nếu vừa chuyển vị trí thì thôi
            else if (KTGlobal.GetCurrentTimeMilis() - client.LastChangePositionTicks < 200)
            {
                return;
            }

            /// Tạo danh sách cần xóa hoặc thêm
            KTRadarMapManager.GetObjectsAroundToAddOrRemove(client, out List<IObject> toRemoveObjsList, out List<IObject> toAddObjsList);

            /// Nếu tồn tại danh sách cần xóa
            if (null != toRemoveObjsList && toRemoveObjsList.Count > 0)
            {
                /// Thực hiện xóa các đối tượng trong danh sách
                KTRadarMapManager.NotifyRemoveObjects(client, toRemoveObjsList);
            }

            /// Nếu tồn tại trong danh sách cần thêm
            if (null != toAddObjsList && toAddObjsList.Count > 0)
            {
                /// Thực hiện thêm các đối tượng trong danh sách
                KTRadarMapManager.NotifyAddObjects(client, toAddObjsList);
            }
        }
        #endregion

        #region Thông báo đối tượng xung quanh
        /// <summary>
        /// Thông báo đối tượng mới xung quanh người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="objsList"></param>
        private static void NotifyAddObjects(KPlayer client, List<IObject> objsList)
        {
            /// Toác
            if (objsList == null)
            {
                /// Bỏ qua
                return;
            }

            /// Tạo mới
            AddNewObjects data = new AddNewObjects()
            {
                Players = new List<RoleDataMini>(),
                Pets = new List<PetDataMini>(),
                Monsters = new List<MonsterData>(),
                GrowPoints = new List<GrowPointObject>(),
                DynamicAreas = new List<DynamicArea>(),
                StallBots = new List<StallBotData>(),
                DecoBots = new List<DecoBotData>(),
                NPCs = new List<NPCRole>(),
                Traps = new List<TrapRole>(),
                Carriages = new List<TraderCarriageData>(),
                GoodsPacks = new List<NewGoodsPackData>(),
            };

            /// Duyệt danh sách đối tượng
            foreach (IObject obj in objsList)
            {
                /// Toác
                if (obj == null)
                {
                    continue;
                }

                /// Nếu là người chơi
                if (obj is KPlayer player)
                {
                    /// Nếu là bản thân thì thôi
                    if (player == client)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu đã Offline thì thôi
                    if (player.LogoutState)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu đang trong trạng thái tàng hình và bản thân không thấy được
                    if (player.IsInvisible() && !player.VisibleTo(client))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.Players.Add(KTGlobal.ClientToRoleDataMini(player));
                }
                /// Nếu là pet
                else if (obj is Pet pet)
                {
                    /// Nếu bản thân không thấy được
                    if (!pet.VisibleTo(client))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.Pets.Add(pet.GetMiniData());
                }
                /// Nếu là quái
                else if (obj is Monster monster)
                {
                    /// Thêm vào danh sách
                    data.Monsters.Add(monster.GetMonsterData());
                }
                /// Nếu là vật phẩm rơi
                else if (obj is KGoodsPack goodsPack)
                {
                    /// Nếu không tồn tại thì thôi
                    if (!goodsPack.IsAlive)
                    {
                        continue;
                    }

                    /// Số sao
                    int nStar = 0;
                    StarLevelStruct starStruct = ItemManager.ItemValueCalculation(goodsPack.GoodsData, out long itemValue,out int LinesCount);
                    /// Nếu không có sao
                    if (starStruct != null)
                    {
                        nStar = starStruct.StarLevel / 2;
                    }

                    /// Thêm vào danh sách
                    data.GoodsPacks.Add(new NewGoodsPackData()
                    {
                        AutoID = goodsPack.ID,
                        PosX = (int) goodsPack.CurrentPos.X,
                        PosY = (int) goodsPack.CurrentPos.Y,
                        GoodsID = goodsPack.GoodsData.GoodsID,
                        GoodCount = goodsPack.GoodsData.GCount,
                        LifeTime = goodsPack.LifeTime,
                        HTMLColor = KTGlobal.GetItemNameColor(goodsPack.GoodsData),
                        Star = nStar,
                        LinesCount = LinesCount,
                        EnhanceLevel = goodsPack.GoodsData.Forge_level,
                    });
                }
                /// Nếu là NPC
                else if (obj is NPC npc)
                {
                    /// Nếu không hiển thị
                    if (!npc.ShowNpc)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.NPCs.Add(new NPCRole()
                    {
                        NPCID = npc.NPCID,
                        ResID = npc.ResID,
                        Name = npc.Name,
                        Title = npc.Title,
                        MapCode = npc.CurrentMapCode,
                        PosX = (int) npc.CurrentPos.X,
                        PosY = (int) npc.CurrentPos.Y,
                        Dir = (int) npc.CurrentDir,
                        VisibleOnMinimap = npc.VisibleOnMinimap,
                    });
                }
                /// Nếu là bẫy
                else if (obj is Trap trap)
                {
                    /// Nếu không nhìn thấy được
                    if (!trap.IsVisibleTo(client))
                    {
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.Traps.Add(new TrapRole()
                    {
                        ID = trap.TrapID,
                        ResID = trap.ResID,
                        PosX = (int) trap.CurrentPos.X,
                        PosY = (int) trap.CurrentPos.Y,
                        LifeTime = trap.LifeTime,
                        CasterID = trap.Owner.RoleID,
                    });
                }
                /// Nếu là điểm thu thập
                else if (obj is GrowPoint growPoint)
                {
                    /// Thêm vào danh sách
                    data.GrowPoints.Add(new GrowPointObject()
                    {
                        ID = growPoint.ID,
                        Name = growPoint.Data.Name,
                        ResID = growPoint.Data.ResID,
                        PosX = (int) growPoint.CurrentPos.X,
                        PosY = (int) growPoint.CurrentPos.Y,
                    });
                }
                /// Nếu là khu vực động
                else if (obj is KDynamicArea dynArea)
                {
                    /// Thêm vào danh sách
                    data.DynamicAreas.Add(new DynamicArea()
                    {
                        ID = dynArea.ID,
                        Name = dynArea.Name,
                        ResID = dynArea.ResID,
                        PosX = (int) dynArea.CurrentPos.X,
                        PosY = (int) dynArea.CurrentPos.Y,
                    });
                }
                /// Nếu là Bot biểu diễn
                else if (obj is KDecoBot decoBot)
                {
                    /// Thêm vào danh sách
                    data.DecoBots.Add(new DecoBotData()
                    {
                        RoleID = decoBot.RoleID,
                        RoleName = decoBot.RoleName,
                        Sex = decoBot.RoleSex,
                        PosX = (int) decoBot.CurrentPos.X,
                        PosY = (int) decoBot.CurrentPos.Y,
                        ArmorID = decoBot.ArmorID,
                        HelmID = decoBot.HelmID,
                        MantleID = decoBot.MantleID,
                        WeaponID = decoBot.WeaponID,
                        HorseID = decoBot.HorseID,
                        WeaponSeries = decoBot.WeaponSeries,
                        WeaponEnhanceLevel = decoBot.WeaponEnhanceLevel,
                        Buffs = decoBot.Buffs.ToBufferData(),
                        Title = decoBot.Title,
                        MoveSpeed = decoBot.GetCurrentRunSpeed(),
                        AtkSpeed = decoBot.GetCurrentAttackSpeed(),
                        CastSpeed = decoBot.GetCurrentCastSpeed(),
                    });
                }
                /// Nếu là Bot bán hàng
                else if (obj is KStallBot stallBot)
                {
                    /// Thêm vào danh sách
                    data.StallBots.Add(new StallBotData()
                    {
                        RoleID = stallBot.RoleID,
                        OwnerRoleName = stallBot.OwnerRoleName,
                        Sex = stallBot.OwnerRoleSex,
                        PosX = (int) stallBot.CurrentPos.X,
                        PosY = (int) stallBot.CurrentPos.Y,
                        ArmorID = stallBot.ArmorID,
                        HelmID = stallBot.HelmID,
                        MantleID = stallBot.MantleID,
                        StallName = stallBot.StallName,
                    });
                }
                /// Nếu là xe tiêu
                else if (obj is TraderCarriage carriage)
                {
                    /// Thêm vào danh sách
                    data.Carriages.Add(new TraderCarriageData()
                    {
                        RoleID = carriage.RoleID,
                        Name = carriage.RoleName,
                        Title = carriage.Title,
                        Direction = (int) carriage.CurrentDir,
                        ResID = carriage.ResID,
                        OwnerID = carriage.Owner.RoleID,
                        PosX = (int) carriage.CurrentPos.X,
                        PosY = (int) carriage.CurrentPos.Y,
                        MoveSpeed = carriage.GetCurrentRunSpeed(),
                        HP = carriage.m_CurrentLife,
                        MaxHP = carriage.m_CurrentLifeMax,
                    });
                }
            }

            /// Gửi gói tin
            client.SendPacket<AddNewObjects>((int) TCPGameServerCmds.CMD_NEW_OBJECTS, data);
        }

        /// <summary>
        /// Thông báo để xóa người chơi khỏi danh sách xung quanh của người khác
        /// </summary>
        private static void NotifyRemoveObjects(KPlayer client, List<IObject> objsList)
        {
            /// Toác
            if (objsList == null)
            {
                /// Bỏ qua
                return;
            }

            /// Tạo mới
            RemoveObjects data = new RemoveObjects()
            {
                Players = new List<int>(),
                Pets = new List<int>(),
                Monsters = new List<int>(),
                GrowPoints = new List<int>(),
                DynamicAreas = new List<int>(),
                StallBots = new List<int>(),
                DecoBots = new List<int>(),
                NPCs = new List<int>(),
                Traps = new List<int>(),
                Carriages = new List<int>(),
                GoodsPacks = new List<int>(),
            };

            /// Duyệt danh sách đối tượng
            foreach (IObject obj in objsList)
            {
                /// Toác
                if (obj == null)
                {
                    continue;
                }

                /// Nếu là người chơi
                if (obj is KPlayer player)
                {
                    /// Nếu là bản thân thì thôi
                    if (player == client)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.Players.Add(player.RoleID);
                }
                /// Nếu là pet
                else if (obj is Pet pet)
                {
                    /// Nếu bản thân không thấy được
                    if (!pet.VisibleTo(client))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.Pets.Add(pet.RoleID);
                }
                /// Nếu là quái
                else if (obj is Monster monster)
                {
                    /// Thêm vào danh sách
                    data.Monsters.Add(monster.RoleID);
                }
                /// Nếu là vật phẩm rơi
                else if (obj is KGoodsPack goodsPack)
                {
                    /// Nếu không tồn tại thì thôi
                    if (!goodsPack.IsAlive)
                    {
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.GoodsPacks.Add(goodsPack.ID);
                }
                /// Nếu là NPC
                else if (obj is NPC npc)
                {
                    /// Nếu không hiển thị
                    if (!npc.ShowNpc)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.NPCs.Add(npc.NPCID);
                }
                /// Nếu là bẫy
                else if (obj is Trap trap)
                {
                    /// Nếu không nhìn thấy được
                    if (!trap.IsVisibleTo(client))
                    {
                        continue;
                    }

                    /// Thêm vào danh sách
                    data.Traps.Add(trap.TrapID);
                }
                /// Nếu là điểm thu thập
                else if (obj is GrowPoint growPoint)
                {
                    /// Thêm vào danh sách
                    data.GrowPoints.Add(growPoint.ID);
                }
                /// Nếu là khu vực động
                else if (obj is KDynamicArea dynArea)
                {
                    /// Thêm vào danh sách
                    data.DynamicAreas.Add(dynArea.ID);
                }
                /// Nếu là Bot biểu diễn
                else if (obj is KDecoBot decoBot)
                {
                    /// Thêm vào danh sách
                    data.DecoBots.Add(decoBot.RoleID);
                }
                /// Nếu là Bot bán hàng
                else if (obj is KStallBot stallBot)
                {
                    /// Thêm vào danh sách
                    data.StallBots.Add(stallBot.RoleID);
                }
                /// Nếu là xe tiêu
                else if (obj is TraderCarriage carriage)
                {
                    /// Thêm vào danh sách
                    data.Carriages.Add(carriage.RoleID);
                }
            }

            /// Gửi gói tin
            client.SendPacket<RemoveObjects>((int) TCPGameServerCmds.CMD_REMOVE_OBJECTS, data);
        }
        #endregion
    }
}
