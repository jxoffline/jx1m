namespace GameDBServer.Logic
{
    /// <summary>
    /// Tham chiếu nhân vật
    /// </summary>
    public class MyWeakReference
    {
        /// <summary>
        /// Mutex luồng
        /// </summary>
        private object _ThreadMutex = new object();

        /// <summary>
        /// Tham chiếu nhân vật
        /// </summary>
        /// <param name="target"></param>
        public MyWeakReference(object target)
        {
            _Target = target;
        }

        /// <summary>
        /// Còn hoạt động không
        /// </summary>
        public bool IsAlive
        {
            get
            {
                lock (_ThreadMutex)
                {
                    return (null != _Target);
                }
            }
        }

        /// <summary>
        /// Đối tượng tương ứng
        /// </summary>
        private object _Target = null;

        /// <summary>
        /// Đối tượng tương ứng
        /// </summary>
        public object Target
        {
            get
            {
                lock (_ThreadMutex)
                {
                    return _Target;
                }
            }

            set
            {
                lock (_ThreadMutex)
                {
                    _Target = value;
                }
            }
        }
    }
}