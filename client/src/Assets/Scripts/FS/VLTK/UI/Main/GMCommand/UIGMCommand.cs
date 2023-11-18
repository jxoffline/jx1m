using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.GMCommand;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung nhập lệnh GM
    /// </summary>
    public class UIGMCommand : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Input nhập lệnh
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_Command;

        /// <summary>
        /// Button thực thi
        /// </summary>
        [SerializeField]
        private Button UIButton_Process;

        /// <summary>
        /// Prefab mô tả
        /// </summary>
        [SerializeField]
        private UIGMCommand_Item UI_DescriptionPrefab;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private Button UIButton_Close;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện thực thi lệnh GM
        /// </summary>
        public Action<string> ProcessGMCommand { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Đối tượng có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách ghi chú lệnh
        /// </summary>
        private readonly List<KeyValuePair<string, string>> commands = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("ReloadServerConfig", "Tải lại thiết lập hệ thống mới nhất."),
            new KeyValuePair<string, string>("ReloadGMList", "Tải lại danh sách GM mới nhất."),

            new KeyValuePair<string, string>("Invisiblity <color=#fff133>state</color>", "Trạng thái tàng hình cho bản thân (0 hoặc 1)."),
            new KeyValuePair<string, string>("Immortality <color=#fff133>state</color>", "Trạng thái bất tử cho bản thân (0 hoặc 1)."),

            new KeyValuePair<string, string>("CheckCcu", "Kiểm tra tổng số người chơi đang Online."),

            new KeyValuePair<string, string>("ClearBag", "Xóa rỗng túi đồ của bản thân."),

            new KeyValuePair<string, string>("LoadLua <color=#fff133>scriptID</color>", "Tải mới Script Lua ID tương ứng trong hệ thống."),

            new KeyValuePair<string, string>("GenerateCaptcha <color=#fff133>targetID</color>", "Tạo Captcha kiểm tra Bot cho bản thân (bỏ trống <color=#fff133>targetID</color>), hoặc cho người chơi có ID tương ứng."),
            
            new KeyValuePair<string, string>("Heal <color=#fff133>targetID</color>", "Trị liệu cho bản thân (bỏ trống <color=#fff133>targetID</color>), hoặc trị liệu cho người chơi có ID tương ứng."),
            
            new KeyValuePair<string, string>("PlayerInfo <color=#fff133>targetID</color>", "Trả về thông tin người chơi ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            
            new KeyValuePair<string, string>("ShowSecondPassword <color=#fff133>targetID</color>", "Hiện khóa an toàn của người chơi ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            new KeyValuePair<string, string>("ClearSecondPassword <color=#fff133>targetID</color>", "Xóa khóa an toàn của người chơi ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),

            new KeyValuePair<string, string>("BanFeature <color=#fff133>targetID</color> <color=#fff133>featureID</color> <color=#fff133>durationSec</color>", "Cấm người chơi ID tương ứng thực hiện chức năng (<color=#fff133>featureID</color> - 1: Giao dịch, 2: Bán vật phẩm, 3: Vứt vật phẩm, 4: Bày bán) trong khoảng thời gian (giây, hoặc -1 nếu vĩnh viễn) nhất định."),
            new KeyValuePair<string, string>("UnbanFeature <color=#fff133>targetID</color> <color=#fff133>featureID</color>", "Hủy cấm người chơi ID tương ứng thực hiện chức năng (<color=#fff133>featureID</color> - 1: Giao dịch, 2: Bán vật phẩm, 3: Vứt vật phẩm, 4: Bày bán)."),
            new KeyValuePair<string, string>("BanLogin <color=#fff133>targetID</color> <color=#fff133>durationSec</color> <color=#fff133>reason</color>", "Cấm người chơi ID tương ứng đăng nhập trong khoảng thời gian (giây, hoặc -1 nếu vĩnh viễn) nhất định. Yêu cầu ghi rõ lý do đằng sau."),
            new KeyValuePair<string, string>("UnbanLogin <color=#fff133>targetID</color>", "Hủy cấm đăng nhập của người chơi có ID tương ứng."),
            new KeyValuePair<string, string>("BanChat <color=#fff133>targetID</color> <color=#fff133>durationSec</color> <color=#fff133>reason</color>", "Cấm người chơi ID tương ứng chat trong khoảng thời gian (giây, hoặc -1 nếu vĩnh viễn) nhất định. Yêu cầu ghi rõ lý do đằng sau."),
            new KeyValuePair<string, string>("UnbanChat <color=#fff133>targetID</color>", "Hủy cấm chat của người chơi có ID tương ứng."),

            new KeyValuePair<string, string>("GoTo <color=#fff133>targetID</color> <color=#fff133>mapID</color> <color=#fff133>posX</color> <color=#fff133>posY</color>", "Dịch chuyển người chơi có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>) tới bản đồ có ID tương ứng hoặc bản đồ hiện tại (nếu bỏ trống <color=#fff133>mapID</color>), tại vị trí chỉ định."),
            
            new KeyValuePair<string, string>("AddSeriesState <color=#fff133>targetID</color> <color=#fff133>stateID</color> <color=#fff133>time</color>", "Thêm trạng thái ngũ hành cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>), trạng thái duy trì trong thời gian tương ứng được thiết lập."),
            new KeyValuePair<string, string>("RemoveSeriesState <color=#fff133>targetID</color> <color=#fff133>stateID</color>", "Xóa trạng thái ngũ hành của đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            
            new KeyValuePair<string, string>("JoinFaction <color=#fff133>targetID</color> <color=#fff133>factionID</color>", "Thiết lập môn phái cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            
            new KeyValuePair<string, string>("AddSkill <color=#fff133>targetID</color> <color=#fff133>skillID</color> <color=#fff133>level</color>", "Thêm kỹ năng cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>) với cấp độ tương ứng."),
            new KeyValuePair<string, string>("AddSkillExp <color=#fff133>targetID</color> <color=#fff133>skillID</color> <color=#fff133>exp</color>", "Thêm kinh nghiệm tu luyện kỹ năng cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>) với cấp độ tương ứng."),
            new KeyValuePair<string, string>("RemoveSkill <color=#fff133>targetID</color> <color=#fff133>skillID</color>", "Xóa kỹ năng khỏi đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            
            new KeyValuePair<string, string>("AddBuff <color=#fff133>targetID</color> <color=#fff133>skillID</color> <color=#fff133>level</color>", "Thêm Buff tương ứng kỹ năng cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>) với cấp độ tương ứng."),
            new KeyValuePair<string, string>("RemoveBuff <color=#fff133>targetID</color> <color=#fff133>skillID</color>", "Xóa Buff tương ứng kỹ năng khỏi đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            
            new KeyValuePair<string, string>("SysChat <color=#fff133>message</color>", "Gửi tin nhắn kênh hệ thống tới tất cả người chơi trong hệ thống."),
            new KeyValuePair<string, string>("SysNotify <color=#fff133>message</color>", "Gửi tin nhắn kênh hệ thống kèm dòng chữ chạy ngang thông báo phía trên đầu tới tất cả người chơi trong hệ thống."),
            
            new KeyValuePair<string, string>("CreatePet <color=#fff133>targetID</color> <color=#fff133>petID</color>", "Tạo tinh linh cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            new KeyValuePair<string, string>("AddPetExp <color=#fff133>targetID</color> <color=#fff133>exp</color>", "Thêm kinh nghiệm cho tinh linh đang tham chiến của người chơi tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            new KeyValuePair<string, string>("AddPetEnlightenment <color=#fff133>targetID</color> <color=#fff133>point</color>", "Thêm điểm lĩnh ngộ cho tinh linh đang tham chiến của người chơi tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            new KeyValuePair<string, string>("AddPetLife <color=#fff133>targetID</color> <color=#fff133>point</color>", "Thêm tuổi thọ cho tinh linh đang tham chiến của người chơi tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            new KeyValuePair<string, string>("AddPetJoyful <color=#fff133>targetID</color> <color=#fff133>point</color>", "Thêm điểm vui vẻ cho tinh linh đang tham chiến của người chơi tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            
            new KeyValuePair<string, string>("CreateItem <color=#fff133>targetID</color> <color=#fff133>itemID</color> <color=#fff133>quantity</color>", "Tạo vật phẩm số lượng nhất định cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            new KeyValuePair<string, string>("EquipEnhance <color=#fff133>targetID</color> <color=#fff133>bagPos</color> <color=#fff133>level</color>", "Thiết lập cấp độ cường hóa trang bị cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>) tại vị trí tương ứng trong túi đồ (đánh số từ 0)."),
            
            new KeyValuePair<string, string>("AddMoney <color=#fff133>targetID</color> <color=#fff133>type</color> <color=#fff133>amount</color>", "Thêm tiền cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>) tùy vào <color=#fff133>type</color> tương ứng (1: Bạc thường, 2: Bạc khóa, 3: KNB thường, 4: KNB khóa)."),
            new KeyValuePair<string, string>("AddDropItem <color=#fff133>targetID</color> <color=#fff133>monsterID</color>", "Thêm bọc rơi dưới MAP cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>) tùy vào <color=#fff133>monster</color> tương ứng."),
            
            new KeyValuePair<string, string>("ResetAllSkillCooldown <color=#fff133>targetID</color>", "Xóa tất cả thời gian phục hồi kỹ năng của đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            
            new KeyValuePair<string, string>("StartActivity <color=#fff133>activityID</color>", "Chủ động bắt đầu hoạt động có ID tương ứng."),
            new KeyValuePair<string, string>("StopActivity <color=#fff133>activityID</color>", "Chủ động kết thúc hoạt động có ID tương ứng."),
            
            new KeyValuePair<string, string>("SetPKValue <color=#fff133>targetID</color> <color=#fff133>value</color>", "Thiết lập giá trị sát của đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>). Giá trị này phải nằm trong khoảng <color=green>[0-10]</color>."),
            
            new KeyValuePair<string, string>("ResetLifeSkillLevelAndExp <color=#fff133>targetID</color> <color=#fff133>lifeSkillID</color> <color=#fff133>level</color> <color=#fff133>exp</color>", "Reset toàn bộ cấp độ và kinh nghiệm kỹ năng sống của đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            new KeyValuePair<string, string>("SetLifeSkillLevelAndExp <color=#fff133>targetID</color> <color=#fff133>lifeSkillID</color> <color=#fff133>level</color> <color=#fff133>exp</color>", "Thiết lập cấp độ và kinh nghiệm kỹ năng sống của đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>). Giá trị cấp độ phải nằm trong khoảng <color=green>[0-120]</color>."),
            new KeyValuePair<string, string>("AddGatherMakePoint <color=#fff133>targetID</color> <color=#fff133>gatherPoint</color> <color=#fff133>makePoint</color>", "Thêm điểm <color=#fff133>tinh lực</color> và <color=#fff133>hoạt lực</color> cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),
            
            new KeyValuePair<string, string>("SendSystemMail <color=#fff133>targetID</color> <color=#fff133>title</color> <color=#fff133>content</color> <color=#fff133>items</color> <color=#fff133>boundMoney</color> <color=#fff133>boundToken</color>", "Gửi Mail cho đối tượng có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID = -1</color>), tiêu đề và nội dung  (<color=orange>dấu cách</color> được thay thế bởi ký tự <color=orange>_</color>), kèm danh sách vật phẩm (ngăn cách bởi ký tự <color=orange>|</color>, thông tin mỗi vật phẩm tương ứng <color=green>ID_SL_Khóa_CấpCH</color>) và bạc khóa, KNB khóa tương ứng."),
            
            new KeyValuePair<string, string>("AddRepute <color=#fff133>targetID</color> <color=#fff133>reputeID</color> <color=#fff133>value</color>", "Thêm danh vọng cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            
            new KeyValuePair<string, string>("AddExp <color=#fff133>targetID</color> <color=#fff133>exp</color>", "Thêm kinh nghiệm cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            new KeyValuePair<string, string>("SetLevel <color=#fff133>targetID</color> <color=#fff133>level</color>", "Thiết lập cấp độ cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            
            //new KeyValuePair<string, string>("SetPrayTimes <color=#fff133>targetID</color> <color=#fff133>value</color>", "Thiết lập số lượt quay chúc phúc trong ngày cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),

            new KeyValuePair<string, string>("ModRoleTitle <color=#fff133>targetID</color> <color=#fff133>method</color> <color=#fff133>titleID</color>", "Thay đổi (-1: Xóa, 1: Thêm) danh hiệu nhân vật cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            new KeyValuePair<string, string>("ModSpecialTitle <color=#fff133>targetID</color> <color=#fff133>method</color> <color=#fff133>titleID</color>", "Thay đổi (-1: Xóa, 1: Thêm) danh hiệu đặc biệt cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            new KeyValuePair<string, string>("SetTempTitle <color=#fff133>targetID</color> <color=#fff133>title</color>", "Thiết lập danh hiệu tạm thời (<color=orange>dấu cách</color> được thay thế bởi ký tự <color=orange>_</color>) cho đối tượng có ID tương ứng hoặc bản thân (nếu bỏ trống <color=#fff133>targetID</color>)."),

            new KeyValuePair<string, string>("ResetKnowledgeChallengeQuestions <color=#fff133>targetID</color>", "Reset lại số lượt đã trả lời câu hỏi Đoán Hoa Đăng trong ngày cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),

            //new KeyValuePair<string, string>("ResetYouLong <color=#fff133>targetID</color>", "Reset lại số lượt đi Du Long Các trong ngày cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            
            new KeyValuePair<string, string>("UpdateTeamBattleAwardState <color=#fff133>targetID</color> <color=#fff133>state</color>", "Cập nhật trạng thái nhận thưởng Võ lâm liên đấu (0: không có phần thưởng, 1: có phần thưởng) cho chiến đội của người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            new KeyValuePair<string, string>("ArrangeTeamBattleFinalRound", "Ngay lập tức xếp hạng chiến đội và tăng bậc cho top các đội được vào chung kết Võ lâm liên đấu."),
            new KeyValuePair<string, string>("ArrangeTeamBattlePlayersRank", "Ngay lập tức xếp hạng người chơi sự kiện Võ lâm liên đấu."),
            new KeyValuePair<string, string>("ArrangeTeamBattlePlayersRankAndUpdateAwardsStateToAllTeams <color=#fff133>state</color>", "Ngay lập tức xếp hạng người chơi và thiết lập trạng thái nhận thưởng cho toàn bộ các chiến đội trong sự kiện Võ lâm liên đấu."),
            new KeyValuePair<string, string>("ResetTeamBattleTeamData", "Làm rỗng danh sách các chiến đội trong sự kiện Võ lâm liên đấu."),
            new KeyValuePair<string, string>("StartTeamBattle", "Bắt đầu lượt đấu ở mốc giờ hiện tại của Võ lâm liên đấu. Lệnh này sẽ không có tác dụng nếu không có mốc giờ hiện tại diễn ra sự kiện."),
            new KeyValuePair<string, string>("ReloadTeamBattle", "Tải mới lại file cấu hình Võ lâm liên đấu."),
            
            new KeyValuePair<string, string>("CreateMonster <color=#fff133>monsterID</color> <color=#fff133>type</color>", "Tạo quái có ID tương ứng. <color=#fff133>Type</color> là loại quái."),
            new KeyValuePair<string, string>("RemoveAllMonsters", "Xóa toàn bộ quái trong bản đồ hoặc phụ bản đang đứng."),
            
            //new KeyValuePair<string, string>("AddXiuLianZhu_Exp <color=#fff133>targetID</color> <color=#fff133>exp</color>", "Thêm kinh nghiệm Tu luyện cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            //new KeyValuePair<string, string>("SetXiuLianZhu_Exp <color=#fff133>targetID</color> <color=#fff133>exp</color>", "Thiết lập kinh nghiệm Tu luyện cho người chơi có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            //new KeyValuePair<string, string>("SetXiuLianZhu_TimeLeft <color=#fff133>targetID</color> <color=#fff133>hour10</color>", "Thiết lập số giờ Tu luyện cho người chơi (đơn vị <color=yellow>giờ * 10</color>, ví dụ <color=green>1.5 giờ = 15</color>, <color=green>10 giờ = 100</color>) có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            
            new KeyValuePair<string, string>("GetEmperorTombData <color=#fff133>targetID</color>", "Trả về thông tin Tần Lăng của người có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng), gồm thời gian còn lại (giây) và danh sách các tầng đã đi qua (sử dụng Dạ Minh Châu)."),
            new KeyValuePair<string, string>("ResetEmperorTombTimeLeft <color=#fff133>targetID</color>", "Reset số giờ trong Tần Lăng của người có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            
            new KeyValuePair<string, string>("ReloadSpecialEvent", "Tải lại File sự kiện đặc biệt mới nhất."),

            new KeyValuePair<string, string>("ReloadLuckyCircle", "Tải lại File sự kiện Vòng quay may mắn mới nhất."),
            new KeyValuePair<string, string>("GetLuckyCircleTotalTurn <color=#fff133>targetID</color>", "Trả về số lượt đã quay Vòng quay may mắn của người có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),
            new KeyValuePair<string, string>("SetLuckyCircleTotalTurn <color=#fff133>targetID</color> <color=#fff133>totalTurn</color>", "Thiết lập về số lượt đã quay Vòng quay may mắn của người có ID tương ứng hoặc bản thân (nếu <color=#fff133>targetID</color> rỗng)."),

            //new KeyValuePair<string, string>("ReloadSeashellCircle", "Tải lại File sự kiện Bách bảo rương mới nhất."),

            //new KeyValuePair<string, string>("", ""),
        };

        /// <summary>
        /// RectTransform của đối tượng chứa danh sách chú thích lệnh
        /// </summary>
        private RectTransform scrollViewTransform;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.scrollViewTransform = this.UI_DescriptionPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.BuildDescription();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.UIInput_Command.text = "";
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Process.onClick.AddListener(this.ButtonProcess_Clicked);
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button thực thi lệnh GM được ấn
        /// </summary>
        private void ButtonProcess_Clicked()
        {
            string command = this.UIInput_Command.text;
            if (string.IsNullOrEmpty(command))
            {
                KTGlobal.AddNotification("Chưa nhập lệnh GM!!!");
            }
            this.ProcessGMCommand?.Invoke(command);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện ở Frame tiếp theo
        /// </summary>
        /// <param name="work"></param>
        private void ExecuteNextFrame(Action work)
        {
            IEnumerator Execute()
            {
                yield return null;
                yield return null;
                work?.Invoke();
            }
            this.StartCoroutine(Execute());
        }

        /// <summary>
        /// Làm rỗng biểu diễn mô tả lệnh GM
        /// </summary>
        private void ClearDescription()
        {
            foreach (Transform child in this.UI_DescriptionPrefab.transform.parent.transform)
            {
                if (child.gameObject != this.UI_DescriptionPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Biểu diễn mô tả lệnh GM
        /// </summary>
        private void BuildDescription()
        {
            /// Làm rỗng biểu diễn
            this.ClearDescription();

            /// Thêm lệnh vào danh sách biểu diễn
            foreach (KeyValuePair<string, string> pair in this.commands)
            {
                UIGMCommand_Item uiItem = GameObject.Instantiate<UIGMCommand_Item>(this.UI_DescriptionPrefab);
                uiItem.transform.SetParent(this.scrollViewTransform, false);
                uiItem.gameObject.SetActive(true);
                uiItem.Command = pair.Key;
                uiItem.Description = pair.Value;
                uiItem.Paste = () => {
                    this.UIInput_Command.text = pair.Key.Split(' ')[0];
                };
            }

            this.ExecuteNextFrame(() => {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.scrollViewTransform);
            });
        }
        #endregion
    }
}
