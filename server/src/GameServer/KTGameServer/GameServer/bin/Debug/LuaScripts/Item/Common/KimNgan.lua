-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '200005' bên dưới thành ID tương ứng
local KimNgan = Scripts[200105]

-- ************************** --
local MoneyType = 2
-- ************************** --

-- ****************************************************** --
--	Hàm này được gọi khi người chơi ấn sử dụng vật phẩm, kiểm tra điều kiện có được dùng hay không
--		scene: Scene - Bản đồ hiện tại
--		item: Item - Vật phẩm tương ứng
--		player: Player - NPC tương ứng
--	Return: True nếu thỏa mãn điều kiện có thể sử dụng, False nếu không thỏa mãn
-- ****************************************************** --
function KimNgan:OnPreCheckCondition(scene, item, player, otherParams)

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
function KimNgan:OnUse(scene, item, player, otherParams)

    -- ************************** --
    local dialog = GUI.CreateItemDialog()
    dialog:AddText("Bạn có chắc chắn muốn sử dụng: " .. item:GetName() .. "?")
    dialog:AddSelection(1, "Chắn chắn")
    dialog:AddSelection(2, "Ta suy nghĩ đã")
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
function KimNgan:OnSelection(scene, item, player, selectionID, otherParams)

    -- ************************** --
	if selectionID == 2 then
		GUI.CloseDialog(player)
		return
	end
	-- ************************** --
	if selectionID == 1 then
		-- Lượng tiền nhận được
		local amount = item:GetItemValue()
		-- Xóa vật phẩm
		Player.RemoveItem(player, item:GetID())
		-- Thêm tiền tương ứng
		if  Player.AddMoney(player, amount, MoneyType) then
			-- Đóng khung
			GUI.CloseDialog(player)
		else
            player:AddNotification("Có lỗi khi sử dụng vật phẩm")
			GUI.CloseDialog(player)
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
function KimNgan:OnItemSelected(scene, item, player, itemID)

    -- ************************** --

    -- ************************** --

end
