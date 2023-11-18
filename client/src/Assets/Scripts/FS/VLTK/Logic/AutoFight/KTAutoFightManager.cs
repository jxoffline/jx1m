using FS.GameEngine.Logic;
using FS.VLTK.Logic.Settings;
using System.Diagnostics;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Luồng thực thi tự đánh
    /// </summary>
    public partial class KTAutoFightManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Luồng thực thi tự đánh
        /// </summary>
        public static KTAutoFightManager Instance { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            KTAutoFightManager.Instance = this;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
		private void Start()
		{
            PlayZone.Instance.StartCoroutine(this.AutoUseFoodAndMedicine());
            PlayZone.Instance.StartCoroutine(this.AutoPet());
		}
        #endregion

        #region Properties
        /// <summary>
        /// Có phải đang tự đánh không
        /// </summary>
        public bool IsAutoFighting { get; private set; }
        #endregion

        #region Public methods
        /// <summary>
        /// Mở Auto
        /// </summary>
        public void StartAuto(bool IsSkipReset = false)
        {
            this.StopAllCoroutines();
            this.StartCoroutine(this.ProcessAutoFight(IsSkipReset));
            if (PlayZone.Instance != null)
            {
                PlayZone.Instance.ShowTextAutoAttack();
            }
            /// Làm rỗng dữ liệu
            this.Clear();
            /// Đánh dấu đang tự đánh
            this.IsAutoFighting = true;

            this.AutoTime = Stopwatch.StartNew();

            PlayZone.Instance.UIMainButtons.ActiveAuto = true;

            /// Ẩn vùng Farm
            Global.Data.GameScene?.RemoveFarmAreaDeco();

          
            if (!KTAutoAttackSetting.Config.EnableAutoPK)
            {
                if (KTAutoAttackSetting.Config.Farm.FarmAround)
                {
                    if(IsSkipReset)
                    {
                        Global.Data.GameScene?.SetFarmAreaDeco(this.StartPos, KTAutoAttackSetting.Config.Farm.ScanRange);
                    }
                    else
                    {
                        Global.Data.GameScene?.SetFarmAreaDeco(Global.Data.Leader.PositionInVector2, KTAutoAttackSetting.Config.Farm.ScanRange);
                    }
                   
                }
            }
        }

        /// <summary>
        /// Dừng tự đánh
        /// </summary>
        public void StopAutoFight()
        {
            this.StopAllCoroutines();
            /// Làm rỗng dữ liệu
            this.Clear();

            if (PlayZone.Instance != null)
            {
                PlayZone.Instance.HideTextAutoAttack();
                PlayZone.Instance.UIMainButtons.ActiveAuto = false;
            }
            /// Bỏ đánh dấu đang tự đánh
            this.IsAutoFighting = false;

            /// Ẩn vùng Farm
            Global.Data.GameScene?.RemoveFarmAreaDeco();
        }
        #endregion
    }
}
