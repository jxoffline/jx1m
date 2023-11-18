using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Repute;
using GameServer.KiemThe.GameEvents.Model;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.FengHuoLianCheng
{
    /// <summary>
    /// Script sự kiện Phong Hỏa Liên Thành
    /// </summary>
    public class FengHuoLianCheng_ActivityScript : IActivityScript
    {
        #region Define
        /// <summary>
        /// Hoạt động tương ứng
        /// </summary>
        public KTActivity Activity { get; set; }

        /// <summary>
        /// Script tương ứng
        /// </summary>
        public FengHuoLianCheng_Script_Main Script { get; private set; } = null;
        #endregion

        #region Public methods
        /// <summary>
        /// Trả về thứ hạng sự kiện lần trước của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int GetLastEventRank(KPlayer player)
        {
            return player.GetValueOfForeverRecore(ForeverRecord.FengHuoLianCheng_LastRank);
        }

        /// <summary>
        /// Thiết lập thứ hạng sự kiện lần trước của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="rank"></param>
        public static void SetLastEventRank(KPlayer player, int rank)
        {
            player.SetValueOfForeverRecore(ForeverRecord.FengHuoLianCheng_LastRank, rank);
        }

        /// <summary>
        /// Có phần quà sự kiện lần trước có thể nhận không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool HasAward(KPlayer player)
        {
            /// Thứ hạng
            int rank = FengHuoLianCheng_ActivityScript.GetLastEventRank(player);
            /// Nếu không có
            if (rank == -1)
            {
                /// Toác
                return false;
            }

            /// Thông tin quà tương ứng
            FengHuoLianCheng.FHLCData.AwardInfo awardInfo = FengHuoLianCheng.Data.Awards.Where(x => x.FromRank <= rank && rank <= x.ToRank).FirstOrDefault();
            /// Nếu không tồn tại
            if (awardInfo == null)
            {
                /// Hủy thứ hạng
                FengHuoLianCheng_ActivityScript.SetLastEventRank(player, -1);
                /// Toác
                return false;
            }
            /// OK
            return true;
        }

        /// <summary>
        /// Nhận phần thưởng sự kiện lần trước
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string GetAward(KPlayer player)
        {
            /// Nếu không có quà
            if (!FengHuoLianCheng_ActivityScript.HasAward(player))
            {
                /// Toác
                return "Ngươi không có quà để nhận.";
            }

            /// Thứ hạng lần trước
            int rank = FengHuoLianCheng_ActivityScript.GetLastEventRank(player);
            /// Thông tin quà tương ứng
            FengHuoLianCheng.FHLCData.AwardInfo awardInfo = FengHuoLianCheng.Data.Awards.Where(x => x.FromRank <= rank && rank <= x.ToRank).FirstOrDefault();

            /// Tổng số ô trống trong túi cần
            int spacesNeed = 0;
            /// Duyệt danh sách vật phẩm thưởng
            foreach (FengHuoLianCheng.FHLCData.AwardInfo.AwardItemData awardItem in awardInfo.AwardItems)
            {
                /// Tăng số ô trống cần
                spacesNeed += KTGlobal.GetTotalSpacesNeedToTakeItem(awardItem.ItemID, awardItem.Quantity);
            }

            /// Nếu không đủ ô trống
            if (!KTGlobal.IsHaveSpace(spacesNeed, player))
            {
                /// Toác
                return string.Format("Túi đồ cần để trống <color=yellow>{0} ô</color> để có thể nhận thưởng.", spacesNeed);
            }

            /// Hủy thứ hạng
            FengHuoLianCheng_ActivityScript.SetLastEventRank(player, -1);

            /// Thêm danh vọng
			KTGlobal.AddRepute(player, awardInfo.ReputeID, awardInfo.ReputePoint);

            /// Gửi tin nhắn thông báo
            KTGlobal.SendDefaultChat(player, string.Format("Nhận <color=yellow>{0}</color> điểm danh vọng Nghĩa quân.", awardInfo.ReputePoint));

            /// Duyệt danh sách phần thưởng
            foreach (FengHuoLianCheng.FHLCData.AwardInfo.AwardItemData awardItem in awardInfo.AwardItems)
            {
                /// Thêm quà tương ứng
                ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, awardItem.ItemID, awardItem.Quantity, awardItem.Bound ? 1 : 0, "FengHuoLianCheng_ActivityScript", true, 0, false, ItemManager.ConstGoodsEndTime, "", -1);
            }

            /// OK
            return "OK";
        }

        /// <summary>
        /// Kiểm tra hiện có phải thời gian báo danh sự kiện không
        /// </summary>
        /// <returns></returns>
        public static bool IsRegistrationTime()
        {
            return FengHuoLianCheng.IsRegisterTime;
        }

        /// <summary>
        /// Đăng ký tham gia sự kiện
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string RegisterEvent(KPlayer player)
        {
            /// Nếu có phần quà chưa nhận
            if (FengHuoLianCheng_ActivityScript.HasAward(player))
            {
                return "Ngươi có phần quà lần trước chưa nhận, hãy nhận thưởng trước sau đó đăng ký báo danh sự kiện lần này.";
            }
            /// Nếu không phải thời gian diễn ra sự kiện
            else if (!FengHuoLianCheng_ActivityScript.IsRegistrationTime())
            {
                return "Hiện không phải thời gian diễn ra sự kiện <color=yellow>[Phong Hỏa Liên Thành]</color>, hãy quay lại sau.";
            }

            /// OK
            return "OK";
        }

        /// <summary>
        /// Đưa người chơi đến khu vực chuẩn bị sự kiện
        /// </summary>
        /// <param name="player"></param>
        public static void BringToBattleOutpost(KPlayer player)
        {
            KTPlayerManager.ChangeMap(player, FengHuoLianCheng.Data.Map.EnterMapID, FengHuoLianCheng.Data.Map.EnterPosX, FengHuoLianCheng.Data.Map.EnterPosY);
        }
        #endregion

        #region Core ActivityScript
        /// <summary>
        /// Bắt đầu sự kiện
        /// </summary>
        public void Begin()
        {
            /// Nếu không có thông tin hoạt động
            if (this.Activity == null)
            {
                return;
            }

            /// Loại gì
            switch (this.Activity.Data.ID)
            {
                /// Thông báo báo danh
                case 600:
                {
                    /// Thông báo
                    KTGlobal.SendSystemEventNotification("Sự kiện Phong Hỏa Liên Thành sẽ bắt đầu sau 10 phút. Hãy đến tìm [Vệ binh thành môn] ở các thành thị để đăng ký báo danh.");
                    /// Đánh dấu đã bắt đầu báo danh
                    FengHuoLianCheng.IsRegisterTime = true;

                    break;
                }
                /// Bắt đầu sự kiện
                case 601:
                {
                    /// Hủy đánh dấu đã bắt đầu báo danh
                    FengHuoLianCheng.IsRegisterTime = false;

                    /// Tổng số người chơi đang có trong bản đồ
                    int totalPlayers = KTPlayerManager.GetPlayersCount(FengHuoLianCheng.Data.Map.EnterMapID);
                    /// Nếu tổng số người không đủ
                    if (totalPlayers < FengHuoLianCheng.Data.Config.MinPlayers)
                    {
                        /// Thông báo
                        KTGlobal.SendSystemEventNotification("Sự kiện Phong Hỏa Liên Thành hôm nay không thể bắt đầu do số lượng người tham gia không đủ.");
                        /// Hủy sự kiện
                        KTActivityManager.StopActivity(this.Activity.Data.ID);
                        /// Duyệt danh sách người chơi đang trong bản đồ sự kiện
                        foreach (KPlayer player in KTPlayerManager.FindAll(FengHuoLianCheng.Data.Map.EnterMapID))
                        {
                            /// Đưa người chơi rời khỏi bản đồ hoạt động
                            KTPlayerManager.ChangeMap(player, FengHuoLianCheng.Data.Map.CityMapID, FengHuoLianCheng.Data.Map.CityPosX, FengHuoLianCheng.Data.Map.CityPosY);
                        }
                        /// Bỏ qua
                        return;
                    }

                    /// Thông báo
                    KTGlobal.SendSystemEventNotification("Sự kiện Phong Hỏa Liên Thành hôm nay đã bắt đầu. Hãy chiến đấu hết mình bảo vệ nguyên soái để lập công.");

                    /// Thông tin bản đồ tương ứng
                    GameMap map = KTMapManager.Find(FengHuoLianCheng.Data.Map.EnterMapID);
                    /// Khởi tạo Script tương ứng
                    this.Script = new FengHuoLianCheng_Script_Main(map, this.Activity);
                    /// Bắt đầu thực thi
                    this.Script.Begin();

                    break;
                }
            }
        }

        /// <summary>
        /// Kết thúc sự kiện
        /// </summary>
        public void Close()
        {
            /// Nếu không có thông tin hoạt động
            if (this.Activity == null)
            {
                return;
            }

            /// Nếu là sự kiện chính
            if (this.Activity.Data.ID == 601)
            {
                
                /// Hủy Script
                this.Script?.Dispose();
            }
        }
        #endregion
    }
}
