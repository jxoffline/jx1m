using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Entities.Pet;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý gói tin
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Vị trí
        /// <summary>
        /// Phản hồi yêu cầu thay đổi vị trí từ Pet Client gửi lên
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
        public static TCPProcessCmdResults ResponsePetChangePos(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);
                /// Vị trí X
                int posX = int.Parse(fields[1]);
                /// Vị trí Y
                int posY = int.Parse(fields[2]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }
                // NẾu list thằng này đéo có con pet nào trường hợp này xảy ra khi thằng kia giao dịch con pet đi rồi
                if(client.PetList==null)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }    
                /// Thông tin pet tương ứng
                PetData petData = client.PetList?.Where(x => x != null && x.ID == petID - (int) ObjectBaseID.Pet).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không có Pet
                if (client.CurrentPet == null)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không phải pet đang tham chiến
                if (client.CurrentPet.RoleID != petID)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu Pet đã bị hủy thì thôi
                if (client.CurrentPet.IsDestroyed)
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Vị trí cần kiểm tra
                UnityEngine.Vector2 clientPos = new UnityEngine.Vector2(posX, posY);
                /// Đồng bộ vị trí
                bool result = client.CurrentPet.ClientSyncPos(clientPos, out UnityEngine.Vector2 destPos);
                /// Nếu cần cập nhật vị trí
                if (result)
                {
                    /// Gửi vị trí đến bọn xung quanh
                    KT_TCPHandler.SendPetChangePositionToClients(client.CurrentPet, UnityEngine.Vector2.Distance(destPos, clientPos) <= 10 ? client : null);
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
        /// Gửi gói tin thông báo Pet thay đổi vị trí tới tất cả người chơi xung quanh
        /// </summary>
        /// <param name="pet"></param>
        /// <param name="ignored"></param>
        /// <param name="isBlink"></param>
        public static void SendPetChangePositionToClients(Pet pet, KPlayer ignored = null, bool isBlink = false)
        {
            /// Dữ liệu
            string cmdData = string.Format("{0}:{1}:{2}:{3}", pet.RoleID, (int) pet.CurrentPos.X, (int) pet.CurrentPos.Y, isBlink ? 1 : 0);

            /// Tìm tất cả người chơi xung quanh để gửi gói tin
            List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(pet);
            if (listObjects == null)
            {
                return;
            }

            /// Duyệt danh sách đối tượng xung quanh
            foreach (KPlayer player in listObjects)
            {
                /// Nếu có thể nhìn thấy pet này
                if (player != ignored && pet.VisibleTo(player))
                {
                    player.SendPacket((int) TCPGameServerCmds.CMD_KT_PET_CHANGEPOS, cmdData);
                }
            }
        }
        #endregion

        #region Truy vấn danh sách pet
        /// <summary>
        /// Phản hồi yêu cầu truy vấn danh sách Pet từ Client
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
        public static TCPProcessCmdResults ResponseGetPetList(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player not exist, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Danh sách Pet
                List<PetData> petList = client.PetList?.Select(x => KTPetManager.ClonePetData(x)).ToList();
                /// Toác
                if (petList == null)
                {
                    petList = new List<PetData>();
                }

                /// Duyệt danh sách Pet
                for (int i = 0; i < petList.Count; i++)
                {
                    /// Thông tin pet và các chỉ số
                    petList[i] = KTPetManager.GetPetDataWithAttributes(petList[i], client);
                }

                /// Dữ liệu gửi đi
                byte[] bData = DataHelper.ObjectToBytes<List<PetData>>(petList);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bData, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi gói tin cập nhật danh sách pet mới
        /// </summary>
        /// <param name="client"></param>
        public static void SendUpdatePetList(KPlayer client)
        {
            /// Danh sách Pet
            List<PetData> petList = client.PetList?.Select(x => KTPetManager.ClonePetData(x)).ToList();
            /// Toác
            if (petList == null)
            {
                petList = new List<PetData>();
            }

            /// Duyệt danh sách Pet
            for (int i = 0; i < petList.Count; i++)
            {
                /// Thông tin pet và các chỉ số
                petList[i] = KTPetManager.GetPetDataWithAttributes(petList[i], client);
            }

            /// Gửi gói tin đi
            client.SendPacket<List<PetData>>((int) TCPGameServerCmds.CMD_KT_PET_UPDATE_PETLIST, petList);
        }

        /// <summary>
        /// Phản hồi yêu cầu truy vấn danh sách Pet của người chơi tương ứng từ Client
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
        public static TCPProcessCmdResults ResponseGetPlayerPetList(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi
                int roleID = int.Parse(fields[0]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player not exist, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer player = KTPlayerManager.Find(roleID);
                /// Nếu không tìm thấy
                if (player == null)
                {
                    KTPlayerManager.ShowNotification(client, "Người chơi không tồn tại hoặc đã rời mạng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Danh sách Pet
                List<PetData> petList = player.PetList?.Select(x => KTPetManager.ClonePetData(x)).ToList();
                /// Toác
                if (petList == null)
                {
                    petList = new List<PetData>();
                }

                /// Duyệt danh sách Pet
                for (int i = 0; i < petList.Count; i++)
                {
                    /// Thông tin pet và các chỉ số
                    petList[i] = KTPetManager.GetPetDataWithAttributes(petList[i], player);
                }

                /// Danh sách trang bị pet
                List<GoodsData> equips = player.GoodsData.FindAll(x => x.Using >= 200);

                /// Dữ liệu gửi đi
                byte[] bData = DataHelper.ObjectToBytes<OtherRolePetData>(new OtherRolePetData()
                {
                    RoleName = player.RoleName,
                    Pets = petList,
                    PetEquips = equips,
                });
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bData, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Thao tác triệu hồi/thu pet
        /// <summary>
        /// Phản hồi yêu cầu xuất chiến hoặc thu hồi pet từ client
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
        public static TCPProcessCmdResults ResponsePetComand(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);
                /// Loại (0: Thu hồi, 1: Xuất chiến)
                int command = int.Parse(fields[1]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Thông tin pet tương ứng
                PetData petData = client.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet does not exist, CMD={0}, Client={1}, PetID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), petID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Loại
                switch (command)
                {
                    /// Thu hồi
                    case 0:
                        {
                            client.CallBackPet();
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }
                    /// Xuất chiến
                    case 1:
                        {
                            /// Nếu điểm vui vẻ không đủ
                            if (petData.Joyful < KPet.Config.CallFightRequịreJoyOver)
                            {
                                KTPlayerManager.ShowNotification(client, string.Format("Độ vui vẻ của tinh linh cần có từ {0} điểm trở lên mới có thể tham chiến!", KPet.Config.CallFightRequịreJoyOver));
                                return TCPProcessCmdResults.RESULT_OK;
                            }
                            /// Nếu tuổi thọ không đủ
                            else if (petData.Life < KPet.Config.CallFightRequịreLifeOver)
                            {
                                KTPlayerManager.ShowNotification(client, string.Format("Tuổi thọ của tinh linh cần đạt từ {0} điểm trở lên mới có thể tham chiến!", KPet.Config.CallFightRequịreLifeOver));
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            client.CallPet(petData);
                            break;
                        }
                    /// Toác
                    default:
                        {
                            LogManager.WriteLog(LogTypes.Error, string.Format("Wrong pet command, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                            return TCPProcessCmdResults.RESULT_FAILED;
                        }
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Đổi tên pet
        /// <summary>
        /// Phản hồi yêu cầu đổi tên pet từ client
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
        public static TCPProcessCmdResults ResponseChangePetName(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);
                /// Tên mới
                string newName = fields[1];

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Chặn Spam-Click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    client.SendClick();
                }

                /// Thông tin pet tương ứng
                PetData petData = client.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet does not exist, CMD={0}, Client={1}, PetID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), petID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đang tham chiến
                if (client.CurrentPet != null && client.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
                {
                    KTPlayerManager.ShowNotification(client, "Tinh linh đang tham chiến, không thể đổi tên!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu tên không hợp lệ
                if (!KTFormValidation.IsValidString(newName, false, true, false, false))
                {
                    KTPlayerManager.ShowNotification(client, "Tên tinh linh không thể chứa ký tự đặc biệt!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu độ dài không đủ
                else if (newName.Length < 6 || newName.Length > 18)
                {
                    KTPlayerManager.ShowNotification(client, "Tên tinh linh phải có từ 6 đến 18 ký tự!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu trùng với tên cũ
                if (petData.Name == newName)
                {
                    KTPlayerManager.ShowNotification(client, "Tên tinh linh không được đặt trùng với tên hiện tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Gửi yêu cầu đổi tên Pet lên gameDB
                string[] resultFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_NAME, string.Format("{0}:{1}:{2}", client.RoleID, petID, newName), client.ServerId);
                /// Toác
                if (resultFields == null)
                {
                    return TCPProcessCmdResults.RESULT_FAILED;
                }
                /// Mã trả về
                int returnCode = int.Parse(resultFields[0]);
                /// Toác
                if (returnCode == -1)
                {
                    KTPlayerManager.ShowNotification(client, "Đổi tên tinh linh thất bại. Hãy thử lại sau!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// OK
                else
                {
                    /// Lưu lại
                    petData.Name = newName;
                    /// Thông báo
                    KTPlayerManager.ShowNotification(client, "Đổi tên tinh linh thành công!");
                    /// Gửi phản hồi
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Sử dụng kỹ năng
        /// <summary>
        /// Phản hồi yêu cầu sử dụng kỹ năng pet
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
        public static TCPProcessCmdResults ResponsePetUseSkill(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            C2G_UseSkill useSkill = null;
            try
            {
                useSkill = DataHelper.BytesToObject<C2G_UseSkill>(data, 0, data.Length);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                int posX = useSkill.PosX;
                int posY = useSkill.PosY;

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kỹ năng tương ứng
                SkillDataEx skillData = KSkill.GetSkillData(useSkill.SkillID);
                if (skillData == null)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "99999", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Pet tương ứng
                if (client.CurrentPet == null)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "99999", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Bản đồ hiện tại
                GameMap gameMap = KTMapManager.Find(client.CurrentMapCode);
                /// Nếu bản đồ không cho phép dùng kỹ năng
                if (!gameMap.AllowUseSkill)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "99999", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu là kỹ năng đánh nhưng bản đồ hiện tại không cho phép dùng
                else if (!gameMap.AllowUseOffensiveSkill && (skillData.Type != 2 || (skillData.TargetType != "self" && skillData.TargetType != "team")))
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "99999", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu kỹ năng bị cấm
                else if (gameMap.BanSkills.Contains(useSkill.SkillID))
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "99999", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu có trạng thái cấm dùng kỹ năng
                else if (client.ForbidUsingSkill)
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "99999", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu đang ở trong khu an toàn
                else if (client.IsInsideSafeZone && (skillData.Type != 2 || (skillData.TargetType != "self" && skillData.TargetType != "team")))
                {
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "99999", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Kiểm tra kỹ năng, nếu không nằm trong danh sách hiện có của pet hoặc có nhưng cấp 0 thì toang
                SkillLevelRef skillRef = client.CurrentPet.SkillTree.GetSkillLevelRef(useSkill.SkillID);
                if (skillRef == null || (skillRef != null && skillRef.Level <= 0))
                {
                    //LogManager.WriteLog(LogTypes.Error, string.Format("Skill not found in player pet's list or not yet studied, CMD={0}, Client={1}, SkillID={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), useSkill.SkillID));
                    //return TCPProcessCmdResults.RESULT_FAILED;
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "99999", nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                GameObject target = null;
                Interface.IObject obj = KTSkillManager.FindSpriteByID(useSkill.TargetID);
                /// Nếu không phải GameObject thì không đánh được
                if (obj != null && (obj is GameObject))
                {
                    target = (GameObject)obj;
                }

                /// Thiết lập hướng quay truyền từ Client lên
                try
                {
                    client.CurrentPet.CurrentDir = (Entities.Direction)useSkill.Direction;
                }
                catch (Exception) { }

                /// Kiểm tra vị trí hiện tại của người pet và vị trí truyền về từ Client xem có hợp lệ không
                client.CurrentPet.ClientSyncPos(new UnityEngine.Vector2(posX, posY), out UnityEngine.Vector2 destPos);

                /// Dùng kỹ năng
                KTSkillManager.UseSkillResult result = KTSkillManager.UseSkill(client.CurrentPet, target, null, skillRef);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", (int)result), nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Học kỹ năng
        /// <summary>
        /// Phản hồi yêu cầu học kỹ năng pet từ Client
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
        public static TCPProcessCmdResults ResponsePetStudySkill(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);
                /// ID sách kỹ năng
                int scrollID = int.Parse(fields[1]);
                /// ID lĩnh hội đan
                int medicineID = int.Parse(fields[2]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Chặn Spam-Click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    client.SendClick();
                }

                /// Thông tin pet tương ứng
                PetData petData = client.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet does not exist, CMD={0}, Client={1}, PetID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), petID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đang tham chiến
                if (client.CurrentPet != null && client.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
                {
                    KTPlayerManager.ShowNotification(client, "Tinh linh đang tham chiến, không thể học kỹ năng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thông tin sách
                GoodsData scrollItemGD = client.GoodsData.Find(scrollID, 0);
                /// Không tìm thấy
                if (scrollItemGD == null)
                {
                    //LogManager.WriteLog(LogTypes.Error, string.Format("Pet Skill Scroll does not exist, CMD={0}, Client={1}, ScrollDBID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), scrollID));
                    //return TCPProcessCmdResults.RESULT_FAILED;
                    KTPlayerManager.ShowNotification(client, "Sách kỹ năng không tồn tại, hãy đóng khung và thao tác lại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Lĩnh hội đan
                GoodsData medicineItemGD = null;
                /// Nếu có tồn tại lĩnh hội đan gửi lên
                if (medicineID != -1)
                {
                    medicineItemGD = client.GoodsData.Find(medicineID, 0);
                    /// Không tìm thấy
                    if (medicineItemGD == null)
                    {
                        //LogManager.WriteLog(LogTypes.Error, string.Format("Pet Skill Medicine does not exist, CMD={0}, Client={1}, ScrollDBID={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), medicineID));
                        //return TCPProcessCmdResults.RESULT_FAILED;
                        KTPlayerManager.ShowNotification(client, "Lĩnh hội đan không tồn tại, hãy đóng khung và thao tác lại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Nếu không phải lĩnh hội đan
                    if (medicineItemGD.GoodsID != KPet.Config.SkillStudyMedicineItemID)
                    {
                        KTPlayerManager.ShowNotification(client, string.Format("Chỉ có thể đặt vào [{0}].", ItemManager.GetNameItem(medicineItemGD)));
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

                /// Thông tin vật phẩm
                ItemData scrollItemData = ItemManager.GetItemTemplate(scrollItemGD.GoodsID);
                /// Toác
                if (scrollItemData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet Skill Scroll item data does not exist, CMD={0}, Client={1}, ScrollID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), scrollItemGD.GoodsID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Kỹ năng tương ứng
                if (!KPet.Config.SkillScrolls.TryGetValue(scrollItemGD.GoodsID, out PetConfigXML.SkillScroll skillScrollData))
                {
                    KTPlayerManager.ShowNotification(client, "Thông tin sách kỹ năng tinh linh bị lỗi. Hãy liên hệ với hỗ trợ để được trợ giúp!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu chưa tồn tại kỹ năng
                if (petData.Skills == null)
                {
                    /// Tạo mới
                    petData.Skills = new Dictionary<int, int>();
                }

                /// ID kỹ năng
                int skillID = skillScrollData.SkillID;
                /// Nếu chưa tồn tại
                if (!petData.Skills.ContainsKey(skillID))
                {
                    /// Thực hiện xóa sách kỹ năng tương ứng
                    if (!ItemManager.DestroyGoods(client, scrollItemGD))
                    {
                        KTPlayerManager.ShowNotification(client, "Không thể xóa sách kỹ năng tinh linh. Hãy thử lại sau!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Tổng số kỹ năng hiện có
                    int totalSkills = petData.Skills.Count;
                    /// Đánh dấu có ghi đè lên kỹ năng cũ không
                    bool isReplaceExistOne = false;
                    /// Nếu đã quá số lượng kỹ năng
                    if (totalSkills >= KPet.Config.MaxSkill || totalSkills >= KPet.Config.SkillStudyRates.Count)
                    {
                        /// Ghi đè
                        isReplaceExistOne = true;
                    }
                    /// Nếu chưa quá số lượng kỹ năng
                    else
                    {
                        /// Tỷ lệ học mới
                        int studyNewRate = KTGlobal.GetRandomNumber(1, 100);
                        /// Nếu lớn hơn tỷ lệ học mới với số lượng kỹ năng hiện tại được thiết lập thì ghi đè
                        isReplaceExistOne = studyNewRate > KPet.Config.SkillStudyRates[totalSkills];
                    }

                    /// Nếu không ghi đè
                    if (!isReplaceExistOne)
                    {
                        /// Thêm kỹ năng mới
                        petData.Skills[skillID] = 1;

                        /// Cập nhật dữ liệu vào DB
                        if (!KT_TCPHandler.SendDBUpdatePetSkills(client, petData))
                        {
                            KTPlayerManager.ShowNotification(client, "Không thể lưu thông tin kỹ năng. Hãy liên hệ với hỗ trợ để báo cáo!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }

                        /// Thông tin kỹ năng
                        SkillDataEx skillData = KSkill.GetSkillData(skillID);
                        /// Thông báo
                        KTPlayerManager.ShowNotification(client, string.Format("Học kỹ năng [{0}] tinh linh thành công!", skillData.Name));
                    }
                    /// Nếu ghi đè
                    else
                    {
                        /// Nếu có lĩnh hội đan thì thôi
                        if (medicineItemGD != null)
                        {
                            /// Xóa vật phẩm
                            if (!ItemManager.AbandonItem(medicineItemGD, client, false))
                            {
                                KTPlayerManager.ShowNotification(client, "Không thể xóa vật phẩm. Hãy liên hệ với hỗ trợ để báo cáo!");
                                return TCPProcessCmdResults.RESULT_OK;
                            }


                            /// Thông tin kỹ năng
                            SkillDataEx skillData = KSkill.GetSkillData(skillID);
                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, string.Format("Học kỹ năng [{0}] tinh linh thất bại!", skillData.Name));
                        }
                        /// Nếu không có lĩnh hội đan thì ghi đè
                        else
                        {
                            /// Cấp độ thấp nhất
                            int minLevel = petData.Skills.Values.Min();
                            /// Danh sách kỹ năng có cùng cấp độ thấp nhất
                            List<int> minLevelSkills = petData.Skills.Where(x => x.Value == minLevel).Select(x => x.Key).ToList();
                            /// Chọn ngẫu nhiên 1 kỹ năng
                            int replacedSkillID = minLevelSkills.RandomRange(1).FirstOrDefault();
                            /// Danh sách kỹ năng mới
                            Dictionary<int, int> skills = new Dictionary<int, int>();
                            /// Duyệt danh sách kỹ năng cũ
                            foreach (KeyValuePair<int, int> pair in petData.Skills)
                            {
                                /// Nếu là kỹ năng bị thay thế
                                if (pair.Key == replacedSkillID)
                                {
                                    /// Thêm kỹ năng mới vào
                                    skills[skillID] = 1;
                                }
                                else
                                {
                                    /// Ghi thông tin kỹ năng này
                                    skills[pair.Key] = pair.Value;
                                }
                            }
                            /// Ghi lại danh sách
                            petData.Skills = skills;

                            /// Cập nhật dữ liệu vào DB
                            if (!KT_TCPHandler.SendDBUpdatePetSkills(client, petData))
                            {
                                KTPlayerManager.ShowNotification(client, "Không thể lưu thông tin kỹ năng. Hãy liên hệ với hỗ trợ để báo cáo!");
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Thông tin kỹ năng
                            SkillDataEx skillData = KSkill.GetSkillData(skillID);
                            /// Thông báo
                            KTPlayerManager.ShowNotification(client, string.Format("Học kỹ năng [{0}] tinh linh thành công!", skillData.Name));
                        }
                    }
                }
                /// Nếu đã tồn tại
                else
                {
                    /// Nếu có lĩnh hội đan
                    if (medicineItemGD != null)
                    {
                        KTPlayerManager.ShowNotification(client, string.Format("[{0}] chỉ có thể được sử dụng khi học mới kỹ năng tinh linh!", ItemManager.GetNameItem(medicineItemGD)));
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Cấp độ hiện tại
                    int skillLevel = petData.Skills[skillID];
                    /// Nếu đã đạt cấp độ tối đa
                    if (skillLevel >= KPet.Config.SkillLevelUps.Count)
                    {
                        KTPlayerManager.ShowNotification(client, "Kỹ năng này đã đạt cấp độ tối đa!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Thông tin thăng cấp
                    PetConfigXML.SkillLevelUpData levelUpData = KPet.Config.SkillLevelUps[skillLevel - 1];
                    /// Nếu lĩnh ngộ không đủ
                    if (petData.Enlightenment < levelUpData.RequireEnlightenment)
                    {
                        KTPlayerManager.ShowNotification(client, "Độ lĩnh ngộ của tinh linh không đủ để thăng cấp kỹ năng này!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Trừ lĩnh ngộ
                    petData.Enlightenment -= levelUpData.RequireEnlightenment;
                    /// Ghi lại giá trị lĩnh ngộ vào DB
                    if (!KT_TCPHandler.SendDBUpdatePetEnlightenment(client, petData))
                    {
                        KTPlayerManager.ShowNotification(client, "Không thể lưu giá trị lĩnh ngộ của tinh linh. Hãy liên hệ với hỗ trợ để báo cáo!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Thực hiện xóa sách kỹ năng tương ứng
                    if (!ItemManager.DestroyGoods(client, scrollItemGD))
                    {
                        KTPlayerManager.ShowNotification(client, "Không thể xóa sách kỹ năng tinh linh. Hãy thử lại sau!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Tỷ lệ thành công
                    int myRate = KTGlobal.GetRandomNumber(1, 100);
                    /// Nếu thành công
                    if (myRate <= levelUpData.Rate)
                    {
                        /// Thăng cấp độ lên
                        petData.Skills[skillID]++;
                        /// Thông tin kỹ năng
                        SkillDataEx skillData = KSkill.GetSkillData(skillID);
                        KTPlayerManager.ShowNotification(client, string.Format("Thăng cấp kỹ năng [{0}] tinh linh thành công lên cấp {1}.", skillData.Name, petData.Skills[skillID]));
                        /// Cấp độ lớn hơn 10
                        if (petData.Skills[skillID] >= 10)
                        {
                            /// Thông tin
                            PetData pd = KTPetManager.CalculatePetAttributes(KTPetManager.ClonePetData(petData));
                            /// Text show hàng
                            string broadcastText = string.Format("Người chơi <color=#65c4f1>[{0}]</color> đã thăng cấp kỹ năng <color=yellow>[{1}]</color> của tinh linh {2} thành công lên <color=green>cấp {3}</color>!", client.RoleName, skillData.Name, KTGlobal.GetPetDescInfoStringForChat(pd), petData.Skills[skillID]);
                            /// Show hàng
                            KTGlobal.SendSystemChat(broadcastText, null, new List<PetData>() { pd });
                        }

                        /// Cập nhật dữ liệu vào DB
                        if (!KT_TCPHandler.SendDBUpdatePetSkills(client, petData))
                        {
                            KTPlayerManager.ShowNotification(client, "Không thể lưu thông tin kỹ năng. Hãy liên hệ với hỗ trợ để báo cáo!");
                            return TCPProcessCmdResults.RESULT_OK;
                        }
                    }
                    /// Nếu thất bại
                    else
                    {
                        /// Thông tin kỹ năng
                        SkillDataEx skillData = KSkill.GetSkillData(skillID);
                        KTPlayerManager.ShowNotification(client, string.Format("Thật đáng tiếc, thăng cấp kỹ năng [{0}] tinh linh thất bại!", skillData.Name));
                    }
                }

                /// Chuỗi danh sách kỹ năng
                List<string> petSkillStrings = new List<string>();
                /// Duyệt danh sách kỹ năng
                foreach (KeyValuePair<int, int> pair in petData.Skills)
                {
                    /// Thêm vào chuỗi
                    petSkillStrings.Add(string.Format("{0}_{1}", pair.Key, pair.Value));
                }

                /// Gửi phản hồi
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}:{1}:{2}", petID, string.Join("|", petSkillStrings), petData.Enlightenment), nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Thông báo các thuộc tính của Pet
        /// <summary>
        /// Gửi thông báo đến chủ nhân các điểm cơ bản của Pet
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pet"></param>
        public static void NotifyPetBaseAttributes(KPlayer player, PetData pet)
        {
            /// Dữ liệu
            string cmdData = string.Format("{0}:{1}:{2}:{3}:{4}:{5}", pet.ID, pet.Joyful, pet.Life, pet.Level, pet.Exp, pet.Enlightenment);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_PET_UPDATE_BASE_ATTRIBUTES, cmdData);
        }

        /// <summary>
        /// Gửi thông báo đến chủ nhân các thuộc tính của pet thay đổi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        public static void NotifyPetAttributes(KPlayer player, PetData petData)
        {
            byte[] data = DataHelper.ObjectToBytes<PetData>(petData);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_PET_UPDATE_ATTRIBUTES, data);
        }
        #endregion

        #region Phân phối và tẩy điểm tiềm năng
        /// <summary>
        /// Phản hồi yêu cầu phân phối tiềm năng pet từ Client
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
        public static TCPProcessCmdResults ResponsePetAssignAttributes(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 5)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);
                /// Sức
                int str = int.Parse(fields[1]);
                /// Thân
                int dex = int.Parse(fields[2]);
                /// Ngoại
                int sta = int.Parse(fields[3]);
                /// Nội
                int ene = int.Parse(fields[4]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Chặn Spam-Click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    client.SendClick();
                }

                /// Thông tin pet tương ứng
                PetData petData = client.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet does not exist, CMD={0}, Client={1}, PetID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), petID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu không có gì để tẩy
                if (str == 0 && dex == 0 && sta == 0 && ene == 0)
                {
                    KTPlayerManager.ShowNotification(client, "Tinh linh này chưa phân phối tiềm năng!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đang tham chiến
                if (client.CurrentPet != null && client.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
                {
                    /// Toác
                    if (!client.CurrentPet.DisputeAttributes(str, dex, sta, ene))
                    {
                        KTPlayerManager.ShowNotification(client, "Phân phối tiềm năng cho tinh linh thất bại. Hãy thử lại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Lưu vào DB
                    if (!KT_TCPHandler.SendDBUpdatePetAttributes(client, client.CurrentPet.RoleID - (int) ObjectBaseID.Pet, client.CurrentPet.BaseStr, client.CurrentPet.BaseDex, client.CurrentPet.BaseSta, client.CurrentPet.BaseInt, client.CurrentPet.BaseRemainPoints))
                    {
                        KTPlayerManager.ShowNotification(client, "Không thể thực hiện phân phối tiềm năng cho tinh linh. Hãy liên hệ với hỗ trợ để báo lỗi!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Thông tin mới
                    petData = client.CurrentPet.GetDBData();
                    /// Tiềm năng
                    petData.RemainPoints = client.CurrentPet.RemainPoints;
                    /// Sức, thân, ngoại, nội
                    petData.Str = client.CurrentPet.Str;
                    petData.Dex = client.CurrentPet.Dex;
                    petData.Sta = client.CurrentPet.Sta;
                    petData.Int = client.CurrentPet.Int;
                }
                /// Nếu không tham chiến
                else
                {
                    /// Tổng tiềm năng sử dụng
                    int totalWillUseRemainPoints = str + dex + sta + ene;
                    /// Tổng tiềm năng có
                    int totalRemainPoints = petData.RemainPoints + KPet.GetTotalRemainPoints(petData.Level) - petData.Str - petData.Dex - petData.Sta - petData.Int;
                    /// Nếu tổng tiềm năng có ít hơn tổng tiềm năng sẽ sử dụng
                    if (totalRemainPoints < totalWillUseRemainPoints)
                    {
                        KTPlayerManager.ShowNotification(client, "Phân phối tiềm năng cho tinh linh thất bại. Hãy thử lại!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Lưu lại
                    petData.Str += str;
                    petData.Dex += dex;
                    petData.Sta += sta;
                    petData.Int += ene;

                    /// Lưu vào DB
                    if (!KT_TCPHandler.SendDBUpdatePetAttributes(client, petData.ID, petData.Str, petData.Dex, petData.Sta, petData.Int, petData.RemainPoints))
                    {
                        KTPlayerManager.ShowNotification(client, "Không thể thực hiện phân phối tiềm năng cho tinh linh. Hãy liên hệ với hỗ trợ để báo lỗi!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Tạo bản sao
                    petData = KTPetManager.ClonePetData(petData);

                    /// Thông tin Pet
                    PetDataXML _data = KPet.GetPetData(petData.ResID);
                    /// Nếu tồn tại
                    if (_data != null)
                    {
                        /// Tiềm năng
                        petData.RemainPoints = KPet.GetTotalRemainPoints(petData.Level) + petData.RemainPoints - petData.Str - petData.Dex - petData.Sta - petData.Int;
                        /// Sức, thân, ngoại, nội
                        petData.Str = _data.Str + _data.StrPerLevel * petData.Level + petData.Str;
                        petData.Dex = _data.Dex + _data.DexPerLevel * petData.Level + petData.Dex;
                        petData.Sta = _data.Sta + _data.StaPerLevel * petData.Level + petData.Sta;
                        petData.Int = _data.Int + _data.IntPerLevel * petData.Level + petData.Int;

                        /// Vật công - ngoại
                        petData.PAtk = KPet.GetStrength2DamagePhysics(petData.ResID, petData.Str);
                        /// Chính xác, né tránh
                        petData.Hit = KPet.GetDexterity2AttackRate(petData.ResID, petData.Dex);
                        petData.Dodge = KPet.GetDexterity2Defence(petData.ResID, petData.Dex);
                        /// Sinh lực
                        petData.MaxHP = KPet.GetVitality2Life(petData.ResID, petData.Sta) + petData.HP;
                        /// Vật công - nội
                        petData.MAtk = KPet.GetEnergy2DamageMagic(petData.ResID, petData.Int);
                    }
                }

                /// Thông báo thành công
                KTPlayerManager.ShowNotification(client, string.Format("Phân phối tiềm năng cho tinh linh [{0}] thành công!", petData.Name));

                /// Gửi kết quả lại cho client
                KT_TCPHandler.NotifyPetAttributes(client, petData);
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu phân phối tẩy điểm tiềm năng pet từ Client
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
        public static TCPProcessCmdResults ResponsePetResetAttributes(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Chặn Spam-Click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    client.SendClick();
                }

                /// Thông tin pet tương ứng
                PetData petData = client.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet does not exist, CMD={0}, Client={1}, PetID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), petID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu không có gì để tẩy
                if (petData.Str == 0 && petData.Dex == 0 && petData.Sta == 0 && petData.Int == 0)
                {
                    KTPlayerManager.ShowNotification(client, "Tinh linh này chưa phân phối tiềm năng, không thể tẩy điểm!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không có vật phẩm yêu cầu
                if (ItemManager.GetItemCountInBag(client, KPet.Config.ResetAttributesItemID) <= 0)
                {
                    KTPlayerManager.ShowNotification(client, string.Format("Tẩy điểm tiềm năng tinh linh cần có [{0}]!", KTGlobal.GetItemName(KPet.Config.ResetAttributesItemID)));
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Xóa vật phẩm tương ứng
                bool ret = ItemManager.RemoveItemFromBag(client, KPet.Config.ResetAttributesItemID, 1);
                /// Toác
                if (!ret)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể xóa vật phẩm. Hãy thử lại sau!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đang tham chiến
                if (client.CurrentPet != null && client.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
                {
                    /// Toác
                    client.CurrentPet.ResetAllAttributes();

                    /// Lưu vào DB
                    if (!KT_TCPHandler.SendDBUpdatePetAttributes(client, client.CurrentPet.RoleID - (int) ObjectBaseID.Pet, client.CurrentPet.BaseStr, client.CurrentPet.BaseDex, client.CurrentPet.BaseSta, client.CurrentPet.BaseInt, client.CurrentPet.BaseRemainPoints))
                    {
                        KTPlayerManager.ShowNotification(client, "Không thể thực hiện phân phối tiềm năng cho tinh linh. Hãy liên hệ với hỗ trợ để báo lỗi!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Thông tin mới
                    petData = client.CurrentPet.GetDBData();
                    /// Tiềm năng
                    petData.RemainPoints = client.CurrentPet.RemainPoints;
                    /// Sức, thân, ngoại, nội
                    petData.Str = client.CurrentPet.Str;
                    petData.Dex = client.CurrentPet.Dex;
                    petData.Sta = client.CurrentPet.Sta;
                    petData.Int = client.CurrentPet.Int;
                }
                /// Nếu không tham chiến
                else
                {
                    /// Lưu lại
                    petData.Str = 0;
                    petData.Dex = 0;
                    petData.Sta = 0;
                    petData.Int = 0;

                    /// Lưu vào DB
                    if (!KT_TCPHandler.SendDBUpdatePetAttributes(client, petData.ID, petData.Str, petData.Dex, petData.Sta, petData.Int, petData.RemainPoints))
                    {
                        KTPlayerManager.ShowNotification(client, "Không thể thực hiện tẩy điểm tiềm năng cho tinh linh. Hãy liên hệ với hỗ trợ để báo lỗi!");
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Tạo bản sao
                    petData = KTPetManager.ClonePetData(petData);

                    /// Thông tin Pet
                    PetDataXML _data = KPet.GetPetData(petData.ResID);
                    /// Nếu tồn tại
                    if (_data != null)
                    {
                        /// Tiềm năng
                        petData.RemainPoints = KPet.GetTotalRemainPoints(petData.Level) + petData.RemainPoints - petData.Str - petData.Dex - petData.Sta - petData.Int;
                        /// Sức, thân, ngoại, nội
                        petData.Str = _data.Str + _data.StrPerLevel * petData.Level + petData.Str;
                        petData.Dex = _data.Dex + _data.DexPerLevel * petData.Level + petData.Dex;
                        petData.Sta = _data.Sta + _data.StaPerLevel * petData.Level + petData.Sta;
                        petData.Int = _data.Int + _data.IntPerLevel * petData.Level + petData.Int;

                        /// Vật công - ngoại
                        petData.PAtk = KPet.GetStrength2DamagePhysics(petData.ResID, petData.Str);
                        /// Chính xác, né tránh
                        petData.Hit = KPet.GetDexterity2AttackRate(petData.ResID, petData.Dex);
                        petData.Dodge = KPet.GetDexterity2Defence(petData.ResID, petData.Dex);
                        /// Sinh lực
                        petData.MaxHP = KPet.GetVitality2Life(petData.ResID, petData.Sta) + petData.HP;
                        /// Vật công - nội
                        petData.MAtk = KPet.GetEnergy2DamageMagic(petData.ResID, petData.Int);
                    }
                }

                /// Thông báo thành công
                KTPlayerManager.ShowNotification(client, string.Format("Tẩy điểm tiềm năng cho tinh linh [{0}] thành công!", petData.Name));

                /// Gửi kết quả lại cho client
                KT_TCPHandler.NotifyPetAttributes(client, petData);
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Tặng quà
        /// <summary>
        /// Phản hồi yêu cầu tăng tuổi thọ hoặc độ vui vẻ của pet từ Client
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
        public static TCPProcessCmdResults ResponseFeedPet(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);
                /// Loại điểm
                int type = int.Parse(fields[1]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Chặn Spam-Click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    client.SendClick();
                }

                /// Thông tin pet tương ứng
                PetData petData = client.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet does not exist, CMD={0}, Client={1}, PetID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), petID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Vật phẩm tìm thấy trong túi
                PetConfigXML.FeedItem item = null;

                /// Loại
                switch (type)
                {
                    /// Tăng độ vui vẻ
                    case 0:
                        {
                            /// Nếu độ vui vẻ đã đạt tối đa
                            if (petData.Joyful >= KPet.Config.MaxJoy)
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Độ vui vẻ của tinh linh này đã đạt tối đa!");
                                /// Bỏ qua
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Duyệt danh sách vật phẩm tăng độ vui vẻ
                            foreach (PetConfigXML.FeedItem itemInfo in KPet.Config.FeedJoyItems.Values)
                            {
                                /// Nếu có trong túi
                                if (ItemManager.GetItemCountInBag(client, itemInfo.ItemID) > 0)
                                {
                                    /// Đánh dấu vật phẩm
                                    item = itemInfo;
                                    /// Thoát
                                    break;
                                }
                            }

                            /// Nếu không tìm thấy
                            if (item == null)
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Không có vật phẩm tăng độ vui vẻ!");
                                /// Bỏ qua
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Xóa vật phẩm tương ứng
                            bool ret = ItemManager.RemoveItemFromBag(client, item.ItemID, 1);
                            /// Toác
                            if (!ret)
                            {
                                KTPlayerManager.ShowNotification(client, "Không thể xóa vật phẩm. Hãy thử lại sau!");
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Tăng độ vui vẻ tương ứng
                            KTPetManager.AddPetJoyful(client, petID, item.Point);
                            break;
                        }
                    /// Tăng tuổi thọ
                    case 1:
                        {
                            /// Nếu độ vui vẻ đã đạt tối đa
                            if (petData.Life >= KPet.Config.MaxLife)
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Tuổi thọ của tinh linh này đã đạt tối đa!");
                                /// Bỏ qua
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Duyệt danh sách vật phẩm tăng độ vui vẻ
                            foreach (PetConfigXML.FeedItem itemInfo in KPet.Config.FeedLifeItems.Values)
                            {
                                /// Nếu có trong túi
                                if (ItemManager.GetItemCountInBag(client, itemInfo.ItemID) > 0)
                                {
                                    /// Đánh dấu vật phẩm
                                    item = itemInfo;
                                    /// Thoát
                                    break;
                                }
                            }

                            /// Nếu không tìm thấy
                            if (item == null)
                            {
                                /// Thông báo
                                KTPlayerManager.ShowNotification(client, "Không có vật phẩm tuổi thọ vui vẻ!");
                                /// Bỏ qua
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Xóa vật phẩm tương ứng
                            bool ret = ItemManager.RemoveItemFromBag(client, item.ItemID, 1);
                            /// Toác
                            if (!ret)
                            {
                                KTPlayerManager.ShowNotification(client, "Không thể xóa vật phẩm. Hãy thử lại sau!");
                                return TCPProcessCmdResults.RESULT_OK;
                            }

                            /// Tăng tuổi thọ tương ứng
                            KTPetManager.AddPetLife(client, petID, item.Point);
                            break;
                        }
                }

                /// OK
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu tặng quà cho pet từ Client
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
        public static TCPProcessCmdResults ResponseGiftPetItems(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);
                /// Danh sách vật phẩm
                string[] itemStrings = fields[1].Split('|');

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Chặn Spam-Click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    client.SendClick();
                }

                /// Thông tin pet tương ứng
                PetData petData = client.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet does not exist, CMD={0}, Client={1}, PetID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), petID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Dánh sách vật phẩm truyền lên
                List<int> inputItemDbIDs = new List<int>();
                /// Duyệt danh sách vật phẩm
                foreach (string itemString in itemStrings)
                {
                    /// ID vật phẩm
                    int itemDbID = int.Parse(itemString);
                    /// Thêm vào danh sách
                    inputItemDbIDs.Add(itemDbID);
                }
                /// Danh sách vật phẩm tương ứng
                List<GoodsData> itemGDs = client.GoodsData.FindAll(x => x.Site == 0 && inputItemDbIDs.Any(y => x.Id == y));
                /// Nếu danh sách không thỏa mãn
                if (inputItemDbIDs.Count != itemGDs.Count)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet award item does not exist, CMD={0}, Client={1}, ClientCount={2}, GSCount={3}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), inputItemDbIDs.Count, itemGDs.Count));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Tổng điểm lĩnh ngộ tăng lên
                int totalEnlightenment = 0;

                /// Duyệt danh sách vật phẩm
                foreach (GoodsData itemGD in itemGDs)
                {
                    /// Nếu không phải vật phẩm tăng lĩnh ngộ
                    if (!KPet.Config.FeedEnlightenmentItems.TryGetValue(itemGD.GoodsID, out PetConfigXML.FeedItem item))
                    {
                        /// Thông báo
                        KTPlayerManager.ShowNotification(client, "Vật phẩm đặt vào không hợp lệ!");
                        /// Toác
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                    /// Số lượng tương ứng
                    int quantity = itemGD.GCount;
                    /// Số điểm tăng thêm
                    totalEnlightenment += quantity * item.Point;

                    /// Xóa vật phẩm tương ứng
                    if (!ItemManager.AbadonItem(itemGD.Id, client, false))
                    {
                        /// Thông báo
                        KTPlayerManager.ShowNotification(client, "Không thể xóa vật phẩm. Hãy liên hệ với hỗ trợ để báo lỗi!");
                        /// Toác
                        return TCPProcessCmdResults.RESULT_OK;
                    }
                }

                /// Tăng điểm lĩnh ngộ
                KTPetManager.AddPetEnlightenment(client, petID, totalEnlightenment);
                /// OK
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Phóng thích
        /// <summary>
        /// Phản hồi yêu cầu phóng thích pet từ Client
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
        public static TCPProcessCmdResults ResponseReleasePet(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new UTF8Encoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Chia thành các trường
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Wrong parameters, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID pet
                int petID = int.Parse(fields[0]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Player does not exist, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }
                // nếu có khóa cấp 2
                if (client.NeedToShowInputSecondPassword())
                {
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Chặn Spam-Click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                else
                {
                    client.SendClick();
                }

                /// Thông tin pet tương ứng
                PetData petData = client.PetList.Where(x => x.ID == petID).FirstOrDefault();
                /// Nếu không tồn tại
                if (petData == null)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pet does not exist, CMD={0}, Client={1}, PetID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), petID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu đang tham chiến
                if (client.CurrentPet != null && client.CurrentPet.RoleID == petID + (int) ObjectBaseID.Pet)
                {
                    KTPlayerManager.ShowNotification(client, "Tinh linh đang tham chiến, không thể phóng thích!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Ngừng Progress
                client.CurrentProgress = null;

                /// Xóa dữ liệu
                client.PetList.Remove(petData);
                /// Thực hiện phóng thích
                bool ret = KT_TCPHandler.SendDBRemovePet(client, petID);
                /// Toác
                if (!ret)
                {
                    KTPlayerManager.ShowNotification(client, "Không thể phóng thích tinh linh. Hãy thử lại sau!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// OK
                else
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(client, "Phóng thích tinh linh thành công!");
                    /// Gửi phản hồi
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, cmdData, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
        #endregion

        #region Thông báo cấp độ thay đổi
        /// <summary>
        /// Gửi gói tin tới tất cả người chơi xung quanh thông báo cấp độ pet thay đổi
        /// </summary>
        /// <param name="pet"></param>
        public static void SendPetLevelChanged(Pet pet)
        {
            /// Dữ liệu
            string cmdData = string.Format("{0}:{1}", pet.RoleID, pet.m_Level);

            /// Tìm tất cả người chơi xung quanh để gửi gói tin
            List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(pet);
            if (listObjects == null)
            {
                return;
            }

            /// Duyệt danh sách đối tượng xung quanh
            foreach (KPlayer player in listObjects)
            {
                /// Nếu có thể nhìn thấy pet này
                if (pet.VisibleTo(player))
                {
                    /// Gửi gói tin
                    player.SendPacket((int) TCPGameServerCmds.CMD_KT_PET_UPDATE_LEVEL, cmdData);
                }
            }
        }
        #endregion

        #region GameDB
        /// <summary>
        /// Gửi yêu cầu thêm Pet cho người chơi lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        /// <returns>ID Pet mới được thêm vào</returns>
        public static int SendDBAddPet(KPlayer player, PetData petData)
        {
            /// Chuỗi Byte chuyển lên
            byte[] cmdData = DataHelper.ObjectToBytes<PetData>(petData);
            /// Chuyển lên GameDB
            TCPProcessCmdResults result = Global.ReadDataFromDb((int)TCPGameServerCmds.CMD_KT_DB_ADD_PET, cmdData, cmdData.Length, out byte[] returnBytesData, player.ServerId);
            /// Nếu có kết quả
            if (result == TCPProcessCmdResults.RESULT_DATA)
            {
                int length = BitConverter.ToInt32(returnBytesData, 0);
                string[] strData = new UTF8Encoding().GetString(returnBytesData, 6, length - 2).Split(':');
                /// ID pet mới tạo
                int petID = int.Parse(strData[0]);
                /// Trả về kết quả
                return petID;
            }
            /// Toác
            return -1;
        }

        /// <summary>
        /// Gửi yêu cầu xóa Pet của người chơi lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petID"></param>
        /// <returns></returns>
        public static bool SendDBRemovePet(KPlayer player, int petID)
        {
            string cmdData = string.Format("{0}:{1}", player.RoleID, petID);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_DELETE_PET, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin cấp độ và kinh nghiệm hiện tại của Pet lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petID"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        public static bool SendDBUpdatePetLevelAndExp(KPlayer player, int petID, int level, int exp)
        {
            string cmdData = string.Format("{0}:{1}:{2}:{3}", player.RoleID, petID, level, exp);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_LEVEL_AND_EXP, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin sinh lực hiện tại của Pet lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static bool SendDBUpdatePetHP(KPlayer player, PetData petData)
        {
            string cmdData = string.Format("{0}:{1}:{2}", player.RoleID, petData.ID, petData.HP);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_HP, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin thuộc tính hiện tại của Pet lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petID"></param>
        /// <param name="str"></param>
        /// <param name="dex"></param>
        /// <param name="sta"></param>
        /// <param name="ene"></param>
        /// <param name="remainPoints"></param>
        /// <returns></returns>
        public static bool SendDBUpdatePetAttributes(KPlayer player, int petID, int str, int dex, int sta, int ene, int remainPoints)
        {
            string cmdData = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", player.RoleID, petID, str, dex, sta, ene, remainPoints);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_ATTRIBUTES, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin điểm vui vẻ của Pet lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static bool SendDBUpdatePetJoyful(KPlayer player, PetData petData)
        {
            string cmdData = string.Format("{0}:{1}:{2}", player.RoleID, petData.ID, petData.Joyful);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_JOYFUL, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin tuổi thọ của Pet lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static bool SendDBUpdatePetLife(KPlayer player, PetData petData)
        {
            string cmdData = string.Format("{0}:{1}:{2}", player.RoleID, petData.ID, petData.Life);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_LIFE, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin của Pet lên GameDB trước khi thu hồi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static bool SendDBUpdatePetDataBeforeCallBack(KPlayer player, PetData petData)
        {
            string cmdData = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", player.RoleID, petData.ID, petData.HP, petData.Joyful, petData.Life, petData.Level, petData.Exp);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_BEFORE_QUIT_GAME, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin kỹ năng của Pet lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static bool SendDBUpdatePetSkills(KPlayer player, PetData petData)
        {
            /// Danh sách mã hóa kỹ năng pet
            List<string> petSkillStrings = new List<string>();
            /// Duyệt danh sách kỹ năng pet
            foreach (KeyValuePair<int, int> pair in petData.Skills)
            {
                /// Thêm vào danh sách mã hóa
                petSkillStrings.Add(string.Format("{0}_{1}", pair.Key, pair.Value));
            }

            string cmdData = string.Format("{0}:{1}:{2}", player.RoleID, petData.ID, string.Join("|", petSkillStrings));
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_SKILLS, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin lĩnh ngộ của Pet lên GameDB
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petData"></param>
        /// <returns></returns>
        public static bool SendDBUpdatePetEnlightenment(KPlayer player, PetData petData)
        {
            string cmdData = string.Format("{0}:{1}:{2}", player.RoleID, petData.ID, petData.Enlightenment);
            string[] resultData = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_ENLIGHTENMENT, cmdData, player.ServerId);
            /// Nếu có kết quả
            if (resultData != null)
            {
                /// Mã trả về
                int retCode = int.Parse(resultData[0]);
                /// OK là 0
                return retCode == 0;
            }
            /// Toác
            else
            {
                return false;
            }
        }
        #endregion
    }
}