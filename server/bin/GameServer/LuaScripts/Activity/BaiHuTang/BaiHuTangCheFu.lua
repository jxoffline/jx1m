-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000013' bên dưới thành ID tương ứng
local BaiHuTangCheFu = Scripts[400011]

--****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function BaiHuTangCheFu:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("Ngươi muốn đi đâu?")
	dialog:AddSelection(101, "Thành thị")
	dialog:Show(npc, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function BaiHuTangCheFu:OnSelection(scene, npc, player, selectionID,otherParams)

	-- ************************** --
	if selectionID == 101 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Ngươi muốn đi đâu?")
		for key ,value in pairs (Global_NameMapThanhID) do
			if key ~= scene:GetID() then
				dialog:AddSelection(key,value.Name)
			end
		end
		dialog:Show(npc,player)
		return
	end  
	-- ************************** --
	if Global_NameMapThanhID[selectionID] ~= nil then
		player:ChangeScene(Global_NameMapThanhID[selectionID].ID, Global_NameMapThanhID[selectionID].PosX, Global_NameMapThanhID[selectionID].PosY)
		return
	end
	-- ************************** --
	
end
	

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function BaiHuTangCheFu:OnItemSelected(scene, npc, player, itemID,otherParams)

	-- ************************** --
	
	-- ************************** --

end
