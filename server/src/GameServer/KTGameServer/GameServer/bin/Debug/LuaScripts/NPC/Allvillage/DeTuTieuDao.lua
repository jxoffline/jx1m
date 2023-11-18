-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local DeTuTieuDao = Scripts[000001400]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function DeTuTieuDao:OnOpen(scene, npc, player)

	-- ************************** --
        local dialog = GUI.CreateNPCDialog()
        dialog.AddText("Ẩn cư đã lâu nhưng không vì thế mà bổn môn thất thế trên giang hồ, hãy gia nhập làm đệ tử bổn môn ngươi sẽ không phải hối hận đâu! haha .")
        dialog.AddSelection(1,"Về môn phái!")
        dialog.AddSelection(9999,"Kết thúc đối thoại!")
        dialog.Show(npc,player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function DeTuTieuDao:OnSelection(scene, npc, player, selectionID)

	-- ************************** --
	if selectionID == 9999 then
       	GUI.CloseDialog(player)
	-- ************************** --
    end
	if selectionID == 1 then
       		player:ChangeScene(95, 4240, 2100)
			GUI.CloseDialog(player)
		return
	-- ************************** --
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
function DeTuTieuDao:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --
	
	-- ************************** --

end