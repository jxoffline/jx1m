-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000033' bên dưới thành ID tương ứng
local LuyenKhiDaiSu = Scripts[0000194]

--****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function LuyenKhiDaiSu:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	Player.SetLastEquipMasterNPC(player, npc)
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("Cường hóa làm tăng thuộc tính của trang bị, cũng như giúp kích hoạt những thuộc tính ẩn, cấp cường hóa càng cao thuộc tính trang bị càng cao.")
	dialog:AddSelection(1, "Cường hóa vật phẩm")
	dialog:AddSelection(2, "Chế Tạo Đồ")
	dialog:AddSelection(3, "Ghép Huyền Tinh")
	dialog:AddSelection(4, "Phân Giải Huyền Tinh Cấp Cao")
	dialog:AddSelection(5, "Thăng Cấp Ngũ Hành Ấn")
	dialog:AddSelection(6, "Muốn tách huyền tinh từ trang bị cao cấp")
	dialog:AddSelection(8, "Sửa trang bị")
	dialog:AddSelection(10, "Tách vật phẩm lấy nguyên liệu")
	dialog:AddSelection(11, "Mở khóa trang bị")
	dialog:AddSelection(12, "Luyện Hóa Đồ")
	dialog:AddSelection(999999, "Ta chỉ đi ngang qua")
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
function LuyenKhiDaiSu:OnSelection(scene, npc, player, selectionID,otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	if selectionID == 1 then
		GUI.OpenUI(player, "UIEnhance")
	elseif selectionID == 2 then
		GUI.OpenUI(player, "UICrafting")
	elseif selectionID == 3 then
		GUI.OpenUI(player, "UICrystalStoneSynthesis")
	elseif selectionID == 4 then
		GUI.OpenUI(player, "UISplitCrystalStone", 6, 80)
	elseif selectionID == 5 then
		GUI.OpenUI(player, "UISignetEnhance")
	elseif selectionID == 6 then	
		GUI.OpenUI(player, "UISplitEquipCrystalStones", 6, 70)
	elseif selectionID == 8 then
		GUI.CloseDialog(player)
		Player.RepairAllItem(player)
	elseif selectionID == 10 then
		GUI.CloseDialog(player)
		Player.CallBreakingItem(player)
	elseif selectionID == 11 then
		GUI.CloseDialog(player)
		Player.UnlockItem(player)
	elseif selectionID == 999999 then
       	GUI.CloseDialog(player)
		return
    end
	if selectionID == 12 then
		GUI.OpenUI(player, "UIEquipLevelUp")
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
function LuyenKhiDaiSu:OnItemSelected(scene, npc, player, itemID,otherParams)

	-- ************************** --

	-- ************************** --

end