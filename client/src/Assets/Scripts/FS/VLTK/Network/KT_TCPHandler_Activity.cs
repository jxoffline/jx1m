using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System.Collections.Generic;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Đua top
        /// <summary>
        /// Gửi yêu cầu truy vấn danh sách đua top
        /// </summary>
        public static void SendGetTopRankingInfo()
        {
            string cmdData = "";
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_TOPRANKING_INFO)));
        }

        /// <summary>
        /// Nhận gói tin danh sách đua top
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveTopRankingInfo(int cmdID, byte[] data, int length)
        {
            List<TopRankingConfig> topRankingList = DataHelper.BytesToObject<List<TopRankingConfig>>(data, 0, length);
            /// Toác
            if (topRankingList == null)
            {
                KTGlobal.AddNotification("Lấy thông tin đua top thất bại. Hãy thử lại sau!");
                return;
            }

            /// Nếu đang hiện UI
            if (PlayZone.Instance.UITopRanking != null)
            {
                /// Cập nhật dữ liệu
                PlayZone.Instance.UITopRanking.Data = topRankingList;
            }
            /// Nếu chưa hiện UI
            else
            {
                /// Hiện khung
                PlayZone.Instance.ShowUITopRanking(topRankingList);
            }
        }

        /// <summary>
        /// Gửi yêu cầu nhận thưởng đua top
        /// </summary>
        /// <param name="rankType"></param>
        /// <param name="awardIndex"></param>
        public static void SendGetTopRankingAward(int rankType, int awardIndex)
        {
            string cmdData = string.Format("{0}:{1}", rankType, awardIndex);
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_TOPRANKING_GETAWARD)));
        }

        /// <summary>
        /// Nhận gói tin nhận thưởng đua top
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveGetTopRankingAward(string[] fields)
        {
            /// Loại
            int rankType = int.Parse(fields[0]);
            /// ID phần quà
            int awardIndex = int.Parse(fields[1]);

            /// Nếu đang hiện khung
            if (PlayZone.Instance.UITopRanking != null)
            {
                /// Cập nhật
                PlayZone.Instance.UITopRanking.UpdateState(rankType, awardIndex);
            }
        }
        #endregion

        #region Vận tiêu
        /// <summary>
        /// Nhận gói tin thông báo có nhiệm vụ vận tiêu mới
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveTakeNewCargoCarriageTask(int cmdID, byte[] data, int length)
        {
            /// Dữ liệu tương ứng
            G2C_CargoCarriageTaskData taskData = DataHelper.BytesToObject<G2C_CargoCarriageTaskData>(data, 0, length);
            /// Toác
            if (taskData == null)
            {
                /// Hủy khung
                PlayZone.Instance.CloseUICargoCarriageTaskInfo();
                /// Bỏ qua
                return;
            }

            /// Thực thi hiệu ứng nhận nhiệm vụ
            KTGlobal.PlayRoleReceiveQuestEffect();

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UICargoCarriageTaskInfo == null)
            {
                /// Mở khung
                PlayZone.Instance.OpenUICargoCarriageTaskInfo(taskData);
            }
            /// Nếu đã mở khung
            else
            {
                /// Cập nhật dữ liệu
                PlayZone.Instance.UICargoCarriageTaskInfo.Data = taskData;
            }
        }

        /// <summary>
        /// Nhận gói tin thông báo cập nhật trạng thái nhiệm vụ vận tiêu
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveUpdateCargoCarriageTaskState(string[] fields)
        {
            /// Trạng thái
            int state = int.Parse(fields[0]);
            /// Nếu là đã hoàn thành
            if (state == 1)
            {
                /// Thực thi hiệu ứng hoàn thành nhiệm vụ
                KTGlobal.PlayRoleCompleteQuestEffect();

                /// Nếu đã mở khung
                if (PlayZone.Instance.UICargoCarriageTaskInfo != null)
                {
                    /// Cập nhật trạng thái
                    PlayZone.Instance.UICargoCarriageTaskInfo.Completed = true;
                }
            }
            /// Nếu là hủy
            else if (state == 0)
            {
                /// Hủy khung
                PlayZone.Instance.CloseUICargoCarriageTaskInfo();
            }
            /// Nếu là bắt đầu
            else if (state == 2)
            {
                /// Số giây còn lại
                int totalSec = int.Parse(fields[1]);
                /// Nếu đã mở khung
                if (PlayZone.Instance.UICargoCarriageTaskInfo != null)
                {
                    /// Cập nhật thời gian
                    PlayZone.Instance.UICargoCarriageTaskInfo.TimeLeft = totalSec;
                }
            }
        }
        #endregion
    }
}
