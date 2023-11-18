namespace GameDBServer.Core
{
    /// <summary>
    /// Lớp này tính toán ra cái thời gian xử lý trung bình trong mỗi gói tin
    /// Để đo lường thời sử dụng trên 1 packet
    /// </summary>
    public class PorcessCmdMoniter
    {
        public int cmd;
        public int processNum = 0;
        public long processTotalTime = 0;
        public long processMaxTime = 0;
        public long maxWaitProcessTime = 0;

        /// <summary>
        /// Thời gian trung bình xử lý trên 1 packet
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="processTime"></param>
        public PorcessCmdMoniter(int cmd, long processTime)
        {
            this.cmd = cmd;
            processNum++;
            processTotalTime += processTime;
            processMaxTime = processTime;
        }

        /// <summary>
        /// Tổng thời gina thực thi
        /// </summary>
        /// <param name="processTime"></param>
        public void onProcessNoWait(long processTime)
        {
            processNum++;
            processTotalTime += processTime;
            if (processMaxTime >= processTime)
            {
                processMaxTime = processTime;
            }
        }

        /// <summary>
        /// Tính toán ra thời gian trung bình xử lý trên 1 packet
        /// </summary>
        /// <returns></returns>
        public long avgProcessTime()
        {
            return processNum > 0 ? processTotalTime / processNum : 0;
        }
    }
}