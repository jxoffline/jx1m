-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local XoYo_HuangFei = Scripts[500003]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function XoYo_HuangFei:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("Nghe đồn Tiêu Dao Cốc có rất nhiều kho báu, ta muốn tới đó dể khám phá nhưng chưa tìm được đồng hành nên vẫn chưa thể vào được. Xem ra ngươi có khả năng phi thường, nếu có thể vào trong cốc thử thách cũng như truy lùng báu vật, sau mỗi tháng ngươi có thể đến chỗ ta nhận thưởng tích lũy căn cứ thứ hạng đạt được tháng trước. Ngươi muốn gì?")
	dialog:AddSelection(1, "Ta có mấy món đồ quý")
	--dialog:AddSelection(2, "Xem tích lũy tháng này")
	--dialog:AddSelection(3, "Nhận thưởng tích lũy tháng trước")
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
function XoYo_HuangFei:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 100 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		Player.OpenShop(npc, player, 132)
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 2 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Tháng này ngươi đạt được:<br>   Tổng tích lũy: <color=yellow>" .. EventManager.XoYo_GetCurrentMonthStoragePoint(player) .. " danh vọng</color><br>   Xếp hạng: <color=green>" .. EventManager.XoYo_GetCurrentMonthRank(player) .. "</color>")
		dialog:AddSelection(100, "Ta biết rồi...")
		dialog:Show(npc, player)
		return
	end
	-- ************************** --
	if selectionID == 3 then
		local ret = EventManager.XoYo_GetLastMonthAward(player)
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText(ret)
		dialog:AddSelection(100, "Ta biết rồi...")
		dialog:Show(npc, player)
		return
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
function XoYo_HuangFei:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --
	
	-- ************************** --

end