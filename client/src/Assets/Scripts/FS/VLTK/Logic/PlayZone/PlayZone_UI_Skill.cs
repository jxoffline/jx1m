using FS.GameEngine.Logic;
using FS.VLTK.Network.Skill;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.SkillQuickKeyChooser;
using FS.VLTK.UI.Main.SkillTree;
using Server.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Skill Quick Key Chooser
    /// <summary>
    /// Khung chọn kỹ năng đặt vào ô kỹ năng đánh tay
    /// </summary>
    public UISkillQuickKeyChooser UISkillQuickKeyChooser { get; protected set; }

    /// <summary>
    /// Hiện khung chọn kỹ năng đặt vào ô kỹ năng đánh tay
    /// </summary>
    public void ShowUIQuickKeyChooser(int type, Action<int> skillClick)
    {
        if (this.UISkillQuickKeyChooser != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISkillQuickKeyChooser = canvas.LoadUIPrefab<UISkillQuickKeyChooser>("MainGame/UISkillQuickKeyChooser");
        canvas.AddUI(this.UISkillQuickKeyChooser);

        this.UISkillQuickKeyChooser.Type = type;
        this.UISkillQuickKeyChooser.SkillClick = (skillID) => {
            this.CloseUIQuickKeyChooser();
            skillClick?.Invoke(skillID);
        };
        this.UISkillQuickKeyChooser.Close = () => {
            this.CloseUIQuickKeyChooser();
        };
    }

    /// <summary>
    /// Đóng khung chọn kỹ năng đặt vào ô kỹ năng đánh tay
    /// </summary>
    public void CloseUIQuickKeyChooser()
    {
        if (this.UISkillQuickKeyChooser != null)
        {
            GameObject.Destroy(this.UISkillQuickKeyChooser.gameObject);
            this.UISkillQuickKeyChooser = null;
        }
    }
    #endregion

    #region Skill Tree
    /// <summary>
    /// Khung kỹ năng nhân vật
    /// </summary>
    public UISkillTree UISkillTree { get; protected set; }

    /// <summary>
    /// Hiển thị khung kỹ năng nhân vật
    /// </summary>
    public void ShowUISkillTree()
    {
        if (this.UISkillTree == null)
        {
            this.UISkillTree = CanvasManager.Instance.LoadUIPrefab<UISkillTree>("MainGame/UISkillTree");
            CanvasManager.Instance.AddUI(this.UISkillTree);
        }
        this.UISkillTree.Accept = (addedSkills) => {
            KTTCPSkillManager.SendDistributeSkillPoints(addedSkills, this.UISkillTree.SelectedSubID);
        };
        this.UISkillTree.Close = () => {
            this.CloseUISkillTree();
        };
        this.UISkillTree.SetHandSkill = () => {
            this.OpenUISetHandSkill();
        };
        this.UISkillTree.SetExpSkill = () =>
        {
            KTTCPSkillManager.SendGetExpSkillList();
        };
        this.UISkillTree.RefreshSkillData();
    }

    /// <summary>
    /// Đóng khung kỹ năng nhân vật
    /// </summary>
    public void CloseUISkillTree()
    {
        if (this.UISkillTree != null)
        {
            GameObject.Destroy(this.UISkillTree.gameObject);
            this.UISkillTree = null;
        }
    }
    #endregion

    #region Thiết lập kỹ năng tay
    /// <summary>
    /// Khung thiết lập kỹ năng tay
    /// </summary>
    public UISetHandSkill UISetHandSkill { get; protected set; }

    /// <summary>
    /// Mở khung thiết lập kỹ năng tay
    /// </summary>
    public void OpenUISetHandSkill()
    {
        if (this.UISetHandSkill != null)
        {
            return;
        }

        this.UISetHandSkill = CanvasManager.Instance.LoadUIPrefab<UISetHandSkill>("MainGame/UISetHandSkill");
        CanvasManager.Instance.AddUI(this.UISetHandSkill);
        this.UISetHandSkill.Close = this.CloseUISetHandSkill;
    }

    /// <summary>
    /// Đóng khung thiết lập kỹ năng tay
    /// </summary>
    public void CloseUISetHandSkill()
    {
        if (this.UISetHandSkill != null)
        {
            GameObject.Destroy(this.UISetHandSkill.gameObject);
            this.UISetHandSkill = null;
        }
    }
    #endregion

    #region Thiết lập kỹ năng tu luyện
    /// <summary>
    /// Khung thiết lập kỹ năng tu luyện
    /// </summary>
    public UISetExpSkill UISetExpSkill { get; protected set; }

    /// <summary>
    /// Mở khung thiết lập kỹ năng tu luyện
    /// </summary>
    /// <param name="data"></param>
    public void OpenUISetExpSkill(List<ExpSkillData> data)
    {
        if (this.UISetExpSkill != null)
        {
            return;
        }

        this.UISetExpSkill = CanvasManager.Instance.LoadUIPrefab<UISetExpSkill>("MainGame/UISetExpSkill");
        CanvasManager.Instance.AddUI(this.UISetExpSkill);
        this.UISetExpSkill.Close = this.CloseUISetExpSkill;
        this.UISetExpSkill.Data = data;
        this.UISetExpSkill.SetAsCurrentExpSkill = (skillID) =>
        {
            /// Gửi yêu cầu
            KTTCPSkillManager.SendSetExpSkill(skillID);
        };
    }

    /// <summary>
    /// Đóng khung thiết lập kỹ năng tu luyện
    /// </summary>
    public void CloseUISetExpSkill()
    {
        if (this.UISetExpSkill != null)
        {
            GameObject.Destroy(this.UISetExpSkill.gameObject);
            this.UISetExpSkill = null;
        }
    }
    #endregion
}
