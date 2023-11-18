using GameServer.KiemThe.Entities;
using GameServer.Logic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Có phải kẻ địch không
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsOpposite(GameObject obj, GameObject target)
        {
            /// Toác gì đó
            if (obj == null || target == null)
            {
                return false;
            }

            /// Nếu bản thân là DecoBot
            if (obj is KDecoBot)
            {
                /// Nếu đối phương là cọc gỗ
                if (target is Monster targetMonster && targetMonster.Tag == "DecoBotColumn")
                {
                    /// Là kẻ địch
                    return true;
                }
                /// Nếu không phải cọc gỗ
                else
                {
                    /// Không phải kẻ địch
                    return false;
                }
            }

            /// Nếu đối phương là DecoBot
            if (target is KDecoBot)
            {
                /// Không phải kẻ địch
                return false;
            }

            /// Nếu đối phương là cọc gỗ của DecoBot
            if (target is Monster mons && mons.Tag == "DecoBotColumn")
            {
                /// Không phải kẻ địch
                return false;
            }

            /// Nếu là chính mình thì bỏ qua
            if (obj == target)
            {
                return false;
            }

            /// Nếu là Pet thì lấy tham chiếu chủ nhân
            if (obj is Pet pet)
            {
                obj = pet.Owner;
            }
            if (target is Pet _pet)
            {
                target = _pet.Owner;
            }

            /// Nếu là xe tiêu thì lấy tham chiếu chủ nhân
            if (obj is TraderCarriage carriage)
            {
                obj = carriage.Owner;
            }
            if (target is TraderCarriage _carriage)
            {
                target = _carriage.Owner;
            }

            /// Nếu là chính mình
            if (obj == target)
            {
                return false;
            }

            /// Nếu là người chơi và đang trong khu an toàn
            if (obj is KPlayer)
            {
                KPlayer objPlayer = obj as KPlayer;
                if (objPlayer.IsInsideSafeZone)
                {
                    return false;
                }
            }
            /// Nếu là người chơi và đang trong khu an toàn
            if (target is KPlayer)
            {
                KPlayer targetPlayer = target as KPlayer;
                if (targetPlayer.IsInsideSafeZone)
                {
                    return false;
                }
            }

            /// Nếu cả 2 đều là người chơi
            if (obj is KPlayer && target is KPlayer)
            {
                /// Nếu cấp độ một trong hai dưới 30
                if (obj.m_Level < 30 || target.m_Level < 30)
                {
                    return false;
                }
            }

            /// Nếu là người chơi
            if (obj is KPlayer && target is KPlayer)
            {
                KPlayer objPlayer = obj as KPlayer;
                KPlayer targetPlayer = target as KPlayer;

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(obj.CurrentMapCode);
                /// Nếu bản đồ không cho phép đánh nhau
                if (gameMap != null && !gameMap.AllowPK)
                {
                    return false;
                }

                /// Nếu đang tuyên chiến cùng
                if (objPlayer.IsActiveFightWith(targetPlayer))
                {
                    return true;
                }

                /// Nếu là trạng thái PK đặc biệt
                if (objPlayer.PKMode == (int) PKMode.Custom && targetPlayer.PKMode == (int) PKMode.Custom)
                {
                    /// Trả về kết quả Camp khác nhau
                    return objPlayer.Camp != -1 && targetPlayer.Camp != -1 && objPlayer.Camp != targetPlayer.Camp;
                }

                /// Nếu cùng nhóm thì không cho đánh nhau
                if (KTGlobal.IsTeamMate(targetPlayer, objPlayer))
                {
                    return false;
                }

                /// Nếu cùng bang hoặc tộc thì không cho đánh nhau
                if (KTGlobal.IsTeamMate(objPlayer, targetPlayer) || KTGlobal.IsSameFamily(objPlayer, targetPlayer) || KTGlobal.IsGuildMate(objPlayer, targetPlayer))
                {
                    return false;
                }

                /// Nếu 1 trong 2 có trạng thái PK đồ sát
                if (objPlayer.PKMode == (int) PKMode.All || targetPlayer.PKMode == (int) PKMode.All)
                {
                    return true;
                }
                /// Nếu 1 trong 2 có trạng thái PK Server
                else if (objPlayer.PKMode == (int) PKMode.Server || targetPlayer.PKMode == (int) PKMode.Server)
                {
                    return objPlayer.ZoneID != targetPlayer.ZoneID;
                }
                /// Nếu cả 2 có trạng thái PK nhóm, hoặc bang
                else if (((objPlayer.PKMode == (int) PKMode.Team || objPlayer.PKMode == (int) PKMode.Guild) && (targetPlayer.PKMode == (int) PKMode.Team || targetPlayer.PKMode == (int) PKMode.Guild)))
                {
                    return true;
                }
                /// Nếu một trong 2 có trạng thái PK thiện ác
                else if (objPlayer.PKMode == (int) PKMode.Moral || targetPlayer.PKMode == (int) PKMode.Moral)
                {
                    /// Nếu bản thân có trạng thái thiện ác và đối phương có sát khí
                    if (objPlayer.PKMode == (int) PKMode.Moral && targetPlayer.PKValue > 0)
                    {
                        return true;
                    }
                    /// Nếu đối phượng có trạng thái thiện ác và bản thân có sát khí
                    else if (targetPlayer.PKMode == (int) PKMode.Moral && objPlayer.PKValue > 0)
                    {
                        return true;
                    }
                }

                /// Không thỏa mãn thì không đánh nhau được
                return false;
            }
            /// Nếu một trong 2 không phải người
            else
            {
                /// Nếu một trong 2 là DynamicNPC
                if ((obj is Monster objMonster && objMonster.MonsterType == MonsterAIType.DynamicNPC) || (target is Monster targetMonster && targetMonster.MonsterType == MonsterAIType.DynamicNPC))
                {
                    return false;
                }

                /// Nếu bản thân là quái và có Camp -1 thì thôi
                if (obj is Monster && obj.Camp == -1)
                {
                    return false;
                }
                /// Nếu đối phương là quái và có Camp -1 thì thôi
                if (target is Monster && target.Camp == -1)
                {
                    return false;
                }

                /// Theo Camp
                return obj.Camp != target.Camp;
            }
        }

        /// <summary>
        /// Có thể tấn công đối phương được không
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool CanAttack(GameObject obj, GameObject target)
        {
            /// Nếu không phải kẻ địch
            if (!KTGlobal.IsOpposite(obj, target))
            {
                return false;
            }

            /// Nếu là Pet thì lấy tham chiếu chủ nhân
            if (obj is Pet pet)
            {
                obj = pet.Owner;
            }
            if (target is Pet _pet)
            {
                target = _pet.Owner;
            }

            /// Nếu là xe tiêu thì lấy tham chiếu chủ nhân
            if (obj is TraderCarriage carriage)
            {
                obj = carriage.Owner;
            }
            if (target is TraderCarriage _carriage)
            {
                target = _carriage.Owner;
            }

            /// Nếu là chính mình
            if (obj == target)
            {
                return false;
            }

            /// Nếu là người chơi và đang trong khu an toàn
            if (obj is KPlayer)
            {
                KPlayer objPlayer = obj as KPlayer;
                if (objPlayer.IsInsideSafeZone)
                {
                    return false;
                }
            }
            /// Nếu là người chơi và đang trong khu an toàn
            if (target is KPlayer)
            {
                KPlayer targetPlayer = target as KPlayer;
                if (targetPlayer.IsInsideSafeZone)
                {
                    return false;
                }
            }

            /// Nếu cả 2 đều là người chơi
            if (obj is KPlayer && target is KPlayer)
            {
                /// Nếu cấp độ một trong hai dưới 30
                if (obj.m_Level < 30 || target.m_Level < 30)
                {
                    return false;
                }
            }

            /// Nếu là người chơi
            if (obj is KPlayer && target is KPlayer)
            {
                KPlayer objPlayer = obj as KPlayer;
                KPlayer targetPlayer = target as KPlayer;

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(obj.CurrentMapCode);
                /// Nếu bản đồ không cho phép đánh nhau
                if (gameMap != null && !gameMap.AllowPK)
                {
                    return false;
                }

                /// Nếu đang tuyên chiến cùng
                if (objPlayer.IsActiveFightWith(targetPlayer))
                {
                    return true;
                }

                /// Nếu là trạng thái PK đặc biệt
                if (objPlayer.PKMode == (int) PKMode.Custom && targetPlayer.PKMode == (int) PKMode.Custom)
                {
                    /// Trả về kết quả Camp khác nhau
                    return objPlayer.Camp != -1 && targetPlayer.Camp != -1 && objPlayer.Camp != targetPlayer.Camp;
                }

                /// Nếu cùng nhóm thì không cho đánh nhau
                if (KTGlobal.IsTeamMate(objPlayer, targetPlayer))
                {
                    return false;
                }

                /// Nếu cùng bang hoặc tộc thì không cho đánh nhau
                if (KTGlobal.IsTeamMate(objPlayer, targetPlayer) || KTGlobal.IsSameFamily(objPlayer, targetPlayer) || KTGlobal.IsGuildMate(objPlayer, targetPlayer))
                {
                    return false;
                }

                /// Nếu bản thân đang trong trạng thái luyện công
                if (objPlayer.PKMode == (int) PKMode.Peace)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            /// Nếu không phải người chơi
            return true;
        }

        /// <summary>
        /// Có phải đồng đội không
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsTeamMate(KPlayer obj, KPlayer target)
        {
            return obj.TeamID != -1 && obj.TeamID == target.TeamID;
        }

        /// <summary>
        /// Có cùng bang không
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsGuildMate(KPlayer obj, KPlayer target)
        {
            /// Nếu cả hai đều không có bang
            if (obj.GuildID <= 0 && target.GuildID <= 0)
            {
                return false;
            }
            return target.GuildID == obj.GuildID;
        }

        /// <summary>
        /// Có cùng tộc không
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsSameFamily(KPlayer obj, KPlayer target)
        {
            /// Nếu cả hai đều không có tộc
            if (obj.FamilyID <= 0 && target.FamilyID <= 0)
            {
                return false;
            }
            return obj.FamilyID == target.FamilyID;
        }
    }
}
