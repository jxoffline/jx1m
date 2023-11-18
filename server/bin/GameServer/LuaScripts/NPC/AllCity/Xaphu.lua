-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000013' bên dưới thành ID tương ứng
local Xaphu = Scripts[0000999999999]
local IDMapTele = {
	[40]={ID=40,Posx = 2120, Posy = 2000,Name="Tẩy Tủy Đảo",ITemID1=8858,ITemID2=8859,ITemID3=8860,ITemID4=8861},
}
--****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function Xaphu:OnOpen(scene, npc, player,otherParams)

	-- ************************** --
	
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText("Muốn đi xa thì phải nhờ đến bọn kéo xe chúng ta thôi ")
	dialog.AddSelection(100001, "Tân Thủ Thôn")
	dialog.AddSelection(100002, "Thành Thị")
	dialog.AddSelection(100003, "Môn Phái")
	dialog.AddSelection(40, "Tẩy Tủy Đảo")
	dialog.AddSelection(100004, "Hoàng Thành - Liên Server")
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
function Xaphu:OnSelection(scene, npc, player, selectionID, otherParams)
	
		-- ************************** --
		if  selectionID == 99999 then
			GUI.CloseDialog(player)
			return
		end
		if selectionID == 100001 then
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText("Ngươi muốn đi đâu ?")
			for key ,value in pairs (Global_NameMapThonID) do
				if key ~= scene:GetID() then
					dialog:AddSelection(key,value.Name)	
				end
			end
			dialog.AddSelection(99999, "Kết thúc đối thoại")
			dialog:Show(npc,player)
			return
		end
		if selectionID == 100002 then
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
		if selectionID == 100003 then
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText("Ngươi muốn đi đâu ?")
			  for key ,value in pairs (Global_NameMapPhaiID) do
				if key ~= scene:GetID() then
					dialog:AddSelection(key,value.Name)	
				end
			end
			dialog.AddSelection(99999, "Kết thúc đối thoại")
			dialog:Show(npc,player)
			return
		end
		-- ************************** --
		if Global_NameMapThanhID[selectionID] ~= nil then
			player:ChangeScene(Global_NameMapThanhID[selectionID].ID,Global_NameMapThanhID[selectionID].PosX,Global_NameMapThanhID[selectionID].PosY)
			GUI.CloseDialog(player)
			return
		-- ************************** --
		elseif Global_NameMapThonID[selectionID] ~= nil then
			player:ChangeScene(Global_NameMapThonID[selectionID].ID,Global_NameMapThonID[selectionID].PosX,Global_NameMapThonID[selectionID].PosY)
			GUI.CloseDialog(player)
			return
		-- ************************** --
		elseif Global_NameMapPhaiID[selectionID] ~= nil then
			player:ChangeScene(Global_NameMapPhaiID[selectionID].ID,Global_NameMapPhaiID[selectionID].PosX,Global_NameMapPhaiID[selectionID].PosY)
			GUI.CloseDialog(player)
			return
		end
		if IDMapTele[selectionID] ~=nil then
			if(selectionID==40)then
				-- Nếu không đủ Đồng Tiền Cổ
				if Player.CountItemInBag(player, IDMapTele[selectionID].ITemID1) < 1 or Player.CountItemInBag(player, IDMapTele[selectionID].ITemID2) < 1 or Player.CountItemInBag(player, IDMapTele[selectionID].ITemID3) < 1 or Player.CountItemInBag(player, IDMapTele[selectionID].ITemID4) < 6 then
					self:ShowDialog(npc, player, "Ta cần 1 Tử Thủy Tinh,1 Lam Thủy Tinh, 1 Lục Thủy Tinh, 6 Tinh Hồng bảo Thạch mới lên được tẩy tủy đảo, có đủ rồi hãy đưa ta. Ta đang rất bận!")
					return
				end 
				-- Xóa Đồng Tiền Cổ
				Player.RemoveItem(player, IDMapTele[selectionID].ITemID1, 1)
				Player.RemoveItem(player, IDMapTele[selectionID].ITemID2, 1)
				Player.RemoveItem(player, IDMapTele[selectionID].ITemID3, 1)
				Player.RemoveItem(player, IDMapTele[selectionID].ITemID4, 6)
				player:ChangeScene(IDMapTele[selectionID].ID,IDMapTele[selectionID].Posx, IDMapTele[selectionID].Posy)
			else
				player:ChangeScene(IDMapTele[selectionID].ID,IDMapTele[selectionID].Posx, IDMapTele[selectionID].Posy)
			end

			GUI.CloseDialog(player)
		 return
		end
		 if selectionID == 100004 then 
        player:ChangeScene(76, 8260, 4760)
        GUI.CloseDialog(player)
        return
    end
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function Xaphu:OnItemSelected(scene, npc, player, itemID, otherParams)

	-- ************************** --
	
	-- ************************** --

end
function Xaphu:ChangeScene(player,ID,PosX, PosY)

	-- ************************** --
		Xaphu:ChangeScene(player,ID,Posx,PosY)
	
	-- ************************** --

end
function Xaphu:ShowDialog(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:Show(npc, player)
	-- ************************** --
	
end