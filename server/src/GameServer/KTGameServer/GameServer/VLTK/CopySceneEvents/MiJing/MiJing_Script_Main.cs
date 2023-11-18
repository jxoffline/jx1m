using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.MiJingFuBen
{
	/// <summary>
	/// Script phụ bản Bí Cảnh
	/// </summary>
	public class MiJing_Script_Main : CopySceneEvent
	{
		#region Constants
		/// <summary>
		/// Thời gian mỗi lần thông báo thông tin sự kiện tới người chơi
		/// </summary>
		private const int NotifyActivityInfoToPlayersEveryTick = 5000;
		#endregion

		#region Private fields
		/// <summary>
		/// Thời điểm thông báo cập nhật thông tin sự kiện tới tất cả người chơi lần trước
		/// </summary>
		private long LastNotifyTick;
		#endregion

		#region Constructor
		/// <summary>
		/// Script phụ bản Bí Cảnh
		/// </summary>
		/// <param name="copyScene"></param>
		public MiJing_Script_Main(KTCopyScene copyScene) : base(copyScene)
		{

		}
		#endregion

		#region Core CopySceneEvent
		/// <summary>
		/// Sự kiện bắt đầu bí cảnh
		/// </summary>
		protected override void OnStart()
		{
			/// Tạo quái
			this.CreateMonsters();
		}

		/// <summary>
		/// Sự kiện Tick
		/// </summary>
		protected override void OnTick()
		{
			/// Nếu đã đến thời gian thông báo thông tin sự kiện
			if (KTGlobal.GetCurrentTimeMilis() - this.LastNotifyTick >= MiJing_Script_Main.NotifyActivityInfoToPlayersEveryTick)
			{
				/// Đánh dấu thời gian thông báo thông tin sự kiện
				this.LastNotifyTick = KTGlobal.GetCurrentTimeMilis();
				/// Cập nhật thông tin sự kiện
				this.UpdateEventDetailsToPlayers(this.CopyScene.Name, MiJing.Duration - this.LifeTimeTicks, "Bí cảnh duy trì");
			}
		}

		/// <summary>
		/// Sự kiện đóng bí cảnh
		/// </summary>
		protected override void OnClose()
		{
			/// Xóa toàn bộ quái
			this.RemoveAllMonsters();
		}

		/// <summary>
		/// Sự kiện khi người chơi vào phụ bản
		/// </summary>
		/// <param name="player"></param>
		public override void OnPlayerEnter(KPlayer player)
		{
			base.OnPlayerEnter(player);

			/// Mở bảng thông báo hoạt động
			this.OpenEventBroadboard(player, (int) GameEvent.MiJing);
			/// Chuyển trạng thái PK hòa bình
			player.PKMode = (int) PKMode.Peace;
			/// Cập nhật thông tin sự kiện
			this.UpdateEventDetailsToPlayers(this.CopyScene.Name, MiJing.Duration - this.LifeTimeTicks, "Bí cảnh duy trì");

			/// Nếu là đội trưởng thì thêm Buff x2 kinh nghiệm vào
			if (player.TeamID != -1 && KTTeamManager.IsTeamExist(player.TeamID) && player.TeamLeader == player)
			{
				/// Xóa Buff x2 kinh nghiệm cũ
				player.Buffs.RemoveBuff(MiJing.DoubleExpBuff);
				/// Nhận Buff x2 kinh nghiệm mới
				player.Buffs.AddBuff(MiJing.DoubleExpBuff, 1);
			}
		}

		/// <summary>
		/// Sự kiện khi người chơi rời phụ bản
		/// </summary>
		/// <param name="player"></param>
		/// <param name="toMap"></param>
		public override void OnPlayerLeave(KPlayer player, GameMap toMap)
		{
			base.OnPlayerLeave(player, toMap);

			/// Đóng bảng thông báo hoạt động
			this.CloseEventBroadboard(player, (int) GameEvent.MiJing);

			/// Xóa Buff x2 kinh nghiệm
			player.Buffs.RemoveBuff(MiJing.DoubleExpBuff);
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Tạo quái
		/// </summary>
		private void CreateMonsters()
		{
			foreach (MiJing.MonsterInfo monsterInfo in MiJing.Monsters)
			{
				/// Mức máu
				int hp = monsterInfo.BaseHP + monsterInfo.HPIncreaseEachLevel * this.CopyScene.Level;
				/// Tạo quái
				KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
				{
					MapCode = this.CopyScene.MapCode,
					CopySceneID = this.CopyScene.ID,
					ResID = monsterInfo.ID,
					PosX = monsterInfo.PosX,
					PosY = monsterInfo.PosY,
					Name = monsterInfo.Name,
					Title = monsterInfo.Title,
					MaxHP = hp,
					Level = this.CopyScene.Level,
					MonsterType = monsterInfo.AIType,
					AIID = monsterInfo.AIScriptID,
					Camp = 65535,
					RespawnTick = monsterInfo.RespawnTicks,
				});
			}
		}
		#endregion
	}
}
