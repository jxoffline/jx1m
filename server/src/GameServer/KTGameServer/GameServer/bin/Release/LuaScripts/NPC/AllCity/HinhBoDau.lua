-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000060' bên dưới thành ID tương ứng
local HinhBoDau = Scripts[202]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
-- ****************************************************** --
function HinhBoDau:OnOpen(scene, npc, player, otherParams)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	-- ************************** --
	if player:GetPKValue(player) == 0 then
		dialog:AddText("Không ngờ gặp ngươi ở nơi này. Muốn đi khỏi đây, muốn đi đâu?")
		dialog:AddSelection(3, "Ta muốn ra khỏi đây")
		dialog:AddSelection(2, "Kết thúc đối thoại")	
	else 
		dialog:AddText("Ngươi sát khí quá nặng, muốn đi khỏi đây thì phải đưa ta <color=yellow>1000 knb</color>, <color=yellow>10 vạn bạc</color> để xóa sát khí. Còn không thì chịu khó ngồi đây xám hối khi nào hết sát khí ta sẽ tha!")
		dialog:AddSelection(1, "Ta muốn tẩy sát khí")
		dialog:AddSelection(2, "Kết thúc đối thoại")	
	end
	-- ************************** --
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
function HinhBoDau:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 2 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		local dialog = GUI.CreateNPCDialog()
		dialog:AddText("Ngươi đồng ý dùng <color=yellow>1000 knb</color>, <color=yellow>10 vạn bạc</color> để tẩy toàn bộ sát khí?")
		dialog:AddSelection(11, "Đồng ý")
		dialog:AddSelection(2, "Hủy bỏ")
		dialog:Show(npc, player)
		return
	end
	-- ************************** --
	if selectionID == 11 then
		-- Nếu không đủ đồng
		if Player.CheckMoney(player, 2) < 1000 then
			HinhBoDau:ShowNotify(npc, player, "Ngươi không có đủ <color=yellow>1000 knb</color>, không thể tẩy sát khí!")
			return
		-- Nếu không đủ bạc
		elseif Player.CheckMoney(player, 1) < 100000 then
			HinhBoDau:ShowNotify(npc, player, "Ngươi không có đủ <color=yellow>10 vạn bạc</color>, không thể tẩy sát khí!")
			return
		else
			-- Thực hiện xóa bạc và đồng
			Player.MinusMoney(player, 2, 1000)
			Player.MinusMoney(player, 1, 100000)
			Player.SetPKValue(player, 0)
			HinhBoDau:ShowNotify(npc, player, "Tẩy sát khí thành công. Ngươi có thể rời khỏi đây rồi. Từ nay nhớ hướng thiện, đừng để phải quay lại đây lần nữa!")
			return
		end
	end
	-- ************************** --
	if selectionID == 3 then
		player:ChangeScene(1, 5670, 3000)
		GUI.CloseDialog(player)
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
function HinhBoDau:OnItemSelected(scene, npc, player, itemID, otherParams)

	-- ************************** --

	-- ************************** --

end


-- ==================================================== --
-- ==================================================== --
function HinhBoDau:ShowNotify(npc, player, text)

	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText(text)
	dialog:AddSelection(2, "Kết thúc đối thoại")
	dialog:Show(npc, player)
	-- ************************** --
	
end