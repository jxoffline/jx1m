using GameServer.Interface;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý bot biểu diễn
    /// </summary>
    public static partial class KTDecoBotManager
    {
        #region Quản lý Pet
        /// <summary>
        /// Thêm Bot
        /// </summary>
        /// <param name="bot"></param>
        /// <returns></returns>
        public static void Add(KDecoBot bot)
        {
            /// Lưu lại vị trí đứng ban đầu
            bot.InitPos = bot.CurrentPos;
            /// Thêm vào danh sách
            KTDecoBotManager.bots[bot.RoleID] = bot;
            /// Bản đồ
            GameMap gameMap = KTMapManager.Find(bot.CurrentMapCode);
            /// Thêm vào MapGrid
            gameMap.Grid.MoveObject((int) bot.CurrentPos.X, (int) bot.CurrentPos.Y, bot);
        }

        /// <summary>
        /// Xóa bot tương ứng
        /// </summary>
        /// <param name="bot"></param>
        public static void Remove(KDecoBot bot)
        {
            /// Toác
            if (bot == null)
            {
                return;
            }

            /// Reset
            bot.Reset(false);
            /// Ngừng luồng Bot
            KTDecoBotTimerManager.Instance.Remove(bot);

            /// Xóa khỏi danh sách
            KTDecoBotManager.bots.TryRemove(bot.RoleID, out _);
        }
        #endregion

        #region Hiển thị
        /// <summary>
        /// Xử lý khi đối tượng bot biểu diễn được tải xuống thành công
        /// </summary>
        /// <param name="client"></param>
        /// <param name="otherClient"></param>
        public static void HandleDecoBotLoaded(KPlayer client, KDecoBot bot)
        {
            string pathString = KTBotStoryBoardEx.Instance.GetCurrentPathString(bot);
            LoadAlreadyData data = new LoadAlreadyData()
            {
                RoleID = bot.RoleID,
                PosX = (int) bot.CurrentPos.X,
                PosY = (int) bot.CurrentPos.Y,
                Direction = (int) bot.CurrentDir,
                Action = (int) bot.m_eDoing,
                PathString = pathString,
                ToX = (int) bot.ToPos.X,
                ToY = (int) bot.ToPos.Y,
                Camp = bot.Camp,
            };
            client.SendPacket<LoadAlreadyData>((int) TCPGameServerCmds.CMD_SPR_LOADALREADY, data);
        }
        #endregion
    }
}
