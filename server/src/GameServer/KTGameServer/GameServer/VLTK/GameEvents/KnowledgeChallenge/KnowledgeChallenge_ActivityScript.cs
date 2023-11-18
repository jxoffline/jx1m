using GameServer.KiemThe.GameEvents.Model;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.KnowledgeChallenge
{
    /// <summary>
    /// Script hoạt động Đoán hoa đăng
    /// </summary>
    public class KnowledgeChallenge_ActivityScript : IActivityScript
    {
        #region Define
        /// <summary>
        /// Hoạt động tương ứng
        /// </summary>
        public KTActivity Activity { get; set; }

        /// <summary>
        /// Danh sách Script đang thực thi
        /// </summary>
        private readonly ConcurrentDictionary<int, KnowledgeChallenge_Script_Main> scripts = new ConcurrentDictionary<int, KnowledgeChallenge_Script_Main>();
        #endregion

        #region Public methods
        /// <summary>
        /// Trả về Script ở bản đồ tương ứng
        /// </summary>
        /// <param name="mapID"></param>
        /// <returns></returns>
        public KnowledgeChallenge_Script_Main GetScript(int mapID)
        {
            if (this.scripts.TryGetValue(mapID, out KnowledgeChallenge_Script_Main script))
            {
                return script;
            }
            return null;
        }
        #endregion

        #region Core ActivityScript
        /// <summary>
        /// Bắt đầu Đoán hoa đăng
        /// </summary>
        public void Begin()
        {
            /// Nếu không có thông tin hoạt động
            if (this.Activity == null)
            {
                return;
            }

            /// ID sự kiện
            int activityID = this.Activity.Data.ID;
            /// Nếu là thông báo trước khi bắt đầu 10 phút
            if (activityID == 300)
            {
                /// Thông báo bắt đầu sự kiện
                KTGlobal.SendSystemEventNotification("Sự kiện Đoán Hoa Đăng sẽ bắt đầu sau 10 phút nữa, các vị bằng hữu hãy mau mau trở về Tân thủ thôn chuẩn bị tham gia sự kiện!");
            }
            /// Nếu là bắt đầu sự kiện
            else if (activityID == 301)
            {
                /// Thông báo bắt đầu sự kiện
                KTGlobal.SendSystemEventNotification("Sự kiện Đoán Hoa Đăng đã bắt đầu, các vị bằng hữu hãy mau mau trở về Tân thủ thôn, tìm gặp Nhan Như Ngọc, và trả lời các câu hỏi của cô ấy trong đợt này để nhận phần thưởng hỗ trợ!");

                /// Làm rỗng Script
                this.scripts.Clear();

                /// Duyệt danh sách bản đồ tương ứng
                foreach (KeyValuePair<int, List<UnityEngine.Vector2Int>> pair in KnowledgeChallenge.Event.NPC.Positions)
                {
                    /// Bản đồ tương ứng
                    GameMap map = KTMapManager.Find(pair.Key);
                    /// Nếu có bản đồ tương ứng
                    if (map != null)
                    {
                        /// Khởi tạo Script tương ứng
                        KnowledgeChallenge_Script_Main script = new KnowledgeChallenge_Script_Main(map, this.Activity)
                        {
                            RandomNPCPositions = pair.Value,
                            RemoveAllObjectsOnDispose = false,
                        };
                        /// Bắt đầu thực thi
                        script.Begin();
                        /// Thêm Script vào danh sách
                        this.scripts[script.Map.MapCode] = script;
                    }
                }
            }
            /// Nếu là kết thúc sự kiện
            else if (activityID == 302)
            {
                /// Thông báo bắt đầu sự kiện
                KTGlobal.SendSystemEventNotification("Sự kiện Đoán Hoa Đăng đợt này đã kết thúc, hẹn quý vị bằng hữu đợt tới!");
            }
        }

        /// <summary>
        /// Kết thúc Đoán hoa đăng
        /// </summary>
        public void Close()
        {
            List<int> keys = this.scripts.Keys.ToList();
            /// Duyệt danh sách Script và thực hiện đóng lại
            foreach (int key in keys)
            {
                /// Nếu không tồn tại
                if (!this.scripts.TryGetValue(key, out KnowledgeChallenge_Script_Main script))
                {
                    continue;
                }

                /// Hủy Script
                script.Dispose();
            }
            /// Giải phóng bộ nhớ
            this.scripts.Clear();
        }
        #endregion
    }
}
