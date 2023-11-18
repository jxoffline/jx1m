using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Logic;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Sử dụng kỹ năng
        /// <summary>
        /// Gửi yêu cầu dùng kỹ năng Pet lên GS
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="skillID"></param>
        /// <param name="targetID"></param>
        /// <param name="direction"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void SendPetUseSkill(int petID, int skillID, int targetID, int direction, int posX, int posY)
        {
            /// Dữ liệu gửi đi
            C2G_UseSkill useSkill = new C2G_UseSkill()
            {
                Direction = direction,
                TargetID = targetID,
                SkillID = skillID,
                PosX = posX,
                PosY = posY,
                TargetPosX = -1,
                TargetPosY = -1,
            };
            byte[] bytes = DataHelper.ObjectToBytes<C2G_UseSkill>(useSkill);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_USE_SKILL)));
        }

        /// <summary>
        /// Nhận phản hồi từ Server về kết quả pet sử dụng kỹ năng
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceivePetUseSkillResult(string[] fields)
        {
            /// Đánh dấu pet không đợi dùng kỹ năng
            KTAutoPetManager.Instance.IsWaitingToUseSkill = false;
        }
        #endregion

        #region Đồng bộ vị trí
        /// <summary>
        /// Đồng bộ vị trí pet lên GS
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void SendSyncPetPos(int petID, int posX, int posY)
        {
            string cmdData = string.Format("{0}:{1}:{2}", petID, posX, posY);
            byte[] bytes = new ASCIIEncoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_CHANGEPOS)));
        }
        #endregion

        #region Thu hoặc triệu hồi pet
        /// <summary>
        /// Gửi gói tin lên Server thực hiện thao tác triệu hồi hoặc thu pet
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="command"></param>
        public static void SendDoPetCommand(int petID, int command)
        {
            string cmdData = string.Format("{0}:{1}", petID, command);
            byte[] bytes = new ASCIIEncoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_DO_PET_COMMAND)));
        }

        /// <summary>
        /// Nhận gói tin từ Server thông báo thao tác triệu hồi hoặc thu pet
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveDoPetCommand(string[] fields)
        {
            /// Pet ID
            int petID = int.Parse(fields[0]);
            /// Loại thao tác
            int command = int.Parse(fields[1]);

            /// Loại thao tác
            switch (command)
            {
                /// Xuất chiến
                case 1:
                {
                    Global.Data.RoleData.CurrentPetID = petID;
                    break;
                }
                /// Thu hồi
                case 0:
                {
                    Global.Data.RoleData.CurrentPetID = -1;
                    break;
                }
            }

            /// Nếu đang mở khung
            if (PlayZone.Instance.UIPet != null)
            {
                /// Mở khung
                PlayZone.Instance.UIPet.UpdateCurrentPetID();
            }

            /// Bỏ đánh dấu đợi gọi pet
            KTAutoFightManager.Instance.IsCallingPet = false;
        }
        #endregion

        #region Danh sách Pet
        /// <summary>
        /// Gửi gói tin lên Server thực hiện lấy danh sách Pet hiện tại
        /// </summary>
        public static void SendGetPetList()
        {
            string cmdData = "";
            byte[] bytes = new ASCIIEncoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GET_PET_LIST)));
        }

        /// <summary>
        /// Nhận gói tin từ Server thông báo cập nhật danh sách Pet và mở khung pet
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveGetPetList(int cmdID, byte[] bytes, int length)
        {
            List<PetData> petList = DataHelper.BytesToObject<List<PetData>>(bytes, 0, length);
            if (petList == null)
            {
                petList = new List<PetData>();
            }

            /// Lưu dữ liệu lại
            Global.Data.RoleData.Pets = petList;

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIPet == null)
            {
                /// Mở khung
                PlayZone.Instance.ShowUIPet(petList);
            }
            /// Nếu đã mở khung
            else
            {
                /// Đồng bộ dữ liệu
                PlayZone.Instance.UIPet.Data = petList;
            }
        }

        /// <summary>
        /// Nhận gói tin từ Server thông báo cập nhật danh sách Pet
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveUpdatePetList(int cmdID, byte[] bytes, int length)
        {
            List<PetData> petList = DataHelper.BytesToObject<List<PetData>>(bytes, 0, length);
            if (petList == null)
            {
                petList = new List<PetData>();
            }

            /// Lưu dữ liệu lại
            Global.Data.RoleData.Pets = petList;

            /// Nếu đang mở khung pet
            if (PlayZone.Instance.UIPet == null)
            {
                /// Đồng bộ dữ liệu
                PlayZone.Instance.UIPet.Data = petList;
            }
        }
        #endregion

        #region Đổi tên pet
        /// <summary>
        /// Gửi gói tin lên Server yêu cầu đổi tên pet
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="newName"></param>
        public static void SendChangePetName(int petID, string newName)
        {
            string cmdData = string.Format("{0}:{1}", petID, newName);
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_CHANGENAME)));
        }

        /// <summary>
        /// Nhận gói tin từ Server thông báo đổi tên pet
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveChangePetName(string[] fields)
        {
            /// Pet ID
            int petID = int.Parse(fields[0]);
            /// Tên mới
            string newName = fields[1];

            /// Dữ liệu Pet tương ứng
            PetData petData = Global.Data.RoleData.Pets?.Where(x => x.ID == petID).FirstOrDefault();
            /// Nếu tìm thấy
            if (petData != null)
            {
                /// Cập nhật thông tin
                petData.Name = newName;
            }

            /// Nếu đang mở khung
            if (PlayZone.Instance.UIPet != null)
            {
                PlayZone.Instance.UIPet.UpdateNewName(petID, newName);
            }
        }
        #endregion

        #region Học kỹ năng pet
        /// <summary>
        /// Gửi gói tin lên Server yêu cầu học kỹ năng pet tương ứng
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="scrollDbID"></param>
        /// <param name="medicineDbID"></param>
        public static void SendPetStudySkill(int petID, int scrollDbID, int medicineDbID)
        {
            string cmdData = string.Format("{0}:{1}:{2}", petID, scrollDbID, medicineDbID);
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_STUDYSKILL)));
        }

        /// <summary>
        /// Nhận gói tin từ Server thông báo học kỹ năng pet tương ứng
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceivePetStudySkill(string[] fields)
        {
            /// ID pet
            int petID = int.Parse(fields[0]);
            /// Danh sỹ năng
            string skillsString = fields[1];
            /// Lĩnh ngộ
            int enlightenment = int.Parse(fields[2]);

            /// Dữ liệu Pet tương ứng
            PetData petData = Global.Data.RoleData.Pets?.Where(x => x.ID == petID).FirstOrDefault();
            /// Nếu tìm thấy
            if (petData != null)
            {
                /// Tạo mới Dict
                Dictionary<int, int> skillList = new Dictionary<int, int>();
                /// Nếu tồn tại
                if (!string.IsNullOrEmpty(skillsString))
                {
                    /// Duyệt thông tin
                    foreach (string skillString in skillsString.Split('|'))
                    {
                        /// chia thành các trường
                        string[] _fields = skillString.Split('_');
                        /// ID kỹ năng
                        int skillID = int.Parse(_fields[0]);
                        /// Cấp độ kỹ năng
                        int skillLevel = int.Parse(_fields[1]);
                        /// Thêm vào Dict
                        skillList[skillID] = skillLevel;
                    }
                }
                /// Cập nhật thông tin
                petData.Skills = skillList;
                petData.Enlightenment = enlightenment;
            }

            /// Nếu đang mở khung học kỹ năng Pet
            if (PlayZone.Instance.UIPetStudySkill != null)
            {
                /// Nếu không trùng ID
                if (PlayZone.Instance.UIPetStudySkill.Data.ID != petID)
                {
                    /// Bỏ qua
                    return;
                }
                /// Tạo mới Dict
                Dictionary<int, int> skillList = new Dictionary<int, int>();
                /// Nếu tồn tại
                if (!string.IsNullOrEmpty(skillsString))
                {
                    /// Duyệt thông tin
                    foreach (string skillString in skillsString.Split('|'))
                    {
                        /// chia thành các trường
                        string[] _fields = skillString.Split('_');
                        /// ID kỹ năng
                        int skillID = int.Parse(_fields[0]);
                        /// Cấp độ kỹ năng
                        int skillLevel = int.Parse(_fields[1]);
                        /// Thêm vào Dict
                        skillList[skillID] = skillLevel;
                    }
                }

                /// Cập nhật dữ liệu
                PlayZone.Instance.UIPetStudySkill.Data.Skills = skillList;
                PlayZone.Instance.UIPetStudySkill.Data.Enlightenment = enlightenment;
                /// Làm mới hiển thị
                PlayZone.Instance.UIPetStudySkill.RefreshData();
            }
        }
        #endregion

        #region Thuộc tính thay đổi
        /// <summary>
        /// Nhận gói tin từ Server cập nhật thuộc tính cơ bản của pet
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveUpdatePetBaseAttributes(string[] fields)
        {
            /// ID pet
            int petID = int.Parse(fields[0]);
            /// ID pet
            int joyful = int.Parse(fields[1]);
            /// ID pet
            int life = int.Parse(fields[2]);
            /// ID pet
            int level = int.Parse(fields[3]);
            /// ID pet
            int exp = int.Parse(fields[4]);
            /// ID pet
            int enlightenment = int.Parse(fields[5]);

            /// Dữ liệu Pet tương ứng
            PetData petData = Global.Data.RoleData.Pets?.Where(x => x.ID == petID).FirstOrDefault();
            /// Nếu tìm thấy
            if (petData != null)
            {
                /// Cập nhật thông tin
                petData.Joyful = joyful;
                petData.Enlightenment = enlightenment;
                petData.Life = life;
                petData.Level = level;
                petData.Exp = exp;
            }

            /// Nếu đang mở khung
            if (PlayZone.Instance.UIPet != null)
            {
                /// Cập nhật thuộc tính cơ bản
                PlayZone.Instance.UIPet.UpdateBaseAttributes(petID, joyful, life, level, exp, enlightenment);
            }
        }

        /// <summary>
        /// Nhận gói tin từ Server cập nhật thuộc tính (sức, thân, nội, ngoại và các chỉ số liên quan) của pet
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveUpdatePetAttributes(int cmdID, byte[] bytes, int length)
        {
            /// Dữ liệu pet
            PetData petData = DataHelper.BytesToObject<PetData>(bytes, 0, length);

            /// Dữ liệu Pet tương ứng
            PetData _petData = Global.Data.RoleData.Pets?.Where(x => x.ID == petData.ID).FirstOrDefault();
            /// Nếu tìm thấy
            if (_petData != null)
            {
                /// Cập nhật thông tin
                _petData.Str = petData.Str;
                _petData.Dex = petData.Dex;
                _petData.Sta = petData.Sta;
                _petData.Int = petData.Int;
            }

            /// Nếu đang mở khung
            if (PlayZone.Instance.UIPet != null)
            {
                /// Cập nhật thuộc tính cơ bản
                PlayZone.Instance.UIPet.UpdateAttributes(petData);
            }
        }
        #endregion

        #region Phân phối và tẩy điểm tiềm năng
        /// <summary>
        /// Gửi yêu cầu phân phối điểm tiềm năng cho pet
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="str"></param>
        /// <param name="dex"></param>
        /// <param name="sta"></param>
        /// <param name="ene"></param>
        public static void SendAssignPetAttributes(int petID, int str, int dex, int sta, int ene)
        {
            string cmdData = string.Format("{0}:{1}:{2}:{3}:{4}", petID, str, dex, sta, ene);
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_ASSIGN_ATTRIBUTES)));
        }

        /// <summary>
        /// Gửi yêu cầu tẩy điểm tiềm năng của pet
        /// </summary>
        /// <param name="petID"></param>
        public static void SendResetPetAttributes(int petID)
        {
            string cmdData = string.Format("{0}", petID);
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_RESET_ATTRIBUTES)));
        }
        #endregion

        #region Tặng quà
        /// <summary>
        /// Gửi yêu cầu tăng tuổi thọ hoặc độ vui vẻ cho pet
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="type"></param>
        public static void SendFeedPet(int petID, int type)
        {
            string cmdData = string.Format("{0}:{1}", petID, type);
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_FEED)));
        }

        /// <summary>
        /// Gửi yêu cầu tặng quà cho pet
        /// </summary>
        /// <param name="petID"></param>
        /// <param name="itemIDs"></param>
        public static void SendGiftPetItems(int petID, List<int> itemIDs)
        {
            string cmdData = string.Format("{0}:{1}", petID, string.Join("|", itemIDs));
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_GIFT_ITEMS)));
        }
        #endregion

        #region Phóng thích
        /// <summary>
        /// Gửi yêu cầu phóng thích pet
        /// </summary>
        /// <param name="petID"></param>
        public static void SendReleasePet(int petID)
        {
            string cmdData = string.Format("{0}", petID);
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_RELEASE)));
        }

        /// <summary>
        /// Nhận gói tin phóng thích pet
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveReleasePet(string[] fields)
        {
            /// ID pet
            int petID = int.Parse(fields[0]);

            /// Xóa Pet có ID tương ứng
            Global.Data.RoleData.Pets.RemoveAll(x => x.ID == petID);

            /// Gửi yêu cầu lấy thông tin pet
            KT_TCPHandler.SendGetPetList();
        }
        #endregion

        #region Cập nhật cấp độ
        /// <summary>
        /// Nhận gói tin thông báo cấp độ pet thay đổi
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceivePetLevelChanged(string[] fields)
        {
            /// ID pet
            int petID = int.Parse(fields[0]);
            /// Cấp độ
            int level = int.Parse(fields[1]);

            /// Dữ liệu Pet tương ứng
            PetData _petData = Global.Data.RoleData.Pets?.Where(x => x.ID == petID).FirstOrDefault();
            /// Nếu tìm thấy
            if (_petData != null)
            {
                /// Cập nhật thông tin
                _petData.Level = level;
                /// Thực thi hiệu ứng thăng cấp
                KTGlobal.PlayPetLevelUpEffect();
            }

            /// Thông tin pet
            if (Global.Data.SystemPets.TryGetValue(petID, out PetDataMini petData))
            {
                /// Cập nhật
                petData.Level = level;
            }
        }
        #endregion

        #region Truy vấn thông tin pet của người chơi
        /// <summary>
        /// Gửi yêu truy vấn thông tin pet của người chơi
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendGetPlayerPetList(int roleID)
        {
            string cmdData = string.Format("{0}", roleID);
            byte[] bytes = new UTF8Encoding().GetBytes(cmdData);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_PET_GET_PLAYER_LIST)));
        }

        /// <summary>
        /// Nhận gói tin từ Server thông báo danh sách pet của người chơi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveGetPlayerPetList(int cmdID, byte[] bytes, int length)
        {
            OtherRolePetData data = DataHelper.BytesToObject<OtherRolePetData>(bytes, 0, length);
            if (data == null)
            {
                /// Toác
                KTGlobal.AddNotification("Truy vấn thông tin tinh linh của người chơi thất bại. Hãy thử lại sau!");
                return;
            }

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIOtherRolePet == null)
            {
                /// Mở khung
                PlayZone.Instance.ShowUIOtherRolePet(data);
            }
            /// Nếu đã mở khung
            else
            {
                /// Đồng bộ dữ liệu
                PlayZone.Instance.UIOtherRolePet.Data = data;
            }
        }
        #endregion
    }
}
