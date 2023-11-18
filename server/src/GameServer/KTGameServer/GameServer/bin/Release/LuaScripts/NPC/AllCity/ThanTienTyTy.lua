-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng

local SuGiaSuKien = Scripts[000001000]
local ChangeFactionCard = 2168 -- Thẻ đổi phái
-- ********************************* --
local CompensationItems = {
	RecordKey = 100056,						-- Khóa lưu trữ
	[1] = {
		ServerID = 1,
		Items = {
			{ItemID = 2167, Number = 1, Bound = 1, timeItem = 10080},
			
		},
	},
	--[[2] = {
		ServerID = 2,
		Items = {
			{ItemID = 2167, Number = 1, Bound = 1, timeItem = 10080},	
			
		},
	},
	[3] = {
		ServerID = 3,
		Items = {
			{ItemID = 2167, Number = 1, Bound = 1, timeItem = 10080},	
			
		},
	},
	[4] = {
		ServerID = 4,
		Items = {
			{ItemID = 2167, Number = 1, Bound = 1, timeItem = 10080},	
			
		},
	},
	[5] = {
		ServerID = 5,
		Items = {
			{ItemID = 2167, Number = 1, Bound = 1, timeItem = 10080},	
			
						
		},
	},
	[6] = {
		ServerID = 6,
		Items = {
			{ItemID = 2167, Number = 1, Bound = 1, timeItem = 10080},	
			
		},
	},]]
}

-- ********************************* --



--****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function SuGiaSuKien:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	local serverID = player:GetZoneID()
	local record = Player.GetValueForeverRecore(player, CompensationItems.RecordKey)
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("Chào mừng bằng hữu đến với <color=green>Võ kiếm </color>. Ta là <color=yellow>NPC Sứ Giả Sự Kiện</color>. Nếu bằng hữu gặp khó khăn gì, cần hỗ trợ hãy tới chỗ ta!")
	-- Nếu chưa nhận quà đền bù, hỗ trợ
	if record ~= 1 and CompensationItems[serverID] ~= nil then
		dialog:AddSelection(50000, "Nhận <color=green>Nhận thẻ đổi tên 7 Ngày</color>")
	end
	--dialog:AddSelection(2, "Luyện Hóa Đồ")
	dialog:AddSelection(50001, "Ta muốn đổi tên")
	dialog:AddSelection(30001, "Tiêu hủy vật phẩm")
	dialog:AddSelection(30002, "Ghép vật phẩm")
	--dialog:AddSelection(30003, "Vòng quay may mắn")
	dialog:AddSelection(30004, "Sự kiện Giỗ Tổ Hùng Vương ")
	dialog:AddSelection(30005, "Vòng quay Tuần")
	dialog:AddSelection(30006, "Thay đổi môn phái")
	dialog:AddSelection(1000, "Kết thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function SuGiaSuKien:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	if selectionID == 1000 then
		GUI.CloseDialog(player)
		return
	end
	if selectionID == 2 then
		GUI.OpenUI(player, "UIEquipLevelUp")
	end
	
	if selectionID == 30002 then
		-- Mở khung ghép vật phẩm
		GUI.OpenMergeItems(player)
		
		-- Đóng khung
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	-- ************************** --
	if selectionID == 30001 then
		GUI.OpenRemoveItems(player)
		GUI.CloseDialog(player)
		return
	end
	if selectionID == 30003 then
		-- Mở khung vong quay
		GUI.OpenLuckyCircle(player)
		GUI.CloseDialog(player)
		return
		-- Đóng khung
	end
	if selectionID == 30004 then
		-- 
		Player.SpecialEventClick(npc, player)
		
		-- Đóng khung
		
		return
	end
	if selectionID == 50000 then
		local serverID = player:GetZoneID()
		local record = Player.GetValueForeverRecore(player, CompensationItems.RecordKey)
		
		-- Nếu đã nhận rồi
		if record == 1 then
			SuGiaSuKien:ShowDialog(npc, player, "Bằng hữu đã nhận <color=green>quà thành công</color>!")
			return
		end
		
		-- Toác
		if CompensationItems[serverID] == nil then
			SuGiaSuKien:ShowDialog(npc, player, "Bằng hữu không có quà để nhận!")
			return
		end
		
		-- Tổng số khoảng trống trong túi cần
		local totalSpacesNeed = 5--0
		
		-- Toác, tạm comment
		-- Tính tổng khoảng trống trong túi cần
		--for k, itemInfo in pairs(CompensationItems[serverID].Items) do
		--	totalSpacesNeed = totalSpacesNeed + Player.GetTotalSpacesNeedToTakeItem(itemInfo.ItemID, itemInfo.Number)
		--end
			-- Nếu không đủ khoảng trống
		if Player.HasFreeBagSpaces(player, totalSpacesNeed) == false then
			SuGiaSuKien:ShowDialog(npc, player, string.format("Bằng hữu cần sắp xếp tối thiểu <color=green>%d ô trống</color> trong túi đồ để nhận quà!", totalSpacesNeed))
			return
		end
		-- Đánh dấu đã nhận rồi
		Player.SetValueOfForeverRecore(player, CompensationItems.RecordKey, 1)
		
	
		
		-- Thêm vật phẩm tương ứng
		for k, itemInfo in pairs(CompensationItems[serverID].Items) do
				Player.AddItemLua(player, itemInfo.ItemID, itemInfo.Number, -1, itemInfo.Bound,0,itemInfo.timeItem)
			-- public static bool AddItemLua(Lua_Player player, int ItemID, int Number, int Series, int LockStatus, int enhanceLevel = 0, int ExpriestimeItem = -1)
		end		
		SuGiaSuKien:ShowDialog(npc, player, "Nhận <color=green>quà</color> thành công. Rất cảm ơn bằng hữu đã luôn đồng hành cùng <color=green>Võ Kiếm - Mobile</color>!")
		return
	end
	if selectionID == 30005 then
		GUI.OpenTurnPlate(player)
		GUI.CloseDialog(player)
	end
	if selectionID == 30006 then
		if (Player.IsHaveEquipBody(player)) ==false then
			-- Nếu không có thẻ đổi phái
			if Player.CountItemInBag(player, ChangeFactionCard) <= 0 then
				SuGiaSuKien:ShowDialog(npc, player,
					"Chức năng này yêu cầu <color=yellow>[Thẻ Đổi Môn Phái]</color>. Khi nào có hãy đến tìm ta.")
				return
				-- Nếu có chiến đội tham gia Võ lâm liên đấu, và đang trong thời gian diễn ra Võ lâm liên đấu thì không thể đổi phái
			elseif EventManager.TeamBattle_IsBattleTimeToday() == true and EventManager.TeamBattle_IsRegistered(player) == true then
				SuGiaSuKien: ShowDialog(npc, player, "Trong thời gian nhậy cảm không thể đổi phái ngươi hay quay lại sau.")
				return
			end

			-- Chọn môn phái
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText("Người muốn đổi qua môn phái nào?")
			for key, value in pairs(Global_FactionName) do
				if key > 0 and player:GetFactionID() ~= key then
					dialog:AddSelection(100 + key, value)
				end
			end
			dialog:Show(npc, player)
			return
		else
			SuGiaSuKien:ShowDialog(npc, player, "Ngươi phải tháo hết đồ xuống mới đổi được phái")
		end

	end
	if selectionID == 50001 then
		GUI.OpenChangeName(player)
		GUI.CloseDialog(player)
		return
	end
	if selectionID >= 101 and selectionID <= #Global_FactionName + 100 then
		-- Nếu không có thẻ đổi phái
		if Player.CountItemInBag(player, ChangeFactionCard) <= 0 then
		SuGiaSuKien:ShowDialog(npc, player,
				"Chức năng này yêu cầu <color=yellow>[Thẻ Đổi Môn Phái]</color>. Khi nào có hãy đến tìm ta.")
			return
			-- Nếu có chiến đội tham gia Võ lâm liên đấu, và đang trong thời gian diễn ra Võ lâm liên đấu thì không thể đổi phái
		elseif EventManager.TeamBattle_IsBattleTimeToday() == true and EventManager.TeamBattle_IsRegistered(player) == true then
		SuGiaSuKien:ShowDialog(npc, player, "Trong thời gian nhậy cảm không thể đổi phái ngươi hay quay lại sau.")
			return
		end

		-- ID môn phái tương ứng
		local factionID = selectionID - 100
		-- Nếu giới tính không phù hợp
		if player:GetSex() == 0 and factionID == Global_FactionID.EMei then
		SuGiaSuKien:ShowDialog(npc, player,
				"Thật đáng tiếc, chưởng môn phái <color=green>Nga My</color> không tiếp nhận <color=green>nam đệ tử</color>, ngươi hãy chọn môn phái khác.")
			return
		elseif player:GetSex() == 0 and factionID == Global_FactionID.CuiYan then
		SuGiaSuKien:ShowDialog(npc, player,
				"Thật đáng tiếc, phương trượng trụ trì phái <color=green>Thúy Yên</color> không tiếp nhận <color=green>nam đệ tử</color>, ngươi hãy chọn môn phái khác.")
			return
		elseif player:GetSex() == 1 and factionID == Global_FactionID.ShaoLin then
		SuGiaSuKien:ShowDialog(npc, player,
				"Thật đáng tiếc, phương trượng trụ trì phái <color=green>Thiếu Lâm</color> không tiếp nhận <color=green>nữ đệ tử</color>, ngươi hãy chọn môn phái khác.")
			return
		end

		-- Xóa thẻ đổi phái
		Player.RemoveItem(player, ChangeFactionCard, 1)

		-- Thực hiện tẩy tiềm năng
		player:UnAssignRemainPotentialPoints()
		-- Thực hiện đổi sang môn phái tương ứng
		SuGiaSuKien:JoinFaction(scene, npc, player, factionID)

		return
	end


	-- ************************** --
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function SuGiaSuKien:OnItemSelected(scene, npc, player, itemID,otherParams)

	-- ************************** --

	-- ************************** --

end


function SuGiaSuKien:ShowDialog(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:Show(npc, player)
	-- ************************** --
	
end
function SuGiaSuKien:JoinFaction(scene, npc, player, factionID)

	-- ************************** --
	local ret = player:JoinFaction(factionID)
	-- ************************** --
	if ret == -1 then
	SuGiaSuKien:ShowDialog(npc, player, "Người chơi không tồn tại!")
		return ret
	elseif ret == 1 then
	SuGiaSuKien:ShowDialog(npc, player,
			"Tẩy tủy thành công! Môn phái cùng toàn bộ điểm tiềm năng và kỹ năng của ngươi đã được phân phối lại")
		return
	end
	-- ************************** --

end

