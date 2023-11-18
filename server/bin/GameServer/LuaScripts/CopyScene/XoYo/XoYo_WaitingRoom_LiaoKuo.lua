-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local XoYo_WaitingRoom_LiaoKuo = Scripts[500001]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function XoYo_WaitingRoom_LiaoKuo:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("Nơi đây là Tiêu Dao Cốc, vượt ải khó khăn không thể thiếu thuốc và thức ăn. Ngươi muốn mua không?")
	dialog:AddSelection(1, "Thuốc")
	dialog:AddSelection(2, "Thức ăn")
	dialog:AddSelection(100, "Ta chỉ ghé qua...")
	dialog:Show(npc, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		selectionID: number - ID chức năng
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function XoYo_WaitingRoom_LiaoKuo:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 1 then
		Player.OpenShop(npc, player, 14)
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 2 then
		Player.OpenShop(npc, player, 21)
		GUI.CloseDialog(player)
	end
	-- ************************** --
	if selectionID == 100 then
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
function XoYo_WaitingRoom_LiaoKuo:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --
	
	-- ************************** --

end