using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.MainUI;
using Server.Data;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
	#region Quản lý
    /// <summary>
    /// Ẩn tắt cả mặt
    /// </summary>
    public void HideAllFace()
    {
        if (this.UIMonsterFace != null && this.UIMonsterFace.Visible)
        {
            this.UIMonsterFace.Visible = false;
        }

        if (this.UIOtherRoleFace != null && this.UIOtherRoleFace.Visible)
        {
            this.UIOtherRoleFace.Visible = false;
        }

        if (this.UIPetFace != null && this.UIPetFace.Visible)
        {
            this.UIPetFace.Visible = false;
        }
    }

    /// <summary>
    /// Loại đối tượng đang hiển thị trên khung
    /// </summary>
    protected GSpriteTypes objectFaceType;

    /// <summary>
    /// ID đối tượng được hiển thị trên khung
    /// </summary>
    protected int objectFaceRoleID;

    /// <summary>
    /// Hiển thị mặt
    /// </summary>
    /// <param name="type"></param>
    private void SetFaceVisiable(int type)
    {
        this.UIOtherRoleFace.Visible = (type == 0);
        this.UIMonsterFace.Visible = (type == 1 || type == 2);
        this.UIPetFace.Visible = (type == 3);
    }

    /// <summary>
    /// Hiển thị mặt đối tượng
    /// </summary>
    /// <param name="type"></param>
    /// <param name="roleID"></param>
    protected void ShowObjectRoleFace(GSpriteTypes type, int roleID)
    {
        if (null == this.UIOtherRoleFace)
        {
            return;
        }

        this.objectFaceType = type;
        this.objectFaceRoleID = roleID;
        if (this.objectFaceType == GSpriteTypes.Other)
        {
            if (Global.Data.OtherRoles.TryGetValue(this.objectFaceRoleID, out RoleData rd))
            {
                this.HideAllFace();
                this.SetFaceVisiable(0);

                this.UIOtherRoleFace.Name = rd.RoleName;
                this.UIOtherRoleFace.Level = rd.Level;
                this.UIOtherRoleFace.RoleID = rd.RoleID;
                this.UIOtherRoleFace.RoleAvartaID = rd.RolePic;
                this.UIOtherRoleFace.Visible = true;
                this.UIOtherRoleFace.AvartaClickable = true;

                if (rd.MaxHP > 0)
                {
                    this.UIOtherRoleFace.HP = rd.CurrentHP;
                    this.UIOtherRoleFace.HPMax = rd.MaxHP;
                }
            }
        }
        else if (this.objectFaceType == GSpriteTypes.Bot)
        {
            if (Global.Data.Bots.ContainsKey(this.objectFaceRoleID))
            {
                this.HideAllFace();
                this.SetFaceVisiable(0);

                Global.Data.Bots.TryGetValue(this.objectFaceRoleID, out RoleData rd);
                this.UIOtherRoleFace.Name = rd.RoleName;
                this.UIOtherRoleFace.Level = rd.Level;
                this.UIOtherRoleFace.RoleID = rd.RoleID;
                this.UIOtherRoleFace.RoleAvartaID = rd.RolePic;
                this.UIOtherRoleFace.Visible = true;
                this.UIOtherRoleFace.AvartaClickable = false;

                if (rd.MaxHP > 0)
                {
                    this.UIOtherRoleFace.HP = rd.CurrentHP;
                    this.UIOtherRoleFace.HPMax = rd.MaxHP;
                }
            }
        }
        else if (this.objectFaceType == GSpriteTypes.Monster)
        {
            if (Global.Data.SystemMonsters.TryGetValue(this.objectFaceRoleID, out MonsterData md))
            {
                this.HideAllFace();
                if (md == null)
                {
                    return;
                }

                this.UIMonsterFace.RoleID = md.RoleID;
                this.UIMonsterFace.Level = md.Level;
                this.UIMonsterFace.Name = md.RoleName;

                /// Ngũ hành
                this.UIMonsterFace.Elemental = (Elemental) md.Elemental;

                if (md.MaxHP > 0)
                {
                    this.UIMonsterFace.HP = (int) md.HP;
                    this.UIMonsterFace.HPMax = (int) md.MaxHP;
                }
                this.UIMonsterFace.Visible = true;
                this.SetFaceVisiable(1);
            }
        }
        else if (this.objectFaceType == GSpriteTypes.Pet)
        {
            if (Global.Data.SystemPets.TryGetValue(this.objectFaceRoleID, out PetDataMini pd))
            {
                this.HideAllFace();
                if (pd == null)
                {
                    return;
                }

                this.UIPetFace.RoleID = pd.RoleID;
                this.UIPetFace.Level = pd.Level;
                this.UIPetFace.Name = pd.Name;

                if (pd.MaxHP > 0)
                {
                    this.UIPetFace.HP = pd.HP;
                    this.UIPetFace.HPMax = pd.MaxHP;
                }
                this.UIPetFace.Visible = true;
                this.SetFaceVisiable(3);
            }
        }
        else if (this.objectFaceType == GSpriteTypes.TraderCarriage)
        {
            if (Global.Data.TraderCarriages.TryGetValue(this.objectFaceRoleID, out TraderCarriageData td))
            {
                this.HideAllFace();
                if (td == null)
                {
                    return;
                }

                this.UIMonsterFace.RoleID = td.RoleID;
                this.UIMonsterFace.Level = 1;
                this.UIMonsterFace.Name = td.Name;

                /// Ngũ hành
                this.UIMonsterFace.Elemental = Elemental.NONE;

                if (td.MaxHP > 0)
                {
                    this.UIMonsterFace.HP = td.HP;
                    this.UIMonsterFace.HPMax = td.MaxHP;
                }
                this.UIMonsterFace.Visible = true;
                this.SetFaceVisiable(1);
            }
        }
    }
	#endregion

	#region Other Role Face
	/// <summary>
	/// Other Role Face
	/// </summary>
	public UIOtherRoleFace UIOtherRoleFace { get; protected set; }

    /// <summary>
    /// Khởi tạo OtherRoleFace
    /// </summary>
    protected void InitializeObjectRoleFace()
    {
        if (this.UIOtherRoleFace != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIOtherRoleFace = canvas.LoadUIPrefab<UIOtherRoleFace>("MainGame/MainUI/UIOtherRoleFace");
        canvas.AddMainUI(this.UIOtherRoleFace);

        this.UIOtherRoleFace.AvartaClicked = () => {
            this.ShowBrowseOtherRoleInfo(this.UIOtherRoleFace.RoleID);
        };
    }
    #endregion

    #region Monster Face
    /// <summary>
    /// Monster Face
    /// </summary>
    public UIMonsterFace UIMonsterFace { get; protected set; }

    /// <summary>
    /// Khởi tạo Monster Face
    /// </summary>
    protected void InitializeMonsterFace()
    {
        if (this.UIMonsterFace != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIMonsterFace = canvas.LoadUIPrefab<UIMonsterFace>("MainGame/MainUI/UIMonsterFace");
        canvas.AddMainUI(this.UIMonsterFace);
        this.UIMonsterFace.Visible = false;
    }
    #endregion

    #region Pet Face
    /// <summary>
    /// Pet Face
    /// </summary>
    public UIPetFace UIPetFace { get; protected set; }

    /// <summary>
    /// Khởi tạo Pet Face
    /// </summary>
    protected void InitializePetFace()
    {
        if (this.UIPetFace != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIPetFace = canvas.LoadUIPrefab<UIPetFace>("MainGame/MainUI/UIPetFace");
        canvas.AddMainUI(this.UIPetFace);
        this.UIPetFace.Visible = false;
    }
    #endregion

    #region Chọn Avarta
    /// <summary>
    /// Khung chọn Avarta nhân vật
    /// </summary>
    public UISelectAvarta UISelectAvarta { get; protected set; }

    /// <summary>
    /// Hiển thị khung chọn Avarta nhân vật
    /// </summary>
    public void ShowUISelectAvarta()
    {
        /// Nếu khung đang hiển thị
        if (this.UISelectAvarta != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISelectAvarta = canvas.LoadUIPrefab<UISelectAvarta>("MainGame/UISelectAvarta");
        canvas.AddUI(this.UISelectAvarta);

        this.UISelectAvarta.Close = this.CloseUISelectAvarta;
        this.UISelectAvarta.AvartaSelected = (avartaID) => {
            /// Gửi gói tin thông báo thay đổi Avarta nhân vật
            KT_TCPHandler.SendRoleAvartaChanged(avartaID);
            /// Đóng khung chọn Avarta
            this.CloseUISelectAvarta();
        };
    }

    /// <summary>
    /// Đóng khung chọn Avarta nhân vật
    /// </summary>
    private void CloseUISelectAvarta()
    {
        /// Nếu khung đang hiển thị
        if (this.UISelectAvarta != null)
        {
            GameObject.Destroy(this.UISelectAvarta.gameObject);
            this.UISelectAvarta = null;
        }
    }
    #endregion
}
