using GameServer.KiemThe.GameEvents.Interface;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.KnowledgeChallenge
{
    /// <summary>
    /// Script chính thực thi Logic sự kiện Đoán hoa đăng
    /// </summary>
    public class KnowledgeChallenge_Script_Main : GameMapEvent
    {
        #region Define
        /// <summary>
        /// Danh sách các vị trí của NPC trong bản đồ
        /// </summary>
        public List<UnityEngine.Vector2Int> RandomNPCPositions { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng NPC đứng yên tại chỗ
        /// </summary>
        private NPC IdeNPC;

        /// <summary>
        /// Đối tượng NPC di chuyển
        /// </summary>
        private Monster MovingNPC;

        /// <summary>
        /// Vị trí tiếp theo NPC sẽ di chuyển tới
        /// </summary>
        private UnityEngine.Vector2Int? NextMoveToPos = null;

        /// <summary>
        /// Vị trí lần trước NPC đứng
        /// </summary>
        private UnityEngine.Vector2Int LastRandomIdePos;

        /// <summary>
        /// Thời điểm lần trước NPC đứng
        /// </summary>
        private long LastNPCIdeTicks = 0;
        #endregion

        #region Core GameMapEvent
        /// <summary>
        /// Script chính thực thi Logic sự kiện Đoán hoa đăng
        /// </summary>
        /// <param name="map"></param>
        /// <param name="activity"></param>
        public KnowledgeChallenge_Script_Main(GameMap map, KTActivity activity) : base(map, activity)
        {
            /// Đánh dấu thời điểm lần trước NPC đứng
            this.LastNPCIdeTicks = KTGlobal.GetCurrentTimeMilis();
        }

        /// <summary>
        /// Sự kiện bắt đầu
        /// </summary>
        protected override void OnStart()
        {
            /// Xóa NPC cũ
            this.RemoveNPC();
            /// Vị trí xuất hiện
            UnityEngine.Vector2Int randomPos = this.GetRandomNPCPos();
            /// Tạo NPC
            this.CreateIdeNPC(randomPos);
            /// Vị trí trên tọa độ lưới
            UnityEngine.Vector2Int gridPos = KTGlobal.WorldPositionToGridPosition(this.Map, randomPos);
            /// Thông báo vị trí xuất hiện
            KTGlobal.SendSystemChat(string.Format("<color=yellow>[Nhan Như Ngọc]</color> đã xuất hiện tại <color=#3dfff9><link=\"GoTo_{0}_{1}_{2}\">[{3} ({4}, {5})]</link></color>", this.Map.MapCode, randomPos.x, randomPos.y, this.Map.MapName, gridPos.x, gridPos.y));
        }

        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        protected override void OnTick()
        {
            /// Nếu đang có NPC đứng
            if (this.IdeNPC != null)
            {
                /// Nếu thời điểm đứng chưa đủ
                if (KTGlobal.GetCurrentTimeMilis() - this.LastNPCIdeTicks <= KnowledgeChallenge.Event.Config.NPCStandDuration)
                {
                    return;
                }
                /// Tạo NPC di chuyển
                this.CreateMovingNPC(this.GetRandomNPCPos());
            }
            /// Nếu không có NPC đứng
            else
            {
                /// Thử tạo NPC đứng ở vị trí tương ứng
                this.TryCreateIdeNPC();
            }
        }

        /// <summary>
        /// Sự kiện kết thúc
        /// </summary>
        protected override void OnClose()
        {
            /// Xóa NPC
            this.RemoveNPC();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Trả về tổng số câu hỏi đã được hỏi hôm nay
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private int GetTotalQuestionsToday(KPlayer player)
        {
            int ret = player.GetValueOfDailyRecore((int) DailyRecord.KnowledgeChallenge_TotalQuestions);
            if (ret < 0)
            {
                ret = 0;
            }
            return ret;
        }

        /// <summary>
        /// Thiết lập tổng số câu hỏi đã được hỏi hôm nay
        /// </summary>
        /// <param name="player"></param>
        /// <param name="count"></param>
        private void SetTotalQuestionsToday(KPlayer player, int count)
        {
            player.SetValueOfDailyRecore((int) DailyRecord.KnowledgeChallenge_TotalQuestions, count);
        }

        /// <summary>
        /// Thử tạo NPC ở vị trí đích di chuyển đến
        /// </summary>
        /// <returns></returns>
        private void TryCreateIdeNPC()
        {
            /// Nếu không có NPC di chuyển
            if (this.MovingNPC == null)
            {
                return;
            }
            /// Nếu có vị trí đích đến
            else if (this.NextMoveToPos != null && this.NextMoveToPos.HasValue)
            {
                /// Vị trí hiện tại
                UnityEngine.Vector2Int currentPos = new UnityEngine.Vector2Int((int) this.MovingNPC.CurrentPos.X, (int) this.MovingNPC.CurrentPos.Y);
                /// Vị trí đích đến
                UnityEngine.Vector2Int destNPCPos = this.NextMoveToPos.Value;
                /// Nếu chưa đến đích
                if (UnityEngine.Vector2Int.Distance(currentPos, destNPCPos) > 10)
                {
                    /// Bỏ qua
                    return;
                }

                /// Tạo NPC đứng ở vị trí tương ứng
                this.CreateIdeNPC(destNPCPos);

                /// Bỏ qua
                return;
            }
            /// Nếu vẫn còn đang di chuyển
            else if (this.MovingNPC.IsMoving)
            {
                return;
            }
        }

        /// <summary>
        /// Tạo NPC đứng yên tại vị trí tương ứng
        /// </summary>
        /// <param name="pos"></param>
        private void CreateIdeNPC(UnityEngine.Vector2Int pos)
        {
            /// Xóa NPC cũ
            this.RemoveNPC();
            /// Tạo NPC mới
            NPC npc = KTNPCManager.Create(new KTNPCManager.NPCBuilder()
            {
                MapCode = this.Map.MapCode,
                ResID = KnowledgeChallenge.Event.NPC.NPCID,
                PosX = pos.x,
                PosY = pos.y,
                Name = KnowledgeChallenge.Event.NPC.Name,
                Title = KnowledgeChallenge.Event.NPC.Title,
                Tag = "KnowledgeChallenge",
                MinimapName = "Sứ giả Hoa Đăng",
            });
            /// Lưu lại thông tin NPC vừa tạo
            this.IdeNPC = npc;
            /// Hủy vị trí đích đến tiếp theo
            this.NextMoveToPos = null;
            /// Đánh dấu thời điểm tạo NPC đứng
            this.LastNPCIdeTicks = KTGlobal.GetCurrentTimeMilis();
            /// Đánh dấu vị trí đứng lần trước
            this.LastRandomIdePos = pos;

            /// Nếu sự kiện đã kết thúc
            if (this.isDisposed)
            {
                /// Xóa NPC
                this.RemoveNPC();
                /// Bỏ qua
                return;
            }

            ///// TEST
            //KTGlobal.SendSystemChat(string.Format("<color=yellow>[Nhan Như Ngọc]</color> đã xuất hiện tại <color=#3dfff9><link=\"GoTo_{0}_{1}_{2}\">[{3} ({4}, {5})]</link></color>", this.Map.MapCode, pos.x, pos.y, this.Map.MapName, pos.x, pos.y));
        }

        /// <summary>
        /// Tạo NPC di chuyển đến vị trí tương ứng
        /// </summary>
        /// <param name="toPos"></param>
        private void CreateMovingNPC(UnityEngine.Vector2Int toPos)
        {
            /// Đánh dấu đích đến
            this.NextMoveToPos = toPos;
            /// Vị trí cũ của NPC
            UnityEngine.Vector2Int oldPos = new UnityEngine.Vector2Int((int) this.IdeNPC.CurrentPos.X, (int) this.IdeNPC.CurrentPos.Y);
            /// Xóa NPC cũ
            this.RemoveNPC();

            /// Tạo NPC mới
            Monster npc = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
            {
                MapCode = this.Map.MapCode,
                ResID = KnowledgeChallenge.Event.NPC.MonsterID,
                PosX = oldPos.x,
                PosY = oldPos.y,
                Name = KnowledgeChallenge.Event.NPC.Name,
                Title = KnowledgeChallenge.Event.NPC.Title,
                MaxHP = 100,
                Level = 80,
                MonsterType = Entities.MonsterAIType.DynamicNPC,
                Tag = "KnowledgeChallenge",
            });

            /// Lưu lại thông tin NPC vừa tạo
            this.MovingNPC = npc;
            /// Miễn dịch toàn bộ
            npc.m_CurrentStatusImmunity = true;
            npc.m_CurrentInvincibility = 1;
            /// Sử dụng thuật toán A* tìm đường
            npc.UseAStarPathFinder = true;
            /// Chọn vị trí di chuyển ngẫu nhiên
            this.NextMoveToPos = toPos;
            /// Thực hiện di chuyển đến vị trí tương ứng
            npc.MoveTo(new System.Windows.Point(this.NextMoveToPos.Value.x, this.NextMoveToPos.Value.y), true);

            /// Nếu sự kiện đã kết thúc
            if (this.isDisposed)
            {
                /// Xóa NPC
                this.RemoveNPC();
                /// Bỏ qua
                return;
            }

            ///// TEST
            //KTGlobal.SendSystemChat(string.Format("<color=yellow>[Nhan Như Ngọc]</color> di chuyển đến <color=#3dfff9><link=\"GoTo_{0}_{1}_{2}\">[{3} ({4}, {5})]</link></color>", this.Map.MapCode, toPos.x, toPos.y, this.Map.MapName, toPos.x, toPos.y));
        }

        /// <summary>
        /// Xóa NPC
        /// </summary>
        private void RemoveNPC()
        {
            /// Nếu tồn tại NPC đang đứng
            if (this.IdeNPC != null)
            {
                /// Hủy
                this.RemoveNPC(this.IdeNPC);
                this.IdeNPC = null;
            }

            /// Nếu tồn tại NPC đang di chuyển
            if (this.MovingNPC != null)
            {
                /// Hủy
                this.RemoveMonster(this.MovingNPC);
                this.MovingNPC = null;
            }
        }

        /// <summary>
        /// Trả về một vị trí ngẫu nhiên để NPC di chuyển đến
        /// </summary>
        /// <returns></returns>
        private UnityEngine.Vector2Int GetRandomNPCPos()
        {
            /// Danh sách không chứa vị trí đứng lần trước
            List<UnityEngine.Vector2Int> availablePos = new List<UnityEngine.Vector2Int>();
            /// Nếu ban đầu chỉ có 1 vị trí thì giữ nguyên
            if (this.RandomNPCPositions.Count <= 1)
            {
                availablePos.AddRange(this.RandomNPCPositions);
            }
            else
            {
                /// Duyệt danh sách vị trí ban đầu
                foreach (UnityEngine.Vector2Int randomPos in this.RandomNPCPositions)
                {
                    /// Nếu khác vị trí cũ
                    if (randomPos != this.LastRandomIdePos)
                    {
                        /// Thêm vào danh sách
                        availablePos.Add(randomPos);
                    }
                }
            }

            return availablePos[KTGlobal.GetRandomNumber(0, availablePos.Count - 1)];
        }

        /// <summary>
        /// Trả về câu hỏi ngẫu nhiên
        /// </summary>
        /// <returns></returns>
        private KnowledgeChallenge.KC_Question GetRandomQuestion()
        {
            /// ID câu hỏi ngẫu nhiên
            int questionID = KnowledgeChallenge.QuestionIDs[KTGlobal.GetRandomNumber(0, KnowledgeChallenge.QuestionIDs.Count - 1)];
            /// Câu hỏi ngẫu nhiên được chọn
            return KnowledgeChallenge.Questions[questionID];
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hàm này gọi đến khi người chơi Click vào NPC
        /// </summary>
        /// <param name="player"></param>
        public void NPCClick(NPC npc, KPlayer player)
        {
            /// Nếu NPC không tồn tại
            if (npc != this.IdeNPC)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Không tìm thấy NPC tương ứng!");
                /// Bỏ qua
                return;
            }
            /// Nếu sự kiện đã kết thúc
            else if (this.isDisposed)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Sự kiện Đoán hoa đăng lần này đã kết thúc, bằng hữu hãy quay lại sau!");
                /// Bỏ qua
                return;
            }
            /// Nếu cấp độ không đủ
            else if (player.m_Level < KnowledgeChallenge.Config.LimitLevel)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, string.Format("Bằng hữu cần đạt tối thiểu cấp {0} mới có thể tham gia sự kiện Đoán hoa đăng!", KnowledgeChallenge.Config.LimitLevel));
                /// Bỏ qua
                return;
            }
            /// Nếu chưa đến thời gian mở câu hỏi
            else if (KTGlobal.GetCurrentTimeMilis() - player.LastOpenKnowledgeChallengeQuestion < KnowledgeChallenge.Event.Config.DelayTicksEachQuestion)
            {
                /// Thời gian còn lại
                int delaySec = (KnowledgeChallenge.Event.Config.DelayTicksEachQuestion - (int) (KTGlobal.GetCurrentTimeMilis() - player.LastOpenKnowledgeChallengeQuestion)) / 1000 + 1;
                /// Thông báo
                KTPlayerManager.ShowNotification(player, string.Format("Bằng hữu vừa mở câu hỏi, cần chờ {0} giây nữa mới có thể mở câu hỏi mới!", delaySec));
                /// Bỏ qua
                return;
            }

            try
            {
                /// Tổng số câu hỏi đã tham gia hôm nay
                int totalQuestions = this.GetTotalQuestionsToday(player);
                /// Nếu vượt quá
                if (totalQuestions >= KnowledgeChallenge.Event.Config.MaxQuestions)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, string.Format("Bằng hữu đã tham gia trả lời tổng cộng {0}/{1} câu hỏi, không thể tham gia thêm!", totalQuestions, KnowledgeChallenge.Event.Config.MaxQuestions));
                    /// Bỏ qua
                    return;
                }
                /// Tăng tổng số câu hỏi đã tham gia hôm nay
                totalQuestions++;
                /// Lưu lại
                this.SetTotalQuestionsToday(player, totalQuestions);
                /// Đánh dấu thời gian mở câu hỏi đoán hoa đăng
                player.LastOpenKnowledgeChallengeQuestion = KTGlobal.GetCurrentTimeMilis();

                /// Đối tượng Random
                Random rand = new Random();
                /// Chọn câu hỏi ngẫu nhiên
                KnowledgeChallenge.KC_Question randomQuestion = this.GetRandomQuestion();
                /// Danh sách câu trả lời
                Dictionary<int, string> answers = randomQuestion.Answers.OrderBy(x => rand.Next()).ToDictionary(tKey => tKey.Key, tValue => tValue.Value);

                /// Tạo Dialog
                KNPCDialog dialog = new KNPCDialog();
                dialog.Text = "<color=yellow>ĐOÁN HOA ĐĂNG</color>" + "<br>"
                            + string.Format("- Tổng số câu hỏi đã tham gia: <color=green>{0}/{1}</color>", this.GetTotalQuestionsToday(player), KnowledgeChallenge.Event.Config.MaxQuestions) + "<br>"
                            + "<br>"
                            + "<color=orange>Câu hỏi lượt này:</color>" + "<br>"
                            + randomQuestion.Content + "<br>"
                            + "<br>"
                            + "<color=orange>Chú ý:</color> Nếu <color=yellow>đóng</color> khung hội thoại này đồng nghĩa với việc <color=green>từ bỏ</color> câu hỏi này.";
                /// Duyệt danh sách câu trả lời
                foreach (KeyValuePair<int, string> pair in answers)
                {
                    /// Thêm vào Dialog
                    dialog.Selections[-pair.Key] = pair.Value;
                }
                dialog.OnSelect = (data) => {
                    /// Nếu sự kiện đã kết thúc
                    if (this.isDisposed)
                    {
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, "Sự kiện Đoán hoa đăng lần này đã kết thúc, bằng hữu hãy quay lại sau!");
                        /// Đóng Dialog
                        KT_TCPHandler.CloseDialog(player);
                        /// Xóa khỏi danh sách quản lý để tránh BUG Click nhiều lần
                        KTNPCDialogManager.RemoveNPCDialog(dialog);
                        /// Bỏ qua
                        return;
                    }

                    /// Nếu đáp án không chính xác
                    if (-data.SelectID != randomQuestion.CorrectAnswerID)
                    {
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, "Thật đáng tiếc, câu trả lời của bằng hữu không chính xác, hãy thử lại lần sau!");
                        /// Đóng Dialog
                        KT_TCPHandler.CloseDialog(player);
                        /// Xóa khỏi danh sách quản lý để tránh BUG Click nhiều lần
                        KTNPCDialogManager.RemoveNPCDialog(dialog);
                    }
                    /// Nếu đáp án chính xác
                    else
                    {
                        /// Thông báo
                        KTPlayerManager.ShowNotification(player, "Chúc mừng bằng hữu đã trả lời đúng!");
                        /// Đóng Dialog
                        KT_TCPHandler.CloseDialog(player);
                        /// Xóa khỏi danh sách quản lý để tránh BUG Click nhiều lần
                        KTNPCDialogManager.RemoveNPCDialog(dialog);
                        /// Thưởng bạc khóa
                        KTPlayerManager.AddBoundMoney(player, KnowledgeChallenge.Award.BoundMoney, "KnowledgeChallenge");
                        /// Thưởng đồng khóa
                        KTPlayerManager.AddBoundToken(player, KnowledgeChallenge.Award.BoundToken, "KnowledgeChallenge");
                        /// Thưởng kinh nghiệm
                        KTPlayerManager.AddExp(player, KnowledgeChallenge.Award.Exp);
                        /// Thưởng uy danh
                        player.Prestige += KnowledgeChallenge.Award.Prestige;

                        /// Gửi tin nhắn thông báo
                        KTGlobal.SendDefaultChat(player, string.Format("Nhận <color=yellow>{0}</color> điểm Uy danh.", KnowledgeChallenge.Award.Prestige));
                    }
                };
                /// Hiện khung
                dialog.ShowDialog(player);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion
    }
}
