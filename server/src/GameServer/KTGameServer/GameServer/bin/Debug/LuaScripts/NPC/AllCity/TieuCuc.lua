-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local TieuCuc = Scripts[0000036]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function TieuCuc:OnOpen(scene, npc, player)
	
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText("<color=yellow>Vận tiêu</color> là sự kiện hàng ngày diễn ra từ <color=green>12:00:00</color> đến <color=green>23:59:59</color>. Trong thời gian diễn ra sự kiện, tới đây ta sẽ giao cho ngươi nhiệm vụ <color=yellow>vận tiêu</color>.")
	dialog.AddText("Có <color=green>3 loại</color>:")
	dialog.AddText("   - <color=yellow>Xe tiêu thường</color>")
	dialog.AddText("   - <color=yellow>Xe tiêu bạch kim</color>")
	dialog.AddText("   - <color=yellow>Xe tiêu hoàng kim</color>")
	dialog.AddText("Theo thứ tự sẽ tương ứng các loại <color=green>phần thưởng</color> khác nhau, quãng đường vận chuyển cũng khác nhau.")
	dialog.AddText("Ngươi muốn chọn loại <color=yellow>xe tiêu</color> nào?")
	dialog.AddSelection(1, "Xe tiêu thường")
	dialog.AddSelection(2, "Xe tiêu bạch kim")
	dialog.AddSelection(3, "Xe tiêu hoàng kim")
	dialog.AddSelection(9999, "Kết thúc đối thoại")
	dialog.Show(npc, player)
	-- ************************** --
	
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function TieuCuc:OnSelection(scene, npc, player, selectionID)
	
	-- ************************** --
	if selectionID == 9999 then
       	GUI.CloseDialog(player)
		return
	-- ************************** --
	end
	-- ************************** --
	if selectionID >= 1 and selectionID <= 3 then
		-- Gọi hàm hệ thống
		local strRet = EventManager.CargoCarriage_GiveTask(player, selectionID)
		-- Toác
		if strRet ~= "OK" then
			local dialog = GUI.CreateNPCDialog()
			dialog.AddText(strRet)
			dialog.AddSelection(9999, "Kết thúc đối thoại")
			dialog.Show(npc, player)
			return
		end

		-- Đóng khung
		--local dialog = GUI.CreateNPCDialog()
		--dialog.AddText("Chức năng tạm khoá.")
        --dialog.AddSelection(9999,"Kết thúc đối thoại!")
		--dialog:Show(npc, player)
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
function TieuCuc:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)
	
	-- ************************** --
	
	-- ************************** --
	
end