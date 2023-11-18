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
        /// <summary>
        /// Nhận gói tin cập nhật vị trí quái vật đặc biệt trong bản đồ khu vực
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveUpdateLocalMapSpecialMonster(int cmdID, byte[] bytes, int length)
        {
            /// Danh sách quái tương ứng
            List<LocalMapMonsterData> monsters = DataHelper.BytesToObject<List<LocalMapMonsterData>>(bytes, 0, length);
            /// Nếu toác thì thôi
            if (monsters == null || monsters.Count <= 0)
            {
                return;
            }

            /// Cập nhật LocalMap
            if (PlayZone.Instance.UILocalMap != null && PlayZone.Instance.UILocalMap.Visible)
            {
                PlayZone.Instance.UILocalMap.UpdateSpecialMonsterList(monsters);
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật vị trí quái vật đặc biệt trong bản đồ khu vực
        /// </summary>
        public static void SendUpdateLocalMapSpecialMonster()
        {
            /// Chuyển chuỗi message thành dạng byte array để gửi đi
            byte[] bytes = new ASCIIEncoding().GetBytes("");
            /// Gửi gói tin về GS
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_UPDATE_LOCALMAP_MONSTER)));
        }
    }
}
