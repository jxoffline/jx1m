using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Định nghĩa
    /// </summary>
    public partial class KTAutoFightManager
    {

       

        /// <summary>
        /// Trạng thái tự bán
        /// </summary>
        public enum AUTOSELLSTATE
        {
            NONE,
            GOTONPC,
            CLICKSELL,
            STARTSELL,
            MOVEBACK,
            END,
        }



        #region Define
        /// <summary>
        /// Trạng thái bán
        /// </summary>
        private AUTOSELLSTATE SELLSTATE = AUTOSELLSTATE.NONE;

        private AUTOSELLSTATE BUYSTATE = AUTOSELLSTATE.NONE;

        /// <summary>
        /// Thời gian kiểm tra lại xem khoảng cách với điểm trian ban đầu
        /// </summary>
        private Stopwatch TrainResetRadius { get; set; }

        /// <summary>
        /// Thơi gian đã auto. Dành để thực thi 1 số task liên quan tới thời gian auto
        /// </summary>
        private Stopwatch AutoTime = Stopwatch.StartNew();

        /// <summary>
        /// Thời gian bị triger. Dành để tính toán thời gian nhân vật sẽ tấn công lại mục tiêu tấn công mình
        /// </summary>
        private Stopwatch TrigerTime = Stopwatch.StartNew();

        /// <summary>
        /// Vị trí tạm thời
        /// </summary>
        private Vector2 TmpPostion { get; set; }

        /// <summary>
        /// Vị trí bắt đầu cắm Auto
        /// </summary>
        public Vector2 StartPos { get; set ; }

        /// <summary>
        /// Thời gian Tick kiểm tra tự đánh
        /// </summary>
        public float AutoFight_TickSec { get; set; } = 0.2f;

        /// <summary>
        /// Thời gian liên tiếp đánh Miss sẽ tự chuyển mục tiêu
        /// </summary>
        public int AutoFight_MissTickToChangeEnemy { get; set; } = 5000;

        /// <summary>
        /// Thời gian liên tiếp thông báo không tìm thấy kỹ năng Auto
        /// </summary>
        public int AutoFight_NotifyNoSkillTick { get; set; } = 5000;

        /// <summary>
        /// Thời gian liên tiếp kiểm tra và kích hoạt các kỹ năng Buff chủ động cho bản thân
        /// </summary>
        public int AutoFight_CheckAndAutoActivatePositiveBuffs { get; set; } = 10000;

        /// <summary>
        /// Thời gian liên tiếp NM tự Buff máu
        /// </summary>
        public int AutoFight_AutoEMBuffEveryTick { get; set; } = 1000;

        /// <summary>
        /// Thời gian kiểm tra dùng rượu liên tục
        /// </summary>
        public int AutoFight_AutoDrinkWineEveryTick { get; set; } = 2000;

        /// <summary>
        /// Thời gian kiểm tra dùng thuốc liên tục
        /// </summary>
        public int AutoFight_AutoUseMedicineEveryTick { get; set; } = 1000;



        /// <summary>
        /// Khoảng thời gian mỗi lần check X2 trong người
        /// </summary>
        public int AutoFight_AutoEatX2Tick { get; set; } = 10000;

        /// <summary>
        /// Thời gian thông báo không có rượu
        /// </summary>
        public int AutoFight_NotifyNoWineTick { get; set; } = 5000;

        /// <summary>
        /// Thời gian thông báo không có thuốc
        /// </summary>
        public int AutoFight_NotifyNoMedicineTick { get; set; } = 5000;

        /// <summary>
        /// Thời gian nhặt vật phẩm mỗi khoảng
        /// </summary>
        public int AutoFight_AutoPickUpItemsEveryTick { get; set; } = 500;

        /// <summary>
        /// Thời gian tự kiểm tra lửa trại xung quanh để đốt
        /// </summary>
        public int AutoFight_CheckNearbyFireCampEveryTick { get; set; } = 2000;

        /// <summary>
        /// Thời gian kiểm tra túi đồ đầy chưa để mang về bán
        /// </summary>
        public int AutoFight_CheckSellFullItemEveryTick { get; set; } = 10000;



        /// <summary>
        /// Chekc item có cần mua hay ko
        /// </summary>
        public int AutoFight_CheckBuyItemEveryTick { get; set; } = 10000;

        /// <summary>
        /// Thời gian xóa danh sách gói vật phẩm bỏ qua không nhặt mỗi khoảng
        /// </summary>
        public int AutoFight_AutoClearListIgnoreGoodsPack { get; set; } = 60000;

        /// <summary>
        /// Thời gian kiểm tra người chơi xung quanh và tự mời vào nhóm
        /// </summary>
        public int AutoFight_CheckNearbyPlayersToInviteToTeamEveryTick { get; set; } = 5000;

        /// <summary>
        /// Mục tiêu trước đó
        /// </summary>
        private GSprite AutoFightLastTarget = null;

        /// <summary>
        /// Mức máu lần trước của mục tiêu
        /// </summary>
        private int AutoFightLastTargetHP = 0;


        /// <summary>
        /// Bug vị trí hiện tại
        /// </summary>
        public int BugPostion = 0;

        /// <summary>
        /// Thời gian tấn công mục tiêu thành công
        /// </summary>
        private long AutoFightLastSuccessAttackTick = 0;

        /// <summary>
        /// Thời gian thông báo lần trước không tìm thấy kỹ năng
        /// </summary>
        private long AutoFightLastNotifyNoSkillTick = 0;

        /// <summary>
        /// Thời gian kiểm tra tự Buff các Buff chủ động cho bản thân
        /// </summary>
        private long AutoFightLastCheckAutoActivatePositiveBuffs = 0;

        /// <summary>
        /// Thời gian kiểm tra NM tự Buff
        /// </summary>
        private long AutoFightLastCheckAutoEMBuff = 0;

        /// <summary>
        /// Thời gian kiểm tra tự sử dụng rượu
        /// </summary>
        private long AutoFightLastCheckDrinkWine = 0;

        /// <summary>
        /// Thời gian kiểm tra tự sử dụng thuốc trước
        /// </summary>
        private long AutoFightLastCheckUseMedicineTick = 0;


        /// <summary>
        /// Khoảng thời gian gần đây nhất ăn X2
        /// </summary>
        private long AutoFightLastCheckEatX2 = 0;

        /// <summary>
        /// Thời gian thông báo không có rượu lần trước
        /// </summary>
        private long AutoFightLastNotifyNoWineTick = 0;

        /// <summary>
        /// Thời gian thông báo không có thuốc lần trước
        /// </summary>
        private long AutoFightLastNotifyNoMedicineTick = 0;

        /// <summary>
        /// Thời gian kiểm tra tự nhặt vật phẩm
        /// </summary>
        private long AutoFightLastCheckAutoPickUpItems = 0;

        /// <summary>
        /// Thời gian xóa danh sách gói vật phẩm bỏ qua không nhặt
        /// </summary>
        private long AutoFightLastCheckClearListIgnoreGoodsPack = 0;

        /// <summary>
        /// Thời gian kiểm tra lửa trại xung quanh
        /// </summary>
        private long AutoFightLastCheckNearbyFireCamp = 0;

        /// <summary>
        /// Thowfi gian kiểm tra túi đồ đầy chưa để bán gần đây nhất
        /// </summary>
        private long AutoFightLastCheckAutoSell = 0;



        /// <summary>
        /// thời gian kiểm tra có cần mua thêm máu ko
        /// </summary>
        private long AutoFightLastCheckBuyItem = 0;


        /// <summary>
        /// Thời gian cảnh báo không có mục tiêu gần đây nhất
        /// </summary>
        private long LastArletNoTarget = 0;

        /// <summary>
        /// Thời gian kiểm tra người chơi xung quanh và tự mời vào nhóm
        /// </summary>
        private long AutoFightLastCheckNearbyPlayersToInviteToTeamEveryTick = 0;

        /// <summary>
        /// Danh sách gói vật phẩm bỏ qua không nhặt
        /// </summary>
        private readonly HashSet<int> ListIgnoreGoodsPack = new HashSet<int>();

        #endregion Define

        #region Public methods

        /// <summary>
        /// Thay đổi mục tiêu đến một mục tiêu ngẫu nhiên gần nhất
        /// </summary>
        /// <returns></returns>
        /// <param name="ignoredTarget">Danh sách mục tiêu bị bỏ qua</param>
        public void ChangeAutoFightTarget(List<GSprite> ignoredTarget = null)
        {
         
            /// Tìm mục tiêu gần nhất khác với mục tiêu hiện tại
            GSprite newTarget = this.FindBestTarget(ignoredTarget, true);
            /// Đánh dấu mục tiêu hiện tại của kỹ năng
            SkillManager.SelectedTarget = newTarget;
            this.AutoFightLastTarget = newTarget;
            /// Reset thời điểm đánh trúng mục tiêu
            this.AutoFightLastSuccessAttackTick = KTGlobal.GetCurrentTimeMilis();
            /// Cập nhật máu
            this.AutoFightLastTargetHP = SkillManager.GetTargetTrueHP(newTarget);
        }

        /// <summary>
        /// Thay đổi mục tiêu đến đối tượng tương ứng
        /// </summary>
        /// <returns></returns>
        public void ChangeAutoFightTarget(GSprite target)
        {
            this.AutoFightLastTarget = target;
            this.AutoFightLastSuccessAttackTick = KTGlobal.GetCurrentTimeMilis();
            this.AutoFightLastTargetHP = SkillManager.GetTargetTrueHP(target);
        }

        /// <summary>
        /// Làm rỗng dữ liệu
        /// </summary>
        public void Clear()
        {
            this.ListIgnoreGoodsPack.Clear();
        }

        #endregion Public methods
    }
}