using System;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Trạng thái ngũ hành của đối tượng
    /// </summary>
    public class KNpcAttribGroup_State
    {
        /// <summary>
        /// Thời điểm bắt đầu
        /// </summary>
        public long StartTick { get; set; }
        
        /// <summary>
        /// Thời gian tồn tại trạng thái hiện tại (Frame)
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Thời gian gây thêm trạng thái hiện tại
        /// </summary>
        public int StateAddTime { get; set; }

        /// <summary>
        /// Giảm thời gian chịu trạng thái hiện tại
        /// </summary>
        public int StateRestTime { get; set; }

        /// <summary>
        /// Tăng tỷ lệ gây trạng thái hiện tại
        /// </summary>
        public int StateAddRate { get; set; }

        /// <summary>
        /// Giảm tỷ lệ bị trạng thái hiện tại
        /// </summary>
        public int StateRestRate { get; set; }

        /// <summary>
        /// Bỏ qua trạng thái
        /// </summary>
        public bool IgnoreRate { get; set; }

        /// <summary>
        /// Tham biến khác
        /// </summary>
        public int OtherParam { get; set; }

        /// <summary>
        /// Trạng thái
        /// </summary>
        public KE_STATE State { get; private set; }

        /// <summary>
        /// Đã quá thời gian tồn tại chưa
        /// </summary>
        public bool IsOver
		{
			get
			{
                /// Chưa thiết lập gì
                if (this.StartTick <= 0 || this.Duration <= 0)
				{
                    return true;
				}
                return KTGlobal.GetCurrentTimeMilis() - this.StartTick >= this.Duration * 1000 / 19;
			}
		}

        /// <summary>
        /// Tạo mới đối tượng
        /// </summary>
        /// <param name="state"></param>
        public KNpcAttribGroup_State(KE_STATE state)
        {
            this.State = state;
        }

        /// <summary>
        /// Xóa trạng thái
        /// </summary>
        public void ClearState()
        {
            this.StartTick = 0;
            this.Duration = 0;
        }

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("State {0}, AddTime = {1}, RestTime = {2}, AddRate = {3}, RestRate = {4}", Enum.GetName(typeof(KE_STATE), this.State), this.StateAddTime, this.StateRestTime, this.StateAddRate, this.StateRestRate);
        }
    }
}