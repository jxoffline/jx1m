-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200034' bên dưới thành ID tương ứng
local TuiPhucHoangKim = Scripts[200051]

-- ************************** --
local USED_LIMIT = 10
-- ************************** --


-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function TuiPhucHoangKim:OnPreCheckCondition(scene, item, player, otherParams)

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
function TuiPhucHoangKim:OnUse(scene, item, player, otherParams)

	-- ************************** --
	local nLevel = item:GetItemLevel()
	if nLevel ~=1 then
		player:AddNotification("Vật phẩm không hợp lệ!")
		return
	end
	
	-- Số lượng đã sử dụng trong ngày
	local usedTimeToday = Player.GetValueOfDailyRecore(player, item:GetItemID())
	if usedTimeToday == -1 then
		usedTimeToday = 1
	end
	

	
	-- Tăng số lượt đã dùng
	-- Lưu lại số lượt đã dùng
	Player.SetValueOfDailyRecore(player, item:GetItemID(), usedTimeToday+1)
	
	-- Xóa vật ph
	
	
	-- Nếu đã dùng quá số lượt cho phép
	if usedTimeToday < USED_LIMIT then
		-- ID hộp thưởng
		local nBoxID = item:GetItemValue()
		-- Thực hiện mở Random quà trong hộp
		if Player.OpenRandomBox(player, item:GetID()) == true then
            Player.RemoveItem(player, item:GetID())
        end
    else 
		-- Thêm bạc khóa
		Player.AddMoney(player, 100, 0)
		Player.RemoveItem(player, item:GetID())
	end
	
	-- Thông báo
	player:AddNotification("Sử dụng thành công. Hôm nay đã dùng " .. usedTimeToday .. " " .. item:GetName() .. ".")
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi ấn vào một trong số các chức năng cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		selectionID: number - ID chức năng
-- ****************************************************** --
function TuiPhucHoangKim:OnSelection(scene, item, player, selectionID, otherParams)

	-- ************************** --
	--player:AddNotification("Enter -> TuiPhucHoangKim:OnSelection, selectionID = " .. selectionID)--
	
	-- ************************** --
	
	item:FinishUsing(player)
	

end

-- ****************************************************** --
--	Hàm này được gọi khi có sự kiện người chơi chọn một trong các vật phẩm, và ấn nút Xác nhận cung cấp bởi vật phẩm thông qua Item Dialog
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--		itemID: number - ID vật phẩm được chọn
-- ****************************************************** --
function TuiPhucHoangKim:OnItemSelected(scene, item, player, itemID)

	-- ************************** --

	-- ************************** --

end