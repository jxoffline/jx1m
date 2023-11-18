-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local XoYo_BaoMing_ZhaoKan = Scripts[500000]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn vào NPC
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function XoYo_BaoMing_ZhaoKan:OnOpen(scene, npc, player, otherParams)


	-- ************************** --
	local dialog = GUI.CreateNPCDialog()
	dialog:AddText("Trong Tiêu Dao Cốc rất nguy hiểm. Để đảm bảo an toàn, <color=green>những người chơi đạt cấp 80 trở lên, tổ đội tối thiểu 4 thành viên và chỉ được phép tham gia vượt ải <color=red>1 lần mỗi ngày</color></color>. Đạt các yêu cầu trên, đội trưởng đến gặp lão phu vào các khung giờ từ <color=green>0 - 2:00</color> và <color=green>12:00 - 23:00</color>, lão phu sẽ đưa các ngươi vào thử thách <color=yellow>Vượt ải Tiêu Dao Cốc</color>.")
	-- Nếu nằm trong các khung giờ cho phép
	if self:IsRegisterTime() == true then
		dialog:AddSelection(1, "Vào <color=yellow>Tiêu Dao Cốc (dễ)</color>")
		dialog:AddSelection(2, "Vào <color=yellow>Tiêu Dao Cốc (trung bình)</color>")
		dialog:AddSelection(3, "Vào <color=yellow>Tiêu Dao Cốc (khó)</color>")
		dialog:AddSelection(100, "Ta vẫn chưa chuẩn bị xong...")
	else
		dialog:AddSelection(100, "Ta chỉ ghé qua...")
	end
	dialog:Show(npc, player)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - Người chơi tương ứng
--		selectionID: number - ID chức năng
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function XoYo_BaoMing_ZhaoKan:OnSelection(scene, npc, player, selectionID, otherParams)

	-- ************************** --
	if selectionID == 1 or selectionID == 2 or selectionID == 3 then
		if self:IsRegisterTime() == false then
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText("Hiện không phải thời gian báo danh Tiêu Dao Cốc, hãy quay lại sau!")
			dialog:Show(npc, player)
			return
		end
		-- Kiểm tra điều kiện tham gia
		local conditionCheck = EventManager.XoYo_CheckCondition(player)
		-- Nếu có lỗi
		if conditionCheck ~= "OK" then
			local dialog = GUI.CreateNPCDialog()
			dialog:AddText(conditionCheck)
			dialog:Show(npc, player)
			return
		end
		-- Thỏa mãn điều kiện thì bắt đầu Tiêu Dao Cốc với độ khó tương ứng
		EventManager.XoYo_Begin(player, selectionID)
		return
	end
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi NPC thông qua NPC Dialog
--		scene: Scene - Bản đồ hiện tại
--		npc: NPC - NPC tương ứng
--		player: Player - NPC tương ứng
--		selectedItemInfo: SelectItem - Vật phẩm được chọn
--		otherParams: Key-Value {number, string} - Danh sách các tham biến khác
-- ****************************************************** --
function XoYo_BaoMing_ZhaoKan:OnItemSelected(scene, npc, player, selectedItemInfo, otherParams)

	-- ************************** --
	
	-- ************************** --

end

-- ======================================================================== --
-- ======================================================================== --
-- ****************************************************** --
--  Kiểm tra có phải thời gian báo danh Tiêu Dao Cốc không
-- ****************************************************** --
function XoYo_BaoMing_ZhaoKan:IsRegisterTime()
	
	-- ************************** --
	local nHour = System.GetHour()
	-- ************************** --
	if nHour >= 0 and nHour < 2 then
		return true
	end
	-- ************************** --
	if nHour >= 12 and nHour < 23 then
		return true
	end
	-- ************************** --
	
end
