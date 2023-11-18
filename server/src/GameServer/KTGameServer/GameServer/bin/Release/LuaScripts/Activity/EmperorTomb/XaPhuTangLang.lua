-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000013' bên dưới thành ID tương ứng
local XaPhuTangLang = Scripts[400032]

--****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function XaPhuTangLang:OnOpen(scene, npc, player,otherParams)

	-- ************************** --
	
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText("Muốn đi ra khỏi đây ư? ta giúp ngươi!!")
	dialog.AddSelection(1, "Rời khỏi Tần Lăng")
	dialog.AddSelection(99999, "Kết thúc đối thoại")
	dialog.Show(npc, player)
		
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function XaPhuTangLang:OnSelection(scene, npc, player, selectionID, otherParams)
	
		-- ************************** --
		if selectionID == 1 then
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText("Ngươi muốn đi đâu ?")
			for key ,value in pairs (Global_NameMapThanhID) do
				if value.ID ~= scene:GetID() then
					dialog:AddSelection(key,value.Name)
				end
			end
			dialog.AddSelection(99999, "Kết thúc đối thoại")
			dialog:Show(npc,player)
			return
		end
		if Global_NameMapThanhID[selectionID] ~= nil then
			player:ChangeScene(Global_NameMapThanhID[selectionID].ID,Global_NameMapThanhID[selectionID].PosX,Global_NameMapThanhID[selectionID].PosY)
			GUI.CloseDialog(player)
			return
		-- ************************** --
		end
		if selectionID == 99999 then
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
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function XaPhuTangLang:OnItemSelected(scene, npc, player, itemID, otherParams)

	-- ************************** --
	
	-- ************************** --

end
function XaPhuTangLang:ChangeScene(player,ID,PosX, PosY)

	-- ************************** --
		XaPhuTangLang:ChangeScene(player,ID,Posx,PosY)
	
	-- ************************** --

end
function XaPhuTangLang:ShowDialog(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:Show(npc, player)
	-- ************************** --
	
end