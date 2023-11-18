using GameServer.Logic;
using Server.Tools;
using System;
using System.Threading.Tasks;

namespace GameServer.Core.Executor
{
    /// <summary>
    /// Interface Task
    /// </summary>
    public interface IKTAsyncTaskInterface
    {
        /// <summary>
        /// Thực thi
        /// </summary>
        void Run();
    }

    /// <summary>
    /// Hàm thực thi delay task bất đồng bộ async
    /// </summary>
    public class DelayAsyncTask : IKTAsyncTaskInterface
    {
        /// <summary>
        /// Sự kiện sẽ gọi đến sau khi hoàn tất Delay
        /// </summary>
        public EventHandler Callback { get; set; } = null;

        /// <summary>
        /// Tên Task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Dữ liệu đi kèm
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Người chơi tương ứng
        /// </summary>
        public KPlayer Player { get; set; }

        /// <summary>
        /// Tạo mới đối tượng sự kiện TaskDelay
        /// </summary>
        public DelayAsyncTask() { }

        /// <summary>
        /// Tạo mới đối tượng sự kiện TaskDelay
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        /// <param name="player"></param>
        /// <param name="callback"></param>
        public DelayAsyncTask(string name, object tag, KPlayer player, EventHandler callback)
        {
            this.Callback = callback;
            this.Name = name;
            this.Player = player;
            this.Tag = tag;
        }

        /// <summary>
        /// Thực thi sự kiện
        /// </summary>
        public void Run()
        {
            try
            {
                this.Callback?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "Execute DelayTask got Exception", false);
                //Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Chuyển đối tượng về dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DelayTask - PlayerID: {0}, TaskName: {1}", this.Player == null ? -1 : this.Player.RoleID, this.Name);
        }
    }



    public class DelayFuntionAsyncTask : IKTAsyncTaskInterface
    {
        /// <summary>
        /// Sự kiện sẽ gọi đến sau khi hoàn tất Delay
        /// </summary>
        public Action FuntionExecute { get; set; } = null;

        /// <summary>
        /// Tên Task
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Tạo mới đối tượng sự kiện TaskDelay
        /// </summary>
        public DelayFuntionAsyncTask() { }

        /// <summary>
        /// Tạo mới đối tượng sự kiện TaskDelay
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tag"></param>
        /// <param name="player"></param>
        /// <param name="callback"></param>
        public DelayFuntionAsyncTask(string name, Action callback)
        {
            this.FuntionExecute = callback;
            this.Name = name;


        }

        /// <summary>
        /// Thực thi sự kiện
        /// </summary>
        public void Run()
        {
            try
            {
                this.FuntionExecute?.Invoke();
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "Execute DelayTask got Exception", false);
                //Console.WriteLine(ex.ToString());
            }
        }


    }

    /// <summary>
    /// Đối tượng quản lý Task thực thi sau khoảng tương ứng
    /// </summary>
    public class KTKTAsyncTask
    {
        /// <summary>
        /// Đối tượng quản lý Task thực thi sau khoảng tương ứng
        /// </summary>
        public static KTKTAsyncTask Instance { get; private set; } = new KTKTAsyncTask();

        /// <summary>
        /// Thực thi sự kiện sau khoảng Delay tương ứng
        /// </summary>
        /// <param name="task"></param>
        /// <param name="delay">Milis</param>
        /// <returns></returns>
        public async Task ScheduleExecuteAsync(IKTAsyncTaskInterface task, int delay)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
            task.Run();
        }
    }
}