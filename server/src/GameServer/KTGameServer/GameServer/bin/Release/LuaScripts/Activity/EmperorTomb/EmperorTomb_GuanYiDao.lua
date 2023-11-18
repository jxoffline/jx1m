-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local EmperorTomb_GuanYiDao = Scripts[400031]
local ItemIDSub = 819			-- Vật phẩm Hòa Thị Bích
local ItemCount =5				-- Số lượng bị trừ Hòa thị Bích
local ItemIDAdd = 828			-- Vật phẩm Vũ khí Thanh Đồng-Luyện Hóa Đồ

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function EmperorTomb_GuanYiDao:OnOpen(scene, npc, player)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("Ta có nhiều món báu vật chỉ có ở nơi độc khí cực nặng Tần Lăng này. Nếu ngươi có tố chất, thì ghé qua xem...")
	dialog:AddSelection(1,"Nhưng ta phải xem cái ngươi vừa nói là cái gì đã chứ")
	dialog:AddSelection(2,"Đổi Thanh Đồng Luyện Phổ")
	dialog:AddSelection(3,"Mua vũ khí Tần Lăng")
	dialog:AddSelection(100, "Kết thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function EmperorTomb_GuanYiDao:OnSelection(scene, npc, player, selectionID)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	if selectionID ==1 then
		Player.OpenShop(npc, player, 155)
		GUI.CloseDialog(player)
	end
	if selectionID == 2 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Ta đã không nhìn lầm người, ngươi thực sự rất mạnh mẽ! Ngươi thu thập được rất nhiều Hòa Thị Bích, phải không? Ta sẽ đổi kho báu của ta để lấy nó!")
		dialog:AddSelection(20, "Dùng 5 Hòa Thị Bích để đổi lấy 1 Vũ khí Thanh Đồng-Luyện Hóa Đồ")
		dialog:AddSelection(100, "Ta suy nghĩ đã")
		dialog:Show(npc, player)
		return
	end
	if selectionID ==3 then
		dialog:AddText("Miêu Miêu Miêu : Bọn ta nhất định sẽ thực hiện được hoài bão của trại chủ!")
		dialog:AddSelection(30,"Mua Vũ Khí cấp 120-130(Kim)")
		dialog:AddSelection(31,"Mua Vũ Khí cấp 120-130(Mộc)")
		dialog:AddSelection(33,"Mua Vũ Khí cấp 120-130(Thủy)")
		dialog:AddSelection(33,"Mua Vũ Khí cấp 120-130(Hỏa)")
		dialog:AddSelection(34,"Mua Vũ Khí cấp 120-130(Thổ)")
		dialog:AddSelection(100, "Ta suy nghĩ đã")
		dialog:Show(npc, player)
	end
	-- ************************** --
		if selectionID ==30 then
			Player.OpenShop(npc, player, 156)
			GUI.CloseDialog(player)
		end
		if selectionID ==31 then
			Player.OpenShop(npc, player, 157)
			GUI.CloseDialog(player)
		end
		if selectionID ==33 then
			Player.OpenShop(npc, player, 158)
			GUI.CloseDialog(player)
		end
		if selectionID ==33 then
			Player.OpenShop(npc, player, 159)
			GUI.CloseDialog(player)
		end
		if selectionID ==34 then
			Player.OpenShop(npc, player, 160)
			GUI.CloseDialog(player)
		end
	if selectionID == 20 then 
		-- Nếu Hòa Thị Bích
		if Player.CountItemInBag(player, ItemIDSub) < ItemCount then
			dialog:AddText( "Ta cần 5 Hòa Thị Bích, có đủ rồi hãy đưa ta. Ta đang rất bận!")
			dialog:Show(npc, player)
			return
		end 
		if Player.HasFreeBagSpaces(player, 1) == false then
			sdialog:AddText( "Túi của ngươi đã đầy, hãy sắp xếp <color=green>1 ô trống</color> trong túi để nhận Vật phẩm Vũ khí Thanh Đồng-Luyện Hóa Đồ!")
			ialog:Show(npc, player)
			return
		end
		-- Xóa Hòa Thị Bích
		
		Player.RemoveItem(player, ItemIDSub, ItemCount)
		
		
		-- Tạo Vật phẩm Vũ khí Thanh Đồng-Luyện Hóa Đồ
		Player.AddItemLua(player, ItemIDAdd, 1, -1, 1)
		
		
		
		dialog:AddText( "Cảm ơn ngươi về những Viên Hòa Thị Bích này, đây là phần thưởng của ngươi, hãy nhận nó!")
		dialog:Show(npc, player)
		return
	end
	if selectionID == 100 then
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
function EmperorTomb_GuanYiDao:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --
	
	-- ************************** --

end

-- ======================================================= --
-- ======================================================= --
function EmperorTomb_GuanYiDao:ShowNotify(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:AddSelection(100, "Kết thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --

end