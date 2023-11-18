using FS.GameEngine.Sprite;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Định nghĩa
    /// </summary>
    public partial class KTAutoPetManager
    {
        /// <summary>
        /// Khoảng cách tối đa so với chủ nhân
        /// </summary>
        private const int MaxDistanceToOwner = 800;

        /// <summary>
        /// Khoảng cách tối đa so với chủ nhân để pet quyết định chạy theo hay dùng kỹ năng tiếp
        /// </summary>
        private const int ContinueWorkingOnDistanceToOwner = 300;

        /// <summary>
        /// Khoảng cách tối đa đứng xung quanh chủ nhân
        /// </summary>
        private const int IdleRadiusNearByOwner = 100;

        private GSprite _Pet;
        /// <summary>
        /// Thông tin Pet
        /// </summary>
        public GSprite Pet
        {
            get
            {
                return this._Pet;
            }
            set
            {
                this._Pet = value;
                /// Hủy mục tiêu
                this.Target = null;
                /// ID kỹ năng dùng lần trước
                this.LastUseSkillID = -1;
                /// Thời điểm dùng kỹ năng lần trước
                this.LastUseSkillTick = 0;
                /// Thời điểm dùng kỹ năng không ảnh hưởng tốc đánh lần trước
                this.LastUseSkillNoAffectAtkSpeedTick = 0;
                /// Đang đợi dùng kỹ năng
                this.IsWaitingToUseSkill = false;
                /// Ngừng luồng đuổi mục tiêu
                this.StopChaseTarget();
                /// Toác
                if (value == null || value.PetData == null)
                {
                    this.Owner = null;
                    this.petUsedSkillTicks.Clear();
                    return;
                }
                else
                {
                    /// Tìm chủ nhân tương ứng
                    this.Owner = KTGlobal.FindSpriteByID(this._Pet.PetData.RoleID);
                }
            }
        }

        /// <summary>
        /// Chủ nhân của Pet
        /// </summary>
        private GSprite Owner = null;

        /// <summary>
        /// Xóa dữ liệu Auto Pet
        /// </summary>
        public void Clear()
        {
            /// Xóa thông tin Pet
            this.Pet = null;
            /// Xóa thông tin chủ nhân
            this.Owner = null;
            /// Hủy mục tiêu
            this.Target = null;
            /// ID kỹ năng dùng lần trước
            this.LastUseSkillID = -1;
            /// Thời điểm dùng kỹ năng lần trước
            this.LastUseSkillTick = 0;
            /// Thời điểm dùng kỹ năng không ảnh hưởng tốc đánh lần trước
            this.LastUseSkillNoAffectAtkSpeedTick = 0;
            /// Đang đợi dùng kỹ năng
            this.IsWaitingToUseSkill = false;
            /// Ngừng luồng đuổi mục tiêu
            this.StopChaseTarget();
            /// Ngừng toàn bộ luồng
            this.StopAllCoroutines();
        }

        /// <summary>
        /// Khởi động lại Auto
        /// </summary>
        public void Restart()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }
            /// Khởi động luồng đồng bộ vị trí lên GS
            this.StartCoroutine(this.SyncPosToGS());
            /// Khởi động luồng thực thi Logic
            this.StartCoroutine(this.DoLogic());
        }
    }
}
