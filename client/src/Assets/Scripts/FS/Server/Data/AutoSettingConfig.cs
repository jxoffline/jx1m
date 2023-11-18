using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Thiết lập Auto
    /// </summary>
    [ProtoContract]
    public class AutoSettingData
    {
        /// <summary>
        /// Tổng quát
        /// </summary>
        [ProtoContract]
        public class AutoGeneral
        {
            /// <summary>
            /// Từ chối thách đấu
            /// </summary>
            [ProtoMember(1)]
            public bool RefuseChallenge { get; set; }

            /// <summary>
            /// Từ chối giao dịch
            /// </summary>
            [ProtoMember(2)]
            public bool RefuseExchange { get; set; }

            /// <summary>
            /// Từ chối tổ đội
            /// </summary>
            [ProtoMember(3)]
            public bool RefuseTeam { get; set; }
        }

        /// <summary>
        /// Thiết lập tự động hỗ trợ
        /// </summary>
        [ProtoContract]
        public class AutoSupport
        {
            /// <summary>
            /// Tự dùng thuốc phục hồi sinh lực
            /// </summary>
            [ProtoMember(1)]
            public bool AutoHP { get; set; }

            /// <summary>
            /// Tự dùng thuốc phục hồi khi sinh lực dưới ngưỡng
            /// </summary>
            [ProtoMember(2)]
            public int AutoHPPercent { get; set; }

            /// <summary>
            /// Tự dùng thuốc phục hồi sinh lực
            /// </summary>
            [ProtoMember(3)]
            public bool AutoMP { get; set; }

            /// <summary>
            /// Tự dùng thuốc phục hồi khi nội lực dưới ngưỡng
            /// </summary>
            [ProtoMember(4)]
            public int AutoMPPercent { get; set; }

            /// <summary>
            /// Nga My tự dùng Từ Hàng Phổ Độ
            /// </summary>
            [ProtoMember(5)]
            public bool EM_AutoHeal { get; set; }

            /// <summary>
            /// Tự dùng Từ Hàng Phổ Độ khi đồng đội sinh lực giảm xuống ngưỡng
            /// </summary>
            [ProtoMember(6)]
            public int EM_AutoHealHPPercent { get; set; }

            /// <summary>
            /// ID thuốc hồi sinh lực
            /// </summary>
            [ProtoMember(7)]
            public int HPMedicine { get; set; }

            /// <summary>
            /// ID thuốc hồi nội lực
            /// </summary>
            [ProtoMember(8)]
            public int MPMedicine { get; set; }

            /// <summary>
            /// Tự dùng thuốc phục hồi sinh lực cho pet
            /// </summary>
            [ProtoMember(9)]
            public bool AutoPetHP { get; set; }

            /// <summary>
            /// Tự dùng thuốc phục hồi khi sinh lực cho pet khi dưới ngưỡng
            /// </summary>
            [ProtoMember(10)]
            public int AutoPetHPPercent { get; set; }

            /// <summary>
            /// ID thuốc hồi sinh lực cho pet
            /// </summary>
            [ProtoMember(11)]
            public int PetHPMedicine { get; set; }

            /// <summary>
            /// Tự dùng vật phẩm tăng tuổi thọ cho pet
            /// </summary>
            [ProtoMember(12)]
            public bool AutoPetLife { get; set; }

            /// <summary>
            /// Tự dùng vật phẩm tăng tuổi thọ cho pet
            /// </summary>
            [ProtoMember(13)]
            public bool AutoPetJoy { get; set; }

            /// <summary>
            /// Tự triệu hồi pet khi chết
            /// </summary>
            [ProtoMember(14)]
            public bool AutoCallPet { get; set; }

            /// <summary>
            /// Tự bú x2
            /// </summary>
            [ProtoMember(15)]
            public bool AutoX2 { get; set; }

            /// <summary>
            /// Tự mua thuốc
            /// </summary>
            [ProtoMember(16)]
            public bool AutoBuyMedicines { get; set; }

            /// <summary>
            /// Số lượng thuốc tự mua mỗi lần
            /// </summary>
            [ProtoMember(17)]
            public int AutoBuyMedicinesQuantity { get; set; }

            /// <summary>
            /// Tự mua thuốc ưu tiên dùng bạc khóa
            /// </summary>
            [ProtoMember(18)]
            public bool AutoBuyMedicinesUsingBoundMoneyPriority { get; set; }
        }

        /// <summary>
        /// Thiết lập tự đánh quái
        /// </summary>
        [ProtoContract]
        public class AutoFarm
        {
            /// <summary>
            /// Đánh quanh điểm
            /// </summary>
            [ProtoMember(1)]
            public bool FarmAround { get; set; }

            /// <summary>
            /// Bán kính quét
            /// </summary>
            [ProtoMember(2)]
            public int ScanRange { get; set; }

            /// <summary>
            /// Đơn mục tiêu
            /// </summary>
            [ProtoMember(3)]
            public bool SingleTarget { get; set; }

            /// <summary>
            /// Danh sách kỹ năng sẽ đánh quái
            /// </summary>
            [ProtoMember(4)]
            public List<int> Skills { get; set; }

            /// <summary>
            /// Tự động đốt lửa trại
            /// </summary>
            [ProtoMember(5)]
            public bool AutoFireCamp { get; set; }

            /// <summary>
            /// Bỏ qua boss
            /// </summary>
            [ProtoMember(6)]
            public bool IgnoreBoss { get; set; }

            /// <summary>
            /// Ưu tiên chọn quái có hp thấp
            /// </summary>
            [ProtoMember(7)]
            public bool LowHPTargetPriority { get; set; }

            /// <summary>
            /// Tự động uống rượu
            /// </summary>
            [ProtoMember(8)]
            public bool AutoDrinkWine { get; set; }

            /// <summary>
            /// Tự động uống rượu
            /// </summary>
            [ProtoMember(9)]
            public bool UseNewbieSkill { get; set; }
        }

        /// <summary>
        /// Thiết lập PK
        /// </summary>
        [ProtoContract]
        public class AutoPK
        {
            /// <summary>
            /// Tự động phản kháng khi bị PK
            /// </summary>
            [ProtoMember(1)]
            public bool AutoReflect { get; set; }

            /// <summary>
            /// Tự động chọn mục tiêu thấp máu
            /// </summary>
            [ProtoMember(2)]
            public bool LowHPTargetPriority { get; set; }

            /// <summary>
            /// Ưu tiên chọn khắc hệ
            /// </summary>
            [ProtoMember(3)]
            public bool SeriesConquarePriority { get; set; }

            /// <summary>
            /// Danh sách skill sẽ chọn để PK
            /// </summary>
            [ProtoMember(4)]
            public List<int> Skills { get; set; }

            /// <summary>
            /// Tự động mời đội
            /// </summary>
            [ProtoMember(5)]
            public bool AutoInviteToTeam { get; set; }

            /// <summary>
            /// Tự động chấp nhận mời đội
            /// </summary>
            [ProtoMember(6)]
            public bool AutoAccectJoinTeam { get; set; }

            /// <summary>
            /// Sử dụng kỹ năng tân thủ không
            /// </summary>
            [ProtoMember(7)]
            public bool UseNewbieSkill { get; set; }

            /// <summary>
            /// Có đuổi mục tiêu không
            /// </summary>
            [ProtoMember(8)]
            public bool ChaseTarget { get; set; }
        }

        /// <summary>
        /// Thiết lập nhặt và bán vật phẩm
        /// </summary>
        [ProtoContract]
        public class AutoPickAndSellItem
        {
            /// <summary>
            /// Tự động nhặt đồ
            /// </summary>
            [ProtoMember(1)]
            public bool EnableAutoPickUp { get; set; }

            /// <summary>
            /// Khoảng cách sẽ nhặt
            /// </summary>
            [ProtoMember(2)]
            public int ScanRadius { get; set; }

            /// <summary>
            /// Chỉ nhặt huyền tinh
            /// </summary>
            [ProtoMember(3)]
            public bool PickUpCrystalStoneOnly { get; set; }

            /// <summary>
            /// Cấp độ huyền tinh sẽ nhặt
            /// </summary>
            [ProtoMember(4)]
            public int PickUpCrystalStoneLevel { get; set; }

            /// <summary>
            /// Nhặt trang bị
            /// </summary>
            [ProtoMember(5)]
            public bool PickUpEquip { get; set; }

            /// <summary>
            /// Số sao sẽ nhặt
            /// </summary>
            [ProtoMember(6)]
            public int PickUpEquipStar { get; set; }

            /// <summary>
            /// Tự động sắp xếp túi đồ
            /// </summary>
            [ProtoMember(7)]
            public bool AutoSortBag { get; set; }

            /// <summary>
            /// Tự động bán đồ khi đầy
            /// </summary>
            [ProtoMember(8)]
            public bool AutoSellItems { get; set; }

            /// <summary>
            /// Số sao tối thiểu sẽ bán
            /// </summary>
            [ProtoMember(9)]
            public int SellEquipStar { get; set; }

            /// <summary>
            /// Nhặt các vật phẩm khác
            /// </summary>
            [ProtoMember(10)]
            public bool PickUpOtherItems { get; set; }

            /// <summary>
            /// Nhặt vật phẩm có số dòng bao nhiêu
            /// </summary>
            [ProtoMember(11)]
            public int PickUpItemByLinesCount { get; set; }
        }

        /// <summary>
        /// Tự động hỗ trợ
        /// </summary>
        [ProtoMember(1)]
        public AutoSupport Support { get; set; }

        /// <summary>
        /// Tự động đánh quái
        /// </summary>
        [ProtoMember(2)]
        public AutoFarm Farm { get; set; }

        /// <summary>
        /// Tự động PK
        /// </summary>
        [ProtoMember(3)]
        public AutoPK PK { get; set; }

        /// <summary>
        /// Tự động nhặt đồ
        /// </summary>
        [ProtoMember(4)]
        public AutoPickAndSellItem PickAndSell { get; set; }

        /// <summary>
        /// Tổng quát
        /// </summary>
        [ProtoMember(5)]
        public AutoGeneral General { get; set; }

        /// <summary>
        /// Mở kích hoạt tự PK
        /// </summary>
        [ProtoMember(6)]
        public bool EnableAutoPK { get; set; }
    }
}