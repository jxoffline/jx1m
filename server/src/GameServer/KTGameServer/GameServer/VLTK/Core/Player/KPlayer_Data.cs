using GameServer.Core.Executor;
using GameServer.Interface;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Activity.CardMonth;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Title;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Entities.Player;
using GameServer.KiemThe.Logic;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý các thuộc tính cơ bản
    /// </summary>
    public partial class KPlayer
    {
        #region Vị trí hợp lệ
        /// <summary>
        /// Vị trí hợp lệ lần trước
        /// </summary>
        public Point LastValidPos { get; set; }

        /// <summary>
        /// Quay trở lại vị trí hợp lệ trước đó
        /// </summary>
        /// <param name="force"></param>
        public void RollbackPosition(bool force = false)
        {
            /// Nếu không bắt buộc thì mới kiểm tra điều kiện
            if (!force)
            {
                /// Nếu đang đợi chuyển bản đồ
                if (this.WaitingForChangeMap)
                {
                    /// Bỏ qua
                    return;
                }
                /// Nếu đang ở vị trí có thể đến được
                else if (!KTGlobal.InObs(this.CurrentMapCode, this.PosX, this.PosY, this.CurrentCopyMapID))
                {
                    /// Bỏ qua
                    return;
                }
                /// Toác
                else if (this.LastValidPos.Equals(default))
                {
                    /// Bỏ qua
                    return;
                }
            }

            /// Thông báo cho bản thân
            KTPlayerManager.ChangePos(this, (int) this.LastValidPos.X, (int) this.LastValidPos.Y);
        }
        #endregion

        #region Mật khẩu cấp 2
        /// <summary>
        /// Mật khẩu cấp 2
        /// </summary>
        public string SecondPassword
        {
            get
            {
                return this._RoleDataEx.SecondPassword;
            }
            set
            {
                lock (this._RoleDataEx)
                {
                    this._RoleDataEx.SecondPassword = value;
                }
            }
        }

        /// <summary>
        /// Tổng số lần nhập sai mật khẩu cấp 2
        /// </summary>
        public int TotalInputIncorrectSecondPasswordTimes
        {
            get
            {
                return this.GetValueOfDailyRecore((int)DailyRecord.TotalInputIncorrectSecondPasswordTimes);
            }
            set
            {
                this.SetValueOfDailyRecore((int)DailyRecord.TotalInputIncorrectSecondPasswordTimes, value);
            }
        }

        /// <summary>
        /// Thời điểm yêu cầu hủy mật khẩu cấp 2
        /// </summary>
        public int RequestRemoveSecondPasswordTicks
        {
            get
            {
                return this.GetValueOfForeverRecore(ForeverRecord.RequestRemoveSecondPasswordTicks);
            }
            set
            {
                this.SetValueOfForeverRecore(ForeverRecord.RequestRemoveSecondPasswordTicks, value);
            }
        }

        /// <summary>
        /// Đánh dấu đã nhập mật khẩu cấp 2 chưa
        /// </summary>
        public bool IsSecondPasswordInput { get; set; } = false;

        /// <summary>
        /// Có yêu cầu nhập mật khẩu cấp 2 trước khi thao tác không
        /// </summary>
        /// <returns></returns>
        public bool NeedToShowInputSecondPassword()
        {
            /// Nếu không có
            if (string.IsNullOrEmpty(this.SecondPassword))
            {
                /// Không cần
                return false;
            }
            /// Nếu đã nhập rồi
            else if (this.IsSecondPasswordInput)
            {
                /// Không cần
                return false;
            }
            /// Nếu nhập sai quá nhiều lần
            else if (this.TotalInputIncorrectSecondPasswordTimes >= 10)
            {
                /// Khóa mõm luôn
                KTPlayerManager.ShowNotification(this, "Bạn đã nhập sai khóa an toàn quá 10 lần, ngày mai hãy nhập chính xác rồi mới có thể thực hiện thao tác này!");
                /// Có khung nhưng bị khóa mõm nên đéo hiện
                return true;
            }
            /// Hiện khung
            KT_TCPHandler.SendOpenInputSecondPassword(this);
            /// Có khung
            return true;
        }
        #endregion

        #region Recore Event

        /// Nhận quà tải game chưa
        public int ReviceBounsDownload { get; set; }

        public DailyDataRecore _DailyDataRecore { get; set; } = new DailyDataRecore();

        public WeekDataRecore _WeekDataRecore { get; set; } = new WeekDataRecore();

        public ForeverRecore _ForeverRecore { get; set; } = new ForeverRecore();

        /// <summary>
        /// Set giá trị lưu trữ vĩnh viễn
        /// Key cần def kỹ để tránh bị trùng với các key khác
        /// Tham khảo tại Enumration để biết quy tắc đặt key sao cho không trùng
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void SetValueOfForeverRecore(ForeverRecord Key, int Value)
        {  // Lấy ra ngày trong năm
            lock (_ForeverRecore)
            {
                // Thử xem đã có key này trong nhật ký ghi chép chưa
                if (_ForeverRecore.EventRecoding.ContainsKey((int)Key))
                {
                    // Nếu có rồi thì thay thế key cũ
                    _ForeverRecore.EventRecoding[(int)Key] = Value;
                }
                else // Nếu key này chưa có trong nhật ký ghi chép thì tạo mới
                {
                    _ForeverRecore.EventRecoding.Add((int)Key, Value);
                }
            }
        }





        public void SetValueOfForeverRecore(int Key, int Value)
        {  // Lấy ra ngày trong năm
            lock (_ForeverRecore)
            {
                // Thử xem đã có key này trong nhật ký ghi chép chưa
                if (_ForeverRecore.EventRecoding.ContainsKey(Key))
                {
                    // Nếu có rồi thì thay thế key cũ
                    _ForeverRecore.EventRecoding[Key] = Value;
                }
                else // Nếu key này chưa có trong nhật ký ghi chép thì tạo mới
                {
                    _ForeverRecore.EventRecoding.Add((int)Key, Value);
                }
            }
        }








        /// <summary>
        /// Lấy ra giá trị key lưu trũ vĩnh viễn
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public int GetValueOfForeverRecore(ForeverRecord Key)
        {
            lock (_ForeverRecore)
            {
                if (_ForeverRecore.EventRecoding.ContainsKey((int)Key))
                {
                    return _ForeverRecore.EventRecoding[(int)Key];
                }
                else
                {
                    return -1;
                }
            }
        }


        public int GetValueOfForeverRecore(int Key)
        {
            lock (_ForeverRecore)
            {
                if (_ForeverRecore.EventRecoding.ContainsKey(Key))
                {
                    return _ForeverRecore.EventRecoding[Key];
                }
                else
                {
                    return -1;
                }
            }
        }



        public void SetValueOfDailyRecore(int Key, int Value)
        {  // Lấy ra ngày trong năm
            int Day = DateTime.Now.DayOfYear;

            lock (_DailyDataRecore)
            {
                // Check xem nếu mà thời gian khác thì tạo mới thực thể
                if (_DailyDataRecore.DayID != Day)
                {
                    _DailyDataRecore = new DailyDataRecore();
                    _DailyDataRecore.DayID = Day;
                }

                // Thử xem đã có key này trong nhật ký ghi chép chưa
                if (_DailyDataRecore.EventRecoding.ContainsKey(Key))
                {
                    // Nếu có rồi thì thay thế key cũ
                    _DailyDataRecore.EventRecoding[Key] = Value;
                }
                else // Nếu key này chưa có trong nhật ký ghi chép thì tạo mới
                {
                    _DailyDataRecore.EventRecoding.Add(Key, Value);
                }
            }
        }

        public int GetValueOfDailyRecore(int Key)
        {
            // Lấy ra ngày trong năm
            int Day = DateTime.Now.DayOfYear;

            lock (_DailyDataRecore)
            {
                // Nếu thông tin lưu trong nhật ký không phải ngày mới nhất thì thôi luôn
                if (_DailyDataRecore.DayID != Day)
                {
                    _DailyDataRecore = new DailyDataRecore();
                    _DailyDataRecore.DayID = Day;
                    return -1;
                }

                if (_DailyDataRecore.EventRecoding.ContainsKey(Key))
                {
                    return _DailyDataRecore.EventRecoding[Key];
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Funtion xử lý việc ghi chép các giá trị trong tuần
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void SetValueOfWeekRecore(int Key, int Value)
        {
            // Lấy ra tuần trong năm
            int WeekID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

            lock (_WeekDataRecore)
            {
                // Check xem nếu mà thời gian khác thì tạo mới thực thể
                if (_WeekDataRecore.WeekID != WeekID)
                {
                    _WeekDataRecore = new WeekDataRecore();
                    _WeekDataRecore.WeekID = WeekID;
                }

                // Thử xem đã có key này trong nhật ký ghi chép chưa
                if (_WeekDataRecore.EventRecoding.ContainsKey(Key))
                {
                    // Nếu có rồi thì thay thế key cũ
                    _WeekDataRecore.EventRecoding[Key] = Value;
                }
                else // Nếu key này chưa có trong nhật ký ghi chép thì tạo mới
                {
                    _WeekDataRecore.EventRecoding.Add(Key, Value);
                }
            }
        }

        public int GetValueOfWeekRecore(int Key)
        {
            // Lấy ra ngày trong năm
            int WeekID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

            lock (_WeekDataRecore)
            {
                // Nếu thông tin lưu trong nhật ký không phải ngày mới nhất thì thôi luôn
                if (_WeekDataRecore.WeekID != WeekID)
                {
                    _WeekDataRecore = new WeekDataRecore();
                    _WeekDataRecore.WeekID = WeekID;
                    return -1;
                }

                if (_WeekDataRecore.EventRecoding.ContainsKey(Key))
                {
                    return _WeekDataRecore.EventRecoding[Key];
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Chuyển về string
        /// </summary>
        public string DailyRecoreString
        {
            get
            {
                lock (_DailyDataRecore)
                {
                    byte[] DataByteArray = DataHelper.ObjectToBytes(this._DailyDataRecore);

                    string InfoEncoding = Convert.ToBase64String(DataByteArray);

                    return InfoEncoding;
                }
            }
        }

        public string ForeverRecoreString
        {
            get
            {
                lock (_ForeverRecore)
                {
                    byte[] DataByteArray = DataHelper.ObjectToBytes(this._ForeverRecore);

                    string InfoEncoding = Convert.ToBase64String(DataByteArray);

                    return InfoEncoding;
                }
            }
        }
        /// <summary>
        /// Chuyển về string
        /// </summary>
        public string WeekRecoreString
        {
            get
            {
                lock (_WeekDataRecore)
                {
                    byte[] DataByteArray = DataHelper.ObjectToBytes(this._WeekDataRecore);

                    string InfoEncoding = Convert.ToBase64String(DataByteArray);

                    return InfoEncoding;
                }
            }
        }


        /// <summary>
        /// Trạng thái nút hiện tại
        /// </summary>
        public int BtnState = -1;
        /// <summary>
        /// Nút hiện tại
        /// </summary>
        public int ReviceIndex = -1;




        #endregion Recore Event

        #region Anti-Auto
        /// <summary>
        /// Hàm này gọi sau khi người chơi trả lời Captcha
        /// </summary>
        public Action<bool> AnswerCaptcha { get; set; }

        /// <summary>
        /// Thời điểm ra Captcha lần tới
        /// </summary>
        public long NextCaptchaTicks { get; set; }

        /// <summary>
        /// Thời điểm hiện bảng trả lời Captcha khi ở tù
        /// </summary>
        public long LastJailCaptchaTicks { get; set; }

        /// <summary>
        /// Vị trí mở Shop lần trước có NPC
        /// </summary>
        public NPC LastShopNPC { get; set; }

        /// <summary>
        /// NPC Dã Luyện Đại Sư trước đó
        /// </summary>
        public NPC LastEquipMasterNPC { get; set; }
        #endregion

        #region Đoán hoa đăng
        /// <summary>
        /// Thời điểm lần trước mở câu hỏi đoán hoa đăng
        /// </summary>
        public long LastOpenKnowledgeChallengeQuestion { get; set; }
        #endregion

        #region Khu an toàn
        /// <summary>
        /// Biến này đánh dấu trước đó có đang ở trong khu an toàn không
        /// </summary>
        private bool _LastIsInsideSafeZone = false;

        /// <summary>
        /// Có phải đang ở trong khu an toàn không
        /// <para>Trong khu an toàn không thể sử dụng kỹ năng tấn công</para>
        /// </summary>
        public bool IsInsideSafeZone
        {
            get
            {
                /// Thông tin bản đồ đang đứng
                GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
                /// Toác
                if (gameMap == null)
                {
                    return false;
                }

                /// Kiểm tra ở NodeGrid
                return gameMap.MyNodeGrid.InSafeArea(this.PosX / gameMap.MapGridWidth, this.PosY / gameMap.MapGridHeight);
            }
        }
        #endregion

        #region Tu Luyện Châu
        private int _XiuLianZhu_Exp;
        /// <summary>
        /// Kinh nghiệm Tu Luyện Châu có
        /// </summary>
        public int XiuLianZhu_Exp
        {
            get
            {
                lock (this)
                {
                    return this._XiuLianZhu_Exp;
                }
            }
            set
            {
                lock (this)
                {
                    this._XiuLianZhu_Exp = value;
                }
            }
        }

        private int _XiuLianZhu_TotalTime;
        /// <summary>
        /// Thời gian Tu Luyện còn lại (Giờ * 10)
        /// </summary>
        public int XiuLianZhu_TotalTime
        {
            get
            {
                /// Ngày hôm nay đã cộng chưa
                int addTime = this.GetValueOfDailyRecore((int)DailyRecord.XiuLianZhu_TodayTimeAdded);
                /// Nếu chưa cộng
                if (addTime < 0)
                {
                    addTime = ItemXiuLianZhuManager.GetHourAddPerDay() * 10;
                    this.SetValueOfDailyRecore((int)DailyRecord.XiuLianZhu_TodayTimeAdded, 1);
                }
                /// Nếu đã cộng rồi thì thôi
				else
                {
                    addTime = 0;
                }

                lock (this)
                {
                    /// Nếu có lượng cộng thêm
                    if (addTime > 0)
                    {
                        int val = this._XiuLianZhu_TotalTime + addTime;
                        this._XiuLianZhu_TotalTime = val;
                        /// Nếu vượt quá 14
                        if (this._XiuLianZhu_TotalTime > 140)
                        {
                            this._XiuLianZhu_TotalTime = 140;
                        }
                    }
                    /// Trả về kết quả
                    return this._XiuLianZhu_TotalTime;
                }
            }
            set
            {
                /// Ngày hôm nay đã cộng chưa
                int addTime = this.GetValueOfDailyRecore((int)DailyRecord.XiuLianZhu_TodayTimeAdded);
                /// Nếu chưa cộng
                if (addTime < 0)
                {
                    addTime = ItemXiuLianZhuManager.GetHourAddPerDay() * 10;
                    this.SetValueOfDailyRecore((int)DailyRecord.XiuLianZhu_TodayTimeAdded, 1);
                }
                /// Nếu đã cộng rồi thì thôi
				else
                {
                    addTime = 0;
                }

                lock (this)
                {
                    int val = value + addTime;
                    if (val > 140)
                    {
                        val = 140;
                    }

                    /// Lưu lại kết quả
                    this._XiuLianZhu_TotalTime = val;

                }
            }
        }
        #endregion

        #region Phụ bản

        /// <summary>
        /// Danh sách biến tạm dùng cho phụ bản
        /// </summary>
        private readonly Dictionary<int, int> TempCopySceneParams = new Dictionary<int, int>();

        /// <summary>
        /// Thiết lập biến tạm phụ bản tương ứng
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetTempCopySceneParam(int key, int value)
        {
            lock (this.TempCopySceneParams)
            {
                this.TempCopySceneParams[key] = value;
            }
        }

        /// <summary>
        /// Trả về biến tạm phụ bản tương ứng
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetTempCopySceneParam(int key)
        {
            if (this.TempCopySceneParams.TryGetValue(key, out int value))
            {
                return value;
            }
            return 0;
        }

        #endregion Phụ bản

        #region Tần Lăng
        /// <summary>
        /// Thời điểm Tick Tần Lăng trước
        /// </summary>
        public long LastEmperorTombTicks { get; set; }
        #endregion

        #region Bách Bảo Rương
        /// <summary>
        /// GM thiết lập cho quay vào rương xấu xí ở lượt tiếp theo
        /// </summary>
        public bool GM_SetWillGetTreasureNextTurn { get; set; } = false;

        /// <summary>
        /// GM thiết lập cho quay vào rương xấu xí ở lượt tiếp theo với số cược tương ứng
        /// <para>-1 sẽ bỏ qua, cược bất cứ giá trị nào cũng được</para>
        /// </summary>
        public int GM_SetWillGetTreasureNextTurnWithBet { get; set; } = -1;

        private long _LastSeashellCircleTicks = 0;
        /// <summary>
        /// Thời điểm lần trước quay sò
        /// </summary>
        public long LastSeashellCircleTicks
        {
            get
            {
                return this._LastSeashellCircleTicks;
            }
            set
            {
                this._LastSeashellCircleTicks = value;
            }
        }


        private int _LastSeashellCircleStopPos = -1;

        /// <summary>
        /// Vị trí dừng lại lần trước khi quay sò
        /// </summary>
        public int LastSeashellCircleStopPos
        {
            get
            {
                return this._LastSeashellCircleStopPos;
            }
            set
            {
                this._LastSeashellCircleStopPos = value;
            }
        }

        private int _LastSeashellCircleStage = -1;

        /// <summary>
        /// Tầng lần trước dừng lại khi quay sò
        /// </summary>
        public int LastSeashellCircleStage
        {
            get
            {
                return this._LastSeashellCircleStage;
            }
            set
            {
                this._LastSeashellCircleStage = value;
            }
        }

        private int _LastSeashellCircleBet = -1;

        /// <summary>
        /// Số sò đặt cược lần trước
        /// </summary>
        public int LastSeashellCircleBet
        {
            get
            {
                return this._LastSeashellCircleBet;
            }
            set
            {
                this._LastSeashellCircleBet = value;
            }
        }

        /// <summary>
        /// Đọc thông tin Bách Bảo Rương từ DB
        /// </summary>
        public void ReadSeashellCircleParamFromDB()
        {
            this._LastSeashellCircleStopPos = this.GetValueOfForeverRecore(ForeverRecord.SeashellCircle_LastSeashellCircleStopPos);
            this._LastSeashellCircleStage = this.GetValueOfForeverRecore(ForeverRecord.SeashellCircle_LastSeashellCircleStage);
            this._LastSeashellCircleBet = this.GetValueOfForeverRecore(ForeverRecord.SeashellCircle_LastSeashellCircleBet);
        }

        /// <summary>
        /// Lưu lại thông tin Bách Bảo Rương vào DB
        /// </summary>
        public void SaveSeashellCircleParamToDB()
        {
            this.SetValueOfForeverRecore(ForeverRecord.SeashellCircle_LastSeashellCircleStopPos, this._LastSeashellCircleStopPos);
            this.SetValueOfForeverRecore(ForeverRecord.SeashellCircle_LastSeashellCircleStage, this._LastSeashellCircleStage);
            this.SetValueOfForeverRecore(ForeverRecord.SeashellCircle_LastSeashellCircleBet, this._LastSeashellCircleBet);
        }

        #endregion Bách Bảo Rương

        #region Vòng quay may mắn
        /// <summary>
        /// Tổng số lượt đã quay Vòng quay may mắn
        /// </summary>
        public int LuckyCircle_TotalTurn
        {
            get
            {
                return this.GetValueOfForeverRecore(ForeverRecord.LuckyCircle_TotalTurn);
            }
            set
            {
                this.SetValueOfForeverRecore(ForeverRecord.LuckyCircle_TotalTurn, value);
            }
        }

        /// <summary>
        /// Vị trí dừng lần cuối trong Vòng quay may mắn
        /// </summary>
        public int LuckyCircle_LastStopPos { get; set; } = -1;

        /// <summary>
        /// Đánh dấu vật phẩm nhận được từ vòng quay may mắn là khóa hay không
        /// </summary>
        public bool LuckyCircle_AwardBound { get; set; } = true;

        /// <summary>
        /// Thời điểm thao tác Vòng quay may mắn trước
        /// </summary>
        public long LuckyCircle_LastTicks { get; set; }
        #endregion

        #region Vòng quay may mắn - đặc biệt
        /// <summary>
        /// Tổng số lượt đã quay Vòng quay may mắn - đặc biệt
        /// </summary>
        public int TurnPlate_TotalTurn
        {
            get
            {
                return this.GetValueOfForeverRecore(ForeverRecord.TurnPlate_TotalTurn);
            }
            set
            {
                this.SetValueOfForeverRecore(ForeverRecord.TurnPlate_TotalTurn, value);
            }
        }

        /// <summary>
        /// Vị trí dừng lần cuối trong Vòng quay may mắn - đặc biệt
        /// </summary>
        public int TurnPlate_LastStopPos { get; set; } = -1;

        /// <summary>
        /// Thời điểm thao tác Vòng quay may mắn trước
        /// </summary>
        public long TurnPlate_LastTicks { get; set; }
        #endregion

        #region Chúc phúc
        private long _LastPrayTicks = 0;
        /// <summary>
        /// Thời điểm lần trước quay chúc phúc
        /// </summary>
        public long LastPrayTicks
        {
            get
            {
                return this._LastPrayTicks;
            }
            set
            {
                this._LastPrayTicks = value;
            }
        }

        private List<string> _LastPrayResult = null;
        /// <summary>
        /// Kết quả chúc phúc lần trước
        /// </summary>
        public List<string> LastPrayResult
        {
            get
            {
                return this._LastPrayResult;
            }
            set
            {
                this._LastPrayResult = value;
            }
        }

        /// <summary>
        /// Đọc dữ liệu chúc phúc từ DB
        /// </summary>
        public void ReadPrayDataFromDB()
        {
            try
            {
                string paramString = Global.GetRoleParamsStringWithNullFromDB(this, RoleParamName.PrayData);
                if (string.IsNullOrEmpty(paramString))
                {
                    this._LastPrayResult = null;
                }
                else
                {
                    string[] fields = paramString.Split('_');
                    /// Nếu có ký tự không hợp lệ
                    foreach (string str in fields)
                    {
                        int x = int.Parse(str);
                        /// Toác
                        if (x < 1 || x > 5)
                        {
                            throw new Exception();
                        }
                    }
                    this._LastPrayResult = fields.ToList();
                }
            }
            catch (Exception)
            {
                this._LastPrayResult = null;
            }
        }

        /// <summary>
        /// Lưu dữ liệu quay chúc phúc lần trước vào DB
        /// </summary>
        public void SavePrayDataToDB()
        {
            Global.SaveRoleParamsStringWithNullToDB(this, RoleParamName.PrayData, string.Format("{0}", this._LastPrayResult == null ? "" : string.Join("_", this._LastPrayResult)), true);
        }
        #endregion

        #region NHIEMVU

        private List<QuestInfo> _GetQuestInfo = new List<QuestInfo>();

        public List<QuestInfo> GetQuestInfo()
        {
            lock (_GetQuestInfo)
            {
                return _GetQuestInfo;
            }
        }

        public void AddQuestInfo(QuestInfo Quest)
        {
            lock (_GetQuestInfo)
            {
                _GetQuestInfo.Add(Quest);
            }
        }


  

        #endregion NHIEMVU

        #region Vinh dự
        private int _WorldMartial = 0;
        /// <summary>
        /// Vinh dự võ lâm liên đấu
        /// </summary>
        public int WorldMartial
        {
            get
            {
                return this._WorldMartial;
            }
            set
            {
                lock (this)
                {
                    this._WorldMartial = value;
                }
                /// Thông báo vinh dự võ lâm liên đấu thay đổi
                KT_TCPHandler.NotifyMyselfPrestigeAndWorldHonorChanged(this);
            }
        }

        private int _WorldHonor = 0;
        /// <summary>
        /// Vinh dự võ lâm
        /// </summary>
        public int WorldHonor
        {
            get
            {
                return this._WorldHonor;
            }
            set
            {
                lock (this)
                {
                    this._WorldHonor = value;
                }
                /// Thông báo vinh dự võ lâm thay đổi
                KT_TCPHandler.NotifyMyselfPrestigeAndWorldHonorChanged(this);
            }
        }

        private int _FactionHonor = 0;
        /// <summary>
        /// Vinh dự môn phái
        /// </summary>
        public int FactionHonor
        {
            get
            {
                return this._FactionHonor;
            }
            set
            {
                lock (this)
                {
                    this._FactionHonor = value;
                }
                /// Thông báo vinh dự thi đấu môn phái thay đổi
                KT_TCPHandler.NotifyMyselfPrestigeAndWorldHonorChanged(this);
            }
        }

        #endregion

        #region Tài phú

        /// <summary>
        /// Tổng tài phú
        /// </summary>
        private long totalValue = 0;

        /// <summary>
        /// Thiết lập tổng tài phú
        /// </summary>
        /// <param name="value"></param>
        public void SetTotalValue(long value)
        {
            lock (this)
            {
                this.totalValue = value;
            }

            //Console.WriteLine(string.Format("[SET] {0} (ID: {1}), TotalValue = {2}", this.RoleName, this.RoleID, value));
            //LogManager.WriteLog(LogTypes.GameMapEvents, string.Format("{0} (ID: {1}), TotalValue = {2}, Source:\n{3}", this.RoleName, this.RoleID, this.GetTotalValue(), new System.Diagnostics.StackTrace().ToString()));
        }

        /// <summary>
        /// Trả về tổng tài phú
        /// </summary>
        /// <returns></returns>
        public long GetTotalValue()
        {
            //Console.WriteLine(string.Format("[GET] {0} (ID: {1}), TotalValue = {2}", this.RoleName, this.RoleID, this.totalValue));

            lock (this)
            {
                return this.totalValue;
            }
        }

        #endregion

        #region Danh vọng

        /// <summary>
        /// Danh vọng
        /// </summary>
        private List<ReputeInfo> Repute = new List<ReputeInfo>();

        /// <summary>
        /// Trả về danh sách danh vọng
        /// </summary>
        /// <returns></returns>
        public List<ReputeInfo> GetRepute()
        {
            return this.Repute;
        }

        /// <summary>
        /// Chuyển danh vọng thành chuỗi mã hóa
        /// </summary>
        public string ReputeInfoToString
        {
            get
            {
                byte[] ItemDataByteArray = DataHelper.ObjectToBytes(this.Repute);

                string ReputeInfoEncoding = Convert.ToBase64String(ItemDataByteArray);

                return ReputeInfoEncoding;
            }
        }

        /// <summary>
        /// Thiết lập danh sách danh vọng
        /// </summary>
        /// <param name="repute"></param>
        public void SetReputeInfo(List<ReputeInfo> repute)
        {
            lock (this.Repute)
            {
                this.Repute = repute;
            }
        }

        #endregion Danh vọng

        #region Danh hiệu
        #region Đặc biệt
        /// <summary>
        /// ID danh hiệu đặc biệt
        /// </summary>
        public int SpecialTitleID
        {
            get
            {
                return this.GetValueOfForeverRecore(ForeverRecord.SpecialTitleID);
            }
            private set
            {
                this.SetValueOfForeverRecore(ForeverRecord.SpecialTitleID, value);
            }
        }

        /// <summary>
        /// Thời điểm nhận danh hiệu đặc biệt (giờ)
        /// </summary>
        public int SpecialTitleInitHour
        {
            get
            {
                return this.GetValueOfForeverRecore(ForeverRecord.SpecialTitleInitHour);
            }
            private set
            {
                this.SetValueOfForeverRecore(ForeverRecord.SpecialTitleInitHour, value);
            }
        }

        /// <summary>
        /// Thiết lập danh hiệu đặc biệt tương ứng
        /// </summary>
        /// <param name="id"></param>
        public void SetSpecialTitle(int titleID)
        {
            /// Nếu không tồn tại
            if (!KTTitleManager.IsSpecialTitleExist(titleID))
            {
                return;
            }

            /// Lưu lại
            this.SpecialTitleID = titleID;
            /// Thời điểm bắt đầu
            this.SpecialTitleInitHour = KTGlobal.GetOffsetHour();

            /// Gửi về Client
            KT_TCPHandler.NotifyOthersMyCurrentRoleTitleChanged(this);
        }

        /// <summary>
        /// Xóa danh hiệu đặc biệt tương ứng hiện tại
        /// </summary>
        public void RemoveSpecialTitle()
        {
            /// Lưu lại
            this.SpecialTitleID = -1;
            /// Thời điểm bắt đầu
            this.SpecialTitleInitHour = -1;

            /// Gửi về Client
            KT_TCPHandler.NotifyOthersMyCurrentRoleTitleChanged(this);
        }

        /// <summary>
        /// Tự động xóa danh hiệu đặc biệt nếu hết hạn
        /// </summary>
        public void AutoRemoveTimeoutSpecialTitle()
        {
            /// Nếu không tồn tại
            if (this.SpecialTitleID == -1)
            {
                /// Bỏ qua
                return;
            }

            /// Thông tin
            KSpecialTitleXML data = KTTitleManager.GetSpecialTitleData(this.SpecialTitleID);
            /// Toác
            if (data == null)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu không có hạn
            if (data.Duration == -1)
            {
                /// Bỏ qua
                return;
            }

            /// Tổng số giờ
            int hours = KTGlobal.GetOffsetHour() - this.SpecialTitleInitHour;
            /// Nếu đã quá hạn
            if (hours >= data.Duration + 1)
            {
                /// Xóa
                this.RemoveSpecialTitle();
            }
        }
        #endregion


        /// <summary>
        /// Danh hiệu
        /// <para>Key: ID danh hiệu</para>
        /// <para>Value: Thời điểm nhận (đơn vị giờ), -1 nghĩa là vĩnh viễn</para>
        /// </summary>
        public ConcurrentDictionary<int, int> RoleTitles { get; set; }

        /// <summary>
        /// Chuỗi mã hóa danh hiệu để lưu vào DB
        /// </summary>
        public string RoleTitlesInfoString
        {
            get
            {
                List<string> titleString = new List<string>();
                foreach (KeyValuePair<int, int> pair in this.RoleTitles)
                {
                    titleString.Add(string.Format("{0}_{1}", pair.Key, pair.Value));
                }

                return string.Format("{0}|{1}", this.CurrentRoleTitleID, string.Join("|", titleString));
            }
        }

        /// <summary>
        /// ID danh hiệu hiện tại
        /// </summary>
        public int CurrentRoleTitleID { get; set; }

        /// <summary>
        /// Thêm danh hiệu tương ứng
        /// </summary>
        /// <param name="titleID"></param>
        public void AddRoleTitle(int titleID)
        {
            /// Nếu không tồn tại
            if (!KTTitleManager.IsTitleExist(titleID))
            {
                return;
            }
            /// Nếu trong danh sách đã tồn tại
            else if (this.RoleTitles.ContainsKey(titleID))
            {
                return;
            }

            /// Thêm vào danh sách
            this.RoleTitles[titleID] = KTGlobal.GetOffsetHour();

            /// Gửi về Client
            KT_TCPHandler.SendModifyMyselfCurrentRoleTitle(this, titleID, 1);
        }

        /// <summary>
        /// Xóa danh hiệu tương ứng
        /// </summary>
        /// <param name="titleID"></param>
        public void RemoveRoleTitle(int titleID)
        {
            /// Nếu không tồn tại
            if (!KTTitleManager.IsTitleExist(titleID))
            {
                return;
            }
            /// Nếu trong danh sách không tồn tại
            else if (!this.RoleTitles.ContainsKey(titleID))
            {
                return;
            }

            /// Xóa khỏi danh sách
            this.RoleTitles.TryRemove(titleID, out _);

            /// Gửi về Client
            KT_TCPHandler.SendModifyMyselfCurrentRoleTitle(this, titleID, -1);

            /// Nếu đang là danh hiệu hiện tại
            if (titleID == this.CurrentRoleTitleID)
            {
                /// Hủy danh hiệu hiện tại
                this.CurrentRoleTitleID = -1;
                /// Thông báo tới tất cả người chơi xung quanh
                KT_TCPHandler.NotifyOthersMyCurrentRoleTitleChanged(this);
            }

            /// Danh hiệu tương ứng
            KTitleXML titleData = KTTitleManager.GetTitleData(titleID);
            /// Nếu danh hiệu tồn tại
            if (titleData != null)
            {
                /// Thông báo hủy danh hiệu
                KTPlayerManager.ShowNotification(this, string.Format("Danh hiệu [{0}] đã bị hủy do hết thời hạn!", titleData.Text));
            }
        }

        /// <summary>
        /// Thiết lập làm danh hiệu hiện tại
        /// </summary>
        /// <param name="titleID"></param>
        public bool SetAsCurrentRoleTitle(int titleID)
        {
            /// Nếu không tồn tại
            if (!KTTitleManager.IsTitleExist(titleID))
            {
                return false;
            }
            /// Nếu trong danh sách không tồn tại
            else if (!this.RoleTitles.ContainsKey(titleID))
            {
                return false;
            }

            /// Thiết lập làm danh hiệu hiện tại
            this.CurrentRoleTitleID = titleID;

            /// Thông báo tới tất cả người chơi xung quanh
            KT_TCPHandler.NotifyOthersMyCurrentRoleTitleChanged(this);

            /// Thành công
            return true;
        }

        /// <summary>
        /// Tự động xóa các danh hiệu đã quá thời hạn
        /// </summary>
        public void AutoRemoveTimeoutTitles()
        {
            /// Danh hiệu thường
            List<int> keys = this.RoleTitles.Keys.ToList();
            /// Duyệt danh sách danh hiệu
            foreach (int key in keys)
            {
                /// Danh hiệu tương ứng
                if (!this.RoleTitles.TryGetValue(key, out int startHours))
                {
                    /// Xóa danh hiệu tương ứng
                    this.RemoveRoleTitle(key);
                    continue;
                }
                /// Thông tin danh hiệu tương ứng
                KTitleXML titleData = KTTitleManager.GetTitleData(key);
                /// Nếu không tồn tại
                if (titleData == null)
                {
                    /// Xóa danh hiệu tương ứng
                    this.RemoveRoleTitle(key);
                    continue;
                }

                /// Nếu tồn tại vĩnh viễn
                if (titleData.Duration == -1)
                {
                    /// Bỏ qua 
                    continue;
                }

                /// Thời gian lệch so với thời điểm hiện tại (Giờ)
                int hours = KTGlobal.GetOffsetHour() - startHours;
                /// Nếu quá số giờ
                if (hours >= titleData.Duration + 1)
                {
                    /// Xóa danh hiệu tương ứng
                    this.RemoveRoleTitle(key);
                }
            }
        }

        #endregion Danh hiệu

        #region Kỹ năng sống

        /// <summary>
        /// Danh sách kỹ năng sống
        /// </summary>
        private Dictionary<int, LifeSkillPram> LifeSkills = new Dictionary<int, LifeSkillPram>();

        /// <summary>
        /// Trả về danh sách kỹ năng sống tương ứng
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, LifeSkillPram> GetLifeSkills()
        {
            return this.LifeSkills;
        }

        /// <summary>
        /// Thiết lập danh sách kỹ năng sống
        /// </summary>
        /// <param name="lifeSkills"></param>
        public void SetLifeSkills(Dictionary<int, LifeSkillPram> lifeSkills)
        {
            lock (this.LifeSkills)
            {
                if (lifeSkills == null)
                {
                    this.LifeSkills = new Dictionary<int, LifeSkillPram>();
                    for (int i = 1; i < 12; i++)
                    {
                        LifeSkillPram param = new LifeSkillPram();
                        param.LifeSkillID = i;
                        param.LifeSkillLevel = 1;
                        param.LifeSkillExp = 0;
                        this.LifeSkills[i] = param;
                    }
                }
                else
                {
                    this.LifeSkills = lifeSkills;
                }
            }
        }

        /// <summary>
        /// Trả về thông tin kỹ năng sống tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LifeSkillPram GetLifeSkill(int id)
        {
            if (this.LifeSkills.TryGetValue(id, out LifeSkillPram lifeSkillParam))
            {
                return lifeSkillParam;
            }
            return null;
        }

        /// <summary>
        /// Thiết lập cấp độ kỹ năng sống tương ứng
        /// </summary>
        /// <param name="id"></param>
        /// <param name="level"></param>
        /// <param name="exp"></param>
        public void SetLifeSkillParam(int id, int level, int exp)
        {
            if (this.LifeSkills.TryGetValue(id, out LifeSkillPram lifeSkillParam))
            {
                lifeSkillParam.LifeSkillLevel = level;
                LifeSkillExp lifeSkillExp = ItemCraftingManager._LifeSkill.TotalExp.Where(x => x.Level == level).FirstOrDefault();
                if (lifeSkillExp == null)
                {
                    lifeSkillParam.LifeSkillExp = 0;
                }
                else
                {
                    if (exp < 0)
                    {
                        exp = 0;
                    }
                    if (exp > lifeSkillExp.Exp - 1)
                    {
                        exp = lifeSkillExp.Exp - 1;
                    }
                }

                /// Gửi thông báo về Client
                KT_TCPHandler.NotifySelfLifeSkillLevelAndExpChanged(this, id, level, exp);
            }
        }

        /// <summary>
        /// Chuyển kỹ năng sống thành dạng String để lưu vào DB
        /// </summary>
        public string LifeSkillToString
        {
            get
            {
                byte[] ItemDataByteArray = DataHelper.ObjectToBytes(this.LifeSkills);
                string LifeSkillEncoding = Convert.ToBase64String(ItemDataByteArray);
                return LifeSkillEncoding;
            }
        }

        private bool _IsCrafting = false;

        /// <summary>
        /// Có đang chế đồ không
        /// </summary>
        public bool IsCrafting
        {
            get
            {
                return this._IsCrafting;
            }
            set
            {
                this._IsCrafting = value;
            }
        }

        #endregion Kỹ năng sống

        #region Bạn bè

        /// <summary>
        /// Danh sách bạn bè
        /// </summary>
        public List<FriendData> FriendDataList
        {
            get { return _RoleDataEx.FriendDataList; }
            set { lock (this) _RoleDataEx.FriendDataList = value; }
        }

        /// <summary>
        /// Danh sách người chơi mà đối tượng đang yêu cầu kết bạn
        /// </summary>
        private readonly HashSet<int> AskingToBeFriendWith = new HashSet<int>();

        /// <summary>
        /// Kiểm tra đối tượng có đang yêu cầu kết bạn với người chơi tương ứng không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsAskingToBeFriendWith(KPlayer player)
        {
            return this.AskingToBeFriendWith.Contains(player.RoleID);
        }

        /// <summary>
        /// Thêm người chơi mà đối tượng đang yêu cầu kết bạn vào danh sách
        /// </summary>
        /// <param name="player"></param>
        public void AddAskingToBeFriend(KPlayer player)
        {
            /// Nếu đã tồn tại thì bỏ qua
            if (this.AskingToBeFriendWith.Contains(player.RoleID))
            {
                return;
            }
            /// Thêm vào danh sách
            this.AskingToBeFriendWith.Add(player.RoleID);
        }

        /// <summary>
        /// Xóa người chơi mà đối tượng đang yêu cầu kết bạn khỏi danh sách
        /// </summary>
        /// <param name="player"></param>
        public void RemoveAskingToBeFriend(KPlayer player)
        {
            /// Nếu không tồn tại thì bỏ qua
            if (!this.AskingToBeFriendWith.Contains(player.RoleID))
            {
                return;
            }
            this.AskingToBeFriendWith.Remove(player.RoleID);
        }

        #endregion Bạn bè

        #region Chat

        /// <summary>
        /// Thời điểm Chat lần trước tại kênh tương ứng
        /// </summary>
        private readonly Dictionary<ChatChannel, long> TickChat = new Dictionary<ChatChannel, long>();

        /// <summary>
        /// Có thể gửi tin nhắn Chat không
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool CanChat(ChatChannel channel, out long tickLeft)
        {
            /// Nếu bị cấm chat
            if (this.IsBannedChat)
            {
                tickLeft = 99999999;
                return false;
            }

            /// Lấy thời điểm Chat trước
            if (!this.TickChat.TryGetValue(channel, out long lastTick))
            {
                this.TickChat[channel] = 0;
                lastTick = 0;
            }

            /// Thời điểm hiện tại
            long currentTick = KTGlobal.GetCurrentTimeMilis();
            /// Khoảng lệch
            long diffTick = currentTick - lastTick;

            switch (channel)
            {
                case ChatChannel.Near:
                    {
                        tickLeft = 1000 - diffTick;
                        return diffTick >= 1000;
                    }
                case ChatChannel.Team:
                    {
                        tickLeft = 500 - diffTick;
                        return diffTick >= 500;
                    }
                case ChatChannel.Private:
                    {
                        tickLeft = 500 - diffTick;
                        return diffTick >= 500;
                    }
                case ChatChannel.System:
                    {
                        tickLeft = 99999999;
                        return false;
                    }
                case ChatChannel.Faction:
                    {
                        tickLeft = 1000 - diffTick;
                        return diffTick >= 1000;
                    }
                case ChatChannel.Allies:
                    {
                        tickLeft = 1000 - diffTick;
                        return diffTick >= 1000;
                    }
                case ChatChannel.Global:
                    {
                        tickLeft = 120000 - diffTick;
                        return diffTick >= 120000;
                    }
                case ChatChannel.KuaFuLine:
                    {
                        tickLeft = 1000 - diffTick;
                        return diffTick >= 1000;
                    }
                case ChatChannel.Guild:
                    {
                        tickLeft = 1000 - diffTick;
                        return diffTick >= 1000;
                    }
                case ChatChannel.Special:
                    {
                        tickLeft = 10000 - diffTick;
                        return diffTick >= 10000;
                    }
                default:
                    {
                        tickLeft = 99999999;
                        return false;
                    }
            }
        }

        /// <summary>
        /// Ghi lại thời điểm chat tại kênh tương ứng
        /// </summary>
        /// <param name="channel"></param>
        public void RecordChatTick(ChatChannel channel)
        {
            this.TickChat[channel] = KTGlobal.GetCurrentTimeMilis();
        }

        #endregion Chat

        #region Dialog
        private long _LastClickDialog = 0;

        /// <summary>
        /// Thời điểm lần trước thực hiện Click chọn chức năng của NPCDialog và ItemDialog
        /// </summary>
        public long LastClickDialog
        {
            get
            {
                lock (this)
                {
                    return this._LastClickDialog;
                }
            }
            set
            {
                lock (this)
                {
                    this._LastClickDialog = value;
                }
            }
        }

        #endregion Dialog

        #region RoleDataEx

        /// <summary>
        /// Thực thể trung gian để giao tiếp với GAMEBD -> VÀ  TOÀN BỘ GAMECLIENT | TOÀN BỘ DỮ LIỆU GIAO TIẾP Ở ATRIBUILD PHẢI ĐẢM BẢO ĐỒNG BỘ VỚI ROLEDATAEX
        /// </summary>
        private RoleDataEx _RoleDataEx = null;

        /// <summary>
        /// Giá trị RoleDataEx
        /// </summary>
        /// <returns></returns>
        public RoleDataEx GetRoleData()
        {
            return _RoleDataEx;
        }

        /// <summary>
        /// Thiết lập RoleDataEx
        /// </summary>
        public RoleDataEx RoleData
        {
            set
            {
                lock (this)
                {
                    this._RoleDataEx = value;
                    /// Khởi tạo quản lý vật phẩm
                    this.GoodsData = new KPlayerGoods(this, this._RoleDataEx.GoodsDataList);
                }
            }
        }

        #endregion RoleDataEx

        #region Define

        private Dictionary<int, long> _LastDBCmdTicksDict = new Dictionary<int, long>();

        /// <summary>
        /// Lần cuối tick với DB Service
        /// </summary>
        public Dictionary<int, long> LastDBCmdTicksDict
        {
            get { return _LastDBCmdTicksDict; }
            set { lock (this) _LastDBCmdTicksDict = value; }
        }

        /// <summary>
        /// Thời điểm Tick cập nhật thời gian Online lần trước
        /// </summary>
        public long LastHeartTicks { get; set; }


        /// <summary>
        /// Toàn bộ dic Sysmboy của ROLE
        /// </summary>
        public Dictionary<string, RoleParamsData> RoleParamsDict
        {
            get { return _RoleDataEx.RoleParamsDict; }
            set { lock (this) _RoleDataEx.RoleParamsDict = value; }
        }

        /// <summary>
        /// ID thư mới
        /// </summary>
        public int LastMailID
        {
            get { return _RoleDataEx.LastMailID; }
            set { lock (this) _RoleDataEx.LastMailID = value; }
        }



        /// <summary>
        /// Hướng quay mặt
        /// </summary>
        public int RoleDirection
        {
            get
            {
                return this._RoleDataEx.RoleDirection;
            }

            set
            {
                lock (this)
                {
                    this._RoleDataEx.RoleDirection = value;
                }
            }
        }

        /// <summary>
        /// Avata nhân vật
        /// </summary>
        public int RolePic
        {
            get
            {
                return this._RoleDataEx.RolePic;
            }
            set
            {
                lock (this)
                {
                    this._RoleDataEx.RolePic = value;
                }
            }
        }

        /// <summary>
        /// Môn phái của người chơi
        /// </summary>
        public KPlayerFaction m_cPlayerFaction { get; private set; }

        /// <summary>
        /// Danh sách đối tượng xung quanh
        /// <para>0: Cần xóa</para>
        /// <para>1: Giữ</para>
        /// <para>2: Cần thêm</para>
        /// <para>3: Tàng hình hoặc gì đó nên không thêm vào danh sách cũng không xóa khỏi danh sách</para>
        /// </summary>
        public ConcurrentDictionary<IObject, byte> VisibleGrid9Objects { get; } = new ConcurrentDictionary<IObject, byte>();

        /// <summary>
        /// 更新某个角色参数命令的时间
        /// </summary>
        private Dictionary<string, long> _LastDBRoleParamCmdTicksDict = new Dictionary<string, long>();

        /// <summary>
        /// 更新某个角色参数命令的时间
        /// </summary>
        public Dictionary<string, long> LastDBRoleParamCmdTicksDict
        {
            get { return _LastDBRoleParamCmdTicksDict; }
            set { lock (this) _LastDBRoleParamCmdTicksDict = value; }
        }

        /// <summary>
        /// 更新某个装备耐久度命令的时间
        /// </summary>
        private Dictionary<int, long> _LastDBEquipStrongCmdTicksDict { get; set; } = new Dictionary<int, long>();

        /// <summary>
        /// 更新某个装备耐久度命令的时间
        /// </summary>
        public Dictionary<int, long> LastDBEquipStrongCmdTicksDict
        {
            get { return _LastDBEquipStrongCmdTicksDict; }
            set { lock (this) _LastDBEquipStrongCmdTicksDict = value; }
        }



        /// <summary>
        /// Thời điểm truy vấn danh sách Top chiến đội trong Võ lâm liên đấu lần trước
        /// </summary>
        public long LastQueryTeamBattleTicks { get; set; }


        #endregion Define

        #region Tìm kiếm

        private long _LastBrowsePlayersTick = 0;

        /// <summary>
        /// Thời điểm lần trước tìm kiếm người chơi khác thông qua khung tìm kiếm
        /// </summary>
        public long LastBrowsePlayersTick
        {
            get
            {
                return this._LastBrowsePlayersTick;
            }
            set
            {
                this._LastBrowsePlayersTick = value;
            }
        }

        #endregion Tìm kiếm

        #region Tổ đội

        private int _TeamID;

        /// <summary>
        /// ID nhóm
        /// </summary>
        public int TeamID
        {
            get { return _TeamID; }
            set { _TeamID = value; }
        }

        /// <summary>
        /// Nhóm trưởng
        /// </summary>
        public KPlayer TeamLeader
        {
            get
            {
                return KTTeamManager.GetTeamLeader(this.TeamID);
            }
        }

        /// <summary>
        /// Danh sách thành viên nhóm
        /// </summary>
        public List<KPlayer> Teammates
        {
            get
            {
                return KTTeamManager.GetTeamPlayers(this.TeamID);
            }
        }

        #endregion Tổ đội

        #region Nhặt đồ
        private long _LastPickUpDropItemTicks = 0;
        /// <summary>
        /// Thời điểm nhặt đồ rơi dưới đất lần trước
        /// </summary>
        public long LastPickUpDropItemTicks
        {
            get
            {
                lock (this)
                {
                    return this._LastPickUpDropItemTicks;
                }
            }
            set
            {
                lock (this)
                {
                    this._LastPickUpDropItemTicks = value;
                }
            }
        }
        #endregion

        #region Thư

        private long _LastCheckMailTick = 0;

        /// <summary>
        /// Thời điểm đọc thư lần cuối
        /// </summary>
        public long LastCheckMailTick
        {
            get
            {
                return this._LastCheckMailTick;
            }
            set
            {
                this._LastCheckMailTick = value;
            }
        }

        #endregion Thư

        #region THETHANG

        /// <summary>
        /// Thẻ tháng của người chơi
        /// </summary>
        public YueKaDetail YKDetail = new YueKaDetail();

        #endregion THETHANG

        #region TONGKIM

        public int ReviceMedicineOfDay { get; set; }

        #endregion TONGKIM

        #region PK Mode

        private long _LastSiteSubPKPointTicks = 0;

        /// <summary>
        /// Thời gian gần đây nhất giảm PK
        /// </summary>
        public long LastSiteSubPKPointTicks
        {
            get
            {
                return this._LastSiteSubPKPointTicks;
            }
            set
            {
                lock (this)
                {
                    this._LastSiteSubPKPointTicks = value;
                }
            }
        }

        private long _LastChangePKModeToFight = 0;

        /// <summary>
        /// Thời gian lần trước thay đổi trạng thái chiến đấu khác Luyện công
        /// </summary>
        public long LastChangePKModeToFight
        {
            get
            {
                return this._LastChangePKModeToFight;
            }
            set
            {
                lock (this)
                {
                    this._LastChangePKModeToFight = value;
                }
            }
        }

        #endregion PK Mode

        #region CacheData
        /// <summary>
        /// Thời gian làm mới cửa hàng gần đây
        /// </summary>
        public long LastRefreshShopTick { get; set; } = 0;

        /// <summary>
        /// Thời gina add Friend Tick
        /// </summary>
        public long[] LastAddFriendTicks { get; set; } = new long[] { 0, 0, 0 };

        #endregion CacheData

        #region Bang hội

        /// <summary>
        /// Tên bang hội
        /// </summary>
        public string GuildName
        {
            get
            {
                return this._RoleDataEx.GuildName;
            }
            set
            {
                lock (this)
                {
                    this._RoleDataEx.GuildName = value;
                }
            }
        }

        /// <summary>
        /// Thời gina tick của Bang HỘI
        /// </summary>
        public long _AddBHMemberTicks = 0;

        /// <summary>
        /// ID bang hội
        /// </summary>
        public int GuildID
        {
            get
            {
                int tmpVar = this._RoleDataEx.GuildID;
                return tmpVar;
            }

            set
            {
                this._RoleDataEx.GuildID = value;
            }
        }

        /// <summary>
        /// Cấp bậc bang hội
        /// </summary>
        public int GuildRank
        {
            get
            {
                return this._RoleDataEx.GuildRank;
            }
            set
            {
                lock (this)
                {
                    this._RoleDataEx.GuildRank = value;
                }
            }
        }

        /// <summary>
        /// Tiền bang hội
        /// </summary>
        public int RoleGuildMoney
        {
            get
            {
                return this._RoleDataEx.RoleGuildMoney;
            }
            set
            {
                lock (this)
                {
                    this._RoleDataEx.RoleGuildMoney = value;
                }
            }
        }

        /// <summary>
        /// Danh hiệu bang hội
        /// </summary>
        public string GuildTitle
        {
            get
            {
                /// Nếu không có bang
                if (this.GuildID <= 0)
                {
                    return "";
                }

                /// Danh hiệu theo chức vụ
                string guildRankName = "";

                if (this.GuildRank <= (int)GameServer.KiemThe.Entities.GuildRank.Member)
                {
                    guildRankName = "Thành viên";
                }
                else if (this.GuildRank == (int)GameServer.KiemThe.Entities.GuildRank.Master)
                {
                    guildRankName = "Bang chủ";
                }
                else if (this.GuildRank == (int)GameServer.KiemThe.Entities.GuildRank.ViceMaster)
                {
                    guildRankName = "Phó bang chủ";
                }
                else if (this.GuildRank == (int)GameServer.KiemThe.Entities.GuildRank.ViceAmbassador)
                {
                    guildRankName = "Đường chủ";
                }
                else if (this.GuildRank == (int)GameServer.KiemThe.Entities.GuildRank.Ambassador)
                {
                    guildRankName = "Trưởng lão";
                }
                else if (this.GuildRank == (int)GameServer.KiemThe.Entities.GuildRank.Elite)
                {
                    guildRankName = "Tinh anh";
                }

                /// Trả về kết quả
                return string.Format("[Bang hội] {0} - {1}", this.GuildName, guildRankName);
            }
        }

        /// <summary>
        /// Quan hàm
        /// </summary>
        public int OfficeRank
        {
            get
            {
                return this._RoleDataEx.OfficeRank;
            }
        }

        #endregion Bang hội

        #region Gia Tộc

        /// <summary>
        /// Tên bang hội
        /// </summary>
        public string FamilyName
        {
            get
            {
                return "";
            }
            set
            {
                lock (this)
                {

                }
            }
        }

        /// <summary>
        /// ID bang hội
        /// </summary>
        public int FamilyID
        {
            get
            {
                int tmpVar = 0;
                return tmpVar;
            }

            set
            {

            }
        }

        /// <summary>
        /// Cấp bậc bang hội
        /// </summary>
        public int FamilyRank
        {
            get
            {
                return 0;
            }
            set
            {
                //lock (this)
                //{
                //    this._RoleDataEx.FamilyRank = value;
                //}
            }
        }

        /// <summary>
        /// Danh hiệu bang hội
        /// </summary>
        public string FamilyTitle
        {
            get
            {
                /// Nếu không có tộc
                if (this.FamilyID <= 0)
                {
                    return "";
                }

                /// Danh hiệu theo chức vụ
                string guildRankName = "";

                if (this.FamilyRank <= (int)GameServer.KiemThe.Entities.FamilyRank.Member)
                {
                    guildRankName = "Thành viên";
                }
                else if (this.FamilyRank == (int)GameServer.KiemThe.Entities.FamilyRank.Master)
                {
                    guildRankName = "Tộc trưởng";
                }
                else if (this.FamilyRank == (int)GameServer.KiemThe.Entities.FamilyRank.ViceMaster)
                {
                    guildRankName = "Tộc phó";
                }

                /// Trả về kết quả
                return string.Format("<color=#ffb92e>[Gia tộc] {0} - {1}</color>", this.FamilyName, guildRankName);
            }
        }

        #endregion Gia Tộc

        #region TIENTEINGAME

        /// <summary>
        /// USER MONEY LOCK
        /// </summary>
        private object _TokenMutex = new object();

        /// <summary>
        ///  USER MONEY LOCK
        /// </summary>
        public object TokenMutex
        {
            get { return _TokenMutex; }
        }

        /// <summary>
        /// Bạc khóa
        /// </summary>
        public int BoundMoney
        {
            get { return _RoleDataEx.BoundMoney; }
            set { lock (this) _RoleDataEx.BoundMoney = value; }
        }

        /// <summary>
        /// Điểm Uy Danh Giang hồ
        /// </summary>
        public int Prestige
        {
            get
            {
                return this._RoleDataEx.Prestige;
            }
            set
            {
                lock (this)
                {
                    this._RoleDataEx.Prestige = value;
                }
                /// Thông báo uy danh thay đổi
                KT_TCPHandler.NotifyMyselfPrestigeAndWorldHonorChanged(this);
            }
        }

        /// <summary>
        /// Đối tượng Mutex dùng trong thao tác thêm bạc lưu trữ ở thương khố cho người chơi
        /// </summary>
        public object StoreMoneyMutex { get; } = new object();

        /// <summary>
        /// Bạc lưu trữ ở thương khố
        /// </summary>
        public int StoreMoney
        {
            get { return _RoleDataEx.Store_Money; }
            set { lock (this) _RoleDataEx.Store_Money = value; }
        }

        /// <summary>
        /// Đồng mua trên kỳ trân các
        /// </summary>
        public int Token
        {
            get { return _RoleDataEx.Token; }
            set { lock (this) _RoleDataEx.Token = value; }
        }

        /// <summary>
        /// LOCKK BẠC KHÓA
        /// </summary>
        private object _BoundTokenMutex = new object();

        /// <summary>
        /// LOCKK BẠC KHÓA
        /// </summary>
        private object _MoneyLock = new object();

        public object GetMoneyLock
        {
            get { return _MoneyLock; }
        }

        /// <summary>
        ///  LOCKK BẠC KHÓA => Việc Lock này đảm bảo cho client chỉ có 1 request thay đổi giá trị 1 lúc
        /// </summary>
        public object BoundTokenMutex
        {
            get { return _BoundTokenMutex; }
        }

        /// <summary>
        /// Đòng khóa
        /// </summary>
        public int BoundToken
        {
            get { return _RoleDataEx.BoundToken; }
            set { lock (this) _RoleDataEx.BoundToken = value; }
        }

        /// <summary>
        /// Lock BoundMoney  => Việc Lock này đảm bảo cho client chỉ có 1 request thay đổi giá trị 1 lúc
        /// </summary>
        private object _BoundMoneyMutex = new object();

        /// <summary>
        ///   Lock BoundMoney   => Việc Lock này đảm bảo cho client chỉ có 1 request thay đổi giá trị 1 lúc
        /// </summary>
        public object BoundMoneyMutex
        {
            get { return _BoundMoneyMutex; }
        }

        /// <summary>
        /// Bạc thường
        /// </summary>
        public int Money
        {
            get
            {
                int tmpVar = this._RoleDataEx.Money;
                return tmpVar;
            }

            set
            {
                this._RoleDataEx.Money = value;
            }
        }
        #endregion TIENTEINGAME

        #region Vị trí
        /// <summary>
        /// Thời điểm lần trước chết
        /// </summary>
        public long LastDeadTicks { get; set; }

        /// <summary>
        /// ZONE ID CỦA MÁY CHỦ
        /// </summary>
        public int ZoneID
        {
            get
            {
                //lock (this)
                {
                    return this._RoleDataEx.ZoneID;
                }
            }
            set
            {
                lock (this)
                {
                    this._RoleDataEx.ZoneID = value;
                }
            }
        }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public int MapCode
        {
            get
            {
                if (this._RoleDataEx == null)
                {
                    return -1;
                }
                return this._RoleDataEx.MapCode;
            }
            set
            {
                if (this._RoleDataEx == null)
                {
                    return;
                }
                this._RoleDataEx.MapCode = value;
            }
        }

        /// <summary>
        /// Tọa độ X hiện tại
        /// </summary>
        public int PosX
        {
            get
            {
                if (this._RoleDataEx == null)
                {
                    return -1;
                }
                return this._RoleDataEx.PosX;
            }
            set
            {
                if (this._RoleDataEx == null)
                {
                    return;
                }
                this._RoleDataEx.PosX = value;
            }
        }

        /// <summary>
        /// Tọa độ Y hiện tại
        /// </summary>
        public int PosY
        {
            get
            {
                if (this._RoleDataEx == null)
                {
                    return -1;
                }
                return this._RoleDataEx.PosY;
            }
            set
            {
                if (this._RoleDataEx == null)
                {
                    return;
                }
                this._RoleDataEx.PosY = value;
            }
        }


        /// <summary>
        /// Thời điểm Tick StoryBoard lần trước
        /// </summary>
        public long LastStoryBoardTicks { get; set; }

        /// <summary>
        /// Đợi chuyển bản đồ
        /// </summary>
        public bool WaitingForChangeMap { get; set; } = false;

        /// <summary>
        /// ID bản đồ đang đợi dịch đến
        /// </summary>
        public int WaitingChangeMapCode { get; set; }

        /// <summary>
        /// Vị trí X đang chờ dịch sang bản đồ đích
        /// </summary>
        public int WaitingChangeMapPosX { get; set; }

        /// <summary>
        /// Vị trí Y đang chờ dịch sang bản đồ đích
        /// </summary>
        public int WaitingChangeMapPosY { get; set; }

        /// <summary>
        /// Vị trí lần trước được chuyển bởi hệ thống (thông qua hàm chuyển bản đồ hoặc thay đổi vị trí)
        /// </summary>
        public Point LastChangedPosition { get; set; } = new Point(0, 0);

        /// <summary>
        /// Thời điểm thay đổi vị trí lần trước
        /// </summary>
        public long LastChangePositionTicks { get; set; }

        /// <summary>
        /// Thời điểm cập nhật danh sách quái đặc biệt trong bản đồ lần trước
        /// </summary>
        public long LastUpdateLocalMapMonsterTicks { get; set; }

        /// <summary>
        /// Thời điểm Tick kiểm tra vật phẩm hết hạn trước
        /// </summary>
        public long LastGoodsLimitUpdateTicks { get; set; }

        #endregion ROLEPOSTION

        #region Task_NHIỆMVU

        /// <summary>
        /// Nhiệm vụ
        /// </summary>
        public List<TaskData> TaskDataList
        {
            get { return _RoleDataEx.TaskDataList; }
            set { lock (this) _RoleDataEx.TaskDataList = value; }
        }

        public List<OldTaskData> OldTasks
        {
            get { return _RoleDataEx.OldTasks; }
            set { lock (this) _RoleDataEx.OldTasks = value; }
        }

        /// <summary>
        /// Nhiệm vụ hàng ngày
        /// </summary>
        public DailyTaskData YesterdayDailyTaskData = null;

        /// <summary>
        /// Nhiệm vụ hàng ngày 2
        /// </summary>
        public DailyTaskData YesterdayTaofaTaskData = null;

        /// <summary>
        /// ID Nhiệm vụ chính tuyến hiện tại
        /// </summary>
        public int MainTaskID
        {
            get { return _RoleDataEx.MainTaskID; }
            set { lock (this) _RoleDataEx.MainTaskID = value; }
        }



        #endregion Task_NHIỆMVU

        #region Kỹ năng
        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        public List<SkillData> SkillDataList
        {
            get { return _RoleDataEx.SkillDataList; }
            set { lock (this) _RoleDataEx.SkillDataList = value; }
        }

        /// <summary>
        /// Danh sách kỹ năng ở ô thiết lập nhanh
        /// </summary>
        public string MainQuickBarKeys
        {
            get { return _RoleDataEx.MainQuickBarKeys; }
            set { lock (this) _RoleDataEx.MainQuickBarKeys = value; }
        }

        /// <summary>
        /// Kỹ năng vòng sáng đang được sử dụng
        /// <para>Dạng: ID_TRẠNG THÁI, ID là ID kỹ năng, TRẠNG THÁI 0 tức là không kích hoạt, 1 tức là kích hoạt</para>
        /// </summary>
        public string OtherQuickBarKeys
        {
            get { return _RoleDataEx.OtherQuickBarKeys; }
            set { lock (this) _RoleDataEx.OtherQuickBarKeys = value; }
        }

        #endregion Kỹ năng

        #region LoginRecore

        private bool _FirstPlayStart = true;

        /// <summary>
        ///  Đăng nhập lần đầu
        /// </summary>
        public bool FirstPlayStart
        {
            get { return _FirstPlayStart; }
            set { lock (this) _FirstPlayStart = value; }
        }

        /// <summary>
        /// Thời gian bảo vệ sức khỏe
        /// </summary>
        private int _AntiAddictionTimeType = 0;

        /// <summary>
        /// Thời gian bảo vệ sức khỏe
        /// </summary>
        public int AntiAddictionTimeType
        {
            get { return _AntiAddictionTimeType; }
            set { lock (this) _AntiAddictionTimeType = value; }
        }

        /// <summary>
        /// Thời điểm bắt đầu bị cấm chat
        /// </summary>
        public long BanChatStartTime
        {
            get { return _RoleDataEx.BanChatStartTime; }
            set { lock (this) _RoleDataEx.BanChatStartTime = value; }
        }

        /// <summary>
        /// Thời gian duy trì cấm chat
        /// </summary>
        public long BanChatDuration
        {
            get { return _RoleDataEx.BanChatDuration; }
            set { lock (this) _RoleDataEx.BanChatDuration = value; }
        }

        /// <summary>
        /// Có đang bị cấm Chat không
        /// </summary>
        public bool IsBannedChat
        {
            get
            {
                /// Nếu bị Ban vĩnh viễn
                if (this.BanChatDuration == -1)
                {
                    /// Toác
                    return true;
                }
                /// Trả về kết quả
                return KTGlobal.GetCurrentTimeMilis() - this.BanChatStartTime < this.BanChatDuration;
            }
        }

        /// <summary>
        /// Danh sách chức năng bị cấm
        /// </summary>
        private Dictionary<int, BanUserByType> BannedList
        {
            get
            {
                return this._RoleDataEx.BannedList;
            }
            set
            {
                lock (this._RoleDataEx)
                {
                    this._RoleDataEx.BannedList = value;
                }
            }
        }

        /// <summary>
        /// Kiểm tra bản thân có đang bị cấm chức năng tương ứng không
        /// </summary>
        /// <param name="type"></param>
        /// <param name="timeLeftSec"></param>
        /// <returns></returns>
        public bool IsBannedFeature(RoleBannedFeature type, out int timeLeftSec)
        {
            /// Thời gian còn lại
            timeLeftSec = 0;
            /// Không có danh sách cấm
            if (this.BannedList == null)
            {
                /// Không bị cấm
                return false;
            }
            /// Nếu không tồn tại
            if (!this.BannedList.TryGetValue((int) type, out BanUserByType banData))
            {
                /// Không bị cấm
                return false;
            }

            /// Nếu bị Ban vĩnh viễn
            if (banData.Duration == -1)
            {
                /// Toác
                return true;
            }
            /// Thời gian còn lại
            long timeLeft = banData.Duration - (KTGlobal.GetCurrentTimeMilis() - banData.StartTime);
            timeLeftSec = (int) timeLeft / 1000;
            /// Trả về kết quả
            return KTGlobal.GetCurrentTimeMilis() - banData.StartTime < banData.Duration;
        }

        /// <summary>
        /// Xóa cấm chức năng tương ứng
        /// </summary>
        /// <param name="type"></param>
        public void RemoveBanFeature(RoleBannedFeature type)
        {
            /// Không có danh sách cấm
            if (this.BannedList == null)
            {
                /// Bỏ qua
                return;
            }
            /// Nếu không tồn tại
            if (!this.BannedList.TryGetValue((int) type, out BanUserByType banData))
            {
                /// Bỏ qua
                return;
            }

            /// Xóa khỏi danh sách
            this.BannedList.Remove((int) type);
        }

        /// <summary>
        /// Thêm chức năng bị cấm tương ứng
        /// </summary>
        /// <param name="type"></param>
        /// <param name="startTicks"></param>
        /// <param name="durationTicks"></param>
        /// <param name="bannedBy"></param>
        public void AddBanFeature(RoleBannedFeature type, long startTicks, long durationTicks, string bannedBy)
        {
            /// Toác
            if (this.BannedList == null)
            {
                this.BannedList = new Dictionary<int, BanUserByType>();
            }

            /// Nếu không tồn tại
            if (!this.BannedList.ContainsKey((int) type))
            {
                /// Tạo mới
                this.BannedList[(int) type] = new BanUserByType()
                {
                    BanType = (int) type,
                    RoleID = this.RoleID,
                    StartTime = startTicks,
                    Duration = durationTicks,
                    BannedBy = bannedBy,
                };
            }
            /// Nếu đã tồn tại
            else
            {
                /// Thay thế
                this.BannedList[(int) type].StartTime = startTicks;
                this.BannedList[(int) type].Duration = durationTicks;
                this.BannedList[(int) type].BannedBy = bannedBy;
            }
        }

        /// <summary>
        /// Tổng số lần đăng nhập
        /// </summary>
        public int LoginNum
        {
            get { return _RoleDataEx.LoginNum; }
            set { lock (this) _RoleDataEx.LoginNum = value; }
        }

        /// <summary>
        /// Tổng thời gina đăng nhập
        /// </summary>
        public int TotalOnlineSecs
        {
            get { return _RoleDataEx.TotalOnlineSecs; }
            set { lock (this) _RoleDataEx.TotalOnlineSecs = value; }
        }

        /// <summary>
        /// Thời gian offline gần đây
        /// </summary>
        public long LastOfflineTime
        {
            get { return _RoleDataEx.LastOfflineTime; }
            set { lock (this) _RoleDataEx.LastOfflineTime = value; }
        }

        /// <summary>
        /// Thời gian đăng ký
        /// </summary>
        public long RegTime
        {
            get { return _RoleDataEx.RegTime; }
            set { lock (this) _RoleDataEx.RegTime = value; }
        }

        /// <summary>
        /// Ngày đăng nhập
        /// </summary>
        private int _LoginDayID = TimeUtil.NowDateTime().DayOfYear;

        /// <summary>
        /// ID ngày đăng nhập
        /// </summary>
        public int LoginDayID
        {
            get { return _LoginDayID; }
            set { lock (this) _LoginDayID = value; }
        }

        #endregion LoginRecore

        #region GIAODICH_TRADE



        private int _ExchangeID;

        /// <summary>
        /// Id giao dịch
        /// </summary>
        public int ExchangeID
        {
            get { return _ExchangeID; }
            set { lock (this) _ExchangeID = value; }
        }

        private long _ExchangeTicks;

        /// <summary>
        /// Thời gian giao dịch gần đây
        /// </summary>
        public long ExchangeTicks
        {
            get { return _ExchangeTicks; }
            set { lock (this) _ExchangeTicks = value; }
        }

        #endregion GIAODICH_TRADE

        #region Danh sách Buff

        /// <summary>
        /// Toàn bộ dánh ách BUFFF hiện tại
        /// </summary>
        public List<BufferData> BufferDataList
        {
            get { lock (this) return _RoleDataEx.BufferDataList; }
            set { lock (this) _RoleDataEx.BufferDataList = value; }
        }

        #endregion Danh sách Buff

        #region ITEMINGAME

        /// <summary>
        /// Danh sách vật phẩm đang bày bán
        /// </summary>
        public List<GoodsData> SaleGoodsDataList
        {
            get { lock (this) return _RoleDataEx.SaleGoodsDataList; }
            set { lock (this) _RoleDataEx.SaleGoodsDataList = value; }
        }

        /// <summary>
        /// Quản lý danh sách vật phẩm có
        /// </summary>
        public KPlayerGoods GoodsData { get; private set; }

        /// <summary>
        /// Danh sách vật phẩm dùng trong khay dùng nhanh
        /// </summary>
        public string QuickItems
        {
            get { return _RoleDataEx.QuickItems; }
            set { lock (this) _RoleDataEx.QuickItems = value; }
        }

        #endregion ITEMINGAME

        #region Pet
        /// <summary>
        /// Danh sách pet
        /// </summary>
        public List<PetData> PetList
        {
            get
            {
                return this._RoleDataEx.Pets;
            }
            set
            {
                lock (this)
                {
                    this._RoleDataEx.Pets = value;
                }
            }
        }
        #endregion


        #region AntiSendMuttipleClick

        public long LastActionReChage = 0;

        public long LastRequestKTCoin = 0;

        public int KTCoin = 0;




        // Send vào để đánh dấu thằng đã click
        public void SendClick()
        {
            this.LastActionReChage = TimeUtil.NOW();
        }

        /// <summary>
        /// Check xem nó đã spam chưa
        /// </summary>
        /// <returns></returns>
        public bool IsSpamClick()
        {
            // Nếu thời gian thao tác chưa quá 1s thì thằng này đang spam

            long _TOTALTIME = TimeUtil.NOW() - this.LastActionReChage;

            if (_TOTALTIME < 1000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Đối tượng Mutex dùng khóa Lock
        /// </summary>
        public object PropPointMutex { get; } = new object();

        /// <summary>
        /// Action hiện tại
        /// </summary>
        public int CurrentAction
        {
            get { return (int)this.m_eDoing; }
            set
            {
                lock (this)
                {
                    if ((int)this.m_eDoing != value)
                    {
                        this.m_eDoing = (KE_NPC_DOING)value;
                    }
                }
            }
        }

        #region CLIENT_CONNECT_DISCONNECT

        /// <summary>
        /// Số lần kiểm tra
        /// </summary>
        private int _ClientHeartCount = 0;

        /// <summary>
        /// Số lần kiểm tra
        /// </summary>
        public int ClientHeartCount
        {
            get { return _ClientHeartCount; }
            set { lock (this) _ClientHeartCount = value; }
        }

        /// <summary>
        /// Lần kiểm tra gàn đây nhất
        /// </summary>
        private long _LastClientHeartTicks = TimeUtil.NOW();

        /// <summary>
        /// Lần kiểm tra gần đây nhất
        /// </summary>
        public long LastClientHeartTicks
        {
            get { return _LastClientHeartTicks; }
            set { lock (this) _LastClientHeartTicks = value; }
        }

        /// <summary>
        /// Close client step
        /// </summary>
        private int _ClosingClientStep = 0;

        /// <summary>
        /// Close client step
        /// </summary>
        public int ClosingClientStep
        {
            get { return _ClosingClientStep; }
            set { lock (this) _ClosingClientStep = value; }
        }

        #endregion CLIENT_CONNECT_DISCONNECT

        #region GIFTCODE

        /// <summary>
        /// Thời gian kích hoạt code
        /// </summary>
        public long LastActiveGiftCodeTicks { get; set; }

        #endregion GIFTCODE

        #region Túi đồ

        ///// <summary>
        ///// Túi phụ
        ///// </summary>
        //public List<GoodsData> _PortableGoodsDataList = null;

        //public List<GoodsData> PortableGoodsDataList
        //{
        //    get { lock (this) return _PortableGoodsDataList; }
        //    set { lock (this) _PortableGoodsDataList = value; }
        //}


        //{
        //    get
        //    {
        //        int tmpVar = _RoleDataEx.BagNum;
        //        return tmpVar;
        //    }

        //    set
        //    {
        //        _RoleDataEx.BagNum = value;
        //    }
        //}

        /// </summary>
        /// Thời giang đang mở túi
        /// </summary>
        private int _OpenPortableGridTime = 0;

        /// <summary>
        /// Thời gian mở túi
        /// </summary>
        public int OpenPortableGridTime
        {
            get { return _OpenPortableGridTime; }
            set { lock (this) _OpenPortableGridTime = value; }
        }



        /// <summary>
        /// Tọa độ mở kho
        /// </summary>
        public Point OpenPortableBagPoint;

        #endregion Túi đồ

        #region Phụ bản

        /// <summary>
        /// ID phụ bản
        /// </summary>
        private int _CopyMapID = -1;

        /// <summary>
        /// ID phụ bản
        /// </summary>
        public int CopyMapID
        {
            get { { return _CopyMapID; } }
            set { lock (this) _CopyMapID = value; }
        }

        #endregion Phụ bản

        #region Fuck lợi


        public RoleWelfare RoleWelfareData
        {
            get { return _RoleDataEx.RoleWelfare; }
            set { lock (this) _RoleDataEx.RoleWelfare = value; }
        }


        #endregion 

        #region Sự kiện liên quan đăng nhập
        /// <summary>
        /// Tổng số giây đã Online trong ngày
        /// </summary>
        public int DayOnlineSecond
        {
            get
            {
                int value = this.GetValueOfDailyRecore((int) DailyRecord.TodayOnlineSec);
                if (value == -1)
                {
                    value = 0;
                }
                return value;
            }
            set
            {
                this.SetValueOfDailyRecore((int) DailyRecord.TodayOnlineSec, value);
            }
        }

        /// <summary>
        /// Tổng số giây đã Online
        /// </summary>
        public int TotalOnlineSecond
        {
            get
            {
                int value = this.GetValueOfForeverRecore(ForeverRecord.TotalOnlineSec);
                if (value == -1)
                {
                    value = 0;
                }
                return value;
            }
            set
            {
                this.SetValueOfForeverRecore(ForeverRecord.TotalOnlineSec, value);
            }
        }

        /// <summary>
        /// Thời điểm cập nhật thời gian Online lần trước
        /// </summary>
        public long LastUpdateOnlineTimeTicks { get; set; }

        /// <summary>
        /// Quà login từ 1-7
        /// </summary>
        public int SeriesLoginNum { get; set; }

        /// <summary>
        /// Thời gian ủy thác bạch cầu hoàn tính bằng phút
        /// </summary>
        public int baijuwan { get; set; }

        /// <summary>
        /// Thời gina ủy thác đại bạch cầu hoàn tính bằng phút
        /// </summary>
        public int baijuwanpro { get; set; }

        #endregion Sự kiện liên quan đăng nhập
    }
}