using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Cường hóa Ngũ hành ấn
        /// <summary>
        /// Gửi yêu cầu Cường hóa Ngũ hành ấn
        /// </summary>
        /// <param name="signet"></param>
        /// <param name="fsList"></param>
        /// <param name="type"></param>
        public static void SendSignetEnhance(GoodsData signet, List<GoodsData> fsList, int type)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// Danh sách DbID Huyền Tinh
            List<int> fs = fsList.Select(x => x.Id).ToList();
            /// DbID trang bị cường hóa
            int equipDbID = signet.Id;

            /// Tạo mới gói tin
            SignetEnhance signetEnhance = new SignetEnhance()
            {
                SignetDbID = equipDbID,
                FSDbID = fs,
                Type = type,
            };
            byte[] bytes = DataHelper.ObjectToBytes<SignetEnhance>(signetEnhance);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SIGNET_ENHANCE)));
        }

        /// <summary>
        /// Nhận thông báo cường hóa Ngũ hành ấn từ Server trả về
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveSignetEnhance(string[] fields)
        {
            int ret = int.Parse(fields[0]);
            /// Nếu thao tác thành công
            if (ret == 1)
            {
                if (PlayZone.Instance.UISignetEnhance != null)
                {
                    PlayZone.Instance.UISignetEnhance.EmptyFiveElementsStoneList();
                    /// Thực thi hiệu ứng cường hóa thành công
                    PlayZone.Instance.UISignetEnhance.PlaySignetEnhanceSuccessEffect();
                    PlayZone.Instance.UISignetEnhance.RefreshEnhanceEquip();
                }
            }
        }
        #endregion

        #region Cường hóa trang bị
        /// <summary>
        /// Gửi yêu cầu Cường hóa trang bị
        /// </summary>
        /// <param name="equipGD"></param>
        /// <param name="crystalStonesGD"></param>
        /// <param name="moneyType"></param>
        public static void SendEquipEnhance(GoodsData equipGD, List<GoodsData> crystalStonesGD, MoneyType moneyType)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// Danh sách DbID Huyền Tinh
            List<int> crystalStones = crystalStonesGD.Select(x => x.Id).ToList();
            /// DbID trang bị cường hóa
            int equipDbID = equipGD.Id;
            /// Loại tiền tệ sử dụng
            int moneyTypeInt = (int) moneyType;

            /// Tạo mới gói tin
            EquipEnhance equipEnhance = new EquipEnhance()
            {
                EquipDbID = equipDbID,
                MoneyType = moneyTypeInt,
                CrystalStonesDbID = crystalStones,
            };
            byte[] bytes = DataHelper.ObjectToBytes<EquipEnhance>(equipEnhance);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_EQUIP_ENHANCE)));
        }

        /// <summary>
        /// Nhận thông báo cường hóa trang bị từ Server trả về
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveEquipEnhance(string[] fields)
        {
            int ret = int.Parse(fields[0]);
            /// Nếu thao tác thành công
            if (ret != -1)
            {
                if (PlayZone.Instance.UIEnhance != null)
                {
                    PlayZone.Instance.UIEnhance.EmptyCrystalStoneGrid();
                    /// Nếu cường hóa thành công thì thực thi hiệu ứng
                    if (ret == 1)
                    {
                        PlayZone.Instance.UIEnhance.PlayEquipEnhanceSuccessEffect();
                        PlayZone.Instance.UIEnhance.RefreshEnhanceEquip();
                    }
                }
            }
        }

        /// <summary>
        /// Gửi yêu cầu ghép Huyền Tinh
        /// </summary>
        /// <param name="crystalStonesGD"></param>
        /// <param name="moneyType"></param>
        public static void SendComposeCrystalStones(List<GoodsData> crystalStonesGD, MoneyType moneyType)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// Danh sách DbID Huyền Tinh
            List<int> crystalStones = crystalStonesGD.Select(x => x.Id).ToList();
            /// Loại tiền tệ sử dụng
            int moneyTypeInt = (int) moneyType;

            /// Tạo mới gói tin
            CrystalStoneCompose crystalStoneCompose = new CrystalStoneCompose()
            {
                MoneyType = moneyTypeInt,
                CrystalStonesDbID = crystalStones,
            };
            byte[] bytes = DataHelper.ObjectToBytes<CrystalStoneCompose>(crystalStoneCompose);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_COMPOSE_CRYSTALSTONES)));
        }

        /// <summary>
        /// Nhận thông báo ghép Huyền Tinh từ Server trả về
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveComposeCrystalStones(string[] fields)
        {
            int ret = int.Parse(fields[0]);
            /// Nếu thao tác thành công
            if (ret == 1)
            {
                if (PlayZone.Instance.UICrystalStoneSynthesis != null)
                {
                    PlayZone.Instance.UICrystalStoneSynthesis.EmptyFrame();
                }
            }
        }

        /// <summary>
        /// Gửi yêu cầu tách Huyền Tinh ở trang bị
        /// </summary>
        /// <param name="equipGD"></param>
        /// <param name="moneyType"></param>
        public static void SendSplitCrystalStones(GoodsData equipGD, MoneyType moneyType)
        {
            KT_TCPHandler.SendSplitCrystalStones(equipGD, (int) moneyType);
        }

        /// <summary>
        /// Gửi yêu cầu tách Huyền Tinh ở trang bị
        /// </summary>
        /// <param name="equipGD"></param>
        /// <param name="moneyType"></param>
        public static void SendSplitCrystalStones(GoodsData equipGD, int moneyType)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// DbID trang bị cường hóa
            int equipDbID = equipGD.Id;

            /// Tạo mới gói tin
            CrystalStoneSplit crystalStoneSplit = new CrystalStoneSplit()
            {
                EquipDbID = equipDbID,
                MoneyType = moneyType,
            };
            byte[] bytes = DataHelper.ObjectToBytes<CrystalStoneSplit>(crystalStoneSplit);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SPLIT_CRYSTALSTONES)));
        }

        /// <summary>
        /// Nhận thông báo tách Huyền Tinh ở trang bị từ Server trả về
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveSplitCrystalStones(string[] fields)
        {
            int ret = int.Parse(fields[0]);
            /// Nếu thao tác thành công
            if (ret == 1)
            {
                if (PlayZone.Instance.UISplitEquipCrystalStones != null)
                {
                    PlayZone.Instance.UISplitEquipCrystalStones.EmptyCrystalStoneGrid();
                    PlayZone.Instance.UISplitEquipCrystalStones.RefreshEquip();
                }
                if (PlayZone.Instance.UISplitCrystalStone != null)
                {
                    PlayZone.Instance.UISplitCrystalStone.EmptyCrystalStoneGrid();
                    PlayZone.Instance.UISplitCrystalStone.RemoveSourceCrystalStone();
                }
            }
        }
		#endregion

		#region Luyện hóa trang bị
        /// <summary>
        /// Gửi yêu cầu luyện hóa trang bị
        /// </summary>
        /// <param name="equipGD"></param>
        /// <param name="recipeGD"></param>
        /// <param name="crystalStones"></param>
        /// <param name="productID"></param>
        public static void SendEquipRefine(GoodsData equipGD, GoodsData recipeGD, List<GoodsData> crystalStonesGD, int productID)
		{
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// Danh sách DbID Huyền Tinh
            List<int> crystalStones = crystalStonesGD.Select(x => x.Id).ToList();
            /// DbID trang bị luyện hóa
            int equipDbID = equipGD.Id;
            /// DbID công thức
            int recipeID = recipeGD.Id;

            /// Tạo mới gói tin
            G2C_EquipRefine equipRefine = new G2C_EquipRefine()
            {
                EquipDbID = equipDbID,
                RecipeDbID = recipeID,
                CrystalStoneDbIDs = crystalStones,
                ProductGoodsID = productID,
            };
            byte[] bytes = DataHelper.ObjectToBytes<G2C_EquipRefine>(equipRefine);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_CLIENT_DO_REFINE)));
		}

        /// <summary>
        /// Nhận gói tin thông báo luyện hóa trang bị thành công
        /// </summary>
        public static void ReceiveEquipRefine()
		{
            /// Đóng khung
            PlayZone.Instance.CloseUIEquipLevelUp();
        }
		#endregion

		#region Tách ngũ hành hồn thạch từ trang bị
        /// <summary>
        /// Gửi yêu cầu tách Ngũ hành hồn thạch từ trang bị
        /// </summary>
        /// <param name="equipGD"></param>
        public static void SendEquipRefineIntoFS(GoodsData equipGD)
		{
            if (!Global.Data.PlayGame)
            {
                return;
            }

            string strCmd = string.Format("{0}", equipGD.Id);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_SPLIT_EQUIP_INTO_FS)));
        }

        /// <summary>
        /// Nhận gói tin thông báo tách Ngũ Hành Hồn Thạch từ trang bị thành công
        /// </summary>
        public static void ReceiveEquipRefineIntoFS()
        {
            /// Nếu đang mở khung
            if (PlayZone.Instance.UIEquipRefineToFS != null)
			{
                PlayZone.Instance.UIEquipRefineToFS.ClearData();
			}
        }
        #endregion
    }
}
