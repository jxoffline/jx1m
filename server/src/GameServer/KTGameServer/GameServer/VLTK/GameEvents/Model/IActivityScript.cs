using GameServer.KiemThe.Logic;

namespace GameServer.KiemThe.GameEvents.Model
{
	/// <summary>
	/// Lớp mẫu hoạt động
	/// </summary>
	public interface IActivityScript
	{
		/// <summary>
		/// Hoạt động tương ứng
		/// </summary>
		KTActivity Activity { get; set; }

		/// <summary>
		/// Bắt đầu hoạt động
		/// </summary>
		void Begin();

		/// <summary>
		/// Đóng hoạt động
		/// </summary>
		void Close();
	}
}
