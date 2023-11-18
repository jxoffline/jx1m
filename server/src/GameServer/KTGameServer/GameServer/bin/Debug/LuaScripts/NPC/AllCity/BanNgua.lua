-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local BanNgua = Scripts[000001]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function BanNgua:OnOpen(scene, npc, player)

	-- ************************** --
        local dialog = GUI.CreateNPCDialog()
        dialog.AddText("Kẻ hành tẩu giang hồ đều cần phải có một con ngựa chiến, con ngựa của ta tuy không phải nổi tiếng nhưng sức chịu đựng của nó thật kinh người, tính nó lại ôn hòa, mà giá tiền cũng không đắt lắm. ")
        dialog.AddSelection(1,"Mở cửa hàng")
        dialog.AddSelection(2,"Ta chỉ đi ngang qua thôi !")
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
function BanNgua:OnSelection(scene, npc, player, selectionID)

	-- ************************** --
	if selectionID == 1 then
        local dialog = GUI.CreateNPCDialog()
        Player.OpenShop(npc,player,22)
        GUI.CloseDialog(player)
    elseif selectionID == 2 then
        local dialog = GUI.CreateNPCDialog()
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
function BanNgua:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --
	
	-- ************************** --

end