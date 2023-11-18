-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local TeamBattle_BattleHallNPC = Scripts[400020]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function TeamBattle_BattleHallNPC:OnOpen(scene, npc, player)

	-- ************************** --
	local szNextBattleTime = EventManager.TeamBattle_GetNextBattleTime()
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	if szNextBattleTime ~= "FAILED" then
		dialog:AddText("Đây là hội trường <color=green>Võ lâm liên đấu</color>.")
		dialog:AddText("Thời gian trận đấu tiếp theo là <color=yellow>" .. szNextBattleTime .. "</color>, bậc <color=#ff70e0>" .. EventManager.TeamBattle_GetNextBattleStage() .. "</color>.")
		dialog:AddText("<color=orange> - Lưu ý:</color> Sau khi <color=orange>báo danh</color> thành công, toàn bộ thành viên chiến đội phải <color=green>đứng yên</color> tại hội trường, các trường hợp thành viên <color=yellow>rời hội trường</color> vì một lý do nào đó đều sẽ <color=green>hủy báo danh</color> thi đấu của chiến đội, và phải tiến hành <color=yellow>báo danh lại</color>.")
		dialog:AddText("")
		dialog:AddText(string.format("Hiện tại có tổng số <color=green>%d chiến đội</color> đã báo danh cho trận kế tiếp!", EventManager.TeamBattle_GetTotalRegisteredForNextBattleTeams()))
		dialog:AddText("")
		-- Nếu chiến đội đã báo danh
		if EventManager.TeamBattle_IsRegisteredForNextBattle(player) == true then
			dialog:AddText("<color=#0ac2ff>Chiến đội đã báo danh trận kế tiếp, hãy chuẩn bị tốt!</color>")
		else
			dialog:AddText("<color=red>Chiến đội chưa báo danh trận kế tiếp, hãy mau mau!</color>")
		end
		
		-- Nếu chiến đội đã đăng ký Võ lâm liên đấu
		if EventManager.TeamBattle_IsRegistered(player) == true then
			dialog:AddSelection(1, "Xem tình hình chiến đội bản thân")
		end
		
		-- Nếu hôm nay có trận đấu, và chiến đội đã đăng ký tham dự Võ lâm liên đấu
		if EventManager.TeamBattle_IsBattleTimeToday() == true and EventManager.TeamBattle_IsRegistered(player) == true then
			-- Nếu chiến đội chưa báo danh
			if player:HasTeam() == true and player:IsTeamLeader() == true and EventManager.TeamBattle_IsRegisteredForNextBattle(player) == false then
				dialog:AddSelection(2, "Báo danh trận tiếp theo")
			end
		end
	else
		dialog:AddText("<color=green>Võ lâm liên đấu</color> hôm nay không có trận đấu nào, hãy quay lại sau!")
		
		-- Nếu chiến đội đã đăng ký Võ lâm liên đấu
		if EventManager.TeamBattle_IsRegistered(player) == true then
			dialog:AddSelection(1, "Xem tình hình chiến đội bản thân")
		end
	end

	dialog:AddSelection(3, "Xem bảng xếp hạng Top chiến đội")
	dialog:AddSelection(4, "Cửa Hàng Liên đấu")
	dialog:AddSelection(100, "Kết thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function TeamBattle_BattleHallNPC:OnSelection(scene, npc, player, selectionID)

	-- ************************** --
	if selectionID == 100 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		local szTeamInfoText = EventManager.TeamBattle_GetMyTeamInfo(player)
		TeamBattle_BattleHallNPC:ShowNotify(npc, player, szTeamInfoText)
		return
	end
	-- ************************** --
	if selectionID == 2 then
		-- Nếu hôm nay không có trận đấu
		if EventManager.TeamBattle_IsBattleTimeToday() == false then
			TeamBattle_BattleHallNPC:ShowNotify(npc, player, "Hôm nay không có trận đấu nào, hãy quay lại vào đúng ngày!")
			return
		-- Nếu chưa có nhóm
		elseif player:HasTeam() == false then
			TeamBattle_BattleHallNPC:ShowNotify(npc, player, "Ngươi phải lập nhóm trước, sau đó mới có thể báo danh trận đấu kế tiếp.")
			return
		-- Nếu không phải nhóm trưởng
		elseif player:IsTeamLeader() == false then
			TeamBattle_BattleHallNPC:ShowNotify(npc, player, "Ngươi không phải trưởng nhóm, chỉ có trưởng nhóm mới có thể báo danh trận đấu kế tiếp.")
			return
		-- Nếu chiến đội chưa đăng ký tham gia Võ lâm liên đấu
		elseif EventManager.TeamBattle_IsRegistered(player) == false then
			TeamBattle_BattleHallNPC:ShowNotify(npc, player, "Chiến đội chưa đăng ký tham gia <color=green>Võ lâm liên đấu</color> tháng này.")
			return
		-- Nếu chiến đội đã báo danh
		elseif EventManager.TeamBattle_IsRegisteredForNextBattle(player) == true then
			TeamBattle_BattleHallNPC:ShowNotify(npc, player, "Chiến đội đã báo danh trận đấu kế tiếp rồi!")
			return
		end
		
		local szRegisterText = EventManager.TeamBattle_RegisterForNextBattle(player)
		TeamBattle_BattleHallNPC:ShowNotify(npc, player, szRegisterText)
		return
	end
	-- ************************** --
	if selectionID == 3 then
		-- Thực hiện truy vấn
		EventManager.TeamBattle_QueryTopTeam(player)
	
		GUI.CloseDialog(player)
		return
	end
	if selectionID == 4 then
		Player.OpenShop(npc, player, 78)
		GUI.CloseDialog(player)
	end
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectedItemInfo: SelectItem - Vật phẩm được chọn
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function TeamBattle_BattleHallNPC:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --
	
	-- ************************** --

end

-- ======================================================= --
-- ======================================================= --
function TeamBattle_BattleHallNPC:ShowNotify(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:AddSelection(100, "Kết thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --

end