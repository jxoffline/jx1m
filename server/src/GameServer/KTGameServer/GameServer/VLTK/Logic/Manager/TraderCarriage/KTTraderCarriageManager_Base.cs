using GameServer.Interface;
using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý xe tiêu
    /// </summary>
    public static partial class KTTraderCarriageManager
    {
        #region Quản lý xe tiêu
        /// <summary>
        /// Tạo mới xe tiêu
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="type"></param>
        /// <param name="resID"></param>
        /// <param name="name"></param>
        /// <param name="vision"></param>
        /// <param name="moveSpeed"></param>
        /// <param name="maxHP"></param>
        /// <param name="lifeTime"></param>
        /// <param name="movePaths"></param>
        /// <returns></returns>
        public static TraderCarriage CreateTraderCarriage(KPlayer owner, int type, int resID, string name, int vision, int moveSpeed, int maxHP, int lifeTime, List<KeyValuePair<int, UnityEngine.Vector2Int>> movePaths)
        {
            /// Tạo mới danh sách hàng đợi đường đi
            Queue<KeyValuePair<int, UnityEngine.Vector2Int>> paths = new Queue<KeyValuePair<int, UnityEngine.Vector2Int>>();
            /// Duyệt danh sách
            foreach (KeyValuePair<int, UnityEngine.Vector2Int> pair in movePaths)
            {
                /// Thêm vào hàng đợi
                paths.Enqueue(pair);
            }

            /// Tạo mới
            TraderCarriage carriage = new TraderCarriage(owner)
            {
                Type = type,
                ResID = resID,
                RoleName = name,
                Vision = vision,
                DurationTicks = lifeTime,
                Paths = paths,
            };
            /// Cập nhật tốc chạy
            carriage.ChangeRunSpeed(moveSpeed, 0, 0);
            /// Cập nhật máu
            carriage.ChangeLifeMax(maxHP, 0, 0);
            carriage.m_CurrentLife = maxHP;
            /// Danh hiệu
            carriage.Title = string.Format("<color=yellow>Xe tiêu của <color=#52bdff>[{0}]</color></color>", owner.RoleName);

            /// Bắt đầu luồng
            KTTraderCarriageTimerManager.Instance.Add(carriage);
            /// Thêm vào danh sách
            KTTraderCarriageManager.carriages[carriage.RoleID] = carriage;
            /// Trả về kết quả
            return carriage;
        }

        /// <summary>
        /// Xóa xe tiêu tương ứng
        /// </summary>
        /// <param name="obj"></param>
        public static void RemoveTraderCarriage(TraderCarriage obj)
        {
            /// Toác
            if (obj == null)
            {
                return;
            }

            /// Ngừng đầu luồng xe tiêu
            KTTraderCarriageTimerManager.Instance.Remove(obj);

            /// Chuyển động tác sang chết
            obj.m_eDoing = KiemThe.Entities.KE_NPC_DOING.do_death;

            /// Reset
            obj.Reset();

            /// Xóa khỏi danh sách
            KTTraderCarriageManager.carriages.TryRemove(obj.RoleID, out _);
        }
        #endregion

        #region Hiển thị
        /// <summary>
        /// Xử lý khi đối tượng xe tiêu được tải xuống thành công
        /// </summary>
        /// <param name="client"></param>
        /// <param name="carriage"></param>
        public static void HandleTraderCarriageLoaded(KPlayer client, TraderCarriage carriage)
        {
            string pathString = KTTraderCarriageStoryBoardEx.Instance.GetCurrentPathString(carriage);
            LoadAlreadyData data = new LoadAlreadyData()
            {
                RoleID = carriage.RoleID,
                PosX = (int) carriage.CurrentPos.X,
                PosY = (int) carriage.CurrentPos.Y,
                Direction = (int) carriage.CurrentDir,
                Action = (int) carriage.m_eDoing,
                PathString = pathString,
                ToX = (int) carriage.ToPos.X,
                ToY = (int) carriage.CurrentPos.Y,
                Camp = carriage.Camp,
            };
            client.SendPacket<LoadAlreadyData>((int) TCPGameServerCmds.CMD_SPR_LOADALREADY, data);
        }
        #endregion
    }
}
