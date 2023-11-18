using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Utilities.Threading;
using Server.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region RelationShip

        private static Color _EnemyPlayerNameColor = Color.white;
        /// <summary>
        /// Màu thanh máu người chơi khi ở trạng thái địch
        /// </summary>
        public static Color EnemyPlayerNameColor
        {
            get
            {
                if (KTGlobal._EnemyPlayerNameColor == Color.white)
                {
                    ColorUtility.TryParseHtmlString("#FF5200", out KTGlobal._EnemyPlayerNameColor);
                }
                return KTGlobal._EnemyPlayerNameColor;
            }
        }

        private static Color _DangerousPlayerNameColor = Color.white;
        /// <summary>
        /// Màu thanh máu người chơi khi ở trạng thái địch
        /// </summary>
        public static Color DangerousPlayerNameColor
        {
            get
            {
                if (KTGlobal._DangerousPlayerNameColor == Color.white)
                {
                    ColorUtility.TryParseHtmlString("#ff52df", out KTGlobal._DangerousPlayerNameColor);
                }
                return KTGlobal._DangerousPlayerNameColor;
            }
        }

        /// <summary>
        /// ID đối tượng đang tỷ thí cùng
        /// </summary>
        public static int ChallengePartnerID { get; set; } = -1;

        /// <summary>
        /// ID hiệu ứng tỷ thí kết thúc cho người thằng cuộc
        /// </summary>
        private const int ChallengeEffectID_Winner = 1062;

        /// <summary>
        /// ID hiệu ứng tỷ thí kết thúc cho người thằng cuộc
        /// </summary>
        private const int ChallengeEffectID_Loser = 1063;

        /// <summary>
        /// Thực thi hiệu ứng kết thúc tỷ thí cho đối tượng tương ứng
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="isWinner"></param>
        public static void PlayChallengeEffect(GSprite sprite, bool isWinner)
        {
            /// Nếu không phải người chơi
            if (sprite.ComponentCharacter == null)
            {
                return;
            }

            if (isWinner)
            {
                sprite.ComponentCharacter.AddEffect(KTGlobal.ChallengeEffectID_Winner, EffectType.CastEffect);
                KTTimerManager.Instance.SetTimeout(5f, () => {
                    sprite.ComponentCharacter.RemoveEffect(KTGlobal.ChallengeEffectID_Winner);
                });
            }
            else
            {
                sprite.ComponentCharacter.AddEffect(KTGlobal.ChallengeEffectID_Loser, EffectType.CastEffect);
                KTTimerManager.Instance.SetTimeout(5f, () => {
                    sprite.ComponentCharacter.RemoveEffect(KTGlobal.ChallengeEffectID_Loser);
                });
            }
        }

        /// <summary>
        /// Danh sách ID đối tượng đang tuyên chiến cùng
        /// </summary>
        public static HashSet<int> ActiveFightWith { get; set; } = new HashSet<int>();

        /// <summary>
        /// Kiểm tra đối tượng có thể tấn công bản thân bất chợt không
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsDangerous(GSprite target)
        {
            /// Nếu đối phương đang ở khu an toàn
            if (KTGlobal.InSafeArea(target.PositionInVector2))
            {
                return false;
            }

            /// Nếu là Bot
            if (target.SpriteType == GSpriteTypes.Bot || target.SpriteType == GSpriteTypes.StallBot)
            {
                return false;
            }

            /// Nếu bản thân là pet
            if (target.PetData != null)
            {
                /// Thay bằng chủ nhân tương ứng
                target = KTGlobal.FindSpriteByID(target.PetData.RoleID);
                /// Nếu không tìm thấy
                if (target == null)
                {
                    return false;
                }
                /// Nếu là chính mình
                else if (target == Global.Data.Leader)
                {
                    /// Không đánh được
                    return false;
                }
            }
            /// Nếu là xe tiêu
            else if (target.TraderCarriageData != null)
            {
                /// Tìm chủ nhân tương ứng
                target = KTGlobal.FindSpriteByID(target.TraderCarriageData.OwnerID);
                /// Nếu không tìm thấy
                if (target == null)
                {
                    /// Cho vã thoải mái
                    return true;
                }
                /// Nếu là chính mình
                else if (target == Global.Data.Leader)
                {
                    /// không đánh được
                    return false;
                }
            }

            /// Nếu đối phương là người chơi
            if (Global.Data.OtherRoles.TryGetValue(target.RoleID, out RoleData _targetRD))
            {
                /// Nếu cấp độ dưới 30
                if (_targetRD.Level < 30)
                {
                    /// Không đánh được
                    return false;
                }
                /// Nếu bản thân cấp độ dưới 30
                else if (Global.Data.RoleData.Level < 30)
                {
                    /// Không đánh được
                    return false;
                }
            }

            /// Nếu là người chơi
            if (Global.Data.OtherRoles.TryGetValue(target.RoleID, out RoleData targetRD))
            {
                /// Nếu bản đồ hiện tại không cho phép đánh nhau
                if (!Global.Data.GameScene.CurrentMapData.Setting.AllowPK)
                {
                    return false;
                }

                /// Nếu đang trong trạng thái tỷ thí và có mục tiêu tỷ thí
                if (KTGlobal.ChallengePartnerID != -1)
                {
                    /// Nếu mục tiêu tỷ thí trùng với đối tượng cần kiểm tra
                    if (KTGlobal.ChallengePartnerID == target.RoleID)
                    {
                        return false;
                    }
                }

                /// Nếu đang tuyên chiến cùng
                if (KTGlobal.ActiveFightWith.Contains(target.RoleID))
                {
                    return false;
                }

                /// Nếu là trạng thái PK đặc biệt
                if (Global.Data.RoleData.PKMode == (int) PKMode.Custom && targetRD.PKMode == (int) PKMode.Custom)
                {
                    /// Trả về kết quả Camp khác nhau
                    return Global.Data.RoleData.Camp != -1 && targetRD.Camp != -1 && Global.Data.RoleData.Camp != targetRD.Camp;
                }

                /// Nếu cùng tổ đội
                if (KTGlobal.IsTeammate(target))
				{
                    return false;
                }
                /// Nếu cùng bang
                if (KTGlobal.IsGuildMate(target))
                {
                    return false;
                }

                /// Nếu đối phương có trạng thái đồ sát
                if (targetRD.PKMode == (int) PKMode.All)
                {
                    return true;
                }
                /// Nếu đối phương có trạng thái PK Server
                else if (targetRD.PKMode == (int) PKMode.Server)
                {
                    /// Trả về kết quả nếu khác Server
                    return Global.Data.RoleData.ZoneID != targetRD.ZoneID;
                }
                /// Nếu cả 2 có trạng thái PK nhóm hoặc bang, hoặc Server
                else if ((Global.Data.RoleData.PKMode == (int) PKMode.Team || Global.Data.RoleData.PKMode == (int) PKMode.Guild) && (targetRD.PKMode == (int) PKMode.Team || targetRD.PKMode == (int) PKMode.Guild))
                {
                    return true;
                }
                /// Nếu đối phương có trạng thái PK thiện ác và bản thân có sát khí
                else if (targetRD.PKMode == (int) PKMode.Moral && Global.Data.RoleData.PKValue > 0)
                {
                    return true;
                }
            }

            /// Không nguy hiểm
            return false;
        }

        /// <summary>
        /// Kiểm tra đối tượng có phải kẻ địch không
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsEnemy(GSprite target)
        {
            /// Nếu đối phương đang ở khu an toàn
            if (KTGlobal.InSafeArea(target.PositionInVector2))
            {
                return false;
            }

            /// Nếu đối phương là Bot
            if (target.SpriteType == GSpriteTypes.Bot || target.SpriteType == GSpriteTypes.StallBot)
            {
                return false;
            }

            /// Nếu là Pet
            if (target.PetData != null)
            {
                /// Tìm chủ nhân tương ứng
                target = KTGlobal.FindSpriteByID(target.PetData.RoleID);
                /// Nếu không tìm thấy
                if (target == null)
                {
                    /// Toác
                    return false;
                }
                /// Nếu là chính mình
                else if (target == Global.Data.Leader)
                {
                    /// Không đánh được
                    return false;
                }
            }
            /// Nếu là xe tiêu
            else if (target.TraderCarriageData != null)
            {
                /// Tìm chủ nhân tương ứng
                target = KTGlobal.FindSpriteByID(target.TraderCarriageData.OwnerID);
                /// Nếu không tìm thấy
                if (target == null)
                {
                    /// Cho vã thoải mái
                    return true;
                }
                /// Nếu là chính mình
                else if (target == Global.Data.Leader)
                {
                    /// Không đánh được
                    return false;
                }
            }

            /// Nếu đối phương là người chơi
            if (Global.Data.OtherRoles.TryGetValue(target.RoleID, out RoleData _targetRD))
            {
                /// Nếu cấp độ dưới 30
                if (_targetRD.Level < 30)
                {
                    /// Không đánh được
                    return false;
                }
                /// Nếu bản thân cấp độ dưới 30
                else if (Global.Data.RoleData.Level < 30)
                {
                    /// Không đánh được
                    return false;
                }
            }

            /// Nếu là người chơi
            if (Global.Data.OtherRoles.TryGetValue(target.RoleID, out RoleData targetRD))
            {
                /// Nếu bản đồ hiện tại không cho phép đánh nhau
                if (!Global.Data.GameScene.CurrentMapData.Setting.AllowPK)
                {
                    return false;
                }

                /// Nếu đang trong trạng thái tỷ thí và có mục tiêu tỷ thí
                if (KTGlobal.ChallengePartnerID != -1)
                {
                    /// Nếu mục tiêu tỷ thí trùng với đối tượng cần kiểm tra
                    if (KTGlobal.ChallengePartnerID == target.RoleID)
                    {
                        return true;
                    }
                }

                /// Nếu đang tuyên chiến cùng
                if (KTGlobal.ActiveFightWith.Contains(target.RoleID))
                {
                    return true;
                }

                /// Nếu là trạng thái PK đặc biệt
                if (Global.Data.RoleData.PKMode == (int) PKMode.Custom && targetRD.PKMode == (int) PKMode.Custom)
                {
                    /// Trả về kết quả Camp khác nhau
                    return Global.Data.RoleData.Camp != -1 && targetRD.Camp != -1 && Global.Data.RoleData.Camp != targetRD.Camp;
                }

                /// Nếu cùng tổ đội
                if (KTGlobal.IsTeammate(target))
                {
                    return false;
                }
                /// Nếu cùng bang
                if (KTGlobal.IsGuildMate(target))
				{
                    return false;
				}

                /// Nếu 1 trong 2 có trạng thái PK đồ sát
                if (Global.Data.RoleData.PKMode == (int) PKMode.All || targetRD.PKMode == (int) PKMode.All)
                {
                    return true;
                }
                /// Nếu 1 trong 2 có trạng thái PK Server
                else if (Global.Data.RoleData.PKMode == (int) PKMode.Server || targetRD.PKMode == (int) PKMode.Server)
                {
                    /// Trả về kết quả nếu khác Server
                    return Global.Data.RoleData.ZoneID != targetRD.ZoneID;
                }
                /// Nếu cả 2 có trạng thái PK nhóm hoặc bang, hoặc Server
                else if ((Global.Data.RoleData.PKMode == (int) PKMode.Team || Global.Data.RoleData.PKMode == (int) PKMode.Guild) && (targetRD.PKMode == (int) PKMode.Team || targetRD.PKMode == (int) PKMode.Guild))
                {
                    return true;
                }
                /// Nếu một trong 2 có trạng thái PK thiện ác
                else if (Global.Data.RoleData.PKMode == (int) PKMode.Moral || targetRD.PKMode == (int) PKMode.Moral)
                {
                    /// Nếu bản thân có trạng thái thiện ác và đối phương có sát khí
                    if (Global.Data.RoleData.PKMode == (int) PKMode.Moral && targetRD.PKValue > 0)
                    {
                        return true;
                    }
                    /// Nếu đối phượng có trạng thái thiện ác và bản thân có sát khí
                    else if (targetRD.PKMode == (int) PKMode.Moral && Global.Data.RoleData.PKValue > 0)
                    {
                        return true;
                    }
                }

                /// Không thỏa mãn thì không đánh nhau được
                return false;
            }

            /// Nếu đối phương có Camp -1 thì không đánh nhau được
            if (target.Camp == -1)
            {
                return false;
            }

            /// Nếu đối phương là NPC di động
            if (target.MonsterData != null && target.MonsterData.MonsterType == (int) MonsterTypes.DynamicNPC)
            {
                return false;
            }

            /// Nếu Camp khác nhau thì đánh nhau
            return Global.Data.RoleData.Camp != target.Camp;
        }

        /// <summary>
        /// Kiểm tra xem có thể tấn công được đối tượng không
        /// </summary>
        /// <param name="sprite"></param>
        /// <returns></returns>
        public static bool CanAttack(GSprite sprite)
        {
            /// Nếu đối phương đang ở khu an toàn
            if (KTGlobal.InSafeArea(sprite.PositionInVector2))
            {
                return false;
            }

            /// Nếu không phải kẻ địch
            if (!KTGlobal.IsEnemy(sprite))
            {
                return false;
            }

            /// Nếu là Pet
            if (sprite.PetData != null)
            {
                /// Tìm chủ nhân tương ứng
                sprite = KTGlobal.FindSpriteByID(sprite.PetData.RoleID);
                /// Nếu không tìm thấy
                if (sprite == null)
                {
                    /// Toác
                    return false;
                }
                /// Nếu là chính mình
                else if (sprite == Global.Data.Leader)
                {
                    /// Không đánh được
                    return false;
                }
            }
            /// Nếu là xe tiêu
            else if (sprite.TraderCarriageData != null)
            {
                /// Tìm chủ nhân tương ứng
                sprite = KTGlobal.FindSpriteByID(sprite.TraderCarriageData.OwnerID);
                /// Nếu không tìm thấy
                if (sprite == null)
                {
                    /// Cho vã thoải mái
                    return true;
                }
                /// Nếu là chính mình
                else if (sprite == Global.Data.Leader)
                {
                    /// Không đánh được
                    return false;
                }
            }

            /// Nếu đối phương là người chơi
            if (Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out RoleData _targetRD))
            {
                /// Nếu cấp độ dưới 30
                if (_targetRD.Level < 30)
                {
                    /// Không đánh được
                    return false;
                }
                /// Nếu bản thân cấp độ dưới 30
                else if (Global.Data.RoleData.Level < 30)
                {
                    /// Không đánh được
                    return false;
                }
            }

            /// Nếu là người chơi
            if (Global.Data.OtherRoles.TryGetValue(sprite.RoleID, out RoleData targetRD))
            {
                /// Nếu bản đồ hiện tại không cho phép đánh nhau
                if (!Global.Data.GameScene.CurrentMapData.Setting.AllowPK)
                {
                    return false;
                }

                /// Nếu đang trong trạng thái tỷ thí và có mục tiêu tỷ thí
                if (KTGlobal.ChallengePartnerID != -1)
                {
                    /// Nếu mục tiêu tỷ thí trùng với đối tượng cần kiểm tra
                    if (KTGlobal.ChallengePartnerID == sprite.RoleID)
                    {
                        return true;
                    }
                }

                /// Nếu đang tuyên chiến cùng
                if (KTGlobal.ActiveFightWith.Contains(sprite.RoleID))
                {
                    return true;
                }

                /// Nếu là trạng thái PK đặc biệt
                if (Global.Data.RoleData.PKMode == (int) PKMode.Custom && targetRD.PKMode == (int) PKMode.Custom)
                {
                    /// Trả về kết quả Camp khác nhau
                    return Global.Data.RoleData.Camp != -1 && targetRD.Camp != -1 && Global.Data.RoleData.Camp != targetRD.Camp;
                }

                /// Nếu cùng tổ đội
                if (KTGlobal.IsTeammate(sprite))
                {
                    return false;
                }
                /// Nếu cùng bang
                if (KTGlobal.IsGuildMate(sprite))
                {
                    return false;
                }

                /// Nếu bản thân đang không phải trong trạng thái Luyện công
                if (Global.Data.RoleData.PKMode != (int) PKMode.Peace)
                {
                    return true;
                }

                /// Không thỏa mãn thì không đánh nhau được
                return false;
            }

            /// Nếu đối phương có Camp -1 thì không đánh được
            if (sprite.Camp == -1)
            {
                return false;
            }

            /// Nếu đối phương là NPC di động
            if (sprite.MonsterData != null && sprite.MonsterData.MonsterType == (int) MonsterTypes.DynamicNPC)
            {
                return false;
            }

            /// Nếu không phải người chơi
            return true;
        }

        /// <summary>
        /// Cập nhật thông tin đội viên
        /// </summary>
        /// <param name="teamMember"></param>
        public static void UpdateTeammateAttributes(TeamMemberAttributes teamMember)
        {
            RoleDataMini rd = Global.Data.Teammates.Where(x => x.RoleID == teamMember.RoleID).FirstOrDefault();
            if (rd != null)
            {
                rd.MapCode = teamMember.MapCode;
                rd.PosX = teamMember.PosX;
                rd.PosY = teamMember.PosY;
                rd.HP = teamMember.HP;
                rd.MaxHP = teamMember.MaxHP;
                rd.AvartaID = teamMember.AvartaID;
                rd.FactionID = teamMember.FactionID;
                rd.Level = teamMember.Level;
            }
        }

        /// <summary>
        /// Kiểm tra đối tượng có phải đồng đội không
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsTeammate(GSprite target)
        {
            /// Nếu là pet
            if (target.PetData != null)
            {
                /// Tìm chủ nhân tương ứng
                target = KTGlobal.FindSpriteByID(target.PetData.RoleID);
                /// Nếu không tìm thấy
                if (target == null)
                {
                    return true;
                }
                /// Nếu là chính mình
                else if (target == Global.Data.Leader)
                {
                    return true;
                }
            }
            /// Nếu là xe tiêu
            else if (target.TraderCarriageData != null)
            {
                /// Tìm chủ nhân tương ứng
                target = KTGlobal.FindSpriteByID(target.TraderCarriageData.OwnerID);
                /// Nếu không tìm thấy
                if (target == null)
                {
                    return true;
                }
                /// Nếu là chính mình
                else if (target == Global.Data.Leader)
                {
                    return true;
                }
            }
            return Global.Data.Teammates.Any(x => x.RoleID == target.RoleID);
        }

        /// <summary>
        /// Kiểm tra đối tượng có phải cùng bang không
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool IsGuildMate(GSprite target)
        {
            /// Nếu là pet
            if (target.PetData != null)
            {
                /// Tìm chủ nhân tương ứng
                target = KTGlobal.FindSpriteByID(target.PetData.RoleID);
                /// Nếu không tìm thấy
                if (target == null)
                {
                    return true;
                }
                /// Nếu là chính mình
                else if (target == Global.Data.Leader)
                {
                    return true;
                }
            }
            /// Nếu là xe tiêu
            else if (target.TraderCarriageData != null)
            {
                /// Tìm chủ nhân tương ứng
                target = KTGlobal.FindSpriteByID(target.TraderCarriageData.OwnerID);
                /// Nếu không tìm thấy
                if (target == null)
                {
                    return true;
                }
                /// Nếu là chính mình
                else if (target == Global.Data.Leader)
                {
                    return true;
                }
            }

            /// Nếu không phải người chơi
            if (!Global.Data.OtherRoles.TryGetValue(target.RoleID, out RoleData rd))
            {
                return false;
            }
            /// Nếu cả hai đều không có bang
            if (rd.GuildID <= 0 && Global.Data.RoleData.GuildID <= 0)
			{
                return false;
			}
            return Global.Data.RoleData.GuildID == rd.GuildID;
        }

        #endregion RelationShip
    }
}
