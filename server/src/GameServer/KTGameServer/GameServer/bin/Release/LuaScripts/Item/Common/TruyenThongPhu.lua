-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng

local ThanHanhPhu = Scripts[200005]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function ThanHanhPhu:OnPreCheckCondition(scene, item, player, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> ThanHanhPhu:OnPreCheckCondition")--
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
	-- ************************** --
	local nLevel = item:GetItemLevel()
	local dialog = GUI.CreateItemDialog()
	dialog:AddText("Ngươi muốn đi đâu?")
	dialog:AddSelection(1, "Thành thị")
	dialog:AddSelection(2, "Tân thủ thôn")
	dialog:AddSelection(3, "Môn phái")
	dialog:AddSelection(99999, "Ta chưa cần...")
	dialog:Show(item, player)
	-- số ngày n ăn được bao nhiêu 
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
	--player:AddNotification("Enter -> ThanHanhPhu:OnSelection, selectionID = " .. selectionID)--
	if selectionID == 99999 then
		GUI.CloseDialog(player)
		return
	  end
	  -- ************************** --
	  if selectionID == 1 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Ngươi muốn đi đâu?")
		for mapID, mapInfo in ipairs(Global_TeleportMaps[MapType.City]) do
		  dialog:AddSelection(mapID + 1000, mapInfo.Name)
		end
		dialog:Show(item, player)
			return
	  end
	  -- ************************** --
	  if selectionID == 2 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Ngươi muốn đi đâu?")
		for mapID, mapInfo in ipairs(Global_TeleportMaps[MapType.village]) do
		  dialog:AddSelection(mapID + 2000, mapInfo.Name)
		end
		dialog:Show(item, player)
			return
	  end
	  -- ************************** --
	  if selectionID == 3 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Ngươi muốn đi đâu?")
		for mapID, mapInfo in ipairs(Global_TeleportMaps[MapType.Faction]) do
		  dialog:AddSelection(mapID + 3000, mapInfo.Name)
		end
		dialog:Show(item, player)
			return
	  end
	  -- ************************** --
		if selectionID >= 1000 and selectionID < 9999 then
			-- ID bản đồ
			local mapID = selectionID
			-- Loại bản đồ
			local MapType
			if selectionID >= 1000 and selectionID <= 1999 then
				MapType = MapType.City
		  mapID = selectionID-1000
			elseif selectionID >= 2000 and selectionID <= 2999 then
				MapType = MapType.village
		  mapID = selectionID-2000
			elseif selectionID >= 3000 and selectionID <= 3999 then
				MapType = MapType.Faction
		  mapID = selectionID-3000
			end
			-- Thông tin bản đồ
			local mapInfo = Global_TeleportMaps[MapType][mapID]
			-- Dịch chuyển đến bản đồ tương ứng
			player:ChangeMap(mapID, mapInfo.PosX, mapInfo.PosY)
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