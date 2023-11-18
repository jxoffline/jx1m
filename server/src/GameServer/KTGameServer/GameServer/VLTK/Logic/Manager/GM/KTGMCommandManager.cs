using GameServer.Entities.Skill.Other;
using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Core.Activity.CardMonth;
using GameServer.KiemThe.Core.Activity.DownloadBouns;
using GameServer.KiemThe.Core.Activity.EveryDayOnlineEvent;
using GameServer.KiemThe.Core.Activity.LevelUpEvent;
using GameServer.KiemThe.Core.Activity.LuckyCircle;
using GameServer.KiemThe.Core.Activity.PlayerPray;
using GameServer.KiemThe.Core.Activity.RechageEvent;
using GameServer.KiemThe.Core.Activity.SeashellCircle;
using GameServer.KiemThe.Core.Activity.TurnPlate;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents.CargoCarriage;
using GameServer.KiemThe.GameEvents.FactionBattle;
using GameServer.KiemThe.GameEvents.SpecialEvent;
using GameServer.KiemThe.GameEvents.TeamBattle;
using GameServer.KiemThe.Logic.Manager.Battle;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.KiemThe.LuaSystem;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.Activity.CheckPoint;
using GameServer.VLTK.Core.Activity.TopRankingEvent;
using GameServer.VLTK.Core.Activity.X2ExpEvent;
using GameServer.VLTK.Core.GuildManager;
using GameServer.VLTK.GameEvents.GrowTree;
using KF.Client;
using Server.Data;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Tmsk.Contract;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Lớp thực hiện lệnh GM
    /// </summary>
    public static class KTGMCommandManager
    {
        #region Khởi tạo

        /// <summary>
        /// Thông tin GM
        /// </summary>
        private class GMInfo
        {
            /// <summary>
            /// ID nhân vật
            /// </summary>
            public int RoleID { get; set; }

            /// <summary>
            /// Địa chỉ IP
            /// </summary>
            public string IP { get; set; }
        }

        /// <summary>
        /// Đối tượng trung gian sử dụng khóa LOCK
        /// </summary>
        private static readonly object Mutex = new object();

        /// <summary>
        /// Danh sách GM trong hệ thống
        /// </summary>
        private static readonly Dictionary<int, GMInfo> GMList = new Dictionary<int, GMInfo>();

        /// <summary>
        /// Địa chỉ IP mà tất cả người chơi đều là GM
        /// </summary>
        private static readonly HashSet<string> EverybodyAtSpecificIPAddressIsGM = new HashSet<string>();

        /// <summary>
        /// Tải danh sách GM mới nhất trong hệ thống
        /// </summary>
        public static void LoadGMList()
        {
            Console.WriteLine("Load GMList.xml");
            try
            {
                lock (KTGMCommandManager.Mutex)
                {
                    KTGMCommandManager.GMList.Clear();
                    KTGMCommandManager.EverybodyAtSpecificIPAddressIsGM.Clear();
                    string xmlText = File.ReadAllText("GMList.xml");
                    XElement xmlNode = XElement.Parse(xmlText);
                    foreach (XElement node in xmlNode.Elements("GM"))
                    {
                        string strRoleID = node.Attribute("RoleID").Value;
                        string strIPAddress = node.Attribute("IP").Value;
                        /// Nếu ID nhân vật là bất kỳ *
                        if (strRoleID == "*")
                        {
                            /// Nếu danh sách địa chỉ IP chưa chứa
                            if (!KTGMCommandManager.EverybodyAtSpecificIPAddressIsGM.Contains(strIPAddress))
                            {
                                KTGMCommandManager.EverybodyAtSpecificIPAddressIsGM.Add(strIPAddress);
                            }
                        }
                        else
                        {
                            GMInfo gmInfo = new GMInfo()
                            {
                                RoleID = int.Parse(strRoleID),
                                IP = strIPAddress,
                            };
                            KTGMCommandManager.GMList[gmInfo.RoleID] = gmInfo;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Load GMList.xml error\nException: {0}", ex.ToString()));
            }
        }

        #endregion Khởi tạo

        #region Kiểm tra

        /// <summary>
        /// Kiểm tra người chơi có ID tương ứng có phải GM không
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public static bool IsGM(TMSKSocket socket, int roleID)
        {
            lock (KTGMCommandManager.Mutex)
            {
                try
                {
                    /// Địa chỉ IP của người chơi
                    IPEndPoint ipEndPoint = socket.RemoteEndPoint as IPEndPoint;
                    string ipAddress = ipEndPoint.Address.ToString();

                    /// Nếu trong danh sách toàn bộ GM thuộc IP có dấu * hoặc IP tương ứng của người chơi này thì là GM
                    if (KTGMCommandManager.EverybodyAtSpecificIPAddressIsGM.Contains("*") || KTGMCommandManager.EverybodyAtSpecificIPAddressIsGM.Contains(ipAddress))
                    {
                        return true;
                    }

                    /// Nếu không có tên trong danh sách GM
                    if (!KTGMCommandManager.GMList.TryGetValue(roleID, out GMInfo gmInfo))
                    {
                        return false;
                    }

                    /// Nếu IP GM config là bất kỳ hoặc IP GM config trùng với IP người chơi thì là GM
                    return gmInfo.IP == "*" || gmInfo.IP == ipAddress;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public static bool IsGMByRoleID(int roleID)
        {
            /// Nếu không có tên trong danh sách GM
            if (!KTGMCommandManager.GMList.TryGetValue(roleID, out GMInfo gmInfo))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Kiểm tra người chơi có phải GM không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsGM(KPlayer player)
        {
            return KTGMCommandManager.IsGM(player.ClientSocket, player.RoleID);
        }

        #endregion Kiểm tra

        #region Thực thi lệnh GM

        /// <summary>
        /// Thực hiện lệnh GM
        /// </summary>
        /// <param name="player">Đối tượng người chơi</param>
        /// <param name="command">Chuỗi biểu diễn lệnh</param>
        public static void Process(KPlayer player, string command)
        {
            /// Nếu dữ liệu không chính xác
            if (player == null || string.IsNullOrEmpty(command))
            {
                return;
            }
            /// Nếu không phải GM
            else if (!KTGMCommandManager.IsGM(player))
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Process GM Command for {0}({1}) => ACCESS DENIED", player.RoleName, player.RoleID));
                return;
            }

            try
            {
                /// Chuẩn hóa
                command = command.Trim();
                while (command.IndexOf("  ") != -1)
                {
                    command = command.Replace("  ", " ");
                }

                /// Phân tích dữ liệu
                string[] para = command.Split(' ');

                /// Tên hàm
                string functionName = para[0];
                /// Tổng số tham biến
                int paramsCount = para.Length - 1;

                /// Kiểm tra và thực thi
                switch (functionName)
                {
                    #region Test

                    case "AllGoTo":
                        {
                            if (paramsCount == 2)
                            {
                                int posX = int.Parse(para[1]);
                                int posY = int.Parse(para[2]);

                                /// Người chơi cùng bản đồ
                                List<KPlayer> objsList = KTPlayerManager.FindAll(x => x.MapCode == player.MapCode);
                                /// Duyệt danh sách
                                foreach (KPlayer otherPlayer in objsList)
                                {
                                    /// Di chuyển đến
                                    KTPlayerManager.ChangePos(otherPlayer, posX, posY);
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AllGoTo' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Test

                    #region Vòng quay may mắn

                    case "ReloadTurnPlate":
                        {
                            if (paramsCount == 0)
                            {
                                KTTurnPlateManager.Init();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ReloadTurnPlate' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "SetTurnPlateTotalTurn":
                        {
                            if (paramsCount == 1)
                            {
                                int totalTurn = int.Parse(para[1]);
                                KTGMCommandManager.SetTurnPlateTotalTurn(player, totalTurn);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int totalTurn = int.Parse(para[2]);
                                KTGMCommandManager.SetLuckyCircleTotalTurn(targetID, totalTurn);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetTurnPlateTotalTurn' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "GetTurnPlateTotalTurn":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.GetLuckyCircleTotalTurn(player, player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.GetTurnPlateTotalTurn(player, targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GetTurnPlateTotalTurn' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "ReloadLuckyCircle":
                        {
                            if (paramsCount == 0)
                            {
                                KTLuckyCircleManager.Init();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ReloadLuckyCircle' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "SetLuckyCircleTotalTurn":
                        {
                            if (paramsCount == 1)
                            {
                                int totalTurn = int.Parse(para[1]);
                                KTGMCommandManager.SetLuckyCircleTotalTurn(player, totalTurn);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int totalTurn = int.Parse(para[2]);
                                KTGMCommandManager.SetLuckyCircleTotalTurn(targetID, totalTurn);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLuckyCircleTotalTurn' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "GetLuckyCircleTotalTurn":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.GetLuckyCircleTotalTurn(player, player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.GetLuckyCircleTotalTurn(player, targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GetLuckyCircleTotalTurn' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Vòng quay may mắn

                    #region Bách Bảo Rương

                    case "ReloadSeashellCircle":
                        {
                            if (paramsCount == 0)
                            {
                                KTSeashellCircleManager.Init();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ReloadSeashellCircle' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "SetSeashellTreasureNextTurn":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.SetSeashellTreasureNextTurn(player, -1);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.SetSeashellTreasureNextTurn(targetID, -1);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int bet = int.Parse(para[2]);
                                KTGMCommandManager.SetSeashellTreasureNextTurn(targetID, bet);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetSeashellTreasureNextTurn' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Bách Bảo Rương

                    #region Tải lại ServerConfig

                    case "ReloadServerConfig":
                        {
                            if (paramsCount == 0)
                            {
                                ServerConfig.Init();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ReloadServerConfig' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Tải lại ServerConfig

                    #region Script-Lua

                    case "LoadLua":
                        {
                            if (paramsCount == 1)
                            {
                                int scriptID = int.Parse(para[1]);
                                KTGMCommandManager.ReloadScriptLua(scriptID);
                                KTPlayerManager.ShowNotification(player, string.Format("Tải mới Script ID {0} thành công!", scriptID));
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'LoadLua' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Script-Lua

                    #region Bất tử GM

                    case "Invisiblity":
                        {
                            if (paramsCount == 1)
                            {
                                int state = int.Parse(para[1]);
                                player.GM_Invisiblity = state == 1;
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'Invisiblity' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "Immortality":
                        {
                            if (paramsCount == 1)
                            {
                                int state = int.Parse(para[1]);
                                player.GM_Immortality = state == 1;
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'Immortality' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Bất tử GM



                    #region Khóa mõm




                    case "BanFeature":
                        {
                            if (paramsCount == 3)
                            {
                                int roleID = int.Parse(para[1]);
                                int featureID = int.Parse(para[2]);
                                int durationSec = int.Parse(para[3]);

                                /// Toác
                                if (featureID < 1 || featureID > (int)RoleBannedFeature.Count)
                                {
                                    LogManager.WriteLog(LogTypes.Error, "Process GM Command 'BanFeature' Faild...\n" + "Invalid param.");
                                    break;
                                }

                                string banTypeString = "";
                                switch (featureID)
                                {
                                    case (int)RoleBannedFeature.Exchange:
                                        {
                                            banTypeString = "giao dịch";
                                            break;
                                        }
                                    case (int)RoleBannedFeature.SellItem:
                                        {
                                            banTypeString = "bán vật phẩm";
                                            break;
                                        }
                                    case (int)RoleBannedFeature.ThrowItem:
                                        {
                                            banTypeString = "vứt vật phẩm";
                                            break;
                                        }
                                    case (int)RoleBannedFeature.SaleGoods:
                                        {
                                            banTypeString = "bày bán";
                                            break;
                                        }
                                }

                                /// Người chơi tương ứng
                                KPlayer targetPlayer = KTPlayerManager.Find(roleID);
                                /// Nếu tìm thấy
                                if (targetPlayer != null)
                                {
                                    /// Gửi yêu cầu lên GameDB
                                    if (KT_TCPHandler.SendDBBanPlayerByType(0, (RoleBannedFeature)featureID, targetPlayer, durationSec * 1000L, string.Format("By GM - [{0}]", player.RoleName)))
                                    {
                                        /// Thêm cấm
                                        targetPlayer.AddBanFeature((RoleBannedFeature)featureID, KTGlobal.GetCurrentTimeMilis(), durationSec * 1000L, string.Format("By GM - [{0}]", player.RoleName));
                                        /// Thời hạn cấm
                                        string banDurationString = durationSec == -1 ? "vĩnh viễn" : KTGlobal.DisplayFullDateAndTime(durationSec);
                                        /// Gửi tin nhắn thông báo bị cấm
                                        KT_TCPHandler.SendChatMessage(targetPlayer, null, targetPlayer, string.Format("Bạn đã bị cấm <color=green>{2}</color> bởi <color=#26b5f2>[{0}]</color>, thời hạn <color=green>{1}</color>.", player.RoleName, banDurationString, banTypeString), ChatChannel.System, null, null);

                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm người chơi " + banTypeString + " thành công!");
                                    }
                                    /// Toác
                                    else
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm người chơi " + banTypeString + " thất bại!");
                                    }
                                }
                                /// Không tìm thấy
                                else
                                {
                                    /// Gửi yêu cầu lên GameDB
                                    if (KT_TCPHandler.SendDBBanPlayerByType(0, (RoleBannedFeature)featureID, roleID, player.ServerId, durationSec * 1000L, string.Format("By GM - [{0}]", player.RoleName)))
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm người chơi " + banTypeString + " thành công!");
                                    }
                                    /// Toác
                                    else
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm người chơi " + banTypeString + " thất bại!");
                                    }
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'BanFeature' Faild...\n" + "Invalid param.");
                            }

                            break;
                        }
                    case "UnbanFeature":
                        {
                            if (paramsCount == 2)
                            {
                                int roleID = int.Parse(para[1]);
                                int featureID = int.Parse(para[2]);

                                /// Toác
                                if (featureID < 1 || featureID > (int)RoleBannedFeature.Count)
                                {
                                    LogManager.WriteLog(LogTypes.Error, "Process GM Command 'BanFeature' Faild...\n" + "Invalid param.");
                                    break;
                                }

                                string banTypeString = "";
                                switch (featureID)
                                {
                                    case (int)RoleBannedFeature.Exchange:
                                        {
                                            banTypeString = "giao dịch";
                                            break;
                                        }
                                    case (int)RoleBannedFeature.SellItem:
                                        {
                                            banTypeString = "bán vật phẩm";
                                            break;
                                        }
                                    case (int)RoleBannedFeature.ThrowItem:
                                        {
                                            banTypeString = "vứt vật phẩm";
                                            break;
                                        }
                                    case (int)RoleBannedFeature.SaleGoods:
                                        {
                                            banTypeString = "bày bán";
                                            break;
                                        }
                                }

                                /// Người chơi tương ứng
                                KPlayer targetPlayer = KTPlayerManager.Find(roleID);

                                /// Gửi yêu cầu lên GameDB
                                if (KT_TCPHandler.SendDBBanPlayerByType(1, (RoleBannedFeature)featureID, targetPlayer, 0, ""))
                                {
                                    /// Nếu tìm thấy
                                    if (targetPlayer != null)
                                    {
                                        /// Hủy cấm
                                        targetPlayer.RemoveBanFeature((RoleBannedFeature)featureID);
                                        /// Gửi tin nhắn thông báo bị cấm chat
                                        KT_TCPHandler.SendChatMessage(targetPlayer, null, targetPlayer, string.Format("Bạn đã được hủy cấm {1} bởi <color=#26b5f2>[{0}]</color>.", player.RoleName, banTypeString), ChatChannel.System, null, null);
                                    }
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(player, "Hủy cấm người chơi " + banTypeString + " thành công!");
                                }
                                else
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(player, "Hủy cấm người chơi " + banTypeString + " thất bại!");
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'UnbanFeature' Faild...\n" + "Invalid param.");
                            }

                            break;
                        }

                    case "BanLogin":
                        {
                            if (paramsCount >= 3)
                            {
                                int roleID = int.Parse(para[1]);
                                int durationSec = int.Parse(para[2]);

                                /// Lý do bị Ban
                                string reason = "";
                                /// Duyệt danh sách
                                for (int i = 3; i < para.Length; i++)
                                {
                                    reason += para[i] + " ";
                                }
                                reason = reason.Trim();

                                /// Người chơi tương ứng
                                KPlayer targetPlayer = KTPlayerManager.Find(roleID);
                                /// Nếu tìm thấy
                                if (targetPlayer != null)
                                {
                                    /// Gửi yêu cầu lên GameDB
                                    if (KT_TCPHandler.SendDBBanPlayer(0, targetPlayer, durationSec * 1000L, reason, string.Format("By GM - [{0}]", player.RoleName)))
                                    {
                                        /// Kick
                                        Global.ForceCloseClient(targetPlayer, "GMKICK");
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm người chơi đăng nhập thành công!");
                                    }
                                    /// Toác
                                    else
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm người chơi đăng nhập thất bại!");
                                    }
                                }
                                /// Không tìm thấy
                                else
                                {
                                    /// Gửi yêu cầu lên GameDB
                                    if (KT_TCPHandler.SendDBBanPlayer(0, roleID, player.ServerId, durationSec * 1000L, reason, string.Format("By GM - [{0}]", player.RoleName)))
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm người chơi đăng nhập thành công!");
                                    }
                                    /// Toác
                                    else
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm người chơi đăng nhập thất bại!");
                                    }
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'BanLogin' Faild...\n" + "Invalid param.");
                            }

                            break;
                        }
                    case "UnbanLogin":
                        {
                            if (paramsCount == 1)
                            {
                                int roleID = int.Parse(para[1]);

                                /// Gửi yêu cầu lên GameDB
                                if (KT_TCPHandler.SendDBBanPlayer(2, roleID, player.ServerId, 0, "", ""))
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(player, "Bỏ cấm người chơi đăng nhập thành công!");
                                }
                                /// Toác
                                else
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(player, "Bỏ cấm người chơi đăng nhập thất bại!");
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'UnbanLogin' Faild...\n" + "Invalid param.");
                            }

                            break;
                        }

                    case "BanChat":
                        {
                            if (paramsCount >= 3)
                            {
                                int roleID = int.Parse(para[1]);
                                int durationSec = int.Parse(para[2]);

                                /// Lý do bị Ban
                                string reason = "";
                                /// Duyệt danh sách
                                for (int i = 3; i < para.Length; i++)
                                {
                                    reason += para[i] + " ";
                                }
                                reason = reason.Trim();

                                /// Người chơi tương ứng
                                KPlayer targetPlayer = KTPlayerManager.Find(roleID);
                                /// Nếu tìm thấy
                                if (targetPlayer != null)
                                {
                                    /// Gửi yêu cầu lên GameDB
                                    if (KT_TCPHandler.SendDBBanPlayer(1, targetPlayer, durationSec * 1000L, reason, string.Format("By GM - [{0}]", player.RoleName)))
                                    {
                                        /// Cập nhật thời gian cấm
                                        targetPlayer.BanChatStartTime = KTGlobal.GetCurrentTimeMilis();
                                        targetPlayer.BanChatDuration = durationSec * 1000L;
                                        /// Thời hạn cấm
                                        string banDurationString = durationSec == -1 ? "vĩnh viễn" : KTGlobal.DisplayFullDateAndTime(durationSec);
                                        /// Gửi tin nhắn thông báo bị cấm chat
                                        KT_TCPHandler.SendChatMessage(targetPlayer, null, targetPlayer, string.Format("Bạn đã bị cấm Chat bởi <color=#26b5f2>[{0}]</color>, thời hạn <color=green>{1}</color>, lý do <color=yellow>'{2}'</color>.", player.RoleName, banDurationString, reason), ChatChannel.System, null, null);
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm chat người chơi thành công!");
                                    }
                                    /// Toác
                                    else
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm chat người chơi thất bại!");
                                    }
                                }
                                /// Không tìm thấy
                                else
                                {
                                    /// Gửi yêu cầu lên GameDB
                                    if (KT_TCPHandler.SendDBBanPlayer(1, roleID, player.ServerId, durationSec * 1000L, reason, string.Format("By GM - [{0}]", player.RoleName)))
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm chat người chơi thành công!");
                                    }
                                    /// Toác
                                    else
                                    {
                                        /// Thông báo
                                        KTPlayerManager.ShowNotification(player, "Cấm chat người chơi thất bại!");
                                    }
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'BanChat' Faild...\n" + "Invalid param.");
                            }

                            break;
                        }
                    case "UnbanChat":
                        {
                            if (paramsCount == 1)
                            {
                                int roleID = int.Parse(para[1]);
                                /// Người chơi tương ứng
                                KPlayer targetPlayer = KTPlayerManager.Find(roleID);

                                /// Gửi yêu cầu lên GameDB
                                if (KT_TCPHandler.SendDBBanPlayer(3, roleID, player.ServerId, 0, "", ""))
                                {
                                    /// Nếu tồn tại
                                    if (targetPlayer != null)
                                    {
                                        targetPlayer.BanChatDuration = 0;
                                        targetPlayer.BanChatStartTime = 0;
                                        /// Gửi tin nhắn thông báo bị cấm chat
                                        KT_TCPHandler.SendChatMessage(targetPlayer, null, targetPlayer, string.Format("Bạn đã được hủy cấm Chat bởi <color=#26b5f2>[{0}]</color>.", player.RoleName), ChatChannel.System, null, null);
                                    }
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(player, "Bỏ cấm chat người chơi thành công!");
                                }
                                /// Toác
                                else
                                {
                                    /// Thông báo
                                    KTPlayerManager.ShowNotification(player, "Bỏ cấm chat người chơi thất bại!");
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'UnbanChat' Faild...\n" + "Invalid param.");
                            }

                            break;
                        }

                    #endregion Khóa mõm

                    #region Tạo Captcha

                    case "GenerateCaptcha":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.GenerateCaptcha(player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.GenerateCaptcha(targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GenerateCaptcha' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Tạo Captcha

                    #region Trạng thái ngũ hành

                    case "AddSeriesState":
                        {
                            if (paramsCount == 2)
                            {
                                string stateID = para[1];
                                float time = float.Parse(para[2]);
                                KTGMCommandManager.AddSeriesState(player, stateID, time);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                string stateID = para[2];
                                float time = float.Parse(para[3]);
                                KTGMCommandManager.AddSeriesState(targetID, stateID, time);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSeriesState' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }
                    case "RemoveSeriesState":
                        {
                            if (paramsCount == 1)
                            {
                                string stateID = para[1];
                                KTGMCommandManager.RemoveSeriesState(player, stateID);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                string stateID = para[2];
                                KTGMCommandManager.RemoveSeriesState(targetID, stateID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'RemoveSeriesState' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Trạng thái ngũ hành

                    #region Trị liệu

                    case "SetHp":
                        {
                            player.m_CurrentLife = 100;
                            break;
                        }
                           

                    case "Heal":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.Heal(player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.Heal(targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'Heal' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Trị liệu

                    #region Tải lại danh sách GM

                    case "ReloadGMList":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.LoadGMList();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ReloadGMList' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Tải lại danh sách GM

                    #region ID người chơi

                    case "PlayerInfo":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.GetPlayerInfo(player, player.RoleName);
                            }
                            else if (paramsCount == 1)
                            {
                                string targetName = para[1];
                                KTGMCommandManager.GetPlayerInfo(player, targetName);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'PlayerInfo' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion ID người chơi

                    #region Dịch chuyển

                    case "GoTo":
                        {
                            if (paramsCount == 2)
                            {
                                int posX = int.Parse(para[1]);
                                int posY = int.Parse(para[2]);
                                KTGMCommandManager.GoTo(player, player.CurrentMapCode, posX, posY);
                            }
                            else if (paramsCount == 3)
                            {
                                int mapCode = int.Parse(para[1]);
                                int posX = int.Parse(para[2]);
                                int posY = int.Parse(para[3]);
                                KTGMCommandManager.GoTo(player, mapCode, posX, posY);
                            }
                            else if (paramsCount == 4)
                            {
                                int targetID = int.Parse(para[1]);
                                int mapCode = int.Parse(para[2]);
                                int posX = int.Parse(para[3]);
                                int posY = int.Parse(para[4]);
                                KTGMCommandManager.GoTo(targetID, mapCode, posX, posY);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GoTo' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Dịch chuyển

                    #region Kỹ năng và Buff

                    case "AddSkill":
                        {
                            if (paramsCount == 2)
                            {
                                int skillID = int.Parse(para[1]);
                                int level = int.Parse(para[2]);
                                KTGMCommandManager.AddSkill(player, skillID, level);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int skillID = int.Parse(para[2]);
                                int level = int.Parse(para[3]);
                                KTGMCommandManager.AddSkill(targetID, skillID, level);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSkill' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }
                    case "RemoveSkill":
                        {
                            if (paramsCount == 1)
                            {
                                int skillID = int.Parse(para[1]);
                                KTGMCommandManager.RemoveSkill(player, skillID);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int skillID = int.Parse(para[2]);
                                KTGMCommandManager.RemoveSkill(targetID, skillID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'RemoveSkill' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }
                    case "AddBuff":
                        {
                            if (paramsCount == 2)
                            {
                                int skillID = int.Parse(para[1]);
                                int level = int.Parse(para[2]);
                                KTGMCommandManager.AddBuff(player, skillID, level);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int skillID = int.Parse(para[2]);
                                int level = int.Parse(para[3]);
                                KTGMCommandManager.AddBuff(targetID, skillID, level);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddBuff' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }
                    case "RemoveBuff":
                        {
                            if (paramsCount == 1)
                            {
                                int skillID = int.Parse(para[1]);
                                KTGMCommandManager.RemoveBuff(player, skillID);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int skillID = int.Parse(para[2]);
                                KTGMCommandManager.RemoveBuff(targetID, skillID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'RemoveBuff' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }
                    case "ResetAllSkillCooldown":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.ResetAllSkillCooldown(player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.ResetAllSkillCooldown(targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetAllSkillCooldown' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "AddSkillExp":
                        {
                            if (paramsCount == 2)
                            {
                                int skillID = int.Parse(para[1]);
                                int exp = int.Parse(para[2]);
                                KTGMCommandManager.AddSkillExp(player, skillID, exp);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int skillID = int.Parse(para[2]);
                                int exp = int.Parse(para[3]);
                                KTGMCommandManager.AddSkillExp(targetID, skillID, exp);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSkillExp' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Kỹ năng và Buff

                    #region System chat

                    case "SysChat":
                        {
                            string message = command.Substring(functionName.Length + 1);
                            KTGMCommandManager.SendSystemChat(message);
                            break;
                        }
                    case "SysNotify":
                        {
                            string message = command.Substring(functionName.Length + 1);
                            KTGMCommandManager.SendSystemEventNotification(message);
                            break;
                        }

                    #endregion System chat

                    #region Tạo vật phẩm

                    case "CreateItem":
                        {
                            if (paramsCount == 2)
                            {
                                int itemID = int.Parse(para[1]);
                                int quantity = int.Parse(para[2]);
                                KTGMCommandManager.CreateItem(player, itemID, quantity, -1);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int itemID = int.Parse(para[2]);
                                int quantity = int.Parse(para[3]);
                                KTGMCommandManager.CreateItem(targetID, itemID, quantity);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreateItem' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "CreateItemEx":
                        {
                            if (paramsCount == 4)
                            {
                                int itemID = int.Parse(para[1]);
                                int quantity = int.Parse(para[2]);
                                int linescount = int.Parse(para[3]);
                                int series = int.Parse(para[4]);
                                KTGMCommandManager.CreateItemEx(player, itemID, quantity, linescount, series);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreateItem' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "CreateItemExp":
                        {
                            if (paramsCount == 3)
                            {
                                int itemID = int.Parse(para[1]);
                                int quantity = int.Parse(para[2]);
                                int EXP = int.Parse(para[3]);
                                KTGMCommandManager.CreateItem(player, itemID, quantity, EXP);
                            }
                            else if (paramsCount == 4)
                            {
                                int targetID = int.Parse(para[1]);
                                int itemID = int.Parse(para[2]);
                                int quantity = int.Parse(para[3]);
                                int EXP = int.Parse(para[4]);
                                KTGMCommandManager.CreateItem(targetID, itemID, quantity);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreateItem' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Tạo vật phẩm

                    #region Tạo quái

                    case "CreateMonster":
                        {
                            if (paramsCount == 2)
                            {
                                int monsterID = int.Parse(para[1]);
                                int monsterType = int.Parse(para[2]);

                                /// Tạo quái
                                KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                                {
                                    MapCode = player.CurrentMapCode,
                                    CopySceneID = player.CurrentCopyMapID,
                                    ResID = monsterID,
                                    PosX = player.PosX,
                                    Camp = 65535,
                                    PosY = player.PosY,
                                    MonsterType = (MonsterAIType)monsterType,
                                });
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreateMonster' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Tạo quái

                    #region Xóa toàn bộ quái trong bản đồ

                    case "RemoveAllMonsters":
                        {
                            if (paramsCount == 0)
                            {
                                /// Danh sách đối tượng trong bản đồ
                                List<Monster> objs = KTMonsterManager.GetMonstersAtMap(player.CurrentMapCode, player.CurrentCopyMapID);
                                /// Duyệt danh sách
                                foreach (Monster obj in objs)
                                {
                                    if (!obj.IsDead())
                                    {
                                        KTMonsterManager.Remove(obj);
                                    }
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'RemoveAllMonsters' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Xóa toàn bộ quái trong bản đồ

                    #region Thiết lập cấp cường hóa cho trang bị

                    case "EquipEnhance":
                        {
                            if (paramsCount == 2)
                            {
                                int slot = int.Parse(para[1]);
                                int level = int.Parse(para[2]);
                                KTGMCommandManager.EquipEnhance(player, slot, level);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int slot = int.Parse(para[2]);
                                int level = int.Parse(para[3]);
                                KTGMCommandManager.EquipEnhance(targetID, slot, level);
                            }
                            break;
                        }

                    #endregion Thiết lập cấp cường hóa cho trang bị

                    #region Thêm tiền

                    case "AddMoney":
                        {
                            if (paramsCount == 2)
                            {
                                int type = int.Parse(para[1]);
                                int amount = int.Parse(para[2]);
                                KTGMCommandManager.AddMoney(player, type, amount);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int type = int.Parse(para[2]);
                                int amount = int.Parse(para[3]);
                                KTGMCommandManager.AddMoney(targetID, type, amount);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddMoney' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "ResetGuildRecoreMoney":
                        {
                            Global.SaveRoleParamsInt32ValueToDB(player, RoleParamName.TotalGuildMoneyAdd, 0, true);
                            Global.SaveRoleParamsInt32ValueToDB(player, RoleParamName.TotalGuildMoneyWithDraw, 0, true);

                            break;
                        }

                    #endregion Thêm tiền

                    #region Thêm vật phẩm rơi ở MAP

                    case "AddDropItem":
                        {
                            if (paramsCount == 1)
                            {
                                int monsterID = int.Parse(para[1]);
                                KTGMCommandManager.AddDropItem(player, monsterID);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int monsterID = int.Parse(para[2]);
                                KTGMCommandManager.AddDropItem(targetID, monsterID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddDropItem' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Thêm vật phẩm rơi ở MAP

                    #region Hoạt động

                    case "StartActivity":
                        {
                            if (paramsCount == 1)
                            {
                                int activityID = int.Parse(para[1]);
                                KTGMCommandManager.StartActivity(activityID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'StartActivity' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }
                    case "StopActivity":
                        {
                            if (paramsCount == 1)
                            {
                                int activityID = int.Parse(para[1]);
                                KTGMCommandManager.StopActivity(activityID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'StopActivity' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Hoạt động

                    #region Sát khí

                    case "SetPKValue":
                        {
                            if (paramsCount == 1)
                            {
                                int value = int.Parse(para[1]);
                                KTGMCommandManager.SetPKValue(player, value);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int value = int.Parse(para[2]);
                                KTGMCommandManager.SetPKValue(targetID, value);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetPKValue' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Sát khí

                    #region Kỹ năng sống

                    case "ResetLifeSkillLevelAndExp":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.ResetLifeSkillLevelAndExp(player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.ResetLifeSkillLevelAndExp(targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetLifeSkillLevelAndExp' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "SetLifeSkillLevelAndExp":
                        {
                            if (paramsCount == 3)
                            {
                                int lifeSkillID = int.Parse(para[1]);
                                int level = int.Parse(para[2]);
                                int exp = int.Parse(para[3]);
                                KTGMCommandManager.SetLifeSkillLevelAndExp(player, lifeSkillID, level, exp);
                            }
                            else if (paramsCount == 4)
                            {
                                int targetID = int.Parse(para[1]);
                                int lifeSkillID = int.Parse(para[2]);
                                int level = int.Parse(para[3]);
                                int exp = int.Parse(para[4]);
                                KTGMCommandManager.SetLifeSkillLevelAndExp(targetID, lifeSkillID, level, exp);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLifeSkillLevelAndExp' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }
                    case "AddGatherMakePoint":
                        {
                            if (paramsCount == 2)
                            {
                                int gatherPoint = int.Parse(para[1]);
                                int makePoint = int.Parse(para[2]);
                                KTGMCommandManager.AddGatherMakePoint(player, gatherPoint, makePoint);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int gatherPoint = int.Parse(para[2]);
                                int makePoint = int.Parse(para[3]);
                                KTGMCommandManager.AddGatherMakePoint(targetID, gatherPoint, makePoint);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddGatherMakePoint' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Kỹ năng sống

                    #region Danh hiệu

                    case "SetTempTitle":
                        {
                            if (paramsCount == 1)
                            {
                                string title = para[1];
                                KTGMCommandManager.SetTempTitle(player, title.Replace("_", " "));
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                string title = para[2];
                                KTGMCommandManager.SetTempTitle(targetID, title.Replace("_", " "));
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetTempTitle' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Danh hiệu

                    #region Thư

                    case "SendSystemMail":
                        {
                            if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                string title = para[2].Replace('_', ' ');
                                string content = para[3].Replace('_', ' ');
                                /// Nếu có mục tiêu
                                if (targetID != -1)
                                {
                                    KTGMCommandManager.SendSystemMail(targetID, title, content, "", 0, 0);
                                }
                                else
                                {
                                    KTGMCommandManager.SendSystemMail(player, title, content, "", 0, 0);
                                }
                            }
                            else if (paramsCount == 4)
                            {
                                int targetID = int.Parse(para[1]);
                                string title = para[2].Replace('_', ' ');
                                string content = para[3].Replace('_', ' ');
                                string items = para[4];
                                /// Nếu có mục tiêu
                                if (targetID != -1)
                                {
                                    KTGMCommandManager.SendSystemMail(targetID, title, content, items, 0, 0);
                                }
                                else
                                {
                                    KTGMCommandManager.SendSystemMail(player, title, content, items, 0, 0);
                                }
                            }
                            else if (paramsCount == 5)
                            {
                                int targetID = int.Parse(para[1]);
                                string title = para[2].Replace('_', ' ');
                                string content = para[3].Replace('_', ' ');
                                int boundMoney = int.Parse(para[4]);
                                int boundToken = int.Parse(para[5]);
                                /// Nếu có mục tiêu
                                if (targetID != -1)
                                {
                                    KTGMCommandManager.SendSystemMail(targetID, title, content, "", boundMoney, boundToken);
                                }
                                else
                                {
                                    KTGMCommandManager.SendSystemMail(player, title, content, "", boundMoney, boundToken);
                                }
                            }
                            else if (paramsCount == 6)
                            {
                                int targetID = int.Parse(para[1]);
                                string title = para[2].Replace('_', ' ');
                                string content = para[3].Replace('_', ' ');
                                string items = para[4];
                                int boundMoney = int.Parse(para[5]);
                                int boundToken = int.Parse(para[6]);
                                /// Nếu có mục tiêu
                                if (targetID != -1)
                                {
                                    KTGMCommandManager.SendSystemMail(targetID, title, content, items, boundMoney, boundToken);
                                }
                                else
                                {
                                    KTGMCommandManager.SendSystemMail(player, title, content, items, boundMoney, boundToken);
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SendSystemMail' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Thư

                    #region Kinh nghiệm và cấp độ

                    case "AddExp":
                        {
                            if (paramsCount == 1)
                            {
                                int exp = int.Parse(para[1]);
                                KTGMCommandManager.AddExp(player, exp);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int exp = int.Parse(para[2]);
                                KTGMCommandManager.AddExp(targetID, exp);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddExp' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "SetLevel":
                        {
                            if (paramsCount == 1)
                            {
                                int level = int.Parse(para[1]);
                                KTGMCommandManager.SetLevel(player, level);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int level = int.Parse(para[2]);
                                KTGMCommandManager.SetLevel(targetID, level);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLevel' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Kinh nghiệm và cấp độ

                    #region Vào phái

                    case "JoinFaction":
                        {
                            if (paramsCount == 1)
                            {
                                int factionID = int.Parse(para[1]);
                                KTGMCommandManager.JoinFaction(player, factionID);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int factionID = int.Parse(para[2]);
                                KTGMCommandManager.JoinFaction(targetID, factionID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'JoinFaction' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Vào phái

                    #region Battle

                    case "StopServer":
                        {
                            Program.Exit();
                        }
                        break;

                    case "CreateFireCamp":
                        {
                            GameMap Map = KTMapManager.Find(player.MapCode);
                            KTMonsterManager.FireCampManager.CreateFireWood(player, player.CurrentMapCode, player.CurrentCopyMapID, player.PosX, player.PosY, () =>
                            {
                                KTPlayerManager.ShowNotification(player, "Destroyed");
                            });
                        }
                        break;

                    case "GlowTreeStart":
                        {
                            GrowTreeManager.GMFORCESTARTEVENT();
                        }
                        break;

                    case "GlowTreeStop":
                        {
                            GrowTreeManager.GMSTOPEVENT();
                        }
                        break;

                    case "SetMainTask":
                        {
                            if (paramsCount == 1)
                            {
                                int State = int.Parse(para[1]);

                                ProcessTask.GMSetMainTaskID(player, State);
                            }
                            else if (paramsCount == 2)
                            {
                                string otherRoleName = para[1];

                                KPlayer otherClient = KTPlayerManager.Find(otherRoleName);

                                if (null != otherClient)
                                {
                                    int State = int.Parse(para[2]);

                                    ProcessTask.GMSetMainTaskID(otherClient, State);
                                }
                            }
                        }
                        break;

                    case "ResetBVD":
                        {
                            if (paramsCount == 1)
                            {
                                string otherRoleName = para[1];

                                KPlayer otherClient = KTPlayerManager.Find(otherRoleName);

                                if (otherClient != null)
                                {
                                    //otherClient.CurenQuestIDBVD = -1;
                                    //otherClient.CanncelQuestBVD = 0;
                                    //otherClient.QuestBVDStreakCount = 0;
                                    //otherClient.QuestBVDTodayCount = 0;

                                    if (otherClient.TaskDataList != null)
                                    {
                                        // Đoạn này để tránh bug nếu mà nhiệm vụ hiện tại khác nhiệm vụ đã nhận trước đó
                                        foreach (TaskData TaskArmy in otherClient.TaskDataList)
                                        {
                                            Task _Task = TaskDailyArmyManager.getInstance().GetTaskTemplate(TaskArmy.DoingTaskID);

                                            //Tức là đang có nhiệm vụ BVD đang nhận
                                            if (_Task != null && _Task.TaskClass == (int)TaskClasses.NghiaQuan)
                                            {
                                                TaskDailyArmyManager.getInstance().CancelTask(otherClient, TaskArmy.DbID, TaskArmy.DoingTaskID);
                                            }
                                        }
                                    }

                                    Global.ForceCloseClient(otherClient, "GMKICK");
                                }
                            }
                        }
                        break;

                    case "ResetHaiTac":
                        {
                            if (paramsCount == 1)
                            {
                                string otherRoleName = para[1];

                                KPlayer otherClient = KTPlayerManager.Find(otherRoleName);

                                if (otherClient != null)
                                {
                                    PirateTaskManager.getInstance().SetQuestIDDay(otherClient, -1);
                                    PirateTaskManager.getInstance().SetNumQuestThisDay(otherClient, 0);

                                    if (otherClient.TaskDataList != null)
                                    {
                                        // Đoạn này để tránh bug nếu mà nhiệm vụ hiện tại khác nhiệm vụ đã nhận trước đó
                                        foreach (TaskData TaskArmy in otherClient.TaskDataList)
                                        {
                                            Task _Task = PirateTaskManager.getInstance().GetTaskTemplate(TaskArmy.DoingTaskID);

                                            //Tức là đang có nhiệm vụ BVD đang nhận
                                            if (_Task != null && _Task.TaskClass == (int)TaskClasses.HaiTac)
                                            {
                                                PirateTaskManager.getInstance().CancelTask(otherClient, TaskArmy.DbID, TaskArmy.DoingTaskID);
                                            }
                                        }
                                    }
                                    Global.ForceCloseClient(otherClient, "GMKICK");
                                }
                            }
                        }
                        break;

                    case "ResetThuongHoi":
                        {
                            if (paramsCount == 1)
                            {
                                string otherRoleName = para[1];

                                KPlayer otherClient = KTPlayerManager.Find(otherRoleName);

                                FirmTaskManager.getInstance().SetNumQuestThisWeek(otherClient, 0);
                            }
                        }
                        break;

                    case "BattleStateTest":
                        {
                            int State = int.Parse(para[1]);

                            G2C_EventState _State = new G2C_EventState();
                            _State.EventID = 50;
                            _State.State = State;

                            player.SendPacket<G2C_EventState>((int)TCPGameServerCmds.CMD_KT_EVENT_STATE, _State);

                            break;
                        }

                    case "StartBattle":
                        {
                            int Level = int.Parse(para[1]);

                            Battel_SonJin_Manager.ForceStartBattle(Level);

                            break;
                        }

                    case "StartFactionBattle":
                        {
                            FactionBattleManager.ForceStartBattle();
                        }
                        break;

                    case "EndFactionBattle":
                        {
                            FactionBattleManager.ForceEndBattle();
                        }
                        break;

                    case "DoneTask":
                        {
                            int TaskID = int.Parse(para[1]);

                            var findtask = player.TaskDataList.Where(x => x.DoingTaskID == TaskID).FirstOrDefault();
                            if (findtask != null)
                            {
                                findtask.DoingTaskVal1 = 100;

                                GameMap map = KTMapManager.Find(player.MapCode);
                                if (map == null)
                                {
                                    KTPlayerManager.ShowNotification(player, "Không lấy được bản đồ đang đứng");
                                }

                                Task FindTask = TaskManager.getInstance().FindTaskById(TaskID);
                                if (FindTask != null)
                                {
                                    NPC npc = KTNPCManager.Find(x => x.MapCode == player.MapCode && x.CopyMapID == player.CopyMapID && x.ResID == FindTask.DestNPC);

                                    if (npc != null)
                                    {
                                        if (FindTask.TaskClass == (int)TaskClasses.MainTask)
                                        {
                                            MainTaskManager.getInstance().CompleteTask(map, npc, player, TaskID);
                                        }
                                        else if (FindTask.TaskClass == (int)TaskClasses.HaiTac)
                                        {
                                            PirateTaskManager.getInstance().CompleteTask(map, npc, player, TaskID);
                                        }
                                        else if (FindTask.TaskClass == (int)TaskClasses.NghiaQuan)
                                        {
                                            TaskDailyArmyManager.getInstance().CompleteTask(map, npc, player, TaskID);
                                        }
                                    }
                                    else
                                    {
                                        npc = KTNPCManager.Find(x => x.MapCode == 8 && x.CopyMapID == player.CopyMapID && x.ResID == 6786);

                                        if (FindTask.TaskClass == (int)TaskClasses.MainTask)
                                        {
                                            MainTaskManager.getInstance().CompleteTask(map, npc, player, TaskID);
                                        }
                                        else if (FindTask.TaskClass == (int)TaskClasses.HaiTac)
                                        {
                                            PirateTaskManager.getInstance().CompleteTask(map, npc, player, TaskID);
                                        }
                                        else if (FindTask.TaskClass == (int)TaskClasses.NghiaQuan)
                                        {
                                            TaskDailyArmyManager.getInstance().CompleteTask(map, npc, player, TaskID);
                                        }
                                    }
                                }
                            }

                            break;
                        }

                    case "EndBattle":
                        {
                            int Level = int.Parse(para[1]);

                            if (Level < 1 || Level > 3)
                            {
                                KTPlayerManager.ShowNotification(player, "Cấp chiến trương không hợp lệ");
                            }
                            else
                            {
                                Battel_SonJin_Manager.ForceEndBattle(Level);
                            }
                            break;
                        }

                    case "KillEffectTest":
                        {
                            int COUNT = int.Parse(para[1]);

                            G2C_KillStreak _State = new G2C_KillStreak();

                            _State.KillNumber = COUNT;

                            player.SendPacket<G2C_KillStreak>((int)TCPGameServerCmds.CMD_KT_KILLSTREAK, _State);

                            break;
                        }

                    case "NotifyTest":
                        {
                            G2C_EventNotification _Notify = new G2C_EventNotification();
                            _Notify.EventName = "Tống Kim Công Báo";
                            _Notify.ShortDetail = "TIME|500";
                            _Notify.TotalInfo = new List<string>();

                            _Notify.TotalInfo.Add("Giết Địch : 100");
                            _Notify.TotalInfo.Add("Bị Giết : 100");

                            _Notify.TotalInfo.Add("Tích Lũy : 10550");

                            _Notify.TotalInfo.Add("Hạng Hiện Tại  : 1");

                            player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);

                            break;
                        }

                    case "CheckSigNet":
                        {
                            int TotalBS = int.Parse(para[1]);

                            int LevelStart = int.Parse(para[2]);

                            int ExpStart = int.Parse(para[3]);

                            ItemEnhance.Caclulation(TotalBS, LevelStart, ExpStart);

                            break;
                        }

                    #endregion Battle

                    #region LIENSV

                    case "GetLine":
                        {
                            int MapCode = int.Parse(para[1]);

                            try
                            {
                                List<KuaFuLineData> list = YongZheZhanChangClient.getInstance().GetKuaFuLineDataList(MapCode) as List<KuaFuLineData>;

                                Console.WriteLine(list);

                                player.SendPacket((int)(TCPGameServerCmds.CMD_SPR_KUAFU_MAP_INFO), list);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }

                            break;
                        }

                    case "TryEnter":
                        {
                            int MapCode = int.Parse(para[1]);
                            int Lines = int.Parse(para[2]);

                            try
                            {
                                string[] cmdParams = new string[2];
                                cmdParams[0] = MapCode + "";
                                cmdParams[1] = Lines + "";
                                KuaFuMapManager.getInstance().ProcessKuaFuMapEnterCmd(player, (int)(TCPGameServerCmds.CMD_SPR_KUAFU_MAP_ENTER), null, cmdParams);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }

                            break;
                        }

                    #endregion LIENSV

                    #region Kiểm tra thông tin hệ thống

                    case "CheckCcu":
                        {
                            try
                            {
                                KTPlayerManager.ShowNotification(player, "CCU ONLINE: " + KTPlayerManager.GetPlayersCount());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }

                            break;
                        }
                    case "CheckCcu2":
                        {
                            try
                            {
                                string[] dbFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_GETTOTALONLINENUM, string.Format("{0}", player.RoleID), GameManager.LocalServerId);
                                if (null == dbFields || dbFields.Length < 1)
                                {
                                }
                                else
                                {
                                    int totalOnlineNum = Global.SafeConvertToInt32(dbFields[0]);
                                    KTPlayerManager.ShowNotification(player, "CCU ONLINE 2: " + totalOnlineNum);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }

                            break;
                        }

                    #endregion Kiểm tra thông tin hệ thống

                    #region Đoán Hoa Đăng

                    case "ResetKnowledgeChallengeQuestions":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.ResetKnowledgeChallengeQuestions(player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.ResetKnowledgeChallengeQuestions(targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetKnowledgeChallengeQuestions' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Đoán Hoa Đăng

                    #region Bí cảnh

                    case "ResetMiJing":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.ResetCopySceneEnterTimes(player, DailyRecord.MiJing);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.ResetCopySceneEnterTimes(targetID, DailyRecord.MiJing);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetMiJing' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Bí cảnh

                    #region Du Long Các

                    case "ResetYouLong":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.ResetCopySceneEnterTimes(player, DailyRecord.YouLong);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.ResetCopySceneEnterTimes(targetID, DailyRecord.YouLong);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetYouLong' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Du Long Các

                    #region Võ lâm liên đấu

                    case "StartTeamBattle":
                        {
                            if (paramsCount == 0)
                            {
                                TeamBattle_ActivityScript.BeginBattle();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'StartTeamBattle' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "ArrangeTeamBattleFinalRound":
                        {
                            if (paramsCount == 0)
                            {
                                TeamBattle_ActivityScript.ArrangeAndIncreaseStageToTopTeamToTheFinalRound();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ArrangeTeamBattleFinalRound' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "ArrangeTeamBattlePlayersRank":
                        {
                            if (paramsCount == 0)
                            {
                                TeamBattle_ActivityScript.ArrangePlayersRank();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ArrangeTeamBattlePlayersRank' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "ArrangeTeamBattlePlayersRankAndUpdateAwardsStateToAllTeams":
                        {
                            if (paramsCount == 1)
                            {
                                int state = int.Parse(para[1]);
                                TeamBattle_ActivityScript.ArrangePlayersRankAndUpdateAllTeamsAwardState(state == 1);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ArrangeTeamBattlePlayersRankAndUpdateAwardsStateToAllTeams' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "UpdateTeamBattleAwardState":
                        {
                            if (paramsCount == 1)
                            {
                                int state = int.Parse(para[1]);
                                KTGMCommandManager.SetTeamBattleAwardState(player, state);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int state = int.Parse(para[2]);
                                KTGMCommandManager.SetTeamBattleAwardState(targetID, state);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'UpdateTeamBattleAwardState' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "ResetTeamBattleTeamData":
                        {
                            if (paramsCount == 0)
                            {
                                TeamBattle_ActivityScript.ClearTeamsData();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetTeamBattleTeamData' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "ReloadTeamBattle":
                        {
                            if (paramsCount == 0)
                            {
                                TeamBattle.Init();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ReloadTeamBattle' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Võ lâm liên đấu

                    #region Uy danh

                    case "AddPrestige":
                        {
                            if (paramsCount == 1)
                            {
                                int value = int.Parse(para[1]);
                                KTGMCommandManager.AddPrestige(player, value);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int value = int.Parse(para[2]);
                                KTGMCommandManager.AddPrestige(targetID, value);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPrestige' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Uy danh

                    #region Danh vọng

                    case "AddRepute":
                        {
                            if (paramsCount == 2)
                            {
                                int reputeID = int.Parse(para[1]);
                                int value = int.Parse(para[2]);
                                KTGMCommandManager.AddRepute(player, reputeID, value);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int reputeID = int.Parse(para[2]);
                                int value = int.Parse(para[3]);
                                KTGMCommandManager.AddRepute(targetID, reputeID, value);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddRepute' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "SetRetupe":
                        {
                            if (paramsCount == 4)
                            {
                                int targetID = int.Parse(para[1]);
                                int reputeID = int.Parse(para[2]);
                                int value = int.Parse(para[3]);
                                int exp = int.Parse(para[4]);

                                KPlayer target = KTGMCommandManager.FindPlayer(targetID);

                                KTGlobal.SetRepute(target, reputeID, value, exp);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddRepute' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Danh vọng

                    #region Xóa túi đồ bản thân

                    case "ClearBag":
                        {
                            List<GoodsData> itemGDs = player.GoodsData.FindAll(x => x.Using < 0 && x.Site == 0);
                            KTPlayerManager.ResolveRemoveItems(player, itemGDs);
                            break;
                        }

                    case "ClearPortableBag":
                        {
                            List<GoodsData> itemGDs = player.GoodsData.FindAll(x => x.Using < 0 && x.Site == 1);
                            KTPlayerManager.ResolveRemovePortableBagItems(player, itemGDs);
                            break;
                        }

                    #endregion Xóa túi đồ bản thân

                    #region Thay đổi danh hiệu cá nhân

                    case "ModRoleTitle":
                        {
                            if (paramsCount == 2)
                            {
                                int method = int.Parse(para[1]);
                                int titleID = int.Parse(para[2]);
                                KTGMCommandManager.ModRoleTitle(player, method, titleID);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int method = int.Parse(para[2]);
                                int titleID = int.Parse(para[3]);
                                KTGMCommandManager.ModRoleTitle(targetID, method, titleID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ModRoleTitle' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "ModSpecialTitle":
                        {
                            if (paramsCount == 2)
                            {
                                int method = int.Parse(para[1]);
                                int titleID = int.Parse(para[2]);
                                KTGMCommandManager.ModRoleSpecialTitle(player, method, titleID);
                            }
                            else if (paramsCount == 3)
                            {
                                int targetID = int.Parse(para[1]);
                                int method = int.Parse(para[2]);
                                int titleID = int.Parse(para[3]);
                                KTGMCommandManager.ModRoleSpecialTitle(targetID, method, titleID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ModSpecialTitle' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Thay đổi danh hiệu cá nhân

                    #region Tu Luyện Châu

                    case "SetXiuLianZhu_TimeLeft":
                        {
                            if (paramsCount == 1)
                            {
                                int hour10 = int.Parse(para[1]);
                                KTGMCommandManager.SetXiuLianZhu_TimeLeft(player, hour10);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int hour10 = int.Parse(para[2]);
                                KTGMCommandManager.SetXiuLianZhu_TimeLeft(targetID, hour10);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetXiuLianZhu_TimeLeft' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "AddXiuLianZhu_Exp":
                        {
                            if (paramsCount == 1)
                            {
                                int exp = int.Parse(para[1]);
                                KTGMCommandManager.AddXiuLianZhu_Exp(player, exp);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int exp = int.Parse(para[2]);
                                KTGMCommandManager.AddXiuLianZhu_Exp(targetID, exp);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddXiuLianZhu_Exp' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "SetXiuLianZhu_Exp":
                        {
                            if (paramsCount == 1)
                            {
                                int exp = int.Parse(para[1]);
                                KTGMCommandManager.SetXiuLianZhu_Exp(player, exp);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int exp = int.Parse(para[2]);
                                KTGMCommandManager.SetXiuLianZhu_Exp(targetID, exp);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetXiuLianZhu_Exp' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Tu Luyện Châu

                    #region Chúc phúc

                    case "ResetPray":
                        {
                            player.LastPrayResult.Clear();
                            player.LastPrayResult = null;
                            player.SavePrayDataToDB();
                            break;
                        }
                    case "SetPrayTimes":
                        {
                            if (paramsCount == 1)
                            {
                                int value = int.Parse(para[1]);
                                KTGMCommandManager.SetPrayTimes(player, value);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int value = int.Parse(para[2]);
                                KTGMCommandManager.SetPrayTimes(targetID, value);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetPrayTimes' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Chúc phúc

                    #region Sự kiện đặc biệt

                    case "ReloadSpecialEvent":
                        {
                            if (paramsCount == 0)
                            {
                                SpecialEvent.Init();
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ReloadSpecialEvent' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Sự kiện đặc biệt

                    #region Set lại chức vụ

                    case "SetRankBangHoi":
                        {
                            int targetID = int.Parse(para[1]);
                            int Rank = int.Parse(para[2]);
                            KPlayer client = KTPlayerManager.Find(targetID);

                            if (client != null)
                            {
                                string CMDBUILD = client.RoleID + ":" + client.GuildID + ":" + Rank;

                                byte[] ByteSendToDB = Encoding.ASCII.GetBytes(CMDBUILD);

                                string[] result = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_GUILD_CHANGERANK, CMDBUILD, client.ServerId);

                                int Status = Int32.Parse(result[0]);

                                if (Status > 0)
                                {
                                    client.GuildRank = Rank;

                                    /// Thông báo danh hiệu thay đổi
                                    KT_TCPHandler.NotifyOthersMyTitleChanged(client);

                                    /// Thông báo cập nhật thông tin gia tộc và bang hội
                                    KT_TCPHandler.NotifyOtherMyGuildRankChanged(client);

                                    string responseData = string.Format("{0}", client.GuildRank);
                                    client.SendPacket((int)TCPGameServerCmds.CMD_KT_GUILD_CHANGERANK, responseData);
                                }
                            }
                            break;
                        }

                    #endregion Set lại chức vụ

                    #region Tạo Pet

                    case "CreatePet":
                        {
                            if (paramsCount == 1)
                            {
                                int petID = int.Parse(para[1]);
                                KTGMCommandManager.CreatePet(player, petID);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int petID = int.Parse(para[2]);
                                KTGMCommandManager.CreatePet(targetID, petID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreatePet' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Tạo Pet

                    #region Thêm điểm lĩnh ngộ Pet

                    case "AddPetEnlightenment":
                        {
                            if (paramsCount == 1)
                            {
                                int point = int.Parse(para[1]);
                                KTGMCommandManager.AddPetEnlightenment(player, point);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int point = int.Parse(para[2]);
                                KTGMCommandManager.AddPetEnlightenment(targetID, point);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetEnlightenment' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Thêm điểm lĩnh ngộ Pet

                    #region Thêm kinh nghiệm Pet

                    case "AddPetExp":
                        {
                            if (paramsCount == 1)
                            {
                                int exp = int.Parse(para[1]);
                                KTGMCommandManager.AddPetExp(player, exp);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int exp = int.Parse(para[2]);
                                KTGMCommandManager.AddPetExp(targetID, exp);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetExp' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Thêm kinh nghiệm Pet

                    #region Thêm điểm vui vẻ Pet

                    case "AddPetJoyful":
                        {
                            if (paramsCount == 1)
                            {
                                int value = int.Parse(para[1]);
                                KTGMCommandManager.AddPetJoyful(player, value);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int value = int.Parse(para[2]);
                                KTGMCommandManager.AddPetJoyful(targetID, value);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetJoyful' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Thêm điểm vui vẻ Pet

                    #region Thêm tuổi thọ Pet

                    case "AddPetLife":
                        {
                            if (paramsCount == 1)
                            {
                                int value = int.Parse(para[1]);
                                KTGMCommandManager.AddPetLife(player, value);
                            }
                            else if (paramsCount == 2)
                            {
                                int targetID = int.Parse(para[1]);
                                int value = int.Parse(para[2]);
                                KTGMCommandManager.AddPetLife(targetID, value);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetLife' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    #endregion Thêm tuổi thọ Pet

                    #region Mật khẩu cấp 2

                    case "ClearSecondPassword":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.ClearSecondPassword(player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.ClearSecondPassword(targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ClearSecondPassword' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }
                    case "ShowSecondPassword":
                        {
                            if (paramsCount == 0)
                            {
                                KTGMCommandManager.ShowSecondPassword(player);
                            }
                            else if (paramsCount == 1)
                            {
                                int targetID = int.Parse(para[1]);
                                KTGMCommandManager.ShowSecondPassword(targetID);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ShowSecondPassword' Faild...\n" + "Invalid param.");
                            }
                            break;
                        }

                    case "SetTotalRechage":
                        {
                            int RoleID = int.Parse(para[1]);
                            int TOTALMONEY = int.Parse(para[2]);
                            KPlayer target = KTGMCommandManager.FindPlayer(RoleID);
                            /// Kiểm tra đối tượng
                            if (target == null)
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ClearSecondPassword' Error...\n" + "Target not found.");
                                return;
                            }

                            string[] dbFields = Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_KT_G2C_RECHAGE, target.GetRoleData().userMiniData.UserId + ":" + TOTALMONEY + ":" + target.RoleID, GameManager.LocalServerId);
                            if (null == dbFields || dbFields.Length != 1 || dbFields[0] != "0")
                            {
                                LogManager.WriteLog(LogTypes.Rechage, "[COSTUMEXCHANGE][" + RoleID + "] Ghi lại tích lũy bị lỗi:" + TOTALMONEY);
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.Rechage, "[COSTUMEXCHANGE][" + target.RoleID + "] Ghi lại tích thành công:" + TOTALMONEY);
                            }
                            break;
                        }

                    case "SetTotalCosume":
                        {
                            int RoleID = int.Parse(para[1]);
                            int TOTALMONEY = int.Parse(para[2]);
                            KPlayer target = KTGMCommandManager.FindPlayer(RoleID);
                            /// Kiểm tra đối tượng
                            if (target == null)
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ClearSecondPassword' Error...\n" + "Target not found.");
                                return;
                            }
                            Global.SaveConsumeLog(target, TOTALMONEY, 1);
                            break;
                        }

                    #endregion Mật khẩu cấp 2

                    #region Thêm nhiệm vụ vận tiêu

                    case "GiveCarriageTask":
                        {
                            int type = int.Parse(para[1]);
                            string result = CargoCarriage_ActivityScript.GiveTask(player, type);
                            KTPlayerManager.ShowMessageBox(player, "Vận tiêu", result);
                            break;
                        }

                    #endregion Thêm nhiệm vụ vận tiêu

                    #region ForceUpdateTOPRanking

                    case "UpdateRankReadTime":
                        {
                            TopRankingManager.GmRankingReadTimeUpdate();

                            break;
                        }

                    case "UpdateRankingToDb":
                        {
                            TopRankingManager.UpdateRankingToDb();

                            break;
                        }

                    #endregion ForceUpdateTOPRanking

                    #region Dã tẩu

                    case "SetTaskCount":
                        {
                            int Number = int.Parse(para[1]);
                            TaskDailyArmyManager.getInstance().SetTaskDailyArmyTotalCount(player, Number);

                            break;
                        }

                    #endregion Dã tẩu

                    #region TestModel

                    case "SetTestMode":
                        {
                            int Number = int.Parse(para[1]);

                            if (Number == 0)
                            {
                                KTGlobal.IsTestModel = false;
                            }
                            else
                            {
                                KTGlobal.IsTestModel = true;
                            }

                            break;
                        }

                    #endregion TestModel

                    #region Bang Hội

                    case "SetGuildExp":
                        {
                            int ExpNum = int.Parse(para[1]);

                            var FindGuild = GuildManager.getInstance()._GetInfoGuildByGuildID(player.GuildID);
                            if (FindGuild != null)
                            {
                                FindGuild.GuildExp += ExpNum;

                                GuildManager.UpdateGuildResource(player.GuildID, GUILD_RESOURCE.EXP, FindGuild.GuildExp + "");
                            }
                            else
                            {
                                KTPlayerManager.ShowNotification(player, "Bạn chưa có bang hội!");
                            }

                            break;
                        }

                    case "ResetFubenGuild":
                        {
                            var FindGuild = GuildManager.getInstance()._GetInfoGuildByGuildName(para[1]);
                            if (FindGuild != null)
                            {
                                FindGuild.Total_Copy_Scenes_This_Week = 0;
                            }
                            else
                            {
                                KTPlayerManager.ShowNotification(player, "Không tìm thấy bang hội!");
                            }

                            break;
                        }

                    #endregion Bang Hội

                    #region StallBot

                    case "CreateStallBot":
                        {
                            KTStallBotManager.Create(player.CurrentMapCode, player.PosX, player.PosY, player, "Sạp hàng Test", 5000);

                            break;
                        }

                    #endregion StallBot

                    #region Reload XML GS

                    case "ReloadWefare":
                        {
                            CardMonthManager.Setup();
                            CheckPointManager.Setup();
                            DownloadBounsManager.Setup();
                            EveryDayOnlineManager.Setup();
                            LevelUpEventManager.Setup();
                            RechageManager.Setup();

                            break;
                        }

                    case "ReloadTopServerEvent":
                        {
                            TopRankingManager.Setup();

                            break;
                        }

                    case "ReloadX2Event":
                        {
                            ExpMutipleEvent.Setup();
                            break;
                        }

                    case "ReloadDropProfiles":
                        {
                            KTMonsterManager.MonsterDropManager.Init();
                            break;
                        }

                    case "ReloadShop":
                        {
                            ShopManager.Setup();
                            break;
                        }

                    case "ReloadRandomBox":
                        {
                            ItemRandomBox.Setup();
                            break;
                        }

                    case "ReloadSystemTask":
                        {
                            TaskManager.getInstance().Setup();
                            break;
                        }

                    case "ReloadSkillData":
                        {
                            KSkill.LoadSkillData();
                            break;
                        }

                    case "ReloadPKConfig":
                        {
                            KTGlobal.LoadPKPunish();
                            break;
                        }

                    #endregion Reload XML GS

                    #region TestWeekRecore

                    case "SetTurnPlateTime":
                        {
                            int RoleID = int.Parse(para[1]);
                            int Time = int.Parse(para[2]);

                            KPlayer target = KTGMCommandManager.FindPlayer(RoleID);
                            /// Kiểm tra đối tượng
                            if (target == null)
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ClearSecondPassword' Error...\n" + "Target not found.");
                                return;
                            }

                            target.TurnPlate_TotalTurn = Time;
                            break;
                        }

                    case "ReadWeekRecore":
                        {
                            int RecoreID = int.Parse(para[1]);

                            int Value = player.GetValueOfWeekRecore(RecoreID);

                            KTPlayerManager.ShowMessageBox(player, "NOTIFY", "READ VALUE :" + Value);

                            break;
                        }

                    case "SetCarriageTime":
                        {
                            int RoleID = int.Parse(para[1]);
                            int Time = int.Parse(para[2]);
                            KPlayer target = KTGMCommandManager.FindPlayer(RoleID);
                            /// Kiểm tra đối tượng
                            if (target == null)
                            {
                                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ClearSecondPassword' Error...\n" + "Target not found.");
                                return;
                            }
                            CargoCarriage_ActivityScript.SetTotalRoundsToday(target, Time);
                            break;
                        }

                    #endregion TestWeekRecore

                    #region GUILDWARTESTCOMMAND

                    case "StartGuildTeamFight":
                        {
                            GuildWarCity.getInstance().ForceStartTeamFight();

                            break;
                        }

                    case "RegisterTeamFight":
                        {
                            GuildWarCity.getInstance().RegisterTeamFightByGmCommand(player);

                            break;
                        }

                    case "StartGuildCityFight":
                        {
                            GuildWarCity.getInstance().ForceStartCityFight();

                            break;
                        }

                        //ForceStartCityFight

                        #endregion GUILDWARTESTCOMMAND
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "Process GM Command Error...\n" + ex.ToString(), false);
            }
        }

        #region Helper

        /// <summary>
        /// Tìm người chơi có tên tương ứng
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        private static KPlayer FindPlayer(string playerName)
        {
            KPlayer player = KTPlayerManager.FindAll(x => x.RoleName == playerName).FirstOrDefault();
            return player;
        }

        /// <summary>
        /// Tìm người chơi có ID tương ứng
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        private static KPlayer FindPlayer(int playerID)
        {
            return KTPlayerManager.Find(playerID);
        }

        #endregion Helper

        #region Script-Lua

        /// <summary>
        /// Tải lại Script Lua mới nhất
        /// </summary>
        /// <param name="scriptID">ID Script Lua</param>
        private static void ReloadScriptLua(int scriptID)
        {
            KTLuaScript.Instance.ReloadScript(KTLuaEnvironment.LuaEnv, scriptID);
        }

        #endregion Script-Lua

        #region Trạng thái ngũ hành

        /// <summary>
        /// Thêm trạng thái ngũ hành cho đối tượng có ID tương ứng
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="stateID"></param>
        /// <param name="time"></param>
        private static void AddSeriesState(int targetID, string stateID, float time)
        {
            GameObject target = KTGMCommandManager.FindPlayer(targetID);
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSeriesState' Error...\n" + "Target ID '" + targetID + "' not found.");
                return;
            }

            KTGMCommandManager.AddSeriesState(target, stateID, time);
        }

        /// <summary>
        /// Thêm trạng thái ngũ hành
        /// </summary>
        /// <param name="target">Đối tượng</param>
        /// <param name="stateID">Trạng thái</param>
        /// <param name="time">Thời gian (giây)</param>
        private static void AddSeriesState(GameObject target, string stateID, float time)
        {
            /// Kiểm tra trạng thái
            if (!Utils.TryParseEnum<KE_STATE>(stateID, out KE_STATE state))
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSeriesState' Error...\n" + "State ID '" + stateID + "' not found.");
                return;
            }

            /// Kiểm tra thời gian
            if (time < 0 || time > 60)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSeriesState' Error...\n" + "Duration is INVALID, must be between 0 and 60s.");
                return;
            }

            /// Thêm trạng thái vào đối tượng
            target.AddSpecialState(target, new UnityEngine.Vector2((int)target.CurrentPos.X, (int)target.CurrentPos.Y), state, (int)(time * 18), (int)(time * 18), true);
        }

        /// <summary>
        /// Xóa trạng thái ngũ hành khỏi đối tượng có ID tương ứng
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="stateID"></param>
        private static void RemoveSeriesState(int targetID, string stateID)
        {
            GameObject target = KTGMCommandManager.FindPlayer(targetID);
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'RemoveSeriesState' Error...\n" + "Target ID '" + targetID + "' not found.");
                return;
            }

            KTGMCommandManager.RemoveSeriesState(target, stateID);
        }

        /// <summary>
        /// Xóa trạng thái ngũ hành
        /// </summary>
        /// <param name="target">Đối tượng</param>
        /// <param name="stateID">ID trạng thái</param>
        private static void RemoveSeriesState(GameObject target, string stateID)
        {
            /// Kiểm tra trạng thái
            if (!Utils.TryParseEnum<KE_STATE>(stateID, out KE_STATE state))
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'RemoveSeriesState' Error...\n" + "State ID '" + stateID + "' not found.");
                return;
            }

            /// Xóa trạng thái khỏi đối tượng
            target.RemoveSpecialState(state, true);
        }

        #endregion Trạng thái ngũ hành

        #region Gia nhập phái

        /// <summary>
        /// Thiết lập môn phái cho đối tượng tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="factionID">ID phái</param>
        private static void JoinFaction(int targetID, int factionID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'Heal' Error...\n" + "Target not found.");
                return;
            }

            /// Thực hiện phục hồi cho mục tiêu
            KTGMCommandManager.JoinFaction(target, factionID);
        }

        /// <summary>
        /// Thiết lập môn phái cho đối tượng tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="factionID">ID phái</param>
        private static void JoinFaction(KPlayer player, int factionID)
        {
            /// Gia nhập môn phái
            KTPlayerManager.JoinFaction(player, factionID);
        }

        #endregion Gia nhập phái

        #region Trị liệu

        /// <summary>
        /// Trị liệu phục hồi sinh lực, nội lực, thể lực cho đối tượng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        private static void Heal(int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'Heal' Error...\n" + "Target not found.");
                return;
            }

            /// Thực hiện phục hồi cho mục tiêu
            KTGMCommandManager.Heal(target);
        }

        /// <summary>
        /// Trị liệu phục hồi sinh lực, nội lực, thể lực
        /// </summary>
        /// <param name="player">Người chơi</param>
        private static void Heal(KPlayer player)
        {
            /// Phục hồi sinh lực
            player.m_CurrentLife = player.m_CurrentLifeMax;
            /// Phục hồi nội lực
            player.m_CurrentMana = player.m_CurrentManaMax;
            /// Phục hồi thể lực
            player.m_CurrentStamina = player.m_CurrentStaminaMax;
        }

        #endregion Trị liệu

        #region Thông tin người chơi

        /// <summary>
        /// Trả ra thông tin người chơi tương ứng
        /// </summary>
        /// <param name="player">Thông tin hiển thị cho người chơi</param>
        /// <param name="playerName">Tên người chơi cần tìm</param>
        private static void GetPlayerInfo(KPlayer player, string playerName)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(playerName);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Không tìm thấy thông tin người chơi {0}!", playerName));
                return;
            }

            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(target.CurrentMapCode);

            string msg = string.Format("Thông tin người chơi: {0}({1}), vị trí: {4} ({2}, {3})", target.RoleName, target.RoleID, (int)target.CurrentPos.X, (int)target.CurrentPos.Y, gameMap == null ? "Chưa rõ" : gameMap.MapName);
            KTPlayerManager.ShowNotification(player, msg);
        }

        #endregion Thông tin người chơi

        #region Dịch chuyển

        /// <summary>
        /// Dịch chuyển đối tượng có ID tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="posX">Vị trí X</param>
        /// <param name="posY">Vị trí Y</param>
        private static void GoTo(int targetID, int mapCode, int posX, int posY)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GoTo' Error...\n" + "Target not found.");
                return;
            }

            /// Thực hiện dịch chuyển đối tượng tương ứng
            KTGMCommandManager.GoTo(target, mapCode, posX, posY);
        }

        /// <summary>
        /// Dịch chuyển người chơi
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="posX">Vị trí X</param>
        /// <param name="posY">Vị trí Y</param>
        private static void GoTo(KPlayer player, int mapCode, int posX, int posY)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            /// Lấy dữ liệu bản đồ đích đến
            if (gameMap != null)
            {
                /// Nếu bản đồ đích khác bản đồ hiện tại
                if (player.CurrentMapCode != mapCode)
                {
                    KTPlayerManager.ChangeMap(player, mapCode, posX, posY);
                }
                else
                {
                    KTPlayerManager.ChangePos(player, posX, posY);
                }
            }
            else
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GoTo' Error...\n" + "Map not exist.");
            }
        }

        #endregion Dịch chuyển

        #region Kỹ năng và Buff

        /// <summary>
        /// Thêm kỹ năng cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        /// <param name="level">Cấp độ</param>
        private static void AddSkill(int targetID, int skillID, int level)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSkill' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddSkill(target, skillID, level);
        }

        /// <summary>
        /// Thêm kỹ năng cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="skillID">ID kỹ năng</param>
        /// <param name="level">Cấp độ</param>
        private static void AddSkill(KPlayer player, int skillID, int level)
        {
            player.Skills.AddSkill(skillID);
            player.Skills.AddSkillLevel(skillID, level);
        }

        /// <summary>
        /// Xóa kỹ năng khỏi đối tượng có ID tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        private static void RemoveSkill(int targetID, int skillID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'RemoveSkill' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.RemoveSkill(target, skillID);
        }

        /// <summary>
        /// Xóa kỹ năng khỏi người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="skillID">ID kỹ năng</param>
        private static void RemoveSkill(KPlayer player, int skillID)
        {
            player.Skills.RemoveSkill(skillID);
        }

        /// <summary>
        /// Thêm Buff cho đối tượng có ID tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        /// <param name="level">Cấp độ</param>
        private static void AddBuff(int targetID, int skillID, int level)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSkill' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddBuff(target, skillID, level);
        }

        /// <summary>
        /// Thêm Buff tương ứng cho đối tượng
        /// </summary>
        /// <param name="target">Đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        /// <param name="level">Cấp độ</param>
        private static void AddBuff(GameObject target, int skillID, int level)
        {
            SkillDataEx skillData = KSkill.GetSkillData(skillID);
            if (skillData == null)
            {
                return;
            }
            SkillLevelRef skill = new SkillLevelRef()
            {
                Data = skillData,
                AddedLevel = level,
                BonusLevel = 0,
                CanStudy = false,
            };

            PropertyDictionary skillPd = skill.Properties;
            int duration = -1;
            if (skillPd.ContainsKey((int)MAGIC_ATTRIB.magic_skill_statetime))
            {
                if (skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_statetime).nValue[0] == -1)
                {
                    duration = -1;
                }
                else
                {
                    duration = skillPd.Get<KMagicAttrib>((int)MAGIC_ATTRIB.magic_skill_statetime).nValue[0] * 1000 / 18;
                }
            }

            target.Buffs.AddBuff(new BuffDataEx()
            {
                Skill = skill,
                Duration = duration,
                LoseWhenUsingSkill = false,
                SaveToDB = false,
                StartTick = KTGlobal.GetCurrentTimeMilis(),
            });
        }

        /// <summary>
        /// Xóa Buff khỏi đối tượng có ID tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        private static void RemoveBuff(int targetID, int skillID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'RemoveBuff' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.RemoveBuff(target, skillID);
        }

        /// <summary>
        /// Xóa Buff khỏi đối tượng tương ứng
        /// </summary>
        /// <param name="target">Đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        private static void RemoveBuff(GameObject target, int skillID)
        {
            target.Buffs.RemoveBuff(skillID);
        }

        /// <summary>
        /// Làm mới toàn bộ dữ liệu phục hồi kỹ năng của đối tượng có ID tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        private static void ResetAllSkillCooldown(int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetAllSkillCooldown' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.ResetAllSkillCooldown(target);
        }

        /// <summary>
        /// Làm mới toàn bộ dữ liệu phục hồi kỹ năng của đối tượng tương ứng
        /// </summary>
        /// <param name="target">Đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        private static void ResetAllSkillCooldown(KPlayer target)
        {
            target.Skills.ClearSkillCooldownList();
        }

        /// <summary>
        /// Thêm kinh nghiệm kỹ năng cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="skillID">ID kỹ năng</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void AddSkillExp(int targetID, int skillID, int exp)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddSkillExp' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddSkillExp(target, skillID, exp);
        }

        /// <summary>
        /// Thêm kinh nghiệm kỹ năng cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="skillID">ID kỹ năng</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void AddSkillExp(KPlayer player, int skillID, int exp)
        {
            /// Kỹ năng tương ứng
            SkillLevelRef skill = player.Skills.GetSkillLevelRef(skillID);
            /// Thực hiện thêm kinh nghiệm cho kỹ năng tương ứng
            KSkillExp.AddSkillExp(player, skill, exp);
        }

        #endregion Kỹ năng và Buff

        #region System Chat

        /// <summary>
        /// Gửi tin nhắn dưới kênh hệ thống
        /// </summary>
        /// <param name="message">Tin nhắn</param>
        private static void SendSystemChat(string message)
        {
            KTGlobal.SendSystemChat(message);
        }

        /// <summary>
        /// Gửi tin nhắn dưới kênh hệ thống đồng thời hiển thị ở dòng chữ chạy ngang trêu đầu
        /// </summary>
        /// <param name="message"></param>
        public static void SendSystemEventNotification(string message)
        {
            KTGlobal.SendSystemEventNotification(message);
        }

        #endregion System Chat

        #region Tạo vật phẩm

        /// <summary>
        /// Tạo vật phẩm số lượng tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="itemID">ID vật phẩm</param>
        /// <param name="quantity">Số lượng</param>
        private static void CreateItem(int targetID, int itemID, int quantity)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreateItem' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.CreateItem(target, itemID, quantity, -1);
        }

        public static void CreateItemEx(KPlayer client, int ItemID, int Quanyti, int Linescount, int Series)
        {
            for (int i = 0; i < Quanyti; i++)
            {
                ItemManager.CreateItemRandomLine(client, ItemID, Linescount, Series);
            }
        }

        /// <summary>
        /// Tạo vật phẩm số lượng tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="itemID">ID vật phẩm</param>
        /// <param name="quantity">Số lượng</param>
        private static void CreateItem(KPlayer player, int itemID, int quantity, int EXP)
        {
            /// Nếu vật phẩm tồn tại
            if (ItemManager._TotalGameItem.ContainsKey(itemID))
            {
                string TimeUsing = ItemManager.ConstGoodsEndTime;

                if (EXP > 0)
                {
                    DateTime dt = DateTime.Now.AddMinutes(EXP);

                    // "1900-01-01 12:00:00";
                    TimeUsing = dt.ToString("yyyy-MM-dd HH:mm:ss");
                }
                ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, itemID, quantity, 0, "GMCOMMAND", true, 0, false, TimeUsing);
            }
            else
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm không tồn tại!");
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreateItem' Error...\n" + "Item ID = " + itemID + " not found.");
            }
        }

        #endregion Tạo vật phẩm

        #region Thiết lập cấp cường hóa cho trang bị

        /// <summary>
        /// Tạo vật phẩm số lượng tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="slot">Vị trí trang bị trong túi</param>
        /// <param name="level">Cấp cường hóa</param>
        private static void EquipEnhance(int targetID, int slot, int level)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreateItem' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.EquipEnhance(target, slot, level);
        }

        /// <summary>
        /// Tạo vật phẩm số lượng tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="slot">Vị trí trang bị trong túi</param>
        /// <param name="level">Cấp cường hóa</param>
        private static void EquipEnhance(KPlayer player, int slot, int level)
        {
            /// Vật phẩm tại vị trí tương ứng trong túi
            GoodsData itemGD = player.GoodsData.Find(x => x.Site == 0 && x.Using == -1 && x.BagIndex == slot);
            /// Nếu vật phẩm tại vị trí tương ứng không tồn tại
            if (itemGD == null)
            {
                KTPlayerManager.ShowNotification(player, "Không tồn tại vật phẩm tại vị trí " + slot + " trong túi đồ!");
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'EquipEnhance' Error...\n" + "BagPos = " + slot + " has no item.");
                return;
            }
            /// Nếu vật phẩm không tồn tại trong hệ thống
            if (!ItemManager._TotalGameItem.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                KTPlayerManager.ShowNotification(player, "Không tồn tại vật phẩm tại vị trí " + slot + " trong túi đồ!");
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'EquipEnhance' Error...\n" + "BagPos = " + slot + ", Item ID = " + itemGD.GoodsID + " not found.");
                return;
            }

            /// Nếu không phải trang bị
            if (!ItemManager.KD_ISEQUIP(itemData.Genre) && !ItemManager.KD_ISPETEQUIP(itemData.Genre))
            {
                KTPlayerManager.ShowNotification(player, "Vật phẩm tại vị trí " + slot + " trong túi đồ không phải trang bị, không thể cường hóa được!");
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'EquipEnhance' Error...\n" + "BagPos = " + slot + ", Item ID = " + itemGD.GoodsID + " is not equip.");
                return;
            }

            ItemManager.SetEquipForgeLevel(itemGD, player, level);
        }

        #endregion Thiết lập cấp cường hóa cho trang bị

        #region Thêm tiền

        /// <summary>
        /// Thêm tiền số lượng tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="type">Loại tiền (1: Bạc thường, 2: Bạc khóa, 3: Đồng thường, 4: Đồng khóa)</param>
        /// <param name="amount">Số lượng</param>
        private static void AddMoney(int targetID, int type, int amount)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddMoney' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddMoney(target, type, amount);
        }

        /// <summary>
        /// Thêm tiền số lượng tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="type">Loại tiền (1: Bạc thường, 2: Bạc khóa, 3: Đồng thường, 4: Đồng khóa)</param>
        /// <param name="amount">Số lượng</param>
        private static void AddMoney(KPlayer player, int type, int amount)
        {
            switch (type)
            {
                case 1:
                    {
                        KTPlayerManager.AddMoney(player, amount, "GMCommand");
                        break;
                    }
                case 2:
                    {
                        KTPlayerManager.AddBoundMoney(player, amount, "GMCommand");
                        break;
                    }
                case 3:
                    {
                        KTPlayerManager.AddToken(player, amount, "GMCommand");
                        break;
                    }
                case 4:
                    {
                        KTPlayerManager.AddBoundToken(player, amount, "GMCommand");
                        break;
                    }
                case 5:
                    {
                        // Add Tích lũy cá nhân ở bang hội
                        KTPlayerManager.AddGuildMoney(player, amount, "GMCommand");
                        break;
                    }
                case 6:
                    {
                        // Add Tích lũy cá nhân ở bang hội
                        KTGlobal.UpdateGuildMoney(500, player.GuildID, player);
                        break;
                    }

                case 7:
                    {
                        // Add Tích lũy cá nhân ở bang hội
                        KTGlobal.UpdateGuildBoundMoney(amount, player.GuildID);
                        break;
                    }
            }
        }

        #endregion Thêm tiền

        #region Thêm vật phẩm rơi ở MAP

        /// <summary>
        /// Thêm vật phẩm rơi ở MAP theo ID quái tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="monsterID">ID cấu hình quái tương ứng bọc</param>
        private static void AddDropItem(int targetID, int monsterID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddDropItem' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddDropItem(target, monsterID);
        }

        /// <summary>
        /// Thêm vật phẩm rơi ở MAP theo ID quái tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="monsterID">ID cấu hình quái tương ứng bọc</param>
        private static void AddDropItem(KPlayer player, int monsterID)
        {
            // ItemDropManager.GetDropMonsterDie(monsterID, player);
        }

        #endregion Thêm vật phẩm rơi ở MAP

        #region Hoạt động

        /// <summary>
        /// Chủ động bắt đầu sự kiện có ID tương ứng
        /// </summary>
        /// <param name="activityID"></param>
        private static void StartActivity(int activityID)
        {
            KTActivityManager.StartActivity(activityID);
        }

        /// <summary>
        /// Chủ động kết thúc sự kiện có ID tương ứng
        /// </summary>
        /// <param name="activityID"></param>
        private static void StopActivity(int activityID)
        {
            KTActivityManager.StopActivity(activityID);
        }

        #endregion Hoạt động

        #region Thiết lập sát khí

        /// <summary>
        /// Thiết lập số điểm sát khí tương ứng cho đối tượng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="value">Giá trị</param>
        private static void SetPKValue(int targetID, int value)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetPKValue' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetPKValue(target, value);
        }

        /// <summary>
        /// Thiết lập số điểm sát khí tương ứng cho đối tượng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="value">Giá trị</param>
        private static void SetPKValue(KPlayer player, int value)
        {
            if (value < 0 || value > 10)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetPKValue' Error...\n" + "Value must be between [0-10].");
                return;
            }

            player.PKValue = value;
        }

        #endregion Thiết lập sát khí

        #region Thiết lập cấp độ và kinh nghiệm kỹ năng sống

        /// <summary>
        /// Thiết lập cấp độ và kinh nghiệm kỹ năng sống cho đối tượng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="lifeSkillID">ID kỹ năng sống</param>
        /// <param name="level">Cấp độ</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void SetLifeSkillLevelAndExp(int targetID, int lifeSkillID, int level, int exp)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLifeSkillLevelAndExp' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetLifeSkillLevelAndExp(target, lifeSkillID, level, exp);
        }

        /// <summary>
        /// Thiết lập cấp độ và kinh nghiệm kỹ năng sống cho đối tượng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="lifeSkillID">ID kỹ năng sống</param>
        /// <param name="level">Cấp độ</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void SetLifeSkillLevelAndExp(KPlayer player, int lifeSkillID, int level, int exp)
        {
            if (level < 0 || level > 120)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLifeSkillLevelAndExp' Error...\n" + "Value must be between [0-120].");
                return;
            }

            LifeSkillPram lifeSkillParam = player.GetLifeSkill(lifeSkillID);
            if (lifeSkillParam == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLifeSkillLevelAndExp' Error...\n" + "Can not find life skill ID = " + lifeSkillID + ".");
                return;
            }

            LifeSkillExp lifeSkillExp = ItemCraftingManager._LifeSkill.TotalExp.Where(x => x.Level == level).FirstOrDefault();
            if (lifeSkillExp == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLifeSkillLevelAndExp' Error...\n" + "Can not find max exp for life skill ID = " + lifeSkillID + ".");
                return;
            }

            if (exp < 0)
            {
                exp = 0;
            }
            if (exp > lifeSkillExp.Exp - 1)
            {
                exp = lifeSkillExp.Exp - 1;
            }

            player.SetLifeSkillParam(lifeSkillID, level, exp);
        }

        #endregion Thiết lập cấp độ và kinh nghiệm kỹ năng sống

        #region Reset cấp độ và kinh nghiệm kỹ năng sống

        /// <summary>
        /// Reset cấp độ và kinh nghiệm kỹ năng sống cho đối tượng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        private static void ResetLifeSkillLevelAndExp(int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetLifeSkillLevelAndExp' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.ResetLifeSkillLevelAndExp(target);
        }

        /// <summary>
        /// Reset cấp độ và kinh nghiệm kỹ năng sống cho đối tượng
        /// </summary>
        /// <param name="player">Người chơi</param>
        private static void ResetLifeSkillLevelAndExp(KPlayer player)
        {
            Dictionary<int, LifeSkillPram> lifeSkills = new Dictionary<int, LifeSkillPram>();
            for (int i = 1; i < 12; i++)
            {
                LifeSkillPram param = new LifeSkillPram();
                param.LifeSkillID = i;
                param.LifeSkillLevel = 1;
                param.LifeSkillExp = 0;
                lifeSkills[i] = param;
            }
            player.SetLifeSkills(lifeSkills);
        }

        #endregion Reset cấp độ và kinh nghiệm kỹ năng sống

        #region Thiết lập điểm tinh lực, hoạt lực

        /// <summary>
        /// Thiết lập cấp độ và kinh nghiệm kỹ năng sống cho đối tượng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="gatherPoint">Tinh lực</param>
        /// <param name="makePoint">Hoạt lực</param>
        private static void AddGatherMakePoint(int targetID, int gatherPoint, int makePoint)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddGatherMakePoint' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddGatherMakePoint(target, gatherPoint, makePoint);
        }

        /// <summary>
        /// Thiết lập cấp độ và kinh nghiệm kỹ năng sống cho đối tượng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="gatherPoint">Tinh lực</param>
        /// <param name="makePoint">Hoạt lực</param>
        private static void AddGatherMakePoint(KPlayer player, int gatherPoint, int makePoint)
        {
            player.ChangeCurMakePoint(makePoint);
            player.ChangeCurGatherPoint(gatherPoint);
        }

        #endregion Thiết lập điểm tinh lực, hoạt lực

        #region Thiết lập danh hiệu tạm thời

        /// <summary>
        /// Thiết lập danh hiệu tạm thời cho đối tượng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="title">Danh hiệu</param>
        private static void SetTempTitle(int targetID, string title)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetTempTitle' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetTempTitle(target, title);
        }

        /// <summary>
        /// Thiết lập cấp độ và kinh nghiệm kỹ năng sống cho đối tượng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="title">Danh hiệu</param>
        private static void SetTempTitle(KPlayer player, string title)
        {
            player.TempTitle = title;
        }

        #endregion Thiết lập danh hiệu tạm thời

        #region Gửi thư

        /// <summary>
        /// Gửi thư cho đối tượng tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="title">Tiêu đề</param>
        /// <param name="content">Nội dung</param>
        /// <param name="items">Danh sách vật phẩm đính kèm</param>
        /// <param name="boundMoney">Bạc khóa đính kèm</param>
        /// <param name="boundToken">Đồng khóa đính kèm</param>
        private static void SendSystemMail(int targetID, string title, string content, string items, int boundMoney, int boundToken)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SendMail' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SendSystemMail(target, title, content, items, boundMoney, boundToken);
        }

        /// <summary>
        /// Thiết lập cấp độ và kinh nghiệm kỹ năng sống cho đối tượng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="title">Tiêu đề</param>
        /// <param name="content">Nội dung</param>
        /// <param name="items">Danh sách vật phẩm đính kèm</param>
        /// <param name="boundMoney">Bạc khóa đính kèm</param>
        /// <param name="boundToken">Đồng khóa đính kèm</param>
        public static void SendSystemMail(KPlayer player, string title, string content, string items, int boundMoney, int boundToken)
        {
            /// Nếu có vật phẩm đính kèm
            if (!string.IsNullOrEmpty(items))
            {
                /// Danh sách vật phẩm đính kèm
                List<Tuple<int, int, int, int>> stickItems = new List<Tuple<int, int, int, int>>();

                foreach (string itemInfo in items.Split('|'))
                {
                    string[] para = itemInfo.Split('_');

                    int itemID, count, binding, enhanceLevel;
                    if (para.Length != 4)
                    {
                        LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SendMail' Error...\n" + "Incorrect item parameters.");
                        return;
                    }
                    else if (!int.TryParse(para[0], out itemID) || !int.TryParse(para[1], out count) || !int.TryParse(para[2], out binding) || !int.TryParse(para[3], out enhanceLevel))
                    {
                        LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SendMail' Error...\n" + "Incorrect item parameters.");
                        return;
                    }

                    stickItems.Add(new Tuple<int, int, int, int>(itemID, count, binding, enhanceLevel));
                }

                /// Thực hiện thêm vật phẩm
                KTMailManager.SendSystemMailToPlayerWithItemIDs(player, stickItems, title, content, boundMoney, boundToken);
            }
            /// Nếu không có vật phẩm đính kèm
            else
            {
                /// Thực hiện thêm vật phẩm
                KTMailManager.SendSystemMailToPlayer(player, title, content, boundMoney, boundToken);
            }
        }

        #endregion Gửi thư

        #region Thêm kinh nghiệm

        /// <summary>
        /// Thêm kinh nghiệm cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void AddExp(int targetID, int exp)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddExp' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddExp(target, exp);
        }

        /// <summary>
        /// Thêm kinh nghiệm cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void AddExp(KPlayer player, int exp)
        {
            KTPlayerManager.AddExp(player, exp);
        }

        #endregion Thêm kinh nghiệm

        #region Thiết lập cấp độ

        /// <summary>
        /// Thiết lập cấp độ cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="level">Cấp độ</param>
        private static void SetLevel(int targetID, int level)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLevel' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetLevel(target, level);
        }

        /// <summary>
        /// Thiết lập cấp độ cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="level">Cấp độ</param>
        private static void SetLevel(KPlayer player, int level)
        {
            KTPlayerManager.SetRoleLevel(player, level);
            Global.ForceCloseClient(player, "GMCommand");
        }

        #endregion Thiết lập cấp độ

        #region Reset số lượt Đoán hoa đăng

        /// <summary>
        /// Reset số lượt Đoán hoa đăng trong ngày
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="eventID">Loại phụ bản</param>
        private static void ResetKnowledgeChallengeQuestions(int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetKnowledgeChallengeQuestions' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.ResetKnowledgeChallengeQuestions(target);
        }

        /// <summary>
        /// Reset số lượt Đoán hoa đăng trong ngày của đối tượng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="eventID">Loại phụ bản</param>
        private static void ResetKnowledgeChallengeQuestions(KPlayer player)
        {
            player.SetValueOfDailyRecore((int)DailyRecord.KnowledgeChallenge_TotalQuestions, 0);
        }

        #endregion Reset số lượt Đoán hoa đăng

        #region Thiết lập lại số lượt phụ bản

        /// <summary>
        /// Reset giá trị số lượt tham gia phụ bản tương ứng trong ngày
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="eventID">Loại phụ bản</param>
        private static void ResetCopySceneEnterTimes(int targetID, DailyRecord eventID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ResetCopySceneEnterTimes' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.ResetCopySceneEnterTimes(target, eventID);
        }

        /// <summary>
        /// Reset giá trị số lượt tham gia phụ bản tương ứng trong ngày của đối tượng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="eventID">Loại phụ bản</param>
        private static void ResetCopySceneEnterTimes(KPlayer player, DailyRecord eventID)
        {
            CopySceneEventManager.SetCopySceneTotalEnterTimesToday(player, eventID, 0);
        }

        #endregion Thiết lập lại số lượt phụ bản

        #region Thêm uy danh

        /// <summary>
        /// Thêm uy danh cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="value">Số uy danh thêm vào</param>
        private static void AddPrestige(int targetID, int value)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPrestige' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddPrestige(target, value);
        }

        /// <summary>
        /// Thêm uy danh cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="value">Số uy danh thêm vào</param>
        private static void AddPrestige(KPlayer player, int value)
        {
            player.Prestige += value;
        }

        #endregion Thêm uy danh

        #region Thêm danh vọng

        /// <summary>
        /// Thêm danh vọng cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="reputeID">Danh vọng</param>
        /// <param name="value">Giá trị</param>
        private static void AddRepute(int targetID, int reputeID, int value)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddRepute' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddRepute(target, reputeID, value);
        }

        /// <summary>
        /// Thêm danh vọng cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="reputeID">Danh vọng</param>
        /// <param name="value">Số uy danh thêm vào</param>
        private static void AddRepute(KPlayer player, int reputeID, int value)
        {
            KTGlobal.AddRepute(player, reputeID, value);
        }

        #endregion Thêm danh vọng

        #region Thêm/xóa danh hiệu nhân vật

        /// <summary>
        /// Thêm/xóa danh hiệu nhân vật cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="method">Loại thao tác (-1: Xóa, 1: Thêm)</param>
        /// <param name="titleID">ID danh hiệu</param>
        private static void ModRoleTitle(int targetID, int method, int titleID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ModRoleTitle' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.ModRoleTitle(target, method, titleID);
        }

        /// <summary>
        /// Thêm/xóa danh hiệu nhân vật cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="method">Loại thao tác (-1: Xóa, 1: Thêm)</param>
        /// <param name="titleID">ID danh hiệu</param>
        private static void ModRoleTitle(KPlayer player, int method, int titleID)
        {
            /// Nếu là thao tác thêm
            if (method == 1)
            {
                player.AddRoleTitle(titleID);
            }
            /// Nếu là thao tác xóa
            else if (method == -1)
            {
                player.RemoveRoleTitle(titleID);
            }
        }

        /// <summary>
        /// Thêm/xóa danh hiệu nhân vật đặc biệt cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="method">Loại thao tác (-1: Xóa, 1: Thêm)</param>
        /// <param name="titleID">ID danh hiệu</param>
        private static void ModRoleSpecialTitle(int targetID, int method, int titleID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ModRoleSpecialTitle' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.ModRoleSpecialTitle(target, method, titleID);
        }

        /// <summary>
        /// Thêm/xóa danh hiệu nhân vật đặc biệt cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="method">Loại thao tác (-1: Xóa, 1: Thêm)</param>
        /// <param name="titleID">ID danh hiệu</param>
        private static void ModRoleSpecialTitle(KPlayer player, int method, int titleID)
        {
            /// Nếu là thao tác thêm
            if (method == 1)
            {
                player.SetSpecialTitle(titleID);
            }
            /// Nếu là thao tác xóa
            else if (method == -1)
            {
                player.RemoveSpecialTitle();
            }
        }

        #endregion Thêm/xóa danh hiệu nhân vật

        #region Thiết lập số lượt chúc phúc

        /// <summary>
        /// Thiết lập số lượt chúc phúc cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="value">Giá trị</param>
        private static void SetPrayTimes(int targetID, int value)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetPrayTimes' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetPrayTimes(target, value);
        }

        /// <summary>
        /// Thiết lập số lượt chúc phúc cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="value">Giá trị</param>
        private static void SetPrayTimes(KPlayer player, int value)
        {
            KTPlayerPrayManager.SetTotalTurnLeft(player, value);
        }

        #endregion Thiết lập số lượt chúc phúc

        #region Thiết lập sẽ quay vào rương xấu xí ở Bách Bảo Rương lượt tiếp theo

        /// <summary>
        /// Thiết lập sẽ quay vào rương xấu xí ở Bách Bảo Rương lượt tiếp theo cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="bet">Số sò cược tương ứng</param>
        private static void SetSeashellTreasureNextTurn(int targetID, int bet)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetSeashellTreasureNextTurn' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetSeashellTreasureNextTurn(target, bet);
        }

        /// <summary>
        /// Thiết lập sẽ quay vào rương xấu xí ở Bách Bảo Rương lượt tiếp theo cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="bet">Số sò cược tương ứng</param>
        private static void SetSeashellTreasureNextTurn(KPlayer player, int bet)
        {
            player.GM_SetWillGetTreasureNextTurn = true;
            player.GM_SetWillGetTreasureNextTurnWithBet = bet;
        }

        #endregion Thiết lập sẽ quay vào rương xấu xí ở Bách Bảo Rương lượt tiếp theo

        #region Thêm kinh nghiệm Tu Luyện Châu

        /// <summary>
        /// Thêm kinh nghiệm Tu Luyện Châu cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void AddXiuLianZhu_Exp(int targetID, int exp)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddXiuLianZhu_Exp' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.AddXiuLianZhu_Exp(target, exp);
        }

        /// <summary>
        /// Thêm kinh nghiệm Tu Luyện Châu cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void AddXiuLianZhu_Exp(KPlayer player, int exp)
        {
            player.XiuLianZhu_Exp += exp;
        }

        #endregion Thêm kinh nghiệm Tu Luyện Châu

        #region Thiết lập kinh nghiệm Tu Luyện Châu

        /// <summary>
        /// Thiết lập kinh nghiệm Tu Luyện Châu cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void SetXiuLianZhu_Exp(int targetID, int exp)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetXiuLianZhu_Exp' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetXiuLianZhu_Exp(target, exp);
        }

        /// <summary>
        /// Thiết lập kinh nghiệm Tu Luyện Châu cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="exp">Kinh nghiệm</param>
        private static void SetXiuLianZhu_Exp(KPlayer player, int exp)
        {
            player.XiuLianZhu_Exp = exp;
        }

        #endregion Thiết lập kinh nghiệm Tu Luyện Châu

        #region Thiết lập số giờ Tu Luyện Châu

        /// <summary>
        /// Thiết lập số giờ Tu Luyện Châu cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="exp">Số giờ (* 10)</param>
        private static void SetXiuLianZhu_TimeLeft(int targetID, int hour10)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddXiuLianZhu_Exp' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetXiuLianZhu_TimeLeft(target, hour10);
        }

        /// <summary>
        /// Thiết lập số giờ Tu Luyện Châu cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="exp">Số giờ (* 10)</param>
        private static void SetXiuLianZhu_TimeLeft(KPlayer player, int hour10)
        {
            player.XiuLianZhu_TotalTime = hour10;
        }

        #endregion Thiết lập số giờ Tu Luyện Châu

        #region Thiết lập trạng thái nhận thưởng liên đấu

        /// <summary>
        /// Thiết lập trạng thái nhận thưởng liên đấu cho chiến đội của người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="state">Trạng thái (0: chưa nhận, 1: đã nhận)</param>
        private static void SetTeamBattleAwardState(int targetID, int state)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'UpdateTeamBattleAwardState' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetTeamBattleAwardState(target, state);
        }

        /// <summary>
        /// Thiết lập trạng thái nhận thưởng liên đấu cho chiến đội của người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="state">Trạng thái (0: chưa nhận, 1: đã nhận)</param>
        private static void SetTeamBattleAwardState(KPlayer player, int state)
        {
            TeamBattle_ActivityScript.UpdateTeamAwardState(player, state == 1);
        }

        #endregion Thiết lập trạng thái nhận thưởng liên đấu

        #region Mở Captcha

        /// <summary>
        /// Mở Captcha cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        private static void GenerateCaptcha(int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GenerateCaptcha' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.GenerateCaptcha(target);
        }

        /// <summary>
        /// Mở Captcha cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        private static void GenerateCaptcha(KPlayer player)
        {
            player.GenerateCaptcha();
        }

        #endregion Mở Captcha

        #region Truy vấn Vòng quay may mắn

        /// <summary>
        /// Thiết lập số lượt đã quay Vòng quay may mắn - đặc biệt của người chơi tương ứng
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="totalTurn"></param>
        public static void SetTurnPlateTotalTurn(int targetID, int totalTurn)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetTurnPlateTotalTurn' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetTurnPlateTotalTurn(target, totalTurn);
        }

        /// <summary>
        /// Thiết lập số lượt đã quay Vòng quay may mắn - đặc biệt của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="totalTurn"></param>
        public static void SetTurnPlateTotalTurn(KPlayer player, int totalTurn)
        {
            player.TurnPlate_TotalTurn = totalTurn;
        }

        /// <summary>
        /// Thiết lập số lượt đã quay Vòng quay may mắn - đặc biệt của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="targetID"></param>
        public static void GetTurnPlateTotalTurn(KPlayer player, int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GetTurnPlateTotalTurn' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.GetTurnPlateTotalTurn(player, target);
        }

        /// <summary>
        /// Thiết lập số lượt đã quay Vòng quay may mắn - đặc biệt của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void GetTurnPlateTotalTurn(KPlayer player, KPlayer target)
        {
            KTPlayerManager.ShowNotification(player, string.Format("Tổng số lượt đã quay: {0}", target.LuckyCircle_TotalTurn));
        }

        /// <summary>
        /// Thiết lập số lượt đã quay Vòng quay may mắn của người chơi tương ứng
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="totalTurn"></param>
        public static void SetLuckyCircleTotalTurn(int targetID, int totalTurn)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'SetLuckyCircleTotalTurn' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.SetLuckyCircleTotalTurn(target, totalTurn);
        }

        /// <summary>
        /// Thiết lập số lượt đã quay Vòng quay may mắn của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="totalTurn"></param>
        public static void SetLuckyCircleTotalTurn(KPlayer player, int totalTurn)
        {
            player.LuckyCircle_TotalTurn = totalTurn;
        }

        /// <summary>
        /// Thiết lập số lượt đã quay Vòng quay may mắn của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="targetID"></param>
        public static void GetLuckyCircleTotalTurn(KPlayer player, int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'GetLuckyCircleTotalTurn' Error...\n" + "Target not found.");
                return;
            }

            KTGMCommandManager.GetLuckyCircleTotalTurn(player, target);
        }

        /// <summary>
        /// Thiết lập số lượt đã quay Vòng quay may mắn của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void GetLuckyCircleTotalTurn(KPlayer player, KPlayer target)
        {
            KTPlayerManager.ShowNotification(player, string.Format("Tổng số lượt đã quay: {0}", target.LuckyCircle_TotalTurn));
        }

        #endregion Truy vấn Vòng quay may mắn

        #region Tạo Pet

        /// <summary>
        /// Tạo pet cho người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="petID">ID pet</param>
        private static void CreatePet(int targetID, int petID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'CreatePet' Error...\n" + "Target not found.");
                return;
            }

            KTPlayerManager.CreatePet(target, petID);
        }

        /// <summary>
        /// Tạo pet cho người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="petID">ID pet</param>
        private static void CreatePet(KPlayer player, int petID)
        {
            KTPlayerManager.CreatePet(player, petID);
        }

        #endregion Tạo Pet

        #region Tăng điểm lĩnh ngộ cho Pet

        /// <summary>
        /// Tăng điểm lĩnh ngộ cho Pet đang tham chiến hiện tại
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="point">Số điểm lĩnh ngộ thêm vào</param>
        private static void AddPetEnlightenment(int targetID, int point)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetEnlightenment' Error...\n" + "Target not found.");
                return;
            }
            /// Nếu không có Pet đang tham chiến
            else if (target.CurrentPet == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetEnlightenment' Error...\n" + "No fighting pet found.");
                return;
            }

            KTGMCommandManager.AddPetEnlightenment(target, point);
        }

        /// <summary>
        /// Tăng điểm lĩnh ngộ cho Pet đang tham chiến hiện tại
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="point">Số điểm lĩnh ngộ thêm vào</param>
        private static void AddPetEnlightenment(KPlayer player, int point)
        {
            /// Nếu không có Pet đang tham chiến
            if (player.CurrentPet == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetEnlightenment' Error...\n" + "No fighting pet found.");
                return;
            }

            KTPetManager.AddPetEnlightenment(player, player.CurrentPet.RoleID - (int)ObjectBaseID.Pet, point);
        }

        #endregion Tăng điểm lĩnh ngộ cho Pet

        #region Tăng kinh nghiệm cho Pet

        /// <summary>
        /// Tăng kinh nghiệm cho Pet đang tham chiến hiện tại
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="exp">Lượng kinh nghiệm tăng thêm</param>
        private static void AddPetExp(int targetID, int exp)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetExp' Error...\n" + "Target not found.");
                return;
            }
            /// Nếu không có Pet đang tham chiến
            else if (target.CurrentPet == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetExp' Error...\n" + "No fighting pet found.");
                return;
            }

            KTGMCommandManager.AddPetExp(target, exp);
        }

        /// <summary>
        /// Tăng kinh nghiệm cho Pet đang tham chiến hiện tại
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="exp">Lượng kinh nghiệm tăng thêm</param>
        private static void AddPetExp(KPlayer player, int exp)
        {
            /// Nếu không có Pet đang tham chiến
            if (player.CurrentPet == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetExp' Error...\n" + "No fighting pet found.");
                return;
            }

            KTPetManager.AddPetExp(player.CurrentPet, exp);
        }

        #endregion Tăng kinh nghiệm cho Pet

        #region Tăng điểm vui vẻ cho Pet

        /// <summary>
        /// Tăng điểm vui vẻ cho Pet đang tham chiến hiện tại
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="value">Giá trị tăng thêm</param>
        private static void AddPetJoyful(int targetID, int value)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetJoyful' Error...\n" + "Target not found.");
                return;
            }
            /// Nếu không có Pet đang tham chiến
            else if (target.CurrentPet == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetJoyful' Error...\n" + "No fighting pet found.");
                return;
            }

            KTGMCommandManager.AddPetJoyful(target, value);
        }

        /// <summary>
        /// Tăng điểm vui vẻ cho Pet đang tham chiến hiện tại
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="value">Giá trị tăng thêm</param>
        private static void AddPetJoyful(KPlayer player, int value)
        {
            /// Nếu không có Pet đang tham chiến
            if (player.CurrentPet == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetJoyful' Error...\n" + "No fighting pet found.");
                return;
            }

            KTPetManager.AddPetJoyful(player, player.CurrentPet.RoleID - (int)ObjectBaseID.Pet, value);
        }

        #endregion Tăng điểm vui vẻ cho Pet

        #region Tăng tuổi thọ cho Pet

        /// <summary>
        /// Tăng tuổi thọ cho Pet đang tham chiến hiện tại
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        /// <param name="value">Giá trị tăng thêm</param>
        private static void AddPetLife(int targetID, int value)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetLife' Error...\n" + "Target not found.");
                return;
            }
            /// Nếu không có Pet đang tham chiến
            else if (target.CurrentPet == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetLife' Error...\n" + "No fighting pet found.");
                return;
            }

            KTGMCommandManager.AddPetLife(target, value);
        }

        /// <summary>
        /// Tăng tuổi thọ cho Pet đang tham chiến hiện tại
        /// </summary>
        /// <param name="player">Người chơi</param>
        /// <param name="value">Giá trị tăng thêm</param>
        private static void AddPetLife(KPlayer player, int value)
        {
            /// Nếu không có Pet đang tham chiến
            if (player.CurrentPet == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'AddPetLife' Error...\n" + "No fighting pet found.");
                return;
            }

            KTPetManager.AddPetLife(player, player.CurrentPet.RoleID - (int)ObjectBaseID.Pet, value);
        }

        #endregion Tăng tuổi thọ cho Pet

        #region Xóa mật khẩu cấp 2

        /// <summary>
        /// Xóa mật khẩu cấp 2 của người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        private static void ClearSecondPassword(int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ClearSecondPassword' Error...\n" + "Target not found.");
                return;
            }
            KTGMCommandManager.ClearSecondPassword(target);
        }

        /// <summary>
        /// Xóa mật khẩu cấp 2 của người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        private static void ClearSecondPassword(KPlayer player)
        {
            player.SecondPassword = "";
            player.TotalInputIncorrectSecondPasswordTimes = -1;
            player.IsSecondPasswordInput = false;

            /// Ghi vào DB
            KT_TCPHandler.SendDBUpdateSecondPassword(player);

            KTPlayerManager.ShowNotification(player, string.Format("Xóa khóa an toàn của [{0}] thành công!", player.RoleName));
        }

        #endregion Xóa mật khẩu cấp 2

        #region Hiện mật khẩu cấp 2

        /// <summary>
        /// Hiện mật khẩu cấp 2 của người chơi tương ứng
        /// </summary>
        /// <param name="targetID">ID đối tượng</param>
        private static void ShowSecondPassword(int targetID)
        {
            KPlayer target = KTGMCommandManager.FindPlayer(targetID);
            /// Kiểm tra đối tượng
            if (target == null)
            {
                LogManager.WriteLog(LogTypes.Error, "Process GM Command 'ShowSecondPassword' Error...\n" + "Target not found.");
                return;
            }
            KTGMCommandManager.ShowSecondPassword(target);
        }

        /// <summary>
        /// Hiện mật khẩu cấp 2 của người chơi tương ứng
        /// </summary>
        /// <param name="player">Người chơi</param>
        private static void ShowSecondPassword(KPlayer player)
        {
            /// Nếu không có
            if (string.IsNullOrEmpty(player.SecondPassword))
            {
                KTPlayerManager.ShowNotification(player, string.Format("Người chơi [{0}] chưa thiết lập khóa an toàn!", player.RoleName));
            }
            else
            {
                KTPlayerManager.ShowNotification(player, string.Format("Khóa an toàn của [{0}] là: {1}", player.RoleName, player.SecondPassword));
            }
        }

        #endregion Hiện mật khẩu cấp 2

        #endregion Thực thi lệnh GM
    }
}