using FS.GameEngine.Logic;
using FS.VLTK.Factory;
using FS.VLTK.UI;
using FS.VLTK.UI.CoreUI;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    public partial class Monster : IDisplayUI
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

        /// <summary>
        /// Trạng thái nhiệm vụ chờ thêm vào
        /// </summary>
        private NPCTaskStates waitingAddTaskState;
        #endregion

        #region Kế thừa IDisplayUI
        /// <summary>
        /// Hiện UI
        /// </summary>
        public void DisplayUI()
        {
            if (this.UIMinimapReference == null)
            {
                this.UIMinimapReference = KTObjectPoolManager.Instance.Instantiate<UIMinimapReference>("Minimap - Reference");
                this.UIMinimapReference.ReferenceObject = this.gameObject;
                this.UIMinimapReference.gameObject.SetActive(true);
            }

            if (this.RefObject != null)
            {
                {
                    this.UIHeader.ShowHPBar = this._ShowHPBar;
                    this.UIHeader.ShowElemental = this._ShowElemental;
                    this.UIHeader.Name = this.RefObject.RoleName;
                    this.UIHeader.NameColor = this._NameColor;
                    this.UIHeader.Title = this.RefObject.Title;
                    this.UIHeader.Series = this.RefObject.Elemental;
                    this.UIHeader.IsBoss = this.RefObject.MonsterData?.MonsterType != (int)MonsterTypes.Normal && this.RefObject.MonsterData?.MonsterType != (int)MonsterTypes.Hater && this.RefObject.MonsterData?.MonsterType != (int)MonsterTypes.Special_Normal && this.RefObject.MonsterData?.MonsterType != (int)MonsterTypes.Static && this.RefObject.MonsterData?.MonsterType != (int)MonsterTypes.Static_ImmuneAll && this.RefObject.MonsterData?.MonsterType != (int)MonsterTypes.DynamicNPC && this.RefObject.PetData == null && this.RefObject.TraderCarriageData == null;
                }

                if (this.UIMinimapReference != null)
                {
                    this.UIMinimapReference.ShowIcon = this._ShowMinimapIcon;
                    this.UIMinimapReference.ShowName = this._ShowMinimapName;
                    this.UIMinimapReference.Name = this.RefObject.RoleName;
                    this.UIMinimapReference.NameColor = this._MinimapNameColor;
                    this.UIMinimapReference.BundleDir = KTGlobal.MinimapIconBundleDir;
                    this.UIMinimapReference.AtlasName = KTGlobal.MinimapIconAtlasName;
                    this.UIMinimapReference.SpriteName = this.RefObject.SpriteType == GSpriteTypes.NPC || this.RefObject.MonsterData?.MonsterType == (int)MonsterTypes.DynamicNPC ? KTGlobal.MinimapNPCIconSpriteName : this.RefObject.SpriteType == GSpriteTypes.Monster ? KTGlobal.MinimapMonsterIconSpriteName : this.RefObject.SpriteType == GSpriteTypes.GrowPoint ? KTGlobal.MinimapGrowPointIconSpriteName : this.RefObject.SpriteType == GSpriteTypes.TraderCarriage ? KTGlobal.MinimapTraderCarriageIconSpriteName : "";
                    this.UIMinimapReference.IconSize = this._MinimapIconSize;
                    this.UIMinimapReference.UpdatePosition();
                    this.UIMinimapReference.NPCTaskState = this.waitingAddTaskState;
                }
            }

            this.UpdateHP();
            this.UpdateTitle();
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
            }
        }
        #endregion

        #region Update changes
        /// <summary>
        /// Cập nhật màu tên và thanh máu căn cứ trạng thái PK
        /// </summary>
        public void UpdateUIHeaderColor()
        {
            /// Nếu không phải Pet hoặc xe tiêu thì thôi
            if (this.RefObject.SpriteType != GSpriteTypes.Pet && this.RefObject.SpriteType != GSpriteTypes.TraderCarriage)
            {
                return;
            }

            /// Nếu mục tiêu tiềm ẩn nguy hiểm
            if (KTGlobal.IsDangerous(this.RefObject))
            {
                this.UIHeader.NameColor = KTGlobal.DangerousPlayerNameColor;
                this.UIHeader.HPBarColor = KTGlobal.DangerousPlayerNameColor;
            }
            /// Nếu là kẻ địch
            else if (KTGlobal.IsEnemy(this.RefObject))
            {
                this.UIHeader.NameColor = KTGlobal.EnemyPlayerNameColor;
                this.UIHeader.HPBarColor = KTGlobal.EnemyPlayerNameColor;

                if (this.UIMinimapReference.SpriteName != KTGlobal.MinimapEnemyRoleIconSpriteName)
                {
                    this.UIMinimapReference.SpriteName = KTGlobal.MinimapEnemyRoleIconSpriteName;
                }

            }
            /// Nếu là đồng đội
            else if (KTGlobal.IsTeammate(this.RefObject))
            {
                this.UIHeader.RestoreColor();

                if (this.UIMinimapReference.SpriteName != KTGlobal.MinimapTeammateRoleIconSpriteName)
                {
                    this.UIMinimapReference.SpriteName = KTGlobal.MinimapTeammateRoleIconSpriteName;
                }
            }
            /// Nếu không phải kẻ địch
            else
            {
                this.UIHeader.RestoreColor();

                if (this.UIMinimapReference.SpriteName != KTGlobal.MinimapOtherRoleIconSpriteName)
                {
                    this.UIMinimapReference.SpriteName = KTGlobal.MinimapOtherRoleIconSpriteName;
                }
            }
        }

        /// <summary>
        /// Cập nhật máu
        /// </summary>
        /// <param name="hp"></param>
        /// <param name="maxHP"></param>
        public void UpdateHP()
        {
            if (this.RefObject.HPMax != 0)
            {
                /// Để ép kiểu thực hiện phép chia trước không nó toác máu do tràn số
                this.UIHeader.HPPercent = (int) ((this.RefObject.HP / (float) this.RefObject.HPMax) * 100);
            }
        }

        /// <summary>
        /// Cập nhật tên đối tượng
        /// </summary>
        public void UpdateName()
        {
            this.UIHeader.Name = this.RefObject.RoleName;
        }

        /// <summary>
        /// Cập nhật danh hiệu đối tượng
        /// </summary>
        public void UpdateTitle()
        {
            this.UIHeader.Title = this.RefObject.Title;
        }

        /// <summary>
        /// Cập nhật hiển thị trạng thái nhiệm vụ của NPC
        /// </summary>
        /// <param name="taskState"></param>
        public void UpdateMinimapNPCTaskState(NPCTaskStates taskState)
        {
            /// Lưu trạng thái nhiệm vụ chờ thêm vào
            this.waitingAddTaskState = taskState;
            /// Chưa tạo ra thì thôi
            if (this.UIMinimapReference == null)
            {
                return;
            }
            /// Cập nhật trạng thái
            this.UIMinimapReference.NPCTaskState = taskState;
        }
        #endregion
    }
}
