using GameServer.Interface;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Đối tượng quản lý bẫy
    /// </summary>
    public static class KTTrapManager
    {
        /// <summary>
        /// Đối tượng sử dụng khóa LOCK
        /// </summary>
        private static readonly object Mutex = new object();

        /// <summary>
        /// Danh sách bẫy
        /// </summary>
        private static readonly Dictionary<int, Trap> Traps = new Dictionary<int, Trap>();

        /// <summary>
        /// Thêm bẫy vào danh sách quản lý
        /// </summary>
        /// <param name="trapID">ID bẫy</param>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="resID">ID Res</param>
        /// <param name="posX">Vị trí X</param>
        /// <param name="posY">Vị trí Y</param>
        /// <param name="lifeTime">Thời gian tồn tại</param>
        public static void AddTrap(int trapID, int mapCode, GameObject owner, int resID, int posX, int posY, float lifeTime)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            Trap trap = new Trap()
            {
                TrapID = trapID,
                MapCode = mapCode,
                Owner = owner,
                ResID = resID,
                CurrentPos = new System.Windows.Point(posX, posY),
                CurrentGrid = new System.Windows.Point(posX / gameMap.MapGridWidth, posY / gameMap.MapGridHeight),
                ObjectType = ObjectTypes.OT_TRAP,
                LifeTime = lifeTime,
            };
            lock (KTTrapManager.Mutex)
            {
                KTTrapManager.Traps[trapID] = trap;
                /// Thêm bẫy vào đối tượng quản lý map
                KTTrapManager.AddTrapToMap(trap);
            }
        }

        /// <summary>
        /// Xóa bẫy khỏi danh sách
        /// </summary>
        /// <param name="trapID">ID bẫy</param>
        public static void RemoveTrap(int trapID)
        {
            lock (KTTrapManager.Mutex)
            {
                if (KTTrapManager.Traps.TryGetValue(trapID, out Trap trap))
                {
                    KTTrapManager.RemoveTrapFromMap(trap);
                    KTTrapManager.Traps.Remove(trapID);
                }
            }
        }

        /// <summary>
        /// Thêm bẫy vào Map
        /// </summary>
        /// <param name="trap">Đối tượng bẫy</param>
        /// <returns></returns>
        private static void AddTrapToMap(Trap trap)
        {
            GameMap gameMap = KTMapManager.Find(trap.MapCode);
            gameMap.Grid.MoveObject((int) trap.CurrentPos.X, (int) (trap.CurrentPos.Y), trap);
        }

        /// <summary>
        /// Xóa bẫy tương ứng khỏi bản đồ
        /// </summary>
        /// <param name="mapCode"></param>
        private static void RemoveTrapFromMap(Trap trap)
        {
            GameMap gameMap = KTMapManager.Find(trap.MapCode);
            if (KTTrapManager.Traps.TryGetValue(trap.TrapID, out _))
            {
                /// Xóa bẫy khỏi Map
                gameMap.Grid.RemoveObject(trap);
            }
        }

        /// <summary>
        /// Gửi danh sách bẫy xung quanh người chơi
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="objsList">Danh sách bẫy</param>
        public static void SendMySelfTraps(KPlayer client, List<IObject> objsList)
        {
            if (null == objsList)
                return;
            Trap trap = null;
            for (int i = 0; i < objsList.Count; i++)
            {
                trap = objsList[i] as Trap;
                if (null == trap)
                {
                    continue;
                }

                if (!trap.IsVisibleTo(client))
                {
                    continue;
                }

                TrapRole trapRole = new TrapRole()
                {
                    ID = trap.TrapID,
                    ResID = trap.ResID,
                    PosX = (int) trap.CurrentPos.X,
                    PosY = (int) trap.CurrentPos.Y,
                    LifeTime = trap.LifeTime,
                    CasterID = trap.Owner.RoleID,
                };
                client.SendPacket<TrapRole>((int) TCPGameServerCmds.CMD_KT_SPR_NEWTRAP, trapRole);
            }
        }

        /// <summary>
        /// Xóa bẫy với người chơi tương ứng
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="objsList">Danh sách bẫy</param>
        public static void DelMySelfTraps(KPlayer client, List<IObject> objsList)
        {
            if (null == objsList)
            {
                return;
            }
            Trap trap;
            for (int i = 0; i < objsList.Count; i++)
            {
                trap = objsList[i] as Trap;
                if (null == trap)
                {
                    continue;
                }

                string strcmd = string.Format("{0}:{1}", trap.TrapID);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_SPR_DELTRAP, strcmd);
            }
        }
    }
}
