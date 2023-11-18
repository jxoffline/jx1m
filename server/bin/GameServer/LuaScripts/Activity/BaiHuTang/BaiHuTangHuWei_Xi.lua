-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local BaiHuTangHuWei_Xi = Scripts[400009]

-- ************************** --
local Details = {
	[43] = {
		RequireLevel = {Min = 60, Max = 89},
		EnterScene = {
			SceneID = 45,			-- Bạch Hổ Đường 1_Tây (sơ)
			PosX = 1449,
			PosY = 821,
		},
	},
	[51] = {
		RequireLevel = {Min = 90, Max = 201},
		EnterScene = {
			SceneID = 53,			-- Bạch Hổ Đường 1_Tây (cao)
			PosX = 1449,
			PosY = 821,
		},
	},
}
-- ************************** --

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function BaiHuTangHuWei_Xi:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	if EventManager.IsBaiHuTangRegisterTime() == false then
		dialog:AddText("Hiện không phải thời gian báo danh <color=green>Bạch Hổ Đường</color>. Hãy quay lại sau!")
	else
		dialog:AddText("Lúc này, rất nhiều võ lâm cao thủ, ẩn sỹ giang hồ đang chuẩn bị tiến vào tranh đoạt chức vị <color=yellow>võ lâm minh chủ</color> tại <color=green>Bạch Hổ Đường</color>. Các hạ có muốn cùng tham gia không?")
		dialog:AddSelection(1, "Tiến vào <color=yellow>Bạch Hổ Đường - Tây</color>")
	end
	dialog:AddSelection(100, "Kết thúc đối thoại")
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
function BaiHuTangHuWei_Xi:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 100 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		if Details[scene:GetID()] == nil then
			self:ShowDialog(npc, player, "Hoạt động bị lỗi. Hãy liên hệ với hỗ trợ để được trợ giúp!")
			return
		elseif player:GetLevel() < Details[scene:GetID()].RequireLevel.Min or player:GetLevel() > Details[scene:GetID()].RequireLevel.Max then
			self:ShowDialog(npc, player, string.format("Tại đây yêu cầu cấp độ từ <color=green>%d</color> đến <color=green>%d</color>!", Details[scene:GetID()].RequireLevel.Min, Details[scene:GetID()].RequireLevel.Max))
			return
		elseif EventManager.IsBaiHuTangRegisterTime() == false then
			self:ShowDialog(npc, player, "Hiện không phải thời gian báo danh <color=green>Bạch Hổ Đường</color>. Hãy quay lại sau!")
			return
		elseif EventManager.BaiHuTang_HasCompletedToday(player) == true then
			self:ShowDialog(npc, player, "Ngày hôm nay các hạ đã tham gia <color=green>Bạch Hổ Đường</color> rồi. Ngày mai hãy quay lại!")
			return
		end
		
		EventManager.BaiHuTang_SetEnteredToday(player)
		player:ChangeScene(Details[scene:GetID()].EnterScene.SceneID, Details[scene:GetID()].EnterScene.PosX, Details[scene:GetID()].EnterScene.PosY)
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
function BaiHuTangHuWei_Xi:OnItemSelected(scene, npc, player, itemID, otherParams)

	-- ************************** --
	
	-- ************************** --

end

-- ======================================================= --
-- ======================================================= --
function BaiHuTangHuWei_Xi:ShowDialog(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:Show(npc, player)
	-- ************************** --
	
end

