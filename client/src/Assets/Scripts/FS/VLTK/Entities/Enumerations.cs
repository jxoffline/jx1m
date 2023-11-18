namespace FS.VLTK.Entities
{
    /// <summary>
    /// Các đối tượng ENUM trong KTScript
    /// </summary>
    public class Enum
    {
        /// <summary>
        /// Loại thiết bị
        /// </summary>
        public enum DeviceType
        {
            /// <summary>
            /// Editor
            /// </summary>
            Editor = 0,
            /// <summary>
            /// Android
            /// </summary>
            Android = 1,
            /// <summary>
            /// IOS
            /// </summary>
            IOS = 2,

            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }

        /// <summary>
        /// ID cơ bản bắt đầu của các đối tượng
        /// 32 bit = 2,147,483,648
        /// 7D2B 0000 = 2,099,970,048
        /// 7F00 0000 = 2,130,706,432
        /// 7F01 0000 = 2,130,771,968
        /// 7F40 0000 = 2,134,900,736
        /// 7F41 0000 = 2,134,966,272
        /// 7F42 0000 = 2,135,031,808
        /// 7F43 0000 = 2,135,097,344
        /// 7F50 0000 = 2,135,949,312
        /// </summary>
        public enum ObjectBaseID
        {
            /// <summary>
            /// Nhân vật
            /// </summary>
            Role = 0,
            /// <summary>
            /// NPC
            /// </summary>
            NPC = 0x7F000000,
            /// <summary>
            /// Quái
            /// </summary>
            Monster = 0x7F010000,
            /// <summary>
            /// Điểm thu thập
            /// </summary>
            GrowPoint = 0x7F400000,
            /// <summary>
            /// Khu vực động
            /// </summary>
            DynamicArea = 0x7F410000,
            /// <summary>
            /// Vật phẩm rơi
            /// </summary>
            GoodsPack = 0x7F420000,
            /// <summary>
            /// Bot biểu diễn
            /// </summary>
            DecoBot = 0x7F430000,
            /// <summary>
            /// Bot bán hàng
            /// </summary>
            StallBot = 0x7F440000,
            /// <summary>
            /// Cổng dịch chuyển (chỉ dùng ở Client)
            /// </summary>
            Teleport = 0x7F450000,
            /// <summary>
            /// Pet
            /// </summary>
            Pet = 0x7F460000,


            /// <summary>
            /// ID Max
            /// </summary>
            Max = 0x7F500000,
        }

        /// <summary>
        /// Vị trí trang bị pet
        /// </summary>
        public enum KE_EQUIP_PET_POSTION
        {
            /// <summary>
            /// Mũ
            /// </summary>
            emEQUIPPOS_HEAD = 200,

            /// <summary>
            /// Áo
            /// </summary>
            emEQUIPPOS_BODY,

            /// <summary>
            /// Lưng
            /// </summary>
            emEQUIPPOS_BELT,

            /// <summary>
            /// Vũ khí
            /// </summary>
            emEQUIPPOS_WEAPON,

            /// <summary>
            /// Giày
            /// </summary>
            emEQUIPPOS_FOOT,

            /// <summary>
            /// Tay
            /// </summary>
            emEQUIPPOS_CUFF,

            /// <summary>
            /// Phù  - Dây truyền
            /// </summary>
            emEQUIPPOS_AMULET,

            /// <summary>
            /// Nhẫn
            /// </summary>
            emEQUIPPOS_RING,

            /// <summary>
            /// Liên
            /// </summary>
            emEQUIPPOS_RING_2,

            /// <summary>
            /// Ngọc Bội
            /// </summary>
            emEQUIPPOS_PENDANT,

            /// <summary>
            /// Ngựa
            /// </summary>
            emEQUIPPOS_HORSE,

            /// <summary>
            /// Mặt nạ
            /// </summary>
            emEQUIPPOS_MASK,

            /// <summary>
            /// Mật tịch
            /// </summary>
            emEQUIPPOS_BOOK,

            /// <summary>
            /// Trang sức
            /// </summary>
            emEQUIPPOS_ORNAMENT,

            /// <summary>
            /// Ngũ hành ấn
            /// </summary>
            emEQUIPPOS_SIGNET,

            /// <summary>
            /// Phi phong
            /// </summary>
            emEQUIPPOS_MANTLE,

            /// <summary>
            /// Quan ấn
            /// </summary>
            emEQUIPPOS_CHOP,

            /// <summary>
            /// Tổng số
            /// </summary>
            emEQUIPPOS_NUM,
        };

        /// <summary>
        /// Các vị trí trang bị pet
        /// </summary>
        public enum PetEquipSlot
        {
            /// <summary>
            /// Mũ
            /// </summary>
            Helm = 0,
            /// <summary>
            /// Giáp
            /// </summary>
            Armor = 1,
            /// <summary>
            /// Lưng
            /// </summary>
            Belt = 2,
            /// <summary>
            /// Vũ khí
            /// </summary>
            Weapon = 3,
            /// <summary>
            /// Giày
            /// </summary>
            Boot = 4,
            /// <summary>
            /// Vòng
            /// </summary>
            Cuff = 5,
            /// <summary>
            /// Dây chuyền
            /// </summary>
            Necklace = 6,
            /// <summary>
            /// Nhẫn
            /// </summary>
            Ring = 7,
        }

        /// <summary>
        /// Nhận quà thẻ tháng
        /// </summary>
        public enum YueKaError
        {
            /// <summary>
            /// Thành công
            /// </summary>
            YK_Success,
            /// <summary>
            /// Không có thẻ tháng
            /// </summary>
            YK_CannotAward_HasNotYueKa,
            /// <summary>
            /// Chưa đến ngày này
            /// </summary>
            YK_CannotAward_DayHasPassed,
            /// <summary>
            /// Đã nhận quà rồi
            /// </summary>
            YK_CannotAward_AlreadyAward,
            /// <summary>
            /// Thời gian không hợp lệ
            /// </summary>
            YK_CannotAward_TimeNotReach,
            /// <summary>
            /// Túi đồ không đủ
            /// </summary>
            YK_CannotAward_BagNotEnough,
            /// <summary>
            /// Lỗi hệ thống
            /// </summary>
            YK_CannotAward_ParamInvalid,
            /// <summary>
            /// Lỗi thiết lập
            /// </summary>
            YK_CannotAward_ConfigError,
            /// <summary>
            /// Lỗi DB
            /// </summary>
            YK_CannotAward_DBError,
        }

        /// <summary>
        /// Danh sách xếp hạng
        /// </summary>
        public enum RankMode
        {
            /// <summary>
            /// Cấp độ
            /// </summary>
            CapDo = 0,
            /// <summary>
            /// Tài phú
            /// </summary>
            TaiPhu = 1,
            /// <summary>
            /// Võ lâm
            /// </summary>
            VoLam = 3,
            /// <summary>
            /// Liên đấu
            /// </summary>
            LienDau = 4,
            /// <summary>
            /// Uy danh
            /// </summary>
            UyDanh = 5,

            /// <summary>
            /// Thiếu Lâm
            /// </summary>
            ThieuLam = 11,
            /// <summary>
            /// Thiên Vương
            /// </summary>
            ThienVuong = 12,
            /// <summary>
            /// Đường Môn
            /// </summary>
            DuongMon = 13,
            /// <summary>
            /// Ngũ độc
            /// </summary>
            NguDoc = 14,
            /// <summary>
            /// Nga My
            /// </summary>
            NgaMy = 15,
            /// <summary>
            /// Thúy Yên
            /// </summary>
            ThuyYen = 16,
            /// <summary>
            /// Cái Bang
            /// </summary>
            CaiBang = 17,
            /// <summary>
            /// Thiên Nhẫn
            /// </summary>
            ThienNhan = 18,
            /// <summary>
            /// Võ Đang
            /// </summary>
            VoDang = 19,
            /// <summary>
            /// Côn Lôn
            /// </summary>
            ConLon = 20,
            /// <summary>
            /// Minh Giáo
            /// </summary>
            MinGiao = 21,
            /// <summary>
            /// Đoàn Thị
            /// </summary>
            DoanThi = 22,

        }

        /// <summary>
        /// Loại Button chức năng ở Main UI
        /// </summary>
        public enum FunctionButtonType
        {
            /// <summary>
            /// Mở Kỳ Trân Các
            /// </summary>
            OpenTokenShop,
            /// <summary>
            /// Mở khung quay sò
            /// </summary>
            OpenSeashellCircle,
            /// <summary>
            /// Mở khung phúc lợi nạp thẻ lần đầu
            /// </summary>
            OpenWelfareFirstRecharge,
            /// <summary>
            /// Mở khung phúc lợi
            /// </summary>
            OpenWelfare,
            /// <summary>
            /// Mở khung danh sách hoạt động
            /// </summary>
            OpenActivityList,
            /// <summary>
            /// Mở khung thiết lập hệ thống
            /// </summary>
            OpenSystemSetting,
            /// <summary>
            /// Mở khung bạn bè
            /// </summary>
            OpenFriendBox,
            /// <summary>
            /// Mở khung bang hội
            /// </summary>
            OpenGuildBox,
            /// <summary>
            /// Mở khung nhiệm vụ
            /// </summary>
            OpenTaskBox,
            /// <summary>
            /// Mở khung tìm người chơi
            /// </summary>
            OpenBrowsePlayer,
            /// <summary>
            /// Mở khung thư
            /// </summary>
            OpenMailBox,
            /// <summary>
            /// Mở khung túi đồ
            /// </summary>
            OpenBag,
            /// <summary>
            /// Mở khung thông tin nhân vật
            /// </summary>
            OpenRoleInfo,
            /// <summary>
            /// Mở khung kỹ năng
            /// </summary>
            OpenSkill,
            /// <summary>
            /// Mở khung kỹ năng sống
            /// </summary>
            OpenLifeSkill,
            /// <summary>
            /// Mở khung bách bảo rương
            /// </summary>
            OpenLuckyCircle,
            /// <summary>
            /// Mở khung bảng xếp hạng
            /// </summary>
            OpenTopRanking,
        }

        /// <summary>
        /// Sự kiện tương tác với Button chức năng
        /// </summary>
        public enum FunctionButtonAction
        {
            /// <summary>
            /// Hiện
            /// </summary>
            Show,
            /// <summary>
            /// Ẩn
            /// </summary>
            Hide,
            /// <summary>
            /// Kích hoạt
            /// </summary>
            Enable,
            /// <summary>
            /// Hủy kích hoạt
            /// </summary>
            Disable,
            /// <summary>
            /// Hint
            /// </summary>
            Hint,
        }

        /// <summary>
        /// Chức vụ bang hội
        /// </summary>
        public enum GuildRank
        {
            /// <summary>
            /// Bang chúng
            /// </summary>
            Member = 0,
            /// <summary>
            /// Bang chủ
            /// </summary>
            Master = 1,
            /// <summary>
            /// Phó bang chủ
            /// </summary>
            ViceMaster = 2,
            /// <summary>
            /// Trưởng lão
            /// </summary>
            Ambassador = 3,
            /// <summary>
            /// Đường chủ
            /// </summary>
            ViceAmbassador = 4,
            /// <summary>
            /// Tinh anh
            /// </summary>
            Elite = 5,
            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }

        /// <summary>
        /// Chức vụ gia tộc
        /// </summary>
        public enum FamilyRank
        {
            /// <summary>
            /// Thành Viên
            /// </summary>
            Member = 0,
            /// <summary>
            /// Tộc Trưởng
            /// </summary>
            Master = 1,
            /// <summary>
            /// Tộc Phó
            /// </summary>
            ViceMaster = 2,
            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }

        /// <summary>
        /// Trạng thái PK
        /// </summary>
        public enum PKMode
        {
            /// <summary>
            /// Hòa bình
            /// </summary>
            Peace = 0,
            /// <summary>
            /// Phe
            /// </summary>
            Team = 1,
            /// <summary>
            /// Bang hội
            /// </summary>
            Guild = 2,
            /// <summary>
            /// Đồ sát
            /// </summary>
            All = 3,
            /// <summary>
            /// Thiện ác
            /// </summary>
            Moral = 4,
            /// <summary>
            /// Tùy chọn tùy theo thiết lập sự kiện
            /// </summary>
            Custom = 5,
            /// <summary>
            /// PK theo Server
            /// </summary>
            Server = 6,
            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }

        /// <summary>
        /// Các loại tiền tệ trong hệ thống
        /// </summary>
        public enum MoneyType
        {
            /// <summary>
            /// Bạc khóa
            /// </summary>
            BacKhoa,
            /// <summary>
            /// Bạc
            /// </summary>
            Bac,
            /// <summary>
            /// KNB
            /// </summary>
            Dong,
            /// <summary>
            /// KNB khóa
            /// </summary>
            DongKhoa,

            /// <summary>
            /// Tiền bang hội
            /// </summary>
            GuildMoney,

            /// <summary>
            /// Thủ khố
            /// </summary>
            StoreMoney,
        }

        /// <summary>
        /// Loại vũ khí
        /// </summary>
        public enum KE_EQUIP_WEAPON_CATEGORY
        {
            /// <summary>
            /// Toàn bộ
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_ALL = 0,
            /// <summary>
            /// Không giới hạn
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_UNLIMITED = 0,

            /// <summary>
            /// Tay ngắn
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_MELEE = 10,
            /// <summary>
            /// Triền thủ
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_HAND = 11,
            /// <summary>
            /// Kiếm
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_SWORD = 12,
            /// <summary>
            /// Đao
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_KNIFE = 13,
            /// <summary>
            /// Côn
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_STICK = 14,
            /// <summary>
            /// Thương
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_SPEAR = 15,
            /// <summary>
            /// Chùy
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_HAMMER = 16,

            /// <summary>
            /// Tay dài
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_RANGE = 20,
            /// <summary>
            /// Phi tiêu
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_DART = 21,
            /// <summary>
            /// Phi đao
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_FLYBAR = 22,
            /// <summary>
            /// Tụ tiễn
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_ARROW = 23,

            /// <summary>
            /// Song đao
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_DOUBLESWORDS = 24,


            /// Trường Đao
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_RANGER_KNIFE = 25,
            /// <summary>
            /// Tổng số
            /// </summary>
            emKEQUIP_WEAPON_CATEGORY_NUM = 30,
        };

        /// <summary>
        /// Param thuộc tính khác của vật phẩm
        /// </summary>
        public enum ItemPramenter
        {
            /// <summary>
            /// Tham biến 1
            /// </summary>
            Pram_1,
            /// <summary>
            /// Tham biến 2
            /// </summary>
            Pram_2,
            /// <summary>
            /// Tham biến 3
            /// </summary>
            Pram_3,
            /// <summary>
            /// ID Người Chế
            /// </summary>
            Creator,
            /// <summary>
            /// Tổng quát
            /// </summary>
            Max,
        }


        /// <summary>
        /// Loại yêu cầu của vũ khí
        /// </summary>
        public enum KE_ITEM_REQUIREMENT
        {
            /// <summary>
            /// Không có
            /// </summary>
            emEQUIP_REQ_NONE = 0,


            emEQUIP_REQ_STR = 32,


            emEQUIP_REQ_DEX = 33,


            emEQUIP_REQ_VIT = 34,



            emEQUIP_REQ_ENG = 35,

            /// <summary>
            /// Yêu cầu cấp độ
            /// </summary>
            emEQUIP_REQ_LEVEL = 36,

            /// <summary>
            /// Yêu cầu môn phái
            /// </summary>
            emEQUIP_REQ_FACTION = 39,

            /// <summary>
            /// Yêu cầu ngũ hành
            /// </summary>
            emEQUIP_REQ_SERIES = 37,

            /// <summary>
            /// Yêu cầu giới tính
            /// </summary>
            emEQUIP_REQ_SEX = 38,
        };
        /// <summary>
        /// Danh sách chi tiết
        /// </summary>
        public enum KE_ITEM_EQUIP_DETAILTYPE
        {
            /// <summary>
            /// Trang bị vũ khí tầm gần
            /// </summary>
            equip_meleeweapon = 0,

            /// <summary>
            /// Trang bị vũ khí tầm xa
            /// </summary>
            equip_rangeweapon = 1,

            /// <summary>
            /// Quần áo
            /// </summary>
            equip_armor = 2,

            /// <summary>
            /// Nhẫn
            /// </summary>
            equip_ring = 3,

            /// <summary>
            /// Bội
            /// </summary>
            equip_amulet = 4,

            /// <summary>
            /// Giày
            /// </summary>
            equip_boots = 5,

            /// <summary>
            /// Lưng
            /// </summary>
            equip_belt = 6,

            /// <summary>
            /// Nũ
            /// </summary>
            equip_helm = 7,

            /// <summary>
            /// Tay
            /// </summary>
            equip_cuff = 8,

            /// <summary>
            /// Nang
            /// </summary>
            equip_pendant = 9,

            /// <summary>
            /// Ngựa
            /// </summary>
            equip_horse = 10,

            /// <summary>
            /// Mặt nạ
            /// </summary>
            equip_mask = 11,

            /// <summary>
            /// Mật tịch
            /// </summary>
            equip_book = 12,

            /// <summary>
            /// Trang sức
            /// </summary>
            equip_ornament = 13,

            /// <summary>
            /// Ngũ hành ấn
            /// </summary>
            equip_signet = 14,

            /// <summary>
            /// Phi phong
            /// </summary>
            equip_mantle = 15,

            /// <summary>
            /// Quan ấn
            /// </summary>
            equip_chop = 16,

            /// <summary>
            /// Tổng số
            /// </summary>
            equip_detailnum = 17,
        };

        /// <summary>
        /// Loại vật phẩm
        /// </summary>
        public enum KE_ITEM_GENRE
        {
            /// <summary>
            /// Đồ trắng và đồ xanh lam
            /// </summary>
            item_equip_general = 0,
            /// <summary>
            /// Thuốc
            /// </summary>
            item_medicine = 1,

            /// <summary>
            /// Đồ Tím
            /// </summary>
            item_equip_purple = 2,

            /// <summary>
            /// Đồ hoàng kim
            /// </summary>
            item_equip_gold = 3,
            /// <summary>
            /// Đồ Xanh
            /// </summary>
            item_equip_green = 4,
            /// <summary>
            /// Vật phẩm kịch bản
            /// </summary>
            item_script = 6,

            /// <summary>
            /// Trang bị pet
            /// </summary>
            item_pet_equip = 7,

            /// <summary>
            /// Vật phẩm kỹ năng
            /// </summary>
            item_skill = 19,
            /// <summary>
            /// Vật phẩm nhiệm vụ
            /// </summary>
            item_quest = 20,
            /// <summary>
            /// Ba lô mở rộng
            /// </summary>
            item_extbag = 21,
            /// <summary>
            /// Vật phẩm kỹ năng sống
            /// </summary>
            item_stuff = 22,
            /// <summary>
            /// Vật phẩm công thức kỹ năng sống
            /// </summary>
            item_plan = 23,
        };
        /// <summary>
        /// Loại trang bị
        /// </summary>
        public enum KE_EQUIP_POSITION
        {
            /// <summary>
            /// Mũ
            /// </summary>
            emEQUIPPOS_HEAD,
            /// <summary>
            /// Áo
            /// </summary>
            emEQUIPPOS_BODY,
            /// <summary>
            /// Lưng
            /// </summary>
            emEQUIPPOS_BELT,
            /// <summary>
            /// Vũ khí
            /// </summary>
            emEQUIPPOS_WEAPON,
            /// <summary>
            /// Giày
            /// </summary>
            emEQUIPPOS_FOOT,
            /// <summary>
            /// Tay
            /// </summary>
            emEQUIPPOS_CUFF,
            /// <summary>
            /// Phù
            /// </summary>
            emEQUIPPOS_AMULET,
            /// <summary>
            /// Nhẫn
            /// </summary>
            emEQUIPPOS_RING,
            /// <summary>
            /// Nhẫn 2
            /// </summary>
            emEQUIPPOS_RING_2,
            /// <summary>
            /// Bội
            /// </summary>
            emEQUIPPOS_PENDANT,
            /// <summary>
            /// Ngựa
            /// </summary>
            emEQUIPPOS_HORSE,
            /// <summary>
            /// Mặt nạ
            /// </summary>
            emEQUIPPOS_MASK,
            /// <summary>
            /// Mật tịch
            /// </summary>
            emEQUIPPOS_BOOK,
            /// <summary>
            /// Trang sức
            /// </summary>
            emEQUIPPOS_ORNAMENT,
            /// <summary>
            /// Ngũ hành ấn
            /// </summary>
            emEQUIPPOS_SIGNET,
            /// <summary>
            /// Phi phong
            /// </summary>
            emEQUIPPOS_MANTLE,
            /// <summary>
            /// Quan ấn
            /// </summary>
            emEQUIPPOS_CHOP,

            /// <summary>
            /// Tổng số
            /// </summary>
            emEQUIPPOS_NUM,
        };

        /// <summary>
        /// Kênh Chat
        /// </summary>
        public enum ChatChannel
        {
            /// <summary>
            /// Không rõ
            /// </summary>
            Default = -1,
            /// <summary>
            /// Hệ thống
            /// </summary>
            System = 0,
            /// <summary>
            /// Hệ thống, hiển thị cả trên dòng chữ chạy ngang
            /// </summary>
            System_Broad_Chat = 1,
            /// <summary>
            /// Bang hội
            /// </summary>
            Guild = 2,
            /// <summary>
            /// Liên minh
            /// </summary>
            Allies = 3,
            /// <summary>
            /// Đội ngũ
            /// </summary>
            Team = 4,
            /// <summary>
            /// Lân cận
            /// </summary>
            Near = 5,
            /// <summary>
            /// Phái
            /// </summary>
            Faction = 6,
            /// <summary>
            /// Mật
            /// </summary>
            Private = 7,
            /// <summary>
            /// Thế giới
            /// </summary>
            Global = 8,
            /// <summary>
            /// Kênh đặc biệt
            /// </summary>
            Special = 9,
            /// <summary>
            /// Kênh liên máy chủ
            /// </summary>
            KuaFuLine = 10,
            /// <summary>
            /// Toàn bộ
            /// </summary>
            All,
        }

        /// <summary>
        /// Trạng thái ngũ hành
        /// </summary>
        public enum KE_STATE
        {
            /// <summary>
            /// Chưa định nghĩa
            /// </summary>
            emSTATE_INVALID = -1,
            /// <summary>
            /// Bắt đầu
            /// </summary>
            emSTATE_BEGIN = 0,

            /// <summary>
            /// Bắt đầu trạng thái ngũ hành
            /// </summary>
            emSTATE_SERISE_BEGIN = emSTATE_BEGIN + 1,
            /// <summary>
            /// Thọ thương
            /// </summary>
            emSTATE_HURT = emSTATE_SERISE_BEGIN + 1,
            /// <summary>
            /// Suy yếu
            /// </summary>
            emSTATE_WEAK,
            /// <summary>
            /// Làm chậm (cả tốc chạy và tốc đánh)
            /// </summary>
            emSTATE_SLOWALL,
            /// <summary>
            /// Bỏng
            /// </summary>
            emSTATE_BURN,
            /// <summary>
            /// Choáng
            /// </summary>
            emSTATE_STUN,
            /// <summary>
            /// Kết thúc trạng thái ngũ hành
            /// </summary>
            emSTATE_SERISE_END = emSTATE_STUN + 1,

            /// <summary>
            /// Bắt đầu trạng thái đặc biệt
            /// </summary>
            emSTATE_SPECIAL_BEGIN,

            /// <summary>
            /// Bất động
            /// </summary>
            emSTATE_FIXED = emSTATE_SPECIAL_BEGIN + 1,
            /// <summary>
            /// Tê liệt
            /// </summary>
            emSTATE_PALSY,
            /// <summary>
            /// Giảm tốc chạy
            /// </summary>
            emSTATE_SLOWRUN,
            /// <summary>
            /// Đóng băng
            /// </summary>
            emSTATE_FREEZE,
            /// <summary>
            /// Hỗn loạn
            /// </summary>
            emSTATE_CONFUSE,
            /// <summary>
            /// Đẩy lui
            /// </summary>
            emSTATE_KNOCK,
            /// <summary>
            /// Kéo lại
            /// </summary>
            emSTATE_DRAG,
            /// <summary>
            /// Bất lực
            /// </summary>
            emSTATE_SILENCE,

            emSTATE_ZHICAN,     //ÖÂ²Ð×´Ì¬
            emSTATE_FLOAT,      //¸¡¿Õ×´Ì¬

            /// <summary>
            /// Kết thúc trạng thái đặc biệt
            /// </summary>
            emSTATE_SPECIAL_END = emSTATE_FLOAT + 1,

            /// <summary>
            /// Tổng số trạng thái ngũ hành + đặc biệt
            /// </summary>
            emSTATE_NUM,

            /// <summary>
            /// Trúng độc
            /// </summary>
            emSTATE_POISON = emSTATE_NUM + 1,
            /// <summary>
            /// Tàng hình
            /// </summary>
            emSTATE_HIDE,
            /// <summary>
            /// Khiên nội lực
            /// </summary>
            emSTATE_SHIELD,
            /// <summary>
            /// Đột tử
            /// </summary>
            emSTATE_SUDDENDEATH,
            /// <summary>
            /// Bỏ qua bẫy
            /// </summary>
            emSTATE_IGNORETRAP,
            /// <summary>
            /// Tất cả
            /// </summary>
            emSTATE_ALLNUM,
        };

        /// <summary>
        /// Trạng thái của đối tượng
        /// </summary>
        public enum KE_NPC_DOING
        {
            /// <summary>
            /// Không làm gì
            /// </summary>
            do_none,
            /// <summary>
            /// Đứng
            /// </summary>
            do_stand,
            /// <summary>
            /// Đi bộ
            /// </summary>
            do_walk,
            /// <summary>
            /// Chạy
            /// </summary>
            do_run,
            /// <summary>
            /// Nhảy
            /// </summary>
            do_jump,
            /// <summary>
            /// Sử dụng kỹ năng
            /// </summary>
            do_skill,
            /// <summary>
            /// Sử dụng phép
            /// </summary>
            do_magic,
            /// <summary>
            /// Tấn công
            /// </summary>
            do_attack,
            /// <summary>
            /// Ngồi
            /// </summary>
            do_sit,
            /// <summary>
            /// Bị thương
            /// </summary>
            do_hurt,
            /// <summary>
            /// Chết
            /// </summary>
            do_death,
            /// <summary>
            /// Đông cứng
            /// </summary>
            do_idle,
            do_specialskill,
            do_special1,
            do_special2,
            do_special3,
            do_special4,
            do_runattack,
            do_manyattack,
            do_jumpattack,
            /// <summary>
            /// Tái sinh
            /// </summary>
            do_revive,
            do_stall,
            do_movepos,
            /// <summary>
            /// Đẩy lui
            /// </summary>
            do_knockback,
            /// <summary>
            /// Kéo lại
            /// </summary>
            do_drag,
            do_rushattack,
            do_runattackmany,
            do_num,
        };

        /// <summary>
        /// Kết quả của kỹ năng
        /// </summary>
        public enum SkillResult
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = 0,
            /// <summary>
            /// Né
            /// </summary>
            Miss = 1,
            /// <summary>
            /// Hóa giải
            /// </summary>
            Adjust = 2,
            /// <summary>
            /// Miễn dịch
            /// </summary>
            Immune = 3,
            /// <summary>
            /// Sát thương thường
            /// </summary>
            Normal = 4,
            /// <summary>
            /// Sát thương chí mạng
            /// </summary>
            Crit = 5,
        }

        /// <summary>
        /// Kết quả trả về của hàm sử dụng kỹ năng kỹ năng
        /// </summary>
        public enum UseSkillResult
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = 0,
            /// <summary>
            /// Không có đối tượng xuất chiêu
            /// </summary>
            Caster_Is_Null = 1,
            /// <summary>
            /// Mục tiêu không nằm trong phạm vi đánh
            /// </summary>
            Target_Not_In_Range = 2,
            /// <summary>
            /// Không đủ nội lực
            /// </summary>
            Not_Enough_Mana = 3,
            /// <summary>
            /// Không tồn tại kỹ năng tương ứng
            /// </summary>
            No_Corresponding_Skill_Found = 4,
            /// <summary>
            /// Kỹ năng bị động không thể sử dụng
            /// </summary>
            Passive_Skill_Can_Not_Be_Used = 5,
            /// <summary>
            /// Kỹ năng đang trong trạng thái phục hồi
            /// </summary>
            Skill_Is_Cooldown = 6,
            /// <summary>
            /// Không có mục tiêu
            /// </summary>
            No_Target_Found = 7,
            /// <summary>
            /// Không tìm thấy dữ liệu cấu hình đạn
            /// </summary>
            Bullet_Data_Not_Found = 8,
            /// <summary>
            /// Không thể tấn công mục tiêu cùng trạng thái hòa bình
            /// </summary>
            Can_Not_Attack_Peace_Target = 9,
            /// <summary>
            /// Đối tượng xuất chiêu đã chết
            /// </summary>
            Caster_Is_Dead = 10,
            /// <summary>
            /// Không thể khinh công lúc này
            /// </summary>
            Can_Not_Fly_This_Time = 11,
            /// <summary>
            /// Trong trạng thái bị khống chế không thể dùng kỹ năng
            /// </summary>
            Can_Not_Use_Skill_While_Being_Locked = 12,
            /// <summary>
            /// Kỹ năng không thể dùng trong trạng thái cưỡi
            /// </summary>
            Can_Not_Use_Skill_While_Riding = 13,
            /// <summary>
            /// Vũ khí không thích hợp
            /// </summary>
            Unsuitable_Weapon = 14,
            /// <summary>
            /// Môn phái không thích hợp
            /// </summary>
            Unsuitable_Faction = 15,
            /// <summary>
            /// Hệ phái không thích hợp
            /// </summary>
            Unsuitable_Route = 16,

            /// <summary>
            /// Thành công
            /// </summary>
            Success,
        }

        /// <summary>
        /// Loại vũ khí
        /// </summary>
        public enum WeaponType
        {
            HAND, DART, MACE, BLADE, SWORD, SPEAR, STAFF
        }

        /// <summary>
        /// Thông tin chữ sát thương
        /// </summary>
        public enum DamageType
        {
            /// <summary>
            /// Gây sát thương
            /// </summary>
            PET_DAMAGE_DEALT,
            /// <summary>
            /// Gây sát thương chí mạng
            /// </summary>
            PET_CRIT_DAMAGE_DEALT,
            /// <summary>
            /// Gây sát thương
            /// </summary>
            DAMAGE_DEALT,
            /// <summary>
            /// Gây sát thương chí mạng
            /// </summary>
            CRIT_DAMAGE_DEALT,
            /// <summary>
            /// Chịu sát thương
            /// </summary>
            DAMAGE_TAKEN,
            /// <summary>
            /// Né
            /// </summary>
            DODGE,
            /// <summary>
            /// Hóa giải
            /// </summary>
            ADJUST,
            /// <summary>
            /// Miễn dịch
            /// </summary>
            IMMUNE,
        }

        /// <summary>
        /// Thông tin thông báo dưới
        /// </summary>
        public enum BottomTextDecorationType
        {
            /// <summary>
            /// Bạc thường
            /// </summary>
            Money,
            /// <summary>
            /// Bạc khóa
            /// </summary>
            BoundMoney,
            /// <summary>
            /// KNB thường
            /// </summary>
            Coupon,
            /// <summary>
            /// KNB khóa
            /// </summary>
            Coupon_Bound,
            /// <summary>
            /// Kinh nghiệm
            /// </summary>
            Exp,
            /// <summary>
            /// Tinh lực
            /// </summary>
            Gather,
            /// <summary>
            /// Hoạt lực
            /// </summary>
            Make,
        }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        public enum Elemental
        {
            /// <summary>
            /// Không có
            /// </summary>
            NONE = 0,
            /// <summary>
            /// Kim
            /// </summary>
            METAL = 1,
            /// <summary>
            /// Mộc
            /// </summary>
            WOOD = 2,
            /// <summary>
            /// Thủy
            /// </summary>
            WATER = 3,
            /// <summary>
            /// Hỏa
            /// </summary>
            FIRE = 4,
            /// <summary>
            /// Thổ
            /// </summary>
            EARTH = 5,
            /// <summary>
            /// Tổng số
            /// </summary>
            COUNT,
        }

        /// <summary>
        /// Hướng quay
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// Không có
            /// </summary>
            NONE = -1,
            /// <summary>
            /// Nam
            /// </summary>
            DOWN = 0,
            /// <summary>
            /// Tây Nam
            /// </summary>
            DOWN_LEFT = 1,
            /// <summary>
            /// Tây
            /// </summary>
            LEFT = 2,
            /// <summary>
            /// Tây Bắc
            /// </summary>
            UP_LEFT = 3,
            /// <summary>
            /// Bắc
            /// </summary>
            UP = 4,
            /// <summary>
            /// Đông Bắc
            /// </summary>
            UP_RIGHT = 5,
            /// <summary>
            /// Đông
            /// </summary>
            RIGHT = 6,
            /// <summary>
            /// Đông Nam
            /// </summary>
            DOWN_RIGHT = 7,
            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }

        /// <summary>
        /// Hướng quay (16 hướng), dùng cho Bullet
        /// </summary>
        public enum Direction16
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = -1,

            /// <summary>
            /// Nam
            /// </summary>
            Down = 0,
            /// <summary>
            /// Tây Nam Nam
            /// </summary>
            Down_Down_Left = 1,
            /// <summary>
            /// Tây Nam
            /// </summary>
            Down_Left = 2,
            /// <summary>
            /// Tây Tây Nam
            /// </summary>
            Down_Up_Left = 3,
            /// <summary>
            /// Tây
            /// </summary>
            Left = 4,
            /// <summary>
            /// Tây Tây Bắc
            /// </summary>
            Up_Down_Left = 5,
            /// <summary>
            /// Tây Bắc
            /// </summary>
            Up_Left = 6,
            /// <summary>
            /// Tây Bắc Bắc
            /// </summary>
            Up_Up_Left = 7,
            /// <summary>
            /// Bắc
            /// </summary>
            Up = 8,
            /// <summary>
            /// Đông Bắc Bắc
            /// </summary>
            Up_Up_Right = 9,
            /// <summary>
            /// Đông Bắc
            /// </summary>
            Up_Right = 10,
            /// <summary>
            /// Đông Đông Bắc
            /// </summary>
            Up_Down_Right = 11,
            /// <summary>
            /// Đông
            /// </summary>
            Right = 12,
            /// <summary>
            /// Đông Đông Nam
            /// </summary>
            Down_Up_Right = 13,
            /// <summary>
            /// Đông Nam
            /// </summary>
            Down_Right = 14,
            /// <summary>
            /// Đông Nam Nam
            /// </summary>
            Down_Down_Right = 15,
            /// <summary>
            /// Tổng số
            /// </summary>
            Count,
        }



        /// <summary>
        /// Hướng quay (32 hướng), dùng cho Bullet
        /// </summary>
        public enum Direction32
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = -1,

            Down = 0,
            Down_L = 1,
            Down_Down_Left = 2,
            Down_Down_Left_L = 3,
            Down_Left = 4,
            Down_Left_L = 5,
            Down_Up_Left = 6,
            Down_Up_Left_L = 7,
            Left = 8,
            Left_L = 9,
            Up_Down_Left = 10,
            Up_Down_Left_L = 11,
            Up_Left = 12,
            Up_Left_L = 13,
            Up_Up_Left = 14,
            Up_Up_Left_L = 15,
            Up = 16,
            Up_L = 17,
            Up_Up_Right = 18,
            Up_Up_Right_L = 19,
            Up_Right = 20,
            Up_Right_L = 21,
            Up_Down_Right = 22,
            Up_Down_Right_L = 23,
            Right = 24,
            Right_L = 25,
            Down_Up_Right = 26,
            Down_Up_Right_L = 27,
            Down_Right = 28,
            Down_Right_L = 29,
            Down_Down_Right = 30,
            Down_Down_Right_L = 31,
            Count,
        }

        /// <summary>
        /// Loại động tác của quái
        /// </summary>
        public enum MonsterActionType
        {
            /// <summary>
            /// Không có
            /// </summary>
            None,
            /// <summary>
            /// Đứng ở trạng thái tấn công
            /// </summary>
            FightStand,
            /// <summary>
            /// Đứng ở trạng thái hòa bình
            /// </summary>
            NormalStand,
            /// <summary>
            /// Bị thương
            /// </summary>
            Wound,
            /// <summary>
            /// Chạy
            /// </summary>
            Run,
            /// <summary>
            /// Tấn công thường
            /// </summary>
            NormalAttack,
            /// <summary>
            /// Tấn công Crit
            /// </summary>
            CritAttack,
            /// <summary>
            /// Chết
            /// </summary>
            Die,
            /// <summary>
            /// Lao nhanh đến phía mục tiêu
            /// </summary>
            RunAttack,
        }

        /// <summary>
        /// Loại động tác của nhân vật
        /// </summary>
        public enum PlayerActionType
        {
            /// <summary>
            /// Không có
            /// </summary>
            None,
            /// <summary>
            /// Đứng ở trạng thái tấn công
            /// </summary>
            FightStand,
            /// <summary>
            /// Đứng ở trạng thái hòa bình
            /// </summary>
            NormalStand,
            /// <summary>
            /// Đi bộ
            /// </summary>
            Walk,
            /// <summary>
            /// Chạy
            /// </summary>
            Run,
            /// <summary>
            /// Bị thương
            /// </summary>
            Wound,
            /// <summary>
            /// Chết
            /// </summary>
            Die,
            /// <summary>
            /// Tấn công thường
            /// </summary>
            NormalAttack,
            /// <summary>
            /// Tấn công Crit
            /// </summary>
            CritAttack,
            /// <summary>
            /// Dùng phép hoặc chưởng
            /// </summary>
            Magic,
            /// <summary>
            /// Ngồi
            /// </summary>
            Sit,
            /// <summary>
            /// Khinh công
            /// </summary>
            Jump,
            /// <summary>
            /// Lao nhanh đến phía mục tiêu
            /// </summary>
            RunAttack,
        }

        /// <summary>
        /// Loại hiệu ứng
        /// </summary>
        public enum EffectType
        {
            /// <summary>
            /// Không có
            /// </summary>
            None,
            /// <summary>
            /// Buff
            /// </summary>
            Buff,
            /// <summary>
            /// Hiệu ứng xuất chiêu
            /// </summary>
            CastEffect,
        }

        /// <summary>
        /// Giới tính
        /// </summary>
        public enum Sex
        {
            /// <summary>
            /// Không có
            /// </summary>
            NONE = -1,
            /// <summary>
            /// Nam
            /// </summary>
            MALE = 0,
            /// <summary>
            /// Nữ
            /// </summary>
            FEMALE = 1,
        }

        /// <summary>
        /// Loại bản đồ
        /// </summary>
        public enum MapType
        {
            /// <summary>
            /// Thành thị
            /// </summary>
            City,
            /// <summary>
            /// Tân thủ thôn
            /// </summary>
            Village,
            /// <summary>
            /// Map môn phái
            /// </summary>
            Faction,
            /// <summary>
            /// Map dã ngoại
            /// </summary>
            Fight,
        }

        /// <summary>
        /// Trạng thái ngũ hành
        /// </summary>
        public enum ElementState
        {
            /// <summary>
            /// Không có
            /// </summary>
            None = 0,
            /// <summary>
            /// Thọ thương
            /// </summary>
            Injury = 1,
            /// <summary>
            /// Suy yếu
            /// </summary>
            Weak = 2,
            /// <summary>
            /// Choáng
            /// </summary>
            Stun = 3,
            /// <summary>
            /// Làm chậm
            /// </summary>
            Slow = 4,
            /// <summary>
            /// Bỏng
            /// </summary>
            Burn = 5,

            /// <summary>
            /// Đẩy lui
            /// </summary>
            Knockback = 6,
            /// <summary>
            /// Đóng băng
            /// </summary>
            Freeze = 7,
            /// <summary>
            /// Hỗn loạn
            /// </summary>
            Panic = 8,
            /// <summary>
            /// Mù
            /// </summary>
            Blind = 9,
        }

        /// <summary>
        /// Loại kỹ năng
        /// </summary>
        public enum SkillType
        {
            /// <summary>
            /// Tấn công
            /// </summary>
            Attack = 2,
            /// <summary>
            /// Buff
            /// </summary>
            Buff = 1,
            /// <summary>
            /// Bị động
            /// </summary>
            Passive = 0,
        }

        /// <summary>
        /// Mục tiêu Buff
        /// </summary>
        public enum BuffTargetType
        {
            /// <summary>
            /// Bản thân
            /// </summary>
            Self = 0,
            /// <summary>
            /// Kẻ địch
            /// </summary>
            Enemy = 1,
            /// <summary>
            /// đồng đội
            /// </summary>
            Teammate = 2,
            /// <summary>
            /// Người chơi khác
            /// </summary>
            PeacePlayer = 3,
            /// <summary>
            /// Kẻ địch là quái
            /// </summary>
            EnemyMonster = 4,
            /// <summary>
            /// Kẻ địch là người chơi khác
            /// </summary>
            EnemyPlayer = 5,
        }

        /// <summary>
        /// Loại hiệu ứng Buff
        /// </summary>
        public enum BuffEffectType
        {
            /// <summary>
            /// Thêm chỉ số
            /// </summary>
            AddProperties = 0,
        }
    }
}
