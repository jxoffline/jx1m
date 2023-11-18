-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local ThuyenNgu = Scripts[0000031]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function ThuyenNgu:OnOpen(scene, npc, player)
	
	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText("Muốn ngồi thuyền đi đâu vậy?")
	dialog.AddSelection(1, "Ngồi thuyền!")
	dialog.AddSelection(9999, "Kết thúc đối thoại!")
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
function ThuyenNgu:OnSelection(scene, npc, player, selectionID)
	
	-- ************************** --
	if selectionID == 1 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Ngươi muốn đi đâu ?")
		dialog:AddSelection(101,"Không ngồi")
		for key, value in pairs(Global_NameMapThanhID) do
			if key ~= scene:GetID() then
				dialog:AddSelection(key, value.Name)
			end
		end
		dialog:Show(npc, player)
	elseif selectionID == 101 then
		GUI.CloseDialog(player)
	-- ************************** --
	end
	if Global_NameMapThanhID[selectionID] ~= nil then
		player:ChangeScene(Global_NameMapThanhID[selectionID].ID,Global_NameMapThanhID[selectionID].PosX,Global_NameMapThanhID[selectionID].PosY)
		GUI.CloseDialog(player)
    end
    if selectionID == 9999 then
        GUI.CloseDialog(player)
		return
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
function ThuyenNgu:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)
	
	-- ************************** --
	
	-- ************************** --
	
end