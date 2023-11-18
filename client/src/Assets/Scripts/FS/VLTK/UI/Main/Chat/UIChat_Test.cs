using FS.GameEngine.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Các lệnh Test ở Client
    /// </summary>
    public class UIChat_Test
    {
        /// <summary>
        /// Xử lý lệnh Test ở Client
        /// </summary>
        /// <param name="cmdName"></param>
        /// <param name="args"></param>
        public static void ResolveClientTestCommand(string cmdName, params string[] args)
        {
            try
            {
                /// Loại lệnh
                switch (cmdName)
                {
                    /// Thay đổi quần áo
                    case "ChangeResArmor":
                    {
                        /// ID bộ
                        string resID = args[1];
                        /// Thực hiện thay đổi
                        Global.Data.Leader.ComponentCharacter.ChangeResArmor(resID);
                        KTGlobal.AddNotification("Thực hiện lệnh thành công!");
                        break;
                    }
                    /// Thay đổi quần áo
                    case "ChangeResHead":
                    {
                        /// ID bộ
                        string resID = args[1];
                        /// Thực hiện thay đổi
                        Global.Data.Leader.ComponentCharacter.ChangeResHelm(resID);
                        KTGlobal.AddNotification("Thực hiện lệnh thành công!");
                        break;
                    }
                    /// Thay đổi vũ khí
                    case "ChangeResWeapon":
                    {
                        /// ID bộ
                        string resID = args[1];
                        /// Thực hiện thay đổi
                        Global.Data.Leader.ComponentCharacter.ChangeResWeapon(resID);
                        KTGlobal.AddNotification("Thực hiện lệnh thành công!");
                        break;
                    }
                    /// Thay đổi phi phong
                    case "ChangeResMantle":
                    {
                        /// ID bộ
                        string resID = args[1];
                        /// Thực hiện thay đổi
                        Global.Data.Leader.ComponentCharacter.ChangeResMantle(resID);
                        KTGlobal.AddNotification("Thực hiện lệnh thành công!");
                        break;
                    }
                    /// Thay đổi phi phong
                    case "ChangeResHorse":
                    {
                        /// ID bộ
                        string resID = args[1];
                        /// Thực hiện thay đổi
                        Global.Data.Leader.ComponentCharacter.ChangeResHorse(resID);
                        KTGlobal.AddNotification("Thực hiện lệnh thành công!");
                        break;
                    }
                    /// Thay đổi trạng thái cưỡi
                    case "SetRideState":
                    {
                        /// Trạng thái
                        bool isRide = int.Parse(args[1]) == 1;
                        /// Thực hiện thay đổi
                        Global.Data.Leader.ComponentCharacter.Data.IsRiding = isRide;
                        KTGlobal.AddNotification("Thực hiện lệnh thành công!");
                        break;
                    }
                    /// Thay đổi res mặt nạ
                    case "ChangeResMask":
                    {
                        /// ID res
                        string resID = args[1];
                        /// Thực hiện thay đổi
                        Global.Data.Leader.ComponentCharacter.SetMaskID(resID);
                        KTGlobal.AddNotification("Thực hiện lệnh thành công!");
                        break;
                    }
                    default:
                    {
                        KTGlobal.AddNotification(string.Format("Không tìm thấy lệnh '{0}'.", cmdName));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                KTGlobal.AddNotification(string.Format("Có lỗi khi thực hiện lệnh '{0}'.", cmdName));
            }
        }
    }
}
