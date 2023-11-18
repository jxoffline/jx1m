using FS.GameEngine.Logic;
using FS.VLTK.Factory;
using FS.VLTK.UI;
using FS.VLTK.UI.CoreUI;
using System;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Đối tượng điểm truyền tống
    /// </summary>
    public partial class Teleport : IDisplayUI
    {
        #region Define
        /// <summary>
        /// Khung trên đầu nhân vật
        /// </summary>
        [SerializeField]
        private UIMonsterHeader UIHeader;

        /// <summary>
        /// Khung biểu diễn trên Minimap
        /// </summary>
        private UIMinimapReference UIMinimapReference;
        #endregion

        #region Kế thừa IDisplayUI
        /// <summary>
        /// Hiển thị UI
        /// </summary>
        public void DisplayUI()
        {
            {
                this.UIHeader.ShowHPBar = false;
                this.UIHeader.ShowElemental = false;
                this.UIHeader.Name = this.Data.Name;
                ColorUtility.TryParseHtmlString("#60ff38", out Color color);
                this.UIHeader.NameColor = color;
                this.UIHeader.Title = "";
            }
            

            if (this.UIMinimapReference == null)
            {
                this.UIMinimapReference = KTObjectPoolManager.Instance.Instantiate<UIMinimapReference>("Minimap - Reference");
                this.UIMinimapReference.ReferenceObject = this.gameObject;
                this.UIMinimapReference.gameObject.SetActive(true);
            }
            if (this.UIMinimapReference != null)
            {
                this.UIMinimapReference.ShowIcon = true;
                this.UIMinimapReference.ShowName = true;
                this.UIMinimapReference.Name = this.Data.Name;
                ColorUtility.TryParseHtmlString("#60ff38", out Color color);
                this.UIMinimapReference.NameColor = color;
                this.UIMinimapReference.BundleDir = KTGlobal.MinimapIconBundleDir;
                this.UIMinimapReference.AtlasName = KTGlobal.MinimapIconAtlasName;
                this.UIMinimapReference.SpriteName = KTGlobal.MinimapTeleportIconSpriteName;
                this.UIMinimapReference.IconSize = this.MinimapIconSize;
                this.UIMinimapReference.UpdatePosition();
            }
        }

        /// <summary>
        /// Xóa UI
        /// </summary>
        public void DestroyUI()
        {
            this.UIHeader.Destroy();

            if (this.UIMinimapReference != null)
            {
                this.UIMinimapReference.Destroy();
                this.UIMinimapReference = null;
            }
        }
        #endregion
    }

}