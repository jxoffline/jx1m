-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local ThanHanhPhu = Scripts[200002]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function ThanHanhPhu:OnPreCheckCondition(scene, item, player, otherParams)

	-- ************************** --
	return true
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi để thực thi Logic khi người sử dụng vật phẩm, sau khi đã thỏa mãn hàm kiểm tra điều kiện
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function ThanHanhPhu:OnUse(scene, item, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	dialog:AddText("Ngươi muốn đi đâu?")
	dialog:AddSelection(101, "Thành thị")
	dialog:AddSelection(102, "Tân thủ thôn")
	dialog:AddSelection(103, "Môn phái")
	if Global_TeleportMiddle[scene:GetID()] then
		dialog:AddSelection(104, "Về giữa bản đồ")
	end
	
	dialog:AddSelection(100, "Ta chưa cần...")
	dialog:Show(item, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function ThanHanhPhu:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 101 then
		local dialog = GUI.CreateItemDialog()
		dialog:AddText("Ngươi muốn đi đâu?")
		for key, value in pairs(Global_NameMapItemThanhID) do
			--if value.ID ~= scene:GetID() then
			dialog:AddSelection(key, value.Name)
			--end
		end
		dialog:AddSelection(100, "Ta chưa cần...")
		dialog:Show(item, player)
		return
	end
	-- ************************** --
	if selectionID == 102 then
		local dialog = GUI.CreateItemDialog()
		dialog:AddText("Ngươi muốn đi đâu?")
		for key, value in pairs(Global_NameMapItemThonID) do
			if value.ID ~= scene:GetID() then
				dialog:AddSelection(key, value.Name)
			end
		end
		dialog:AddSelection(100, "Ta chưa cần...")
		dialog:Show(item, player)
		return
	end
	-- ************************** --
	if selectionID == 103 then
		local dialog = GUI.CreateItemDialog()
		dialog:AddText("Ngươi muốn đi đâu?")
		for key, value in pairs(Global_NameMapItemPhaiID) do
			if value.ID ~= scene:GetID() then
				dialog:AddSelection(key, value.Name)
			end
		end
		dialog:AddSelection(100, "Ta chưa cần...")
		dialog:Show(item, player)
	end
	if selectionID == 104 then
		for key, value in pairs(Global_TeleportMiddle) do
			if value.ID == scene:GetID() then
				player:ChangeScene(key, value.PosX, value.PosY)
			end
		end
	end
	-- ************************** --
	if selectionID == 100 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if Global_NameMapItemThanhID[selectionID] ~= nil then
		player:ChangeScene(Global_NameMapItemThanhID[selectionID].ID, Global_NameMapItemThanhID[selectionID].PosX,
			Global_NameMapItemThanhID[selectionID].PosY)
		GUI.CloseDialog(player)
		return
	elseif Global_NameMapItemThonID[selectionID] ~= nil then
		player:ChangeScene(Global_NameMapItemThonID[selectionID].ID, Global_NameMapItemThonID[selectionID].PosX,
			Global_NameMapItemThonID[selectionID].PosY)
		GUI.CloseDialog(player)
		return
	elseif Global_NameMapItemPhaiID[selectionID] ~= nil then
		player:ChangeScene(Global_NameMapItemPhaiID[selectionID].ID, Global_NameMapItemPhaiID[selectionID].PosX,
			Global_NameMapItemPhaiID[selectionID].PosY)
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function ThanHanhPhu:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end
