-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local KeoHoLo = Scripts[0000038]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function KeoHoLo:OnOpen(scene, npc, player)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText("Ngươi hãy tích cực tham gia các hoạt động của Võ Kiếm 2023 để lấy danh vọng mua đồ ở chỗ ta !")

	dialog.AddSelection(298, "Cửa hàng danh vọng tống kim tương dương	")
	dialog.AddSelection(299, "Cửa hàng danh vọng tống kim phượng tường	")
	dialog.AddSelection(300, "Cửa hàng danh vọng tống kim biện kinh	")
	dialog.AddSelection(301, "Cửa hàng danh vọng bạch hổ đường	")
	dialog.AddSelection(302, "Cửa hàng danh vọng dã tẩu	")
	dialog.AddSelection(303, "Cửa hàng danh vọng quân doanh	")
	dialog.AddSelection(500, "Cửa hàng danh vọng phong hỏa liên thành	")
	dialog.AddSelection(501, "Cửa hàng Công Thành Chiến")
	dialog.AddSelection(502, "Cửa hàng danh vọng Tần Lăng")
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
function KeoHoLo:OnSelection(scene, npc, player, selectionID)

	-- ************************** --
	if selectionID == 500 then
	local dialog = GUI.CreateNPCDialog()
		dialog.AddText("Ngươi hãy tích cực tham gia các hoạt động của Võ Kiếm 2023 để lấy danh vọng mua đồ ở chỗ ta !")
		dialog.AddSelection(304, "Cửa hàng danh vọng phong hỏa liên thành khắc kim	")
		dialog.AddSelection(305, "Cửa hàng danh vọng phong hỏa liên thành khắc mộc	")
		dialog.AddSelection(306, "Cửa hàng danh vọng phong hỏa liên thành khắc thủy	")
		dialog.AddSelection(307, "Cửa hàng danh vọng phong hỏa liên thành khắc hỏa	")
		dialog.AddSelection(308, "Cửa hàng danh vọng phong hỏa liên thành khắc thổ	")
		dialog.Show(npc, player)
		-- ************************** --
	end
	if selectionID == 501 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 228)
		GUI.CloseDialog(player)
	end
	if selectionID == 502 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 229)
		GUI.CloseDialog(player)
	end
	if selectionID == 298 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 298)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 299 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 298)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 300 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 300)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 301 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 301)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 302 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 302)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 303 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 303)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 304 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 304)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 305 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 305)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 306 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 306)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 307 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 307)
		GUI.CloseDialog(player)
		-- ************************** --
	end
	if selectionID == 308 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 308)
		GUI.CloseDialog(player)
		-- ************************** --
	end

	if selectionID == 309 then
		if player:GetLevel() >= 10 then
			if player:GetFactionID() == 1 then
				Player.OpenShop(npc, player, 309)
			elseif player:GetFactionID() == 2 then
				Player.OpenShop(npc, player, 310)
			elseif player:GetFactionID() == 3 then
				Player.OpenShop(npc, player, 311)
			elseif player:GetFactionID() == 4 then
				Player.OpenShop(npc, player, 312)
			elseif player:GetFactionID() == 5 then
				Player.OpenShop(npc, player, 313)
			elseif player:GetFactionID() == 6 then
				Player.OpenShop(npc, player, 314)
			elseif player:GetFactionID() == 7 then
				Player.OpenShop(npc, player, 315)
			elseif player:GetFactionID() == 8 then
				Player.OpenShop(npc, player, 316)
			elseif player:GetFactionID() == 9 then
				Player.OpenShop(npc, player, 317)
			elseif player:GetFactionID() == 10 then
				Player.OpenShop(npc, player, 318)
			elseif player:GetFactionID() == 11 then
				Player.OpenShop(npc, player, 319)
			end
			GUI.CloseDialog(player)
		else
			self:ShowDialog(npc, player, "Ngươi chưa đủ cấp để có thể tham quan cửa hàng!")
		end
	end
	if selectionID == 9999 then
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
function KeoHoLo:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --

	-- ************************** --

end

function KeoHoLo:ShowDialog(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:AddSelection(9999, "Kết thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --

end
