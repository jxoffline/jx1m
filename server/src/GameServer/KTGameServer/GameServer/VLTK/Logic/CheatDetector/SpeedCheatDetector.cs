using GameServer.Logic;
using GameServer.Server;
using System;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic.CheatDetector
{
    /// <summary>
    /// Phát hiện Bug tốc chạy
    /// </summary>
    public class SpeedCheatDetector
    {
        /// <summary>
        /// Khoảng cách tối đa cho phép
        /// </summary>
        private const int MaxAllowedDistance = 400;

        /// <summary>
        /// Khoảng cách tin tưởng
        /// </summary>
        private const int TrustDistance = 300;

        /// <summary>
        /// Số lượt tin tưởng tối đa
        /// </summary>
        private const int MaxTrustTimes = 3;

        /// <summary>
        /// Đối tượng người chơi tương ứng
        /// </summary>
        public KPlayer Player { get; }

        /// <summary>
        /// Tổng số lượt đã tin tưởng
        /// </summary>
        private int totalTrustedTimes;

        /// <summary>
        /// Phát hiện Bug tốc chạy
        /// </summary>
        /// <param name="player"></param>
        public SpeedCheatDetector(KPlayer player)
        {
            this.Player = player;
            this.totalTrustedTimes = 0;
        }

        /// <summary>
        /// Kiểm tra vị trí truyền lên từ Client hợp lệ không
        /// </summary>
        /// <param name="client"></param>
        /// <param name="clientPosX"></param>
        /// <param name="clientPosY"></param>
        /// <returns></returns>
        public bool Validate(int clientPosX, int clientPosY)
        {
            /// Vị trí ở Client truyền lên
            UnityEngine.Vector2Int clPos = new UnityEngine.Vector2Int(clientPosX, clientPosY);
            /// Vị trí hiện tại ở GS
            UnityEngine.Vector2Int gsPos = new UnityEngine.Vector2Int(this.Player.PosX, this.Player.PosY);
            /// Nếu vị trí hiện tại ở Client không thể đến được
            if (KTGlobal.InObs(this.Player.CurrentMapCode, clientPosX, clientPosY, this.Player.CurrentCopyMapID))
            {
                /// Rollback vị trí
                this.ChangePos(gsPos.x, gsPos.y);
                /// Toác
                return false;
            }
            /// Khoảng cách giữa 2 vị trí
            float distance = UnityEngine.Vector2.Distance(clPos, gsPos);
            /// Nếu khoảng cách vượt quá ngưỡng cho phép
            if (distance > SpeedCheatDetector.MaxAllowedDistance)
            {
                /// TODO: Làm gì nó thì làm
                //Console.WriteLine("Player {0} position is bugged!!!", this.Player.RoleName);
                /// Rollback vị trí
                this.ChangePos(gsPos.x, gsPos.y);
                /// Toác
                return false;
            }
            /// Nếu vượt quá ngưỡng tin tưởng
            else if (distance > SpeedCheatDetector.TrustDistance)
            {
                /// Nếu vượt quá số lượt tin tưởng
                if (this.totalTrustedTimes >= SpeedCheatDetector.MaxTrustTimes)
                {
                    /// TODO: Làm gì nó thì làm
                    //Console.WriteLine("Player {0} is untrusted!!!", this.Player.RoleName);
                    /// Rollback vị trí
                    this.ChangePos(gsPos.x, gsPos.y);
                    /// Toác
                    return false;
                }

                /// Tăng số lượt tin tưởng lên
                this.totalTrustedTimes++;
                /// Thiết lập lại vị trí lấy vị trí ở Client
                this.Player.CurrentPos = new System.Windows.Point(clientPosX, clientPosY);
                /// Bản đồ
                GameMap gameMap = KTMapManager.Find(this.Player.CurrentMapCode);
                /// Cập nhật vị trí đối tượng vào Map
                gameMap.Grid.MoveObject(clientPosX, clientPosY, this.Player);
                //Console.WriteLine("Player {0} is temporary trusted. Total trusted times: {1}", this.Player.RoleName, this.totalTrustedTimes);
                /// OK
                return true;
            }
            /// Trong ngưỡng tin tưởng
            else
            {
                /// Reset số lượt vượt quá ngưỡng tin tưởng
                this.totalTrustedTimes = 0;
                /// Thiết lập lại vị trí lấy vị trí ở Client
                this.Player.CurrentPos = new System.Windows.Point(clientPosX, clientPosY);
                /// Bản đồ
                GameMap gameMap = KTMapManager.Find(this.Player.CurrentMapCode);
                /// Cập nhật vị trí đối tượng vào Map
                gameMap.Grid.MoveObject(clientPosX, clientPosY, this.Player);
                /// OK
                return true;
            }
        }

        /// <summary>
        /// Thay đổi vị trí người chơi
        /// </summary>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="notifyOthers"></param>
        private void ChangePos(int posX, int posY, bool notifyOthers = false)
        {
            KTPlayerManager.ChangePos(this.Player, posX, posY, notifyOthers);
        }
    }
}
