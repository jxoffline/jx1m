using GameServer.Interface;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý quái
    /// </summary>
    public static partial class KTMonsterManager
    {
        #region Hiển thị
        /// <summary>
        /// Xử lý khi đối tượng quái được tải xuống thành công
        /// </summary>
        /// <param name="client"></param>
        /// <param name="otherClient"></param>
        public static void HandleMonsterLoaded(KPlayer client, Monster monster)
        {
            string pathString = KTMonsterStoryBoardEx.Instance.GetCurrentPathString(monster);
            /// Dữ liệu
            LoadAlreadyData data = new LoadAlreadyData()
            {
                RoleID = monster.RoleID,
                PosX = (int) monster.CurrentPos.X,
                PosY = (int) monster.CurrentPos.Y,
                Direction = (int) monster.CurrentDir,
                Action = (int) monster.m_eDoing,
                PathString = pathString,
                ToX = (int) monster.ToPos.X,
                ToY = (int) monster.ToPos.Y,
                Camp = monster.Camp,
            };

            /// Gửi gói tin
            client.SendPacket<LoadAlreadyData>((int) TCPGameServerCmds.CMD_SPR_LOADALREADY, data);
        }

        /// <summary>
        /// Thông báo quái tái sinh
        /// </summary>
        /// <param name="obj"></param>
        public static void NotifyMonsterRelive(Monster obj)
        {
            /// Danh sách người chơi xung quanh
            List<KPlayer> players = KTRadarMapManager.GetPlayersAround(obj);
            /// Toác
            if (players == null)
            {
                /// Bỏ qua
                return;
            }

            /// Dữ liệu
            MonsterRealiveData monsterRealiveData = new MonsterRealiveData()
            {
                RoleID = obj.RoleID,
                PosX = (int) obj.CurrentPos.X,
                PosY = (int) obj.CurrentPos.Y,
                Direction = (int) obj.CurrentDir,
                Series = (int) obj.m_Series,
                CurrentHP = obj.m_CurrentLife,
            };

            KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_REALIVE, players, DataHelper.ObjectToBytes<MonsterRealiveData>(monsterRealiveData), obj, null);
        }
        #endregion
    }
}
