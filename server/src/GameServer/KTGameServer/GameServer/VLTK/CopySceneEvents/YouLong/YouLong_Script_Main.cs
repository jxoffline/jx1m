using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.YouLongGe
{
	/// <summary>
	/// Script điều khiển phụ bản Du Long Các
	/// </summary>
	public class YouLong_Script_Main : CopySceneEvent
	{
		#region Private fields
		/// <summary>
		/// Thời điểm tỷ thí lần trước
		/// </summary>
		private long lastChallengeTicks = 0;

		/// <summary>
		/// Bước chiến đấu hiện tại
		/// </summary>
		private int fightStep = 0;

		/// <summary>
		/// Danh sách phần thưởng ngẫu nhiên
		/// </summary>
		private List<YouLong.AwardInfo.Award> randomAwards = new List<YouLong.AwardInfo.Award>();

		/// <summary>
		/// Phần thưởng hiện tại
		/// </summary>
		private YouLong.AwardInfo.Award currentAward;

		/// <summary>
		/// Số lần đã tỷ thí lại
		/// </summary>
		private int rechallengeTimes = 0;
		#endregion

		#region Constructor
		/// <summary>
		/// Script điều khiển phụ bản Du Long Các
		/// </summary>
		/// <param name="copyScene"></param>
		public YouLong_Script_Main(KTCopyScene copyScene) : base(copyScene)
		{
			/// Đánh dấu thời điểm tỷ thí lần trước
			this.lastChallengeTicks = KTGlobal.GetCurrentTimeMilis();
		}
		#endregion

		#region Core CopySceneEvent
		/// <summary>
		/// Hàm này gọi đến khi bắt đầu phụ bản
		/// </summary>
		protected override void OnStart()
		{
			/// Tạo NPC
			this.CreateNPC();
		}

		/// <summary>
		/// Hàm này gọi liên tục mỗi nửa giây chừng nào phụ bản còn tồn tại
		/// </summary>
		protected override void OnTick()
		{
			/// Nếu thời điểm lần cuối tỷ thí vượt quá thời gian cho phép
			if (KTGlobal.GetCurrentTimeMilis() - this.lastChallengeTicks >= YouLong.Data.KickOutIfNoChallengeFor)
			{
				/// Người chơi tương ứng
				KPlayer player = this.teamPlayers.FirstOrDefault();
				/// Nếu có người chơi
				if (player != null)
				{
					/// Thông báo
					KTPlayerManager.ShowNotification(player, "Đã quá thời gian không phát sinh tỷ thí mới, tự rời phụ bản!");
				}
				/// Hủy phụ bản
				this.Dispose();
			}
		}

		/// <summary>
		/// Hàm này gọi đến khi đóng phụ bản
		/// </summary>
		protected override void OnClose()
		{
			this.randomAwards.Clear();
			this.randomAwards = null;
			this.currentAward = null;
		}

		/// <summary>
		/// Hàm này gọi đến khi người chơi vào phụ bản
		/// </summary>
		/// <param name="player"></param>
		public override void OnPlayerEnter(KPlayer player)
		{
			base.OnPlayerEnter(player);
			/// Mở bảng thông báo sự kiện
			this.OpenEventBroadboard(player, (int) GameEvent.YouLong);
			/// Cập nhật thông tin sự kiện
			this.UpdateEventDetailsToPlayers(this.CopyScene.Name, YouLong.Data.Duration - this.LifeTimeTicks, "Thời gian còn lại");
		}

		/// <summary>
		/// Hàm này gọi đến khi người chơi rời phụ bản
		/// </summary>
		/// <param name="player"></param>
		/// <param name="toMap"></param>
		public override void OnPlayerLeave(KPlayer player, GameMap toMap)
		{
			base.OnPlayerLeave(player, toMap);
			/// Đóng bảng thông báo hoạt động
			this.CloseEventBroadboard(player, (int) GameEvent.YouLong);
		}

		/// <summary>
		/// Sự kiện khi người chơi giết Boss
		/// </summary>
		/// <param name="player"></param>
		/// <param name="obj"></param>
		public override void OnKillObject(KPlayer player, GameObject obj)
		{
			base.OnKillObject(player, obj);

			/// Nếu là Boss
			if (obj is Monster monster && monster.Tag == "Boss")
			{
				/// Tạo NPC
				this.CreateNPC();
				/// Mở khung phần thưởng
				this.OpenAwardsBox();
				/// Cập nhật thông tin sự kiện
				this.UpdateEventDetailsToPlayers(this.CopyScene.Name, YouLong.Data.Duration - this.LifeTimeTicks, "Thời gian còn lại");
			}
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Trả về danh sách vật phẩm thưởng ngẫu nhiên
		/// </summary>
		/// <returns></returns>
		private List<YouLong.AwardInfo.Award> GetRandomAwardsList()
		{
			/// Tạo mới danh sách
			List<YouLong.AwardInfo.Award> awards = new List<YouLong.AwardInfo.Award>();

			/// Lặp từ đầu đến số lượng phải lấy
			for (int i = 1; i <= YouLong.Data.Awards.Count; i++)
			{
				/// Tỷ lệ hiện tại
				int currentRate = KTGlobal.GetRandomNumber(1, 1000);
				/// Chọn các vật phẩm thuộc tỷ lệ tương ứng
				List<YouLong.AwardInfo.Award> groupRateAwards = YouLong.Data.Awards.Awards.Where(x => x.AppearRate >= currentRate).ToList();
				/// Vật phẩm được chọn
				YouLong.AwardInfo.Award awardInfo;
				/// Nếu không tồn tại
				if (groupRateAwards.Count <= 0)
				{
					/// Chọn vật phẩm có tỷ lệ cao nhất
					awardInfo = YouLong.Data.Awards.Awards.MaxBy(x => x.AppearRate);
				}
				/// Nếu tồn tại thì chọn ngẫu nhiên trong danh sách
				else
				{
					awardInfo = groupRateAwards[KTGlobal.GetRandomNumber(0, groupRateAwards.Count - 1)];
				}

				/// Thêm vào danh sách
				awards.Add(awardInfo);
			}

			/// Trả về kết quả
			return awards;
		}

		/// <summary>
		/// Mở khung phần thưởng
		/// </summary>
		private void OpenAwardsBox()
		{
			/// Người chơi tương ứng
			KPlayer player = this.teamPlayers.FirstOrDefault();
			/// Nếu không có người chơi
			if (player == null)
			{
				return;
			}

			/// Cập nhật bước chiến đấu hiện tại
			this.fightStep = 2;

			/// Gửi yêu cầu mở khung phần thưởng
			KT_TCPHandler.SendOpenYouLongAwardsBox(player, this.randomAwards);
		}

		/// <summary>
		/// Bắt đầu tỷ thí
		/// </summary>
		private void BeginChallenge()
		{
			/// Cập nhật bước chiến đấu hiện tại
			this.fightStep = 1;
			/// Cập nhật thông tin sự kiện
			this.UpdateEventDetailsToPlayers(this.CopyScene.Name, YouLong.Data.ChallengeDuration - this.LifeTimeTicks, "Thời gian tỷ thí");
			/// Đánh dấu thời điểm tỷ thí lần trước
			this.lastChallengeTicks = KTGlobal.GetCurrentTimeMilis();

			/// Xóa toàn bộ NPC
			this.RemoveAllNPCs();
			/// Xóa toàn bộ quái
			this.RemoveAllMonsters();
			/// Tạo Boss tương ứng
			this.CreateBoss();
		}

		/// <summary>
		/// Tạo Boss
		/// </summary>
		private void CreateBoss()
		{
			/// Mức máu
			int hp = YouLong.Data.Boss.BaseHP + YouLong.Data.Boss.HPIncreaseEachLevel * this.CopyScene.Level;
			/// Tạo quái
			Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
			{
				MapCode = this.CopyScene.MapCode,
				CopySceneID = this.CopyScene.ID,
				ResID = YouLong.Data.Boss.ID,
				PosX = YouLong.Data.Boss.PosX,
				PosY = YouLong.Data.Boss.PosY,
				Name = YouLong.Data.Boss.Name,
				Title = YouLong.Data.Boss.Title,
				MaxHP = hp,
				Level = this.CopyScene.Level,
				MonsterType = YouLong.Data.Boss.AIType,
				AIID = YouLong.Data.Boss.AIScriptID,
				Tag = "Boss",
				Camp = 65535,
			});

			/// Nếu có kỹ năng
			if (YouLong.Data.Boss.Skills.Count > 0)
			{
				/// Duyệt danh sách kỹ năng
				foreach (SkillLevelRef skill in YouLong.Data.Boss.Skills)
				{
					/// Thêm vào danh sách kỹ năng dùng của quái
					monster.CustomAISkills.Add(skill);
				}
			}

			/// Nếu có vòng sáng
			if (YouLong.Data.Boss.Auras.Count > 0)
			{
				/// Duyệt danh sách vòng sáng
				foreach (SkillLevelRef aura in YouLong.Data.Boss.Auras)
				{
					/// Kích hoạt vòng sáng
					monster.UseSkill(aura.SkillID, aura.Level, monster);
				}
			}
		}

		/// <summary>
		/// Tạo NPC
		/// </summary>
		private void CreateNPC()
		{
			/// Tạo NPC
			NPC npc = KTNPCManager.Create(new KTNPCManager.NPCBuilder()
			{
				MapCode = this.CopyScene.MapCode,
				CopySceneID = this.CopyScene.ID,
				ResID = YouLong.Data.NPC.ID,
				PosX = YouLong.Data.NPC.PosX,
				PosY = YouLong.Data.NPC.PosY,
				Name = YouLong.Data.NPC.Name,
				Title = YouLong.Data.NPC.Title,
			});
			/// Sự kiện Click
			npc.Click = (player) => {
				/// Nếu không có người chơi
				if (player == null)
				{
					return;
				}

				/// Nếu là bước 2
				if (this.fightStep == 2 && this.randomAwards.Count > 0)
				{
					/// Gửi yêu cầu mở khung phần thưởng
					KT_TCPHandler.SendOpenYouLongAwardsBox(player, this.randomAwards);
					return;
				}
				/// Nếu là bước 3
				else if (this.fightStep == 3 && this.currentAward != null)
				{
					/// Gửi yêu cầu mở khung phần thưởng
					KT_TCPHandler.SendOpenYouLongAwardsBox(player, this.randomAwards);
					/// Gửi yêu cầu cập nhật danh sách thưởng
					KT_TCPHandler.SendUpdateYouLongAward(player, this.currentAward.ID, this.currentAward.Number, this.currentAward.NumberOfCoins, false);
					return;
				}
				/// Nếu là bước 4
				else if (this.fightStep == 4)
				{
					/// Gửi yêu cầu mở khung phần thưởng
					KT_TCPHandler.SendOpenYouLongAwardsBox(player, this.randomAwards);
					/// Gửi thông báo hoàn tất nhận thưởng
					KT_TCPHandler.SendGetYouLongAwardSuccessfully(player);
					return;
				}

				KNPCDialog dialog = new KNPCDialog();
				dialog.Owner = player;
				dialog.Text = "Ngươi đến đây tỷ thí với Du Long Các - Tiểu Long Nữ ta? Tốt lắm. Bảo vật ở đây rất nhiều, tuy nhiên ngươi phải vượt qua thử thách của ta, mói đủ tư cách nhận!";
				dialog.Selections = new Dictionary<int, string>()
					{
						{ -1, "Ta đến tỷ võ" },
						{ -2, "Nguyệt Ảnh Thạch đổi Chiến Thư" },
						{ -3, "Đổi Du Long Danh Vọng Lệnh" },
						{ -4, "Ta muốn rời khỏi đây..." },
					};
				dialog.OnSelect = (x) => {
					/// Bắt đầu tỷ võ
					if (x.SelectID == -1)
					{
						/// Nếu không phải bước đầu
						if (this.fightStep != 0)
						{
							KNPCDialog dialogEx = new KNPCDialog();
							dialogEx.Owner = player;
							dialogEx.Text = "Ngươi không thể thao tác lúc này!";
							KTNPCDialogManager.AddNPCDialog(dialogEx);
							dialogEx.Show(npc, player);
							return;
						}
						/// Nếu không có chiến thư
						else if (ItemManager.GetItemCountInBag(player, YouLong.Data.RequireItem) <= 0)
						{
							KNPCDialog dialogEx = new KNPCDialog();
							dialogEx.Owner = player;
							dialogEx.Text = "Không có Chiến Thư, không thể khiêu chiến. Nếu ngươi có Nguyệt Ảnh Thạch, ngươi có thể đổi Chiến Thư ngay tại đây, sau đó tiến hành tỷ thí lại với ta!";
							KTNPCDialogManager.AddNPCDialog(dialogEx);
							dialogEx.Show(npc, player);
							return;
						}

						/// Thực hiện xóa chiến thư
						if (!ItemManager.RemoveItemFromBag(player, YouLong.Data.RequireItem, 1, -1, "YouLong"))
						{
							KNPCDialog dialogEx = new KNPCDialog();
							dialogEx.Owner = player;
							dialogEx.Text = "Có lỗi khi thực hiện xóa Chiến Thư!";
							KTNPCDialogManager.AddNPCDialog(dialogEx);
							dialogEx.Show(npc, player);
							return;
						}

						/// Hủy thông tin vật phẩm hiện tại
						this.currentAward = null;
						/// Hủy danh sách vật phẩm thưởng
						this.randomAwards.Clear();
						/// Cập nhật tổng số lần tỷ thí lại
						this.rechallengeTimes = 0;

						/// Danh sách vật phẩm ngẫu nhiên
						this.randomAwards = this.GetRandomAwardsList();

						/// Thực hiện tỷ thí
						this.BeginChallenge();

						/// Gửi gói tin đóng khung thưởng
						KT_TCPHandler.SendCloseYouLongAwardsBox(player);
						/// Đóng NPCDialog
						KT_TCPHandler.CloseDialog(player);
					}
					/// Đổi chiến thư
					else if (x.SelectID == -2)
					{
						/// Nếu không phải bước đầu
						if (this.fightStep != 0)
						{
							KNPCDialog dialogEx = new KNPCDialog();
							dialogEx.Owner = player;
							dialogEx.Text = "Ngươi không thể thao tác lúc này!";
							KTNPCDialogManager.AddNPCDialog(dialogEx);
							dialogEx.Show(npc, player);
							return;
						}
						/// Nếu không có Nguyệt Ảnh Thạch
						else if (ItemManager.GetItemCountInBag(player, YouLong.Data.Items.MoonStoneID) <= 0)
						{
							KNPCDialog dialogEx = new KNPCDialog();
							dialogEx.Owner = player;
							dialogEx.Text = "Không có Nguyệt Ảnh Thạch, không thể đổi chiến thư!";
							KTNPCDialogManager.AddNPCDialog(dialogEx);
							dialogEx.Show(npc, player);
							return;
						}

						/// Hiện bảng nhập
						KTPlayerManager.ShowInputNumberBox(player, "Nhập số lượng muốn đổi.", (number) => {
							/// Nếu không đủ số lượng Nguyệt Ảnh Thạch tương ứng
							if (ItemManager.GetItemCountInBag(player, YouLong.Data.Items.MoonStoneID) < number)
							{
								KNPCDialog dialogEx = new KNPCDialog();
								dialogEx.Owner = player;
								dialogEx.Text = "Số lượng Nguyệt Ảnh Thạch không đủ, không thể đổi chiến thư!";
								KTNPCDialogManager.AddNPCDialog(dialogEx);
								dialogEx.Show(npc, player);
								return;
							}

							/// Xóa Nguyệt Ảnh Thạch
							if (!ItemManager.RemoveItemFromBag(player, YouLong.Data.Items.MoonStoneID, number, -1, "YouLong"))
							{
								KNPCDialog dialogEx = new KNPCDialog();
								dialogEx.Owner = player;
								dialogEx.Text = "Có lỗi khi xóa Nguyệt Ảnh Thạch!";
								KTNPCDialogManager.AddNPCDialog(dialogEx);
								dialogEx.Show(npc, player);
								return;
							}

							/// Thêm Chiến Thư
							if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, YouLong.Data.RequireItem, number, 0, "YouLong_Script_Main", true, 0, false, ItemManager.ConstGoodsEndTime, "", -1))
							{
								KNPCDialog dialogEx = new KNPCDialog();
								dialogEx.Owner = player;
								dialogEx.Text = "Có lỗi khi nhận Chiến Thư Du Long";
								KTNPCDialogManager.AddNPCDialog(dialogEx);
								dialogEx.Show(npc, player);
								return;
							}

							/// Thông báo đổi thành công
							KNPCDialog _dialogEx = new KNPCDialog();
							_dialogEx.Owner = player;
							_dialogEx.Text = "Đổi Chiến Thư thành công!";
							KTNPCDialogManager.AddNPCDialog(_dialogEx);
							_dialogEx.Show(npc, player);
						});

					}
					/// Đổi Du Long Danh Vọng Lệnh
					else if (x.SelectID == -3)
					{
						/// Nếu không phải bước đầu
						if (this.fightStep != 0)
						{
							KNPCDialog dialogEx = new KNPCDialog();
							dialogEx.Owner = player;
							dialogEx.Text = "Ngươi không thể thao tác lúc này!";
							KTNPCDialogManager.AddNPCDialog(dialogEx);
							dialogEx.Show(npc, player);
							return;
						}

						KNPCDialog _dialogEx = new KNPCDialog();
						_dialogEx.Owner = player;
						_dialogEx.Text = "Chức năng này đang được phát triển!";
						KTNPCDialogManager.AddNPCDialog(_dialogEx);
						_dialogEx.Show(npc, player);
					}
					/// Rời khỏi
					else if (x.SelectID == -4)
					{
						KNPCDialog dialogEx = new KNPCDialog();
						dialogEx.Owner = player;
						dialogEx.Text = "Xác nhận rời khỏi đây?";
						dialogEx.Selections = new Dictionary<int, string>()
							{
								{ -1, "Xác nhận" },
								{ -2, "Hủy bỏ" },
							};
						dialogEx.OnSelect = (xx) => {
							/// Nếu xác nhận
							if (xx.SelectID == -1)
							{
								/// Đóng Dialog
								KT_TCPHandler.CloseDialog(player);
								/// Hủy phụ bản
								this.Dispose();
							}
							/// Nếu hủy bỏ
							else
							{
								/// Đóng Dialog
								KT_TCPHandler.CloseDialog(player);
							}
						};
						KTNPCDialogManager.AddNPCDialog(dialogEx);
						dialogEx.Show(npc, player);
					}
				};
				KTNPCDialogManager.AddNPCDialog(dialog);
				dialog.Show(npc, player);
			};

			/// Cập nhật thông tin sự kiện
			this.UpdateEventDetailsToPlayers(this.CopyScene.Name, YouLong.Data.Duration - this.LifeTimeTicks, "Thời gian còn lại");
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Lấy một phần thưởng ngẫu nhiên trong danh sách
		/// </summary>
		public void GetRandomAward()
		{
			/// Người chơi tương ứng
			KPlayer player = this.teamPlayers.FirstOrDefault();
			/// Nếu không có người chơi
			if (player == null)
			{
				return;
			}

			/// Nếu không phải bước này
			if (this.fightStep != 2)
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận phần thưởng ngẫu nhiên!");
				return;
			}

			/// Nếu danh sách rỗng
			if (this.randomAwards.Count <= 0)
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận phần thưởng ngẫu nhiên!");
				return;
			}

			/// Cập nhật bước kế tiếp
			this.fightStep = 3;

			/// Tỷ lệ may mắn
			int luckyRate = KTGlobal.GetRandomNumber(1, 1000);

			/// Danh sách vật phẩm có tỷ lệ may mắn nhận được
			List<YouLong.AwardInfo.Award> awards = this.randomAwards.Where(x => x.Rate >= luckyRate).ToList();
			/// Nếu không tìm được cái nào thỏa mãn
			if (awards.Count <= 0)
			{
				/// Chọn thằng có Rate cao nhất
				YouLong.AwardInfo.Award award = this.randomAwards.MaxBy(x => x.Rate);
				/// Thêm vào danh sách
				awards.Add(award);
			}
			/// Chọn một vật phẩm trong danh sách
			this.currentAward = awards[KTGlobal.GetRandomNumber(0, awards.Count - 1)];

			/// Xóa vật phẩm vừa nhận khỏi danh sách
			this.randomAwards.Remove(this.currentAward);

			/// Xóa List
			awards.Clear();
			awards = null;

			/// Gửi gói tin thông báo vật phẩm nhận được
			KT_TCPHandler.SendUpdateYouLongAward(player, this.currentAward.ID, this.currentAward.Number, this.currentAward.NumberOfCoins, true);
		}

		/// <summary>
		/// Nhận quà thưởng của người chơi
		/// </summary>
		public void GetAward()
		{
			/// Người chơi tương ứng
			KPlayer player = this.teamPlayers.FirstOrDefault();
			/// Nếu không có người chơi
			if (player == null)
			{
				return;
			}

			/// Nếu không phải bước này
			if (this.fightStep != 3)
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi nhận phần thưởng!");
				return;
			}

			/// Nếu không có quà thưởng
			if (this.currentAward == null)
			{
				KTPlayerManager.ShowNotification(player, "Ngươi không có phần thưởng!");
				return;
			}

			/// Số ô đồ trống cần
			int totalSpacesNeed = KTGlobal.GetTotalSpacesNeedToTakeItem(this.currentAward.ID, this.currentAward.Number);

			/// Kiểm tra túi đã đầy chưa
			if (!KTGlobal.IsHaveSpace(totalSpacesNeed, player))
			{
				KTPlayerManager.ShowNotification(player, "Túi đồ không đủ chỗ trống, hãy sắp xếp lại ít nhất " + this.currentAward.Number +  " và thử lại!");
				return;
			}

			/// Cập nhật bước kế tiếp
			this.fightStep = 4;

			/// Thực hiện thêm phần thưởng tương ứng
			if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, this.currentAward.ID, this.currentAward.Number, 0, "YouLong_Script_Main", true, 0, false, ItemManager.ConstGoodsEndTime, "", -1))
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo phần thưởng!");
				//return;
			}

			/// Ghi LOG
			LogManager.WriteLog(LogTypes.YouLongGe, string.Format("{0} (ID: {1}) mở được {2} cái {3}.", player.RoleName, player.RoleID, this.currentAward.Number, KTGlobal.GetItemName(this.currentAward.ID)));

			/// Hủy quà
			this.currentAward = null;

			/// Thông báo
			KTPlayerManager.ShowNotification(player, "Nhận thành công!");

			/// Gửi gói tin thông báo nhận và đổi thưởng thành công
			KT_TCPHandler.SendGetYouLongAwardSuccessfully(player);
		}

		/// <summary>
		/// Đổi Tiền Du Long
		/// </summary>
		public void ExchangeCoin()
		{
			/// Người chơi tương ứng
			KPlayer player = this.teamPlayers.FirstOrDefault();
			/// Nếu không có người chơi
			if (player == null)
			{
				return;
			}

			/// Nếu không phải bước này
			if (this.fightStep != 3)
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi đổi Tiền Du Long!");
				return;
			}

			/// Nếu không có quà thưởng
			if (this.currentAward == null)
			{
				KTPlayerManager.ShowNotification(player, "Ngươi không có phần thưởng!");
				return;
			}

			/// Số ô đồ trống cần
			int totalSpacesNeed = KTGlobal.GetTotalSpacesNeedToTakeItem(YouLong.Data.Items.YouLongCoinID, this.currentAward.NumberOfCoins);

			/// Kiểm tra túi đã đầy chưa
			if (!KTGlobal.IsHaveSpace(totalSpacesNeed, player))
			{
				KTPlayerManager.ShowNotification(player, "Túi đồ không đủ chỗ trống, hãy sắp xếp lại ít nhất " + this.currentAward.NumberOfCoins + " và thử lại!");
				return;
			}

			/// Cập nhật bước kế tiếp
			this.fightStep = 4;

			/// Thực hiện thêm phần thưởng tương ứng
			if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, player, YouLong.Data.Items.YouLongCoinID, this.currentAward.NumberOfCoins, 0, "YouLong_Script_Main", true, 1, false, ItemManager.ConstGoodsEndTime, "", -1))
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi tạo Tiền Du Long!");
				//return;
			}

			/// Ghi LOG
			LogManager.WriteLog(LogTypes.YouLongGe, string.Format("{0} (ID: {1}) đổi được {2} Tiền du long.", player.RoleName, player.RoleID, this.currentAward.Number));

			/// Hủy quà
			this.currentAward = null;

			/// Thông báo
			KTPlayerManager.ShowNotification(player, "Đổi thành công!");

			/// Gửi gói tin thông báo nhận và đổi thưởng thành công
			KT_TCPHandler.SendGetYouLongAwardSuccessfully(player);
		}

		/// <summary>
		/// Bắt đầu vòng khiêu chiến mới
		/// </summary>
		public void StartNewRound()
		{
			/// Người chơi tương ứng
			KPlayer player = this.teamPlayers.FirstOrDefault();
			/// Nếu không có người chơi
			if (player == null)
			{
				return;
			}

			/// Nếu không phải bước này
			if (this.fightStep != 4)
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi bắt đầu vòng khiêu chiến mới!");
				return;
			}

			/// Số lượt đã tham gia trong ngày
			int challengeTimes = CopySceneEventManager.GetCopySceneTotalEnterTimesToday(player, DailyRecord.YouLong);
			/// Nếu đã tham gia trong ngày rồi thì thôi
			if (challengeTimes >= YouLong.Data.LimitRoundPerDay)
			{
				KTPlayerManager.ShowNotification(player, "Ngươi đã tham gia đủ số lượt trong ngày, không thể tham gia nữa!");
				return;
			}

			/// Nếu không có chiến thư
			if (ItemManager.GetItemCountInBag(player, YouLong.Data.RequireItem) <= 0)
			{
				KTPlayerManager.ShowNotification(player, "Không có Chiến Thư, không thể khiêu chiến!");
				return;
			}

			/// Thực hiện xóa chiến thư
			if (!ItemManager.RemoveItemFromBag(player, YouLong.Data.RequireItem, 1,-1, "YouLong"))
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi thực hiện xóa Chiến Thư!");
				return;
			}

			/// Tăng số lượt đi trong ngày
			CopySceneEventManager.SetCopySceneTotalEnterTimesToday(player, DailyRecord.YouLong, challengeTimes + 1);

			/// Xóa dữ liệu phần thưởng
			this.currentAward = null;
			this.randomAwards.Clear();

			/// Cập nhật bước
			this.fightStep = 0;
			/// Cập nhật tổng số lần tỷ thí lại
			this.rechallengeTimes = 0;

			/// Danh sách vật phẩm ngẫu nhiên
			this.randomAwards = this.GetRandomAwardsList();

			/// Bắt đầu tỷ thí
			this.BeginChallenge();

			/// Gửi gói tin đóng khung thưởng
			KT_TCPHandler.SendCloseYouLongAwardsBox(player);

			/// Ghi LOG
			LogManager.WriteLog(LogTypes.YouLongGe, string.Format("{0} (ID: {1}) bắt đầu lượt khiêu chiến mới.", player.RoleName, player.RoleID));
		}

		/// <summary>
		/// Tỷ thí lại
		/// </summary>
		public void Rechallenge()
		{
			/// Người chơi tương ứng
			KPlayer player = this.teamPlayers.FirstOrDefault();
			/// Nếu không có người chơi
			if (player == null)
			{
				return;
			}

			/// Nếu không phải bước này
			if (this.fightStep != 4)
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi bắt đầu vòng khiêu chiến mới!");
				return;
			}

			/// Nếu số lần tỷ thí đã vượt quá giới hạn
			if (this.rechallengeTimes >= YouLong.Data.Awards.MaxTryTime)
			{
				KTPlayerManager.ShowNotification(player, "Đã vượt quá số lần có thể tỷ thí lại, hãy chọn Tỷ thí mới!");
				return;
			}

			/// Số lượt đã tham gia trong ngày
			int challengeTimes = CopySceneEventManager.GetCopySceneTotalEnterTimesToday(player, DailyRecord.YouLong);
			/// Nếu đã tham gia trong ngày rồi thì thôi
			if (challengeTimes >= YouLong.Data.LimitRoundPerDay)
			{
				KTPlayerManager.ShowNotification(player, "Ngươi đã tham gia đủ số lượt trong ngày, không thể tham gia nữa!");
				return;
			}

			/// Nếu không có chiến thư
			if (ItemManager.GetItemCountInBag(player, YouLong.Data.RequireItem) <= 0)
			{
				KTPlayerManager.ShowNotification(player, "Không có Chiến Thư, không thể khiêu chiến!");
				return;
			}

			/// Thực hiện xóa chiến thư
			if (!ItemManager.RemoveItemFromBag(player, YouLong.Data.RequireItem, 1,-1, "YouLong"))
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi thực hiện xóa Chiến Thư!");
				return;
			}

			/// Tăng số lượt đi trong ngày
			CopySceneEventManager.SetCopySceneTotalEnterTimesToday(player, DailyRecord.YouLong, challengeTimes + 1);

			/// Hủy thông tin vật phẩm hiện tại
			this.currentAward = null;
			/// Tăng tổng số lần tỷ thí lại
			this.rechallengeTimes++;
			/// Bắt đầu tỷ thí
			this.BeginChallenge();

			/// Gửi gói tin đóng khung thưởng
			KT_TCPHandler.SendCloseYouLongAwardsBox(player);

			/// Ghi LOG
			LogManager.WriteLog(LogTypes.YouLongGe, string.Format("{0} (ID: {1}) tái chiến.", player.RoleName, player.RoleID));
		}

		/// <summary>
		/// Rời khỏi phụ bản
		/// </summary>
		public void Leave()
		{
			/// Người chơi tương ứng
			KPlayer player = this.teamPlayers.FirstOrDefault();
			/// Nếu không có người chơi
			if (player == null)
			{
				return;
			}

			/// Nếu không phải bước này
			if (this.fightStep != 4)
			{
				KTPlayerManager.ShowNotification(player, "Có lỗi khi thoát phụ bản!");
				return;
			}

			/// Hủy phụ bản
			this.Dispose();

			/// Gửi gói tin đóng khung thưởng
			KT_TCPHandler.SendCloseYouLongAwardsBox(player);
		}
		#endregion
	}
}
