-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local ChuongMon = Scripts[0000067]

local NguHanhCard = 18735
local IDSkillKingKong = 10 --IDskill king cong
local RecordKey90 = 999999 -- key dùng 1 lần duy nhất thằng này chỉ dùng 1 lần nick
local RecordKey150 = 2132023 -- key dùng 1 lần duy nhất thằng này chỉ dùng 1 lần nick
local ThuyTinh = { TuThuyTinh = 8858, LamThuyTinh = 8859, LucThuyTinh = 8860, TinhHongBaoThach = 8861 } -- 3 thủy tinh , và 1 hồng tinh
local RuongBiKip150 = { Item = 10469, number = 1, series = -1, bind = 1 }
local SetItemSkill90 = {
	--SKill 90x
	[1] = {
		ItemID1 = 8489,
		ItemID2 = 8490,
		ItemID3 = 8491,

	},
	[2] = {

		ItemID1 = 8492,
		ItemID2 = 8493,
		ItemID3 = 8494,

	},
	[3] = {

		ItemID1 = 8497,
		ItemID2 = 8498,
		ItemID3 = 8499,
		ItemID4 = 8515,

	},
	[4] = {

		ItemID1 = 8495,
		ItemID2 = 8496,
		ItemID3 = 8516,
	},
	[5] = {

		ItemID1 = 8500,
		ItemID2 = 8501,
		ItemID3 = 8514,
	},
	[6] = {

		ItemID1 = 8502,
		ItemID2 = 8503,
		--
	},
	[7] = {


		ItemID1 = 8504,
		ItemID2 = 8505,
		--

	},
	[8] = {

		ItemID1 = 8506,
		ItemID2 = 8507,
		ItemID3 = 8517,
	},
	[9] = {

		ItemID1 = 8508,
		ItemID2 = 8509,
		--
	},
	[10] = {

		ItemID1 = 8510,
		ItemID2 = 8511,
		ItemID3 = 8518,
	},
	[11] = {

		ItemID1 = 8512,
		ItemID2 = 8513,

	},
	[12] = {

		ItemID1 = 8566,
		ItemID2 = 8567,
	},
	[15] = {
		ItemID1 = 34618,--Bạch Nhật Sâm Thần 90
		ItemID2 = 34619,--Tê Chiếu Phồn Thương 90
	},
	[16] = {

		ItemID1 = 8572,
		ItemID2 = 8573,
	}
}
local ItemSKill150 = {
	[1] = {
		ItemID1 = 8437, -- Vi Đà Hiến Xử
		ItemID2 = 8438, -- Tam Giới Quy Thiền
		ItemID3 = 8439, -- Đại Lực Kim Cang Chưởng
	},
	[2] = {
		ItemID1 = 8440, -- Hào Hùng Trảm
		ItemID2 = 8441, -- Tung Hoành Bát Hoang
		ItemID3 = 8442, -- Bá Vương Tạm Kim
	},
	[4] = {
		ItemID1 = 8443, -- "Hình Tiêu Cốt Lập
		ItemID2 = 8444, -- U Hồn Phệ Ảnh
	},
	[3] = {
		ItemID1 = 8445, -- Thiết Liên Tứ Sát
		ItemID2 = 8446, -- Càn Khôn Nhất Trịch
		ItemID3 = 8447, -- Vô Ảnh Xuyên
		ItemID4 = 8463, -- Tích Lịch Loạn Hoàn Kích
	},
	[5] = {
		ItemID1 = 8448, -- Kiếm Hoa Vãn Tinh
		ItemID2 = 8449, -- Băng Vũ Lạc Tinh
		ItemID3 = 8450, -- Ngọc Tuyền Tâm Kinh
	},
	[6] = {
		ItemID1 = 8451, -- "Băng Tước Hoạt Kỳ
		ItemID2 = 8452, -- Thủy Anh Man Tú
	},
	[7] = {
		ItemID1 = 8453, -- Thời Thặng Lục Long
		ItemID2 = 8454, -- Bổng Huýnh Lược Địa
	},
	[8] = {
		ItemID1 = 8455, -- Giang Hải Nộ Lan
		ItemID2 = 8456, -- Tật Hỏa Liệu Nguyên
	},
	[9] = {
		ItemID1 = 8457, -- Tạo Hóa Thái Thanh
		ItemID2 = 8458, -- "Kiếm Thùy Tinh Hà
	},
	[10] = {
		ItemID1 = 8459, -- Cửu Thiên Cương Phong
		ItemID2 = 8460, -- Thiên Lôi Chấn Nhạc
	},
	[11] = {
		ItemID1 = 8461, -- Cửu Kiếm Hợp Nhất
		ItemID2 = 8462, -- Thần Quang Toàn Nhiễu
	},
	[12] = {
		ItemID1 = 8570, -- Bí Kíp cửu hi hỗn dương
		ItemID2 = 8571, -- Bí Kíp thánh hỏa lệnh pháp
	},
	
	[16] = {
		ItemID1 = 8578,
		ItemID2 = 8579,
		ItemID3 = 8580,
		ItemID4 = 8581,
	},
	
}
-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function ChuongMon:OnOpen(scene, npc, player, otherParams)
	-- ************************** --
	-- Nếu chưa có môn
	if player:GetSex() == 1 and scene:GetID() == 28 then
		self:ShowDialog(npc, player, "Giới tính của người không phù hợp")
		return
	end
	--Nam không cho vào
	if player:GetSex() == 0 and (scene:GetID() == 19 or scene:GetID() == 29) then
		self:ShowDialog(npc, player, "Giới tính của người không phù hợp")
		return
	end
	if player:GetFactionID() == 0 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText(
			"Chưởng Môn: Thiên phú của ngươi đã vượt qua người thường, chi bằng hãy gia nhập bổn phái để giúp ích cho ngươi rèn luyện về sau. <br><color=#FFFF00>Nếu còn thắc mắc gì, sau khi gia nhập bổn phái sẽ giúp ngươi hiểu rõ tường tận.</color>")
		dialog:AddSelection(1, "<color=#00ff44>Ta muốn gia nhập môn phái, xin hãy thu nhận ta.</color>")
		dialog:AddSelection(2, "Để ta suy nghĩ sau")
		dialog:Show(npc, player)
		-- Nếu môn phái tồn tại
	elseif Global_NameMapItemPhaiID[scene:GetID()] ~= nil then
		-- Thông tin môn phái tương ứng
		local factionInfo = Global_NameMapItemPhaiID[scene:GetID()]

		-- Nếu trùng với môn phái này
		if factionInfo.FactionID == player:GetFactionID() then
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText("Chưởng Môn: Lâu lắm không gặp ngươi " ..
			player:GetName() ..
			", nhận thấy ngươi rất có tố chất, xứng danh với môn đồ bổn phái. Ngươi muốn sư phụ giúp gì?")
			dialog:AddSelection(3, "Nhận Ngũ hành ấn")
			dialog:AddSelection(6, "Ta muốn mua trang bị thi đấu mốn phái")
			dialog:AddSelection(7, "Hoạt động thi đấu môn phái")
			dialog:AddSelection(9, "Nhận <color=#FFFF00>Nhận kỹ năng 90 </color>")
			dialog:AddSelection(10, "Nhận <color=#FFFF00>Nhận kỹ năng 150 </color>")
			dialog:AddSelection(2, "Để ta suy nghĩ sau")
			dialog:Show(npc, player)
			-- Nếu ở phái khác
		else
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText(
				"Chưởng Môn: Bổn phái tồn tại bấy lâu trên giang hồ cũng không phải hư danh. Phái <color=#FFFF00>"
				..
				player:GetFactionName() ..
				"</color> đại hiệp muốn thỉnh giáo võ công hay bàn luận thế sự ta mặc sức có thể đón tiếp!")
			dialog:AddSelection(2, "Kết thúc đối thoại")
			dialog:Show(npc, player)
		end
	end
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function ChuongMon:OnSelection(scene, npc, player, selectionID)
	-- ************************** --
	if selectionID == 2 then
		GUI.CloseDialog(player)
		return
		-- ************************** --
	end

	if selectionID == 1 then
		-- Nếu đã có phái thì toác
		if player:GetFactionID() ~= 0 then
			self:ShowDialog(npc, player, "Ngươi đã gia nhập môn phái, không thể gia nhập thêm!")
			return
		end
		self:ShowDialog()
		-- nếu là nữ thì k cho vào
		if player:GetSex() == 1 and scene:GetID() == 28 then
			self:ShowDialog(npc, player, "Giới tính của người không  phù hợp")
			return
		end
		--Nam không cho vào
		if player:GetSex() == 0 and (scene:GetID() == 19 or scene:GetID() == 29) then
			self:ShowDialog(npc, player, "Giới tính của người không phù hợp")
			return
		end
		-- Nếu chưa đạt cấp độ
		if player:GetLevel() < 10 then
			self:ShowDialog(npc, player,
				"Ngươi chưa đạt đủ cấp độ để gia nhập môn phái. Hãy quay lại khi đạt <color=green>cấp 10!</color>")
			return
		end
		-- Nếu toác
		if Global_NameMapItemPhaiID[scene:GetID()] == nil then
			self:ShowDialog(npc, player, "Thông tin môn phái bị lỗi, hãy thử lại sau!")
			return
		end

		--Kiểm tra túi đủ chỗ trống không
		if Player.HasFreeBagSpaces(player, 10) == false then
			self:ShowDialog(npc, player,
				"Ta có phần quà nhập môn dành cho ngươi, hãy sắp xếp <color=green>10 ô trống</color> trong túi để nhận!")
			return
		end

		-- Thông tin môn phái
		local factionInfo = Global_NameMapItemPhaiID[scene:GetID()]

		-- Cho vào môn phái
		self:JoinFaction(scene, npc, player, factionInfo.FactionID)

		Player.AddItemLua(player, ThuyTinh.LamThuyTinh, 1, -1, 1)
		Player.AddItemLua(player, ThuyTinh.LucThuyTinh, 1, -1, 1)
		Player.AddItemLua(player, ThuyTinh.TuThuyTinh, 1, -1, 1)
		Player.AddItemLua(player, ThuyTinh.TinhHongBaoThach, 6, -1, 1)

		-- Nếu chưa có kỹ năng này
		if player:HasSkill(IDSkillKingKong) == false then
			-- Thêm kỹ năng vào
			player:AddSkill(IDSkillKingKong)
		end
		-- Thêm kỹ năng tương ứng với cấp độ 1
		player:AddSkillLevel(IDSkillKingKong, 1)
		-- Thêm 3 lệnh bài Tẩy Tủy Đảo
		--Player.AddItemLua(player, LenhBaiTayTuyDao, 3, -1, 1)
		self:ShowDialog(npc, player,
			"Ngươi đã nhận <color=green>1 Tử Thủy Tinh, 1 Lam Thủy Tinh, 1 Lục Thủy Tinh, 6 Tinh Hồng bảo Thạch, vũ khí môn phái và Ngươi đã học kỹ năng khinh công</color> thành công!")
		return
	end
	if selectionID == 3 then
		-- Kiểm tra túi đủ chỗ trống không
		if Player.HasFreeBagSpaces(player, 1) == false then
			self:ShowDialog(npc, player,
				"Túi của ngươi đã đầy, hãy sắp xếp <color=green>1 ô trống</color> trong túi để nhận Ngũ Hành Ấn!")
			return
		end
		Player.AddItemLua(player, NguHanhCard, 1, -1, 1)
		-- Thông báo nhận thành công
		self:ShowDialog(npc, player, "Nhận <color=green>Ngũ Hành Ấn</color> thành công!")
	end

	if selectionID == 6 then
		if player:GetLevel() >= 10 then
			if player:GetFactionID() == 1 then
				Player.OpenShop(npc, player, 309)
			elseif player:GetFactionID() == 2 then
				Player.OpenShop(npc, player, 310)
			elseif player:GetFactionID() == 3 then
				Player.OpenShop(npc, player, 311)
			elseif player:GetFactionID() == 4 then
				Player.OpenShop(npc, player, 312)
			elseif player:GetFactionID() == 5 then
				Player.OpenShop(npc, player, 313)
			elseif player:GetFactionID() == 6 then
				Player.OpenShop(npc, player, 314)
			elseif player:GetFactionID() == 7 then
				Player.OpenShop(npc, player, 315)
			elseif player:GetFactionID() == 8 then
				Player.OpenShop(npc, player, 316)
			elseif player:GetFactionID() == 9 then
				Player.OpenShop(npc, player, 317)
			elseif player:GetFactionID() == 10 then
				Player.OpenShop(npc, player, 318)
			elseif player:GetFactionID() == 11 then
				Player.OpenShop(npc, player, 319)
			elseif player:GetFactionID() == 12 then
				Player.OpenShop(npc, player, 320)
			elseif player:GetFactionID() == 16 then
				Player.OpenShop(npc, player, 321)
			elseif player:GetFactionID() == 15 then
				Player.OpenShop(npc, player, 322)
			elseif player:GetFactionID() == 13 then
				Player.OpenShop(npc, player, 323)
			elseif player:GetFactionID() == 14 then
				Player.OpenShop(npc, player, 324)
			else
				
				self:ShowDialog(npc, player, "Cửa hàng đang phát triển")
			end
			GUI.CloseDialog(player)
		else
			self:ShowDialog(npc, player, "Ngươi chưa đủ cấp để có thể tham quan cửa hàng!")
		end
	end

	if selectionID == 7 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText(
			"Thi đấu môn phái vào tối thứ 3 và thứ 6 hàng tuần sẽ bắt đầu báo danh - 20:00 Sẽ bắt đầu\n<color=red>Điểm của mỗi lần thi đấu sẽ được thay đổi ở những lần thi tiếp theo</color>.")
		dialog:AddSelection(103, "Ta muốn tham gia thi đấu môn phái")
		dialog:AddSelection(2, "Ta đã hiểu")
		dialog:Show(npc, player)
		return
	end
	--thông tin phái
	local factionInfo = Global_NameMapItemPhaiID[scene:GetID()]
	-- get số lần của
	local record90 = Player.GetValueForeverRecore(player, RecordKey90)
	if selectionID == 9 then
		if record90 ~= 1 then
			if player:GetLevel() < 90 then
				self:ShowDialog(npc, player,
					"Ngươi chưa đạt đủ cấp độ để nhận kỹ năng môn phái. Hãy quay lại khi đạt <color=green>cấp 90!</color>")
				return
			end
			-- Kiểm tra túi đủ chỗ trống không
			if Player.HasFreeBagSpaces(player, 5) == false then
				self:ShowDialog(npc, player,
					"Túi của ngươi đã đầy, hãy sắp xếp <color=green>5 ô trống</color> trong túi để nhận kỹ năng 9x!")
				return
			end

			for key, value in pairs(SetItemSkill90) do
				if player:GetFactionID() == key then
					self:AddItem(player, value.ItemID1, 1, -1, 1)
					self:AddItem(player, value.ItemID2, 1, -1, 1)
					self:AddItem(player, value.ItemID3, 1, -1, 1)
					self:AddItem(player, value.ItemID4, 1, -1, 1)
				end
			end
			self:ShowDialog(npc, player,
				"Ngươi đã nhận kỹ năng thành công!")
			Player.SetValueOfForeverRecore(player, RecordKey90, 1)
			return
		else
			self:ShowDialog(npc, player,
				"Ngươi đã nhận kỹ năng rồi không thể nhận được nữa!")
			return
		end
	end
	--Lấy giá trí của key dùng 1 lần
	local record150 = Player.GetValueForeverRecore(player, RecordKey150)
	if selectionID == 10 then
		--ckeck nếu chưa nhận được thì vào hàm if else
		if record150 ~= 1 then
			--ckeck xem đủ lelevl hay chưa
			if player:GetLevel() < 150 then
				self:ShowDialog(npc, player,
					"Ngươi chưa đạt đủ cấp độ để nhận kỹ năng môn phái. Hãy quay lại khi đạt <color=green>cấp 150!</color>")
				return
			end
			-- Kiểm tra túi đủ chỗ trống không
			if Player.HasFreeBagSpaces(player, 5) == false then
				self:ShowDialog(npc, player,
					"Túi của ngươi đã đầy, hãy sắp xếp <color=green>5 ô trống</color> trong túi để nhận kỹ năng 150x!")
				return
			end
			--add rương bí kíp cấp 150
			if Player.AddItemLua(player, RuongBiKip150.Item, RuongBiKip150.number, RuongBiKip150.series, RuongBiKip150.bind) == true then
				self:ShowDialog(npc, player, "Nhận Kỹ năng <color=red>thành công</color>")
			else
				dialog:AddText("Lỗi không có vật phẩm")
			end
			-- set đã nhận thành công
			Player.SetValueOfForeverRecore(player, RecordKey150, 1)
			return
		else
			self:ShowDialog(npc, player,
				"Ngươi đã nhận kỹ năng rồi không thể nhận được nữa!")
			return
		end
	end
	if selectionID == 103 then
		-- Nếu toác
		if Global_NameMapItemPhaiID[scene:GetID()] == nil then
			self:ShowDialog(npc, player, "Thông tin môn phái bị lỗi, hãy thử lại sau!")
			return
		end

		-- Thông tin môn phái

		-- Nếu không phải đệ tử môn phái
		if player:GetFactionID() ~= factionInfo.FactionID then
			self:ShowDialog(npc, player, "Ngươi không phải đệ tử bản phái!")
			return
		end

		-- Đưa vào đấu trường thi đấu môn phái
		Player.JoinFactionBattle(npc, player)
		return
	end
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectedItemInfo: SelectItem - Vật phẩm được chọn
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function ChuongMon:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)
	-- ************************** --

	-- ************************** --
end

function ChuongMon:JoinFaction(scene, npc, player, factionID)
	-- ************************** --
	local ret = player:JoinFaction(factionID)
	-- ************************** --
	if ret == -1 then
		ChuongMon:ShowDialog(npc, player, "Người chơi không tồn tại!")
		return
	elseif ret == -2 then
		ChuongMon:ShowDialog(npc, player, "Môn phái không tồn tại!")
		return
	elseif ret == 0 then
		ChuongMon:ShowDialog(npc, player, "Giới tính của bạn không phù hợp với môn phái này!")
		return
	elseif ret == 1 then
		ChuongMon:ShowDialog(npc, player,
			"Gia nhập phái <color=blue>" .. player:GetFactionName() .. "</color> thành công!")
		return
	else
		ChuongMon:ShowDialog(npc, player, "Chuyển phái thất bại, lỗi chưa rõ!")
		return
	end
	-- ************************** --
end

function ChuongMon:ShowDialog(npc, player, text)
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:Show(npc, player)
	-- ************************** --
end

function ChuongMon:AddItem(player, Item, number, seri, Look)
	-- ************************** --
	if (Item == nil) then
		return
	end
	if (number == nil) then
		return
	end
	Player.AddItemLua(player, Item, number, seri, Look)
	-- ************************** --
end
