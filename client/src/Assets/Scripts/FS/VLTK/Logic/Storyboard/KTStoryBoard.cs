namespace FS.VLTK.Logic
{
	/// <summary>
	/// StoryBoard quản lý di chuyển của đối tượng
	/// </summary>
	public partial class KTStoryBoard : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// StoryBoard quản lý di chuyển của đối tượng
        /// </summary>
        public static KTStoryBoard Instance { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đôi tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            KTStoryBoard.Instance = this;
        }
        #endregion
    }
}
