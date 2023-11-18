using GameServer.Core.Executor;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        /// <summary>
        /// Thực thi tăng kinh nghiệm cho người chơi
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="experience"></param>
        private static bool ProcessAddExp(KPlayer sprite, long experience)
        {
            long nNeedExp = KPlayerSetting.GetNeedExpUpExp(sprite.m_Level);

            // Nếu số EXP add vào quá số exp yêu cầu thì cho lên cấp
            if (sprite.m_Level < KPlayerSetting.GetMaxLevel() && sprite.m_Experience + experience >= nNeedExp)
            {
                /// Giới hạn cấp
                if (sprite.m_Level >= ServerConfig.Instance.LimitLevel)
                {
                    return false;
                }

                sprite.m_Level += 1;

                experience = sprite.m_Experience + experience - nNeedExp;

                sprite.m_Experience = 0;

                KTPlayerManager.ProcessAddExp(sprite, experience);
            }
            else
            {
                sprite.m_Experience += (int) experience;
                sprite.m_Experience = Math.Max(0, sprite.m_Experience);
            }

            return true;
        }

        /// <summary>
        /// Thêm kinh nghiệm cho người chơi
        /// </summary>
        /// <param name="client"></param>
        /// <param name="experience"></param>
        public static void AddExp(KPlayer client, long experience)
        {

            //if (client.RoleID == 200002)
            //{
            //    Console.WriteLine("ADĐ EXP :" + client.RoleName + "|" + experience);
            //}
            /// Nếu exp < 0 thì bỏ qua
            if (experience <= 0)
            {
                return;
            }

            /// Cấp độ cũ
            int oldLevel = client.m_Level;

            /// Thực hiện tăng kinh nghiệm
            bool ret = KTPlayerManager.ProcessAddExp(client, experience);
            /// Toác
            if (!ret)
            {

                //if (client.RoleID == 200002)
                //{
                //    Console.WriteLine("TOAC ADD EXP :" + client.RoleName + "|" + experience);
                //}
                /// Bỏ qua 
                return;
            }

            /// Nếu 2 cấp độ khác nhau
            if (oldLevel != client.m_Level)
            {
                /// Ghi vào DB
                Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATE_EXPLEVEL, string.Format("{0}:{1}:{2}:{3}", client.RoleID, client.m_Level, client.m_Experience, client.Prestige), client.ServerId);
                long nowTicks = TimeUtil.NOW();
                Global.SetLastDBCmdTicks(client, (int) TCPGameServerCmds.CMD_DB_UPDATE_EXPLEVEL, nowTicks);

                /// Gọi sự kiện thay đổi cấp độ của nhân vật
                client.OnLevelChanged();

                /// Ghi lại logs tăng level cho nhân vật
                KTGlobal.AddRoleUpgradeEvent(client, oldLevel);

                /// Thông báo cho thằng khác cấp độ bản thân thay đổi
                KT_TCPHandler.NotifySelfLevelChanged(client);
            }

         //   KTPlayerManager.ShowNotification(client, "["+DateTime.Now.ToShortTimeString()+"]Nhận EXP :" + experience + "|| Sau khi nhận EXP tặng  :" +  client.m_Experience);
            /// Thông báo về Client
            KT_TCPHandler.NotifySelfExperience(client);
        }

        /// <summary>
        /// Thiết lập cấp độ cho người chơi
        /// </summary>
        public static void SetRoleLevel(KPlayer player, int level)
        {
            int oldLevel = player.m_Level;
            if (level == player.m_Level)
            {
                return;
            }

            Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_UPDATE_EXPLEVEL,
                string.Format("{0}:{1}:{2}:{3}", player.RoleID, level, player.m_Experience, player.Prestige),
                player.ServerId);

            /// Thiết lập cấp độ cho nhân vật
            player.m_Level = level;

            /// Gọi sự kiện thay đổi cấp độ của nhân vật
            player.OnLevelChanged();

            //Ghi lại logs thay đổi level cho nhân vật
            KTGlobal.AddRoleUpgradeEvent(player, oldLevel);

            // NOTIFY EXPX VỀ CHO NGƯỜI CHƠI
            KT_TCPHandler.NotifySelfExperience(player);

            /// Thông báo cho thằng khác cấp độ bản thân thay đổi
            KT_TCPHandler.NotifySelfLevelChanged(player);
        }
    }
}
