-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200003' bên dưới thành ID tương ứng
local TuLuyenChau = Scripts[200009]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function TuLuyenChau:OnPreCheckCondition(scene, item, player, otherParams)

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
function TuLuyenChau:OnUse(scene, item, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	dialog:AddText("Đặt tay lên cảm thấy khí huyết cuộn dâng. Có thể mở <color=orange>trạng thái tu luyện</color> để nhận <color=green>kinh nghiệm đánh quái x2</color> và tăng <color=green>10 điểm may mắn</color><br>" .. string.format("Mỗi ngày sẽ nhận được <color=green>%.1f giờ Tu Luyện</color>. Mỗi giờ tu luyện theo cấp hiện tại sẽ tương đương <color=green>%d kinh nghiệm tu luyện</color>.<br><br>", player:GetXiuLianZhu_TimeAddedPerDay() / 10, player:GetXiuLianZhu_ExpAddedByHour(10)) .. string.format("   - Kinh nghiệm Tu Luyện còn: <color=yellow>%d điểm</color><br>", player:GetXiuLianZhu_Exp()) .. string.format("   - Thời gian tu luyện tích lũy còn: <color=yellow>%.1f giờ</color>", player:GetXiuLianZhu_TimeLeft() / 10))
	dialog:AddSelection(1, "Tiến hành Tu Luyện")
	dialog:AddSelection(1000, "Kết thúc đối thoại")
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
function TuLuyenChau:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 1000 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		local dialog = GUI.CreateItemDialog()
		dialog:AddText("Chọn số giờ muốn Tu Luyện.")
		dialog:AddSelection(10, "Mở Tu Luyện <color=yellow>0.5 giờ</color>")
		dialog:AddSelection(11, "Mở Tu Luyện <color=yellow>1 giờ</color>")
		dialog:AddSelection(12, "Mở Tu Luyện <color=yellow>1.5 giờ</color>")
		dialog:AddSelection(13, "Mở Tu Luyện <color=yellow>2 giờ</color>")
		dialog:AddSelection(14, "Mở Tu Luyện <color=yellow>4 giờ</color>")
		dialog:AddSelection(15, "Mở Tu Luyện <color=yellow>6 giờ</color>")
		dialog:AddSelection(16, "Mở Tu Luyện <color=yellow>8 giờ</color>")
		dialog:AddSelection(1000, "Ta muốn suy nghĩ thêm")
		dialog:Show(item, player)
		return
	end
	-- ************************** --
	if selectionID >= 10 and selectionID <= 16 then
		local nTime = 0
		if selectionID == 10 then
			nTime = 5
		elseif selectionID == 11 then
			nTime = 10
		elseif selectionID == 12 then
			nTime = 15
		elseif selectionID == 13 then
			nTime = 20
		elseif selectionID == 14 then
			nTime = 40
		elseif selectionID == 15 then
			nTime = 60
		elseif selectionID == 16 then
			nTime = 80
		end
		
		-- Nếu sử dụng thành công
		if Player.UseXiuLianZhu(player, nTime) == true then
			local dialog = GUI.CreateItemDialog()
			dialog:AddText("Mở Tu Luyện thành công!")
			dialog:Show(item, player)
		end
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
function TuLuyenChau:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end