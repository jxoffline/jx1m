using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main.RoleInfo
{
    /// <summary>
    /// Đối tượng Item của Mục danh vọng
    /// </summary>
    public class UIRoleInfo_ReputesTab_ReputeCategory_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên danh vọng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text cấp độ danh vọng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text kinh nghiệm danh vọng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Exp;
        #endregion

        #region Properties
        private ReputeInfo _Data = null;
        /// <summary>
        /// Dữ liệu danh vọng
        /// </summary>
        public ReputeInfo Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Nhóm tương ứng
                ReputeCamp reputeCamp = Loader.Loader.Reputes.Camp.Where(x => x.Id == value.Camp).FirstOrDefault();
                /// Lớp tương ứng
                ReputeClass reputeClass = reputeCamp.Class.Where(x => x.Id == value.Class).FirstOrDefault();
                /// Cấp độ tương ứng
                ReputeLevel reputeLevel = reputeClass.Level.Where(x => x.Id == value.Level).FirstOrDefault();

                this.UIText_Name.text = reputeClass.Name;
                this.UIText_Level.text = value.Level.ToString();

                /// Nếu không tìm thấy thì Exp = 0
                if (reputeLevel == null)
				{
                    this.UIText_Exp.text = "0/0";
				}
				else
				{
                    this.UIText_Exp.text = string.Format("{0}/{1}", value.Exp, reputeLevel.LevelUp);
                }
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }
        #endregion
    }
}
