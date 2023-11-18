-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000012' bên dưới thành ID tương ứng

local ThuKho = Scripts[0000222]

-- *************************************************s***** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function ThuKho:OnOpen(scene, npc, player,otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	local MapID = scene:GetID()
	dialog:AddText("Ta hứa sẽ luôn ở đây bảo quản đồ đạc thận cẩn thận")
	if((MapID==2) or( MapID==32) or (MapID==39)) then
		dialog:AddSelection(1, "Mở thương khố")
		dialog:AddSelection(2, "Lưu điểm hồi thành")
		dialog:AddSelection(3, "Kết thúc đối thoại")
		dialog:Show(npc, player)
	else
		dialog:AddSelection(1, "Mở thương khố")
		dialog:AddSelection(3, "Kết thúc đối thoại")
		dialog:Show(npc, player)
	end

	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function ThuKho:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	if selectionID == 1 then
		Player.PortableGoods(npc,player)
	elseif selectionID == 2 then
		local dialog = GUI.CreateNPCDialog()
		local pos = npc:GetPos()
		local posX = pos:GetX()
		local posY = pos:GetY()
		local mapID = scene:GetID()
		player:SetDefaultReliveInfo(mapID, posX, posY)
		dialog:AddText("Đã lưu điểm hồi thành")
		dialog:Show(npc, player)
	elseif selectionID == 3 then
       	GUI.CloseDialog(player)
		return
	-- ************************** --
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
function ThuKho:OnItemSelected(scene, npc, player, itemID, otherParams)

	-- ************************** --
	
	-- ************************** --

end