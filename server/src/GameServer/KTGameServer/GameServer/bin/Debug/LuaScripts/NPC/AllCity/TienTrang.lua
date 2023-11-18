-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local TienTrang = Scripts[0000032]
local Item = { ID = 10460, number = 1, Series = -1, Bound = 0 } -- ID Kim Ngân Không bị khóa
local Money = { types = 2, SubMoney = 10000 }; -- kiểu tiền

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
-- ****************************************************** --
function TienTrang:OnOpen(scene, npc, player)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog.AddText("Danh dự của Tiền trang chúng tôi rất quan trọng, tuyệt không gạt người. Vị thiếu hiệp kia cần lão phu giúp gì?")
	dialog.AddSelection(1, "Mở cửa hàng!")
	--dialog.AddSelection(2, "Đổi Kim Ngân")
	dialog:AddSelection(3, "Tách trang bị chế hoặc Phi Phong ")
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
function TienTrang:OnSelection(scene, npc, player, selectionID)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	local szMsg = string.format("Bạn xác nhận đổi <color=yellow>10000 KNB lấy 1 Kim Ngân</color> không?");
	local totalSpacesNeed = 2 -- Tổng số khoảng trống trong túi cần
		
	if selectionID == 1 then
		local dialog = GUI.CreateNPCDialog()
		Player.OpenShop(npc, player, 245)
		GUI.CloseDialog(player)
	elseif  selectionID == 2 then
		if Player.CheckMoney(player, Money.types) < Money.SubMoney then
			TienTrang:ShowDialog(npc, player,"Ngươi không có đủ <color=yellow>10.000 KNB</color>, Hãy quay lại gặp ta khi đủ KNB!")
			return
		else
			dialog:AddText(szMsg)
			dialog:AddSelection(4, "Ta đồng ý")
			dialog:AddSelection(9999, "Để ta suy nghĩ đã")
			dialog:Show(npc, player)
			return
		end
	end
	if selectionID == 3 then
		GUI.OpenUI(player, "UIEquipRefineToFS")
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 4 then
		--ckeck tiền của người chơi

		if Player.CheckMoney(player, Money.types) < Money.SubMoney then
			TienTrang:ShowDialog(npc, player,"Ngươi không có đủ <color=yellow>10.000 KNB</color>, Hãy quay lại gặp ta khi đủ KNB!")
			return
		end
		-- Nếu không đủ chỗ chống
		if Player.HasFreeBagSpaces(player, totalSpacesNeed) == false then
			dialog:ShowDialog(npc, player,string.format("Bằng hữu cần sắp xếp tối thiểu <color=green>%d ô trống</color> trong túi đồ để nhận Kim Ngân!"
					, totalSpacesNeed))
			return
		end
		-- Thực hiện trừ đồng
		Player.MinusMoney(player, Money.types, Money.SubMoney)
		Player.AddItemLua(player, Item.ID, Item.number, Item.Series, Item.Bound)
		TienTrang:ShowDialog(npc, player,"Ngươi đã đổi 10.000 KNB lấy 1 Kim Ngân thành công !")
		
		return
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
function TienTrang:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --

	-- ************************** --

end

function TienTrang:ShowDialog(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:Show(npc, player)
	-- ************************** --

end
