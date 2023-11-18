using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Factory;
using FS.VLTK.Factory.UIManager;
using FS.VLTK.UI;
using FS.VLTK.UI.CoreUI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.InputBox;
using FS.VLTK.UI.Main.ItemSlotBox;
using FS.VLTK.UI.Main.MessageBox;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Game Notification
        /// <summary>
        /// Hiển thị thông báo trong Game
        /// </summary>
        /// <param name="msg"></param>
        public static void AddNotification(string msg)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            canvas.UINotificationTip.AddText(msg);
        }
        #endregion

        #region System Notification
        /// <summary>
        /// Hiển thị dòng chữ chạy hệ thống
        /// </summary>
        /// <param name="msg"></param>
        public static void AddSystemMessage(string msg)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            canvas.UISystemNotification.AddMessage(msg);
        }
        #endregion

        #region InputNumber
        /// <summary>
        /// Hiển thị khung nhập số
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onOK"></param>
        public static void ShowInputNumber(string text, Action<int> onOK)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIInputNumber uiInputNumber = canvas.LoadUIPrefab<UIInputNumber>("MainGame/InputBox/UIInputNumber");
            canvas.AddUI(uiInputNumber, true);

            uiInputNumber.Text = text;
            uiInputNumber.OK = onOK;
        }

        /// <summary>
        /// Hiển thị khung nhập số
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="text"></param>
        /// <param name="onOK"></param>
        public static void ShowInputNumber(string text, int minValue, int maxValue, Action<int> onOK)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIInputNumber uiInputNumber = canvas.LoadUIPrefab<UIInputNumber>("MainGame/InputBox/UIInputNumber");
            canvas.AddUI(uiInputNumber, true);

            uiInputNumber.MinValue = minValue;
            uiInputNumber.MaxValue = maxValue;
            uiInputNumber.Text = text;
            uiInputNumber.OK = onOK;
        }

        /// <summary>
        /// Hiển thị khung nhập số
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="initValue"></param>
        /// <param name="text"></param>
        /// <param name="onOK"></param>
        public static void ShowInputNumber(string text, int minValue, int maxValue, int initValue, Action<int> onOK)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIInputNumber uiInputNumber = canvas.LoadUIPrefab<UIInputNumber>("MainGame/InputBox/UIInputNumber");
            canvas.AddUI(uiInputNumber, true);

            uiInputNumber.MinValue = minValue;
            uiInputNumber.MaxValue = maxValue;
            uiInputNumber.InitValue = initValue;
            uiInputNumber.Text = text;
            uiInputNumber.OK = onOK;
        }


        /// <summary>
        /// Hiển thị khung nhập số
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onOK"></param>
        /// <param name="onCancel"></param>
        public static void ShowInputNumber(string text, Action<int> onOK, Action onCancel)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIInputNumber uiInputNumber = canvas.LoadUIPrefab<UIInputNumber>("MainGame/InputBox/UIInputNumber");
            canvas.AddUI(uiInputNumber, true);

            uiInputNumber.Text = text;
            uiInputNumber.OK = onOK;
            uiInputNumber.Cancel = onCancel;
        }

        /// <summary>
        /// Hiển thị khung nhập số
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="text"></param>
        /// <param name="onOK"></param>
        /// <param name="onCancel"></param>
        public static void ShowInputNumber(string text, int minValue, int maxValue, Action<int> onOK, Action onCancel)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIInputNumber uiInputNumber = canvas.LoadUIPrefab<UIInputNumber>("MainGame/InputBox/UIInputNumber");
            canvas.AddUI(uiInputNumber, true);

            uiInputNumber.MinValue = minValue;
            uiInputNumber.MaxValue = maxValue;
            uiInputNumber.Text = text;
            uiInputNumber.OK = onOK;
            uiInputNumber.Cancel = onCancel;
        }

        /// <summary>
        /// Hiển thị khung nhập số
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="initValue"></param>
        /// <param name="text"></param>
        /// <param name="onOK"></param>
        /// <param name="onCancel"></param>
        public static void ShowInputNumber(string text, int minValue, int maxValue, int initValue, Action<int> onOK, Action onCancel)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIInputNumber uiInputNumber = canvas.LoadUIPrefab<UIInputNumber>("MainGame/InputBox/UIInputNumber");
            canvas.AddUI(uiInputNumber, true);

            uiInputNumber.MinValue = minValue;
            uiInputNumber.MaxValue = maxValue;
            uiInputNumber.InitValue = initValue;
            uiInputNumber.Text = text;
            uiInputNumber.OK = onOK;
            uiInputNumber.Cancel = onCancel;
        }
        #endregion

        #region Input String
        /// <summary>
        /// Hiển thị khung nhập chuỗi
        /// </summary>
        /// <param name="onOK"></param>
        public static void ShowInputString(Action<string> onOK)
		{
            KTGlobal.ShowInputString("Nhập vào chuỗi bên dưới", "", onOK, null);
        }

        /// <summary>
        /// Hiển thị khung nhập chuỗi
        /// </summary>
        /// <param name="onOK"></param>
        /// <param name="onCancel"></param>
        public static void ShowInputString(Action<string> onOK, Action onCancel)
		{
            KTGlobal.ShowInputString("Nhập vào chuỗi bên dưới", "", onOK, onCancel);
        }

        /// <summary>
        /// Hiển thị khung nhập chuỗi
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onOK"></param>
        /// <param name="onCancel"></param>
        public static void ShowInputString(string text, Action<string> onOK, Action onCancel)
		{
            KTGlobal.ShowInputString(text, "", onOK, onCancel);
        }

        /// <summary>
        /// Hiển thị khung nhập chuỗi
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onOK"></param>
        public static void ShowInputString(string text, Action<string> onOK)
		{
            KTGlobal.ShowInputString(text, "", onOK, null);
        }

        /// <summary>
        /// Hiển thị khung nhập chuỗi
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="initValue"></param>
        /// <param name="onOK"></param>
        public static void ShowInputString(string text, string initValue, Action<string> onOK)
		{
            KTGlobal.ShowInputString(text, initValue, onOK, null);
		}

        /// <summary>
        /// Hiển thị khung nhập chuỗi
        /// </summary>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="initValue"></param>
        /// <param name="onOK"></param>
        /// <param name="onCancel"></param>
        public static void ShowInputString(string text, string initValue, Action<string> onOK, Action onCancel)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIInputString uiInputString = canvas.LoadUIPrefab<UIInputString>("MainGame/InputBox/UIInputString");
            canvas.AddUI(uiInputString, true);

            uiInputString.Description = text;
            uiInputString.Text = initValue;
            uiInputString.OK = onOK;
            uiInputString.Cancel = onCancel;
        }
        #endregion

        #region InputItems
        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="hint"></param>
        /// <param name="predicate"></param>
        /// <param name="onAccept"></param>
        public static void ShowInputItems(string title, string description, string hint, Predicate<GoodsData> predicate, Action<List<GoodsData>> onAccept)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIInputItems uiInputItems = canvas.LoadUIPrefab<UIInputItems>("MainGame/UIInputItems");
            canvas.AddUI(uiInputItems);

            uiInputItems.Title = title;
            uiInputItems.Description = description;
            uiInputItems.OtherDetail = hint;
            uiInputItems.Predicate = predicate;
            uiInputItems.OK = onAccept;
            uiInputItems.Close = () =>
            {
                GameObject.Destroy(uiInputItems.gameObject);
            };
        }
        #endregion

        #region Select pet
        /// <summary>
        /// Mở khung chọn pet
        /// </summary>
        /// <param name="onSelect"></param>
        public static void ShowSelectPet(Action<int> onSelect)
        {
            PlayZone.Instance.OpenUISelectPet(onSelect);
        }
        #endregion

        #region Game MessageBox
        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề "Thông báo" và không có button nào
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content)
        {
            KTGlobal.ShowMessageBox("Thông báo", content, null, null);
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề "Thông báo" và gồm button đồng ý hay không
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isShowButtonOK"></param>
        public static void ShowMessageBox(string content, bool isShowButtonOK)
        {
            if (isShowButtonOK)
            {
                KTGlobal.ShowMessageBox("Thông báo", content, () => { }, null);
            }
            else
            {
                KTGlobal.ShowMessageBox("Thông báo", content, null, null);
            }
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề "Thông báo" và gồm button đồng ý
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content, Action onOK)
        {
            KTGlobal.ShowMessageBox("Thông báo", content, onOK, null);
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề "Thông báo", gồm button đồng ý và có button hủy bỏ hay không 
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content, Action onOK, bool isShowButtonCancel)
        {
            if (isShowButtonCancel)
            {
                KTGlobal.ShowMessageBox("Thông báo", content, onOK, () => { });
            }
            else
            {
                KTGlobal.ShowMessageBox("Thông báo", content, onOK, null);
            }
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề "Thông báo", gồm button đồng ý và button hủy bỏ
        /// </summary>
        /// <param name="content"></param>
        public static void ShowMessageBox(string content, Action onOK, Action onCancel)
        {
            KTGlobal.ShowMessageBox("Thông báo", content, onOK, onCancel);
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề chỉ định không có button nào
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        public static void ShowMessageBox(string title, string content)
        {
            KTGlobal.ShowMessageBox(title, content, null, null);
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề chỉ định gồm button đồng ý hay không
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="isShowButtonOK"></param>
        public static void ShowMessageBox(string title, string content, bool isShowButtonOK)
        {
            if (isShowButtonOK)
            {
                KTGlobal.ShowMessageBox(title, content, () => { }, null);
            }
            else
            {
                KTGlobal.ShowMessageBox(title, content, null, null);
            }
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề chỉ định gồm button đồng ý
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="onOK"></param>
        public static void ShowMessageBox(string title, string content, Action onOK)
        {
            KTGlobal.ShowMessageBox(title, content, onOK, null);
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề chỉ định gồm button đồng ý và có button hủy bỏ hay không 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="onOK"></param>
        /// <param name="isShowButtonCancel"></param>
        public static void ShowMessageBox(string title, string content, Action onOK, bool isShowButtonCancel)
        {
            if (isShowButtonCancel)
            {
                KTGlobal.ShowMessageBox(title, content, onOK, () => { });
            }
            else
            {
                KTGlobal.ShowMessageBox(title, content, onOK, null);
            }
        }

        /// <summary>
        /// Hiển thị bảng thông báo IN-GAME tiêu đề chỉ định gồm 2 button đồng ý và hủy bỏ 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="onOK"></param>
        /// <param name="onCancel"></param>
        public static void ShowMessageBox(string title, string content, Action onOK, Action onCancel)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIMessageBox uiMessageBox = canvas.LoadUIPrefab<UIMessageBox>("MainGame/UIMessageBox");
            canvas.AddUI(uiMessageBox, true);
            uiMessageBox.Title = title;
            uiMessageBox.Content = content;
            uiMessageBox.ShowButtonOK = onOK != null;
            uiMessageBox.ShowButtonCancel = onCancel != null;
            uiMessageBox.OK = onOK;
            uiMessageBox.Cancel = onCancel;
        }
        #endregion

        #region Thông báo nhặt vật phẩm
        /// <summary>
        /// Thông báo nhận được vật phẩm số lượng tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="count"></param>
        public static void HintNewGoodsText(GoodsData itemGD, int count)
        {
            if (itemGD == null || count <= 0)
            {
                return;
            }

            /// Thêm vào danh sách biểu diễn
            UIHintItemManager.Instance.AddHint(itemGD, count);
        }
        #endregion

        #region Skill Result
        /// <summary>
        /// Hiển thị kết quả sát thương của kỹ năng
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="type"></param>
        /// <param name="damage"></param>
        public static void ShowSkillResultText(GSprite caster, GSprite target, int type, int damage)
        {
            switch (type)
            {
                case (int) VLTK.Entities.Enum.SkillResult.Miss:
                    /// Nếu bản thân là đối tượng bị tấn công
                    if (Global.Data.Leader == target)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DODGE, "Trượt");
                    }
                    /// Nếu pet của bản thân là đối tượng bị tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == target.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DODGE, "Trượt");
                    }
                    /// Nếu xe tiêu của bản thân là đối tượng bị tấn công
                    else if (target.SpriteType == GSpriteTypes.TraderCarriage && target.TraderCarriageData.OwnerID == Global.Data.RoleData.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DODGE, "Trượt");
                    }
                    /// Nếu bản thân là đối tượng tấn công
                    else if (Global.Data.Leader == caster)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DODGE, "Trượt");
                    }
                    /// Nếu pet của bản thân là đối tượng tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == caster.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DODGE, "Trượt");
                    }
                    break;
                case (int) VLTK.Entities.Enum.SkillResult.Immune:
                    /// Nếu bản thân là đối tượng bị tấn công
                    if (Global.Data.Leader == target)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.IMMUNE, "Miễn dịch");
                    }
                    /// Nếu pet của bản thân là đối tượng bị tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == target.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.IMMUNE, "Miễn dịch");
                    }
                    /// Nếu xe tiêu của bản thân là đối tượng bị tấn công
                    else if (target.SpriteType == GSpriteTypes.TraderCarriage && target.TraderCarriageData.OwnerID == Global.Data.RoleData.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.IMMUNE, "Miễn dịch");
                    }
                    /// Nếu bản thân là đối tượng tấn công
                    else if (Global.Data.Leader == caster)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.IMMUNE, "Miễn dịch");
                    }
                    /// Nếu pet của bản thân là đối tượng tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == caster.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.IMMUNE, "Miễn dịch");
                    }
                    break;
                case (int) VLTK.Entities.Enum.SkillResult.Adjust:
                    /// Nếu bản thân là đối tượng bị tấn công
                    if (Global.Data.Leader == target)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.ADJUST, "Hóa giải");
                    }
                    /// Nếu pet của bản thân là đối tượng bị tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == target.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.ADJUST, "Hóa giải");
                    }
                    /// Nếu xe tiêu của bản thân là đối tượng bị tấn công
                    else if (target.SpriteType == GSpriteTypes.TraderCarriage && target.TraderCarriageData.OwnerID == Global.Data.RoleData.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.ADJUST, "Hóa giải");
                    }
                    /// Nếu bản thân là đối tượng tấn công
                    else if (Global.Data.Leader == caster)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.ADJUST, "Hóa giải");
                    }
                    /// Nếu pet của bản thân là đối tượng tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == caster.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.ADJUST, "Hóa giải");
                    }
                    break;
                case (int) VLTK.Entities.Enum.SkillResult.Normal:
                    /// Nếu bản thân là đối tượng bị tấn công
                    if (Global.Data.Leader == target)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DAMAGE_TAKEN, damage.ToString());
                    }
                    /// Nếu pet của bản thân là đối tượng bị tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == target.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DAMAGE_TAKEN, damage.ToString());
                    }
                    /// Nếu xe tiêu của bản thân là đối tượng bị tấn công
                    else if (target.SpriteType == GSpriteTypes.TraderCarriage && target.TraderCarriageData.OwnerID == Global.Data.RoleData.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DAMAGE_TAKEN, damage.ToString());
                    }
                    /// Nếu bản thân là đối tượng tấn công
                    else if (Global.Data.Leader == caster)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DAMAGE_DEALT, damage.ToString());
                    }
                    /// Nếu pet của bản thân là đối tượng tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == caster.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.PET_DAMAGE_DEALT, damage.ToString());
                    }
                    break;
                case (int) VLTK.Entities.Enum.SkillResult.Crit:
                    /// Nếu bản thân là đối tượng bị tấn công
                    if (Global.Data.Leader == target)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DAMAGE_TAKEN, damage.ToString());
                    }
                    /// Nếu pet của bản thân là đối tượng bị tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == target.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DAMAGE_TAKEN, damage.ToString());
                    }
                    /// Nếu xe tiêu của bản thân là đối tượng bị tấn công
                    else if (target.SpriteType == GSpriteTypes.TraderCarriage && target.TraderCarriageData.OwnerID == Global.Data.RoleData.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.DAMAGE_TAKEN, damage.ToString());
                    }
                    /// Nếu bản thân là đối tượng tấn công
                    else if (Global.Data.Leader == caster)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.CRIT_DAMAGE_DEALT, damage.ToString());
                    }
                    /// Nếu pet của bản thân là đối tượng tấn công
                    else if (Global.Data.RoleData.CurrentPetID != -1 && Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet == caster.RoleID)
                    {
                        KTGlobal.ShowSpriteHeaderText(target, FS.VLTK.Entities.Enum.DamageType.PET_CRIT_DAMAGE_DEALT, damage.ToString());
                    }
                    break;
            }
        }

        /// <summary>
        /// Hiển thị text sát thương nhận
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        public static void ShowSpriteHeaderText(GSprite sprite, FS.VLTK.Entities.Enum.DamageType type, string content)
        {
            sprite.AddHeadText(type, content);
        }
        #endregion

        #region Thêm kinh nghiệm, vàng, tinh hoạt lực
        /// <summary>
        /// Hiển thị text kinh nghiệm hoặc tiền vàng
        /// </summary>
        /// <param name="type"></param>
        /// <param name="content"></param>
        public static void ShowTextForExpMoneyOrGatherMakePoint(FS.VLTK.Entities.Enum.BottomTextDecorationType type, string content)
        {
            /// Tạo mới dữ liệu
            UIBottomTextManager.TextData textData = new UIBottomTextManager.TextData();
            textData.Text = content;
            textData.Offset = Vector2.zero;
            textData.Duration = 3f;

            Color _color;
            switch (type)
            {
                case VLTK.Entities.Enum.BottomTextDecorationType.Money:
                    ColorUtility.TryParseHtmlString("#ffff4d", out _color);
                    textData.Color = _color;
                    break;
                case VLTK.Entities.Enum.BottomTextDecorationType.BoundMoney:
                    ColorUtility.TryParseHtmlString("#ffff4d", out _color);
                    textData.Color = _color;
                    break;
                case VLTK.Entities.Enum.BottomTextDecorationType.Coupon:
                    ColorUtility.TryParseHtmlString("#ffff4d", out _color);
                    textData.Color = _color;
                    break;
                case VLTK.Entities.Enum.BottomTextDecorationType.Coupon_Bound:
                    ColorUtility.TryParseHtmlString("#ffff4d", out _color);
                    textData.Color = _color;
                    break;
                case VLTK.Entities.Enum.BottomTextDecorationType.Exp:
                    ColorUtility.TryParseHtmlString("#ff4de4", out _color);
                    textData.Color = _color;
                    break;
                case VLTK.Entities.Enum.BottomTextDecorationType.Gather:
                    ColorUtility.TryParseHtmlString("#ff4de4", out _color);
                    textData.Color = _color;
                    break;
                case VLTK.Entities.Enum.BottomTextDecorationType.Make:
                    ColorUtility.TryParseHtmlString("#ff4de4", out _color);
                    textData.Color = _color;
                    break;
            }
  
            /// Thêm vào hàng đợi biểu diễn
            UIBottomTextManager.Instance.AddText(textData);
        }
        #endregion

        #region Màn hình chờ tải cái gì đó
        /// <summary>
        /// Hiển thị màn hình chờ tải cái gì đó
        /// </summary>
        /// <param name="hint"></param>
        public static void ShowLoadingFrame(string hint)
        {
            UILoadingProgress uiLoadingProgress = Global.MainCanvas.GetComponent<CanvasManager>().UILoadingProgress;
            uiLoadingProgress.Hint = hint;
            uiLoadingProgress.Show();
        }

        /// <summary>
        /// Ẩn màn hình chờ tải cái gì đó
        /// </summary>
        public static void HideLoadingFrame()
        {
            UILoadingProgress uiLoadingProgress = Global.MainCanvas.GetComponent<CanvasManager>().UILoadingProgress;
            uiLoadingProgress.Hide();
        }
        #endregion

        #region Khung danh sách vật phẩm
        /// <summary>
        /// Hiện khung danh sách vật phẩm
        /// </summary>
        /// <param name="description"></param>
        /// <param name="goods"></param>
        /// <param name="itemClick"></param>
        /// <param name="close"></param>
        public static UIListItemBox ShowItemListBox(string description, List<GoodsData> goods, Action<GoodsData> itemClick, Action close)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIListItemBox uiListItemBox = canvas.LoadUIPrefab<UIListItemBox>("MainGame/UIListItemBox");
            canvas.AddUI(uiListItemBox);
            uiListItemBox.Description = description;
            uiListItemBox.Items = goods;
            uiListItemBox.ItemClick = itemClick;
            uiListItemBox.Close = close;
            return uiListItemBox;
        }

        /// <summary>
        /// Hiện khung danh sách vật phẩm
        /// </summary>
        /// <param name="goods"></param>
        public static UIListItemBox ShowItemListBox(List<GoodsData> goods)
        {
            return KTGlobal.ShowItemListBox("Danh sách vật phẩm", goods, null, null);
        }

        /// <summary>
        /// Hiện khung danh sách vật phẩm
        /// </summary>
        /// <param name="description"></param>
        /// <param name="goods"></param>
        public static UIListItemBox ShowItemListBox(string description, List<GoodsData> goods)
        {
            return KTGlobal.ShowItemListBox(description, goods, null, null);
        }

        /// <summary>
        /// Hiện khung danh sách vật phẩm
        /// </summary>
        /// <param name="goods"></param>
        /// <param name="itemClick"></param>
        public static UIListItemBox ShowItemListBox(List<GoodsData> goods, Action<GoodsData> itemClick)
        {
            return KTGlobal.ShowItemListBox("Danh sách vật phẩm", goods, itemClick, null);
        }

        /// <summary>
        /// Hiện khung danh sách vật phẩm
        /// </summary>
        /// <param name="description"></param>
        /// <param name="goods"></param>
        /// <param name="itemClick"></param>
        public static UIListItemBox ShowItemListBox(string description, List<GoodsData> goods, Action<GoodsData> itemClick)
        {
            return KTGlobal.ShowItemListBox(description, goods, itemClick, null);
        }

        /// <summary>
        /// Hiện khung danh sách vật phẩm
        /// </summary>
        /// <param name="goods"></param>
        /// <param name="itemClick"></param>
        /// <param name="close"></param>
        public static UIListItemBox ShowItemListBox(List<GoodsData> goods, Action<GoodsData> itemClick, Action close)
        {
            return KTGlobal.ShowItemListBox("Danh sách vật phẩm", goods, itemClick, close);
        }
        #endregion
    }
}
