using System.Collections.Generic;
using Server.Data;
using FS.Drawing;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Scene;
using System;
using UnityEngine;
using FS.GameFramework.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Loader;
using FS.VLTK.Entities.Config;

namespace FS.GameEngine.Data
{
    /// <summary>
    /// Dữ liệu của Game
    /// </summary>
    public class GData
    {
        #region Bản đồ

        /// <summary>
        /// Bản đồ hiện tại
        /// </summary>
        public GScene GameScene { get; set; } = null;

        /// <summary>
        /// Nhân vật hiện tại
        /// </summary>
        public GSprite Leader
        {
            get
            {
                if (this.GameScene == null)
                {
                    return null;
                }
                return this.GameScene.GetLeader();
            }
        }

        /// <summary>
        /// Đang đợi chuyển bản đồ
        /// </summary>
        public bool WaitingForMapChange { get; set; } = false;

        #endregion

        #region Tải
        /// <summary>
        /// Tên bản đồ đang thực hiện tải xuống
        /// </summary>
        public string CurrentDownloadingMapName { get; set; } = "";

        /// <summary>
        /// Tiến độ đang tải bản đồ hiện tại
        /// </summary>
        public int CurrentDownloadingMapProgress { get; set; } = 0;
        #endregion

        #region Người chơi

        /// <summary>
        /// ID người chơi
        /// </summary>
        public string UserID
        {
            get { return GameInstance.Game.CurrentSession.UserID; }
        }

        /// <summary>
        /// Đánh dấu đã vào Game chưa
        /// </summary>
        public bool PlayGame
        {
            get { return GameInstance.Game.CurrentSession.PlayGame; }
        }

        /// <summary>
        /// Dữ liệu nhân vật
        /// </summary>
        public RoleData RoleData
        {
            get { return GameInstance.Game.CurrentSession.roleData; }
        }

        /// <summary>
        /// ID máy chủ
        /// </summary>
        public int GameServerID
        {
            get 
            {
                int serverID = 1;
                int lastServerId = PlayerPrefs.GetInt("NewLastServerInfoID");
                if (lastServerId != 0)
                {
                    serverID = lastServerId;
                }
                return serverID;
            }  
        }
        #endregion

        #region Thông tin liên máy chủ

        /// <summary>
        /// Thông tin máy chủ
        /// </summary>
        public XuanFuServerData ServerData { get; set; }

        #endregion

        #region Tương quan nhiệm vụ
        /// <summary>
        /// Danh sách nhiệm vụ đã hoàn thành
        /// </summary>
        public Dictionary<int, HashSet<int>> CompletedTasks { get; } = new Dictionary<int, HashSet<int>>();

        /// <summary>
        /// Xây danh sách nhiệm vụ đã hoàn thành
        /// </summary>
        public void BuildCompletedTasks()
        {
            /// Làm rỗng danh sách nhiệm vụ đã hoàn thành
            this.CompletedTasks.Clear();

            /// Duyệt danh sách nhiệm vụ đã hoàn thành
            foreach (QuestInfo info in Global.Data.RoleData.QuestInfo)
            {
                /// Nếu chưa tồn tại thì tạo mới danh sách
                if (!this.CompletedTasks.ContainsKey(info.TaskClass))
                {
                    this.CompletedTasks[info.TaskClass] = new HashSet<int>();
                }

                /// ID nhiệm vụ hoàn thành lần cuối
                int lastCompletedTaskID = info.CurTaskIndex;

                /// ID nhiệm vụ đang kiểm tra hiện tại
                int currentTaskID = lastCompletedTaskID;
                do
                {
                    /// Nếu nhiệm vụ không tồn tại
                    if (!Loader.Tasks.TryGetValue(currentTaskID, out TaskDataXML taskDataXML))
                    {
                        break;
                    }

                    /// Danh mục nhiệm vụ
                    int category = taskDataXML.TaskClass;

                    /// Toác
                    if (!this.CompletedTasks.ContainsKey(category))
                    {
                        this.CompletedTasks[category] = new HashSet<int>();
                    }

                    /// Thên nhiệm vụ đã hoàn thành vào danh sách hiện tại
                    this.CompletedTasks[category].Add(currentTaskID);

                    /// Nhiệm vụ trước đó
                    int prevTaskID = taskDataXML.PrevTask;

                    /// Tiếp tục kiểm tra nhiệm vụ sau đó
                    currentTaskID = prevTaskID;
                }
                while (true);
            }
        }
        #endregion

        #region Chức năng
        /// <summary>
        /// ID pet lần trước
        /// </summary>
        public int LastPetID { get; set; } = -1;

        /// <summary>
        /// Danh sách lời mời vào bang hội
        /// </summary>
        public List<RoleDataMini> GuildInvitationPlayers { get; set; } = new List<RoleDataMini>();

        /// <summary>
        /// Danh sách người chơi xin vào bang hội
        /// </summary>
        public List<RoleDataMini> GuildRequestJoinPlayers { get; set; } = new List<RoleDataMini>();

        /// <summary>
        /// Danh sách người chơi mời bản thân vào nhóm
        /// </summary>
        public List<RoleDataMini> InvitedToTeamPlayers { get; set; } = new List<RoleDataMini>();

        /// <summary>
        /// Danh sách thành viên trong nhóm
        /// </summary>
        public List<RoleDataMini> Teammates { get; set; } = new List<RoleDataMini>();

        /// <summary>
        /// Danh sách người chơi khác theo ID
        /// </summary>
        public Dictionary<int, RoleData> OtherRoles { get; set; } = new Dictionary<int, RoleData>();

        /// <summary>
        /// Danh sách BOT theo ID
        /// </summary>
        public Dictionary<int, RoleData> Bots { get; set; } = new Dictionary<int, RoleData>();

        /// <summary>
        /// Danh sách Bot bán hàng theo ID
        /// </summary>
        public Dictionary<int, StallBotData> StallBots { get; set; } = new Dictionary<int, StallBotData>();

        /// <summary>
        /// Danh sách người chơi khác theo tên
        /// </summary>
        public Dictionary<string, RoleData> OtherRolesByName { get; set; } = new Dictionary<string, RoleData>();

        /// <summary>
        /// Danh sách quái theo ID
        /// </summary>
        public Dictionary<int, MonsterData> SystemMonsters { get; set; } = new Dictionary<int, MonsterData>();

        /// <summary>
        /// Danh sách Pet theo ID
        /// </summary>
        public Dictionary<int, PetDataMini> SystemPets { get; set; } = new Dictionary<int, PetDataMini>();

        /// <summary>
        /// Danh sách xe tiêu theo ID
        /// </summary>
        public Dictionary<int, TraderCarriageData> TraderCarriages { get; set; } = new Dictionary<int, TraderCarriageData>();

        /// <summary>
        /// ID mục tiêu được chọn
        /// </summary>
        public int TargetNpcID { get; set; } = -1;

        /// <summary>
        /// Đang hiển thị Set dự phòng
        /// </summary>
        public bool ShowReserveEquip { get; set; } = false;

        /// <summary>
        /// Danh sách vật phẩm trong thương khố
        /// </summary>
        public List<GoodsData> PortableGoodsDataList { get; set; } = null;

        /// <summary>
        /// Danh sách thư
        /// </summary>
        public List<MailData> MailDataList { get; set; } = null;

        /// <summary>
        /// Danh sách bạn bè theo loại
        /// </summary>
        public List<FriendData> FriendDataList { get; set; } = null;

        /// <summary>
        /// Danh sách người chơi đang chờ thêm bạn
        /// </summary>
        public List<RoleDataMini> AskToBeFriendList { get; } = new List<RoleDataMini>();

        /// <summary>
        /// ID phiên giao dịch
        /// </summary>
        public int ExchangeID { get; set; } = -1;

        /// <summary>
        /// Dữ liệu vật phẩm và tiền tệ giao dịch
        /// </summary>
        public ExchangeData ExchangeDataItem { get; set; } = null;

        /// <summary>
        /// Dữ liệu gian hàng của bản thân
        /// </summary>
        public StallData StallDataItem { get; set; } = null;
        #endregion

        #region Tương quan PK
        /// <summary>
        /// Khóa di chuyển bằng JoyStick
        /// <para>Khi chức năng này được kích hoạt thì khi dùng JoyStick sẽ chỉ đổi hướng nhân vật về hướng tương ứng</para>
        /// </summary>
        public bool LockJoyStickMove { get; set; } = false;
        #endregion

        #region Tương quan sự kiện và hoạt động
        /// <summary>
        /// Đánh dấu có đang hiện khung Mini hoạt động
        /// </summary>
        public bool ShowUIMiniEventBroadboard { get; set; } = false;
        #endregion

        /// <summary>
        /// Làm rỗng dữ liệu
        /// </summary>
        public void Clear()
        {
            /// Bản đồ
            this.GameScene?.Destroy();
            this.GameScene = null;
            /// Tải
            this.CurrentDownloadingMapName = "";
            this.CurrentDownloadingMapProgress = 0;
            /// Liên máy chủ
            this.ServerData = null;
            /// Danh sách nhiệm vụ đã hoàn thành
            this.CompletedTasks.Clear();
            /// Chức năng
            this.GuildInvitationPlayers.Clear();
            this.GuildRequestJoinPlayers.Clear();
            this.InvitedToTeamPlayers.Clear();
            this.Teammates.Clear();
            this.OtherRoles.Clear();
            this.Bots.Clear();
            this.StallBots.Clear();
            this.OtherRolesByName.Clear();
            this.SystemMonsters.Clear();
            this.SystemPets.Clear();
            this.TargetNpcID = -1;
            this.ShowReserveEquip = false;
            this.PortableGoodsDataList?.Clear();
            this.MailDataList?.Clear();
            this.FriendDataList?.Clear();
            this.AskToBeFriendList?.Clear();
            this.ExchangeID = -1;
            this.ExchangeDataItem = null;
            this.StallDataItem = null;
            /// PK
            this.LockJoyStickMove = false;
            /// Sự kiện và hoạt động
            this.ShowUIMiniEventBroadboard = false;
        }
    }
}
