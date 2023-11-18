-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000021' bên dưới thành ID tương ứng
local TayTuyDaiSu = Scripts[00000197]
local ChangeFactionCard = 2168 -- Thẻ đổi phái

--****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function TayTuyDaiSu:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("A di đà phật, nơi này là <color=orange>Tẩy Tủy Đảo</color>, ta là đại sư phụ trách nơi đây. Bần tăng có thể giúp ngươi <color=yellow>phân phối lại</color> điểm <color=green>tiềm năng</color>, <color=green>kỹ năng</color>, cũng như <color=yellow>thay đổi môn phái</color>.")
	dialog:AddSelection(1, "Tẩy điểm tiềm năng ")
	dialog:AddSelection(2, "Tẩy điêm kỹ năng")
	dialog:AddSelection(3, "Tẩy điểm tiềm năng và kỹ năng")
	dialog:AddSelection(4, "Thay đổi môn phái")

	dialog:AddSelection(6, "Rời khỏi tẩy tủy đảo")
	dialog:AddSelection(99999, "Để ta suy nghĩ đã...")
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
function TayTuyDaiSu:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --


	-- ************************** --

	-- ************************** --
	if selectionID == 1 then
		if (Player.IsHaveEquipBody(player)) ==false then
			player:UnAssignRemainPotentialPoints()

			-- Thông báo
			TayTuyDaiSu:ShowNotify(npc, player, "Tẩy tủy thành công! Tất cả điểm tiềm năng của ngươi đã được phân phối lại.")
		else
			TayTuyDaiSu:ShowNotify(npc, player, "Ngươi phải tháo hết đồ xuống mới tẩy được điểm!")
		end

		-- Thực hiện tẩy tiềm năng

		return
	end
	-- ************************** --
	if selectionID == 2 then
		if (Player.IsHaveEquipBody(player)) ==false then
			player:ResetAllSkillsLevel()
			-- Thông báo
			TayTuyDaiSu:ShowNotify(npc, player, "Tẩy tủy thành công! Tất cả kỹ năng của ngươi đã được phân phối lại.")
		else
			TayTuyDaiSu:ShowNotify(npc, player, "Ngươi phải tháo hết đồ xuống mới tẩy được điểm!")
		end
		-- Thực hiện tẩy kỹ năng

		return
	end
	if selectionID == 3 then
		if (Player.IsHaveEquipBody(player)) ==false then
			-- Thực hiện tẩy tiềm năng
			player:UnAssignRemainPotentialPoints()
			-- Thực hiện tẩy kỹ năng
			player:ResetAllSkillsLevel()
			-- Thông báo
			TayTuyDaiSu:ShowNotify(npc, player,
				"Tẩy tủy thành công! Tất cả điểm tiềm năng và kỹ năng của ngươi đã được phân phối lại.")
		else
			TayTuyDaiSu:ShowNotify(npc, player, "Ngươi phải tháo hết đồ xuống mới tẩy được điểm!")

		end

	end
	if selectionID == 4 then
		if (Player.IsHaveEquipBody(player)) ==false then
			-- Nếu không có thẻ đổi phái
			if Player.CountItemInBag(player, ChangeFactionCard) <= 0 then
				TayTuyDaiSu:ShowNotify(npc, player,
					"Chức năng này yêu cầu <color=yellow>[Thẻ Đổi Môn Phái]</color>. Khi nào có hãy đến tìm ta.")
				return
				-- Nếu có chiến đội tham gia Võ lâm liên đấu, và đang trong thời gian diễn ra Võ lâm liên đấu thì không thể đổi phái
			elseif EventManager.TeamBattle_IsBattleTimeToday() == true and EventManager.TeamBattle_IsRegistered(player) == true then
				TayTuyDaiSu:ShowNotify(npc, player, "Trong thời gian nhậy cảm không thể đổi phái ngươi hay quay lại sau.")
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
			TayTuyDaiSu:ShowNotify(npc, player, "Ngươi phải tháo hết đồ xuống mới đổi được phái")
		end

	end
	--if selectionID == 5 then
	--	GUI.OpenChangeSignetMantleAndChopstick(player)
	--	GUI:CloseDialog(player)
	--	return
--	end
	-- ************************** --
	if selectionID == 6 then
		player:ChangeScene(1, 4450, 2100)
		GUI.CloseDialog(player)
		return
	end
	if selectionID >= 101 and selectionID <= #Global_FactionName + 100 then
		-- Nếu không có thẻ đổi phái
		if Player.CountItemInBag(player, ChangeFactionCard) <= 0 then
			TayTuyDaiSu:ShowNotify(npc, player,
				"Chức năng này yêu cầu <color=yellow>[Thẻ Đổi Môn Phái]</color>. Khi nào có hãy đến tìm ta.")
			return
			-- Nếu có chiến đội tham gia Võ lâm liên đấu, và đang trong thời gian diễn ra Võ lâm liên đấu thì không thể đổi phái
		elseif EventManager.TeamBattle_IsBattleTimeToday() == true and EventManager.TeamBattle_IsRegistered(player) == true then
			TayTuyDaiSu:ShowNotify(npc, player, "Trong thời gian nhậy cảm không thể đổi phái ngươi hay quay lại sau.")
			return
		end

		-- ID môn phái tương ứng
		local factionID = selectionID - 100
		-- Nếu giới tính không phù hợp
		if player:GetSex() == 0 and factionID == Global_FactionID.EMei then
			TayTuyDaiSu:ShowNotify(npc, player,
				"Thật đáng tiếc, chưởng môn phái <color=green>Nga My</color> không tiếp nhận <color=green>nam đệ tử</color>, ngươi hãy chọn môn phái khác.")
			return
		elseif player:GetSex() == 0 and factionID == Global_FactionID.CuiYan then
			TayTuyDaiSu:ShowNotify(npc, player,
				"Thật đáng tiếc, phương trượng trụ trì phái <color=green>Thúy Yên</color> không tiếp nhận <color=green>nam đệ tử</color>, ngươi hãy chọn môn phái khác.")
			return
		elseif player:GetSex() == 1 and factionID == Global_FactionID.ShaoLin then
			TayTuyDaiSu:ShowNotify(npc, player,
				"Thật đáng tiếc, phương trượng trụ trì phái <color=green>Thiếu Lâm</color> không tiếp nhận <color=green>nữ đệ tử</color>, ngươi hãy chọn môn phái khác.")
			return
		end

		-- Xóa thẻ đổi phái
		Player.RemoveItem(player, ChangeFactionCard, 1)

		-- Thực hiện tẩy tiềm năng
		player:UnAssignRemainPotentialPoints()
		-- Thực hiện đổi sang môn phái tương ứng
		TayTuyDaiSu:JoinFaction(scene, npc, player, factionID)

		return
	end

	-- ************************** --
	if selectionID == 99999 then
		GUI:CloseDialog(player)
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
function TayTuyDaiSu:OnItemSelected(scene, npc, player, itemID, otherParams)

	-- ************************** --

	-- ************************** --

end

-- ======================================================= --
-- ======================================================= --
function TayTuyDaiSu:JoinFaction(scene, npc, player, factionID)

	-- ************************** --
	local ret = player:JoinFaction(factionID)
	-- ************************** --
	if ret == -1 then
		TayTuyDaiSu:ShowNotify(npc, player, "Người chơi không tồn tại!")
		return ret
	elseif ret == 1 then
		TayTuyDaiSu:ShowNotify(npc, player,
			"Tẩy tủy thành công! Môn phái cùng toàn bộ điểm tiềm năng và kỹ năng của ngươi đã được phân phối lại")
		return
	end
	-- ************************** --

end

-- ======================================================= --
-- ======================================================= --
function TayTuyDaiSu:ShowNotify(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:AddSelection(99999, "Kết thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --

end
