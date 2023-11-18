-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi    Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200034' bên dưới thành ID tương ứng
local CauHonNgoc = Scripts[200086]

-- ************************** --
local Boss = {
	[431] = {				-- Câu Hồn Ngọc (sơ)
		[1] = {
			BossID = 2969,
			Name = "Vân Tuyết Sơn [Mộc]",
		},
		[2] = {
			BossID = 2970,
			Name = "Hình Bổ Đầu [Thổ]",
		},
		[3] = {
			BossID = 2971,
			Name = "Vạn Lão Điên [Hỏa]",
		},
		[4] = {
			BossID = 2972,
			Name = "Cao Sĩ Hiền [Thổ]",
		},
		[5] = {
			BossID = 2973,
			Name = "Thác Bạt Sơn Uyên [Kim]",
		},
		[6] = {
			BossID = 2974,
			Name = "Dương Liễu [Thủy]",
		},
	},
	[432] = {				-- Câu Hồn Ngọc (trung)
		[1] = {
			BossID = 2978,
			Name = "Thần Thương Phương Vãn [Kim]",
		},
		[2] = {
			BossID = 2979,
			Name = "Triệu Ứng Tiên [Mộc]",
		},
		[3] = {
			BossID = 2980,
			Name = "Hương Ngọc Tiên [Thủy]",
		},
		[4] = {
			BossID = 2981,
			Name = "Man Tăng Bất Giới Hòa Thượng [Hỏa]",
		},
		[5] = {
			BossID = 2982,
			Name = "Nam Quách Nho [Thổ]",
		},
	},
	[433] = {				-- Câu Hồn Ngọc (cao)
		[1] = {
			BossID = 2934,
			Name = "Nhu Tiểu Thúy [Kim]",
		},
		[2] = {
			BossID = 2935,
			Name = "Trương Thiện Đức [Mộc]",
		},
		[3] = {
			BossID = 2936,
			Name = "Giả Dật Sơn [Thủy]",
		},
		[4] = {
			BossID = 2937,
			Name = "Ô Sơn Thanh [Hỏa]",
		},
		[5] = {
			BossID = 2938,
			Name = "Trần Vô Mệnh [Thổ]",
		},
	},
}
-- ************************** --

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function CauHonNgoc:OnPreCheckCondition(scene, item, player, otherParams)

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
function CauHonNgoc:OnUse(scene, item, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	dialog:AddText("Trong phụ bản <color=yellow>Thần Bí Bảo Khố</color>, sau khi hoàn thành tiêu diệt Boss, có thể sử dụng <color=green>Câu Hồn Ngọc</color> để triệu hồi <color=orange>Võ lâm cao thủ</color>.<br><color=green>Câu Hồn Ngọc (sơ)</color> có thể triệu hồi <color=orange>Võ lâm cao thủ cấp 55</color><br><color=green>Câu Hồn Ngọc (trung)</color> có thể triệu hồi <color=orange>Võ lâm cao thủ cấp 75</color><br><color=green>Câu Hồn Ngọc (cao)</color> có thể triệu hồi <color=orange>Võ lâm cao thủ cấp 95</color>")
	dialog:AddSelection(1, "Triệu hồi Võ lâm cao thủ")
    dialog:AddSelection(1000, "Ta suy nghĩ đã")
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
function CauHonNgoc:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 1000 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		-- ID vật phẩm
		local itemID = item:GetItemID()
		-- Toác
		if not Boss[itemID] then
			self:ShowDialog(item, player, "Thông tin vật phẩm bị lỗi!")
			return
		end
		
		-- Kiểm tra điều kiện
		local ret = EventManager.ShenMiBaoKu_UseCallBossItem_CheckCondition(player)
		-- Nếu toạch
		if ret ~= "OK" then
			self:ShowDialog(item, player, ret)
			return
		end
		
		local dialog = GUI.CreateItemDialog()
		dialog:AddText("Chọn <color=yellow>Cao thủ võ lâm</color> muốn triệu hồi.")
		for key, value in pairs(Boss[itemID]) do
			dialog:AddSelection(10 * itemID + key, value.Name)
		end
		dialog:Show(item, player)
		return
	end
	-- ************************** --
	if (selectionID >= 10 * 431 + 1 and selectionID <= 10 * 431 + 5) or (selectionID >= 10 * 432 + 1 and selectionID <= 10 * 432 + 5) or (selectionID >= 10 * 433 + 1 and selectionID <= 10 * 433 + 5) then
		-- ID vật phẩm
		local itemID = item:GetItemID()
		-- Toác
		if not Boss[itemID] then
			self:ShowDialog(item, player, "Thông tin vật phẩm bị lỗi!")
			return
		end
		
		-- Kiểm tra điều kiện
		local ret = EventManager.ShenMiBaoKu_UseCallBossItem_CheckCondition(player)
		-- Nếu toạch
		if ret ~= "OK" then
			self:ShowDialog(item, player, ret)
			return
		end
		
		-- Xóa vật phẩm
		Player.RemoveItem(player, item:GetID())  
		
		-- Thứ tự Boss
		local idx = 1
		if selectionID >= 10 * 431 + 1 and selectionID <= 10 * 431 + 5 then
			idx = selectionID - 10 * 431
		elseif selectionID >= 10 * 432 + 1 and selectionID <= 10 * 432 + 5 then
			idx = selectionID - 10 * 432
		elseif selectionID >= 10 * 433 + 1 and selectionID <= 10 * 433 + 5 then
			idx = selectionID - 10 * 433
		end
		-- Thông tin Boss
		local bossInfo = Boss[itemID][idx]
		
		-- Sử dụng vật phẩm
		EventManager.ShenMiBaoKu_FinishUsingCallBossItem(player, bossInfo.BossID)
		
		-- Đóng Dialog
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
function CauHonNgoc:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end


-- ======================================================= --
-- ======================================================= --
function CauHonNgoc:ShowDialog(item, player, text)

	-- ************************** --
	local dialog = GUI.CreateItemDialog()
	dialog:AddText(text)
	dialog:AddSelection(1000, "Kết thúc đối thoại")
	dialog:Show(item, player)
	-- ************************** --
	
end