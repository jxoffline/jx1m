using System;
using UnityEngine;
using TMPro;
using FS.VLTK.Entities.Config;
using FS.VLTK.Logic;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.ActivityList
{
    /// <summary>
    /// Thông tin hoạt động trong khung danh sách hoạt động
    /// </summary>
    public class UIActivityList_ActivityInfo : MonoBehaviour
    {
        #region Define
        [SerializeField]
        private UIToggleSprite UIToggle;

        /// <summary>
        /// Text tên hoạt động
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ActivityName;

        /// <summary>
        /// Text thời gian hoạt động
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ActivityTime;

        /// <summary>
        /// Button tham gia
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Join;

        /// <summary>
        /// Mark chưa mở
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_Unopened;

        /// <summary>
        /// Mark đã kết thúc
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_Ended;

        /// <summary>
        /// Mark đang diễn ra
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_Opening;
        #endregion

        #region Properties
        private ActivityXML _Data;
        /// <summary>
        /// Thông tin hoạt động
        /// </summary>
        public ActivityXML Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Làm mới hiển thị
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện chọn đối tượng
        /// </summary>
        public Action Select { get; set; }
        
        /// <summary>
        /// Kích hoạt đối tượng
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIToggle.Active;
            }
            set
            {
                this.UIToggle.Active = value;
            }
        }
        #endregion

        #region Core MonoBahaviour
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
            this.UIButton_Join.onClick.AddListener(this.ButtonJoin_Clicked);
            this.UIToggle.OnSelected = (isSelected) =>
            {
                if (!isSelected)
                {
                    return;
                }
                this.Select?.Invoke();
            };
        }

        /// <summary>
        /// Sự kiện khi Button tham gia được ấn
        /// </summary>
        private void ButtonJoin_Clicked()
        {
            /// Nếu có Tips thì hiện Tips
            if (!string.IsNullOrEmpty(this._Data.MsgTips))
            {
                KTGlobal.AddNotification(this._Data.MsgTips);
            }
            /// Nếu không có Tips thì cho chạy đến NPC
            else
            {
                /// Tự dịch chuyển đến NPC tương ứng
                KTGlobal.QuestAutoFindPathToNPC(this._Data.MapCode, this._Data.NPCID, () => {
                    AutoQuest.Instance.StopAutoQuest();
                    AutoPathManager.Instance.StopAutoPath();
                    GSprite sprite = KTGlobal.FindNearestNPCByResID(this._Data.NPCID);
                    if (sprite == null)
                    {
                        KTGlobal.AddNotification("Không tìm thấy NPC tương ứng!");
                        return;
                    }
                    Global.Data.TargetNpcID = sprite.RoleID;
                    Global.Data.GameScene.NPCClick(sprite);
                });
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới
        /// </summary>
        private void RefreshData()
        {
            /// Làm rỗng tên hoạt động
            this.UIText_ActivityName.text = "";
            /// Làm rỗng thời gian
            this.UIText_ActivityTime.text = "";
            /// Hủy trạng thái toàn bộ Button
            this.UIButton_Join.gameObject.SetActive(false);
            this.UIMark_Ended.gameObject.SetActive(false);
            this.UIMark_Unopened.gameObject.SetActive(false);
            this.UIMark_Opening.gameObject.SetActive(false);

            /// Nếu không có dữ liệu
            if (this._Data == null)
            {
                return;
            }

            /// Tên hoạt động
            this.UIText_ActivityName.text = this._Data.ActivityName;
            /// Thời gian diễn ra
            this.UIText_ActivityTime.text = this._Data.GetNearestActivityTimes();
            /// Trạng thái hoạt động
            ActivityXML.ActivityState state = this._Data.GetActivityState();
            /// Xem trạng thái gì để cập nhật hiển thị
            switch (state)
            {
                case ActivityXML.ActivityState.NOTOPEN:
                {
                    this.UIButton_Join.gameObject.SetActive(false);
                    this.UIMark_Ended.gameObject.SetActive(false);
                    this.UIMark_Unopened.gameObject.SetActive(true);
                    this.UIMark_Opening.gameObject.SetActive(false);
                    break;
                }
                case ActivityXML.ActivityState.OPEN:
                {
                    this.UIButton_Join.gameObject.SetActive(false);
                    this.UIMark_Ended.gameObject.SetActive(false);
                    this.UIMark_Unopened.gameObject.SetActive(false);
                    this.UIMark_Opening.gameObject.SetActive(true);
                    break;
                }
                case ActivityXML.ActivityState.CANJOIN:
                {
                    this.UIButton_Join.gameObject.SetActive(true);
                    this.UIMark_Ended.gameObject.SetActive(false);
                    this.UIMark_Unopened.gameObject.SetActive(false);
                    this.UIMark_Opening.gameObject.SetActive(false);
                    break;
                }
                case ActivityXML.ActivityState.HASEND:
                {
                    this.UIButton_Join.gameObject.SetActive(false);
                    this.UIMark_Ended.gameObject.SetActive(true);
                    this.UIMark_Unopened.gameObject.SetActive(false);
                    this.UIMark_Opening.gameObject.SetActive(false);
                    break;
                }
            }
        }
        #endregion
    }
}
