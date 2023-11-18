using GameServer.KiemThe.GameDbController;
using GameServer.Logic;
using System;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        /// <summary>
        /// Quản lý các công việc ngầm
        /// </summary>
        public static class BackgroundWork
        {
            #region Local update
            /// <summary>
            /// Ghi lại tổng thời gian đã Online
            /// </summary>
            /// <param name="player"></param>
            private static void UpdateOnlineSec(KPlayer player)
            {
                /// Tổng số giây tính từ lúc Update lần cuối
                int totalSecs = (int) ((KTGlobal.GetCurrentTimeMilis() - player.LastUpdateOnlineTimeTicks) / 1000);
                /// Tăng tổng số giây Online trong ngày
                player.DayOnlineSecond += totalSecs;
                /// Tăng tổng số giây đã Online
                player.TotalOnlineSecs += totalSecs;

                /// Phúc lợi đăng nhập ngày
                if (player.RoleWelfareData.logindayid != DateTime.Now.DayOfYear)
                {
                    /// Thực hiện reset online nhận thưởng các kiểu
                    Global.UpdateWelfareRole(player);
                }
            }

            /// <summary>
            /// Thực hiện cập nhật nội bộ dữ liệu tương ứng
            /// </summary>
            /// <param name="client"></param>
            public static void DoLocalUpdate(KPlayer player)
            {
                /// Ghi lại thời gian Online
                BackgroundWork.UpdateOnlineSec(player);

                /// Tự xóa các danh hiệu hết thời hạn
                player.AutoRemoveTimeoutTitles();
                player.AutoRemoveTimeoutSpecialTitle();

                /// Xử lý các sự kiện qua ngày
                KTGlobal.ChangeDayLoginNum(player);

                /// Thực hiện xóa bỏ các vật phẩm hết hạn
                player.GoodsData.RemoveExpiredItems();

                /// Thực hiện update trạng thái các ICON Tới client
                player._IconStateMgr.DoSpriteIconTicks(player);
            }
            #endregion

            #region GameDB Update
            /// <summary>
            /// Thực hiện cập nhật GameDB
            /// </summary>
            /// <param name="client"></param>
            public static void DoGameDBUpdate(KPlayer client)
            {
                /// Lưu các thông khác vào DB
                GameDb.ProcessDBCmdByTicks(client, false);

                /// Lưu thông tin về skill vào DB
                GameDb.ProcessDBSkillCmdByTicks(client, false);

                /// Lưu các thông tin Pram vào DB
                GameDb.ProcessDBRoleParamCmdByTicks(client, false);
            }
            #endregion
        }
    }
}
