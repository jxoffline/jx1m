using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using System.Collections.Generic;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        #region Kick-out
        /// <summary>
        /// Kick người chơi tương ứng ra khỏi Game
        /// </summary>
        /// <param name="player"></param>
        public static void KickOut(KPlayer player)
        {
            /// Ngừng di chuyển
            KTPlayerStoryBoardEx.Instance.Remove(player);
            /// Đóng Socket tương ứng
            TCPManager.getInstance().MySocketListener.CloseSocket(player.ClientSocket);
        }
        #endregion

        #region Gia nhập môn phái, đổi nhánh tu luyện
        /// <summary>
        /// Gia nhập môn phái
        /// </summary>
        /// <param name="player"></param>
        /// <param name="factionID"></param>
        /// <returns>-1: Player NULL, -2: Môn phái không tồn tại, 0: Giới tính không phù hợp, 1: Thành công</returns>
        public static int JoinFaction(KPlayer player, int factionID)
        {
            if (player == null)
            {
                return -1;
            }

            KFaction.KFactionAttirbute faction = KFaction.GetFactionInfo(factionID);
            if (faction == null)
            {
                KTPlayerManager.ShowNotification(player, "Không tìm thấy môn phái tương ứng!");
                return -2;
            }

            if (faction.nSexLimit != -1 && faction.nSexLimit != (int) player.RoleSex)
            {
                KTPlayerManager.ShowNotification(player, "Giới tính của bạn không phù hợp với môn phái này!");
                return 0;
            }

            if (player.m_cPlayerFaction.ChangeFaction(factionID))
            {
                ItemManager.AddFactionItem(player, factionID);
                /// Thông báo về Client môn phái thay đổi
                KT_TCPHandler.NotificationFactionChanged(player);

                /// Hủy các kỹ năng đã thiết lập ở thanh kỹ năng dùng nhanh, lưu vào DB
                player.MainQuickBarKeys = "-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1";
                KT_TCPHandler.SendSaveQuickKeyToDB(player);
                player.OtherQuickBarKeys = "-1_0";
                KT_TCPHandler.SendSaveAruaKeyToDB(player);

                return 1;
            }
            return -99999;
        }

        /// <summary>
        /// Đổi nhánh tu luyện
        /// </summary>
        /// <param name="player"></param>
        public static int ResetAllSkillsLevel(KPlayer player)
        {
            /// Nếu người chơi không tồn tại
            if (player == null)
            {
                return -1;
            }
            /// ID phái
            int factionID = player.m_cPlayerFaction.GetFactionId();
            /// Thông tin phái
            KFaction.KFactionAttirbute faction = KFaction.GetFactionInfo(factionID);
            /// Mếu không tồn tại
            if (faction == null)
            {
                return -2;
            }

            /// Giới tính không phù hợp
            if (faction.nSexLimit != -1 && faction.nSexLimit != (int) player.RoleSex)
            {
                return 0;
            }

            /// Thực hiện tẩy điểm
            if (player.m_cPlayerFaction.ResetAllSkillsPoint())
            {
                /// Thông báo về Client tẩy điểm kỹ năng
                KT_TCPHandler.NotificationFactionChanged(player);

                /// Hủy các kỹ năng đã thiết lập ở thanh kỹ năng dùng nhanh, lưu vào DB
                player.MainQuickBarKeys = "-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1|-1";
                KT_TCPHandler.SendSaveQuickKeyToDB(player);
                player.OtherQuickBarKeys = "-1_0";
                KT_TCPHandler.SendSaveAruaKeyToDB(player);

                return 1;
            }
            else
            {
                return -9999;
            }
        }
        #endregion

        #region Relive
        /// <summary>
        /// Hồi sinh người chơi
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="posX">Vị trí X</param>
        /// <param name="posY">Vị trí Y</param>
        /// <param name="hpPercent">Phục hồi % sinh lực</param>
        /// <param name="mpPercent">Phục hồi % nội lực</param>
        /// <param name="staminaPercent">Phục hồi % thể lực</param>
        /// <returns>-1: Bản đồ không tồn tại, -2: Vị trí hồi sinh không thể đi vào được, 0: Không thể hồi sinh khi % sinh lực dưới 0, 1: Thành công</returns>
        public static void Relive(KPlayer player, int mapCode, int posX, int posY, int hpPercent, int mpPercent, int staminaPercent)
        {
            /// Ngừng di chuyển
            KTPlayerStoryBoardEx.Instance.Remove(player);

            /// Hủy trạng thái khinh công
            player.StopBlink();

            /// Bản đồ hồi sinh
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (gameMap == null)
            {
                return;
            }

            /// Chuyển sang tọa độ lưới
            UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(gameMap, new UnityEngine.Vector2(posX, posY));

            /// Kiểm tra vị trí điểm hồi sinh có nằm trong vùng Block không
            if (!gameMap.CanMove((int) gridPos.x, (int) gridPos.y, player.CurrentCopyMapID))
            {
                return;
            }

            /// Nếu % máu dưới 1 thì không thể hồi sinh được
            if (hpPercent <= 0)
            {
                return;
            }

            /// Thằng này còn sống không
            bool isAlive = !player.IsDead();

            int hp = player.m_CurrentLifeMax * hpPercent / 100;
            int mp = player.m_CurrentManaMax * mpPercent / 100;
            int stamina = player.m_CurrentStaminaMax * staminaPercent / 100;

            /// Thiết lập máu của người chơi
            player.m_CurrentLife = hp;
            player.m_CurrentMana = mp;
            player.m_CurrentStamina = stamina;
            /// Thực hiện động tác đứng
            player.m_eDoing = KE_NPC_DOING.do_stand;

            /// Nếu ID bản đồ đích trùng với ID bản đồ hiện tại
            if (mapCode == player.CurrentMapCode)
            {
                /// Nếu nó còn sống
                if (isAlive)
                {
                    /// Thực hiện chuyển vị trí
                    KTPlayerManager.ChangePos(player, posX, posY);
                    /// Bỏ qua
                    return;
                }

                /// Thông báo cho bọn xung quanh (vị trí cũ) xóa thằng này
                KTPlayerManager.NotifyMyselfLeaveOthers(player);

                /// Set lại tọa độ mới cho người chơi sau khi hồi sinh
                player.PosX = posX;
                player.PosY = posY;

                /// Cập nhật vào MapGrid
                gameMap.Grid.MoveObject(posX, posY, player);

                /// Thông báo bản thân sống lại (ở vị trí mới)
                KT_TCPHandler.NotifyMyselfRelive(player, true, true);
            }
            else
            {
                /// Nếu nó còn sống
                if (isAlive)
                {
                    /// Thực hiện chuyển bản đồ
                    KTPlayerManager.ChangeMap(player, mapCode, posX, posY);
                    /// Bỏ qua
                    return;
                }

                /// Thông báo cho bọn xung quanh (vị trí cũ) xóa thằng này
                KTPlayerManager.NotifyMyselfLeaveOthers(player);

                /// Thông báo bản thân sống lại
                KT_TCPHandler.NotifyMyselfRelive(player, true, false);

                /// Thực hiện dịch chuyển đến bản đồ chỉ định
                KTPlayerManager.ChangeMap(player, mapCode, posX, posY);
            }

            /// Thực hiện thao tác sống lại
            player.DoRelive();
            /// Thực hiện sự kiện hồi sinh
            player.OnRevive();

            return;
        }
        #endregion

        #region Kinh nghiệm
        /// <summary>
        /// Thông báo kinh nghiệm ủy thác Bạch Câu Hoàn
        /// </summary>
        /// <param name="player"></param>
        /// <param name="EXPGAIN"></param>
        /// <param name="EXPNORMAL"></param>
        /// <param name="EXPPRO"></param>
        /// <param name="MINPRO"></param>
        /// <param name="MINNORMAL"></param>
        /// <param name="OFFLINEMIN"></param>
        public static void NotifyBCH(KPlayer player, double EXPGAIN, double EXPNORMAL, double EXPPRO, int MINPRO, int MINNORMAL, int OFFLINEMIN)
        {
            KTPlayerManager.AddExp(player, (long) EXPGAIN);

            string NOTIFY = "Bạn đã offline:" + KTGlobal.CreateStringByColor(OFFLINEMIN + "", ColorType.Importal) + " phút!\n\nSố phút ủy thác " + KTGlobal.CreateStringByColor("ĐẠI BẠCH CẦU HOÀN", ColorType.Done) + " là : " + KTGlobal.CreateStringByColor(MINPRO + "", ColorType.Done) + " với " + KTGlobal.CreateStringByColor(EXPPRO + "", ColorType.Done) + " EXP \n\nSố phút ủy thác " + KTGlobal.CreateStringByColor("BẠCH CẦU HOÀN", ColorType.Done) + " là : " + KTGlobal.CreateStringByColor(MINNORMAL + "", ColorType.Done) + " với " + KTGlobal.CreateStringByColor(EXPNORMAL + "", ColorType.Done) + " Exp\n\nTổng số EXP nhận :" + KTGlobal.CreateStringByColor(EXPGAIN + "", ColorType.Importal) + "\n\nSố phút ủy thác " + KTGlobal.CreateStringByColor("ĐẠI BẠCH CẦU HOÀN", ColorType.Done) + " còn lại là :" + KTGlobal.CreateStringByColor(player.baijuwanpro + "", ColorType.Accpect) + "\n\nSố phút ủy thác " + KTGlobal.CreateStringByColor("BẠCH CẦU HOÀN", ColorType.Done) + " còn lại là :" + KTGlobal.CreateStringByColor(player.baijuwan + "", ColorType.Accpect);

            KT_TCPHandler.OpenNPCDialog(player, 1000, 1000, NOTIFY, new Dictionary<int, string>(), null, false, "", new Dictionary<int, string>());
        }
        #endregion
    }
}
