namespace GameServer.KiemThe.Entities.Player
{
    /// <summary>
    /// Định nghĩa Captcha hiện tại của người chơi
    /// </summary>
    public class KPlayer_Captcha
    {
        /// <summary>
        /// Chuỗi Captcha
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Thời điểm bắt đầu
        /// </summary>
        public long StartTick { get; set; }

        /// <summary>
        /// Thời gian tồn tại (milis)
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Đã hết hạn chưa
        /// </summary>
        public bool IsOver
        {
            get
            {
                return KTGlobal.GetCurrentTimeMilis() - this.StartTick >= this.Duration;
            }
        }
    }
}
