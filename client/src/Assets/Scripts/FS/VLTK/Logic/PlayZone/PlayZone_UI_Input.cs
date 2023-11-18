using FS.GameEngine.Logic;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.SecondPassword;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region UIInputItems
    /// <summary>
    /// Khung đặt vào danh sách vật phẩm
    /// </summary>
    public UIInputItems UIInputItems { get; protected set; }

    /// <summary>
    /// Mở khung đặt vào danh sách vật phẩm
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="otherDetail"></param>
    /// <param name="tag"></param>
    public void OpenUIInputItems(string title, string description, string otherDetail, string tag)
    {
        if (this.UIInputItems != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIInputItems = canvas.LoadUIPrefab<UIInputItems>("MainGame/UIInputItems");
        canvas.AddUI(this.UIInputItems);

        this.UIInputItems.Title = title;
        this.UIInputItems.Description = description;
        this.UIInputItems.OtherDetail = otherDetail;
        this.UIInputItems.Close = this.CloseUIInputItems;
        this.UIInputItems.OK = (items) => {
            KT_TCPHandler.SendInputItems(items, tag);
        };
    }

    /// <summary>
    /// Đóng khung đặt vào danh sách vật phẩm
    /// </summary>
    public void CloseUIInputItems()
    {
        if (this.UIInputItems != null)
        {
            GameObject.Destroy(this.UIInputItems.gameObject);
            this.UIInputItems = null;
        }
    }
    #endregion

    #region UIInputEquipAndMaterials
    /// <summary>
    /// Khung đặt vào danh sách vật phẩm
    /// </summary>
    public UIInputEquipAndMaterials UIInputEquipAndMaterials { get; protected set; }

    /// <summary>
    /// Mở khung đặt vào danh sách vật phẩm
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="otherDetail"></param>
    /// <param name="mustInputItems"></param>
    /// <param name="tag"></param>
    public void OpenUIInputEquipAndMaterials(string title, string description, string otherDetail, bool mustInputItems, string tag)
    {
        if (this.UIInputEquipAndMaterials != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIInputEquipAndMaterials = canvas.LoadUIPrefab<UIInputEquipAndMaterials>("MainGame/UIInputEquipAndMaterials");
        canvas.AddUI(this.UIInputEquipAndMaterials);

        this.UIInputEquipAndMaterials.Title = title;
        this.UIInputEquipAndMaterials.Description = description;
        this.UIInputEquipAndMaterials.OtherDetail = otherDetail;
        this.UIInputEquipAndMaterials.MustIncludeMaterials = mustInputItems;
        this.UIInputEquipAndMaterials.Close = this.CloseUIInputEquipAndMaterials;
        this.UIInputEquipAndMaterials.OK = (equip, items) => {
            KT_TCPHandler.SendInputEquipAndMaterials(equip, items, tag);
        };
    }

    /// <summary>
    /// Đóng khung đặt vào danh sách vật phẩm
    /// </summary>
    public void CloseUIInputEquipAndMaterials()
    {
        if (this.UIInputEquipAndMaterials != null)
        {
            GameObject.Destroy(this.UIInputEquipAndMaterials.gameObject);
            this.UIInputEquipAndMaterials = null;
        }
    }
    #endregion

    #region UISelectPet
    /// <summary>
    /// Khung chọn pet
    /// </summary>
    public UISelectPet UISelectPet { get; protected set; }

    /// <summary>
    /// Mở khung chọn pet
    /// </summary>
    /// <param name="title"></param>
    /// <param name="description"></param>
    /// <param name="otherDetail"></param>
    /// <param name="tag"></param>
    public void OpenUISelectPet(Action<int> onSelect)
    {
        if (this.UISelectPet != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISelectPet = canvas.LoadUIPrefab<UISelectPet>("MainGame/Pet/UISelectPet");
        canvas.AddUI(this.UISelectPet);

        this.UISelectPet.Close = this.CloseUISelectPet;
        this.UISelectPet.Select = onSelect;
    }

    /// <summary>
    /// Đóng khung chọn pet
    /// </summary>
    public void CloseUISelectPet()
    {
        if (this.UISelectPet != null)
        {
            GameObject.Destroy(this.UISelectPet.gameObject);
            this.UISelectPet = null;
        }
    }
    #endregion

    #region Mật khẩu cấp 2
    #region Quản lý
    /// <summary>
    /// Khung nhập mật mã cấp 2
    /// </summary>
    public UISecondPassword_Main UISecondPassword_Main { get; protected set; }

    /// <summary>
    /// Mở khung quản lý mật khẩu cấp 2
    /// </summary>
    /// <param name="removeSecLeft"></param>
    public void OpenUISecondPassword_Main(int removeSecLeft)
    {
        if (this.UISecondPassword_Main != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISecondPassword_Main = canvas.LoadUIPrefab<UISecondPassword_Main>("MainGame/SecondPassword/UISecondPassword_Main");
        canvas.AddUI(this.UISecondPassword_Main);

        this.UISecondPassword_Main.Close = this.CloseUISecondPassword_Main;
        this.UISecondPassword_Main.AutoRemoveSecLeft = removeSecLeft;
        this.UISecondPassword_Main.RequestRemove = () =>
        {
            /// Đóng khung
            this.CloseUISecondPassword_Main();
            /// Gửi yêu cầu
            KT_TCPHandler.SendRequestRemoveSecondPassword();
        };
        this.UISecondPassword_Main.CancelRemove = () =>
        {
            /// Đóng khung
            this.CloseUISecondPassword_Main();
            /// Gửi yêu cầu
            KT_TCPHandler.SendCancelRequestRemoveSecondPassword();
        };
        this.UISecondPassword_Main.Change = () =>
        {
            /// Đóng khung
            this.CloseUISecondPassword_Main();
            /// Gửi yêu cầu
            KT_TCPHandler.SendOpenChangeSecondPassword();
        };
    }

    /// <summary>
    /// Đóng khung quản lý mật khẩu cấp 2
    /// </summary>
    public void CloseUISecondPassword_Main()
    {
        if (this.UISecondPassword_Main != null)
        {
            GameObject.Destroy(this.UISecondPassword_Main.gameObject);
            this.UISecondPassword_Main = null;
        }
    }
    #endregion

    #region Thiết lập
    /// <summary>
    /// Khung thiết lập mật mã cấp 2
    /// </summary>
    public UISecondPassword_Set UISecondPassword_Set { get; protected set; }

    /// <summary>
    /// Mở khung thiết lập mật khẩu cấp 2
    /// </summary>
    public void OpenUISecondPassword_Set()
    {
        if (this.UISecondPassword_Set != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISecondPassword_Set = canvas.LoadUIPrefab<UISecondPassword_Set>("MainGame/SecondPassword/UISecondPassword_Set");
        canvas.AddUI(this.UISecondPassword_Set);

        this.UISecondPassword_Set.Close = this.CloseUISecondPassword_Set;
        this.UISecondPassword_Set.Submit = (password, reinputPassword) =>
        {
            /// Đóng khung
            this.CloseUISecondPassword_Set();
            /// Gửi yêu cầu
            KT_TCPHandler.SendSetSecondPassword(password, reinputPassword);
        };
    }

    /// <summary>
    /// Đóng khung thiết lập mật khẩu cấp 2
    /// </summary>
    public void CloseUISecondPassword_Set()
    {
        if (this.UISecondPassword_Set != null)
        {
            GameObject.Destroy(this.UISecondPassword_Set.gameObject);
            this.UISecondPassword_Set = null;
        }
    }
    #endregion

    #region Đổi
    /// <summary>
    /// Khung đổi mật mã cấp 2
    /// </summary>
    public UISecondPassword_Change UISecondPassword_Change { get; protected set; }

    /// <summary>
    /// Mở khung đổi mật khẩu cấp 2
    /// </summary>
    public void OpenUISecondPassword_Change()
    {
        if (this.UISecondPassword_Change != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISecondPassword_Change = canvas.LoadUIPrefab<UISecondPassword_Change>("MainGame/SecondPassword/UISecondPassword_Change");
        canvas.AddUI(this.UISecondPassword_Change);

        this.UISecondPassword_Change.Close = this.CloseUISecondPassword_Change;
        this.UISecondPassword_Change.Submit = (oldPassword, newPassword, reinputNewPassword) =>
        {
            /// Đóng khung
            this.CloseUISecondPassword_Change();
            /// Gửi yêu cầu
            KT_TCPHandler.SendChangeSecondPassword(oldPassword, newPassword, reinputNewPassword);
        };
    }

    /// <summary>
    /// Đóng khung đổi mật khẩu cấp 2
    /// </summary>
    public void CloseUISecondPassword_Change()
    {
        if (this.UISecondPassword_Change != null)
        {
            GameObject.Destroy(this.UISecondPassword_Change.gameObject);
            this.UISecondPassword_Change = null;
        }
    }
    #endregion

    #region Nhập
    /// <summary>
    /// Khung nhập mật mã cấp 2
    /// </summary>
    public UISecondPassword_Input UISecondPassword_Input { get; protected set; }

    /// <summary>
    /// Mở khung nhập mật khẩu cấp 2
    /// </summary>
    /// <param name="removeSecLeft"></param>
    public void OpenUISecondPassword_Input(int removeSecLeft)
    {
        if (this.UISecondPassword_Input != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISecondPassword_Input = canvas.LoadUIPrefab<UISecondPassword_Input>("MainGame/SecondPassword/UISecondPassword_Input");
        canvas.AddUI(this.UISecondPassword_Input);

        this.UISecondPassword_Input.Close = this.CloseUISecondPassword_Input;
        this.UISecondPassword_Input.AutoRemoveSecLeft = removeSecLeft;
        this.UISecondPassword_Input.RequestRemove = () =>
        {
            /// Đóng khung
            this.CloseUISecondPassword_Input();
            /// Gửi yêu cầu
            KT_TCPHandler.SendRequestRemoveSecondPassword();
        };
        this.UISecondPassword_Input.CancelRemove = () =>
        {
            /// Đóng khung
            this.CloseUISecondPassword_Input();
            /// Gửi yêu cầu
            KT_TCPHandler.SendCancelRequestRemoveSecondPassword();
        };
        this.UISecondPassword_Input.Submit = (password) =>
        {
            /// Đóng khung
            this.CloseUISecondPassword_Input();
            /// Gửi yêu cầu
            KT_TCPHandler.SendInputSecondPassword(password);
        };
    }

    /// <summary>
    /// Đóng khung nhập mật khẩu cấp 2
    /// </summary>
    public void CloseUISecondPassword_Input()
    {
        if (this.UISecondPassword_Input != null)
        {
            GameObject.Destroy(this.UISecondPassword_Input.gameObject);
            this.UISecondPassword_Input = null;
        }
    }
    #endregion
    #endregion
}
