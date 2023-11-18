using GameServer.KiemThe.Core.Item;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý chế đồ
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Phản hồi yêu cầu chế tạo vật phẩm
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="socket"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="tcpRandKey"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ResponseCraftItem(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi ở tham số gửi lên, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID công thức chế
                int recipeID = int.Parse(fields[0]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đang thực hiện chế đồ
                if (client.IsCrafting)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, xin hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thực hiện chế đồ
                bool ret = ItemCraftingManager.DoCrafting(recipeID, client, () => {
                    KT_TCPHandler.SendFinishCraftItem(client, recipeID);
                });

                /// Nếu thao tác thành công
                if (ret)
                {
                    /// Gửi gói tin thông báo bắt đầu chế tạo về Client
                    KT_TCPHandler.SendBeginCraftItem(client);
                }
                /// Nếu thao tác thất bại
                else
                {
                    KT_TCPHandler.SendFinishCraftItem(client, -1);
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi gói tin thông báo bắt đầu chế vật phẩm tương ứng về Client
        /// </summary>
        /// <param name="client"></param>
        public static void SendBeginCraftItem(KPlayer client)
        {
            byte[] cmdData = new ASCIIEncoding().GetBytes("");
            client.SendPacket((int) TCPGameServerCmds.CMD_KT_BEGIN_CRAFT, cmdData);
        }

        /// <summary>
        /// Gửi gói tin thông báo bắt đầu chế vật phẩm tương ứng về Client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="recipeID"></param>
        public static void SendFinishCraftItem(KPlayer client, int recipeID)
        {
            /// Dữ liệu gói tin gửi đi
            string strCmd;

            /// Công thức tương ứng
            Recipe recipe = ItemCraftingManager._LifeSkill.TotalRecipe.Where(x => x.ID == recipeID).FirstOrDefault();
            /// Nếu công thức không tồn tại, tức thao tác thất bại
            if (recipe == null)
            {
                strCmd = "-1";
            }
            /// Nếu thao tác thành công
            else
            {
                /// Thông tin kỹ năng tương ứng
                LifeSkillPram lifeSkillParam = client.GetLifeSkill(recipe.Belong);
                /// Nếu thông tin kỹ năng tương ứng không tồn tại
                if (lifeSkillParam == null)
                {
                    strCmd = "-1";
                }
                else
                {
                    strCmd = string.Format("{0}:{1}:{2}", recipeID, lifeSkillParam.LifeSkillLevel, lifeSkillParam.LifeSkillExp);
                }
            }
            
            byte[] cmdData = new ASCIIEncoding().GetBytes(strCmd);
            client.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_FINISH_CRAFT, cmdData);
        }
    }
}
