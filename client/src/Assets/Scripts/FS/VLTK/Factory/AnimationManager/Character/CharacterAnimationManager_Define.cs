namespace FS.VLTK.Factory.Animation
{
	/// <summary>
	/// Định nghĩa thực thể
	/// </summary>
	public partial class CharacterAnimationManager
    {
#if UNITY_EDITOR || UNITY_IOS
		/// <summary>
		/// Sử dụng cơ chế tải xuống tài nguyên theo phương thức Async
		/// </summary>
		private const bool UseAsyncLoad = false;
#else
		/// <summary>
		/// Sử dụng cơ chế tải xuống tài nguyên theo phương thức Async
		/// </summary>
		private const bool UseAsyncLoad = true;
#endif
	}
}
