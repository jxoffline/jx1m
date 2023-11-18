using GameServer.KiemThe.Core.Activity.LuckyCircle;
using GameServer.KiemThe.Core.Activity.TurnPlate;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem.Entities;
using GameServer.Logic;
using MoonSharp.Interpreter;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameServer.KiemThe.LuaSystem.Logic
{
    /// <summary>
    /// Cung cấp thư viện dùng cho Lua, giao diện
    /// </summary>
    [MoonSharpUserData]
    public static class KTLuaLib_GUI
    {
        #region NPC, Item Dialog
        /// <summary>
        /// Tạo mới cửa sổ hội thoại của NPC gồm danh sách các sự lựa chọn, và danh sách vật phẩm cần chọn ra 1 cái hoặc chỉ hiện ra mà không cho chọn
        /// </summary>
        /// <returns></returns>
        public static Lua_NPCDialog CreateNPCDialog()
        {
            return new Lua_NPCDialog();
        }


     
        /// <summary>
        /// Tạo mới cửa sổ hội thoại của vật phẩm gồm danh sách các sự lựa chọn, và danh sách vật phẩm cần chọn ra 1 cái hoặc chỉ hiện ra mà không cho chọn
        /// </summary>
        /// <returns></returns>
        public static Lua_ItemDialog CreateItemDialog()
        {
            return new Lua_ItemDialog();
        }

        /// <summary>
        /// Đóng bảng hội thoại từ NPCDialog hoặc ItemDialog đã mở
        /// </summary>
        public static void CloseDialog(Lua_Player player)
        {
            KT_TCPHandler.CloseDialog(player.RefObject);
        }
        #endregion

        #region Notification
        /// <summary>
        /// Hiển thị thông báo lên người chơi
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="message"></param>
        public static void ShowNotification(Lua_Player player, string message)
        {
            if (player.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on GUI.ShowNotification, Player is NULL."));
                return;
            }

            KTPlayerManager.ShowNotification(player.RefObject, message);
        }
        #endregion

        #region Open UI
        /// <summary>
        /// Mở khung bất kỳ cho người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="uiName"></param>
        /// <param name="parameters"></param>
        public static void OpenUI(Lua_Player player, string uiName, params int[] parameters)
        {
            if (player.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on GUI.OpenUI, Player is NULL."));
                return;
            }

            KT_TCPHandler.SendOpenUI(player.RefObject, uiName, parameters);
        }

        /// <summary>
        /// Đóng khung bất kỳ của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="uiName"></param>
        public static void CloseUI(Lua_Player player, string uiName)
        {
            if (player.RefObject == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Lua error on GUI.CloseUI, Player is NULL."));
                return;
            }

            KT_TCPHandler.SendCloseUI(player.RefObject, uiName);
        }
        #endregion

        #region Send System message
        /// <summary>
        /// Gửi tin nhắn hệ thống tới tất cả người chơi
        /// </summary>
        /// <param name="message"></param>
        /// <param name="player"></param>
        /// <param name="items"></param>
        /// <param name="pets"></param>
        public static void SendSystemMessage(string message, Lua_Item[] items = null, Lua_Player player = null, int[] pets = null)
        {
            /// Danh sách vật phẩm
            List<GoodsData> itemGDs = null;
            /// Nếu tồn tại danh sách vật phẩm
            if (items != null && items.Length > 0)
            {
                /// Tạo mới
                itemGDs = new List<GoodsData>();
                /// Duyệt danh sách
                foreach (Lua_Item luaItem in items)
                {
                    /// Thêm vào
                    itemGDs.Add(luaItem.RefObject);
                }
            }

            /// Danh sách pet
            List<PetData> petDs = null;
            /// Nếu tòn tại danh sách pet
            if (player != null && pets != null && pets.Length > 0)
            {
                /// Tạo mới
                petDs = new List<PetData>();
                /// Duyệt danh sách
                foreach (int petID in pets)
                {
                    /// Thông tin pet
                    PetData pd;
                    /// Nếu pet này đang tham chiến
                    if (player.RefObject.CurrentPet != null && player.RefObject.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
                    {
                        pd = player.RefObject.CurrentPet.GetDBData();
                    }
                    /// Nếu pet này không tham chiến
                    else
                    {
                        /// Thông tin pet tương ứng
                        pd = player.RefObject.PetList?.Where(x => x.ID == petID).FirstOrDefault();
                        /// Toác
                        if (pd == null)
                        {
                            /// Tiếp tục
                            continue;
                        }
                        /// Tính toán chỉ số ảo
                        pd = KTPetManager.CalculatePetAttributes(pd);
                    }

                    /// Thêm vào danh sách
                    petDs.Add(pd);
                }

                /// Toác
                if (petDs.Count <= 0)
                {
                    petDs = null;
                }
            }

            /// Thực hiện
            KTGlobal.SendSystemChat(message, itemGDs, petDs);
        }

        /// <summary>
        /// Gửi tin nhắn kênh hệ thống kèm dòng chữ chạy trên đầu tới tất cả người chơi
        /// </summary>
        /// <param name="message"></param>
        /// <param name="player"></param>
        /// <param name="items"></param>
        /// <param name="pets"></param>
        public static void SendSystemEventNotification(string message, Lua_Item[] items = null, Lua_Player player = null, int[] pets = null)
        {
            /// Danh sách vật phẩm
            List<GoodsData> itemGDs = null;
            /// Nếu tồn tại danh sách vật phẩm
            if (items != null && items.Length > 0)
            {
                /// Tạo mới
                itemGDs = new List<GoodsData>();
                /// Duyệt danh sách
                foreach (Lua_Item luaItem in items)
                {
                    /// Thêm vào
                    itemGDs.Add(luaItem.RefObject);
                }
            }

            /// Danh sách pet
            List<PetData> petDs = null;
            /// Nếu tòn tại danh sách pet
            if (player != null && pets != null && pets.Length > 0)
            {
                /// Tạo mới
                petDs = new List<PetData>();
                /// Duyệt danh sách
                foreach (int petID in pets)
                {
                    /// Thông tin pet
                    PetData pd;
                    /// Nếu pet này đang tham chiến
                    if (player.RefObject.CurrentPet != null && player.RefObject.CurrentPet.RoleID - (int) ObjectBaseID.Pet == petID)
                    {
                        pd = player.RefObject.CurrentPet.GetDBData();
                    }
                    /// Nếu pet này không tham chiến
                    else
                    {
                        /// Thông tin pet tương ứng
                        pd = player.RefObject.PetList?.Where(x => x.ID == petID).FirstOrDefault();
                        /// Toác
                        if (pd == null)
                        {
                            /// Tiếp tục
                            continue;
                        }
                        /// Tính toán chỉ số ảo
                        pd = KTPetManager.CalculatePetAttributes(pd);
                    }

                    /// Thêm vào danh sách
                    petDs.Add(pd);
                }

                /// Toác
                if (petDs.Count <= 0)
                {
                    petDs = null;
                }
            }

            /// Thực hiện
            KTGlobal.SendSystemEventNotification(message, itemGDs, petDs);
        }
        #endregion

        #region Message Box
        /// <summary>
        /// Hiện MessageBox tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        public static void ShowMessageBox(Lua_Player player, string title, string text)
        {
            KTPlayerManager.ShowMessageBox(player.RefObject, title, text);
        }

        /// <summary>
        /// Hiện MessageBox tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        public static void ShowMessageBox(Lua_Player player, string title, string text, Closure ok)
        {
            KTPlayerManager.ShowMessageBox(player.RefObject, title, text, () => {
                KTLuaScript.Instance.ExecuteFunctionAsync("MessageBox:OK", ok, null, null);
            });
        }

        /// <summary>
        /// Hiện MessageBox tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        public static void ShowMessageBox(Lua_Player player, string title, string text, Closure ok, Closure cancel)
        {
            KTPlayerManager.ShowMessageBox(player.RefObject, title, text, () => {
                KTLuaScript.Instance.ExecuteFunctionAsync("MessageBox:OK", ok, null, null);
            }, () => {
                KTLuaScript.Instance.ExecuteFunctionAsync("MessageBox:Cancel", cancel, null, null);
            });
        }

        /// <summary>
        /// Hiện InputNumber tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        public static void ShowInputNumber(Lua_Player player, string text, Closure ok)
        {
            KTPlayerManager.ShowInputNumberBox(player.RefObject, text, (value) => {
                object[] parameters = new object[]
                {
                    value,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync("InputNumber:OK", ok, parameters, null);
            });
        }

        /// <summary>
        /// Hiện InputNumber tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="text"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        public static void ShowInputNumber(Lua_Player player, string text, Closure ok, Closure cancel)
        {
            KTPlayerManager.ShowInputNumberBox(player.RefObject, text, (value) => {
                object[] parameters = new object[]
                {
                    value,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync("InputNumber:OK", ok, parameters, null);
            }, () => {
                KTLuaScript.Instance.ExecuteFunctionAsync("InputNumber:Cancel", cancel, null, null);
            });
        }

        /// <summary>
        /// Hiện InputString tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        public static void ShowInputString(Lua_Player player, string description, Closure ok, Closure cancel)
        {
            KTPlayerManager.ShowInputStringBox(player.RefObject, description, (value) => {
                object[] parameters = new object[]
                {
                    value,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync("InputString:OK", ok, parameters, null);
            }, () => {
                KTLuaScript.Instance.ExecuteFunctionAsync("InputString:Cancel", cancel, null, null);
            });
        }

        /// <summary>
        /// Hiện InputString tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="initValue"></param>
        /// <param name="ok"></param>
        public static void ShowInputString(Lua_Player player, string description, Closure ok)
        {
            KTPlayerManager.ShowInputStringBox(player.RefObject, description, (value) => {
                object[] parameters = new object[]
                {
                    value,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync("InputString:OK", ok, parameters, null);
            });
        }

        /// <summary>
        /// Hiện InputString tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="initValue"></param>
        /// <param name="ok"></param>
        public static void ShowInputString(Lua_Player player, string description, string initValue, Closure ok)
		{
            KTPlayerManager.ShowInputStringBox(player.RefObject, description, initValue, (value) => {
                object[] parameters = new object[]
                {
                    value,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync("InputString:OK", ok, parameters, null);
            });
        }

        /// <summary>
        /// Hiện InputString tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="initValue"></param>
        /// <param name="ok"></param>
        /// <param name="cancel"></param>
        public static void ShowInputString(Lua_Player player, string description, string initValue, Closure ok, Closure cancel)
		{
            KTPlayerManager.ShowInputStringBox(player.RefObject, description, initValue, (value) => {
                object[] parameters = new object[]
                {
                    value,
                };
                KTLuaScript.Instance.ExecuteFunctionAsync("InputString:OK", ok, parameters, null);
            }, () => {
                KTLuaScript.Instance.ExecuteFunctionAsync("InputString:Cancel", cancel, null, null);
            });
        }
		#endregion

		#region Input Second Password
        /// <summary>
        /// Mở khung nhập mật khẩu cấp 2
        /// </summary>
        /// <param name="player"></param>
        public static void OpenInputSecondPassword(Lua_Player player)
		{
            KT_TCPHandler.SendOpenInputSecondPassword(player.RefObject);
		}
		#endregion

		#region Danh sách xóa vật phẩm
        /// <summary>
        /// Mở khung tiêu hủy vật phẩm
        /// </summary>
        /// <param name="player"></param>
        public static void OpenRemoveItems(Lua_Player player)
		{
            KTPlayerManager.OpenRemoveItems(player.RefObject);
		}
        #endregion

        #region Danh sách ghép vật phẩm
        /// <summary>
        /// Mở khung ghép vật phẩm
        /// </summary>
        /// <param name="player"></param>
        public static void OpenMergeItems(Lua_Player player)
		{
            KTPlayerManager.OpenMergeItems(player.RefObject);
		}
        #endregion

        #region Đổi quan ấn, phi phong, ngũ hành ấn về đúng hệ
        /// <summary>
        /// Mở khung đổi quan ấn, phi phong, ngũ hành ấn về đúng hệ
        /// </summary>
        /// <param name="player"></param>
        public static void OpenChangeSignetMantleAndChopstick(Lua_Player player)
        {
            KTPlayerManager.OpenChangeSignetMantleAndChopstick(player.RefObject);
        }
        #endregion

        #region Đổi tên
        /// <summary>
        /// Mở khung đổi tên nhân vật
        /// </summary>
        /// <param name="player"></param>
        public static void OpenChangeName(Lua_Player player)
        {
            /// Nếu không có thẻ đổi tên
            if (ItemManager.GetItemCountInBag(player.RefObject, KTGlobal.ChangeNameCardItemID) < 1)
			{
                KTPlayerManager.ShowNotification(player.RefObject, "Cần có [Thẻ đổi tên] mới có thể sử dụng chức năng này!");
                return;
			}

            /// Mở khung nhập tên cần đổi
            KTPlayerManager.ShowInputStringBox(player.RefObject, "Nhập tên cần đổi (từ 6 đến 18 ký tự).", (newName) => {
                /// Thực hiện đổi tên
                KTChangeNameManager.Instance.ProcessChangeName(player.RefObject, newName, (_oldName, _newName) => {
                    /// Xóa Thẻ đổi tên
                    ItemManager.RemoveItemFromBag(player.RefObject, KTGlobal.ChangeNameCardItemID, 1);
                    /// Show hàng
                    KTGlobal.SendSystemChat(string.Format("Người chơi <color=#38c0ff>[{0}]</color> đã đổi tên thành <color=#38c0ff>[{1}]</color>", _oldName, _newName));
                });
            });
		}
        #endregion

        #region Mở Vòng quay may mắn
        /// <summary>
        /// Mở khung Vòng quay may mắn
        /// </summary>
        /// <param name="player"></param>
        public static void OpenLuckyCircle(Lua_Player player)
        {
            KTLuckyCircleManager.OpenCircle(player.RefObject);
        }

        /// <summary>
        /// Mở khung Vòng quay may mắn - đặc biệt
        /// </summary>
        /// <param name="player"></param>
        public static void OpenTurnPlate(Lua_Player player)
        {
            KTTurnPlateManager.OpenCircle(player.RefObject);
        }
        #endregion

        #region Chuỗi thông tin
        /// <summary>
        /// Trả về thông tin vật phẩm tương ứng dạng chuỗi
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetItemInfoString(Lua_Item item)
        {
            return KTGlobal.GetItemDescInfoStringForChat(item.RefObject);
        }

        /// <summary>
        /// Trả về thông tin pet tương ứng dạng chuỗi
        /// </summary>
        /// <param name="player"></param>
        /// <param name="petID"></param>
        /// <returns></returns>
        public static string GetPetInfoString(Lua_Player player, int petID)
        {
            PetData pd = player.RefObject.PetList?.Where(x => x.ID == petID).FirstOrDefault();
            /// Toác
            if (pd == null)
            {
                return "";
            }
            return KTGlobal.GetPetDescInfoStringForChat(pd);
        }
        #endregion
    }
}
