using GameServer.Entities.Skill.Other;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.GameEvents.SpecialEvent;
using GameServer.Logic;
using GameServer.VLTK.Core.GuildManager;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        public static void Tick()
        {
            /// Tick khu vực quái
            KTMonsterManager.MonsterZone_Tick();
            /// Tick quái di động
            KTMonsterManager.DynamicMonster_Tick();
            /// Tick boss thế giới
            KTMonsterManager.WorldBoss_Tick();
        }

        /// <summary>
        /// Sự kiện khi quái chết
        /// </summary>
        /// <param name="monster"></param>
        public static void OnMonsterDie(Monster monster)
        {
            /// Bản đồ
            GameMap gameMap = KTMapManager.Find(monster.MapCode);
            /// Xóa khỏi MapGrid
            gameMap.Grid.RemoveObject(monster);

            /// Hủy đối tượng
            monster.Dispose();

            /// Nếu có khu quản lý
            if (monster.MonsterZoneID != -1)
            {
                /// Thông tin khu
                if (KTMonsterManager.zones.TryGetValue(monster.MapCode, out Dictionary<int, MonsterZone> zones))
                {
                    if (zones.TryGetValue(monster.MonsterZoneID, out MonsterZone zone))
                    {
                        /// Thực thi sự kiện
                        zone.OnMonsterDie(monster);
                    }
                }
                /// Bỏ qua
                return;
            }

            /// Nếu không tái sinh
            if (monster.DynamicRespawnTicks == -1)
            {
                /// Xóa luôn
                KTMonsterManager.Remove(monster);
                /// Bỏ qua
                return;
            }

            /// Thêm vào danh sách tái sinh
            KTMonsterManager.AddDynamicMonsterWaitToRelive(monster);
        }

        /// <summary>
        /// Thực thi sự kiện người chơi giết quái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="monster"></param>
        public static void ProcessKillMonster(KPlayer player, Monster monster)
        {
            try
            {
                /// Tỷ lệ kinh nghiệm nhân lên
                int expMultiply = 1;
                /// Nếu là Boss thế giới
                if (monster.Tag == "WorldBoss")
                {
                    /// Lấy thông tin đối tượng gây sát thương nhiều nhất
                    Monster.DamgeGroup topDamage = monster.GetTopDamage();
                    /// Nếu tìm thấy
                    if (topDamage != null)
                    {
                        /// Nếu có sát thương
                        if (topDamage.TotalDamage > 0)
                        {
                            /// Nếu là cá nhân
                            if (!topDamage.IsTeam)
                            {
                                player = KTPlayerManager.Find(topDamage.ID);
                            }
                            /// Nếu là nhóm
                            else
                            {
                                player = KTTeamManager.GetTeamLeader(topDamage.ID);
                            }
                        }
                    }

                    /// Xử lý khi Boss thế giới chết
                    WorldBossManager.OnBossDie(player, monster);
                    /// Tỷ lệ kinh nghiệm nhân lên
                    expMultiply = 250;
                }

                /// Kinh nghiệm cơ bản nhận được
                int nExp = MonsterUtilities.GetExpByLevel(monster.m_Level) * expMultiply;
                /// Giá trị kinh nghiệm thằng này nhận được (dùng để tính toán rơi tiền)
                int finalExp = nExp;

                /// Nếu có nhóm
                if (player.TeamID != -1)
                {
                    /// Danh sách thành viên nhóm
                    List<KPlayer> teammates = player.Teammates;

                    /// Kinh nghiệm chia cho số thành viên đội
                    float nExpRate = (100 - (teammates.Count - 1) * 8) / 100f;
                    /// Kinh nghiệm thằng dứt điểm sẽ nhận được
                    float nMainExp = nExp * nExpRate;
                    /// Kinh nghiệm nhận được nếu là đồng đội dứt điểm
                    float nExpShare = nMainExp * KTGlobal.KD_MAX_TEAMATE_EXP_SHARE / 100f;

                    /// Duyệt danh sách thành viên đội
                    foreach (KPlayer member in teammates)
                    {
                        /// Nếu khác bản đồ
                        if (member.MapCode != player.MapCode)
                        {
                          
                            /// Bỏ qua
                            continue;
                        }
                        /// Nếu mà không trong phạm vi quái chết
                        else if (KTGlobal.GetDistanceBetweenPoints(member.CurrentPos, monster.CurrentPos) > 1000)
                        {
                          
                            /// Bỏ qua
                            continue;
                        }
                        /// Nếu đã chết
                        else if (member.IsDead())
                        {
                          
                            /// Bỏ qua
                            continue;
                        }

                        /// Kinh nghiệm bản thân nhận được
                        int expGet;
                        /// Nếu không phải thằng dứt điểm quái
                        if (member.RoleID != player.RoleID)
                        {
                            expGet = MonsterUtilities.CalculatePlayerExpByMonsterLevel((int) nExpShare, member.m_Level, monster.m_Level);
                            /// Kinh nghiệm nhận được từ đồng đội khi đánh quái
                            if (member.m_nShareExpP > 0)
                            {
                                /// Cộng thêm lượng kinh nghiệm tương ứng từ đồng đội khi đánh quái
                                expGet += expGet * member.m_nShareExpP / 100;
                            }

                         
                        }
                        /// Nếu là thằng dứt điểm quái
                        else
                        {
                            expGet = MonsterUtilities.CalculatePlayerExpByMonsterLevel((int) nMainExp, member.m_Level, monster.m_Level);
                        }

                        /// Nếu đây là bản thân
                        if (member.RoleID == player.RoleID)
                        {
                            /// Lưu lại giá trị kinh nghiệm nó nhận được (không tính Buff)
                            finalExp = expGet;
                        }

                        /// Kinh nghiệm cộng thêm từ buff các kiểu
                        expGet += expGet * member.m_nExpEnhancePercent / 100;
                        expGet += expGet * member.m_nExpAddtionP / 100;

                     
                        /// Thêm kinh nghiệm cho người chơi tương ứng
                        KTPlayerManager.AddExp(member, expGet);

                        /// Thực hiện thăng cấp kỹ năng nếu có
                        KSkillExp.ProcessSkillExpGain(member, monster, expGet);

                        /// Thực hiện thêm kinh nghiệm cho pet hiện tại
                        KTPetManager.ProcessPetExpGainByPlayerKillMonster(member, expGet);

                        /// Thực thi kiểm tra nhiệm vụ khi giết quái
                        ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, member, monster.RoleID, monster.MonsterInfo.Code, -1, TaskTypes.KillMonster);
                    }
                }
                /// Nếu không có nhóm
                else
                {
                    /// Kinh nghiệm sẽ nhận được
                    float nMainExp = nExp;

                    /// Tính toán lại theo cấp độ quái
                    int expGet = MonsterUtilities.CalculatePlayerExpByMonsterLevel((int) nMainExp, player.m_Level, monster.m_Level);

                    /// Lưu lại giá trị kinh nghiệm nó nhận được (không tính Buff)
                    finalExp = expGet;

                    /// Kinh nghiệm cộng thêm từ buff các kiểu
                    expGet += expGet * player.m_nExpEnhancePercent / 100;
                    expGet += expGet * player.m_nExpAddtionP / 100;

                    /// Nếu còn sống
                    if (!player.IsDead())
                    {
                        /// Thêm kinh nghiệm cho người chơi tương ứng
                        KTPlayerManager.AddExp(player, expGet);

                        /// Thực hiện thăng cấp kỹ năng nếu có
                        KSkillExp.ProcessSkillExpGain(player, monster, expGet);

                        /// Thực hiện thêm kinh nghiệm cho pet hiện tại
                        KTPetManager.ProcessPetExpGainByPlayerKillMonster(player, expGet);

                        /// Thực thi kiểm tra nhiệm vụ khi giết quái
                        ProcessTask.Process(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, player, monster.RoleID, monster.MonsterInfo.Code, -1, TaskTypes.KillMonster);

                        /// Nếu thằng này có bang hội
                        if (player.GuildID > 0)
                        {
                            /// Thực thi kiểm tra nhiệm vụ bang hội khi giết quái
                            GuildManager.getInstance().TaskProsecc(player.GuildID, player, monster.MonsterInfo.Code, TaskTypes.KillMonster);
                        }
                    }
                }

                /// Thực hiện rơi vật phẩm
                MonsterDropManager.ProcessMonsterDrop(monster, player);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }

            try
            {
                /// Thực thi hàm CallBack khi quái chết ở sự kiện đặc biệt
                SpecialEvent_Logic.ProcessMonsterDie(player, monster);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
    }
}
