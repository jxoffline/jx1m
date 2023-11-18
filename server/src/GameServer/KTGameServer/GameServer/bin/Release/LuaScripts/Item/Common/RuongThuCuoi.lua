-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local RuongThuCuoi = Scripts[200303]
  local IDThucuoi ={
		 [8240 ]={ Name="Hỏa Tinh Kim Hổ Vương ",Number =1,Series =-1,Bin=1 },
		 [8250]={ Name="Kim Tinh Bạch Hổ Vương",Number =1,Series =-1,Bin=1 },
		 [8260]={ Name="Long Tinh Hắc Hổ Vương",Number =1,Series =-1,Bin=1 },
		 }

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function RuongThuCuoi:OnPreCheckCondition(scene, item, player, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> RuongThuCuoi:OnPreCheckCondition")--
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
function RuongThuCuoi:OnUse(scene, item, player, otherParams)

	-- ************************** --
	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	dialog:AddText("Dùng được trang bị cực phẩm. Hãy chọn trang bị mình cần.<br><color=#53f9e8>(Nhận Thú cưỡi sẽ tự động khóa)</color>")
	for key, value in pairs(IDThucuoi) do
		dialog:AddSelection(key,value.Name)
	end
	dialog:AddSelection(5,"Để ta suy nghĩ đã")
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
function RuongThuCuoi:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> RuongThuCuoi:OnSelection, selectionID = " .. selectionID)--
	
	local dialog = GUI.CreateItemDialog()
	for key, value in pairs(IDThucuoi) do
		if selectionID == key then
			Player.RemoveItem(player,item:GetID())
			Player.AddItemLua(player,key,value.Number,value.Series,value.Bin)
			player:AddNotification("Nhận thú cưỡi <color=red>thành công</color>")
			GUI.CloseDialog(player)
		else 	
			dialog:AddText("Lỗi không có vật phẩm") 		
		end
	end
	if selectionID == 5 then
		GUI.CloseDialog(player)
	end
	-- ************************** --
	item:FinishUsing(player)
	-- ************************** --
	
end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function RuongThuCuoi:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end